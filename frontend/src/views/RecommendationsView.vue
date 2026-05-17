<template>
  <div class="recommendations">
    <div class="container">
      <div class="top ds-reveal">
        <h1>Подбор мероприятий</h1>
        <p>Сначала выберите базовые параметры, затем при желании уточните подбор текстом.</p>
      </div>

      <section class="panel wizard ds-panel ds-reveal ds-reveal-delay-1">
        <div class="steps">
          <button
            v-for="step in steps"
            :key="step.id"
            type="button"
            class="step"
            :class="{ active: step.id === currentStep }"
            @click="goToStep(step.id)"
          >
            {{ step.id }}. {{ step.label }}
          </button>
        </div>
        <div class="wizard-progress" role="presentation">
          <span class="wizard-progress__bar" :style="{ width: progressPercent + '%' }"></span>
        </div>

        <div class="question-box">
          <transition name="wizard-step" mode="out-in">
            <div :key="'step-' + currentStep" class="question-content">
          <template v-if="currentStep === 1">
            <h3>С кем планируете идти?</h3>
            <p class="wizard-hint wizard-hint--tight">Начнем с простого: отметьте один или несколько вариантов.</p>
            <div class="wizard-choice-grid" role="group" aria-label="Компания">
              <label v-for="item in wizardOptions.company" :key="item.value" class="check-row">
                <input
                  :checked="isWizardOptionSelected('company', item.value)"
                  type="checkbox"
                  @change="toggleWizardOption('company', item.value)"
                />
                <span>{{ item.label }}</span>
              </label>
            </div>
          </template>

          <template v-else-if="currentStep === 2">
            <h3>Какой формат и ритм?</h3>
            <p class="wizard-hint wizard-hint--tight">Можно отметить несколько пунктов в каждом блоке.</p>
            <div class="wizard-prefs-grid">
              <label class="wizard-label">
                <span>Формат</span>
                <div class="wizard-choice-grid" role="group" aria-label="Формат">
                  <label v-for="item in wizardOptions.format" :key="item.value" class="check-row">
                    <input
                      :checked="isWizardOptionSelected('format', item.value)"
                      type="checkbox"
                      @change="toggleWizardOption('format', item.value)"
                    />
                    <span>{{ item.label }}</span>
                  </label>
                </div>
              </label>
              <label class="wizard-label">
                <span>Время суток</span>
                <div class="wizard-choice-grid" role="group" aria-label="Время суток">
                  <label v-for="item in wizardOptions.timeOfDay" :key="item.value" class="check-row">
                    <input
                      :checked="isWizardOptionSelected('timeOfDay', item.value)"
                      type="checkbox"
                      @change="toggleWizardOption('timeOfDay', item.value)"
                    />
                    <span>{{ item.label }}</span>
                  </label>
                </div>
              </label>
              <label class="wizard-label">
                <span>Интенсивность</span>
                <div class="wizard-choice-grid" role="group" aria-label="Интенсивность">
                  <label v-for="item in wizardOptions.energy" :key="item.value" class="check-row">
                    <input
                      :checked="isWizardOptionSelected('energy', item.value)"
                      type="checkbox"
                      @change="toggleWizardOption('energy', item.value)"
                    />
                    <span>{{ item.label }}</span>
                  </label>
                </div>
              </label>
              <label class="wizard-label">
                <span>Возрастной рейтинг</span>
                <div class="wizard-choice-grid" role="group" aria-label="Возрастной рейтинг">
                  <label v-for="item in wizardOptions.agePreference" :key="item.value" class="check-row">
                    <input
                      :checked="isWizardOptionSelected('agePreference', item.value)"
                      type="checkbox"
                      @change="toggleWizardOption('agePreference', item.value)"
                    />
                    <span>{{ item.label }}</span>
                  </label>
                </div>
              </label>
            </div>
          </template>

          <template v-else-if="currentStep === 3">
            <h3>Фильтры по каталогу</h3>
            <div class="wizard-filters">
              <div class="wizard-field wizard-field--full">
                <span class="wizard-field__label">Категории</span>
                <div v-if="categories.length" class="category-checkboxes" role="group" aria-label="Категории">
                  <label v-for="cat in categories" :key="cat.id" class="check-row">
                    <input v-model="answers.categoryIds" type="checkbox" :value="cat.id" />
                    <span>{{ cat.name }}</span>
                  </label>
                </div>
                <p v-else class="wizard-hint">Категории загружаются…</p>
              </div>
              <div class="wizard-row-2">
                <label class="wizard-label">
                  <span>Мин. бюджет (₽)</span>
                  <input
                    v-model.number="answers.minPrice"
                    type="number"
                    inputmode="numeric"
                    min="0"
                    step="1"
                    placeholder="500"
                  />
                </label>
                <label class="wizard-label">
                  <span>Макс. бюджет (₽)</span>
                  <input
                    v-model.number="answers.maxPrice"
                    type="number"
                    inputmode="numeric"
                    min="0"
                    step="1"
                    placeholder="2000"
                  />
                </label>
              </div>
              <div class="wizard-row-2">
                <label class="wizard-label">
                  <span>Дата не раньше</span>
                  <input v-model="answers.dateFrom" type="date" />
                </label>
                <label class="wizard-label">
                  <span>Дата не позже</span>
                  <input v-model="answers.dateTo" type="date" />
                </label>
              </div>
            </div>
          </template>
          <template v-else-if="currentStep === 4">
            <h3>Уточнение запроса (необязательно)</h3>
            <p class="wizard-hint">Если хотите, опишите, что именно ищете. Можно пропустить этот шаг.</p>
            <textarea
              v-model.trim="answers.query"
              class="wizard-textarea"
              rows="4"
              placeholder="Например: камерный концерт, выставка современного искусства, активный выходной"
            />
          </template>
            </div>
          </transition>
          <div v-if="currentStepChips.length" class="wizard-picked">
            <span class="wizard-picked__label">Вы выбрали:</span>
            <div class="wizard-picked__chips">
              <span v-for="chip in currentStepChips" :key="chip" class="wizard-picked__chip">{{ chip }}</span>
            </div>
          </div>
        </div>

        <div class="wizard-actions ds-btn-group--toolbar">
          <button
            type="button"
            class="ds-btn ds-btn--secondary"
            :disabled="currentStep === 1 || loading"
            @click="prevStep"
          >
            Назад
          </button>
          <div class="wizard-actions__right ds-btn-group">
            <button type="button" class="ds-btn ds-btn--ghost" :disabled="loading" @click="resetWizard">Сбросить</button>
            <button
              v-if="currentStep < steps.length"
              type="button"
              class="ds-btn ds-btn--primary"
              :disabled="loading"
              @click="nextStep"
            >
              Далее
            </button>
            <button
              v-else
              type="button"
              class="ds-btn ds-btn--primary"
              :disabled="loading || !canRunRecommendations"
              @click="() => runRecommendations()"
            >
              Подобрать
            </button>
          </div>
        </div>
      </section>

      <section class="panel ds-panel ds-reveal ds-reveal-delay-2">
        <div class="result-head">
          <h2>Результат</h2>
          <span>{{ recommendedEvents.length }} из {{ filteredPoolCount }}</span>
        </div>

        <div v-if="!hasWizardInput" class="state">
          Заполните анкету и нажмите «Подобрать», чтобы увидеть результат.
        </div>
        <div v-else-if="loading" class="skeleton-grid" aria-label="Загрузка рекомендаций">
          <div v-for="n in 3" :key="'reco-skeleton-' + n" class="skeleton-card">
            <div class="skeleton-block skeleton-block--media"></div>
            <div class="skeleton-card__body">
              <div class="skeleton-block skeleton-block--line skeleton-block--w60"></div>
              <div class="skeleton-block skeleton-block--line skeleton-block--w90"></div>
              <div class="skeleton-block skeleton-block--line skeleton-block--w75"></div>
              <div class="skeleton-block skeleton-block--line skeleton-block--w40"></div>
            </div>
          </div>
        </div>
        <div v-else-if="rankError" class="state state--error">{{ rankError }}</div>
        <div v-else-if="recommendedEvents.length === 0 && filteredPoolCount === 0" class="state">
          Выберите мероприятия в афише, а в этом подборе ответьте на вопросы анкеты и нажмите «Подобрать» — так появятся рекомендации именно для вас.
        </div>
        <div v-else-if="recommendedEvents.length === 0" class="state">
          Не удалось составить список. Попробуйте смягчить строгость подбора или сбросить фильтры.
        </div>

        <transition-group v-else name="reco-list" tag="div" class="grid">
          <article
            v-for="eventItem in recommendedEvents"
            :key="'e-' + eventItem.id"
            class="card ds-card-pro reco-card"
          >
            <div class="poster ds-media-zoom">
              <img v-if="getEventImage(eventItem)" :src="getEventImage(eventItem)" :alt="eventItem.title" />
              <div v-else class="poster-fallback">{{ eventItem.title?.charAt(0) || 'С' }}</div>
            </div>
            <div class="content">
              <div class="chips-inline">
                <span>{{ eventItem.category?.name || 'Категория' }}</span>
                <span>{{ eventItem.ageRating }}</span>
              </div>
              <h3>{{ eventItem.title }}</h3>
              <p class="reason">{{ eventItem.__reason }}</p>
              <p class="meta">{{ formatDate(eventItem.eventDate) }} • {{ formatTime(eventItem.eventTime) }}</p>
              <p class="meta">{{ eventItem.venue?.name }}</p>
              <p class="price">{{ formatEventPrice(eventItem.price) }}</p>
            </div>
            <footer class="reco-card-footer">
              <router-link :to="`/events/${eventItem.id}`" class="ds-btn ds-btn--secondary ds-btn--block">Подробнее</router-link>
            </footer>
          </article>
        </transition-group>
      </section>
    </div>
  </div>
