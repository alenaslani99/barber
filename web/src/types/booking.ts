export interface Service {
  id: string;
  name: string;
  durationMinutes: number;
  priceCents: number;
}

export interface Barber {
  id: string;
  firstName: string;
  lastName: string;
}

export type BookingStepKey = 'service' | 'barber' | 'slots' | 'details' | 'ticket';

export interface BookingStep {
  key: BookingStepKey;
  label: string;
}

export interface TimeSlot {
  start: string;
}

export interface BookingDraft {
  serviceId: string | null;
  barberId: string | null;
  slotStart: string | null;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

export function emptyDraft(): BookingDraft {
  return {
    serviceId: null,
    barberId: null,
    slotStart: null,
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
  };
}
