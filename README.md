# C# Message Broker / Distributed App Framework

Minimal in-process message broker with topic/partition routing, replication, TTL, eviction, outbox/inbox idempotency, Blazor client, and FE/BE .NET version matrix.

## Build

```bash
python build.py
```

## Run the UI (two terminals)

```powershell
# Terminal 1 — Broker API
$env:ASPNETCORE_URLS="http://localhost:5082"
dotnet run --project backend_csharp/backend_csharp.csproj -c Release

# Terminal 2 — Blazor WebAssembly UI
dotnet run --project frontend_csharp/frontend_csharp.csproj
```

Open **http://localhost:5172**

Pages (top navigation): **Home** → **Publish** → **Consume** → **Acknowledge** → **Stats**

API: `http://localhost:5082` · Health: `http://localhost:5082/health`

The UI uses built-in Blazor WebAssembly routing, layout, and `HttpClient` only.

Conceptual inspiration only: food-delivery-microservices messaging patterns (not a clone).
