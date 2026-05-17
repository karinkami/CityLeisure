<template>
  <div class="event-detail">
    <div class="container">
      <div v-if="loading" class="state">Загрузка мероприятия...</div>
      <div v-else-if="error" class="state">{{ error }}</div>
      <div v-else-if="eventItem" class="layout ds-panel ds-reveal" :class="{ 'layout--seated': usesSeatSelection }">
        <div class="poster poster--no-zoom">
          <img v-if="!imageError" :src="getEventImage(eventItem)" :alt="eventItem.title" @error="imageError = true" />
          <div v-else class="fallback">{{ eventItem.title.charAt(0) }}</div>
          <p v-if="posterSourceNote" class="poster-caption">{{ posterSourceNote }}</p>
        </div>

        <template v-if="usesSeatSelection">
          <div class="info info--lead">
            <div class="head-row">
              <h1>{{ eventItem.title }}</h1>
              <button
                v-if="isAuthenticated"
                type="button"
                class="ds-btn ds-btn--ghost fav"
                :class="{ active: isFavorite }"
                @click="toggleFavorite"
              >
                {{ isFavorite ? 'В избранном' : 'В избранное' }}
              </button>
            </div>
            <p class="desc">{{ eventItem.description }}</p>
            <EventStatusChips v-if="eventItem" class="detail-status" :event-item="eventItem" />
            <div class="meta-grid">
              <div class="meta-pair"><span class="meta-k">Категория</span><span class="meta-v">{{ eventItem.category?.name }}</span></div>
              <div class="meta-pair"><span class="meta-k">Площадка</span><span class="meta-v">{{ eventItem.venue?.name }}</span></div>
              <div class="meta-pair"><span class="meta-k">Адрес</span><span class="meta-v">{{ eventItem.venue?.address }}</span></div>
              <div class="meta-pair"><span class="meta-k">Дата и время</span><span class="meta-v">{{ formatDate(eventItem.eventDate) }}, {{ formatTime(eventItem.eventTime) }}</span></div>
              <div class="meta-pair"><span class="meta-k">Возраст</span><span class="meta-v">{{ eventItem.ageRating }}</span></div>
              <div class="meta-pair"><span class="meta-k">Свободно</span><span class="meta-v">{{ eventItem.availableTickets }} {{ ticketWord }}</span></div>
            </div>
          </div>

          <div class="booking-split" :class="{ 'booking-split--no-map': !eventItem.venue }">
            <div v-if="eventItem.venue" class="how-to ds-inset how-to--split">
              <h2 class="how-to__title">Как добраться</h2>
              <p class="how-to__addr">{{ fullAddress }}</p>
              <div v-if="mapEmbedUrl" class="how-to__map" aria-hidden="true">
                <iframe :src="mapEmbedUrl" title="Карта места проведения" loading="lazy" referrerpolicy="no-referrer-when-downgrade" />
              </div>
              <p v-else-if="fullAddress" class="how-to__map-note">Карту по адресу можно открыть по ссылке ниже.</p>
              <div class="how-to__actions">
                <a v-if="mapUrl" :href="mapUrl" class="ds-btn ds-btn--secondary" target="_blank" rel="noopener noreferrer">Яндекс.Карты</a>
                <a v-if="osmMapUrl" :href="osmMapUrl" class="ds-btn ds-btn--ghost" target="_blank" rel="noopener noreferrer">OpenStreetMap</a>
              </div>
            </div>
            <div class="booking-split__aside">
              <p class="price-line">
                <template v-if="Number(eventItem.price) > 0">
                  <span class="price">{{ formatPrice(eventItem.price) }} ₽</span>
                </template>
                <template v-else>
                  <span class="price price--free">Вход свободный</span>
                </template>
              </p>

              <div class="hall ds-inset">
                <p class="hall__hint">Экран / сцена</p>
                <div class="hall__rows">
                  <div v-for="(row, idx) in seatLayoutRows" :key="idx" class="hall__row">
                    <span class="hall__row-label">{{ row.rowLabel }}</span>
                    <div class="hall__seats">
                      <button
                        v-for="seat in row.seats"
                        :key="seat"
                        type="button"
                        class="seat"
                        :class="{
                          'seat--booked': isSeatBooked(seat),
                          'seat--picked': selectedSeats.includes(seat)
                        }"
                        :disabled="isSeatBooked(seat)"
                        @click="toggleSeat(seat)"
                      >
                        {{ seatShort(seat) }}
                      </button>
                    </div>
                  </div>
                </div>
                <p class="hall__legend">
                  Нажмите на свободные места. Выбрано: <strong>{{ selectedSeats.length }}</strong>
                  <button v-if="selectedSeats.length" type="button" class="hall__clear" @click="clearSeats">Сбросить</button>
                </p>
              </div>

              <div class="ticket-panel ds-inset">
                <div class="qty-field qty-field--seats">
                  <span class="qty-label">Режим</span>
                  <p class="qty-aside">Укажите места на схеме слева — число билетов совпадает с числом выбранных мест.</p>
                </div>
                <div class="ticket-panel__cta">
                  <button
                    v-if="isAuthenticated"
                    type="button"
                    class="ds-btn ds-btn--primary ds-btn--block"
                    :disabled="addingToCart || !canAddToCart"
                    @click="addToCart"
                  >
                    {{ addingToCart ? 'Добавляем…' : addButtonLabel }}
                  </button>
                  <router-link v-else to="/auth" class="ds-btn ds-btn--primary ds-btn--block">Войти, чтобы оформить</router-link>
                </div>
              </div>
            </div>
          </div>
        </template>

        <div v-else class="info">
          <div class="head-row">
            <h1>{{ eventItem.title }}</h1>
            <button
              v-if="isAuthenticated"
              type="button"
              class="ds-btn ds-btn--ghost fav"
              :class="{ active: isFavorite }"
              @click="toggleFavorite"
            >
              {{ isFavorite ? 'В избранном' : 'В избранное' }}
            </button>
          </div>
          <p class="desc">{{ eventItem.description }}</p>
          <EventStatusChips v-if="eventItem" class="detail-status" :event-item="eventItem" />
          <div class="meta-grid">
            <div class="meta-pair"><span class="meta-k">Категория</span><span class="meta-v">{{ eventItem.category?.name }}</span></div>
            <div class="meta-pair"><span class="meta-k">Площадка</span><span class="meta-v">{{ eventItem.venue?.name }}</span></div>
            <div class="meta-pair"><span class="meta-k">Адрес</span><span class="meta-v">{{ eventItem.venue?.address }}</span></div>
            <div class="meta-pair"><span class="meta-k">Дата и время</span><span class="meta-v">{{ formatDate(eventItem.eventDate) }}, {{ formatTime(eventItem.eventTime) }}</span></div>
            <div class="meta-pair"><span class="meta-k">Возраст</span><span class="meta-v">{{ eventItem.ageRating }}</span></div>
            <div class="meta-pair"><span class="meta-k">Свободно</span><span class="meta-v">{{ eventItem.availableTickets }} {{ ticketWord }}</span></div>
          </div>

          <div v-if="eventItem.venue" class="how-to ds-inset">
            <h2 class="how-to__title">Как добраться</h2>
            <p class="how-to__addr">{{ fullAddress }}</p>
            <div v-if="mapEmbedUrl" class="how-to__map" aria-hidden="true">
              <iframe :src="mapEmbedUrl" title="Карта места проведения" loading="lazy" referrerpolicy="no-referrer-when-downgrade" />
            </div>
            <p v-else-if="fullAddress" class="how-to__map-note">Карту по адресу можно открыть по ссылке ниже.</p>
            <div class="how-to__actions">
              <a v-if="mapUrl" :href="mapUrl" class="ds-btn ds-btn--secondary" target="_blank" rel="noopener noreferrer">Яндекс.Карты</a>
              <a v-if="osmMapUrl" :href="osmMapUrl" class="ds-btn ds-btn--ghost" target="_blank" rel="noopener noreferrer">OpenStreetMap</a>
            </div>
          </div>

          <p class="price-line">
            <template v-if="Number(eventItem.price) > 0">
              <span class="price">{{ formatPrice(eventItem.price) }} ₽</span>
            </template>
            <template v-else>
              <span class="price price--free">Вход свободный</span>
            </template>
          </p>

          <div class="ticket-panel ds-inset">
            <label class="qty-field">
              <span class="qty-label">Количество билетов</span>
              <input
                v-model.number="quantity"
                type="number"
                min="1"
                :max="eventItem.availableTickets || 1"
              />
            </label>
            <div class="ticket-panel__cta">
              <button
                v-if="isAuthenticated"
                type="button"
                class="ds-btn ds-btn--primary ds-btn--block"
                :disabled="addingToCart || !canAddToCart"
                @click="addToCart"
              >
                {{ addingToCart ? 'Добавляем…' : addButtonLabel }}
              </button>
              <router-link v-else to="/auth" class="ds-btn ds-btn--primary ds-btn--block">Войти, чтобы оформить</router-link>
            </div>
          </div>
        </div>
      </div>
      <section v-if="similarEvents.length" class="recs ds-reveal ds-reveal-delay-2">
        <h2 class="recs-title">Похожие мероприятия</h2>
        <div class="rec-grid">
          <router-link v-for="rec in similarEvents" :key="rec.id" :to="`/events/${rec.id}`" class="rec-card ds-card-pro">
            <strong>{{ rec.title }}</strong>
            <time>{{ formatDate(rec.eventDate) }}</time>
          </router-link>
        </div>
      </section>
    </div>
  </div>
