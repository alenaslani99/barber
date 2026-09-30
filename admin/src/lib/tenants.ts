import { api } from './api';

export interface Tenant {
  id: string;
  slug: string;
  databaseName: string;
  isActive: boolean;
  createdAt: string;
}

export interface TenantSetup {
  tenant: Tenant;
  hasShop: boolean;
  hasOwner: boolean;
  staffCount: number;
  serviceCount: number;
}

export function listTenants(): Promise<Tenant[]> {
  return api<Tenant[]>('/api/admin/tenants');
}

export function getTenantSetup(slug: string): Promise<TenantSetup> {
  return api<TenantSetup>(`/api/admin/tenants/${encodeURIComponent(slug)}`);
}

export function createTenant(slug: string, databaseName: string): Promise<Tenant> {
  return api<Tenant>('/api/admin/tenants', { method: 'POST', body: { slug, databaseName } });
}
