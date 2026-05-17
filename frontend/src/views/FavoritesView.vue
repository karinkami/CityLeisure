<template>
  <div class="favorites-page">
    <div class="container">
      <header class="head">
        <h1 class="page-title ds-reveal">Избранное</h1>
        <router-link to="/profile" class="back ds-reveal">← В личный кабинет</router-link>
      </header>

      <div v-if="!isAuthenticated" class="state">Войдите, чтобы видеть избранное.</div>
      <div v-else-if="loading" class="state">Загрузка…</div>
      <div v-else-if="!items.length" class="state muted">Вы ещё не добавили мероприятия. Загляните в <router-link to="/events">афишу</router-link>.</div>
      <div v-else class="grid">
        <article
          v-for="ev in items"
          :key="ev.id"
          class="card ds-card-pro ds-reveal"
        >
          <div class="poster">
            <img v-if="!imgErr.has(ev.id)" :src="getEventImage(ev)" :alt="ev.title" @error="imgErr.add(ev.id)" />
            <div v-else class="ph">{{ ev.title?.charAt(0) }}</div>
          </div>
          <div class="body">
            <h2>{{ ev.title }}</h2>
            <p class="meta">{{ formatDate(ev.eventDate) }} · {{ formatTime(ev.eventTime) }}</p>
            <p class="meta">{{ ev.venue?.name }}</p>
            <div class="row">
              <router-link :to="`/events/${ev.id}`" class="ds-btn ds-btn--secondary">Открыть</router-link>
              <button type="button" class="ds-btn ds-btn--ghost" @click="remove(ev.id)">Убрать</button>
            </div>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script>
import { isAuthenticated as hasSession } from '@/services/session'
import { getFavoriteEventDetails, removeFavoriteEvent } from '@/services/favoritesService'
import { resolveEventImageUrl } from '@/utils/imageResolver'
export default {
  name: 'FavoritesView',
  data() {
    return { items: [], loading: true, imgErr: new Set() }
  },
  computed: {
    isAuthenticated() {
      return hasSession()
    }
  },
  mounted() {
    if (!this.isAuthenticated) {
      this.loading = false
      return
    }
    this.load()
  },
  methods: {
    async load() {
      this.loading = true
      try {
        this.items = await getFavoriteEventDetails()
      } catch {
        this.items = []
        this.$root.$toast?.error('Не удалось загрузить избранное.')
      } finally {
        this.loading = false
      }
    },
    getEventImage(ev) {
      return resolveEventImageUrl(ev?.imageUrl, ev?.id)
    },
    formatDate(d) {
      return new Date(d).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })
    },
    formatTime(t) {
      return (t || '').slice(0, 5)
    },
    async remove(id) {
      try {
        await removeFavoriteEvent(id)
        this.items = this.items.filter((e) => e.id !== id)
        this.$root.$toast?.info('Удалено из избранного.')
      } catch {
        this.$root.$toast?.error('Не удалось удалить.')
      }
    }
  }
}
</script>

<style scoped>
.favorites-page {
  padding-bottom: var(--space-6);
  min-height: calc(100vh - 140px);
  background:
    radial-gradient(760px 340px at 6% -8%, rgba(245, 158, 11, 0.08), transparent 56%),
    radial-gradient(700px 320px at 94% 4%, rgba(14, 165, 233, 0.08), transparent 54%),
    var(--bg);
}

.head {
  margin-bottom: var(--space-5);
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-3);
  flex-wrap: wrap;
}

.page-title {
  margin: 0;
  background: linear-gradient(120deg, #0f172a 0%, #b45309 48%, #2563eb 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.back {
  display: inline-block;
  font-weight: 600;
  color: var(--primary);
  text-decoration: none;
  transition: transform 0.2s ease, color 0.2s ease;
}

.back:hover {
  text-decoration: underline;
  transform: translateX(2px);
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-4);
}

.card {
  overflow: hidden;
  display: flex;
  flex-direction: column;
  border: 1px solid rgba(15, 23, 42, 0.08);
  transition: transform 0.22s ease, box-shadow 0.22s ease, border-color 0.22s ease;
  animation: fav-rise 360ms ease both;
  position: relative;
  isolation: isolate;
}

.card:hover {
  transform: perspective(900px) translateY(-3px) rotateX(1deg) rotateY(-1deg);
  box-shadow: 0 20px 34px -26px rgba(2, 6, 23, 0.34);
  border-color: rgba(14, 165, 233, 0.25);
}

.poster {
  height: 140px;
  background: var(--surface-muted);
  overflow: hidden;
}

.poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.card:hover .poster img {
  transform: scale(1.05);
}

.ph {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-secondary);
}

.body {
  padding: var(--space-4);
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  flex: 1;
  position: relative;
  z-index: 1;
}

.body h2 {
  margin: 0;
  font-size: 1.05rem;
  line-height: 1.35;
}

.meta {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.row {
  margin-top: auto;
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
  padding-top: var(--space-3);
}

.state {
  text-align: center;
  padding: var(--space-6);
  color: var(--text-secondary);
  animation: fav-rise 360ms ease both;
}

.state.muted a {
  color: var(--primary);
  font-weight: 600;
}

.favorites-page :deep(.ds-btn) {
  position: relative;
  overflow: hidden;
}

.favorites-page :deep(.ds-btn)::after {
  content: '';
  position: absolute;
  left: 50%;
  top: 50%;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  transform: translate(-50%, -50%) scale(0);
  background: rgba(255, 255, 255, 0.32);
  opacity: 0;
  pointer-events: none;
}

.favorites-page :deep(.ds-btn):active::after {
  animation: fav-ripple 420ms ease-out;
}

@keyframes fav-rise {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
@keyframes fav-ripple {
  0% {
    transform: translate(-50%, -50%) scale(0);
    opacity: 0.42;
  }
  100% {
    transform: translate(-50%, -50%) scale(18);
    opacity: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .card,
  .state {
    animation: none;
  }
  .back,
  .card,
  .favorites-page :deep(.ds-btn)::after,
  .poster img {
    transition: none;
  }
}
</style>
