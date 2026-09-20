# TODO — Auth Wiring (Backend + Frontend)

## Backend — done
- [x] Refresh token as http-only cookie (`barber_refresh`, Lax, `/api/auth`, 14 days; `Secure` off in dev)
- [x] `refresh`/`logout` read the cookie, no body
- [x] `given_name` + `family_name` claims on the access token
- [x] `AccessTokenMinutes` 60 → 15
- [x] `.http` examples updated to the cookie flow
- [ ] Confirm tenant slug `demo` exists and is active in catalog DB (done via SQL: `demo | t`)

## Frontend (`web/`) — done
- [x] `.env.development` with `VITE_API_URL=http://localhost:5118` and `VITE_TENANT_SLUG=demo`
- [x] `src/lib/api.ts` — fetch wrapper (base URL, `X-Tenant-Slug`, `credentials: include`, ProblemDetails → typed `ApiError`, single 401-refresh-and-retry)
- [x] `src/stores/auth.ts` (Pinia) — access token in memory, login/register/logout/refresh with single-flight, proactive refresh 60s before expiry, `displayName` from JWT
- [x] `src/main.ts` — Pinia + silent refresh on boot
- [x] `LoginView` — real submit; 401 → `POGREŠAN EMAIL ILI LOZINKA` banner; network error → `GREŠKA U VEZI, POKUŠAJ PONOVO`; loading state
- [x] `RegisterView` — real submit; 409 → `EMAIL JE ZAUZET` / `TELEFON JE ZAUZET` on the field; 400 field errors pass through
- [x] `BookingView reserve()` — gate on `isAuthenticated`: draft to localStorage, redirect `/login?next=/`, restore after login
- [x] `ShopHeader` — `USERNAME → /account` when logged in
- [x] `/account` stub route + guard (real page later)

## Reference
- `POST /api/auth/register` `{email,password,firstName,lastName,phone}` → `{accessToken,accessTokenExpiresAt}` + refresh cookie
- `POST /api/auth/login` `{email,password}` → same
- `POST /api/auth/refresh`, `POST /api/auth/logout` — cookie only, no body
