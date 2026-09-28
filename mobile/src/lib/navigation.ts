export function safeNextPath(value: unknown): string {
  if (typeof value !== 'string' || value === '') return '/booking';
  if (value.startsWith('/') && !value.startsWith('//')) return value;
  return '/booking';
}
