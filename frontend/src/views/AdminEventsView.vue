<template>
  <div class="admin-events">
    <div class="container">
      <header class="topbar">
        <h1>Администрирование мероприятий</h1>
      </header>

      <section class="card">
        <div class="form-head">
          <h2>{{ editId ? 'Редактирование мероприятия' : 'Создание нового мероприятия' }}</h2>
        </div>
        <form class="form-grid" @submit.prevent="saveEvent">
          <div class="form-field">
            <label>Название мероприятия</label>
            <input v-model.trim="form.title" required placeholder="Например: Ночной джаз в парке" />
          </div>

          <div class="form-field">
            <label>Описание</label>
            <input v-model.trim="form.description" required placeholder="Кратко опишите мероприятие" />
          </div>

          <div class="form-field">
            <label>Категория</label>
            <select v-model.number="form.categoryId" required>
              <option :value="0" disabled>Выберите категорию</option>
              <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
            </select>
          </div>

          <div class="form-field">
            <label>Площадка</label>
            <select v-model.number="form.venueId" required>
              <option :value="0" disabled>Выберите площадку</option>
              <option v-for="v in venues" :key="v.id" :value="v.id">{{ v.name }}</option>
            </select>
          </div>

          <div class="form-field">
            <label>Адрес площадки</label>
            <input v-model.trim="form.venueAddress" placeholder="Например: ул. Ленина, 10" />
          </div>

          <div class="form-field">
            <label>Город площадки</label>
            <input v-model.trim="form.venueCity" placeholder="Например: Москва" />
          </div>

          <div class="form-field">
            <label>Ссылка на изображение</label>
            <input v-model.trim="form.imageUrl" placeholder="https://... (необязательно)" />
            <small class="field-hint">Из папки: укажите имя файла (например, <code>poster-03.jpg</code>). Из интернета: вставьте прямую ссылку <code>https://...</code> на картинку.</small>
          </div>

          <div class="form-field">
            <label>Дата проведения</label>
            <input v-model="form.eventDate" required type="date" />
          </div>

          <div class="form-field">
            <label>Время начала</label>
            <input v-model="form.eventTime" required type="time" />
          </div>

          <div class="form-field">
            <label>Цена билета, ₽</label>
            <input v-model.number="form.price" required type="number" min="0" step="1" placeholder="0" />
          </div>

          <div class="form-field">
            <label>Доступно билетов</label>
            <input v-model.number="form.availableTickets" required type="number" min="0" step="1" placeholder="100" />
          </div>

          <div class="form-field">
            <label>Возрастной рейтинг</label>
            <input v-model.trim="form.ageRating" placeholder="Например: 0+, 6+, 12+, 16+, 18+" />
          </div>

          <div class="form-field">
            <label>Статус мероприятия</label>
            <select v-model="form.status">
              <option value="active">active (показывается в афише)</option>
              <option value="inactive">inactive (скрыто из афиши)</option>
            </select>
          </div>

          <div class="form-field">
            <label>Тип рассадки</label>
            <select v-model="form.seatingType">
              <option value="general">general (без выбора мест)</option>
              <option value="assigned">assigned (с выбором мест)</option>
            </select>
          </div>

          <div class="form-field">
            <label>JSON-схема мест</label>
            <textarea v-model.trim="form.seatLayoutJson" placeholder="Заполняйте только для типа assigned" rows="3"></textarea>
            <small class="field-hint">Для general можно оставить пустым.</small>
          </div>

          <div class="actions">
            <div v-if="formError" class="error-message">{{ formError }}</div>
            <button class="ds-btn ds-btn--primary" type="submit" :disabled="saving">
              {{ saving ? 'Сохраняем...' : editId ? 'Сохранить изменения' : 'Создать мероприятие' }}
            </button>
            <button class="ds-btn ds-btn--secondary" type="button" @click="resetForm">Отмена</button>
          </div>
        </form>
      </section>

      <section class="card">
        <h2>Список мероприятий</h2>
        <div v-if="loading">Загрузка...</div>
        <div v-else class="list">
          <article v-for="eventItem in events" :key="eventItem.id" class="item">
            <div>
              <h3>{{ eventItem.title }}</h3>
              <p>{{ formatDate(eventItem.eventDate) }} {{ formatTime(eventItem.eventTime) }} • {{ Number(eventItem.price) }} ₽</p>
            </div>
            <div class="row-actions">
              <button class="ds-btn ds-btn--secondary" @click="startEdit(eventItem)">Редактировать</button>
              <button
                v-if="eventItem.status !== 'inactive'"
                class="ds-btn ds-btn--secondary"
                @click="setInactive(eventItem)"
              >
                Скрыть
              </button>
              <button class="ds-btn ds-btn--secondary row-actions__danger" @click="deleteEvent(eventItem.id)">Удалить</button>
            </div>
          </article>
        </div>
      </section>
    </div>
  </div>
