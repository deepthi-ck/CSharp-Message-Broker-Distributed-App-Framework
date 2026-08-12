using System.Text.Json;
using MessageBroker.Backend;
using MessageBroker.BrokerCore;
using MessageBroker.Distribution;
using MessageBroker.Shared;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var configuration = LoadConfiguration();
var policy = BrokerPolicy.FromConfiguration(configuration);
var store = new InMemoryMessageStore();
var stats = new BrokerStatistics();
stats.SetNodeCount(policy.NodeSlotCount);
stats.SetPartitionCount(policy.NodeSlotCount);

var registry = new NodeRegistry();
for (var i = 0; i < policy.NodeSlotCount; i++)
    registry.Register(new BrokerNode(i, store, policy));

var router = new PartitionRouter(registry, policy.NodeSlotCount);
var replication = new ReplicationManager();
var eviction = new EvictionManager(store, policy, stats);
var expiration = new ExpirationManager(store, stats);
var guard = new OutboxInboxGuard();
var manager = new BrokerManager(router, replication, stats, eviction, expiration, store, guard);
var service = new BrokerService(manager);

var frontend = Environment.GetEnvironmentVariable("FRONTEND_DOTNET") ?? Detect("FrontendDotnetVersion") ?? "6";
var backend = Environment.GetEnvironmentVariable("BACKEND_DOTNET") ?? Detect("BackendDotnetVersion") ?? ExtractTfm() ?? "8";
var branch = Environment.GetEnvironmentVariable("BRANCH_NAME") ?? Detect("BranchName") ?? "CSharp_FE6_BE8";
var version = VersionInfo.FromEnvironment(frontend, backend, branch);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton(policy);
builder.Services.AddSingleton(store);
builder.Services.AddSingleton(stats);
builder.Services.AddSingleton(registry);
builder.Services.AddSingleton(router);
builder.Services.AddSingleton(replication);
builder.Services.AddSingleton(eviction);
builder.Services.AddSingleton(expiration);
builder.Services.AddSingleton(guard);
builder.Services.AddSingleton(manager);
builder.Services.AddSingleton(service);
builder.Services.AddSingleton(version);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("csharp-message-broker"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddConsoleExporter());

var app = builder.Build();
app.UseCors();
app.MapBrokerApi();
app.MapGet("/", () => Results.Json(new { application = version.Application, branch = version.Branch, status = "running" }));

if (string.Equals(Environment.GetEnvironmentVariable("SEED_SAMPLE_DATA"), "1", StringComparison.Ordinal))
    LoadSample(service);

var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? configuration.ApiBaseUrl;
app.Urls.Clear();
app.Urls.Add(urls);
Console.WriteLine($"Broker platform listening on {urls} branch={branch} FE={frontend} BE={backend}");
app.Run();

static BrokerConfiguration LoadConfiguration()
{
    var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "config", "brokersettings.json"));
    if (File.Exists(path))
        return JsonSerializer.Deserialize<BrokerConfiguration>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new BrokerConfiguration();
    return new BrokerConfiguration();
}

static void LoadSample(BrokerService service)
{
    var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "data", "sample-broker-data.json"));
    if (!File.Exists(path)) return;
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    if (!doc.RootElement.TryGetProperty("seed_messages", out var msgs)) return;
    foreach (var m in msgs.EnumerateArray())
    {
        try
        {
            service.Publish(new BrokerRequest
            {
                MessageId = m.GetProperty("messageId").GetString(),
                Topic = m.GetProperty("topic").GetString(),
                Payload = m.GetProperty("payload").GetString()
            });
        }
        catch { }
    }
}

static string? Detect(string key)
{
    var props = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Directory.Build.props"));
    if (!File.Exists(props)) return null;
    var text = File.ReadAllText(props);
    var tag = $"<{key}>";
    var start = text.IndexOf(tag, StringComparison.Ordinal);
    if (start < 0) return null;
    start += tag.Length;
    var end = text.IndexOf('<', start);
    return end < 0 ? null : text[start..end].Trim();
}

static string? ExtractTfm()
{
    var tfm = AppContext.TargetFrameworkName;
    if (tfm is null) return null;
    var idx = tfm.LastIndexOf('v');
    return idx < 0 ? null : tfm[(idx + 1)..].Split('.')[0];
}

public partial class Program { }
