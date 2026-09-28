import { Tabs } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          backgroundColor: '#080808',
          borderTopColor: '#262626',
          borderTopWidth: 1,
          height: 88,
        },
        tabBarActiveTintColor: '#3A86FF',
        tabBarInactiveTintColor: '#A1A1A1',
        tabBarLabelStyle: {
          fontFamily: 'BebasNeue_400Regular',
          fontSize: 16,
          letterSpacing: 1,
        },
      }}
    >
      <Tabs.Screen
        name="booking"
        options={{
          title: 'ZAKAŽI',
          tabBarItemStyle: { borderRightWidth: 1, borderRightColor: '#262626' },
          tabBarIcon: ({ color }) => <Ionicons name="cut" color={color} size={28} />,
        }}
      />
      <Tabs.Screen
        name="info"
        options={{
          title: 'INFO',
          tabBarItemStyle: { borderRightWidth: 1, borderRightColor: '#262626' },
          tabBarIcon: ({ color }) => (
            <Ionicons name="information-circle" color={color} size={28} />
          ),
        }}
      />
      <Tabs.Screen
        name="account"
        options={{
          title: 'NALOG',
          tabBarIcon: ({ color }) => <Ionicons name="person" color={color} size={28} />,
        }}
      />
    </Tabs>
  );
}
