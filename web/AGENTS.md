# Web Client Agent Guide

Scope: `web/` SPA only. No backend, no mobile.

## Stack

- Vue 3 + Vite + TypeScript + Tailwind CSS v4 + Vue Router.
- SPA-only (no SSG/SSR). Static export to Cloudflare Pages. API via `X-Tenant-Slug` + JWT.

## Design Tokens (source of truth)

All values via CSS variables in `src/styles/tokens.css` (Tailwind `@theme`). Never hardcode hex/spacing outside tokens.

### Brand palette

- `primary: #2b2a26` — CTAs, headings emphasis, links
- `accent: #e69b00` — highlights, success/warning/danger tinted variants
- `neutral: #f0e6dd` — warm background base, card surfaces
- `text: #1a1a1a` — body text (4.5:1 contrast on neutral)
- Default palette as above. Clients may choose 3–4 complementary palettes that pair with this base — switch via CSS variable override without layout changes.

### Typography

- `font-family: Inter, system-ui, -apple-system, "Segoe UI", Roboto, Arial, sans-serif`
- Scale (base 16px): `Display1 48px / Display2 40px / Heading1 32px / Heading2 28px / Heading3 22px / Body 16px / Small 12px`
- `line-height: 1.15` for Display/Headings, `1.5` for Body/Small
- `font-weight: 700` headings, `400–600` body (contextual)
- `letter-spacing: -0.02em` headings, `normal` body

### Spacing (4px base)

- Unit `4px`. Multiples: `1u 4px / 2u 8px / 3u 12px / 4u 16px / 5u 20px / 6u 24px / 8u 32px / 10u 40px`
- Container paddings: `8–24px` small screens, `24–64px` large screens
- Gaps: use `4 / 8 / 12 / 16 / 24 / 32px` only

### Border / Radius / Shadow

- Border: `1px solid rgba(0,0,0,0.08)` cards
- Radius: `6px` default, `8px` large surfaces, `4px` small components
- Shadow presets: `subtle 0 1px 3px rgba(0,0,0,0.08) / medium 0 4px 12px rgba(0,0,0,0.12) / strong 0 8px 20px rgba(0,0,0,0.15)`

### Color usage

- Background: neutral or light variants
- Links: primary or accent
- States: accent-tinted variants (never introduce outside-palette hues)

### Example token usage

- Button primary: `background: var(--color-primary); color: white; border-radius: var(--radius-default); padding: 10px 14px`
- Card: `background: white; border: var(--border-card); border-radius: var(--radius-lg); padding: var(--space-4); box-shadow: var(--shadow-subtle)`
- Heading: `font-size: var(--text-h1); line-height: var(--leading-tight)`

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
- Warm neutral base, bold contrasted CTAs.

## Client Palette Switching

- Allow 3–4 palette options via CSS variable overrides (e.g. `data-theme="palette-2"`).
- Toggle must not change layout — only color variables. Keep typography/spacing constant.
