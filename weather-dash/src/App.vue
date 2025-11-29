<script setup lang="ts">
import { ref } from 'vue'

// 1. Define the Interface (Matches your C# WeatherModel)
interface WeatherData {
  city: string;
  temperature: number;
  weather: string;
  precipitation: number;
}

// 2. Reactive Variables
const weather = ref<WeatherData | null>(null)
const cityInput = ref('')
const loading = ref(false)
const error = ref('')

// 3. Fetch Logic
const fetchWeather = async () => {
  if (!cityInput.value) return;

  loading.value = true
  error.value = ''
  weather.value = null

  try {
    // ⚠️ CRITICAL: Check if 5160 matches your dotnet run output!
    const response = await fetch(`http://localhost:5160/Weather/${cityInput.value}`)

    if (!response.ok) throw new Error("City not found")

    const data: WeatherData = await response.json()
    weather.value = data
  } catch (err) {
    error.value = (err as Error).message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div style="font-family: sans-serif; text-align: center; margin-top: 50px;">
    <h1>🌩️ Weather & Vue Connection</h1>

    <div style="margin-bottom: 20px;">
      <input
          v-model="cityInput"
          @keyup.enter="fetchWeather"
          placeholder="Enter City (e.g. London)"
          style="padding: 10px; font-size: 16px;"
      />
      <button @click="fetchWeather" style="padding: 10px 15px; margin-left: 10px; cursor: pointer;">
        {{ loading ? 'Searching...' : 'Search' }}
      </button>
    </div>

    <p v-if="error" style="color: red; font-weight: bold;">{{ error }}</p>

    <div v-if="weather" style="border: 1px solid #ddd; display: inline-block; padding: 20px; border-radius: 8px; background: #808080;">
      <h2>{{ weather.city }}</h2>
      <p>Temperature: <strong>{{ weather.temperature }}°C</strong></p>
      <p>Condition: <strong>{{ weather.weather }}</strong></p>
      <p>Humidity: <strong>{{ weather.precipitation }}%</strong></p>
    </div>
  </div>
</template>

