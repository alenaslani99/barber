import { api } from './api';

export interface ApiBarber {
  id: string;
  firstName: string;
  lastName: string;
  seniority: string;
  isActive: boolean;
}

export interface ApiService {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
}

export interface ApiShopHour {
  day: number;
  open: string;
  close: string;
  closed: boolean;
}

export interface ApiShop {
  id: string;
  name: string;
  tagline: string;
  description: string;
  address: string;
  phone: string;
  hours: ApiShopHour[];
  staff: ApiBarber[];
  services: ApiService[];
}

export interface Availability {
  date: string;
  closed: boolean;
  open: string;
  close: string;
  slotMinutes: number;
  taken: string[];
}

export function fetchBarbers(
  includeInactive = false,
  token: string | null = null,
): Promise<ApiBarber[]> {
  const path = includeInactive ? '/api/staff?includeInactive=true' : '/api/staff';
  return api<ApiBarber[]>(path, { token });
}

export function fetchServices(): Promise<ApiService[]> {
  return api<ApiService[]>('/api/service', { auth: false });
}

export function fetchShop(): Promise<ApiShop> {
  return api<ApiShop>('/api/barbershop', { auth: false });
}

export function fetchAvailability(
  staffId: string,
  dateIso: string,
  serviceId: string,
): Promise<Availability> {
  const params = new URLSearchParams({ staffId, date: dateIso, serviceId });
  return api<Availability>(`/api/booking/availability?${params.toString()}`, { auth: false });
}
