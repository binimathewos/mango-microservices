# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Mango is a .NET 10 e-commerce microservices solution. It consists of independent ASP.NET Core Web API services behind an Ocelot API gateway, an MVC web front end, and a shared RabbitMQ messaging library. Every project targets `net10.0` with nullable + implicit usings enabled.

## Build & Run

There is **no working solution file for the whole repo** — `Mango.slnx` only references two projects (CouponApi + Web). Build and run projects individually by path:

```bash
# Build / restore a single project
dotnet build Services/Mango.Service.ProductApi/Mango.Service.ProductApi.csproj

# Run a service (uses Properties/launchSettings.json ports below)
dotnet run --project Services/Mango.Service.ProductApi
```

There are no test projects in this repo.

### Fixed local ports (from launchSettings.json / ocelot.json)

| Component | https | Notes |
|-----------|-------|-------|
| Gateway (Ocelot) | 7777 | Public entrypoint; routes to services below |
| ProductApi | 7000 | |
| CouponApi | 7001 | |
| AuthApi | 7002 | |
| ShoppingCartApi | 7003 | |
| OrderApi | 7004 | |
| RewardApi | 7050 | |
| EmailApi | 7239 | Background Service Bus consumer |
| Web (MVC front end) | 7223 | |

The Ocelot routes in `Gateway/Mango.Gateway/ocelot.json` hardcode these downstream ports, so they must match when running locally.

## Architecture

### Request flow
Browser → `Mango.Web` (MVC, cookie auth) → typed `HttpClient` services (`FrontEnd/Mango.Web/Service`) → **Ocelot gateway (7777)** → downstream API. `SD.*` static fields in `Mango.Web` hold service base URLs, populated from `ServiceUrls:*` config. The web tier authenticates with cookies but forwards the JWT as a Bearer token to APIs.

### Authentication
JWT is issued only by **AuthApi** (`JwtTokenGenerator`, signed with a symmetric secret from `ApiSettings:JwtOptions`). Every other API and the gateway *validate* the same token via the shared `AddAppAuthentication()` extension (each service has its own copy in `Extensions/WebApplicationBuilderExtensions.cs`) using `ApiSettings:{Issuer,Audience,Secret}`. These three values must be identical across all services or auth breaks. Protected Ocelot routes use `AuthenticationProviderKey: "Bearer"`.

### Databases (per-service, EF Core)
Each service owns its own `AppDbContext` and applies migrations **automatically at startup** via an `ApplyMigration()` call at the end of `Program.cs`. **All services use PostgreSQL** (`UseNpgsql`, `Npgsql.EntityFrameworkCore.PostgreSQL`); **AuthApi** additionally uses ASP.NET Identity (`ApplicationUser : IdentityUser`) on Postgres.

Connection strings come from `ConnectionStrings:DefaultConnection`. To add/modify schema, edit models then create a migration with `dotnet ef migrations add <Name> --project Services/<Service>`.

> Note: EF migrations are provider-specific. The committed migrations are Npgsql, so `Program.cs` uses `UseNpgsql` unconditionally (dev-focused). A production deploy would therefore also target Postgres — the Azure SQL production settings/workflows are left in place but are not wired to the current provider.

### Messaging (`Integration/Mango.MessageBus`)
Shared library wrapping **RabbitMQ** (`RabbitMQ.Client` v7 async API). `IMessageBus.PublishMessage(object, name)` is the producer side; it declares a **fanout exchange** named after `name` and publishes to it. Consumers each declare their own queue and bind it to that exchange, so single- and multi-consumer flows are uniform (e.g. the `OrderCreated` exchange is consumed by **both** EmailApi and RewardApi, each via its own queue). Producers: **AuthApi** (register-user), **ShoppingCartApi** (email-cart), **OrderApi** (order-created). Consumers: **EmailApi** (all three) and **RewardApi** (order-created), each a `RabbitMqConsumer` started/stopped via `UseRabbitMqConsumer()` on the app lifetime.

Important: `MessageBus` takes RabbitMQ connection settings via **constructor** (config keys `RabbitMq:{HostName,UserName,Password}`) — there is no parameterless constructor. Register it as:
```csharp
builder.Services.AddScoped<IMessageBus>(_ =>
    new MessageBus(
        builder.Configuration.GetValue<string>("RabbitMq:HostName"),
        builder.Configuration.GetValue<string>("RabbitMq:UserName"),
        builder.Configuration.GetValue<string>("RabbitMq:Password")));
```
`PublishMessage` and the consumers **no-op** when the host is empty or still a `<your-...>` placeholder, and consumer `Start()` swallows broker-connection errors, so a missing/unreachable broker does not break requests or crash startup. Local dev expects RabbitMQ at `localhost:5672` (guest/guest). Queue/exchange names live under `TopicAndQueueNames:*` in config.

### Common per-service conventions
- Controllers return a shared `ResponseDto` (`IsSuccess`, `Result`, `Message`) wrapper.
- AutoMapper maps entities ↔ DTOs via a static `MappingConfig.RegisterMaps()`.
- Swagger is registered with a JWT Bearer security definition; enabled in Development for most services, but **all environments** for AuthApi.

## Secrets & configuration
- `appsettings.Development.json` is **gitignored** — real local connection strings, JWT secrets, and RabbitMQ credentials go there, not in committed config.
- Committed `appsettings*.json` / `appsettings.Production.json` files hold **placeholders only**. In production, real secrets come from Azure App Service settings / Connection Strings, never from committed files.

## Deployment
Only **AuthApi** has CI/CD: `.github/workflows/deploy-authapi.yml` builds and publishes to the Azure Web App `addis-auth-api` on push to `main` (paths under AuthApi or MessageBus), using publish profile secret `AZURE_WEBAPP_PUBLISH_PROFILE`. See the `authapi-azure-deployment` memory for Azure resource details and deployment gotchas.
