export interface DayHours {
  day: string;
  short: string;
  open: string;
  close: string;
  closed: boolean;
}

export interface ShopInfo {
  name: string;
  tagline: string;
  description: string;
  address: string;
  phone: string;
  hours: DayHours[];
}

export interface Barber {
  id: string;
  name: string;
  role: string;
  nextAvailable: string;
}

export interface Service {
  id: string;
  name: string;
  description: string;
  durationMin: number;
  price: string;
}

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

export const shop: ShopInfo = {
  name: 'FRANKFURT BARBER CO.',
  tagline: 'OD 2016. - FRANKFURT NA MAJNI',
  description: 'OŠTRE FRIZURE. BEZ GALAME.',
  address: 'KAISERSTRASSE 12, 60311 FRANKFURT',
  phone: '+49 69 00 00 00',
  hours: [
    { day: 'PONEDELJAK', short: 'PON', open: '09:00', close: '20:00', closed: false },
    { day: 'UTORAK', short: 'UTO', open: '09:00', close: '20:00', closed: false },
    { day: 'SREDA', short: 'SRE', open: '09:00', close: '20:00', closed: false },
    { day: 'ČETVRTAK', short: 'ČET', open: '09:00', close: '20:00', closed: false },
    { day: 'PETAK', short: 'PET', open: '09:00', close: '20:00', closed: false },
    { day: 'SUBOTA', short: 'SUB', open: '09:00', close: '18:00', closed: false },
    { day: 'NEDELJA', short: 'NED', open: '', close: '', closed: true },
  ],
};

export const barbers: Barber[] = [
  { id: 'b-adem', name: 'ADEM', role: 'MASTER BERBERIN', nextAvailable: 'DANAS 14:30' },
  { id: 'b-denis', name: 'DENIS', role: 'FEJD SPECIJALISTA', nextAvailable: 'DANAS 16:00' },
  { id: 'b-kay', name: 'KAY', role: 'BRADA + BRIJANJE', nextAvailable: 'SUTRA 10:00' },
  { id: 'b-omar', name: 'OMAR', role: 'KLASIČNE FRIZURE', nextAvailable: 'SUTRA 11:30' },
];

export const services: Service[] = [
  { id: 's-buzz', name: 'MAŠINICA', description: 'JEDNA DUŽINA, ČIST VRAT', durationMin: 20, price: '2.000 RSD' },
  { id: 's-classic', name: 'KLASIČNO ŠIŠANJE', description: 'MAKAZE + MAŠINICA', durationMin: 30, price: '2.500 RSD' },
  { id: 's-fade', name: 'SKIN FEJD', description: 'NULA PA PRELAZ, BRIJAČ', durationMin: 40, price: '2.800 RSD' },
  { id: 's-beard', name: 'OBLIKOVANJE BRADE', description: 'OBLIK + TOPLI PEŠKIR', durationMin: 20, price: '1.800 RSD' },
  { id: 's-cut-beard', name: 'ŠIŠANJE + BRADA', description: 'KOMPLETNA USLUGA, TOPLI PEŠKIR', durationMin: 55, price: '4.000 RSD' },
  { id: 's-shave', name: 'BRIJANJE TOPLIM PEŠKIROM', description: 'BRITVA, HLADAN FINIŠ', durationMin: 30, price: '2.200 RSD' },
];

const SLOT_TIMES = [
  '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
  '12:00', '12:30', '13:00', '13:30', '14:00', '14:30',
  '15:00', '15:30', '16:00', '16:30', '17:00', '17:30',
  '18:00', '18:30', '19:00', '19:30',
];

const WEEKDAYS = ['NED', 'PON', 'UTO', 'SRE', 'ČET', 'PET', 'SUB'];
const MONTHS = ['JAN', 'FEB', 'MAR', 'APR', 'MAJ', 'JUN', 'JUL', 'AVG', 'SEP', 'OKT', 'NOV', 'DEC'];

export function getNextDays(count = 14): DayOption[] {
  const days: DayOption[] = [];
  const now = new Date();
  for (let i = 0; i < count; i += 1) {
    const d = new Date(now);
    d.setDate(now.getDate() + i);
    const closed = d.getDay() === 0;
    days.push({
      iso: d.toISOString().slice(0, 10),
      weekday: WEEKDAYS[d.getDay()] ?? '',
      dayNum: String(d.getDate()).padStart(2, '0'),
      month: MONTHS[d.getMonth()] ?? '',
      closed,
    });
  }
  return days;
}

function hashCode(input: string): number {
  let h = 0;
  for (let i = 0; i < input.length; i += 1) {
    h = (h * 31 + input.charCodeAt(i)) | 0;
  }
  return Math.abs(h);
}

export function getSlots(dateIso: string, barberId: string | null): TimeSlot[] {
  return SLOT_TIMES.map((time) => {
    const h = hashCode(`${dateIso}|${barberId ?? 'any'}|${time}`);
    return { id: `${dateIso}-${time}`, time, available: h % 10 < 7 };
  });
}
