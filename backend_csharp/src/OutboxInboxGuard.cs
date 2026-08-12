namespace MessageBroker.Backend;

/// <summary>Minimal in-memory outbox/inbox idempotency guard.</summary>
public sealed class OutboxInboxGuard
{
    private readonly HashSet<string> _outbox = new(StringComparer.Ordinal);
    private readonly HashSet<string> _inbox = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public bool TryRegisterPublish(string messageId)
    {
        lock (_gate) { return _outbox.Add(messageId); }
    }

    public bool TryRegisterConsume(string messageId)
    {
        lock (_gate) { return _inbox.Add(messageId); }
    }

    public bool AlreadyPublished(string messageId)
    {
        lock (_gate) { return _outbox.Contains(messageId); }
    }
}
