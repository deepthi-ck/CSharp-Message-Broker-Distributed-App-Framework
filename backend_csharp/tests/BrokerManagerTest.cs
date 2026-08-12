using Xunit;

namespace MessageBroker.Backend.Tests;

public class BrokerManagerTest
{
    [Fact]
    public void Statistics_ReflectOperations()
    {
        var (_, manager, _, _) = TestHost.Create();
        manager.Publish("order:1", "orders.created", "A");
        manager.Consume("orders.created");
        manager.AckDelete("order:1");
        Assert.Equal(1, manager.Stats.PublishCount);
        Assert.Equal(1, manager.Stats.ConsumeCount);
        Assert.Equal(1, manager.Stats.AckDeleteCount);
    }

    [Fact]
    public async Task Ttl_ExpiresMessage()
    {
        var (_, manager, _, _) = TestHost.Create(ttlSeconds: 1);
        manager.Publish("order:ttl", "orders.created", "Visvantha");
        await Task.Delay(1100);
        var cons = manager.Consume("orders.created");
        Assert.False(cons.Success);
        Assert.Equal("EMPTY", cons.Status);
    }

    [Fact]
    public void Eviction_RespectsCapacity()
    {
        var (_, manager, _, store) = TestHost.Create(maxMessages: 2);
        manager.Publish("m1", "t", "a");
        manager.Publish("m2", "t", "b");
        manager.Publish("m3", "t", "c");
        Assert.True(store.Count <= 2);
        Assert.True(store.Contains("m3"));
    }

    [Fact]
    public void OutboxGuard_BlocksDuplicatePublish()
    {
        var (_, manager, _, _) = TestHost.Create();
        Assert.True(manager.Publish("dup:1", "t", "a").Success);
        Assert.False(manager.Publish("dup:1", "t", "b").Success);
    }
}
