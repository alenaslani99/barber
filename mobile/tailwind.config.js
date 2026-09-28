/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './app/**/*.{js,jsx,ts,tsx}',
    './components/**/*.{js,jsx,ts,tsx}',
    './constants/**/*.{js,jsx,ts,tsx}',
    './src/**/*.{js,jsx,ts,tsx}',
  ],
  presets: [require('nativewind/preset')],
  theme: {
    extend: {
      colors: {
        ink: '#080808',
        surface: '#111111',
        bone: '#F5F5F0',
        ash: '#A1A1A1',
        line: '#262626',
        blaze: '#FF5C00',
        blue: '#3A86FF',
        alarm: '#FF4D4D',
      },
      fontFamily: {
        display: ['BebasNeue_400Regular'],
        form: ['Inter_400Regular'],
      },
    },
  },
  plugins: [],
};
