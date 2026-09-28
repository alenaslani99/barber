import { useCallback, useEffect, useRef, useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  RefreshControl,
  ScrollView,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Redirect, router } from 'expo-router';
import { Button } from '@/components/ui/Button';
import { UiInput, type UiInputHandle } from '@/components/ui/UiInput';
import { api, ApiRequestError } from '@/src/lib/api';
import { formatWhen, statusLabel } from '@/src/lib/bookings';
import {
  CACHE_KEYS,
  CACHE_TTL_MIN,
  cacheGet,
  cacheInvalidate,
  cacheSet,
} from '@/src/lib/cache';
import { useAuthStore } from '@/src/stores/auth';

interface Profile {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

interface MyBooking {
  id: string;
  serviceName: string;
  barberName: string;
  startsAt: string;
  endsAt: string;
  status: string;
}

interface BookingsPage {
  items: MyBooking[];
  total: number;
}

type Tab = 'general' | 'history' | 'password';

const PAGE_SIZE = 5;

export default function Account() {
  const hydrated = useAuthStore((s) => s.hydrated);
  const token = useAuthStore((s) => s.accessToken);
  const logout = useAuthStore((s) => s.logout);

  const [tab, setTab] = useState<Tab>('general');
  const [profile, setProfile] = useState<Profile | null>(null);
  const [bookings, setBookings] = useState<MyBooking[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [loadError, setLoadError] = useState('');
  const [cancelError, setCancelError] = useState('');

  const [currentPw, setCurrentPw] = useState('');
  const [newPw, setNewPw] = useState('');
  const [confirmPw, setConfirmPw] = useState('');
  const [pwLoading, setPwLoading] = useState(false);
  const [pwError, setPwError] = useState('');

  const currentPwInput = useRef<UiInputHandle>(null);
  const newPwInput = useRef<UiInputHandle>(null);
  const confirmPwInput = useRef<UiInputHandle>(null);

  const loadAll = useCallback(
    async (force: boolean): Promise<void> => {
      if (!token) return;
      if (force) {
        setRefreshing(true);
      } else {
        // Instant render from cache, then silent revalidate below.
        const [cachedProfile, cachedHistory] = await Promise.all([
          cacheGet<Profile>(CACHE_KEYS.profile, CACHE_TTL_MIN.profile),
          cacheGet<BookingsPage>(CACHE_KEYS.history, CACHE_TTL_MIN.history),
        ]);
        if (cachedProfile) setProfile(cachedProfile);
        if (cachedHistory) {
          setBookings(cachedHistory.items);
          setTotal(cachedHistory.total);
        }
        if (cachedProfile && cachedHistory) setLoading(false);
      }
      setLoadError('');
      try {
        const [p, page] = await Promise.all([
          api<Profile>('/api/auth/me', { token }),
          api<BookingsPage>(`/api/booking/mine?skip=0&take=${PAGE_SIZE}`, { token }),
        ]);
        setProfile(p);
        setBookings(page.items);
        setTotal(page.total);
        await Promise.all([
          cacheSet(CACHE_KEYS.profile, p),
          cacheSet(CACHE_KEYS.history, page),
        ]);
      } catch {
        setLoadError('GREŠKA U VEZI, POKUŠAJ PONOVO');
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [token],
  );

  useEffect(() => {
    if (!hydrated || !token) return;
    void loadAll(false);
  }, [hydrated, token, loadAll]);

  if (!hydrated) return null;
  if (!token) return <Redirect href="/login?next=/account" />;

  const canShowMore = bookings.length < total;

  async function loadHistory(): Promise<void> {
    if (!token) return;
    setLoadingMore(true);
    try {
      const page = await api<BookingsPage>(
        `/api/booking/mine?skip=${bookings.length}&take=${PAGE_SIZE}`,
        { token },
      );
      const merged = [...bookings, ...page.items];
      setBookings(merged);
      setTotal(page.total);
      await cacheSet(CACHE_KEYS.history, { items: merged, total: page.total });
    } catch {
      setLoadError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    } finally {
      setLoadingMore(false);
    }
  }

  async function changePassword(): Promise<void> {
    setPwError('');
    const currentOk = currentPwInput.current?.validate() ?? false;
    const newOk = newPwInput.current?.validate() ?? false;
    const confirmOk = confirmPwInput.current?.validate() ?? false;
    if (!currentOk || !newOk || !confirmOk) return;
    setPwLoading(true);
    try {
      await api<void>('/api/auth/change-password', {
        method: 'POST',
        token,
        body: { currentPassword: currentPw, newPassword: newPw },
      });
      await logout();
      router.replace('/login');
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        currentPwInput.current?.setError('TRENUTNA LOZINKA NIJE ISPRAVNA');
        return;
      }
      if (error instanceof ApiRequestError && error.status === 400) {
        newPwInput.current?.setError('MIN 8 KARAKTERA');
        return;
      }
      setPwError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    } finally {
      setPwLoading(false);
    }
  }

  async function signOut(): Promise<void> {
    await logout();
    router.replace('/login');
  }

  async function cancelBooking(b: MyBooking): Promise<void> {
    setCancelError('');
    try {
      const updated = await api<MyBooking>(`/api/booking/${b.id}/status`, {
        method: 'PATCH',
        token,
        body: { status: 'Cancelled' },
      });
      setBookings((prev) => prev.map((x) => (x.id === b.id ? updated : x)));
      // History changed server-side — drop the cache so the next open refetches.
      await cacheInvalidate(CACHE_KEYS.history);
    } catch {
      setCancelError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    }
  }

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-ink">
      <ScrollView
        className="flex-1"
        contentContainerStyle={{ flexGrow: 1 }}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => void loadAll(true)}
            tintColor="#F5F5F0"
          />
        }
      >
        <View className="flex-1 px-4 py-10">
          <Text className="text-center font-display text-6xl uppercase leading-none tracking-wide text-bone">
            NALOG
          </Text>

          <View
            accessibilityLabel="Sekcije naloga"
            className="mt-8 flex-row border border-line"
          >
            <TabButton label="OPŠTE" active={tab === 'general'} onPress={() => setTab('general')} first />
            <TabButton label="ISTORIJA" active={tab === 'history'} onPress={() => setTab('history')} />
            <TabButton label="LOZINKA" active={tab === 'password'} onPress={() => setTab('password')} last />
          </View>

          {loading ? (
            <Text className="mt-8 font-display text-xl uppercase tracking-widest text-ash">
              UČITAVANJE...
            </Text>
          ) : loadError && !profile ? (
            <Text className="mt-8 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
              {loadError}
            </Text>
          ) : profile && tab === 'general' ? (
            <View>
              <View accessibilityLabel="Podaci o nalogu" className="mt-8 border-y border-line">
                <ProfileRow label="IME" value={profile.firstName} />
                <ProfileRow label="PREZIME" value={profile.lastName} bordered />
                <ProfileRow label="EMAIL" value={profile.email} bordered />
                <ProfileRow label="TELEFON" value={profile.phone} bordered />
              </View>
              <View className="mt-10">
                <Button title="ODJAVI SE" variant="danger" onPress={() => void signOut()} />
              </View>
            </View>
          ) : profile && tab === 'history' ? (
            <View>
              <Text className="mt-8 font-display text-3xl uppercase tracking-widest text-bone">
                MOJE REZERVACIJE{' '}
                <Text className="text-blue">({total})</Text>
              </Text>
              {cancelError ? (
                <Text className="mt-4 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                  {cancelError}
                </Text>
              ) : null}
              {bookings.length === 0 ? (
                <Text className="mt-4 font-display text-xl uppercase tracking-widest text-ash">
                  NEMAŠ REZERVACIJA
                </Text>
              ) : (
                <View className="mt-4 gap-3">
                  {bookings.map((b) => (
                    <View key={b.id} accessibilityLabel="Rezervacija" className="border border-line bg-surface p-4">
                      <View className="flex-row items-center justify-between gap-3">
                        <Text className="font-display text-2xl uppercase leading-none tracking-widest text-bone">
                          {b.serviceName}
                        </Text>
                        <Text className="shrink-0 font-display text-lg uppercase tracking-widest text-blue">
                          {statusLabel(b.status)}
                        </Text>
                      </View>
                      <Text className="mt-1 font-display text-lg uppercase tracking-widest text-ash">
                        {b.barberName} / {formatWhen(b.startsAt)}
                      </Text>
                      {b.status === 'Pending' || b.status === 'Confirmed' ? (
                        <View className="mt-3">
                          <Button title="OTKAŽI" variant="outline" onPress={() => void cancelBooking(b)} />
                        </View>
                      ) : null}
                    </View>
                  ))}
                </View>
              )}
              {canShowMore ? (
                <View className="mt-4">
                  <Button
                    title={loadingMore ? 'UČITAVANJE...' : 'PRIKAŽI JOŠ'}
                    variant="outline"
                    disabled={loadingMore}
                    onPress={() => void loadHistory()}
                  />
                </View>
              ) : null}
            </View>
          ) : profile ? (
            <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
              <View className="mt-8">
                <UiInput
                  ref={currentPwInput}
                  label="TRENUTNA LOZINKA"
                  value={currentPw}
                  onChangeText={setCurrentPw}
                  secure
                  placeholder="TRENUTNA LOZINKA"
                  rules={[(v) => (!v ? 'LOZINKA JE OBAVEZNA' : '')]}
                />
                <View className="mt-2">
                  <UiInput
                    ref={newPwInput}
                    label="NOVA LOZINKA"
                    value={newPw}
                    onChangeText={setNewPw}
                    secure
                    placeholder="MIN 8 KARAKTERA"
                    rules={[
                      (v) => (!v ? 'LOZINKA JE OBAVEZNA' : ''),
                      (v) => (v.length < 8 ? 'MIN 8 KARAKTERA' : ''),
                    ]}
                  />
                </View>
                <View className="mt-2">
                  <UiInput
                    ref={confirmPwInput}
                    label="POTVRDI NOVU LOZINKU"
                    value={confirmPw}
                    onChangeText={setConfirmPw}
                    secure
                    placeholder="PONOVI NOVU LOZINKU"
                    rules={[
                      (v) => (!v ? 'POTVRDI LOZINKU' : ''),
                      (v) => (v !== newPw ? 'LOZINKE SE NE POKLAPAJU' : ''),
                    ]}
                  />
                </View>
                {pwError ? (
                  <Text className="mt-4 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                    {pwError}
                  </Text>
                ) : null}
                <View className="mt-6">
                  <Button
                    title={pwLoading ? 'UČITAVANJE...' : 'SAČUVAJ'}
                    variant="blaze"
                    disabled={pwLoading}
                    onPress={() => void changePassword()}
                  />
                </View>
              </View>
            </KeyboardAvoidingView>
          ) : null}
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function TabButton({
  label,
  active,
  onPress,
  first = false,
  last = false,
}: {
  label: string;
  active: boolean;
  onPress: () => void;
  first?: boolean;
  last?: boolean;
}) {
  return (
    <Pressable
      accessibilityRole="tab"
      accessibilityState={{ selected: active }}
      onPress={onPress}
      className={
        active
          ? 'flex-1 bg-bone py-2'
          : first
            ? 'flex-1 border-r border-line py-2'
            : last
              ? 'flex-1 border-l border-line py-2'
              : 'flex-1 border-x border-line py-2'
      }
      style={({ pressed }) => ({ opacity: pressed ? 0.7 : 1 })}
    >
      <Text
        className={
          active
            ? 'text-center font-display text-xl uppercase tracking-widest text-ink'
            : 'text-center font-display text-xl uppercase tracking-widest text-ash'
        }
      >
        {label}
      </Text>
    </Pressable>
  );
}

function ProfileRow({
  label,
  value,
  bordered = false,
}: {
  label: string;
  value: string;
  bordered?: boolean;
}) {
  return (
    <View
      className={
        bordered
          ? 'flex-row items-center justify-between border-t border-line py-2'
          : 'flex-row items-center justify-between py-2'
      }
    >
      <Text className="font-display text-lg uppercase tracking-widest text-ash">{label}</Text>
      <Text className="font-display text-xl uppercase tracking-widest text-bone">{value}</Text>
    </View>
  );
}
