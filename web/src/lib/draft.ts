const DRAFT_PREFIX = 'barber.booking.draft';

export const BOOKING_DRAFT_KEY = `${DRAFT_PREFIX}:v1`;

export function clearBookingDrafts(): void {
  const doomed: string[] = [];
  for (let i = 0; i < localStorage.length; i += 1) {
    const key = localStorage.key(i);
    if (key && key.startsWith(DRAFT_PREFIX)) doomed.push(key);
  }
  for (const key of doomed) localStorage.removeItem(key);
}
