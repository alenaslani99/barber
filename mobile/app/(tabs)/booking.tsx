import { useCallback, useEffect, useMemo, useState } from 'react';
import { Pressable, ScrollView, Text, TextInput, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { Button } from '@/components/ui/Button';
import { SectionTitle } from '@/components/ui/SectionTitle';
import { ShopHeader } from '@/components/layout/ShopHeader';
import { BarberOption } from '@/components/features/booking/BarberOption';
import { BookingStepper } from '@/components/features/booking/BookingStepper';
import { BookingSummary } from '@/components/features/booking/BookingSummary';
import { DateStrip } from '@/components/features/booking/DateStrip';
import { ServiceOption } from '@/components/features/booking/ServiceOption';
import { TimeGrid } from '@/components/features/booking/TimeGrid';
import { api, ApiRequestError } from '@/src/lib/api';
import { fetchAvailability, type ApiBarber, type ApiService, type Availability } from '@/src/lib/catalog';
import { todayIsoDay } from '@/src/lib/dates';
import { getNextDays, type TimeSlot } from '@/src/lib/days';
import { CACHE_KEYS, cacheInvalidate } from '@/src/lib/cache';
import { useAuthStore } from '@/src/stores/auth';
import { useShopStore } from '@/src/stores/shop';

type Step = 1 | 2 | 3 | 4;

function buildSlots(avail: Availability | null, iso: string | null): TimeSlot[] {
  if (!avail || avail.closed || !iso) return [];
  const taken = new Set(avail.taken);
  const [oh, om] = avail.open.split(':').map(Number) as [number, number];
  const [ch, cm] = avail.close.split(':').map(Number) as [number, number];
  const step = avail.slotMinutes > 0 ? avail.slotMinutes : 30;
  const now = new Date();
  const isToday = iso === todayIsoDay(now);
  const [y, mo, d] = iso.split('-').map(Number) as [number, number, number];
  const result: TimeSlot[] = [];
  for (let m = (oh ?? 0) * 60 + (om ?? 0); m + step <= (ch ?? 0) * 60 + (cm ?? 0); m += step) {
    const hh = String(Math.floor(m / 60)).padStart(2, '0');
    const mm = String(m % 60).padStart(2, '0');
    const time = `${hh}:${mm}`;
    let available = !taken.has(time);
    if (available && isToday) {
      available = new Date(y, (mo ?? 1) - 1, d, Number(hh), Number(mm)).getTime() > now.getTime();
    }
    result.push({ id: `${iso}-${time}`, time, available });
  }
  return result;
}

export default function Booking() {
  const token = useAuthStore((s) => s.accessToken);
  const shop = useShopStore((s) => s.shop);
  const loadShop = useShopStore((s) => s.load);

  const [step, setStep] = useState<Step>(1);
  const [barberId, setBarberId] = useState<string | null>(null);
  const [serviceId, setServiceId] = useState<string | null>(null);
  const [dateIso, setDateIso] = useState<string | null>(null);
  const [time, setTime] = useState<string | null>(null);
  const [notes, setNotes] = useState('');
  const [booked, setBooked] = useState(false);
  const [bookingRef, setBookingRef] = useState('');
  const [reserveLoading, setReserveLoading] = useState(false);
  const [reserveError, setReserveError] = useState('');
  const [barbers, setBarbers] = useState<ApiBarber[]>([]);
  const [services, setServices] = useState<ApiService[]>([]);
  const [catalogLoading, setCatalogLoading] = useState(true);
  const [catalogError, setCatalogError] = useState('');
  const [avail, setAvail] = useState<Availability | null>(null);
  const [availLoading, setAvailLoading] = useState(false);
  const [availError, setAvailError] = useState('');

  const loadCatalog = useCallback(async () => {
    setCatalogLoading(true);
    setCatalogError('');
    const data = await loadShop();
    if (!data) {
      setBarbers([]);
      setServices([]);
      setCatalogError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    } else {
      setBarbers(data.staff);
      setServices(data.services);
    }
    setCatalogLoading(false);
  }, [loadShop]);

  useEffect(() => {
    void loadCatalog();
  }, [loadCatalog]);

  const loadAvailability = useCallback(async () => {
    setAvailError('');
    if (!barberId || !serviceId || !dateIso) {
      setAvail(null);
      return;
    }
    setAvailLoading(true);
    try {
      setAvail(await fetchAvailability(barberId, dateIso, serviceId));
    } catch {
      setAvail(null);
      setAvailError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    } finally {
      setAvailLoading(false);
    }
  }, [barberId, serviceId, dateIso]);

  useEffect(() => {
    void loadAvailability();
  }, [loadAvailability]);

  const closedDays = useMemo(
    () => (shop ? shop.hours.filter((h) => h.closed).map((h) => h.day) : [0]),
    [shop],
  );
  const days = useMemo(() => getNextDays(14, closedDays), [closedDays]);
  const slots = useMemo(() => buildSlots(avail, dateIso), [avail, dateIso]);

  const selectedBarber = barbers.find((b) => b.id === barberId) ?? null;
  const selectedService = services.find((s) => s.id === serviceId) ?? null;
  const dateLabel = days.find((d) => d.iso === dateIso);
  const dateLabelText = dateLabel ? `${dateLabel.weekday} ${dateLabel.dayNum} ${dateLabel.month}` : '';

  const canContinue =
    step === 1 ? barberId !== null : step === 2 ? serviceId !== null : step === 3 ? dateIso !== null && time !== null : true;

  function selectBarber(id: string): void {
    setBarberId(id);
  }

  function selectService(id: string): void {
    setServiceId(id);
  }

  function selectDate(iso: string): void {
    setDateIso(iso);
    setTime(null);
  }

  function next(): void {
    if (step < 4 && canContinue) setStep((step + 1) as Step);
  }

  function back(): void {
    if (step > 1) setStep((step - 1) as Step);
  }

  function goTo(s: number): void {
    if (s >= 1 && s <= 4 && s < step) setStep(s as Step);
  }

  async function reserve(): Promise<void> {
    setReserveError('');
    if (!token) {
      router.replace('/login');
      return;
    }
    if (!barberId || !serviceId || !dateIso || !time) return;
    const [y, m, d] = dateIso.split('-').map(Number) as [number, number, number];
    const [hh, mm] = time.split(':').map(Number) as [number, number];
    const startsAt = new Date(y, (m ?? 1) - 1, d, hh, mm).toISOString();
    setReserveLoading(true);
    try {
      const booking = await api<{ id: string; status: string; startsAt: string; endsAt: string }>(
        '/api/booking',
        {
          method: 'POST',
          token,
          body: { serviceId, staffId: barberId, startsAt, notes: notes || undefined },
        },
      );
      setBookingRef(booking.id.replace(/-/g, '').slice(0, 8).toUpperCase());
      setBooked(true);
      // History changed server-side — next NALOG open refetches.
      await cacheInvalidate(CACHE_KEYS.history);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 409) {
        setReserveError('TERMIN JE ZAUZET, IZABERI DRUGI');
      } else if (error instanceof ApiRequestError && error.status === 400) {
        setReserveError('NEISPRAVNA REZERVACIJA');
      } else {
        setReserveError('GREŠKA U VEZI, POKUŠAJ PONOVO');
      }
    } finally {
      setReserveLoading(false);
    }
  }

  function reset(): void {
    setStep(1);
    setBarberId(null);
    setServiceId(null);
    setDateIso(null);
    setTime(null);
    setNotes('');
    setBooked(false);
    setBookingRef('');
  }

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-ink">
      <ScrollView className="flex-1" contentContainerStyle={{ flexGrow: 1 }}>
        <ShopHeader name={shop?.name} />
        <View className="flex-1 px-4 pb-4">
          {booked ? (
            <View>
              <Text className="mt-8 font-display text-7xl uppercase leading-none tracking-wide text-bone">
                ZAKAZANO
              </Text>
              <Text className="mt-2 font-display text-xl uppercase tracking-widest text-ash">
                POKAŽI OVAJ KOD U SALONU
              </Text>
              <Text className="mt-4 border border-line bg-surface p-4 text-center font-display text-4xl uppercase tracking-widest text-bone">
                {bookingRef}
              </Text>
              {selectedBarber && selectedService && dateLabelText && time ? (
                <View className="mt-6">
                  <BookingSummary
                    barberName={`${selectedBarber.firstName} ${selectedBarber.lastName}`}
                    serviceName={selectedService.name}
                    durationMin={selectedService.durationMinutes}
                    price={selectedService.price}
                    dateLabel={dateLabelText}
                    time={time}
                  />
                </View>
              ) : null}
              <View className="mt-6">
                <Button title="NOVA REZERVACIJA" onPress={reset} />
              </View>
            </View>
          ) : (
            <View>
              <BookingStepper step={step} onGo={goTo} />

              {step === 1 ? (
                <View>
                  <SectionTitle>01 / IZABERI BERBERINA</SectionTitle>
                  {catalogLoading ? (
                    <Text className="mt-4 font-display text-xl uppercase tracking-widest text-ash">
                      UČITAVANJE...
                    </Text>
                  ) : catalogError ? (
                    <View className="mt-4">
                      <Text className="border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                        {catalogError}
                      </Text>
                      <View className="mt-3">
                        <Button title="POKUŠAJ PONOVO" variant="outline" onPress={() => void loadCatalog()} />
                      </View>
                    </View>
                  ) : (
                    <View className="mt-4 gap-3">
                      {barbers.map((b) => (
                        <BarberOption
                          key={b.id}
                          barber={b}
                          selected={barberId === b.id}
                          onSelect={selectBarber}
                        />
                      ))}
                    </View>
                  )}
                  <View className="mt-6">
                    <Button title="DALJE" disabled={!canContinue} onPress={next} />
                  </View>
                </View>
              ) : null}

              {step === 2 ? (
                <View>
                  <SectionTitle>02 / IZABERI USLUGU</SectionTitle>
                  {catalogLoading ? (
                    <Text className="mt-4 font-display text-xl uppercase tracking-widest text-ash">
                      UČITAVANJE...
                    </Text>
                  ) : catalogError ? (
                    <View className="mt-4">
                      <Text className="border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                        {catalogError}
                      </Text>
                      <View className="mt-3">
                        <Button title="POKUŠAJ PONOVO" variant="outline" onPress={() => void loadCatalog()} />
                      </View>
                    </View>
                  ) : (
                    <View className="mt-4 gap-3">
                      {services.map((s) => (
                        <ServiceOption
                          key={s.id}
                          service={s}
                          selected={serviceId === s.id}
                          onSelect={selectService}
                        />
                      ))}
                    </View>
                  )}
                  <View className="mt-6 flex-row gap-2">
                    <View className="flex-1">
                      <Button title="NAZAD" variant="outline" onPress={back} />
                    </View>
                    <View className="flex-1">
                      <Button title="DALJE" disabled={!canContinue} onPress={next} />
                    </View>
                  </View>
                </View>
              ) : null}

              {step === 3 ? (
                <View>
                  <SectionTitle>03 / IZABERI TERMIN</SectionTitle>
                  <View className="mt-4">
                    <DateStrip days={days} selectedIso={dateIso} onSelect={selectDate} />
                  </View>
                  {!dateIso ? (
                    <Text className="mt-4 font-display text-xl uppercase tracking-widest text-ash">
                      PRVO IZABERI DAN
                    </Text>
                  ) : (
                    <View className="mt-4">
                      {availLoading ? (
                        <Text className="font-display text-xl uppercase tracking-widest text-ash">
                          UČITAVANJE...
                        </Text>
                      ) : availError ? (
                        <View>
                          <Text className="border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                            {availError}
                          </Text>
                          <View className="mt-3">
                            <Button title="POKUŠAJ PONOVO" variant="outline" onPress={() => void loadAvailability()} />
                          </View>
                        </View>
                      ) : avail?.closed ? (
                        <Text className="font-display text-xl uppercase tracking-widest text-ash">
                          ZATVORENO
                        </Text>
                      ) : (
                        <View>
                          <TimeGrid slots={slots} selectedTime={time} onSelect={setTime} />
                          {!time ? (
                            <Text className="mt-4 font-display text-xl uppercase tracking-widest text-ash">
                              IZABERI TERMIN
                            </Text>
                          ) : null}
                        </View>
                      )}
                    </View>
                  )}
                  <View className="mt-6 flex-row gap-2">
                    <View className="flex-1">
                      <Button title="NAZAD" variant="outline" onPress={back} />
                    </View>
                    <View className="flex-1">
                      <Button title="DALJE" disabled={!canContinue} onPress={next} />
                    </View>
                  </View>
                </View>
              ) : null}

              {step === 4 ? (
                <View>
                  <SectionTitle>04 / REZERVACIJA</SectionTitle>
                  {selectedBarber && selectedService && dateLabelText && time ? (
                    <View className="mt-4">
                      <BookingSummary
                        barberName={`${selectedBarber.firstName} ${selectedBarber.lastName}`}
                        serviceName={selectedService.name}
                        durationMin={selectedService.durationMinutes}
                        price={selectedService.price}
                        dateLabel={dateLabelText}
                        time={time}
                      />
                    </View>
                  ) : null}
                  <Text className="mt-6 font-display text-lg uppercase tracking-widest text-ash">
                    NAPOMENA (OPCIONO)
                  </Text>
                  <TextInput
                    value={notes}
                    onChangeText={setNotes}
                    multiline
                    numberOfLines={3}
                    maxLength={500}
                    placeholder="NEŠTO ŠTO BERBERIN TREBA DA ZNA"
                    placeholderTextColor="#A1A1A1"
                    className="mt-2 w-full border border-line bg-ink px-3 py-4 font-form text-xl text-bone"
                  />
                  {reserveError ? (
                    <Text className="mt-4 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                      {reserveError}
                    </Text>
                  ) : null}
                  <View className="mt-4">
                    <Button
                      title={reserveLoading ? 'UČITAVANJE...' : 'REZERVIŠI'}
                      variant="blaze"
                      disabled={reserveLoading}
                      onPress={() => void reserve()}
                    />
                  </View>
                  <View className="mt-3">
                    <Button title="NAZAD" variant="outline" onPress={back} />
                  </View>
                </View>
              ) : null}
            </View>
          )}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
