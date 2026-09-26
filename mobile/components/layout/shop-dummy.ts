import type { ApiShopHour } from '@/src/lib/catalog';

// Dummy data until screens wire the real /api/barbershop response.
export const DUMMY_SHOP = {
  name: 'GANG BARBER',
  tagline: 'FRANKFURT NA MAJNI',
  description: 'KLASIČNO BRIJANJE I MODERNE FRIZURE',
  address: 'Berger Str. 123, Frankfurt',
  phone: '+49 69 000 00 00',
  hours: [
    { day: 1, open: '09:00', close: '19:00', closed: false },
    { day: 2, open: '09:00', close: '19:00', closed: false },
    { day: 3, open: '09:00', close: '19:00', closed: false },
    { day: 4, open: '09:00', close: '19:00', closed: false },
    { day: 5, open: '09:00', close: '19:00', closed: false },
    { day: 6, open: '10:00', close: '16:00', closed: false },
    { day: 0, open: '09:00', close: '19:00', closed: true },
  ] as ApiShopHour[],
};
