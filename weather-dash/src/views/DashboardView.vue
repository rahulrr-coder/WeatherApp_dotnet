<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

// Types
interface DayPart { partName: string; temp: number; condition: string; }
interface WeatherData { city: string; currentTemp: number; currentCondition: string; humidity: number; windSpeed: number; aqi: number; maxTemp: number; minTemp: number; dayParts: DayPart[]; }
interface AiAdvice { summary: string; outfit: string; safety: string; }

// State
const router = useRouter()
const username = ref(localStorage.getItem('userName') || 'Guest')
const userEmail = ref(localStorage.getItem('userEmail') || '') // Should now be correct!
const viewState = ref<'HOME' | 'CITY'>('HOME')

const weather = ref<WeatherData | null>(null)
const aiData = ref<AiAdvice | null>(null)
const favorites = ref<string[]>([])
const cityInput = ref('')
const loading = ref(false)
const aiLoading = ref(false)
const showProfile = ref(false)

// Actions
const loadFavorites = async () => {
  try {
    const res = await axios.get(`http://localhost:5160/api/Favorites/${username.value}`)
    favorites.value = res.data;
  } catch { favorites.value = [] }
}

const goHome = () => {
  viewState.value = 'HOME';
  weather.value = null;
  cityInput.value = '';
}

const fetchWeather = async (city: string) => {
  if (!city) return;
  loading.value = true;
  viewState.value = 'CITY';
  weather.value = null;
  aiData.value = null;

  try {
    const res = await axios.get<WeatherData>(`http://localhost:5160/api/Weather/${city}`);
    weather.value = res.data;
    
    // AI Call
    aiLoading.value = true;
    try {
        const aiRes = await axios.get(`http://localhost:5160/api/Weather/advice?city=${city}`);
        const raw = aiRes.data.advice.replace(/```json|```/g, '').trim();
        aiData.value = JSON.parse(raw);
    } catch { aiData.value = { summary: `Enjoy ${city}.`, outfit: "-", safety: "-" }; }
    aiLoading.value = false;

  } catch { 
    alert("City not found");
    viewState.value = 'HOME';
  } 
  finally { loading.value = false; }
}

const logout = () => { localStorage.clear(); router.push('/'); }
const isFav = () => weather.value ? favorites.value.includes(weather.value.city) : false;

const toggleFav = async () => {
  if (!weather.value) return;
  const city = weather.value.city;
  try {
    if (isFav()) {
      await axios.delete(`http://localhost:5160/api/Favorites/${username.value}/${city}`);
      favorites.value = favorites.value.filter(c => c !== city);
    } else {
      await axios.post(`http://localhost:5160/api/Favorites`, { username: username.value, cityName: city });
      favorites.value.push(city);
    }
  } catch {}
}

onMounted(() => { loadFavorites(); })
</script>

