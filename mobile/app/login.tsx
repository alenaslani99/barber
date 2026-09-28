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

export default function Login() {
  const params = useLocalSearchParams<{ next?: string }>();
  const login = useAuthStore((s) => s.login);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState('');

  const emailInput = useRef<UiInputHandle>(null);
  const passwordInput = useRef<UiInputHandle>(null);

  async function submit(): Promise<void> {
    setFormError('');
    const emailOk = emailInput.current?.validate() ?? false;
    const passOk = passwordInput.current?.validate() ?? false;
    if (!emailOk || !passOk) return;
    setLoading(true);
    try {
      // Session persists in SecureStore on every login — no remember checkbox
      // on mobile. Logout is explicit only.
      await login(email, password);
      router.replace(safeNextPath(params.next) as '/booking');
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        setFormError('POGREŠAN EMAIL ILI LOZINKA');
      } else if (error instanceof ApiRequestError && error.status === 429) {
        setFormError('PREVIŠE POKUŠAJA, POKUŠAJ KASNIJE');
      } else {
        setFormError('GREŠKA U VEZI, POKUŠAJ PONOVO');
      }
    } finally {
      setLoading(false);
    }
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
              PRIJAVA
            </Text>
            <Text className="mt-2 text-center font-display text-xl uppercase tracking-widest text-ash">
              PRIJAVI SE DA ZAKAŽEŠ TERMIN
            </Text>
            <View className="mt-8">
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
              {formError ? (
                <Text className="mt-4 border border-alarm p-3 text-center font-display text-lg uppercase tracking-widest text-alarm">
                  {formError}
                </Text>
              ) : null}
              <View className="mt-6">
                <Button
                  title={loading ? 'UČITAVANJE...' : 'PRIJAVI SE'}
                  variant="blaze"
                  disabled={loading}
                  onPress={() => void submit()}
                />
              </View>
            </View>
            <Text className="mt-6 text-center font-display text-lg uppercase tracking-widest text-ash">
              NEMAŠ NALOG?{' '}
              <Link href="/register" className="text-blue">
                REGISTRUJ SE
              </Link>
            </Text>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