</template>

<script>
import { getRecommendationCategories, runWizardRecommendations } from '@/services/recommendationsService'
import { resolveEventImageUrl } from '@/utils/imageResolver'
import { formatEventPrice } from '@/utils/ticketFormat'

export default {
  name: 'RecommendationsView',
  data() {
    return {
      steps: [
        { id: 1, label: 'Компания' },
        { id: 2, label: 'Предпочтения' },
        { id: 3, label: 'Фильтры' },
        { id: 4, label: 'Уточнение' }
      ],
      currentStep: 1,
      categories: [],
      loading: false,
      recommendedEvents: [],
      filteredPoolCount: 0,
      rankError: '',
      _wizardRankSeq: 0,
      wizardOptions: {
        company: [
          { value: 'solo', label: 'Одна' },
          { value: 'friends', label: 'С друзьями' },
          { value: 'family', label: 'С семьёй' },
          { value: 'date', label: 'На свидание' }
        ],
        format: [
          { value: 'indoor', label: 'В помещении' },
          { value: 'outdoor', label: 'На улице' }
        ],
        timeOfDay: [
          { value: 'morning', label: 'Утро' },
          { value: 'day', label: 'День' },
          { value: 'evening', label: 'Вечер' }
        ],
        energy: [
          { value: 'calm', label: 'Спокойно' },
          { value: 'balanced', label: 'Сбалансированно' },
          { value: 'active', label: 'Активно' }
        ],
        agePreference: [
          { value: 'kids', label: '0+ / 6+' },
          { value: 'teen', label: '12+' },
          { value: 'adult', label: '16+ / 18+' }
        ]
      },
      answers: {
        query: '',
        company: [],
        format: [],
        timeOfDay: [],
        energy: [],
        agePreference: [],
        categoryIds: [],
        minPrice: null,
        maxPrice: null,
        dateFrom: '',
        dateTo: '',
        strictness: 'balanced',
        resultLimit: 9,
        diversify: true
      }
    }
  },
  computed: {
    hasWizardInput() {
      const hasQuery = !!(this.answers.query && this.answers.query.trim())
      const hasListSelections =
        this.answers.company.length > 0 ||
        this.answers.format.length > 0 ||
        this.answers.timeOfDay.length > 0 ||
        this.answers.energy.length > 0 ||
        this.answers.agePreference.length > 0 ||
        this.answers.categoryIds.length > 0
      const hasPrice = this.answers.minPrice != null || this.answers.maxPrice != null
      const hasDate = !!this.answers.dateFrom || !!this.answers.dateTo
      return hasQuery || hasListSelections || hasPrice || hasDate
    },
    canRunRecommendations() {
      return this.hasWizardInput
    },
    progressPercent() {
      return (this.currentStep / Math.max(1, this.steps.length)) * 100
    },
    currentStepChips() {
      if (this.currentStep === 1) {
        return this.labelSelectedOptions('company')
      }
      if (this.currentStep === 2) {
        return [
          ...this.labelSelectedOptions('format', 'Формат: '),
          ...this.labelSelectedOptions('timeOfDay', 'Время: '),
          ...this.labelSelectedOptions('energy', 'Ритм: '),
          ...this.labelSelectedOptions('agePreference', 'Возраст: ')
        ]
      }
      if (this.currentStep === 3) {
        const byId = new Map((this.categories || []).map((c) => [Number(c.id), c.name]))
        const categoryChips = (this.answers.categoryIds || [])
          .map((id) => byId.get(Number(id)))
          .filter(Boolean)
          .map((name) => `Категория: ${name}`)
        const priceChips = []
        if (this.answers.minPrice != null && this.answers.minPrice !== '')
          priceChips.push(`Мин. бюджет: ${this.answers.minPrice} ₽`)
        if (this.answers.maxPrice != null && this.answers.maxPrice !== '')
          priceChips.push(`Макс. бюджет: ${this.answers.maxPrice} ₽`)
        const dateChips = []
        if (this.answers.dateFrom) dateChips.push(`С даты: ${this.answers.dateFrom}`)
        if (this.answers.dateTo) dateChips.push(`До даты: ${this.answers.dateTo}`)
        return [...categoryChips, ...priceChips, ...dateChips]
      }
      if (this.currentStep === 4) {
        const q = (this.answers.query || '').trim()
        if (!q) return ['Без текстового уточнения']
        return [q.length > 56 ? `${q.slice(0, 56)}...` : q]
      }
      return []
    }
  },
  async mounted() {
    await this.loadCategories()
  },
  methods: {
    formatEventPrice,
    mapWizardResponseRow(row) {
      const inner = row?.eventItem ?? row?.EventItem ?? row?.Event ?? row
      const rawId = inner?.id ?? inner?.Id
      const id = Number(rawId)
      const rest = inner && typeof inner === 'object' ? { ...inner } : {}
      delete rest.id
      delete rest.Id
      return {
        ...rest,
        id: Number.isFinite(id) ? id : null,
        __reason: row?.reason ?? row?.Reason ?? ''
      }
    },
    isWizardOptionSelected(field, value) {
      const selected = this.answers[field]
      return Array.isArray(selected) && selected.includes(value)
    },
    toggleWizardOption(field, value) {
      const selected = Array.isArray(this.answers[field]) ? [...this.answers[field]] : []
      const index = selected.indexOf(value)
      if (index >= 0) selected.splice(index, 1)
      else selected.push(value)
      this.answers[field] = selected
    },
    async loadCategories() {
      this.categories = await getRecommendationCategories()
    },
    buildRequestPayload() {
      let minP = this.answers.minPrice
      let maxP = this.answers.maxPrice
      let minPrice =
        typeof minP === 'number' && !Number.isNaN(minP) && minP > 0 ? minP : null
      let maxPrice =
        typeof maxP === 'number' && !Number.isNaN(maxP) && maxP > 0 ? maxP : null
      if (minPrice != null && maxPrice != null && minPrice > maxPrice)
        [minPrice, maxPrice] = [maxPrice, minPrice]
      return {
        query: this.answers.query || null,
        company: this.answers.company || [],
        format: this.answers.format || [],
        timeOfDay: this.answers.timeOfDay || [],
        energy: this.answers.energy || [],
        agePreference: this.answers.agePreference || [],
        categoryIds: (this.answers.categoryIds || [])
          .map((id) => Number(id))
          .filter((id) => Number.isFinite(id) && id > 0),
        minPrice,
        maxPrice,
        dateFrom: this.answers.dateFrom || null,
        dateTo: this.answers.dateTo || null,
        strictness: this.answers.strictness || 'balanced',
        resultLimit: this.answers.resultLimit || 9,
        diversify: this.answers.diversify !== false
      }
    },
    async runRecommendations() {
      if (!this.hasWizardInput) {
        this.rankError = ''
        this.filteredPoolCount = 0
        this.recommendedEvents = []
        return
      }
      const seq = ++this._wizardRankSeq
      this.loading = true
      this.rankError = ''
      try {
        const data = await runWizardRecommendations(this.buildRequestPayload(), 120000)
        if (seq !== this._wizardRankSeq) return
        this.filteredPoolCount = data?.filteredPoolCount ?? 0
        const rows = data?.events || []
        const mapped = rows.map((row) => this.mapWizardResponseRow(row)).filter((e) => e.id != null)
        const seenIds = new Set()
        this.recommendedEvents = mapped.filter((e) => {
          if (seenIds.has(e.id)) return false
          seenIds.add(e.id)
          return true
        })
      } catch (e) {
        console.error(e)
        if (seq !== this._wizardRankSeq) return
        this.filteredPoolCount = 0
        this.recommendedEvents = []
        if (e.code === 'ECONNABORTED') {
          this.rankError =
            'Сервер отвечает слишком долго (таймаут). Подождите и обновите страницу или проверьте, что API запущен.'
        } else if (e.code === 'ECONNREFUSED' || !e.response) {
          this.rankError =
            'Не удаётся связаться с API (localhost:5001). Запустите бэкенд: в папке backend/CityLeisure.Api выполните dotnet run.'
        } else {
          const d = e.response?.data
          this.rankError =
            (d && (d.message || d.error || (typeof d === 'string' ? d : ''))) ||
            `Ошибка сервера (${e.response?.status || '?' }). Откройте консоль браузера (F12) для подробностей.`
        }
      } finally {
        if (seq !== this._wizardRankSeq) return
        this.loading = false
      }
    },
    nextStep() {
      if (this.currentStep < this.steps.length) this.currentStep += 1
    },
    prevStep() {
      if (this.currentStep > 1) this.currentStep -= 1
    },
    goToStep(stepId) {
      const id = Number(stepId)
      if (!Number.isFinite(id)) return
      if (id < 1 || id > this.steps.length) return
      this.currentStep = id
    },
    labelSelectedOptions(field, prefix = '') {
      const selected = Array.isArray(this.answers[field]) ? this.answers[field] : []
      const options = Array.isArray(this.wizardOptions[field]) ? this.wizardOptions[field] : []
      const labelByValue = new Map(options.map((o) => [o.value, o.label]))
      return selected
        .map((value) => labelByValue.get(value))
        .filter(Boolean)
        .map((label) => `${prefix}${label}`)
    },
    resetWizard() {
      this.currentStep = 1
      this.answers = {
        query: '',
        company: [],
        format: [],
        timeOfDay: [],
        energy: [],
        agePreference: [],
        categoryIds: [],
        minPrice: null,
        maxPrice: null,
        dateFrom: '',
        dateTo: '',
        strictness: 'balanced',
        resultLimit: 9,
        diversify: true
      }
      this.runRecommendations()
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long' })
    },
    formatTime(time) {
      return (time || '').slice(0, 5)
    },
    getEventImage(eventItem) {
      return resolveEventImageUrl(eventItem?.imageUrl, eventItem?.id)
    }
  }
}
</script>

