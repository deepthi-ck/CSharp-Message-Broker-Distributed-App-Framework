using MessageBroker.BrokerCore;
using MessageBroker.Distribution;
using MessageBroker.Shared;

namespace MessageBroker.Backend;

public sealed class BrokerManager
{
    private readonly PartitionRouter _router;
    private readonly ReplicationManager _replication;
    private readonly BrokerStatistics _stats;
    private readonly EvictionManager _eviction;
    private readonly ExpirationManager _expiration;
    private readonly InMemoryMessageStore _store;
    private readonly OutboxInboxGuard _guard;

    public BrokerManager(PartitionRouter router, ReplicationManager replication, BrokerStatistics stats,
        EvictionManager eviction, ExpirationManager expiration, InMemoryMessageStore store, OutboxInboxGuard guard)
    {
        _router = router; _replication = replication; _stats = stats;
        _eviction = eviction; _expiration = expiration; _store = store; _guard = guard;
    }

    public BrokerStatistics Stats => _stats;
    public PartitionRouter Router => _router;
    public ReplicationManager Replication => _replication;
    public InMemoryMessageStore Store => _store;

    public BrokerResponse Publish(string messageId, string topic, string payload)
    {
        _expiration.Sweep();
        _eviction.EnsureCapacity();
        if (!_guard.TryRegisterPublish(messageId))
            return Fail("Duplicate publish blocked by outbox guard");

        var node = _router.Route(topic);
        var entry = node.Publish(messageId, topic, payload);
        _replication.Replicate(entry);
        _stats.RecordPublish();
        _expiration.Refresh();
        return Ok(entry, "PUBLISH = SUCCESS");
    }

    public BrokerResponse Consume(string topic, bool preferReplica = false)
    {
        _expiration.Sweep();
        _stats.RecordConsume();
        if (preferReplica)
        {
            var replica = _replication.GetReplicaByTopic(topic);
            if (replica is not null)
            {
                _stats.RecordDelivered();
                return Ok(replica, "OK_REPLICA");
            }
        }

        var node = _router.Route(topic);
        var entry = node.Consume(topic);
        if (entry is null)
        {
            _stats.RecordNotFound();
            return Fail("NOT_FOUND", "EMPTY");
        }
        _guard.TryRegisterConsume(entry.MessageId);
        _stats.RecordDelivered();
        _expiration.Refresh();
        return Ok(entry, "CONSUME = SUCCESS");
    }

    public BrokerResponse AckDelete(string messageId)
    {
        var found = _store.AckDelete(messageId);
        _replication.Remove(messageId);
        if (!found)
        {
            _stats.RecordNotFound();
            return Fail("NOT_FOUND", "EMPTY");
        }
        _stats.RecordAck();
        _expiration.Refresh();
        return new BrokerResponse { Success = true, Status = "ACKED", Message = "ACK/DELETE = SUCCESS" };
    }

    public object Health()
    {
        _expiration.Sweep();
        return new { status = "healthy", broker = "available", nodes = _stats.NodeCount };
    }

    private static BrokerResponse Ok(MessageEntry entry, string message) =>
        new() { Success = true, Status = "OK", Message = message, Entry = entry };
    private static BrokerResponse Fail(string message, string status = "ERROR") =>
        new() { Success = false, Status = status, Message = message };
}
