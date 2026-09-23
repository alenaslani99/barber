export function safeNextPath(value: unknown): string {
  if (typeof value !== 'string' || value === '') return '/';
  if (value.startsWith('/') && !value.startsWith('//')) return value;
  return '/';
}
