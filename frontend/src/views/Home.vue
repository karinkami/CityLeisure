<template>
  <div class="home">
    <div class="container home-frame">
      <section class="mast">
        <div class="mast__decor" aria-hidden="true" />
        <div class="mast__orbs" aria-hidden="true">
          <span class="mast__orb mast__orb--a"></span>
          <span class="mast__orb mast__orb--b"></span>
          <span class="mast__orb mast__orb--c"></span>
        </div>
        <div class="mast__inner">
          <div class="mast__copy mast__copy--full">
            <span class="mast__eyebrow ds-reveal">Афиша · билеты · подбор</span>
            <h1 class="mast__title ds-reveal ds-reveal-delay-1">Городские события без суеты</h1>
            <p class="mast__lead ds-reveal ds-reveal-delay-2">
              Выбирайте по категориям, местам, смотрите постеры и оформляйте билеты — всё в одном спокойном интерфейсе.
            </p>
            <div class="mast__actions ds-reveal ds-reveal-delay-3">
              <router-link to="/events" class="ds-btn ds-btn--primary">Смотреть афишу</router-link>
              <router-link to="/recommendations" class="mast__link">Подбор мероприятий →</router-link>
            </div>
            <div class="mast__stats ds-reveal ds-reveal-delay-3">
              <div class="mast__stat">
                <strong>Smart</strong>
                <span>подбор</span>
              </div>
              <div class="mast__stat">
                <strong>Fast</strong>
                <span>оформление</span>
              </div>
              <div class="mast__stat">
                <strong>Clean UI</strong>
                <span>без перегруза</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="ds-panel ds-reveal ds-reveal-delay-2">
        <header class="panel__head">
          <h2 class="panel__title">Куда сходить</h2>
          <p class="panel__hint">Быстрый фильтр по настроению</p>
        </header>
        <div class="cats">
          <router-link
            v-for="cat in categoryLinks"
            :key="cat.key || cat.label"
            :to="cat.to || '/events'"
            class="cats__item ds-card-pro"
            :class="'cats__item--' + cat.tone"
          >
            <span class="cats__icon" aria-hidden="true">{{ cat.icon }}</span>
            <span class="cats__label">{{ cat.label }}</span>
          </router-link>
        </div>
      </section>

      <section class="ds-panel ds-reveal ds-reveal-delay-3">
        <header class="panel__head panel__head--row">
          <div>
            <h2 class="panel__title">Популярное</h2>
            <p class="panel__hint">То, что сейчас смотрят чаще всего</p>
          </div>
          <router-link to="/events" class="panel__more">Вся афиша →</router-link>
        </header>

        <div v-if="popularEvents.length" class="tiles">
          <div
            v-for="eventItem in popularEvents"
            :key="eventItem.id"
            class="tile ds-card-pro"
          >
            <router-link :to="`/events/${eventItem.id}`" class="tile__main">
              <div class="tile__media ds-media-zoom">
                <img v-if="getEventImage(eventItem)" :src="getEventImage(eventItem)" :alt="eventItem.title" />
                <div v-else class="tile__ph">{{ eventItem.title?.charAt(0) || '·' }}</div>
              </div>
              <div class="tile__body">
                <time class="tile__date">{{ formatDate(eventItem.eventDate) }}</time>
                <h3 class="tile__name">{{ eventItem.title }}</h3>
                <p class="tile__place">{{ eventItem.venue?.name || 'Площадка уточняется' }}</p>
              </div>
            </router-link>
          </div>
        </div>
        <div v-else class="empty-block">
          <p>Пока нет данных для блока «Популярное». Загляните в афишу — там уже есть события.</p>
          <router-link to="/events" class="ds-btn ds-btn--primary">Открыть афишу</router-link>
        </div>
      </section>

      <section v-if="isAuthenticated && hasUserActivity && recommendedEvents.length" class="ds-panel ds-panel--soft ds-reveal ds-reveal-delay-4">
        <header class="panel__head panel__head--row">
          <div>
            <h2 class="panel__title">Для вас</h2>
            <p class="panel__hint">На основе избранного и покупок</p>
          </div>
          <router-link to="/recommendations" class="panel__more">Настроить подбор →</router-link>
        </header>
        <div class="tiles">
          <div
            v-for="eventItem in recommendedEvents"
            :key="`rec-${eventItem.id}`"
            class="tile ds-card-pro"
          >
            <router-link :to="`/events/${eventItem.id}`" class="tile__main">
              <div class="tile__media ds-media-zoom">
                <img v-if="getEventImage(eventItem)" :src="getEventImage(eventItem)" :alt="eventItem.title" />
                <div v-else class="tile__ph">{{ eventItem.title?.charAt(0) || '·' }}</div>
              </div>
              <div class="tile__body">
                <time class="tile__date">{{ formatDate(eventItem.eventDate) }}</time>
                <h3 class="tile__name">{{ eventItem.title }}</h3>
                <p class="tile__place">{{ eventItem.venue?.name || 'Площадка уточняется' }}</p>
              </div>
            </router-link>
          </div>
        </div>
      </section>

      <section v-else-if="isAuthenticated && !hasUserActivity" class="ds-panel ds-panel--soft ds-reveal ds-reveal-delay-4">
        <header class="panel__head panel__head--row">
          <div>
            <h2 class="panel__title">Для вас</h2>
            <p class="panel__hint">Персональные рекомендации появятся после первых действий</p>
          </div>
        </header>
        <div class="empty-block">
          <p>
            Пока у нас нет данных о ваших предпочтениях.
            Пройдите быстрый подбор, чтобы сразу получить подходящие мероприятия.
          </p>
          <router-link to="/recommendations" class="ds-btn ds-btn--primary">Подобрать мероприятие</router-link>
        </div>
      </section>
    </div>
  </div>
