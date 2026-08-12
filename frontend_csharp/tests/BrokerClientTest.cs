using System.Text.Json;
using MessageBroker.Frontend;
using MessageBroker.Shared;
using RichardSzalay.MockHttp;
using Xunit;

namespace MessageBroker.Frontend.Tests;

public class BrokerClientTest
{
    [Fact]
    public async Task PublishAsync_PostsToBrokerApi()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "http://localhost/broker/publish")
            .Respond("application/json", JsonSerializer.Serialize(new BrokerResponse
            {
                Success = true, Message = "PUBLISH = SUCCESS",
                Entry = new MessageEntry { MessageId = "order:1001", Topic = "orders.created", Payload = "Visvantha" }
            }));
        var http = mock.ToHttpClient();
        http.BaseAddress = new Uri("http://localhost/");
        var client = new BrokerClient(http);
        var result = await client.PublishAsync("order:1001", "orders.created", "Visvantha");
        Assert.True(result!.Success);
        Assert.Equal("order:1001", result.Entry!.MessageId);
    }
}
