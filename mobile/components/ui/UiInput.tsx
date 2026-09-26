import { forwardRef, useImperativeHandle, useState } from 'react';
import { Pressable, Text, TextInput, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';

export interface UiInputHandle {
  validate: () => boolean;
  setError: (message: string) => void;
}

interface UiInputProps {
  label: string;
  value: string;
  onChangeText: (value: string) => void;
  secure?: boolean;
  keyboardType?: 'default' | 'email-address' | 'phone-pad';
  placeholder?: string;
  autoCapitalize?: 'none' | 'sentences' | 'words' | 'characters';
  rules?: Array<(value: string) => string>;
}

export const UiInput = forwardRef<UiInputHandle, UiInputProps>(function UiInput(
  {
    label,
    value,
    onChangeText,
    secure = false,
    keyboardType = 'default',
    placeholder = '',
    autoCapitalize = 'none',
    rules = [],
  },
  ref,
) {
  const [error, setErrorState] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const inputClass = secure
    ? error
      ? 'w-full border border-alarm bg-ink px-3 py-4 pr-16 font-form text-2xl text-bone'
      : 'w-full border border-line bg-ink px-3 py-4 pr-16 font-form text-2xl text-bone'
    : error
      ? 'w-full border border-alarm bg-ink px-3 py-4 font-form text-2xl text-bone'
      : 'w-full border border-line bg-ink px-3 py-4 font-form text-2xl text-bone';

  useImperativeHandle(ref, () => ({
    validate: () => {
      for (const rule of rules) {
        const message = rule(value);
        if (message) {
          setErrorState(message);
          return false;
        }
      }
      setErrorState('');
      return true;
    },
    setError: (message: string) => setErrorState(message),
  }));

  return (
    <View>
      <Text className="font-display text-lg uppercase tracking-widest text-ash">{label}</Text>
      <View className="relative mt-1">
        <TextInput
          value={value}
          onChangeText={(next) => {
            onChangeText(next);
            if (error) setErrorState('');
          }}
          secureTextEntry={secure && !showPassword}
          keyboardType={keyboardType}
          placeholder={placeholder}
          placeholderTextColor="#A1A1A1"
          autoCapitalize={autoCapitalize}
          accessibilityLabel={label}
          className={inputClass}
        />
        {secure ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={showPassword ? 'Sakrij lozinku' : 'Prikaži lozinku'}
            onPress={() => setShowPassword((v) => !v)}
            className="absolute right-3 top-1/2 -translate-y-1/2"
          >
            <Ionicons
              name={showPassword ? 'eye-off' : 'eye'}
              size={24}
              color="#A1A1A1"
            />
          </Pressable>
        ) : null}
      </View>
      {/* Reserved error slot (min height) so validation never shifts layout. */}
      <Text className="mt-1 min-h-6 font-display text-lg uppercase tracking-widest text-alarm">
        {error}
      </Text>
    </View>
  );
});
