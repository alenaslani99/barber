import { createApp } from 'vue'
import '@fontsource-variable/inter'
import '@fontsource-variable/jetbrains-mono'
import './styles/tokens.css'
import App from './App.vue'
import router from './router'

createApp(App).use(router).mount('#app')
