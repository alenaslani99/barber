# Backend TODO

- [ ] `[Authorize]` the tenant probe + enforce tenant claim matches the resolved slug.
- [ ] Bookings slice: availability query, create booking with double-booking guard.
- [ ] Barbershop / Service / Staff CRUD (Owner and Barber roles).
- [ ] Notification outbox sender (background worker: email, SMS, push).
- [ ] Handler unit tests (MSTest, fakes only, no live Postgres).
- [ ] Tenant provisioning flow (new client: catalog row + database + migrate + seed owner).
- [ ] Production hardening: secrets via env, tighten CORS, Jwt key via env, Redis wiring.
