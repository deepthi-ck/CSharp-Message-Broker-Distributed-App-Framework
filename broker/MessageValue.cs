using MessageBroker.Shared;
namespace MessageBroker.BrokerCore;
public sealed class MessageValue
{
    public MessageEntry Entry { get; }
    public DateTimeOffset LastActivityUtc { get; set; } = DateTimeOffset.UtcNow;
    public MessageValue(MessageEntry entry) => Entry = entry;
    public void Touch() => LastActivityUtc = DateTimeOffset.UtcNow;
}
