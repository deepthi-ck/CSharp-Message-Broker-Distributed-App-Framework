using MessageBroker.Frontend;
using MessageBroker.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<BrokerDashboard>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5082/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<BrokerClient>();
builder.Services.AddSingleton(VersionInfo.FromEnvironment(
    Environment.GetEnvironmentVariable("FRONTEND_DOTNET") ?? "6",
    Environment.GetEnvironmentVariable("BACKEND_DOTNET") ?? "8",
    Environment.GetEnvironmentVariable("BRANCH_NAME") ?? "CSharp_FE6_BE8"));
await builder.Build().RunAsync();
