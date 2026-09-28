export const STATUS_SR: Record<string, string> = {
  Pending: 'NA ČEKANJU',
  Confirmed: 'POTVRĐENA',
  Cancelled: 'OTKAZANA',
  Completed: 'ZAVRŠENA',
  NoShow: 'NEDOLAZAK',
};

export function statusLabel(status: string): string {
  return STATUS_SR[status] ?? status.toUpperCase();
}

export function formatWhen(iso: string): string {
  const d = new Date(iso);
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const hh = String(d.getHours()).padStart(2, '0');
  const mm = String(d.getMinutes()).padStart(2, '0');
  return `${day}.${month}.${d.getFullYear()} ${hh}:${mm}`;
}
