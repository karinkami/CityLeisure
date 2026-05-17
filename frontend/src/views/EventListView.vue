<template>
  <div class="catalog">
    <div class="container">
      <header class="topbar ds-reveal">
        <div>
          <h1>Афиша</h1>
        </div>
        <div class="view-meta">
          <template v-if="loadFailed">—</template>
          <template v-else>{{ events.length }} {{ eventsLabel }}</template>
        </div>
      </header>

      <div class="afisha-panel ds-inset ds-reveal" role="search" aria-label="Фильтры афиши">
        <input
          v-model.trim="search"
          class="afisha-input afisha-input--search"
          type="search"
          placeholder="Поиск по названию"
          aria-label="Поиск по названию"
        />

        <div class="afisha-filters-grid">
          <div class="afisha-field">
            <label class="afisha-field__label" for="afisha-category">Категория</label>
              <select
                id="afisha-category"
                v-model="selectedCategory"
                class="afisha-input afisha-input--block"
                aria-label="Категория"
                @change="onFilterChange"
              >
              <option value="">Все категории</option>
              <option value="free">Бесплатные</option>
              <option v-for="cat in categories" :key="cat.id" :value="String(cat.id)">{{ cat.name }}</option>
            </select>
          </div>

          <div class="afisha-field">
            <label class="afisha-field__label" for="afisha-venue">Площадка</label>
            <select
              id="afisha-venue"
              v-model="selectedVenueId"
              class="afisha-input afisha-input--block"
              aria-label="Площадка"
              @change="onFilterChange"
            >
              <option value="">Все площадки</option>
              <option v-for="v in venues" :key="v.id" :value="String(v.id)">{{ v.name }}</option>
            </select>
          </div>

          <div class="afisha-field afisha-field--span">
            <span class="afisha-field__label">Цена</span>
            <div class="afisha-price-block">
              <div class="afisha-price-range">
                <input
                  v-model.number="minPrice"
                  class="afisha-input afisha-input--num"
                  type="number"
                  min="0"
                  step="1"
                  placeholder="от"
                  aria-label="Цена от"
                  :disabled="priceInputsDisabled"
                  @change="onFilterChange"
                />
                <span class="afisha-price-range__sep" aria-hidden="true">—</span>
                <input
                  v-model.number="maxPrice"
                  class="afisha-input afisha-input--num"
                  type="number"
                  min="0"
                  step="1"
                  placeholder="до"
                  aria-label="Цена до"
                  :disabled="priceInputsDisabled"
                  @change="onFilterChange"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="afisha-toolbar">
          <div class="afisha-segment" role="tablist" aria-label="Подборки">
            <button
              v-for="tab in availableSectionTabs"
              :key="tab.key"
              type="button"
              class="afisha-segment__btn"
              :class="{ 'afisha-segment__btn--active': isTabActive(tab) }"
              role="tab"
              :aria-selected="isTabActive(tab)"
              @click="selectSection(tab.key)"
            >
              {{ tab.label }}
            </button>
          </div>
          <button type="button" class="ds-btn ds-btn--secondary afisha-toolbar__reset" @click="resetFilters">Сбросить фильтры</button>
        </div>
      </div>

      <div v-if="loading" class="skeleton-grid" aria-label="Загрузка мероприятий">
        <div v-for="n in 6" :key="'catalog-skeleton-' + n" class="skeleton-card">
          <div class="skeleton-block skeleton-block--media"></div>
          <div class="skeleton-card__body">
            <div class="skeleton-block skeleton-block--line skeleton-block--w70"></div>
            <div class="skeleton-block skeleton-block--line skeleton-block--w90"></div>
            <div class="skeleton-block skeleton-block--line skeleton-block--w80"></div>
            <div class="skeleton-block skeleton-block--line skeleton-block--w45"></div>
          </div>
        </div>
      </div>
      <div v-else-if="loadFailed" class="state state--error">
        <p class="state-title">Не удалось загрузить афишу</p>
        <p class="state-text">Проверьте соединение и нажмите «Повторить».</p>
        <button type="button" class="ds-btn ds-btn--secondary" @click="loadEvents">Повторить</button>
      </div>
      <div v-else-if="events.length === 0" class="empty">
        <p class="empty-title">Подходящих мероприятий нет</p>
        <p class="empty-text">Измените фильтры или сбросьте их.</p>
        <button type="button" class="ds-btn ds-btn--primary" @click="resetFilters">Сбросить фильтры</button>
      </div>

      <transition-group v-else name="catalog-list" tag="div" class="grid ds-reveal ds-reveal-delay-2">
        <article
          v-for="eventItem in events"
          :key="eventItem.id"
          class="card ds-card-pro"
        >
          <div class="poster ds-media-zoom">
            <img v-if="!imageErrors.has(eventItem.id)" :src="getEventImage(eventItem)" :alt="eventItem.title" @error="imageErrors.add(eventItem.id)" />
            <div v-else class="poster-fallback">{{ eventItem.title.charAt(0) }}</div>
            <button
              v-if="isAuthenticated"
              class="favorite-btn"
              :class="{ active: favorites.has(eventItem.id) }"
              @click.prevent="toggleFavorite(eventItem.id)"
              title="В избранное"
            >
              {{ favorites.has(eventItem.id) ? '★' : '☆' }}
            </button>
          </div>
          <div class="content">
            <div class="chips">
              <span class="chip chip--warm">{{ eventItem.category?.name }}</span>
              <span class="chip chip--muted">{{ eventItem.ageRating }}</span>
              <span v-if="eventUsesSeatSelection(eventItem)" class="chip chip--seat">Места</span>
            </div>
            <EventStatusChips class="chips-row" :event-item="eventItem" />
            <h3>{{ eventItem.title }}</h3>
            <p class="description">{{ eventItem.description }}</p>
            <p class="meta">{{ formatDate(eventItem.eventDate) }} • {{ formatTime(eventItem.eventTime) }}</p>
            <p v-if="eventItem.venue?.name" class="meta meta--venue">{{ eventItem.venue.name }}</p>
            <p class="price">{{ priceLabel(eventItem) }}</p>
          </div>
          <footer class="card-footer">
            <router-link :to="`/events/${eventItem.id}`" class="ds-btn ds-btn--secondary ds-btn--block">Подробнее</router-link>
            <template v-if="isAuthenticated">
              <router-link
                v-if="eventUsesSeatSelection(eventItem)"
                :to="`/events/${eventItem.id}`"
                class="ds-btn ds-btn--primary ds-btn--block"
              >
                Выбор мест
              </router-link>
              <button
                v-else
                type="button"
                class="ds-btn ds-btn--primary ds-btn--block"
                :disabled="addingToCart === eventItem.id"
                @click="addToCart(eventItem.id)"
              >
                {{ addingToCart === eventItem.id ? 'Добавляем…' : Number(eventItem.price) === 0 ? 'Забронировать' : 'В корзину' }}
              </button>
            </template>
            <router-link v-else to="/auth" class="ds-btn ds-btn--primary ds-btn--block">Войти, чтобы купить</router-link>
          </footer>
        </article>
      </transition-group>
    </div>
  </div>
