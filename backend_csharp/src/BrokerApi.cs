using MessageBroker.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MessageBroker.Backend;

public static class BrokerApi
{
    public static IEndpointRouteBuilder MapBrokerApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", (BrokerService svc) => Results.Json(svc.Health()));
        app.MapGet("/version", (VersionInfo v) => Results.Json(new
        {
            application = v.Application,
            frontend_dotnet = v.FrontendDotnet,
            backend_dotnet = v.BackendDotnet,
            branch = v.Branch,
            broker = v.Broker
        }));
        app.MapGet("/broker/stats", (BrokerService svc) => Results.Json(svc.Stats()));
        app.MapPost("/broker/publish", (BrokerRequest request, BrokerService svc) =>
        {
            var response = svc.Publish(request);
            return response.Success ? Results.Json(response) : Results.BadRequest(response);
        });
        app.MapGet("/broker/consume/{topic}", (string topic, BrokerService svc, bool replica = false) =>
        {
            var response = svc.Consume(topic, replica);
            return response.Success ? Results.Json(response) : Results.NotFound(response);
        });
        app.MapPost("/broker/ack/{id}", (string id, BrokerService svc) => Results.Json(svc.Ack(id)));
        app.MapDelete("/broker/messages/{id}", (string id, BrokerService svc) => Results.Json(svc.Delete(id)));
        return app;
    }
}
