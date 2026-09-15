# Barber Booking SaaS — Monorepo Guide

Multi-tenant barber booking SaaS (one backend, one database per client).

## Layout

- `Backend/` — .NET 10 Web API. Conventions: `Backend/AGENTS.md`.
- `web/` — Vue 3 + Vite SPA client. Conventions: `web/AGENTS.md`.
- `mobile/` — Expo mobile client (later).
- `db/` — local Postgres compose + init notes. EF migrations in `Backend/` stay the source of truth.

## Git Rules

- NEVER `git commit`, `git push`, or otherwise write git history. Only propose the commit message plus the exact file list — the user executes.
- Commit messages: subject line only (`type: summary`, e.g. `feat: add booking availability query`), no body, no footer, unless explicitly asked.