</template>

<script>
import { isAuthenticated as hasSession } from '@/services/session'
import { getCartItems } from '@/services/cartService'
import { getEventCategories, getRecommendedEvents, getTopPopularEvents } from '@/services/eventsService'
import { getFavoriteEventIds } from '@/services/favoritesService'
import { getUserOrders } from '@/services/ordersService'
import { resolveEventImageUrl } from '@/utils/imageResolver'
const CATEGORY_QUICK = [
  { type: 'free', label: 'Бесплатно', icon: '◇', tone: 'free', to: { path: '/events', query: { categoryId: 'free' } } },
  { type: 'cat', match: 'Кино', icon: '▶', tone: 'film' },
  { type: 'cat', match: 'Концерты', icon: '♪', tone: 'music' },
  { type: 'cat', match: 'Театр', icon: '⌘', tone: 'stage' },
  { type: 'cat', match: 'Выставки', icon: '◆', tone: 'art' },
  { type: 'cat', match: 'Парки и набережные', icon: '⌂', tone: 'park' },
  { type: 'cat', match: 'Мастер-классы и хобби', icon: '✎', tone: 'workshop' },
  { type: 'cat', match: 'Уличные фестивали и маркеты', icon: '✦', tone: 'streetfair' }
]

export default {
  name: 'Home',
  data() {
    return {
      topPopular: [],
      recommendedEvents: [],
      categoryLinks: [],
      hasUserActivity: false
    }
  },
  computed: {
    isAuthenticated() {
      return hasSession()
    },
    popularEvents() {
      return this.topPopular.slice(0, 4)
    }
  },
  async mounted() {
    try {
      const [popularEvents, categories] = await Promise.all([
        getTopPopularEvents(),
        getEventCategories().catch(() => [])
      ])
      this.topPopular = popularEvents
      this.categoryLinks = this.buildCategoryLinks(categories || [])

      if (this.isAuthenticated) {
        this.hasUserActivity = await this.detectUserActivity()
        if (this.hasUserActivity) {
          this.recommendedEvents = await getRecommendedEvents(4)
        } else {
          this.recommendedEvents = []
        }
      } else {
        this.hasUserActivity = false
        this.recommendedEvents = []
      }
    } catch {
      this.topPopular = []
      this.recommendedEvents = []
      this.hasUserActivity = false
      this.categoryLinks = this.buildCategoryLinks([])
    }
  },
  methods: {
    async detectUserActivity() {
      const [favoritesRes, cartRes, ordersRes] = await Promise.allSettled([
        getFavoriteEventIds(),
        getCartItems(),
        getUserOrders()
      ])

      const favorites = favoritesRes.status === 'fulfilled' && Array.isArray(favoritesRes.value)
        ? favoritesRes.value
        : []
      const cartItems = cartRes.status === 'fulfilled' && Array.isArray(cartRes.value)
        ? cartRes.value
        : []
      const orders = ordersRes.status === 'fulfilled' && Array.isArray(ordersRes.value)
        ? ordersRes.value
        : []

      return favorites.length > 0 || cartItems.length > 0 || orders.length > 0
    },
    buildCategoryLinks(apiCategories) {
      const byName = new Map((apiCategories || []).map((c) => [c.name, c]))
      return CATEGORY_QUICK.map((row) => {
        if (row.type === 'free') return { ...row, key: 'free' }
        const cat = byName.get(row.match)
        const to = cat
          ? { path: '/events', query: { categoryId: String(cat.id) } }
          : { path: '/events' }
        return {
          key: row.match,
          label: cat?.name ?? row.match,
          icon: row.icon,
          tone: row.tone,
          to
        }
      })
    },
    getEventImage(eventItem) {
      return resolveEventImageUrl(eventItem?.imageUrl, eventItem?.id)
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
    }
  }
}
</script>

