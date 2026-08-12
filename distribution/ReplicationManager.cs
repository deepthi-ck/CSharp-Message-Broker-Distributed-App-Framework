using MessageBroker.Shared;
namespace MessageBroker.Distribution;

public sealed class ReplicationManager
{
    private readonly Dictionary<string, MessageEntry> _replicaMirror = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public void Replicate(MessageEntry entry)
    {
        lock (_gate) { _replicaMirror[entry.MessageId] = entry.Clone(); }
    }

    public MessageEntry? GetReplica(string messageId)
    {
        lock (_gate) { return _replicaMirror.TryGetValue(messageId, out var e) ? e.Clone() : null; }
    }

    public MessageEntry? GetReplicaByTopic(string topic)
    {
        lock (_gate)
        {
            return _replicaMirror.Values.FirstOrDefault(e => e.Topic == topic && e.Status != "acked")?.Clone();
        }
    }

    public void Remove(string messageId)
    {
        lock (_gate) { _replicaMirror.Remove(messageId); }
    }
}
