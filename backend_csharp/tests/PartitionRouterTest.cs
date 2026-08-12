using MessageBroker.BrokerCore;
using MessageBroker.Distribution;
using Xunit;

namespace MessageBroker.Backend.Tests;

public class PartitionRouterTest
{
    [Fact]
    public void SameTopic_RoutesConsistently()
    {
        var policy = new BrokerPolicy { NodeSlotCount = 3 };
        var store = new InMemoryMessageStore();
        var registry = new NodeRegistry();
        for (var i = 0; i < 3; i++) registry.Register(new BrokerNode(i, store, policy));
        var router = new PartitionRouter(registry, 3);
        var a = router.ResolveSlot("orders.created");
        var b = router.ResolveSlot("orders.created");
        Assert.Equal(a, b);
        Assert.InRange(a, 0, 2);
    }
}
