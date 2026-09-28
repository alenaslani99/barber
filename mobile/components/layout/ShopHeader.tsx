import { Text, View } from 'react-native';
import { DUMMY_SHOP } from './shop-dummy';

// Slim header: barbershop name only. Hours/address/phone live on the INFO tab.
export function ShopHeader({ name = DUMMY_SHOP.name }: { name?: string }) {
  return (
    <View accessibilityLabel="Barbershop info" className="bg-ink">
      <View className="border-b border-line px-4 py-4">
        <Text className="font-display text-6xl uppercase leading-none tracking-wide text-bone">
          {name}
        </Text>
      </View>
    </View>
  );
}
