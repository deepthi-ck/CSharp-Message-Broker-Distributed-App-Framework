namespace MessageBroker.BrokerCore;
public readonly record struct MessageKey(string Value)
{
    public override string ToString() => Value;
    public static MessageKey Of(string v) => new(v);
}