</template>

<script>
import { isAuthenticated as hasSession } from '@/services/session'
import { addCartItem } from '@/services/cartService'
import { getEventById, getEventRecommendations, getEventSeatMap } from '@/services/eventsService'
import { addFavoriteEvent, getFavoriteEventIds, removeFavoriteEvent } from '@/services/favoritesService'
import { getFallbackPosterFileName, resolveEventImageUrl, usesAutomaticPoster } from '@/utils/imageResolver'
import { buildVenueMapUrl, buildVenueOsmUrl, buildYandexMapWidgetUrl } from '@/utils/mapLink'
import { eventUsesSeatSelection } from '@/utils/seating'
import EventStatusChips from '@/components/EventStatusChips.vue'

export default {
  name: 'EventDetailsView',
  components: { EventStatusChips },
  data() {
    return {
      eventItem: null,
      loading: true,
      error: '',
      imageError: false,
      quantity: 1,
      addingToCart: false,
      similarEvents: [],
      isFavorite: false,
      seatLayoutRows: [],
      bookedSeats: [],
      selectedSeats: []
    }
  },
  computed: {
    isAuthenticated() {
      return hasSession()
    },
    usesSeatSelection() {
      return eventUsesSeatSelection(this.eventItem)
    },
    ticketWord() {
      const n = this.eventItem?.availableTickets ?? 0
      const m = n % 10
      const m100 = n % 100
      if (m100 >= 11 && m100 <= 14) return 'билетов'
      if (m === 1) return 'билет'
      if (m >= 2 && m <= 4) return 'билета'
      return 'билетов'
    },
    canAddToCart() {
      if (!this.eventItem) return false
      if (this.usesSeatSelection) return this.selectedSeats.length > 0
      return this.quantity >= 1
    },
    addButtonLabel() {
      if (!this.eventItem) return 'В корзину'
      if (this.usesSeatSelection) {
        const n = this.selectedSeats.length
        return n ? `В корзину (${n} ${this.seatWord(n)})` : 'Выберите места'
      }
      return Number(this.eventItem.price) === 0 ? 'Забронировать бесплатно' : 'В корзину'
    },
    mapUrl() {
      return buildVenueMapUrl(this.eventItem?.venue)
    },
    osmMapUrl() {
      return buildVenueOsmUrl(this.eventItem?.venue)
    },
    fullAddress() {
      const v = this.eventItem?.venue
      if (!v) return ''
      return [v.address, v.city].filter(Boolean).join(', ')
    },
    mapEmbedUrl() {
      return buildYandexMapWidgetUrl(this.eventItem?.venue || null)
    },
    posterSourceNote() {
      const e = this.eventItem
      if (!e || !usesAutomaticPoster(e.imageUrl)) return ''
      const fn = getFallbackPosterFileName(e.id)
      if (fn) {
        return `Сейчас показывается автопостер «${fn}».`
      }
      return 'У события нет собственной картинки. Добавьте ссылку в поле image_url, чтобы вместо плейсхолдера отображался постер.'
    }
  },
  async mounted() {
    this.scrollToTop()
    await Promise.all([this.loadEvent(), this.loadSimilar(), this.loadFavoriteState()])
  },
  watch: {
    '$route.params.id': {
      async handler() {
        this.scrollToTop()
        this.loading = true
        this.error = ''
        await Promise.all([this.loadEvent(), this.loadSimilar(), this.loadFavoriteState()])
      }
    }
  },
  methods: {
    scrollToTop() {
      window.scrollTo({ top: 0, left: 0, behavior: 'instant' })
    },
    async loadEvent() {
      this.selectedSeats = []
      this.seatLayoutRows = []
      this.bookedSeats = []
      try {
        this.eventItem = await getEventById(this.$route.params.id)
        this.quantity = 1
        if (eventUsesSeatSelection(this.eventItem)) {
          await this.loadSeatMap()
        }
      } catch (error) {
        this.error = error.response?.status === 404 ? 'Мероприятие не найдено' : 'Ошибка загрузки мероприятия.'
      } finally {
        this.loading = false
      }
    },
    async loadSeatMap() {
      try {
        const data = await getEventSeatMap(this.$route.params.id)
        const layout = this.parseSeatLayoutJson(data?.layoutJson)
        this.seatLayoutRows = this.normalizeSeatLayoutRows(layout)
        const rawBooked = data?.booked ?? data?.Booked
        this.bookedSeats = Array.isArray(rawBooked)
          ? rawBooked.map((s) => String(s).trim()).filter(Boolean)
          : []
      } catch {
        this.seatLayoutRows = []
        this.bookedSeats = []
      }
    },
    parseSeatLayoutJson(layoutJson) {
      if (layoutJson == null || layoutJson === '') return {}
      if (typeof layoutJson === 'object' && !Array.isArray(layoutJson)) return layoutJson
      if (typeof layoutJson !== 'string') return {}
      try {
        return JSON.parse(layoutJson)
      } catch {
        return {}
      }
    },
    normalizeSeatLayoutRows(layout) {
      const rows = layout?.rows ?? layout?.Rows
      if (!Array.isArray(rows)) return []
      return rows.map((row, idx) => {
        const rawSeats = row?.seats ?? row?.Seats ?? row?.seat ?? []
        const seats = Array.isArray(rawSeats)
          ? rawSeats.map((s) => String(s).trim()).filter(Boolean)
          : []
        const rowLabel =
          row?.rowLabel ?? row?.RowLabel ?? row?.row_label ?? row?.label ?? `Ряд ${idx + 1}`
        return { rowLabel, seats }
      })
    },
    isSeatBooked(seat) {
      const s = String(seat).trim()
      return this.bookedSeats.some((b) => String(b).trim() === s)
    },
    toggleSeat(seat) {
      if (this.isSeatBooked(seat)) return
      const i = this.selectedSeats.indexOf(seat)
      if (i >= 0) this.selectedSeats.splice(i, 1)
      else this.selectedSeats.push(seat)
    },
    clearSeats() {
      this.selectedSeats = []
    },
    seatShort(seat) {
      const parts = String(seat).split('-')
      return parts.length === 2 ? parts[1] : seat
    },
    seatWord(n) {
      const m = n % 10
      const m100 = n % 100
      if (m100 >= 11 && m100 <= 14) return 'мест'
      if (m === 1) return 'место'
      if (m >= 2 && m <= 4) return 'места'
      return 'мест'
    },
    async loadSimilar() {
      try {
        this.similarEvents = await getEventRecommendations(this.$route.params.id)
      } catch {
        this.similarEvents = []
      }
    },
    async loadFavoriteState() {
      if (!this.isAuthenticated) return
      try {
        const ids = await getFavoriteEventIds()
        this.isFavorite = ids.includes(Number(this.$route.params.id))
      } catch {
        this.isFavorite = false
      }
    },
    formatPrice(price) {
      return new Intl.NumberFormat('ru-RU').format(price)
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long', year: 'numeric' })
    },
    formatTime(time) {
      return (time || '').slice(0, 5)
    },
    getEventImage(eventItem) {
      return resolveEventImageUrl(eventItem?.imageUrl, eventItem?.id)
    },
    async addToCart() {
      this.addingToCart = true
      try {
        const payload = { eventId: this.eventItem.id, quantity: this.quantity }
        if (this.usesSeatSelection) {
          const seats = [...this.selectedSeats].sort()
          payload.quantity = seats.length
          payload.seats = seats
        }
        await addCartItem(payload)
        const n = this.usesSeatSelection ? payload.quantity : this.quantity
        this.$root.$toast?.success(`В корзине: ${n} ${n === 1 ? 'билет' : n < 5 ? 'билета' : 'билетов'}`)
        if (this.usesSeatSelection) {
          this.clearSeats()
          await this.loadSeatMap()
        }
        try {
          const latestEvent = await getEventById(this.eventItem.id)
          if (latestEvent?.availableTickets != null) this.eventItem.availableTickets = latestEvent.availableTickets
        } catch {
        }
      } catch (error) {
        this.$root.$toast?.error(error.response?.data?.message || 'Не удалось добавить билеты.')
      } finally {
        this.addingToCart = false
      }
    },
    async toggleFavorite() {
      try {
        if (this.isFavorite) {
          await removeFavoriteEvent(this.eventItem.id)
          this.isFavorite = false
        } else {
          await addFavoriteEvent(this.eventItem.id)
          this.isFavorite = true
        }
      } catch {
        this.$root.$toast?.error('Не удалось обновить избранное.')
      }
    }
  }
}
</script>

