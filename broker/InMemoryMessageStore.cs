using MessageBroker.Shared;
namespace MessageBroker.BrokerCore;

public sealed class InMemoryMessageStore
{
    private readonly Dictionary<string, MessageValue> _byId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Queue<string>> _byTopic = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public MessageEntry Publish(string messageId, string topic, string payload, TimeSpan ttl, string slot)
    {
        lock (_gate)
        {
            if (_byId.ContainsKey(messageId))
                throw new InvalidOperationException($"Message already exists: {messageId}");
            var entry = new MessageEntry
            {
                MessageId = messageId, Topic = topic, Payload = payload,
                CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.Add(ttl),
                NodeSlot = slot, Status = "ready"
            };
            _byId[messageId] = new MessageValue(entry);
            if (!_byTopic.TryGetValue(topic, out var q))
            {
                q = new Queue<string>();
                _byTopic[topic] = q;
            }
            q.Enqueue(messageId);
            return entry.Clone();
        }
    }

    public MessageEntry? Consume(string topic)
    {
        lock (_gate)
        {
            SweepExpiredLocked();
            if (!_byTopic.TryGetValue(topic, out var q)) return null;
            while (q.Count > 0)
            {
                var id = q.Peek();
                if (!_byId.TryGetValue(id, out var mv) || IsExpired(mv))
                {
                    q.Dequeue();
                    _byId.Remove(id);
                    continue;
                }
                q.Dequeue();
                mv.Entry.DeliveryCount++;
                mv.Entry.Status = "delivered";
                mv.Touch();
                return mv.Entry.Clone();
            }
            return null;
        }
    }

    public MessageEntry? Peek(string topic)
    {
        lock (_gate)
        {
            SweepExpiredLocked();
            if (!_byTopic.TryGetValue(topic, out var q)) return null;
            foreach (var id in q)
            {
                if (_byId.TryGetValue(id, out var mv) && !IsExpired(mv))
                    return mv.Entry.Clone();
            }
            return null;
        }
    }

    public bool AckDelete(string messageId)
    {
        lock (_gate)
        {
            if (!_byId.Remove(messageId, out var mv)) return false;
            if (_byTopic.TryGetValue(mv.Entry.Topic, out var q))
            {
                var remaining = q.Where(x => x != messageId).ToList();
                _byTopic[mv.Entry.Topic] = new Queue<string>(remaining);
            }
            return true;
        }
    }

    public bool Contains(string messageId)
    {
        lock (_gate)
        {
            if (!_byId.TryGetValue(messageId, out var mv)) return false;
            if (IsExpired(mv)) { _byId.Remove(messageId); return false; }
            return true;
        }
    }

    public void Clear() { lock (_gate) { _byId.Clear(); _byTopic.Clear(); } }
    public int Count { get { lock (_gate) { return _byId.Count; } } }

    public List<string> EvictExpired()
    {
        lock (_gate) { return SweepExpiredLocked(); }
    }

    public string? EvictOldest()
    {
        lock (_gate)
        {
            if (_byId.Count == 0) return null;
            var victim = _byId.OrderBy(kv => kv.Value.LastActivityUtc).First();
            AckDeleteLocked(victim.Key);
            return victim.Key;
        }
    }

    private void AckDeleteLocked(string messageId)
    {
        if (!_byId.Remove(messageId, out var mv)) return;
        if (_byTopic.TryGetValue(mv.Entry.Topic, out var q))
            _byTopic[mv.Entry.Topic] = new Queue<string>(q.Where(x => x != messageId));
    }

    private List<string> SweepExpiredLocked()
    {
        var expired = _byId.Where(kv => IsExpired(kv.Value)).Select(kv => kv.Key).ToList();
        foreach (var id in expired) AckDeleteLocked(id);
        return expired;
    }

    private static bool IsExpired(MessageValue mv) =>
        mv.Entry.ExpiresAt is { } exp && exp <= DateTimeOffset.UtcNow;
}
