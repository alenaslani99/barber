export interface DayOption {
  iso: string;
  weekday: string;
  dayNum: string;
  month: string;
  closed: boolean;
}

export interface TimeSlot {
  id: string;
  time: string;
  available: boolean;
}

const WEEKDAYS = ['NED', 'PON', 'UTO', 'SRE', 'ČET', 'PET', 'SUB'];
const MONTHS = [
  'JAN',
  'FEB',
  'MAR',
  'APR',
  'MAJ',
  'JUN',
  'JUL',
  'AVG',
  'SEP',
  'OKT',
  'NOV',
  'DEC',
];

export function getNextDays(count = 14, closedDays: number[] = [0]): DayOption[] {
  const days: DayOption[] = [];
  const now = new Date();
  for (let i = 0; i < count; i += 1) {
    const d = new Date(now);
    d.setDate(now.getDate() + i);
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const closed = closedDays.includes(d.getDay());
    days.push({
      iso: `${y}-${m}-${day}`,
      weekday: WEEKDAYS[d.getDay()] ?? '',
      dayNum: String(d.getDate()).padStart(2, '0'),
      month: MONTHS[d.getMonth()] ?? '',
      closed,
    });
  }
  return days;
}
