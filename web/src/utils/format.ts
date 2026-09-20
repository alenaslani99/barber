const priceFormatter = new Intl.NumberFormat('de-DE', {
  style: 'currency',
  currency: 'EUR',
  minimumFractionDigits: 0,
});

export function formatPrice(priceCents: number): string {
  return priceFormatter.format(priceCents / 100);
}

export function formatDuration(durationMinutes: number): string {
  return `${durationMinutes} min`;
}
