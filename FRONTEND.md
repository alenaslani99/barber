# Frontend Plan (web)

## Stack

- Plain Vue 3 + Vite (Router + Pinia). Scaffolds in `barber/web/`.
- SPA-only: no landing pages, no SEO requirement. (Nuxt/Angular rejected: SSR machinery unneeded, enterprise weight unneeded.)

## App shape

- Single booking origin for all shops (slug picks the tenant). One CORS entry regardless of client count.
- Per-shop subdomains only if ever needed for marketing pages — not in scope.

## API integration

- Same tenant API: `X-Tenant-Slug` header + JWT (login/register/refresh/logout live).
- Dev CORS for localhost origins is configured in the Api.

## Hosting (later)

- Static export to Cloudflare Pages (free tier). API + Postgres + Redis stay on the VPS.
