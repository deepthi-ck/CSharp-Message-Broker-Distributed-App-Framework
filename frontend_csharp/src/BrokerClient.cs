using System.Net.Http.Json;
using MessageBroker.Shared;

namespace MessageBroker.Frontend;

public sealed class BrokerClient
{
    private readonly HttpClient _http;
    public BrokerClient(HttpClient http) => _http = http;
    public string? LastError { get; private set; }
    public MessageEntry? LastEntry { get; private set; }
    public event Action? StateChanged;

    public async Task<BrokerResponse?> PublishAsync(string messageId, string topic, string payload)
    {
        var response = await _http.PostAsJsonAsync("/broker/publish",
            new BrokerRequest { MessageId = messageId, Topic = topic, Payload = payload });
        return await ReadAsync(response);
    }

    public async Task<BrokerResponse?> ConsumeAsync(string topic, bool preferReplica = false)
    {
        var q = preferReplica ? "?replica=true" : string.Empty;
        var response = await _http.GetAsync($"/broker/consume/{Uri.EscapeDataString(topic)}{q}");
        return await ReadAsync(response);
    }

    public async Task<BrokerResponse?> AckAsync(string id)
    {
        var response = await _http.PostAsync($"/broker/ack/{Uri.EscapeDataString(id)}", null);
        return await ReadAsync(response);
    }

    public async Task<BrokerResponse?> DeleteAsync(string id)
    {
        var response = await _http.DeleteAsync($"/broker/messages/{Uri.EscapeDataString(id)}");
        return await ReadAsync(response);
    }

    public Task<object?> StatsAsync() => _http.GetFromJsonAsync<object>("/broker/stats");
    public Task<object?> HealthAsync() => _http.GetFromJsonAsync<object>("/health");
    public Task<object?> VersionAsync() => _http.GetFromJsonAsync<object>("/version");

    private async Task<BrokerResponse?> ReadAsync(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<BrokerResponse>();
            if (payload is null) { LastError = "Empty response"; return null; }
            LastError = payload.Success ? null : payload.Message;
            LastEntry = payload.Entry ?? LastEntry;
            StateChanged?.Invoke();
            return payload;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            StateChanged?.Invoke();
            return null;
        }
    }
}