<style scoped>
.event-detail {
  min-height: calc(100vh - 140px);
}

.state {
  padding: var(--space-6);
  text-align: center;
  color: var(--text-secondary);
}

.layout {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  gap: var(--space-5);
}

.layout--seated .poster {
  grid-column: 1;
  grid-row: 1;
}

.layout--seated .info--lead {
  grid-column: 2;
  grid-row: 1;
}

.booking-split {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  gap: var(--space-5);
  align-items: start;
}

.layout--seated .booking-split {
  grid-column: 1 / -1;
  grid-row: 2;
  margin-top: var(--space-1);
}

.booking-split--no-map {
  grid-template-columns: 1fr;
}

.booking-split--no-map .booking-split__aside {
  max-width: 720px;
  margin-inline: auto;
  width: 100%;
}

.booking-split__aside {
  min-width: 0;
}

.how-to--split {
  margin-top: 0;
}

.layout--seated .how-to--split .how-to__map iframe {
  height: min(360px, 52vh);
}

.head-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-4);
  flex-wrap: wrap;
}

.head-row h1 {
  margin: 0;
  font-size: clamp(1.35rem, 2.5vw, 1.85rem);
  line-height: 1.2;
  flex: 1;
  min-width: 0;
}

.fav.active {
  border-color: #fcd34d !important;
  color: #b45309 !important;
  background: #fffbeb !important;
}

