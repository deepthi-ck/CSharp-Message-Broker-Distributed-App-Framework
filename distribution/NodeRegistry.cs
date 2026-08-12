namespace MessageBroker.Distribution;
public sealed class NodeRegistry
{
    private readonly Dictionary<int, BrokerNode> _nodes = new();
    public void Register(BrokerNode node) => _nodes[node.Slot] = node;
    public BrokerNode Get(int slot) =>
        _nodes.TryGetValue(slot, out var n) ? n : throw new KeyNotFoundException($"Node slot not registered: {slot}");
    public IReadOnlyCollection<BrokerNode> All => _nodes.Values;
    public int Count => _nodes.Count;
}
