namespace MessageBroker.BrokerCore;
public sealed class BrokerPolicy
{
    public int MessageTtlSeconds { get; init; } = 300;
    public int MaxMessages { get; init; } = 64;
    public int NodeSlotCount { get; init; } = 3;
    public static BrokerPolicy FromConfiguration(MessageBroker.Shared.BrokerConfiguration cfg) => new()
    {
        MessageTtlSeconds = cfg.MessageTtlSeconds,
        MaxMessages = cfg.MaxMessages,
        NodeSlotCount = cfg.NodeSlotCount
    };
}