</template>

<script>
import { api } from '@/services/api'
import { isAuthenticated as hasSession } from '@/services/session'
import { addCartItem } from '@/services/cartService'
import { getEventCategories, getEvents, getVenues } from '@/services/eventsService'
import { addFavoriteEvent, getFavoriteEventIds, removeFavoriteEvent } from '@/services/favoritesService'
import { resolveEventImageUrl } from '@/utils/imageResolver'
import { formatEventPrice } from '@/utils/ticketFormat'
import { eventUsesSeatSelection } from '@/utils/seating'
import EventStatusChips from '@/components/EventStatusChips.vue'
export default {
  name: 'EventListView',
  components: { EventStatusChips },
  data() {
    return {
      events: [],
      categories: [],
      venues: [],
      search: '',
      selectedCategory: '',
      selectedVenueId: '',
      minPrice: null,
      maxPrice: null,
      activeSection: 'all',
      sectionTabs: [
        { key: 'all', label: 'Вся афиша' },
        { key: 'recommended', label: 'Рекомендуем' },
        { key: 'popular', label: 'Популярные' },
        { key: 'free', label: 'Бесплатные' }
      ],
      loadFailed: false,
      loading: true,
      imageErrors: new Set(),
      addingToCart: null,
      favorites: new Set(),
      searchDebounceTimer: null,
      skipNextQuerySync: false
    }
  },
  computed: {
    isAuthenticated() {
      return hasSession()
    },
    eventsLabel() {
      const n = this.events.length
      const mod10 = n % 10
      const mod100 = n % 100
      if (mod100 >= 11 && mod100 <= 14) return 'событий'
      if (mod10 === 1) return 'событие'
      if (mod10 >= 2 && mod10 <= 4) return 'события'
      return 'событий'
    },
    availableSectionTabs() {
      if (this.isAuthenticated) return this.sectionTabs
      return this.sectionTabs.filter((tab) => tab.key !== 'recommended')
    },
    priceInputsDisabled() {
      return this.selectedCategory === 'free' || this.activeSection === 'free'
    }
  },
  watch: {
    search() {
      clearTimeout(this.searchDebounceTimer)
      this.searchDebounceTimer = setTimeout(() => {
        this.loadEvents()
        this.syncQueryToRoute()
      }, 380)
    },
    '$route.query': {
      handler() {
        if (this.skipNextQuerySync) return
        this.applyQueryFromRoute()
        this.loadEvents()
      },
      deep: true
    }
  },
  async mounted() {
    this.applyQueryFromRoute()
    await Promise.all([this.loadEventCategories(), this.loadVenues(), this.loadFavorites(), this.loadEvents()])
  },
  methods: {
    eventUsesSeatSelection,
    onFilterChange() {
      if (this.selectedCategory === 'free') {
        this.activeSection = 'all'
        this.minPrice = null
        this.maxPrice = null
      } else if (this.selectedCategory && this.activeSection === 'free') {
        this.activeSection = 'all'
      }
      this.syncQueryToRoute()
      this.loadEvents()
    },
    queryOne(val) {
      if (val == null) return undefined
      if (Array.isArray(val)) return val.length ? String(val[0]) : undefined
      return String(val)
    },
    applyQueryFromRoute() {
      const q = this.$route.query || {}
      const allowed = new Set(['recommended', 'popular', 'free'])
      const rawSection = this.queryOne(q.section)
      if (rawSection && allowed.has(rawSection)) this.activeSection = rawSection
      else this.activeSection = 'all'
      if (!this.isAuthenticated && this.activeSection === 'recommended') {
        this.activeSection = 'all'
      }

      const rawQ = this.queryOne(q.q)
      this.search = rawQ != null ? rawQ : ''

      const rawCat = this.queryOne(q.categoryId)
      if (rawCat === 'free') this.selectedCategory = 'free'
      else this.selectedCategory = rawCat != null && rawCat !== '' ? String(rawCat) : ''

      const rawVenue = this.queryOne(q.venueId)
      this.selectedVenueId = rawVenue != null && rawVenue !== '' && !Number.isNaN(Number(rawVenue)) ? String(Number(rawVenue)) : ''
    },
    isTabActive(tab) {
      return this.activeSection === tab.key
    },
    syncQueryToRoute() {
      const query = {}
      if (this.activeSection && this.activeSection !== 'all') query.section = this.activeSection
      if (this.search) query.q = this.search
      if (this.selectedCategory) query.categoryId = this.selectedCategory === 'free' ? 'free' : this.selectedCategory
      if (this.selectedVenueId) query.venueId = this.selectedVenueId
      this.skipNextQuerySync = true
      const q = Object.keys(query).length ? query : {}
      this.$router
        .replace({ path: '/events', query: q })
        .catch(() => {})
        .finally(() => {
          this.$nextTick(() => {
            this.skipNextQuerySync = false
          })
        })
    },
    buildEventParams() {
      const params = {}
      if (this.search) params.q = this.search
      if (this.selectedCategory === 'free') params.freeOnly = true
      else if (this.selectedCategory) {
        const cid = Number(this.selectedCategory)
        if (!Number.isNaN(cid) && cid > 0) params.categoryId = cid
      }
      if (this.selectedVenueId) {
        const vid = Number(this.selectedVenueId)
        if (!Number.isNaN(vid) && vid > 0) params.venueId = vid
      }
      if (this.minPrice != null && this.minPrice !== '' && !Number.isNaN(Number(this.minPrice))) params.priceMin = Number(this.minPrice)
      if (this.maxPrice != null && this.maxPrice !== '' && !Number.isNaN(Number(this.maxPrice))) params.priceMax = Number(this.maxPrice)
      if (this.activeSection && this.activeSection !== 'all') params.section = this.activeSection
      if (!this.isAuthenticated && params.section === 'recommended') {
        delete params.section
      }
      return params
    },
    selectSection(key) {
      const k = key == null || key === '' ? 'all' : String(key)
      this.activeSection = k
      if (this.activeSection === 'free') {
        this.selectedCategory = ''
      }
      this.syncQueryToRoute()
      this.loadEvents()
    },
    async loadEvents() {
      this.loading = true
      this.loadFailed = false
      try {
        this.events = await getEvents(this.buildEventParams())
      } catch (error) {
        console.error('Ошибка загрузки мероприятий:', error)
        this.events = []
        this.loadFailed = true
        this.$root.$toast?.error('Не удалось загрузить афишу. Проверьте соединение с сервером.')
      } finally {
        this.loading = false
      }
    },
    resetFilters() {
      this.search = ''
      this.selectedCategory = ''
      this.selectedVenueId = ''
      this.minPrice = null
      this.maxPrice = null
      this.activeSection = 'all'
      this.syncQueryToRoute()
      this.loadEvents()
    },
    async loadEventCategories() {
      this.categories = await getEventCategories()
    },
    async loadVenues() {
      try {
        this.venues = await getVenues()
      } catch {
        this.venues = []
      }
    },
    async loadFavorites() {
      if (!this.isAuthenticated) return
      try {
        this.favorites = new Set(await getFavoriteEventIds())
      } catch {
        this.favorites = new Set()
      }
    },
    formatPrice(price) {
      return new Intl.NumberFormat('ru-RU').format(price)
    },
    priceLabel(eventItem) {
      return formatEventPrice(eventItem?.price)
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long' })
    },
    formatTime(time) {
      return (time || '').slice(0, 5)
    },
    getEventImage(eventItem) {
      return resolveEventImageUrl(eventItem?.imageUrl, eventItem?.id)
    },
    async addToCart(eventId) {
      this.addingToCart = eventId
      try {
        await addCartItem({ eventId, quantity: 1 })
        this.$root.$toast?.success('Билет добавлен в корзину.')
      } catch (error) {
        this.$root.$toast?.error(error.response?.data?.message || 'Не удалось добавить билет.')
      } finally {
        this.addingToCart = null
      }
    },
    async toggleFavorite(eventId) {
      try {
        if (this.favorites.has(eventId)) {
          await removeFavoriteEvent(eventId)
          this.favorites.delete(eventId)
          this.$root.$toast?.info('Удалено из избранного.')
        } else {
          await addFavoriteEvent(eventId)
          this.favorites.add(eventId)
          this.$root.$toast?.success('Добавлено в избранное.')
        }
      } catch {
        this.$root.$toast?.error('Не удалось обновить избранное.')
      }
    }
  }
}
</script>