</template>

<script>
import {
  createAdminEvent,
  deleteAdminEvent,
  getAdminCategories,
  getAdminEvents,
  getAdminVenues,
  updateAdminEvent,
  updateAdminVenue
} from '@/services/adminEventsService'

const emptyForm = () => ({
  title: '',
  description: '',
  categoryId: 0,
  venueId: 0,
  imageUrl: '',
  venueAddress: '',
  venueCity: '',
  eventDate: '',
  eventTime: '',
  price: 0,
  availableTickets: 0,
  ageRating: '0+',
  status: 'active',
  seatingType: 'general',
  seatLayoutJson: ''
})

const DEFAULT_SEAT_LAYOUT = JSON.stringify(
  {
    section: 'Основной зал',
    rows: [
      { rowLabel: 'Ряд 1', seats: ['1-1', '1-2', '1-3', '1-4', '1-5', '1-6', '1-7', '1-8', '1-9', '1-10'] },
      { rowLabel: 'Ряд 2', seats: ['2-1', '2-2', '2-3', '2-4', '2-5', '2-6', '2-7', '2-8', '2-9', '2-10'] },
      { rowLabel: 'Ряд 3', seats: ['3-1', '3-2', '3-3', '3-4', '3-5', '3-6', '3-7', '3-8', '3-9', '3-10'] },
      { rowLabel: 'Ряд 4', seats: ['4-1', '4-2', '4-3', '4-4', '4-5', '4-6', '4-7', '4-8', '4-9', '4-10'] },
      { rowLabel: 'Ряд 5', seats: ['5-1', '5-2', '5-3', '5-4', '5-5', '5-6', '5-7', '5-8', '5-9', '5-10'] },
      { rowLabel: 'Ряд 6', seats: ['6-1', '6-2', '6-3', '6-4', '6-5', '6-6', '6-7', '6-8', '6-9', '6-10'] }
    ]
  },
  null,
  2
)

