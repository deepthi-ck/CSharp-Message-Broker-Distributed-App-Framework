using MessageBroker.BrokerCore;
using MessageBroker.Distribution;

namespace MessageBroker.Backend.Tests;

internal static class TestHost
{
    public static (BrokerService Service, BrokerManager Manager, ReplicationManager Replication, InMemoryMessageStore Store) Create(
        int maxMessages = 64, int ttlSeconds = 300, int slots = 3)
    {
        var policy = new BrokerPolicy { MaxMessages = maxMessages, MessageTtlSeconds = ttlSeconds, NodeSlotCount = slots };
        var store = new InMemoryMessageStore();
        var stats = new BrokerStatistics();
        stats.SetNodeCount(slots);
        stats.SetPartitionCount(slots);
        var registry = new NodeRegistry();
        for (var i = 0; i < slots; i++) registry.Register(new BrokerNode(i, store, policy));
        var router = new PartitionRouter(registry, slots);
        var replication = new ReplicationManager();
        var eviction = new EvictionManager(store, policy, stats);
        var expiration = new ExpirationManager(store, stats);
        var guard = new OutboxInboxGuard();
        var manager = new BrokerManager(router, replication, stats, eviction, expiration, store, guard);
        return (new BrokerService(manager), manager, replication, store);
    }
}
