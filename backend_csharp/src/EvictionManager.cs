using MessageBroker.BrokerCore;
namespace MessageBroker.Backend;
public sealed class EvictionManager
{
    private readonly InMemoryMessageStore _store;
    private readonly BrokerPolicy _policy;
    private readonly BrokerStatistics _stats;
    public EvictionManager(InMemoryMessageStore store, BrokerPolicy policy, BrokerStatistics stats)
    { _store = store; _policy = policy; _stats = stats; }

    public string? EnsureCapacity()
    {
        if (_store.Count < _policy.MaxMessages) return null;
        var evicted = _store.EvictOldest();
        _stats.SetMessageCount(_store.Count);
        return evicted;
    }
}