export default {
  name: 'AdminEventsView',
  data() {
    return {
      loading: true,
      saving: false,
      formError: '',
      events: [],
      categories: [],
      venues: [],
      editId: null,
      form: emptyForm()
    }
  },
  async mounted() {
    await Promise.all([this.loadCatalogs(), this.loadEvents()])
  },
  watch: {
    'form.venueId'(nextId) {
      const venue = this.venues.find(v => v.id === Number(nextId))
      if (!venue) return
      this.form.venueAddress = venue.address || ''
      this.form.venueCity = venue.city || ''
    },
    'form.seatingType'(nextType) {
      if (nextType === 'assigned' && !String(this.form.seatLayoutJson || '').trim()) {
        this.form.seatLayoutJson = DEFAULT_SEAT_LAYOUT
      }
    }
  },
  methods: {
    async loadCatalogs() {
      const [cats, venues] = await Promise.all([getAdminCategories(), getAdminVenues()])
      this.categories = Array.isArray(cats) ? cats : []
      this.venues = Array.isArray(venues) ? venues : []
      if (!this.form.categoryId && this.categories.length) this.form.categoryId = this.categories[0].id
      if (!this.form.venueId && this.venues.length) this.form.venueId = this.venues[0].id
      const selectedVenue = this.venues.find(v => v.id === Number(this.form.venueId))
      if (selectedVenue) {
        this.form.venueAddress = selectedVenue.address || ''
        this.form.venueCity = selectedVenue.city || ''
      }
    },
    async loadEvents() {
      this.loading = true
      try {
        this.events = await getAdminEvents()
      } finally {
        this.loading = false
      }
    },
    async saveEvent() {
      this.formError = ''
      const validationError = this.validateForm()
      if (validationError) {
        this.formError = validationError
        this.$root.$toast?.error(validationError)
        return
      }

      this.saving = true
      const payload = {
        ...this.form,
        imageUrl: this.normalizeImageUrl(this.form.imageUrl),
        eventTime: this.normalizeTimeForApi(this.form.eventTime),
        seatLayoutJson: this.form.seatLayoutJson || null
      }
      delete payload.venueAddress
      delete payload.venueCity

      try {
        await this.updateVenueAddressIfChanged()
        if (this.editId) await updateAdminEvent(this.editId, payload)
        else await createAdminEvent(payload)
        this.$root.$toast?.success(this.editId ? 'Мероприятие обновлено.' : 'Мероприятие создано.')
        this.resetForm()
        await this.loadEvents()
      } catch (error) {
        this.$root.$toast?.error(error.response?.data?.message || 'Не удалось сохранить мероприятие.')
      } finally {
        this.saving = false
      }
    },
    startEdit(eventItem) {
      this.editId = eventItem.id
      this.form = {
        title: eventItem.title || '',
        description: eventItem.description || '',
        categoryId: eventItem.categoryId || 0,
        venueId: eventItem.venueId || 0,
        imageUrl: eventItem.imageUrl || '',
        venueAddress: eventItem.venue?.address || '',
        venueCity: eventItem.venue?.city || '',
        eventDate: this.toDateInput(eventItem.eventDate),
        eventTime: this.toTimeInput(eventItem.eventTime),
        price: Number(eventItem.price || 0),
        availableTickets: Number(eventItem.availableTickets || 0),
        ageRating: eventItem.ageRating || '0+',
        status: eventItem.status || 'active',
        seatingType: eventItem.seatingType || 'general',
        seatLayoutJson:
          (eventItem.seatingType || 'general') === 'assigned'
            ? (eventItem.seatLayoutJson || DEFAULT_SEAT_LAYOUT)
            : (eventItem.seatLayoutJson || '')
      }
      window.scrollTo({ top: 0, behavior: 'smooth' })
    },
    async deleteEvent(id) {
      const confirmed = await this.$root.$confirm?.show(
        'Удаление мероприятия',
        'Удалить мероприятие? Это действие нельзя отменить.',
        'Удалить'
      )
      if (!confirmed) return
      try {
        await deleteAdminEvent(id)
        if (this.editId === id) this.resetForm()
        this.$root.$toast?.info('Мероприятие удалено.')
        await this.loadEvents()
      } catch (error) {
        this.$root.$toast?.error(error.response?.data?.message || 'Не удалось удалить мероприятие.')
      }
    },
    async setInactive(eventItem) {
      const confirmed = await this.$root.$confirm?.show(
        'Скрыть мероприятие',
        `Перевести «${eventItem.title}» в статус inactive?`,
        'Скрыть'
      )
      if (!confirmed) return

      const payload = {
        title: eventItem.title || '',
        description: eventItem.description || '',
        categoryId: eventItem.categoryId || 0,
        venueId: eventItem.venueId || 0,
        imageUrl: this.normalizeImageUrl(eventItem.imageUrl || ''),
        eventDate: this.toDateInput(eventItem.eventDate),
        eventTime: this.normalizeTimeForApi(this.toTimeInput(eventItem.eventTime)),
        price: Number(eventItem.price || 0),
        availableTickets: Number(eventItem.availableTickets || 0),
        ageRating: eventItem.ageRating || '0+',
        status: 'inactive',
        seatingType: eventItem.seatingType || 'general',
        seatLayoutJson: eventItem.seatLayoutJson || null
      }

      try {
        await updateAdminEvent(eventItem.id, payload)
        this.$root.$toast?.success('Мероприятие скрыто из афиши.')
        if (this.editId === eventItem.id) {
          this.form.status = 'inactive'
        }
        await this.loadEvents()
      } catch (error) {
        this.$root.$toast?.error(error.response?.data?.message || 'Не удалось скрыть мероприятие.')
      }
    },
    resetForm() {
      this.editId = null
      this.formError = ''
      this.form = emptyForm()
      if (this.categories.length) this.form.categoryId = this.categories[0].id
      if (this.venues.length) this.form.venueId = this.venues[0].id
      const selectedVenue = this.venues.find(v => v.id === Number(this.form.venueId))
      if (selectedVenue) {
        this.form.venueAddress = selectedVenue.address || ''
        this.form.venueCity = selectedVenue.city || ''
      }
    },
    async updateVenueAddressIfChanged() {
      const venue = this.venues.find(v => v.id === Number(this.form.venueId))
      if (!venue) return

      const nextAddress = String(this.form.venueAddress || '').trim()
      const nextCity = String(this.form.venueCity || '').trim()
      const oldAddress = String(venue.address || '').trim()
      const oldCity = String(venue.city || '').trim()
      if (nextAddress === oldAddress && nextCity === oldCity) return

      await updateAdminVenue(venue.id, {
        name: venue.name,
        address: nextAddress,
        city: nextCity,
        description: venue.description || '',
        mapUrl: venue.mapUrl || null
      })

      venue.address = nextAddress
      venue.city = nextCity
    },
    validateForm() {
      if (!String(this.form.title || '').trim()) return 'Укажите название мероприятия'
      if (!String(this.form.description || '').trim()) return 'Укажите описание мероприятия'
      if (!(Number(this.form.categoryId) > 0)) return 'Выберите категорию'
      if (!(Number(this.form.venueId) > 0)) return 'Выберите площадку'
      if (!String(this.form.eventDate || '').trim()) return 'Укажите дату проведения'
      if (!String(this.form.eventTime || '').trim()) return 'Укажите время начала'
      if (Number.isNaN(Number(this.form.price)) || Number(this.form.price) < 0) return 'Цена должна быть неотрицательной'
      if (Number.isNaN(Number(this.form.availableTickets)) || Number(this.form.availableTickets) < 0) return 'Количество билетов должно быть неотрицательным'

      const seatingType = String(this.form.seatingType || 'general').trim().toLowerCase()
      if (seatingType !== 'general' && seatingType !== 'assigned') return 'Тип рассадки должен быть general или assigned'
      if (seatingType === 'assigned') {
        const raw = String(this.form.seatLayoutJson || '').trim()
        if (!raw) return 'Для assigned заполните JSON-схему мест'
        try {
          const parsed = JSON.parse(raw)
          if (!Array.isArray(parsed?.rows)) return 'JSON-схема мест должна содержать массив rows'
          const hasAnySeat = parsed.rows.some(r => Array.isArray(r?.seats) && r.seats.length > 0)
          if (!hasAnySeat) return 'JSON-схема мест должна содержать хотя бы одно место'
        } catch {
          return 'JSON-схема мест содержит некорректный JSON'
        }
      }
      return ''
    },
    toDateInput(v) {
      if (!v) return ''
      const d = new Date(v)
      const month = `${d.getMonth() + 1}`.padStart(2, '0')
      const day = `${d.getDate()}`.padStart(2, '0')
      return `${d.getFullYear()}-${month}-${day}`
    },
    toTimeInput(v) {
      return String(v || '').slice(0, 5)
    },
    normalizeTimeForApi(v) {
      const s = String(v || '').trim()
      if (!s) return s
      if (/^\d{2}:\d{2}$/.test(s)) return `${s}:00`
      return s
    },
    normalizeImageUrl(v) {
      const s = String(v || '').trim()
      if (!s) return null
      if (s === 'https://... (необязательно)' || s === 'https://...') return null
      return s
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU')
    },
    formatTime(time) {
      return String(time || '').slice(0, 5)
    }
  }
}
</script>

