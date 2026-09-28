import { Pressable, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

const STEPS: Array<{ n: number; label: string; icon: keyof typeof Ionicons.glyphMap }> = [
  { n: 1, label: 'BERBERIN', icon: 'cut' },
  { n: 2, label: 'USLUGA', icon: 'sparkles' },
  { n: 3, label: 'TERMIN', icon: 'time' },
  { n: 4, label: 'REZERVACIJA', icon: 'ticket' },
];

export function BookingStepper({ step, onGo }: { step: number; onGo: (step: number) => void }) {
  return (
    <View accessibilityLabel="Booking steps" className="mt-6 flex-row border border-line">
      {STEPS.map((s) => {
        const active = s.n === step;
        const done = s.n < step;
        const locked = s.n > step;
        return (
          <Pressable
            key={s.n}
            accessibilityRole="button"
            disabled={locked}
            onPress={() => onGo(s.n)}
            className={
              active
                ? 'flex-1 items-center border-r border-line bg-bone py-2'
                : 'flex-1 items-center border-r border-line py-2'
            }
            style={({ pressed }) => ({ opacity: locked ? 0.6 : pressed ? 0.7 : 1 })}
          >
            <Text
              className={
                active
                  ? 'font-display text-sm uppercase tracking-widest text-ink'
                  : done
                    ? 'font-display text-sm uppercase tracking-widest text-bone'
                    : 'font-display text-sm uppercase tracking-widest text-ash'
              }
            >
              0{s.n}
            </Text>
            <Text
              className={
                active
                  ? 'font-display text-lg uppercase tracking-widest text-ink'
                  : done
                    ? 'font-display text-lg uppercase tracking-widest text-bone'
                    : 'font-display text-lg uppercase tracking-widest text-ash'
              }
            >
              {s.label}
            </Text>
            <Ionicons
              name={s.icon}
              size={16}
              color={active ? '#080808' : done ? '#F5F5F0' : '#A1A1A1'}
            />
          </Pressable>
        );
      })}
    </View>
  );
}
