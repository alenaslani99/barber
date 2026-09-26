import { Linking, Pressable, Text, View } from 'react-native';
import { DUMMY_SHOP } from './shop-dummy';

interface ShopFooterProps {
  address?: string;
  phone?: string;
}

// Bottom-of-page footer: placed last in page content, spacing via margin/padding.
export function ShopFooter({
  address = DUMMY_SHOP.address,
  phone = DUMMY_SHOP.phone,
}: ShopFooterProps) {
  return (
    <View className="mt-auto px-4 pb-6 pt-10">
      <View className="flex-row items-center justify-between gap-4 border-t border-line pt-3">
        <Pressable
          accessibilityRole="link"
          onPress={() =>
            Linking.openURL(
              `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`,
            )
          }
        >
          <Text className="font-display text-lg uppercase tracking-widest text-ash">{address}</Text>
        </Pressable>
        <Pressable
          accessibilityRole="link"
          onPress={() => Linking.openURL(`tel:${phone.replace(/[^+\d]/g, '')}`)}
        >
          <Text className="font-display text-lg uppercase tracking-widest text-bone">{phone}</Text>
        </Pressable>
      </View>
    </View>
  );
}
