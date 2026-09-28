import { Pressable, Text, View } from 'react-native';
import type { ApiService } from '@/src/lib/catalog';

export function ServiceOption({
  service,
  selected,
  onSelect,
}: {
  service: ApiService;
  selected: boolean;
  onSelect: (id: string) => void;
}) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ selected }}
      onPress={() => onSelect(service.id)}
      className={
        selected
          ? 'w-full flex-row items-center justify-between gap-3 border border-bone bg-bone p-4'
          : 'w-full flex-row items-center justify-between gap-3 border border-line bg-surface p-4'
      }
      style={({ pressed }) => ({ opacity: pressed ? 0.7 : 1 })}
    >
      <View>
        <Text
          className={
            selected
              ? 'font-display text-2xl uppercase leading-none tracking-widest text-ink'
              : 'font-display text-2xl uppercase leading-none tracking-widest text-bone'
          }
        >
          {service.name}
        </Text>
        <Text
          className={
            selected
              ? 'font-display text-lg uppercase tracking-widest text-ink'
              : 'font-display text-lg uppercase tracking-widest text-ash'
          }
        >
          {service.durationMinutes} MIN
        </Text>
      </View>
      <Text
        className={
          selected
            ? 'shrink-0 font-display text-3xl uppercase tracking-widest text-ink'
            : 'shrink-0 font-display text-3xl uppercase tracking-widest text-blue'
        }
      >
        {service.price}
      </Text>
    </Pressable>
  );
}
