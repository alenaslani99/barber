# TODO — Auth Wiring (Backend + Frontend)

## Backend
- [ ] Add Vite dev origin to CORS in `appsettings.Development.json` (`http://localhost:5173`)
- [ ] Confirm tenant slug `demo` exists and is active in catalog DB (or provide real slug)

## Frontend (`web/`)
- [ ] Add `.env.development` with `VITE_API_URL=http://localhost:5118` and `VITE_TENANT_SLUG=demo`
- [ ] Create `src/lib/api.ts` — fetch wrapper (base URL, `X-Tenant-Slug` header, ProblemDetails → typed `ApiError`)
- [ ] Create `src/stores/auth.ts` (Pinia) — access token in memory, refresh token in localStorage, login/register/logout/refresh, proactive refresh before `accessTokenExpiresAt`, `displayName` from JWT decode
- [ ] `LoginView` — call store on submit; 401 → `POGREŠAN EMAIL ILI LOZINKA` banner; network error → `GREŠKA U VEZI, POKUŠAJ PONOVO`; loading state on button
- [ ] `RegisterView` — call store on submit; 409 → `EMAIL JE ZAUZET` / `TELEFON JE ZAUZET` on the field; 400 field errors pass through
- [ ] `BookingView reserve()` — gate on `isAuthenticated`: keep draft, redirect `/login?next=/` when logged out
- [ ] `ShopHeader` — swap `PRIJAVA` for `USERNAME → /account` when logged in
- [ ] Add `/account` stub route (real page later)

## Reference
- `POST /api/auth/register` `{email,password,firstName,lastName,phone}` → `{accessToken,accessTokenExpiresAt,refreshToken}`
- `POST /api/auth/login` `{email,password}` → same
- `POST /api/auth/refresh`, `POST /api/auth/logout` with `{refreshToken}`