<style scoped>
.top { margin-bottom: var(--space-4); }
.top h1 {
  margin: 0 0 var(--space-2);
  font-size: clamp(1.35rem, 3vw, 1.75rem);
  animation: rise-in 420ms ease both;
}
.top p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9375rem;
  max-width: 62ch;
  line-height: 1.55;
  animation: rise-in 520ms ease both;
}
.panel {
  margin-bottom: var(--space-4);
}
.wizard { display: grid; gap: var(--space-4); }
.steps { display: flex; flex-wrap: wrap; gap: var(--space-2); }
.step {
  border: 1px solid var(--border);
  border-radius: 999px;
  padding: var(--space-1) var(--space-3);
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
  background: var(--surface);
  transition: transform 0.18s ease, background-color 0.18s ease, border-color 0.18s ease;
  cursor: pointer;
}
.step.active { color: var(--primary); border-color: rgba(15, 118, 110, 0.35); background: var(--primary-muted); }
.step.active { transform: translateY(-1px); }
.wizard-progress {
  position: relative;
  height: 6px;
  border-radius: 999px;
  overflow: hidden;
  background: rgba(148, 163, 184, 0.2);
}
.wizard-progress__bar {
  display: block;
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, #0f766e, #2563eb);
  transition: width 0.25s ease;
}
.question-box {
  background: var(--surface-muted);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: clamp(var(--space-4), 2.5vw, var(--space-5));
  overflow: hidden;
}
.wizard-picked {
  margin-top: var(--space-3);
  display: grid;
  gap: var(--space-2);
}
.wizard-picked__label {
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--text-secondary);
}
.wizard-picked__chips {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}
.wizard-picked__chip {
  font-size: 0.75rem;
  font-weight: 600;
  padding: var(--space-1) var(--space-2);
  border-radius: 999px;
  border: 1px solid rgba(15, 118, 110, 0.28);
  color: var(--primary);
  background: rgba(15, 118, 110, 0.1);
}
.question-content { will-change: transform, opacity; }
.question-box h3 { margin: 0 0 var(--space-2); font-size: 1.0625rem; }
.question-box .hint { color: var(--text-secondary); font-size: 0.875rem; line-height: 1.5; }
.wizard-hint {
  margin: 0 0 var(--space-3);
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.5;
  max-width: 58ch;
}
.wizard-hint--tight { margin-bottom: var(--space-2); }
.wizard-label {
  display: grid;
  gap: var(--space-2);
  align-content: start;
}
.wizard-label > span {
  color: var(--text-secondary);
  font-size: 0.875rem;
  font-weight: 600;
}
.wizard input:not([type="checkbox"]),
.wizard select,
.wizard textarea {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: var(--space-3) var(--space-3);
  background: var(--surface);
  width: 100%;
  max-width: 100%;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
  min-height: 48px;
  font-size: 1rem;
  line-height: 1.4;
}
.wizard textarea.wizard-textarea {
  min-height: 7.5rem;
  resize: vertical;
  line-height: 1.5;
}
.wizard input:focus,
.wizard select:focus,
.wizard textarea:focus {
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}
.wizard-prefs-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-4);
}
@media (min-width: 640px) {
  .wizard-prefs-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
.wizard-prefs-grid--three {
  grid-template-columns: 1fr;
}
@media (min-width: 900px) {
  .wizard-prefs-grid--three { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}
.wizard-filters { display: flex; flex-direction: column; gap: var(--space-5); }
.wizard-field__label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
}
.category-checkboxes {
  max-height: 240px;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--surface);
  padding: var(--space-1) 0;
  -webkit-overflow-scrolling: touch;
}
.wizard-choice-grid {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--surface);
  padding: var(--space-1) 0;
}
.check-row {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-2) var(--space-3);
  cursor: pointer;
  font-size: 0.9375rem;
  min-height: 44px;
  transition: background-color 0.18s ease, transform 0.18s ease;
}
.check-row:hover {
  background: var(--surface-muted);
  transform: translateX(2px);
}
.check-row input[type="checkbox"] {
  width: 1.25rem;
  height: 1.25rem;
  margin: 0;
  flex-shrink: 0;
  min-height: 0;
  padding: 0;
}
.wizard-row-2 {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-4);
}
@media (min-width: 560px) {
  .wizard-row-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
.wizard-actions { margin-top: var(--space-2); }
.wizard-actions__right { flex-wrap: wrap; justify-content: flex-end; }
.result-head { display: flex; align-items: baseline; justify-content: space-between; margin-bottom: var(--space-4); gap: var(--space-3); }
.result-head h2 { margin: 0; font-size: 1.125rem; }
.result-head span, .state { color: var(--text-secondary); font-size: 0.9375rem; }
.state--error { color: var(--danger, #b91c1c); }
.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: var(--space-4); }
.grid .reco-card.ds-card-pro,
.grid .reco-card.ds-card-pro:hover {
  transform: none;
  transition: box-shadow 0.22s ease, border-color 0.22s ease;
}
.grid .reco-card.ds-card-pro:hover {
  box-shadow: var(--shadow-elevated-hover);
  border-color: #d1d5db;
  transform: perspective(900px) translateY(-1px) rotateX(1deg) rotateY(-1deg);
}
.card {
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-height: 380px;
  transition: box-shadow 0.22s ease, border-color 0.22s ease;
  position: relative;
  isolation: isolate;
}
.poster { height: 160px; background: var(--surface-muted); }
.poster img { width: 100%; height: 100%; object-fit: cover; }
.poster-fallback { height: 100%; display: flex; align-items: center; justify-content: center; color: var(--text-secondary); font-size: 2rem; font-weight: 700; }
.content { padding: var(--space-4); display: flex; flex-direction: column; gap: var(--space-2); flex: 1; }
.chips-inline { display: flex; flex-wrap: wrap; gap: var(--space-2); }
.chips-inline span { font-size: 0.75rem; font-weight: 600; padding: var(--space-1) var(--space-2); border-radius: 999px; border: 1px solid var(--border); }
.chips-inline span:first-of-type { background: var(--accent-warm-muted); color: #b45309; border-color: #fde68a; }
.chips-inline span:last-of-type { background: var(--surface-muted); color: var(--text-secondary); }
.content h3 { font-size: 1.0625rem; line-height: 1.35; margin: 0; min-height: 2.7em; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.reason { font-size: 0.8125rem; color: var(--text-secondary); background: var(--surface-muted); border: 1px solid var(--border); border-radius: var(--radius-sm); padding: var(--space-2) var(--space-3); line-height: 1.45; }
.meta { color: var(--text-secondary); font-size: 0.875rem; margin: 0; }
.price { color: var(--primary); font-weight: 700; font-size: 1.0625rem; margin: 0; }
.reco-card-footer {
  margin-top: auto;
  padding: var(--space-4);
  border-top: 1px solid var(--border);
  background: var(--surface);
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
  height: 160px;
  border-radius: 0;
}
.skeleton-block--line {
  height: 12px;
}
.skeleton-block--w90 { width: 90%; }
.skeleton-block--w75 { width: 75%; }
.skeleton-block--w60 { width: 60%; }
.skeleton-block--w40 { width: 40%; }

.recommendations :deep(.ds-btn) {
  position: relative;
  overflow: hidden;
}

.recommendations :deep(.ds-btn)::after {
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

.recommendations :deep(.ds-btn):active::after {
  animation: reco-ripple 420ms ease-out;
}

.wizard-step-enter-active,
.wizard-step-leave-active {
  transition: opacity 0.22s ease, transform 0.22s ease;
}
.wizard-step-enter-from,
.wizard-step-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

.reco-list-enter-active {
  animation: card-in 320ms ease both;
}
.reco-list-move {
  transition: transform 0.28s ease;
}

@keyframes rise-in {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes card-in {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.985);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
@keyframes reco-ripple {
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
  .top h1,
  .top p,
  .reco-list-enter-active {
    animation: none;
  }
  .wizard-step-enter-active,
  .wizard-step-leave-active,
  .reco-list-move,
  .step,
  .wizard-progress__bar,
  .check-row,
  .card,
  .recommendations :deep(.ds-btn)::after {
    transition: none;
  }
}
@media (max-width: 680px) {
  .wizard-actions.ds-btn-group--toolbar { flex-direction: column; align-items: stretch; }
  .wizard-actions__right { justify-content: stretch; }
  .wizard-actions__right .ds-btn { flex: 1; }
}
</style>

