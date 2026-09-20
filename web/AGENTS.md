# Web Client Agent Guide

Scope: `web/` SPA only. No backend, no mobile.

## Stack

- Vue 3 + Vite + TypeScript + Tailwind CSS v4 + Vue Router.
- SPA-only (no SSG/SSR). Static export to Cloudflare Pages. API via `X-Tenant-Slug` + JWT.

## Design Tokens (source of truth)

All values via CSS variables in `src/styles/tokens.css` (Tailwind `@theme`). Never hardcode hex/spacing outside tokens.

### Brand palette (dark-first)

- `ink: #0a0a0a` — page background
- `surface: #141412` — cards, raised elements
- `volt: #e8f000` — THE accent: primary CTAs, active/selected states, key data only
- `bone: #f5f5f0` — primary text
- `ash: #8b8b84` — secondary text, labels
- `line: #262622` — hairline borders
- Usage: Tailwind theme tokens (`bg-ink`, `text-bone`, `bg-volt`, `border-line`, `text-ash`, `bg-surface`). One volt pill action per screen max — restraint is the brand.

### Typography

- Display: `"Archivo Black"` — `font-display`, UPPERCASE, `tracking-squeeze` (-0.03em). Headlines, prices, the pill button.
- Body/UI: `"Archivo"` 400–700 — `font-body`. Names, labels, form text.
- Data/utility: `"JetBrains Mono"` — `font-mono`, uppercase + `tracking-widest` for labels; plain for times, durations, ticket codes.
- Self-hosted `.woff2` in `public/fonts/` (declared in `tokens.css`). Same files reusable by the Expo app.

### Spacing (4px base)

- Unit `4px`. Multiples: `1u 4px / 2u 8px / 3u 12px / 4u 16px / 5u 20px / 6u 24px / 8u 32px / 10u 40px`
- Layout: mobile-first single column, centered `max-w-md` on all breakpoints (parity with the future Expo app). No wide desktop layout.

### Border / Radius / Shadow

- Border: `1px solid var(--color-line)` on cards/inputs
- Radius: `rounded-full` (999px) pills for ALL primary actions + selectable chips; `rounded-2xl` cards; `rounded-lg` inputs
- No shadows. Depth comes from `surface` vs `ink` and hairline borders.

## Tailwind / Styling Rules

- Utility-first only. No `<style scoped>` except complex animations.
- Use `cn()` (`src/utils/cn.ts` = `clsx` + `tailwind-merge`) to merge conditional classes: `:class="cn('px-4 py-2 rounded-md', props.class)"`.
- No arbitrary values (e.g. `bg-[#123456]`) outside tokens. Reference variables: `bg-[var(--color-primary)]` or Tailwind theme tokens.
- Responsive: mobile-first, breakpoints `sm/md/lg`.

## Component Rules

- `<script setup lang="ts">` only.
- Props typed via `defineProps<>` with explicit interfaces/types (no `any`, no `var`).
- Atomic structure: `src/components/ui/` (primitives: Button, Card, Input), `src/components/layout/` (Header, Container), `src/components/features/<section>/` (domain).
- Colocate feature component with its section types if needed; do not duplicate.

## Types Rules

- All types in `src/types/`, one file per section: `auth.ts`, `booking.ts`, `barbershop.ts`, `common.ts` (shared).
- No duplicate interfaces. Single source of truth per domain entity; re-export from section file.
- Use explicit types, `type` vs `interface` consistently (prefer `type` for unions, `interface` for extensible objects — pick one and stay consistent within a file).

## Accessibility

- Semantic HTML, `aria-label` where appropriate, keyboard navigable.
- Contrast `>= 4.5:1` body text on backgrounds.
- Dark `ink` base, `bone` text (contrast ≥ 4.5:1), volt reserved for CTAs/active states.

## Client Palette Switching

- Allow 3–4 palette options via CSS variable overrides (e.g. `data-theme="palette-2"`).
- Toggle must not change layout — only color variables. Keep typography/spacing constant.