.poster {
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--surface-muted);
  min-height: 320px;
  max-height: 420px;
}

.poster img {
  width: 100%;
  height: 100%;
  max-height: 420px;
  object-fit: cover;
  transform: none;
  transition: none;
}

.poster-caption {
  margin: var(--space-2) 0 0;
  font-size: 0.8125rem;
  line-height: 1.45;
  color: var(--text-secondary);
}

.poster--no-zoom img {
  transform: none !important;
}

.fallback {
  min-height: 320px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
  font-weight: 700;
  color: var(--text-secondary);
  background: var(--surface-muted);
}

.desc {
  color: var(--text-secondary);
  margin: var(--space-4) 0;
  font-size: 0.9375rem;
  line-height: 1.55;
}

.meta-grid {
  display: grid;
  gap: var(--space-3);
  margin: 0 0 var(--space-4);
}

.meta-pair {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: var(--space-3);
  padding-bottom: var(--space-3);
  border-bottom: 1px solid var(--border);
  font-size: 0.9375rem;
}

.meta-pair:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.meta-k {
  font-weight: 600;
  color: var(--text-secondary);
}

.meta-v {
  color: var(--text-primary);
}

.price-line {
  margin: 0 0 var(--space-4);
}

.price {
  font-size: 1.5rem;
  color: var(--primary);
  font-weight: 700;
}

