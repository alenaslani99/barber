import { Linking, Pressable, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { ApiShopHour } from '@/src/lib/catalog';
import { ShopHeader } from '@/components/layout/ShopHeader';
import { DUMMY_SHOP } from '@/components/layout/shop-dummy';
import { useShopStore } from '@/src/stores/shop';

const DAY_NAMES = [
  'NEDELJA',
  'PONEDELJAK',
  'UTORAK',
  'SREDA',
  'ČETVRTAK',
  'PETAK',
  'SUBOTA',
];

export default function Info() {
  const shop = useShopStore((s) => s.shop);

  const name = shop?.name ?? DUMMY_SHOP.name;
  const description = shop?.description ?? DUMMY_SHOP.description;
  const address = shop?.address ?? DUMMY_SHOP.address;
  const phone = shop?.phone ?? DUMMY_SHOP.phone;
  const hours = shop?.hours ?? DUMMY_SHOP.hours;

  const byDay = new Map(hours.map((h) => [h.day, h]));
  const orderedHours = [1, 2, 3, 4, 5, 6, 0]
    .map((day) => byDay.get(day))
    .filter((h): h is ApiShopHour => h !== undefined);

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-ink">
      <ScrollView className="flex-1" contentContainerStyle={{ flexGrow: 1 }}>
        <ShopHeader name={name} />
        <View className="flex-1 px-4 pb-8">
          <Text className="mt-6 font-display text-lg uppercase tracking-widest text-ash">
            O SALONU
          </Text>
          <Text className="mt-2 font-display text-2xl uppercase leading-snug tracking-widest text-bone">
            {description}
          </Text>

          <Text className="mt-8 font-display text-lg uppercase tracking-widest text-ash">
            RADNO VREME
          </Text>
          <View accessibilityLabel="Working hours" className="mt-2 border-y border-line">
            {orderedHours.map((h) => (
              <View
                key={h.day}
                className="flex-row items-center justify-between border-t border-line py-2 first:border-t-0"
              >
                <Text className="font-display text-lg uppercase tracking-widest text-ash">
                  {DAY_NAMES[h.day]}
                </Text>
                <Text className="font-display text-lg uppercase tracking-widest text-bone">
                  {h.closed ? 'ZATVORENO' : `${h.open} - ${h.close}`}
                </Text>
              </View>
            ))}
          </View>

          <Text className="mt-8 font-display text-lg uppercase tracking-widest text-ash">
            LOKACIJA
          </Text>
          <Pressable
            accessibilityRole="link"
            accessibilityLabel="Otvori mapu"
            onPress={() =>
              Linking.openURL(
                `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`,
              )
            }
            className="mt-2 aspect-square w-full items-center justify-center border border-line bg-surface"
          >
            <Text className="font-display text-2xl uppercase tracking-widest text-ash">
              MAPA USKORO
            </Text>
            <Text className="mt-2 px-6 text-center font-display text-lg uppercase tracking-widest text-bone">
              {address}
            </Text>
          </Pressable>
          <Pressable
            accessibilityRole="link"
            onPress={() => Linking.openURL(`tel:${phone.replace(/[^+\d]/g, '')}`)}
            className="mt-3 border border-line py-3"
          >
            <Text className="text-center font-display text-2xl uppercase tracking-widest text-blue">
              {phone}
            </Text>
          </Pressable>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}