<style scoped>
.home {
  padding-bottom: var(--space-6);
  background:
    radial-gradient(900px 420px at 8% -8%, rgba(15, 118, 110, 0.08), transparent 55%),
    radial-gradient(820px 360px at 96% 6%, rgba(245, 158, 11, 0.08), transparent 52%),
    var(--bg);
}

.home-frame {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.mast {
  position: relative;
  overflow: hidden;
  border-radius: 20px;
  border: 1px solid var(--border);
  background: linear-gradient(135deg, #ffffff 0%, #f5fbff 40%, #f5f7ff 100%);
  box-shadow:
    0 1px 2px rgba(15, 23, 42, 0.04),
    0 24px 48px -24px rgba(15, 23, 42, 0.12);
  isolation: isolate;
  animation: section-rise 480ms ease both;
}

.mast::before {
  content: '';
  position: absolute;
  width: 360px;
  height: 360px;
  right: -120px;
  top: -180px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(15, 118, 110, 0.2), rgba(15, 118, 110, 0));
  filter: blur(2px);
  pointer-events: none;
  z-index: 0;
}

.mast__decor {
  position: absolute;
  inset: 0;
  background:
    repeating-linear-gradient(
      -12deg,
      transparent,
      transparent 14px,
      rgba(15, 118, 110, 0.03) 14px,
      rgba(15, 118, 110, 0.03) 15px
    );
  pointer-events: none;
  opacity: 0.9;
  animation: shimmer 16s linear infinite;
}
.mast__orbs {
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}
.mast__orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(1px);
  opacity: 0.7;
}
.mast__orb--a {
  width: 180px;
  height: 180px;
  right: 8%;
  top: 8%;
  background: radial-gradient(circle, rgba(14, 165, 233, 0.25), transparent 70%);
  animation: floaty 8s ease-in-out infinite;
}
.mast__orb--b {
  width: 220px;
  height: 220px;
  right: -40px;
  bottom: -70px;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.24), transparent 70%);
  animation: floaty 10s ease-in-out infinite reverse;
}
.mast__orb--c {
  width: 140px;
  height: 140px;
  left: -30px;
  bottom: -30px;
  background: radial-gradient(circle, rgba(16, 185, 129, 0.22), transparent 70%);
  animation: floaty 9s ease-in-out infinite;
}

.mast__inner {
  position: relative;
  padding: clamp(var(--space-5), 5vw, var(--space-6));
  z-index: 1;
}

.mast__copy {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  justify-content: center;
  width: 100%;
  max-width: none;
}

.mast__copy--full {
  align-items: stretch;
  text-align: left;
}

.mast__eyebrow {
  align-self: flex-start;
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: var(--primary);
  padding: var(--space-1) var(--space-3);
  border-radius: 999px;
  background: rgba(15, 118, 110, 0.08);
  border: 1px solid rgba(15, 118, 110, 0.15);
}

