import { Text, View } from 'react-native';

export function BookingSummary({
  barberName,
  serviceName,
  durationMin,
  price,
  dateLabel,
  time,
}: {
  barberName: string;
  serviceName: string;
  durationMin: number;
  price: number;
  dateLabel: string;
  time: string;
}) {
  return (
    <View accessibilityLabel="Rezime rezervacije" className="border-y border-line">
      <SummaryRow label="BERBERIN" value={barberName} />
      <SummaryRow label="USLUGA" value={serviceName} bordered />
      <SummaryRow label="KADA" value={`${dateLabel} / ${time}`} bordered />
      <SummaryRow label="TRAJANJE" value={`${durationMin} MIN`} bordered />
      <View className="flex-row items-center justify-between border-t border-line py-2">
        <Text className="font-display text-lg uppercase tracking-widest text-ash">UKUPNO</Text>
        <Text className="font-display text-3xl uppercase tracking-widest text-blue">{price}</Text>
      </View>
    </View>
  );
}

function SummaryRow({
  label,
  value,
  bordered = false,
}: {
  label: string;
  value: string;
  bordered?: boolean;
}) {
  return (
    <View
      className={
        bordered
          ? 'flex-row items-center justify-between border-t border-line py-2'
          : 'flex-row items-center justify-between py-2'
      }
    >
      <Text className="font-display text-lg uppercase tracking-widest text-ash">{label}</Text>
      <Text className="font-display text-xl uppercase tracking-widest text-bone">{value}</Text>
    </View>
  );
}
