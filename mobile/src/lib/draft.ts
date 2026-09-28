import AsyncStorage from '@react-native-async-storage/async-storage';

const DRAFT_PREFIX = 'barber.booking.draft';

export const BOOKING_DRAFT_KEY = `${DRAFT_PREFIX}:v1`;

export interface BookingDraft {
  barberId: string | null;
  serviceId: string | null;
  dateIso: string | null;
  time: string | null;
  notes: string;
  step: number;
}

export async function saveBookingDraft(draft: BookingDraft): Promise<void> {
  try {
    await AsyncStorage.setItem(BOOKING_DRAFT_KEY, JSON.stringify(draft));
  } catch {
    // Draft is best-effort; booking still works without it.
  }
}

export async function loadBookingDraft(): Promise<BookingDraft | null> {
  try {
    const raw = await AsyncStorage.getItem(BOOKING_DRAFT_KEY);
    if (!raw) return null;
    const draft = JSON.parse(raw) as BookingDraft;
    await AsyncStorage.removeItem(BOOKING_DRAFT_KEY);
    return draft;
  } catch {
    try {
      await AsyncStorage.removeItem(BOOKING_DRAFT_KEY);
    } catch {
      // ignore
    }
    return null;
  }
}

export async function clearBookingDrafts(): Promise<void> {
  try {
    const keys = await AsyncStorage.getAllKeys();
    const doomed = keys.filter((k) => k.startsWith(DRAFT_PREFIX));
    if (doomed.length > 0) await AsyncStorage.multiRemove(doomed);
  } catch {
    // ignore
  }
}