<style scoped>
.admin-events { padding-bottom: 32px; }
.topbar { margin-bottom: 16px; }
.card {
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 16px;
  background: var(--surface);
  margin-bottom: 16px;
}
.form-grid {
  display: grid;
  gap: 10px;
}
.form-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}
.mode-switch {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.mode-switch__active {
  background: var(--primary-muted);
  color: var(--primary);
  border-color: rgba(15, 118, 110, 0.28);
}
.form-field {
  display: grid;
  gap: 6px;
}
.form-field label {
  font-size: 14px;
  font-weight: 600;
}
.form-grid input, .form-grid select, .form-grid textarea {
  min-height: 40px;
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 8px 10px;
}
.field-hint {
  color: var(--text-secondary);
  font-size: 12px;
}
.actions { display: flex; gap: 8px; }
.error-message {
  width: 100%;
  color: var(--danger);
  margin-bottom: var(--space-2);
  padding: var(--space-3);
  background: var(--danger-muted);
  border: 1px solid #fecaca;
  border-radius: var(--radius-md);
  text-align: center;
  font-size: 0.9375rem;
}
.list { display: grid; gap: 10px; }
.item {
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 10px;
}
.item h3 { margin: 0 0 4px; }
.item p { margin: 0; color: var(--text-secondary); }
.row-actions { display: flex; gap: 8px; }
.row-actions__danger { color: #b91c1c; border-color: #fecaca; }
</style>
