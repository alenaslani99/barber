import { computed, ref, type Ref } from 'vue';
import { fetchAvailability, type Availability } from '../lib/catalog';
import { todayIsoDay } from '../lib/dates';
import type { TimeSlot } from '../data/mock';

export function useAvailability(
  barberId: Ref<string | null>,
  serviceId: Ref<string | null>,
  dateIso: Ref<string | null>,
) {
  const avail = ref<Availability | null>(null);
  const loading = ref(false);
  const error = ref('');

  async function load(): Promise<void> {
    error.value = '';
    if (!barberId.value || !serviceId.value || !dateIso.value) {
      avail.value = null;
      return;
    }
    loading.value = true;
    try {
      avail.value = await fetchAvailability(
        barberId.value,
        dateIso.value,
        serviceId.value,
      );
    } catch {
      avail.value = null;
      error.value = 'GREŠKA U VEZI, POKUŠAJ PONOVO';
    } finally {
      loading.value = false;
    }
  }

  const slots = computed<TimeSlot[]>(() => {
    const current = avail.value;
    const iso = dateIso.value;
    if (!current || current.closed || !iso) return [];
    const taken = new Set(current.taken);
    const [oh, om] = current.open.split(':').map(Number);
    const [ch, cm] = current.close.split(':').map(Number);
    const step = current.slotMinutes > 0 ? current.slotMinutes : 30;
    const now = new Date();
    const isToday = iso === todayIsoDay(now);
    const [y, mo, d] = iso.split('-').map(Number);
    const result: TimeSlot[] = [];
    for (let m = oh * 60 + om; m + step <= ch * 60 + cm; m += step) {
      const hh = String(Math.floor(m / 60)).padStart(2, '0');
      const mm = String(m % 60).padStart(2, '0');
      const time = `${hh}:${mm}`;
      let available = !taken.has(time);
      if (available && isToday) {
        available =
          new Date(y, mo - 1, d, Number(hh), Number(mm)).getTime() > now.getTime();
      }
      result.push({ id: `${iso}-${time}`, time, available });
    }
    return result;
  });

  return { avail, slots, availLoading: loading, availError: error, loadAvailability: load };
}
