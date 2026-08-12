using Xunit;

namespace MessageBroker.Backend.Tests;

public class ReplicationManagerTest
{
    [Fact]
    public void Publish_ReplicatesAndReplicaConsumeWorks()
    {
        var (_, manager, replication, _) = TestHost.Create();
        manager.Publish("order:1001", "orders.created", "Visvantha");
        var replica = replication.GetReplica("order:1001");
        Assert.NotNull(replica);
        Assert.Equal("Visvantha", replica!.Payload);
        var viaManager = manager.Consume("orders.created", preferReplica: true);
        Assert.True(viaManager.Success);
        Assert.Equal("OK_REPLICA", viaManager.Message);
        Assert.Equal("Visvantha", viaManager.Entry!.Payload);
    }
}
