import { useRef, useState } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Link, router, useLocalSearchParams } from 'expo-router';
import { Button } from '@/components/ui/Button';
import { UiInput, type UiInputHandle } from '@/components/ui/UiInput';
import { ApiRequestError } from '@/src/lib/api';
import { safeNextPath } from '@/src/lib/navigation';
import { useAuthStore } from '@/src/stores/auth';

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^[+\d][\d\s/().-]{5,}$/;

export default function Register() {
  const params = useLocalSearchParams<{ next?: string }>();
  const register = useAuthStore((s) => s.register);

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState('');

  const firstNameInput = useRef<UiInputHandle>(null);
  const lastNameInput = useRef<UiInputHandle>(null);
  const emailInput = useRef<UiInputHandle>(null);
  const phoneInput = useRef<UiInputHandle>(null);
  const passwordInput = useRef<UiInputHandle>(null);
  const confirmInput = useRef<UiInputHandle>(null);

  async function submit(): Promise<void> {
    setFormError('');
    const results = [
      firstNameInput.current?.validate() ?? false,
      lastNameInput.current?.validate() ?? false,
      emailInput.current?.validate() ?? false,
      phoneInput.current?.validate() ?? false,
      passwordInput.current?.validate() ?? false,
      confirmInput.current?.validate() ?? false,
    ];
    if (results.includes(false)) return;
    setLoading(true);
    try {
      await register({ firstName, lastName, email, phone, password });
      router.replace(safeNextPath(params.next) as '/booking');
    } catch (error) {
      if (error instanceof ApiRequestError) {
        if (error.status === 409) {
          const title = error.title.toLowerCase();
          if (title.includes('email')) emailInput.current?.setError('EMAIL JE ZAUZET');
          else if (title.includes('phone')) phoneInput.current?.setError('TELEFON JE ZAUZET');
          else setFormError('NALOG VEĆ POSTOJI');
          return;
        }
        if (error.status === 400) {
          applyFieldErrors(error.errors);
          return;
        }
        if (error.status === 429) {
          setFormError('PREVIŠE POKUŠAJA, POKUŠAJ KASNIJE');
          return;
        }
      }
      setFormError('GREŠKA U VEZI, POKUŠAJ PONOVO');
    } finally {
      setLoading(false);
    }
  }

  function applyFieldErrors(errors: Record<string, string[]>): void {
    let leftover = false;
    for (const key of Object.keys(errors)) {
      if (!errors[key]?.length) continue;
      if (key === 'Email') emailInput.current?.setError('UNESI ISPRAVAN EMAIL');
      else if (key === 'Phone') phoneInput.current?.setError('UNESI ISPRAVAN TELEFON');
      else if (key === 'Password') passwordInput.current?.setError('MIN 8 KARAKTERA');
      else if (key === 'FirstName') firstNameInput.current?.setError('IME JE OBAVEZNO');
      else if (key === 'LastName') lastNameInput.current?.setError('PREZIME JE OBAVEZNO');
      else leftover = true;
    }
    if (leftover) setFormError('NEISPRAVAN UNOS');
  }

  return (
    <SafeAreaView className="flex-1 bg-ink">
      <KeyboardAvoidingView
        className="flex-1"
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <ScrollView
          className="flex-1"
          contentContainerStyle={{ flexGrow: 1 }}
          keyboardShouldPersistTaps="handled"
        >
        <View className="flex-1 justify-center px-4 py-10">
          <Text className="text-center font-display text-6xl uppercase leading-none tracking-wide text-bone">
            REGISTRACIJA
          </Text>
          <Text className="mt-2 text-center font-display text-xl uppercase tracking-widest text-ash">
            NAPRAVI NALOG DA ZAKAŽEŠ TERMIN
          </Text>
          <View className="mt-8">
            <View className="flex-row gap-3">
              <View className="flex-1">
                <UiInput
                  ref={firstNameInput}
                  label="IME"
                  value={firstName}
                  onChangeText={setFirstName}
                  autoCapitalize="words"
                  placeholder="NIKOLA"
                  rules={[(v) => (!v ? 'IME JE OBAVEZNO' : '')]}
                />
              </View>
              <View className="flex-1">
                <UiInput
                  ref={lastNameInput}
                  label="PREZIME"
                  value={lastName}
                  onChangeText={setLastName}
                  autoCapitalize="words"
                  placeholder="PETROVIĆ"
                  rules={[(v) => (!v ? 'PREZIME JE OBAVEZNO' : '')]}
                />
              </View>
            </View>
            <View className="mt-2">
              <UiInput
                ref={emailInput}
                label="EMAIL"
                value={email}
                onChangeText={setEmail}
                keyboardType="email-address"
                placeholder="YOU@MAIL.COM"
                rules={[
                  (v) => (!v ? 'EMAIL JE OBAVEZAN' : ''),
                  (v) => (!EMAIL_RE.test(v) ? 'UNESI ISPRAVAN EMAIL' : ''),
                ]}
              />
            </View>
            <View className="mt-2">
              <UiInput
                ref={phoneInput}
                label="TELEFON"
                value={phone}
                onChangeText={setPhone}
                keyboardType="phone-pad"
                placeholder="+381 60 000 00 00"
                rules={[
                  (v) => (!v ? 'TELEFON JE OBAVEZAN' : ''),
                  (v) => (!PHONE_RE.test(v) ? 'UNESI ISPRAVAN TELEFON' : ''),
                ]}
              />
            </View>
            <View className="mt-2">
              <UiInput
                ref={passwordInput}
                label="LOZINKA"
                value={password}
                onChangeText={setPassword}
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
                ref={confirmInput}
                label="POTVRDI LOZINKU"
                value={confirm}
                onChangeText={setConfirm}
                secure
                placeholder="PONOVI LOZINKU"
                rules={[
                  (v) => (!v ? 'POTVRDI LOZINKU' : ''),
                  (v) => (v !== password ? 'LOZINKE SE NE POKLAPAJU' : ''),
                ]}
              />
            </View>
            {formError ? (
              <Text className="mt-4 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                {formError}
              </Text>
            ) : null}
            <View className="mt-6">
              <Button
                title={loading ? 'UČITAVANJE...' : 'REGISTRUJ SE'}
                variant="blaze"
                disabled={loading}
                onPress={() => void submit()}
              />
            </View>
          </View>
          <Text className="mt-6 text-center font-display text-lg uppercase tracking-widest text-ash">
            IMAŠ NALOG?{' '}
            <Link href="/login" className="text-blue">
              PRIJAVI SE
            </Link>
          </Text>
        </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