.mast__title {
  margin: 0;
  width: 100%;
  font-size: clamp(2rem, 5.5vw, 3.15rem);
  font-weight: 800;
  line-height: 1.04;
  letter-spacing: -0.035em;
  color: #0f172a;
  text-wrap: balance;
  background: linear-gradient(120deg, #0f172a 0%, #0f766e 42%, #2563eb 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.mast__lead {
  margin: 0;
  width: 100%;
  max-width: none;
  font-size: clamp(1.05rem, 2.1vw, 1.25rem);
  color: var(--text-secondary);
  line-height: 1.65;
  text-wrap: pretty;
}

.mast__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-4);
  margin-top: var(--space-2);
}
.mast__stats {
  margin-top: var(--space-2);
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}
.mast__stat {
  display: inline-flex;
  align-items: baseline;
  gap: 0.35rem;
  padding: 0.45rem 0.7rem;
  border-radius: 999px;
  border: 1px solid rgba(15, 23, 42, 0.08);
  background: rgba(255, 255, 255, 0.76);
  backdrop-filter: blur(8px);
  box-shadow: 0 10px 20px -16px rgba(15, 23, 42, 0.45);
}
.mast__stat strong {
  font-size: 0.74rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #0f766e;
}
.mast__stat span {
  font-size: 0.78rem;
  color: var(--text-secondary);
}

.mast__link {
  font-weight: 700;
  font-size: 0.9375rem;
  color: var(--primary);
  text-decoration: none;
  border-bottom: 2px solid rgba(15, 118, 110, 0.25);
  padding-bottom: 2px;
  transition: border-color 0.2s ease, color 0.2s ease, transform 0.2s ease;
}

.mast__link:hover {
  border-bottom-color: var(--primary);
  color: var(--primary-hover);
  transform: translateX(2px);
}

.panel__head {
  margin-bottom: var(--space-4);
}

.panel__head--row {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-3);
}

.panel__title {
  margin: 0 0 var(--space-1);
  font-size: 1.35rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.panel__hint {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.panel__more {
  font-weight: 700;
  font-size: 0.875rem;
  color: var(--primary);
  text-decoration: none;
  white-space: nowrap;
  transition: color 0.2s ease, transform 0.2s ease;
}

.panel__more:hover {
  text-decoration: underline;
  transform: translateX(2px);
}

.cats {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-3);
}

.cats__item {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-3);
  padding: var(--space-4);
  min-height: 108px;
  text-decoration: none;
  color: var(--text-primary);
  font-weight: 700;
  overflow: hidden;
  transition: transform 0.24s ease, box-shadow 0.24s ease, border-color 0.24s ease, filter 0.24s ease;
  isolation: isolate;
}
.cats__item > * { position: relative; z-index: 1; }

.cats__item::after {
  content: '';
  position: absolute;
  right: -20%;
  bottom: -30%;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  opacity: 0.35;
  pointer-events: none;
  transition: transform 0.3s ease;
}

.cats__icon {
  font-size: 1.35rem;
  opacity: 0.85;
  transition: transform 0.24s ease;
}

.cats__label {
  font-size: 0.9375rem;
  line-height: 1.3;
  hyphens: auto;
}

.cats__item:hover {
  transform: perspective(800px) translateY(-3px) rotateX(1deg) rotateY(-1deg);
  box-shadow: 0 16px 30px -22px rgba(15, 23, 42, 0.28);
  filter: saturate(1.06);
}

.cats__item:hover::after {
  transform: scale(1.12);
}

.cats__item:hover .cats__icon {
  transform: scale(1.08) rotate(-4deg);
}

