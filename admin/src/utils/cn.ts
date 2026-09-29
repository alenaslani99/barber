type ClassValue = string | false | null | undefined;

/**
 * Joins class names, dropping falsy values. No clsx/tailwind-merge dependency
 * so the admin app stays dependency-light.
 */
export function cn(...values: ClassValue[]): string {
  return values.filter(Boolean).join(' ');
}
