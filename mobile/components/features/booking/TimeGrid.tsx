import { Pressable, Text, View } from 'react-native';
import type { TimeSlot } from '@/src/lib/days';

export function TimeGrid({
  slots,
  selectedTime,
  onSelect,
}: {
  slots: TimeSlot[];
  selectedTime: string | null;
  onSelect: (time: string) => void;
}) {
  return (
    <View accessibilityLabel="Pick a time" className="flex-row flex-wrap gap-2">
      {slots.map((s) => {
        const active = selectedTime === s.time;
        return (
          <Pressable
            key={s.id}
            accessibilityRole="button"
            accessibilityState={{ selected: active, disabled: !s.available }}
            disabled={!s.available}
            onPress={() => onSelect(s.time)}
            className={
              active
                ? 'w-[31%] border border-bone bg-bone py-2'
                : s.available
                  ? 'w-[31%] border border-line bg-surface py-2'
                  : 'w-[31%] border border-line bg-surface py-2 opacity-30'
            }
            style={({ pressed }) => ({ opacity: !s.available ? 0.3 : pressed ? 0.7 : 1 })}
          >
            <Text
              className={
                active
                  ? 'text-center font-display text-xl uppercase tracking-widest text-ink'
                  : s.available
                    ? 'text-center font-display text-xl uppercase tracking-widest text-bone'
                    : 'text-center font-display text-xl uppercase tracking-widest text-bone line-through'
              }
            >
              {s.time}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
