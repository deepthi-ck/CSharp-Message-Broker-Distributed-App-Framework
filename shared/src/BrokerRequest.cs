namespace MessageBroker.Shared;

public sealed class BrokerRequest
{
    public string? MessageId { get; set; }
    public string? Topic { get; set; }
    public string? Payload { get; set; }
    public string? Key { get; set; }
}
