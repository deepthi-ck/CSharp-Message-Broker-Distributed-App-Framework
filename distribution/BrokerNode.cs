using MessageBroker.BrokerCore;
using MessageBroker.Shared;
namespace MessageBroker.Distribution;

public sealed class BrokerNode
{
    private readonly InMemoryMessageStore _store;
    private readonly BrokerPolicy _policy;
    public int Slot { get; }
    public BrokerNode(int slot, InMemoryMessageStore store, BrokerPolicy policy)
    { Slot = slot; _store = store; _policy = policy; }

    public MessageEntry Publish(string id, string topic, string payload) =>
        _store.Publish(id, topic, payload, TimeSpan.FromSeconds(_policy.MessageTtlSeconds), $"slot-{Slot}");
    public MessageEntry? Consume(string topic) => _store.Consume(topic);
    public MessageEntry? Peek(string topic) => _store.Peek(topic);
    public bool AckDelete(string id) => _store.AckDelete(id);
    public bool Contains(string id) => _store.Contains(id);
}