<template>
  <div class="luxury-app">
    <component is="style">
      @import url('https://fonts.googleapis.com/css2?family=Lato:wght@300;400;700&family=Playfair+Display:ital,wght@0,400;0,600;1,400&display=swap');
    </component>

    <nav class="navbar">
      <div class="logo" @click="goHome">Atmosphere<span class="dot">.</span></div>
      
      <div class="profile-trigger" @click="showProfile = !showProfile">
        <span class="initial">{{ username.charAt(0).toUpperCase() }}</span>
        
        <div v-if="showProfile" class="dropdown fade-in">
          <div class="meta">
            <span class="u-name">{{ username }}</span>
            <span class="u-email">{{ userEmail }}</span>
          </div>
          <button @click="logout">Log Out</button>
        </div>
      </div>
    </nav>

    <main class="main-body">

      <div v-if="viewState === 'HOME'" class="home-view fade-in">
        <div class="hero">
          <h1>Explore the air around you.</h1>
          <div class="search-line">
            <input v-model="cityInput" @keyup.enter="fetchWeather(cityInput)" placeholder="Search city (e.g. Paris)..." />
            <button @click="fetchWeather(cityInput)">→</button>
          </div>
        </div>

        <div class="fav-section">
          <p class="label">SAVED LOCATIONS</p>
          <div v-if="favorites.length" class="fav-grid">
            <div v-for="fav in favorites" :key="fav" class="fav-item" @click="fetchWeather(fav)">
              <span>{{ fav }}</span>
              <span class="arrow">↗</span>
            </div>
          </div>
          <p v-else class="empty">No saved cities yet.</p>
        </div>
      </div>

      <div v-if="viewState === 'CITY'" class="city-view fade-in">
        
        <div v-if="loading" class="loader">
           <div class="spinner"></div>
           <p>Analyzing atmosphere...</p>
        </div>

        <div v-else-if="weather" class="detail-grid">
          
          <section class="left-panel">
            <button class="back-btn" @click="goHome">← Back</button>
            
            <div class="weather-header">
              <h1 class="city-name">{{ weather.city }}</h1>
              <button @click="toggleFav" class="fav-btn" :class="{active: isFav()}">
                {{ isFav() ? '♥' : '♡' }}
              </button>
            </div>

            <div class="temp-block">
              <span class="val">{{ Math.round(weather.currentTemp) }}</span>
              <span class="unit">°</span>
            </div>
            <div class="condition">{{ weather.currentCondition }}</div>

            <div class="metrics">
              <div class="m-item">
                <label>HUMIDITY</label>
                <span>{{ weather.humidity }}%</span>
              </div>
              <div class="m-item">
                <label>WIND</label>
                <span>{{ Math.round(weather.windSpeed) }} km/h</span>
              </div>
              <div class="m-item">
                <label>AQI</label>
                <span>{{ weather.aqi }}</span>
              </div>
            </div>
          </section>

          <section class="right-panel">
            <div class="panel-inner">
              <div class="brief-header">DAILY BRIEFING</div>
              
              <div v-if="aiLoading" class="skeleton">
                <div class="line l1"></div><div class="line l2"></div><div class="line l3"></div>
              </div>
              
              <div v-else>
                <p class="ai-summary">"{{ aiData?.summary }}"</p>
                
                <div class="advisories">
                  <div class="advice">
                    <span class="icon">🧥</span>
                    <div class="adv-text">
                      <strong>Outfit</strong>
                      <span>{{ aiData?.outfit }}</span>
                    </div>
                  </div>
                  <div class="advice">
                    <span class="icon">☂️</span>
                    <div class="adv-text">
                      <strong>Safety</strong>
                      <span>{{ aiData?.safety }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>

        </div>
      </div>

    </main>
  </div>
</template>

<style scoped>
/* RESET & TYPOGRAPHY */
.luxury-app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: #fdfdfd;
  color: #1a1a1a;
  font-family: 'Lato', sans-serif;
}
h1, h2, h3, .city-name, .temp-block { font-family: 'Playfair Display', serif; }

