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

export function fetchBarbers(): Promise<ApiBarber[]> {
  return api<ApiBarber[]>('/api/staff', { auth: false });
}

export function fetchServices(): Promise<ApiService[]> {
  return api<ApiService[]>('/api/service', { auth: false });
}
