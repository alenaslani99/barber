import { Pressable, ScrollView, Text } from 'react-native';
import type { DayOption } from '@/src/lib/days';

export function DateStrip({
  days,
  selectedIso,
  onSelect,
}: {
  days: DayOption[];
  selectedIso: string | null;
  onSelect: (iso: string) => void;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      accessibilityLabel="Pick a day"
      contentContainerStyle={{ gap: 8, paddingBottom: 8 }}
    >
      {days.map((d) => {
        const active = selectedIso === d.iso;
        return (
          <Pressable
            key={d.iso}
            accessibilityRole="button"
            accessibilityState={{ selected: active, disabled: d.closed }}
            disabled={d.closed}
            onPress={() => onSelect(d.iso)}
            className={
              active
                ? 'w-16 shrink-0 items-center border border-bone bg-bone py-2'
                : d.closed
                  ? 'w-16 shrink-0 items-center border border-line bg-surface py-2 opacity-40'
                  : 'w-16 shrink-0 items-center border border-line bg-surface py-2'
            }
            style={({ pressed }) => ({ opacity: d.closed ? 0.4 : pressed ? 0.7 : 1 })}
          >
            <Text
              className={
                active
                  ? 'font-display text-base uppercase tracking-widest text-ink'
                  : 'font-display text-base uppercase tracking-widest text-ash'
              }
            >
              {d.weekday}
            </Text>
            <Text
              className={
                active
                  ? 'font-display text-3xl uppercase leading-none tracking-widest text-ink'
                  : 'font-display text-3xl uppercase leading-none tracking-widest text-bone'
              }
            >
              {d.dayNum}
            </Text>
            <Text
              className={
                active
                  ? 'font-display text-base uppercase tracking-widest text-ink'
                  : 'font-display text-base uppercase tracking-widest text-ash'
              }
            >
              {d.closed ? 'NE RADI' : d.month}
            </Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}
