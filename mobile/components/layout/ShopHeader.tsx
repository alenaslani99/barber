import { Link } from 'expo-router';
import { Pressable, Text, View } from 'react-native';
import { useDisplayName, useIsAuthenticated } from '@/src/stores/auth';

interface ShopHeaderProps {
  name?: string;
  tagline?: string;
  description?: string;
  hideAuthLink?: boolean;
}

export function ShopHeader({
  name,
  tagline,
  description,
  hideAuthLink = false,
}: ShopHeaderProps) {
  const isAuthenticated = useIsAuthenticated();
  const displayName = useDisplayName();

  return (
    <View accessibilityLabel="Barbershop info" className="bg-ink">
      <View className="border-b border-line px-4">
        <View className="flex-row items-center justify-between py-3">
          <Text className="font-display text-lg uppercase tracking-widest text-ash">{tagline}</Text>
          {!hideAuthLink ? (
            isAuthenticated ? (
              <View accessibilityLabel="Nalog" className="border border-blaze px-4 py-1">
                <Text className="font-display text-lg uppercase tracking-widest text-blaze">
                  {displayName}
                </Text>
              </View>
            ) : (
              <Link href="/login" asChild>
                <Pressable
                  accessibilityRole="button"
                  accessibilityLabel="Prijava"
                  className="border border-blaze px-4 py-1"
                >
                  <Text className="font-display text-lg uppercase tracking-widest text-blaze">
                    PRIJAVA
                  </Text>
                </Pressable>
              </Link>
            )
          ) : null}
        </View>

        <Text className="font-display text-6xl uppercase leading-none tracking-wide text-bone">
          {name}
        </Text>
        <Text className="mt-2 pb-4 font-display text-xl uppercase tracking-widest text-ash">
          {description}
        </Text>
      </View>
    </View>
  );
}