/* NAVBAR */
.navbar {
  display: flex; justify-content: space-between; align-items: center;
  padding: 25px 50px; border-bottom: 1px solid #f0f0f0;
}
.logo { font-size: 1.5rem; font-weight: 700; cursor: pointer; letter-spacing: -0.5px; }
.logo .dot { color: #d4a373; }

.profile-trigger { position: relative; cursor: pointer; }
.initial { 
  width: 40px; height: 40px; background: #1a1a1a; color: white; border-radius: 50%; 
  display: flex; align-items: center; justify-content: center; font-family: 'Playfair Display';
}
.dropdown {
  position: absolute; right: 0; top: 50px; background: white; border: 1px solid #eee;
  box-shadow: 0 10px 30px rgba(0,0,0,0.05); width: 200px; padding: 20px; z-index: 100;
}
.meta span { display: block; }
.u-name { font-weight: bold; margin-bottom: 5px; }
.u-email { font-size: 0.8rem; color: #888; margin-bottom: 15px; }
.dropdown button { background: none; border: none; color: #d32f2f; cursor: pointer; font-size: 0.9rem; padding: 0; }

/* MAIN BODY */
.main-body { flex: 1; display: flex; flex-direction: column; }

/* VIEW 1: HOME */
.home-view { 
  flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; 
  text-align: center; padding: 40px; 
}
.hero h1 { font-size: 3.5rem; color: #1a1a1a; margin-bottom: 40px; }
.search-line { 
  display: flex; width: 100%; max-width: 500px; border-bottom: 1px solid #ccc; padding-bottom: 10px; margin: 0 auto 80px auto;
}
.search-line input { flex: 1; border: none; font-size: 1.5rem; outline: none; font-family: 'Lato'; background: transparent; }
.search-line button { background: none; border: none; font-size: 2rem; cursor: pointer; color: #1a1a1a; }

.label { font-size: 0.75rem; letter-spacing: 2px; color: #aaa; margin-bottom: 20px; }
.fav-grid { display: flex; gap: 20px; flex-wrap: wrap; justify-content: center; }
.fav-item { 
  padding: 15px 30px; border: 1px solid #eee; cursor: pointer; transition: all 0.3s; display: flex; align-items: center; gap: 10px;
}
.fav-item:hover { border-color: #1a1a1a; transform: translateY(-2px); }

/* VIEW 2: CITY (The Layout Engine) */
.city-view { flex: 1; display: flex; flex-direction: column; }
.loader { text-align: center; margin-top: 100px; color: #aaa; }
.spinner { width: 40px; height: 40px; border: 2px solid #eee; border-top-color: #1a1a1a; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 20px; }

.detail-grid { display: grid; grid-template-columns: 1fr 1fr; flex: 1; height: 100%; min-height: 80vh; }
@media(max-width: 900px) { .detail-grid { grid-template-columns: 1fr; } }

/* Left Panel */
.left-panel { padding: 60px; display: flex; flex-direction: column; justify-content: center; }
.back-btn { background: none; border: none; color: #aaa; cursor: pointer; margin-bottom: 40px; align-self: flex-start; }
.weather-header { display: flex; align-items: center; gap: 20px; margin-bottom: 20px; }
.city-name { font-size: 3rem; margin: 0; line-height: 1; }
.fav-btn { font-size: 2rem; background: none; border: none; cursor: pointer; color: #eee; transition: color 0.2s; }
.fav-btn.active { color: #d32f2f; }

.temp-block { font-size: 8rem; line-height: 1; color: #1a1a1a; }
.unit { font-size: 3rem; vertical-align: top; }
.condition { font-size: 1.5rem; color: #666; margin-bottom: 60px; font-style: italic; }

.metrics { display: flex; gap: 50px; border-top: 1px solid #eee; padding-top: 30px; }
.m-item { display: flex; flex-direction: column; }
.m-item label { font-size: 0.7rem; letter-spacing: 1px; color: #aaa; margin-bottom: 5px; }
.m-item span { font-weight: bold; font-size: 1.1rem; }

/* Right Panel (Editorial) */
.right-panel { 
  background: #f9f9f9; padding: 60px; 
  display: flex; flex-direction: column; justify-content: center; 
}
.panel-inner { max-width: 500px; margin: 0 auto; }
.brief-header { 
  font-size: 0.8rem; letter-spacing: 2px; color: #d4a373; border-bottom: 2px solid #d4a373; 
  display: inline-block; padding-bottom: 5px; margin-bottom: 40px; 
}
.ai-summary { font-family: 'Playfair Display'; font-size: 1.6rem; line-height: 1.5; color: #333; margin-bottom: 50px; }

.advisories { display: flex; flex-direction: column; gap: 30px; }
.advice { display: flex; gap: 20px; align-items: flex-start; }
.advice .icon { font-size: 1.8rem; }
.adv-text { display: flex; flex-direction: column; }
.adv-text strong { font-size: 0.75rem; letter-spacing: 1px; color: #aaa; text-transform: uppercase; margin-bottom: 5px; }
.adv-text span { font-size: 1.1rem; color: #1a1a1a; }

/* Animations */
.fade-in { animation: fadeIn 0.8s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes spin { to { transform: rotate(360deg); } }
.skeleton .line { height: 12px; background: #e0e0e0; margin-bottom: 15px; animation: pulse 1.5s infinite; }
.skeleton .l2 { width: 80%; } .skeleton .l3 { width: 60%; }
@keyframes pulse { 0% { opacity: 0.5; } 50% { opacity: 1; } 100% { opacity: 0.5; } }
</style>