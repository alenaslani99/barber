import { Pressable, Text } from 'react-native';

type ButtonVariant = 'bone' | 'blaze' | 'outline' | 'danger';

interface ButtonProps {
  title: string;
  onPress: () => void;
  variant?: ButtonVariant;
  disabled?: boolean;
}

export function Button({ title, onPress, variant = 'bone', disabled = false }: ButtonProps) {
  const container =
    variant === 'bone'
      ? 'w-full bg-bone py-3'
      : variant === 'blaze'
        ? 'w-full bg-blaze py-3'
        : variant === 'danger'
          ? 'w-full border border-alarm bg-transparent py-3'
          : 'w-full border border-line bg-transparent py-3';
  const label =
    variant === 'outline'
      ? 'text-center font-display text-2xl uppercase tracking-widest text-bone'
      : variant === 'danger'
        ? 'text-center font-display text-2xl uppercase tracking-widest text-alarm'
        : 'text-center font-display text-2xl uppercase tracking-widest text-ink';
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={title}
      accessibilityState={{ disabled }}
      disabled={disabled}
      onPress={onPress}
      className={container}
      style={({ pressed }) => ({ opacity: disabled ? 0.4 : pressed ? 0.7 : 1 })}
    >
      <Text className={label}>{title}</Text>
    </Pressable>
  );
}