<style scoped>
.catalog {
  padding-bottom: var(--space-6);
}

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-4);
  margin-bottom: var(--space-5);
  flex-wrap: wrap;
}

.catalog h1 {
  margin: 0 0 var(--space-1);
  font-size: clamp(1.5rem, 3vw, 2rem);
  animation: catalog-rise-in 420ms ease both;
}

.afisha-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-bottom: var(--space-5);
  padding: var(--space-4);
  animation: catalog-rise-in 520ms ease both;
}

.afisha-input {
  min-height: 44px;
  box-sizing: border-box;
  padding: 0 var(--space-3);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--surface);
  font-size: 0.9375rem;
  color: var(--text-primary);
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;
}
.afisha-input:focus {
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
  outline: none;
}

.afisha-input--search {
  width: 100%;
}

.afisha-input--block {
  width: 100%;
  cursor: pointer;
}

.afisha-input--num {
  width: 100%;
  min-width: 0;
  flex: 1 1 5.5rem;
}

.afisha-filters-grid {
  display: grid;
  gap: var(--space-4);
  grid-template-columns: 1fr;
}

@media (min-width: 720px) {
  .afisha-filters-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    align-items: end;
  }

  .afisha-field--span {
    grid-column: 1 / -1;
  }
}

@media (min-width: 960px) {
  .afisha-filters-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

.afisha-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  justify-content: space-between;
  gap: var(--space-3);
}