.cats__item--film {
  background: linear-gradient(145deg, #fff7ed 0%, #fff 55%);
}
.cats__item--film::after {
  background: rgba(245, 158, 11, 0.2);
}

.cats__item--music {
  background: linear-gradient(145deg, #f0fdfa 0%, #fff 55%);
}
.cats__item--music::after {
  background: rgba(15, 118, 110, 0.15);
}

.cats__item--stage {
  background: linear-gradient(145deg, #fef2f2 0%, #fff 55%);
}
.cats__item--stage::after {
  background: rgba(185, 28, 28, 0.08);
}

.cats__item--art {
  background: linear-gradient(145deg, #f8fafc 0%, #fff 55%);
}
.cats__item--art::after {
  background: rgba(71, 85, 105, 0.12);
}

.cats__item--free {
  background: linear-gradient(145deg, #ecfdf5 0%, #fff 55%);
}
.cats__item--free::after {
  background: rgba(16, 185, 129, 0.18);
}

.cats__item--park {
  background: linear-gradient(145deg, #ecfdf5 0%, #fff 55%);
}
.cats__item--park::after {
  background: rgba(5, 150, 105, 0.14);
}

.cats__item--workshop {
  background: linear-gradient(145deg, #fdf4ff 0%, #fff 55%);
}
.cats__item--workshop::after {
  background: rgba(168, 85, 247, 0.12);
}

.cats__item--streetfair {
  background: linear-gradient(145deg, #fff7ed 0%, #fff 55%);
}
.cats__item--streetfair::after {
  background: rgba(249, 115, 22, 0.14);
}

.tiles {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-4);
}

.tile {
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-height: 300px;
  transition: transform 0.24s ease, box-shadow 0.24s ease, border-color 0.24s ease;
  animation: tile-appear 340ms ease both;
  border: 1px solid rgba(15, 23, 42, 0.06);
  position: relative;
  isolation: isolate;
}
.tile__main { position: relative; z-index: 1; }

.tile:hover {
  transform: perspective(900px) translateY(-3px) rotateX(1deg) rotateY(-1deg);
  box-shadow: 0 20px 34px -26px rgba(2, 6, 23, 0.34);
}

.home :deep(.ds-btn) {
  position: relative;
  overflow: hidden;
}

.home :deep(.ds-btn)::after {
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

.home :deep(.ds-btn):active::after {
  animation: btn-ripple 420ms ease-out;
}

.tile__main {
  text-decoration: none;
  color: inherit;
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.tile__media {
  position: relative;
  height: 152px;
  background: linear-gradient(145deg, #e5e7eb, #f3f4f6);
  overflow: hidden;
}

.tile__media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.35s ease;
}

.tile:hover .tile__media img {
  transform: scale(1.05);
}

.tile__ph {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  font-weight: 800;
  color: var(--primary);
  background: var(--primary-muted);
}

.tile__body {
  padding: var(--space-4);
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  flex: 1;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), #fff);
}

.tile__date {
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--accent-warm);
}

.tile__name {
  margin: 0;
  font-size: 1rem;
  font-weight: 800;
  line-height: 1.35;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.tile__place {
  margin: auto 0 0;
  font-size: 0.8125rem;
  color: var(--text-secondary);
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.empty-block {
  text-align: center;
  padding: var(--space-6) var(--space-4);
  border-radius: var(--radius-md);
  border: 1px dashed var(--border);
  background: var(--surface-muted);
  animation: section-rise 480ms ease both;
}

.empty-block p {
  margin: 0 auto var(--space-4);
  color: var(--text-secondary);
  max-width: 42ch;
  line-height: 1.55;
}

@media (max-width: 1024px) {
  .cats,
  .tiles {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 560px) {
  .cats,
  .tiles {
    grid-template-columns: 1fr;
  }

  .mast__actions {
    flex-direction: column;
    align-items: flex-start;
  }
}

@keyframes shimmer {
  0% {
    transform: translateX(0);
  }
  50% {
    transform: translateX(18px);
  }
  100% {
    transform: translateX(0);
  }
}

@keyframes section-rise {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes tile-appear {
  from {
    opacity: 0;
    transform: translateY(12px) scale(0.985);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
@keyframes btn-ripple {
  0% {
    transform: translate(-50%, -50%) scale(0);
    opacity: 0.42;
  }
  100% {
    transform: translate(-50%, -50%) scale(18);
    opacity: 0;
  }
}
@keyframes floaty {
  0% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
  100% {
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .mast,
  .tile,
  .empty-block {
    animation: none;
  }
  .mast__decor {
    animation: none;
  }
  .mast__orb {
    animation: none;
  }
  .mast__link,
  .panel__more,
  .cats__item,
  .cats__item::after,
  .cats__icon,
  .tile,
  .home :deep(.ds-btn)::after,
  .tile__media img {
    transition: none;
  }
}
</style>
