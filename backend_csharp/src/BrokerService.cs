using MessageBroker.Shared;
namespace MessageBroker.Backend;

public sealed class BrokerService
{
    private readonly BrokerManager _manager;
    public BrokerService(BrokerManager manager) => _manager = manager;
    public BrokerManager Manager => _manager;

    public BrokerResponse Publish(BrokerRequest request)
    {
        var id = string.IsNullOrWhiteSpace(request.MessageId)
            ? (string.IsNullOrWhiteSpace(request.Key) ? $"msg:{Guid.NewGuid():N}" : request.Key!)
            : request.MessageId!;
        var topic = string.IsNullOrWhiteSpace(request.Topic) ? "default" : request.Topic!;
        var payload = request.Payload ?? string.Empty;
        return _manager.Publish(id, topic, payload);
    }

    public BrokerResponse Consume(string topic, bool preferReplica = false) =>
        _manager.Consume(topic, preferReplica);

    public BrokerResponse Ack(string id) => _manager.AckDelete(id);
    public BrokerResponse Delete(string id) => _manager.AckDelete(id);
    public object Stats() => _manager.Stats.Snapshot();
    public object Health() => _manager.Health();
}
