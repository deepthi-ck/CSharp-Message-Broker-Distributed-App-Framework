namespace MessageBroker.Shared;

public sealed class BrokerConfiguration
{
    public int MessageTtlSeconds { get; set; } = 300;
    public int MaxMessages { get; set; } = 64;
    public int NodeSlotCount { get; set; } = 3;
    public string ApiBaseUrl { get; set; } = "http://localhost:5082";
}
