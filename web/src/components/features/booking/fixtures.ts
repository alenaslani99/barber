import type { Service } from '../../../types/booking';

export const fixtureServices: Service[] = [
  { id: 'classic-cut', name: 'Classic Cut', durationMinutes: 30, priceCents: 2500 },
  { id: 'skin-fade', name: 'Skin Fade', durationMinutes: 45, priceCents: 3000 },
  { id: 'beard-trim', name: 'Beard Trim', durationMinutes: 20, priceCents: 1500 },
  { id: 'cut-and-beard', name: 'Cut + Beard', durationMinutes: 60, priceCents: 3800 },
  { id: 'kids-cut', name: 'Kids Cut', durationMinutes: 30, priceCents: 2000 },
];