.afisha-toolbar .afisha-segment {
  flex: 1 1 min(0, 28rem);
  min-width: 0;
}

.afisha-toolbar__reset {
  flex: 0 0 auto;
  white-space: nowrap;
  min-height: 44px;
  align-self: stretch;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding-left: var(--space-4);
  padding-right: var(--space-4);
}

.afisha-field {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  min-width: 0;
}

.afisha-field__label {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--text-secondary);
}

.afisha-price-block {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-3);
}

.afisha-price-range {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-2);
  flex: 1 1 12rem;
  min-width: 0;
}

.afisha-price-range__sep {
  color: var(--text-secondary);
  font-weight: 600;
  flex: 0 0 auto;
  user-select: none;
}

.afisha-segment {
  display: grid;
  width: 100%;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1px;
  padding: 1px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--border);
}

.afisha-segment__btn {
  min-height: 44px;
  margin: 0;
  padding: var(--space-2) var(--space-2);
  border: none;
  border-radius: 0;
  background: var(--surface-muted);
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
  cursor: pointer;
  white-space: normal;
  text-align: center;
  line-height: 1.25;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s ease, color 0.15s ease;
}

@media (max-width: 640px) {
  .afisha-segment {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.afisha-segment__btn:hover {
  color: var(--text-primary);
  background: rgba(0, 0, 0, 0.04);
}

.afisha-segment__btn--active {
  background: var(--primary-muted);
  color: var(--primary);
  font-weight: 700;
}

.chips-row {
  margin-bottom: var(--space-1);
}

.view-meta {
  color: var(--text-secondary);
  font-size: 0.9375rem;
  font-weight: 600;
  white-space: nowrap;
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
  min-height: 440px;
  transition: transform 0.22s ease, box-shadow 0.22s ease, border-color 0.22s ease;
  animation: card-fade-in 320ms ease both;
  position: relative;
  isolation: isolate;
}

.card:hover {
  transform: perspective(900px) translateY(-2px) rotateX(1deg) rotateY(-1deg);
}

.poster {
  position: relative;
  height: 180px;
  background: var(--surface-muted);
  isolation: isolate;
}

.poster img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.poster-fallback {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.25rem;
  font-weight: 700;
  color: var(--text-secondary);
  background: var(--surface-muted);
}

.content {
  padding: var(--space-4);
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  flex: 1;
}

.chips {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.chip {
  font-size: 0.75rem;
  font-weight: 600;
  padding: var(--space-1) var(--space-2);
  border-radius: 999px;
  border: 1px solid var(--border);
}

.chip--warm {
  background: var(--accent-warm-muted);
  color: #b45309;
  border-color: #fde68a;
}

.chip--muted {
  background: var(--surface-muted);
  color: var(--text-secondary);
}

.chip--free {
  background: var(--primary-muted);
  color: var(--primary);
  border-color: rgba(15, 118, 110, 0.35);
}

.chip--seat {
  background: #eff6ff;
  color: #1d4ed8;
  border-color: #bfdbfe;
}

.content h3 {
  margin: 0;
  font-size: 1.0625rem;
  line-height: 1.35;
  min-height: 2.7em;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.description {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.875rem;
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  min-height: 2.9em;
}

.meta {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.price {
  margin: var(--space-2) 0 0;
  font-weight: 700;
  font-size: 1.125rem;
  color: var(--primary);
}

.card-footer {
  margin-top: auto;
  padding: var(--space-4);
  border-top: 1px solid var(--border);
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-2);
  align-items: stretch;
  background: var(--surface);
}

.card-footer :deep(.ds-btn) {
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}

.catalog :deep(.ds-btn) {
  position: relative;
  overflow: hidden;
}

.catalog :deep(.ds-btn)::after {
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

.catalog :deep(.ds-btn):active::after {
  animation: afisha-ripple 420ms ease-out;
}

.favorite-btn {
  position: absolute;
  z-index: 2;
  top: var(--space-3);
  right: var(--space-3);
  border: 1px solid var(--border);
  background: rgba(255, 255, 255, 0.95);
  width: 40px;
  height: 40px;
  border-radius: 999px;
  font-size: 1.125rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s ease, border-color 0.15s ease;
}

.favorite-btn:hover {
  background: #fff;
}

.favorite-btn.active {
  color: var(--accent-warm);
  border-color: #fcd34d;
  background: #fffbeb;
}

.loading,
.empty,
.state {
  padding: var(--space-6) var(--space-4);
  text-align: center;
  font-size: 0.9375rem;
}

.loading {
  color: var(--text-secondary);
}

.empty {
  color: var(--text-secondary);
  max-width: 40rem;
  margin: 0 auto;
}

.empty-title,
.state-title {
  margin: 0 0 var(--space-2);
  font-size: 1.125rem;
  font-weight: 800;
  color: var(--text-primary);
}

.empty-text,
.state-text {
  margin: 0 0 var(--space-4);
  line-height: 1.55;
}

.state--error .state-title {
  color: #b91c1c;
}

.state .ds-btn {
  margin-top: var(--space-1);
}
.skeleton-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-4);
}
.skeleton-card {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--surface);
}
.skeleton-card__body {
  padding: var(--space-4);
  display: grid;
  gap: var(--space-2);
}
.skeleton-block {
  position: relative;
  overflow: hidden;
  background: #e5e7eb;
  border-radius: var(--radius-sm);
}
.skeleton-block::after {
  content: '';
  position: absolute;
  inset: 0;
  transform: translateX(-100%);
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.65), transparent);
  animation: skeleton-sweep 1.2s ease-in-out infinite;
}
.skeleton-block--media {
  height: 180px;
  border-radius: 0;
}
.skeleton-block--line {
  height: 12px;
}
.skeleton-block--w90 { width: 90%; }
.skeleton-block--w80 { width: 80%; }
.skeleton-block--w70 { width: 70%; }
.skeleton-block--w45 { width: 45%; }

@media (max-width: 720px) {
  .topbar {
    flex-direction: column;
    align-items: flex-start;
  }
}

@media (max-width: 520px) {
  .card {
    min-height: 0;
  }

  .afisha-price-range {
    width: 100%;
  }
}

.catalog-list-enter-active {
  animation: card-fade-in 320ms ease both;
}

.catalog-list-move {
  transition: transform 0.28s ease;
}

@keyframes catalog-rise-in {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes card-fade-in {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.985);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
@keyframes afisha-ripple {
  0% {
    transform: translate(-50%, -50%) scale(0);
    opacity: 0.42;
  }
  100% {
    transform: translate(-50%, -50%) scale(18);
    opacity: 0;
  }
}
@keyframes skeleton-sweep {
  to {
    transform: translateX(100%);
  }
}

@media (prefers-reduced-motion: reduce) {
  .catalog h1,
  .afisha-panel,
  .card,
  .catalog-list-enter-active {
    animation: none;
  }
  .card,
  .catalog-list-move,
  .catalog :deep(.ds-btn)::after,
  .afisha-input,
  .favorite-btn,
  .afisha-segment__btn {
    transition: none;
  }
}
</style>

