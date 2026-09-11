# Barber Booking SaaS — Backend Agent Guide

Scope: backend API + databases only. No frontend, no mobile, no web.

## Stack

- .NET 10 (`net10.0`), ASP.NET Core Web API, EF Core 10 + Npgsql.
- Postgres: one catalog DB + one database per tenant (client).
- Redis for cache and background jobs.
- Auth: JWT access + refresh tokens. Tenant resolved per request via `X-Tenant-Slug` header (or subdomain) -> catalog lookup -> tenant connection string.

## Folder Structure (non-strict rule)

- Prefer layered projects under one solution: `Api`, `Domain`, `DataAccess`, `Application` (command/query handlers), `Tests`.
- Non-strict: merge or split projects when it keeps solo velocity. Never duplicate API projects, never commit `.vs/`, keep one solution file in `Backend/`, no loose SQL dumps at root.
- Api interior (non-strict): `Controllers/`, `DTO/`, `Validation/`, `Extensions/`, `Core/`, `Program.cs`, `appsettings*.json`, one `.http` file. Omit `wwwroot/` unless serving static files. Never commit `bin/`, `obj/`, `*.csproj.user`.
- DataAccess interior (non-strict): `Configurations/`, `Migrations/`, one context per database (`CatalogContext`, `TenantContext`). No business logic here.
- Domain interior (non-strict): flat entities plus `Entity` / `NamedEntity` base only. No EF attributes, no business logic.
- Application interior (non-strict): `UseCases/` (command/query handlers), `Validators/`, `Exceptions/`, `Logging/`. Thin handlers, validation next to its use case.

## Patterns

- Light command/query separation (not full CQRS): separate writes from reads at handler/folder level, sharing one EF model and one Postgres schema per tenant. Keep handlers small, one intent per handler. No separate read/write stores, no events, no event sourcing unless a proven hotspot requires it. This domain is mostly simple CRUD, so full CQRS would slow solo velocity.
- SOLID: apply where it helps readability and testability. Do NOT follow SOLID 100% if it damages the code (e.g. no speculative interfaces, no one-method classes, no deep inheritance). Prefer composition and explicit code over clever abstractions.

## C# Style

- Use explicit types everywhere, never `var`.

## API Rules

- Use the `dotnet-webapi` skill when adding endpoints: correct HTTP semantics, OpenAPI metadata, global error-handling middleware.
- Every new endpoint ships with OpenAPI annotations and a `.http` test file.
- No business logic in controllers/minimal APIs — delegate to command/query handlers.

## Data Rules

- All tenant data access goes through the tenant `DbContext` resolved from the current tenant. Never hardcode connection strings.
- Catalog `DbContext` is only for tenant registry and cross-tenant lookups.
- Use the `optimizing-ef-core-queries` skill for slow queries. Prefer `AsNoTracking` for reads, explicit includes over lazy loading, no N+1.
- Migrations must work across N tenant databases: new migration + migrator path that applies to catalog and rolls across all tenant DBs.
- Prefer EF Core LINQ over raw SQL. Raw SQL only with a comment explaining why.

## Build Conventions

- SDK pinned via `global.json` (see `setup-local-sdk` skill).
- NuGet Central Package Management via `Directory.Packages.props` (see `convert-to-cpm` skill).
- `Nullable enable`, `dotnet build`, `dotnet format --verify-no-changes` before commit.

## Test Conventions

- Test projects use MSTest on Microsoft.Testing.Platform (see `scaffold-dotnet-test-project` skill).
- Run via the `run-tests` skill (correct `dotnet test` syntax, filters). Example: `dotnet test --filter "Category=Unit"`.
- Naming: `Method_Scenario_Expected`. Unit tests never touch live Postgres — use fakes/in-memory doubles for handlers.

## Git Rules

- NEVER `git commit`, `git push`, or otherwise write git history. Only propose the commit message plus the exact file list — the user executes.
- Commit messages: subject line only (`type: summary`, e.g. `feat: add booking availability query`), no body, no footer, unless explicitly asked.
