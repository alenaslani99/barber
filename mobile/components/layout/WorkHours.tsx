import { useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import type { ApiShopHour } from '@/src/lib/catalog';
import { DUMMY_SHOP } from './shop-dummy';

const DAY_NAMES = [
  'NEDELJA',
  'PONEDELJAK',
  'UTORAK',
  'SREDA',
  'ČETVRTAK',
  'PETAK',
  'SUBOTA',
];

export function WorkHours({ hours = DUMMY_SHOP.hours }: { hours?: ApiShopHour[] }) {
  const [open, setOpen] = useState(false);

  const byDay = new Map(hours.map((h) => [h.day, h]));
  const orderedHours = [1, 2, 3, 4, 5, 6, 0]
    .map((day) => byDay.get(day))
    .filter((h): h is ApiShopHour => h !== undefined);

  const today = hours.find((h) => h.day === new Date().getDay());
  const todaySummary =
    !today || today.closed ? 'DANAS ZATVORENO' : `DANAS ${today.open} - ${today.close}`;

  return (
    <View className="border-y border-line">
      <Pressable
        accessibilityRole="button"
        accessibilityState={{ expanded: open }}
        onPress={() => setOpen((v) => !v)}
        className="flex-row items-center justify-between gap-4 py-2"
      >
        <Text className="font-display text-lg uppercase tracking-widest text-ash">RADNO VREME</Text>
        <Text className="font-display text-lg uppercase tracking-widest text-ash">
          {open ? 'PRIKAŽI MANJE' : 'PRIKAŽI VIŠE'}
        </Text>
        <Text className="font-display text-lg uppercase tracking-widest text-bone">
          {todaySummary}
        </Text>
      </Pressable>
      {open ? (
        <View accessibilityLabel="Working hours" className="border-t border-line">
          {orderedHours.map((h) => (
            <View key={h.day} className="flex-row items-center justify-between py-2">
              <Text className="font-display text-lg uppercase tracking-widest text-ash">
                {DAY_NAMES[h.day]}
              </Text>
              <Text className="font-display text-lg uppercase tracking-widest text-bone">
                {h.closed ? 'ZATVORENO' : `${h.open} - ${h.close}`}
              </Text>
            </View>
          ))}
        </View>
      ) : null}
    </View>
  );
}
