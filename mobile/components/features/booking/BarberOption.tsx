import { Pressable, Text, View } from 'react-native';
import type { ApiBarber } from '@/src/lib/catalog';

export function BarberOption({
  barber,
  selected,
  onSelect,
}: {
  barber: ApiBarber;
  selected: boolean;
  onSelect: (id: string) => void;
}) {
  const initials = `${barber.firstName.charAt(0)}${barber.lastName.charAt(0)}`;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ selected }}
      onPress={() => onSelect(barber.id)}
      className={
        selected
          ? 'w-full flex-row items-center gap-4 border border-bone bg-bone p-4'
          : 'w-full flex-row items-center gap-4 border border-line bg-surface p-4'
      }
      style={({ pressed }) => ({ opacity: pressed ? 0.7 : 1 })}
    >
      <View
        className={
          selected
            ? 'h-12 w-12 shrink-0 items-center justify-center border border-ink bg-ink'
            : 'h-12 w-12 shrink-0 items-center justify-center border border-line bg-ink'
        }
      >
        <Text className="font-display text-2xl uppercase tracking-widest text-bone">
          {initials}
        </Text>
      </View>
      <View>
        <Text
          className={
            selected
              ? 'font-display text-2xl uppercase leading-none tracking-widest text-ink'
              : 'font-display text-2xl uppercase leading-none tracking-widest text-bone'
          }
        >
          {barber.firstName} {barber.lastName}
        </Text>
        <Text
          className={
            selected
              ? 'font-display text-lg uppercase tracking-widest text-ink'
              : 'font-display text-lg uppercase tracking-widest text-ash'
          }
        >
          {barber.seniority}
        </Text>
      </View>
    </Pressable>
  );
}
