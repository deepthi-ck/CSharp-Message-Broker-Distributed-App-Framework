using System.Security.Cryptography;
using System.Text;
namespace MessageBroker.Distribution;

public sealed class PartitionRouter
{
    private readonly NodeRegistry _registry;
    private readonly int _slotCount;
    public PartitionRouter(NodeRegistry registry, int slotCount)
    { _registry = registry; _slotCount = Math.Max(1, slotCount); }

    public int ResolveSlot(string keyOrTopic)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(keyOrTopic));
        return (int)(BitConverter.ToUInt32(hash, 0) % (uint)_slotCount);
    }

    public BrokerNode Route(string keyOrTopic) => _registry.Get(ResolveSlot(keyOrTopic));
    public BrokerNode ReplicaOf(string keyOrTopic)
    {
        var primary = ResolveSlot(keyOrTopic);
        var replica = (primary + 1) % _slotCount;
        return _registry.Get(replica);
    }
}
