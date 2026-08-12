namespace MessageBroker.Shared;

public sealed class MessageEntry
{
    public string MessageId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public int Revision { get; set; }
    public int DeliveryCount { get; set; }
    public string Status { get; set; } = "ready";
    public string NodeSlot { get; set; } = string.Empty;

    public MessageEntry Clone() => new()
    {
        MessageId = MessageId, Topic = Topic, Payload = Payload,
        CreatedAt = CreatedAt, ExpiresAt = ExpiresAt, Revision = Revision,
        DeliveryCount = DeliveryCount, Status = Status, NodeSlot = NodeSlot
    };
}
