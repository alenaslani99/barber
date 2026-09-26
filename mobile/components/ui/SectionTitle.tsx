import { Text } from 'react-native';

export function SectionTitle({ children }: { children: string }) {
  return (
    <Text className="mt-8 font-display text-3xl uppercase tracking-widest text-bone">
      {children}
    </Text>
  );
}
