using MessageBroker.Shared;
using Xunit;

namespace MessageBroker.Backend.Tests;

public class BrokerServiceTest
{
    [Fact]
    public void PublishConsumeAck_Flow()
    {
        var (svc, _, _, _) = TestHost.Create();
        var pub = svc.Publish(new BrokerRequest { MessageId = "order:1001", Topic = "orders.created", Payload = "Visvantha" });
        Assert.True(pub.Success);
        Assert.Contains("PUBLISH", pub.Message);

        var cons = svc.Consume("orders.created");
        Assert.True(cons.Success);
        Assert.Equal("Visvantha", cons.Entry!.Payload);

        var ack = svc.Ack("order:1001");
        Assert.True(ack.Success);
        Assert.Equal("ACKED", ack.Status);

        var again = svc.Consume("orders.created");
        Assert.False(again.Success);
        Assert.Equal("EMPTY", again.Status);
    }
}
