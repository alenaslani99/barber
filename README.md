# Barber

Multi-tenant barber booking SaaS (one backend, one database per client).

## Layout

- `Backend/` — .NET 10 Web API (Api, Domain, DataAccess, Application, Tests)
- `web/` — Nuxt web client (later)
- `mobile/` — Expo mobile client (later)

## Backend quickstart

Prereqs: .NET 10 SDK, Postgres, Redis.

```sh
dotnet build Backend/Backend.slnx
```

Conventions: see `AGENTS.md`.
