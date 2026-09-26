import { Redirect } from 'expo-router';
import { useAuthStore } from '@/src/stores/auth';

export default function Index() {
  const hydrated = useAuthStore((s) => s.hydrated);
  const accessToken = useAuthStore((s) => s.accessToken);
  if (!hydrated) return null;
  return <Redirect href={accessToken ? '/hello' : '/login'} />;
}
