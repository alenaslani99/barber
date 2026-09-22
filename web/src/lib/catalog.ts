import { api } from './api';

export interface ApiBarber {
  id: string;
  firstName: string;
  lastName: string;
  seniority: string;
}

export interface ApiService {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
}

export interface Availability {
  date: string;
  closed: boolean;
  open: string;
  close: string;
  slotMinutes: number;
  taken: string[];
}

export function fetchBarbers(): Promise<ApiBarber[]> {
  return api<ApiBarber[]>('/api/staff', { auth: false });
}

export function fetchServices(): Promise<ApiService[]> {
  return api<ApiService[]>('/api/service', { auth: false });
}

export function fetchAvailability(
  staffId: string,
  dateIso: string,
  serviceId: string,
): Promise<Availability> {
  const params = new URLSearchParams({ staffId, date: dateIso, serviceId });
  return api<Availability>(`/api/booking/availability?${params.toString()}`, { auth: false });
}
