namespace MessageBroker.Shared;

public sealed class BrokerResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = "OK";
    public string Message { get; set; } = string.Empty;
    public MessageEntry? Entry { get; set; }
    public object? Stats { get; set; }
}
