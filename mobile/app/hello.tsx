import { Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useDisplayName } from '@/src/stores/auth';

// Temporary post-login landing until the booking screen exists.
export default function Hello() {
  const displayName = useDisplayName();
  return (
    <SafeAreaView className="flex-1 bg-ink">
      <View className="flex-1 items-center justify-center px-4">
        <Text className="text-center font-display text-6xl uppercase leading-none tracking-wide text-bone">
          HELLO
        </Text>
        <Text className="mt-2 text-center font-display text-2xl uppercase tracking-widest text-blaze">
          {displayName}
        </Text>
      </View>
    </SafeAreaView>
  );
}