.price--free {
  color: var(--primary);
}

.hall {
  margin-bottom: var(--space-4);
}

.hall__hint {
  text-align: center;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: var(--text-secondary);
  margin: 0 0 var(--space-3);
  padding: var(--space-2);
  border-radius: var(--radius-md);
  background: linear-gradient(180deg, #e5e7eb 0%, #d1d5db 100%);
}

.hall__rows {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  max-height: 280px;
  overflow-y: auto;
  padding-right: var(--space-1);
}

.hall__row {
  display: grid;
  grid-template-columns: 4.5rem 1fr;
  gap: var(--space-2);
  align-items: center;
}

.hall__row-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-secondary);
}

.hall__seats {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.seat {
  min-width: 28px;
  height: 28px;
  padding: 0 4px;
  border-radius: 6px;
  border: 1px solid var(--border);
  background: var(--surface);
  font-size: 0.6875rem;
  font-weight: 700;
  cursor: pointer;
  color: var(--text-primary);
  transition: background 0.15s ease, border-color 0.15s ease, transform 0.12s ease;
}

.seat:hover:not(:disabled) {
  border-color: var(--primary);
  background: var(--primary-muted);
}

.seat--picked {
  background: var(--primary);
  color: #fff;
  border-color: var(--primary);
}

.seat--booked {
  background: var(--surface-muted);
  color: var(--text-secondary);
  cursor: not-allowed;
  text-decoration: line-through;
}

.hall__legend {
  margin: var(--space-3) 0 0;
  font-size: 0.875rem;
  color: var(--text-secondary);
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-3);
}

