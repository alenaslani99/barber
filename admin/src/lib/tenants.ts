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

export interface ShopDetails {
  id: string;
  name: string;
  address: string | null;
  phone: string | null;
  tagline: string;
  description: string;
  timeZone: string;
}

export type ShopInput = Omit<ShopDetails, 'id'>;

export interface Owner {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

export type OwnerInput = Omit<Owner, 'id'>;

/** Returned once at account creation. Keep it in component state only, never persist it. */
export interface Credentials {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

function tenantPath(slug: string, rest: string): string {
  return `/api/admin/tenants/${encodeURIComponent(slug)}/${rest}`;
}

export function getShop(slug: string): Promise<ShopDetails> {
  return api<ShopDetails>(tenantPath(slug, 'shop'));
}

export function saveShop(slug: string, input: ShopInput): Promise<ShopDetails> {
  return api<ShopDetails>(tenantPath(slug, 'shop'), { method: 'PUT', body: input });
}

export function getOwner(slug: string): Promise<Owner> {
  return api<Owner>(tenantPath(slug, 'owner'));
}

export function createOwner(slug: string, input: OwnerInput): Promise<Credentials> {
  return api<Credentials>(tenantPath(slug, 'owner'), { method: 'POST', body: input });
}
