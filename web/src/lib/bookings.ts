export const STATUS_SR: Record<string, string> = {
  Pending: 'NA ČEKANJU',
  Confirmed: 'POTVRĐENA',
  Cancelled: 'OTKAZANA',
  Completed: 'ZAVRŠENA',
  NoShow: 'NEDOLAZAK',
};

export const STATUS_KEYS = ['Pending', 'Confirmed', 'Cancelled', 'Completed', 'NoShow'];

export interface AdminBooking {
  id: string;
  serviceName: string;
  barberName: string;
  clientName: string;
  startsAt: string;
  endsAt: string;
  status: string;
}

export interface AdminBookingsPage {
  items: AdminBooking[];
  total: number;
}

export function statusLabel(status: string): string {
  return STATUS_SR[status] ?? status.toUpperCase();
}

export function formatWhen(iso: string): string {
  return new Date(iso).toLocaleString('sr-Latn', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
