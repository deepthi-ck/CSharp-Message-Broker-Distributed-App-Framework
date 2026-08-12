namespace MessageBroker.BrokerCore;
public sealed class BrokerStatistics
{
    private long _publish, _consume, _ack, _delivered, _notFound;
    private int _messages, _nodes, _partitions;
    public long PublishCount => Interlocked.Read(ref _publish);
    public long ConsumeCount => Interlocked.Read(ref _consume);
    public long AckDeleteCount => Interlocked.Read(ref _ack);
    public long DeliveredCount => Interlocked.Read(ref _delivered);
    public long NotFoundCount => Interlocked.Read(ref _notFound);
    public int MessageCount => Volatile.Read(ref _messages);
    public int NodeCount => Volatile.Read(ref _nodes);
    public int PartitionCount => Volatile.Read(ref _partitions);
    public void RecordPublish() => Interlocked.Increment(ref _publish);
    public void RecordConsume() => Interlocked.Increment(ref _consume);
    public void RecordAck() => Interlocked.Increment(ref _ack);
    public void RecordDelivered() => Interlocked.Increment(ref _delivered);
    public void RecordNotFound() => Interlocked.Increment(ref _notFound);
    public void SetMessageCount(int v) => Volatile.Write(ref _messages, v);
    public void SetNodeCount(int v) => Volatile.Write(ref _nodes, v);
    public void SetPartitionCount(int v) => Volatile.Write(ref _partitions, v);
    public object Snapshot() => new
    {
        publish_count = PublishCount,
        consume_count = ConsumeCount,
        ack_delete_count = AckDeleteCount,
        delivered_count = DeliveredCount,
        not_found_count = NotFoundCount,
        message_count = MessageCount,
        node_count = NodeCount,
        topic_partition_count = PartitionCount
    };
}
