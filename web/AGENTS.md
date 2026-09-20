# Web Client Agent Guide

Scope: `web/` SPA only. No backend, no mobile.

## Stack

- Vue 3 + Vite + TypeScript + Tailwind CSS v4 + Vue Router + Pinia.
- SPA-only (no SSG/SSR). Static export to Cloudflare Pages. API via `X-Tenant-Slug` + JWT (later, dummy data for now).

## Design Tokens (source of truth)

All values via CSS variables in `src/styles/tokens.css` (Tailwind `@theme`). Never hardcode hex/spacing outside tokens.

### Brand palette (black/white only)

- `ink: #080808` — page background (very dark black)
- `surface: #111111` — cards, raised elements
- `bone: #F5F5F0` — primary text + primary CTA background
- `ash: #A1A1A1` — secondary text, labels, disabled
- `line: #262626` — hairline borders, dividers
- `blaze: #FF5C00` — single accent, links + key actions only (max 1–2 per screen). Never for selected states (those stay inverted `bg-bone text-ink`).
- `alarm: #FF4D4D` — validation errors only (input borders + messages). Never decoration.
- Usage: `bg-ink`, `bg-surface`, `text-bone`, `text-ash`, `border-line`, `text-blaze`, `border-blaze`, `bg-blaze`.

### Typography (Bebas everywhere)

- All text uses `"Bebas Neue"` (`font-display`, also set as `font-sans` base) — self-hosted in `public/fonts/BebasNeue-Regular.woff2` (+ `.woff` fallback), declared in `tokens.css`.
- No mono font. Times, dates, codes, labels all in Bebas.
- Style: UPPERCASE everywhere, letter-spacing `0.04em`–`0.08em` for readability. Body base size `18px`+ since Bebas runs narrow.
- Scale: shop name huge (display), prices/section titles large, labels smaller + `text-ash`.

### Spacing (4px base)

- Unit `4px`. Multiples: `1u 4px / 2u 8px / 3u 12px / 4u 16px / 5u 20px / 6u 24px / 8u 32px / 10u 40px`
- Layout: mobile-first single column. Full width (`w-full`) on small screens, half width centered (`lg:w-1/2 mx-auto`) on big screens. No wide desktop layout.

### Border / Radius / Shadow

- Boxy, sharp-edged design. `rounded-none` everywhere. NEVER pills, NEVER `rounded-full/2xl/lg`.
- Border: `1px solid var(--color-line)` on cards/inputs/option squares.
- No shadows. Depth comes from `surface` vs `ink` and hairline borders.

## Tailwind / Styling Rules

- Utility-first only. No `<style scoped>` except complex animations.
- Use `cn()` (`src/utils/cn.ts` = `clsx` + `tailwind-merge`) to merge conditional classes.
- No arbitrary values (e.g. `bg-[#123456]`) outside tokens. Use theme tokens.
- Responsive: mobile-first, breakpoints `sm/md/lg`.

## Header Rule (no classic nav)

- No classic navbar. `ShopHeader` = barbershop info block:
  - Top-right corner only: `LOGIN` link, or `USERNAME` -> `/account` when logged in.
  - Big Bebas shop name, 1-line description, open days/hours grid, address/phone.
  - Sharp `border-y border-line` dividers. No logo image, no nav links.

## Booking UX Rules (index `/`)

- 4 steps in order: `01 BARBER` -> `02 SERVICE` -> `03 TIME` -> `04 RESERVE`.
- Single service per booking only (no multi-select).
- Barber list: all barbers, no shortcut entries.
- Selecting barber/service auto-advances. Time requires date + slot.
- Sticky bottom bar with selection recap + `CONTINUE` / `BACK`. Square buttons only.
- Reserve gating: browsing is public, clicking `RESERVE` while logged out saves draft and pushes to `/login?next=/`. `/account` requires auth.
- Dummy data lives in `src/data/mock.ts` until API lands.

## Component Rules

- `<script setup lang="ts">` only.
- Props typed via `defineProps<>` with explicit interfaces/types (no `any`, no `var`).
- Atomic structure: `src/components/ui/` (Button, Card, Input, SectionTitle), `src/components/layout/` (Container, ShopHeader), `src/components/features/booking/` (Stepper, BarberOption, ServiceOption, DateStrip, TimeGrid, BookingSummary).
- Square interactive elements only (`rounded-none`). Selected = `bg-bone text-ink`, unselected = `bg-surface text-bone border-line`.
- Form inputs use `defineModel` (no emits); validation owned by the input via `rules` + exposed `validate()`.

## Types Rules

- All types in `src/types/`, one file per section: `auth.ts`, `booking.ts`, `barbershop.ts`, `common.ts` (shared).
- No duplicate interfaces. Single source of truth per domain entity.
- Prefer `interface` for extensible objects, `type` for unions. Stay consistent within a file.

## Accessibility

- Semantic HTML, `aria-label` where appropriate, keyboard navigable (`button` elements, visible focus).
- Contrast `>= 4.5:1`: `bone` on `ink`, `ink` on `bone` for selected states.
