using MessageBroker.BrokerCore;
namespace MessageBroker.Backend;
public sealed class ExpirationManager
{
    private readonly InMemoryMessageStore _store;
    private readonly BrokerStatistics _stats;
    public ExpirationManager(InMemoryMessageStore store, BrokerStatistics stats) { _store = store; _stats = stats; }
    public IReadOnlyList<string> Sweep()
    {
        var expired = _store.EvictExpired();
        Refresh();
        return expired;
    }
    public void Refresh() => _stats.SetMessageCount(_store.Count);
}
