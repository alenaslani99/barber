import type { Component } from 'vue';

export interface NavItem {
  label: string;
  icon: Component;
  to: string;
  /** Route name that must be active for the item to show as active. */
  match?: string;
  /** Locked until a tenant exists in the current session. */
  requiresTenant?: boolean;
}

export interface NavSection {
  label: string;
  items: NavItem[];
}