.hall__clear {
  border: none;
  background: none;
  color: var(--primary);
  font-weight: 700;
  cursor: pointer;
  text-decoration: underline;
  font-size: inherit;
}

.qty-field--seats .qty-aside {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.45;
}

.ticket-panel {
  display: grid;
  grid-template-columns: minmax(120px, 160px) 1fr;
  gap: var(--space-4);
  align-items: end;
}

.qty-field {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.qty-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.qty-field input {
  width: 100%;
  min-height: 44px;
  padding: 0 var(--space-3);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--surface);
}

.ticket-panel__cta {
  min-width: 0;
}

.detail-status {
  margin: 0 0 var(--space-3);
}

.how-to {
  margin: var(--space-4) 0;
  padding: var(--space-4);
}

.how-to__title {
  margin: 0 0 var(--space-2);
  font-size: 1rem;
}

.how-to__addr {
  margin: 0 0 var(--space-3);
  color: var(--text-secondary);
  font-size: 0.9375rem;
}

.how-to__map {
  margin-bottom: var(--space-3);
  border-radius: var(--radius-md);
  overflow: hidden;
  border: 1px solid var(--border);
}

.how-to__map iframe {
  width: 100%;
  height: 240px;
  border: none;
  display: block;
}

.how-to__map-note {
  margin: 0 0 var(--space-3);
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.how-to__actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-3);
}

.recs {
  margin-top: var(--space-5);
}

.recs-title {
  font-size: 1.125rem;
  margin: 0 0 var(--space-3);
}

.rec-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-3);
}

.rec-card {
  padding: var(--space-4);
  text-decoration: none;
  color: inherit;
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.rec-card time {
  color: var(--text-secondary);
  font-size: 0.875rem;
}

@media (max-width: 900px) {
  .layout {
    grid-template-columns: 1fr;
  }

  .layout--seated .poster,
  .layout--seated .info--lead,
  .layout--seated .booking-split {
    grid-column: 1;
    grid-row: auto;
  }

  .booking-split {
    grid-template-columns: 1fr;
  }

  .layout--seated .how-to--split .how-to__map iframe {
    height: 240px;
  }

  .meta-pair {
    grid-template-columns: 1fr;
    gap: var(--space-1);
  }

  .ticket-panel {
    grid-template-columns: 1fr;
  }
}
</style>

