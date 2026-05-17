<template>
  <div class="checkout-page">
    <div class="container">
      <h1 class="ds-reveal">Оформление заказа билетов</h1>
      
      <div v-if="loading" class="loading">
        <div class="spinner"></div>
        <p>Загрузка...</p>
      </div>

      <div v-else-if="cartItems.length === 0" class="empty-cart ds-panel ds-reveal">
        <div class="empty-icon">🎫</div>
        <p>Корзина билетов пуста</p>
        <router-link to="/events" class="ds-btn ds-btn--primary">Открыть афишу</router-link>
      </div>

      <div v-else class="checkout-content">
        <div class="checkout-form-section ds-panel ds-reveal ds-reveal-delay-1">
          <h2>Данные покупателя</h2>
          <form @submit.prevent="submitOrder" class="checkout-form">
            <div class="form-group">
              <label>Имя *</label>
              <input 
                type="text" 
                v-model="orderForm.name" 
                required 
                placeholder="Введите ваше имя"
              />
            </div>
            <div class="form-group">
              <label>Email *</label>
              <input 
                type="email" 
                v-model="orderForm.email" 
                required 
                placeholder="Введите email"
              />
            </div>
            <div class="form-group">
              <label>Телефон *</label>
              <input 
                type="tel" 
                v-model="orderForm.phone" 
                required 
                placeholder="+7 (999) 123-45-67"
              />
            </div>
            <div class="form-group">
              <label>Комментарий</label>
              <textarea v-model="orderForm.comment" placeholder="Необязательно" rows="3"></textarea>
            </div>
            <div v-if="error" class="error-message">{{ error }}</div>
            <button type="submit" :disabled="submitting" class="ds-btn ds-btn--primary ds-btn--block">
              {{ submitting ? 'Оформление...' : 'Подтвердить покупку' }}
            </button>
          </form>
        </div>

        <div class="checkout-summary-section ds-panel ds-reveal ds-reveal-delay-2">
          <h2>Состав заказа</h2>
          <div class="order-items">
            <div v-for="item in cartItems" :key="item.id" class="order-item ds-card-pro">
              <div class="item-image ds-media-zoom">
                <img 
                  v-if="!isImageError(item.id)" 
                  :src="getEventImage(item)" 
                  :alt="item.event.title"
                  @error="setImageError(item.id)"
                />
                <div v-else class="no-image">
                  <div class="image-placeholder">
                    <span>{{ item.event?.title?.charAt(0) || '?' }}</span>
                  </div>
                </div>
              </div>
              <div class="item-details">
                <h3>{{ item.event?.title }}</h3>
                <p v-if="item.event?.venueName" class="item-venue">{{ item.event.venueName }}</p>
                <p v-if="item.event?.venueAddress" class="item-addr">{{ item.event.venueAddress }}</p>
                <div class="item-quantity-price">
                  <span>Количество: {{ item.quantity }} шт.</span>
                  <span class="item-price">{{ unitPriceLabel(item) }}</span>
                </div>
                <p v-if="item.selectedSeats?.length && eventUsesSeatSelection(item.event)" class="item-seats">
                  Места: {{ item.selectedSeats.join(', ') }}
                </p>
              </div>
              <div class="item-total">
                {{ lineMoney(item) }}
              </div>
            </div>
          </div>
          <div class="order-summary">
            <div class="summary-row">
              <span>Билетов:</span>
              <span>{{ totalItems }} шт.</span>
            </div>
            <div class="summary-row total">
              <span>Итого:</span>
              <span>{{ totalMoney }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { getStoredUser } from '@/services/session'
import { getCartItems } from '@/services/cartService'
import { createOrder } from '@/services/ordersService'
import { resolveEventImageUrl } from '@/utils/imageResolver'
import { formatEventPrice } from '@/utils/ticketFormat'
import { eventUsesSeatSelection } from '@/utils/seating'

export default {
  name: 'Checkout',
  data() {
    return {
      cartItems: [],
      loading: true,
      submitting: false,
      error: '',
      orderForm: {
        name: '',
        email: '',
        phone: '',
        comment: ''
      },
      imageErrors: new Set()
    }
  },
  computed: {
    totalAmount() {
      return this.cartItems.reduce((sum, item) => {
        return sum + (item.event?.price || 0) * item.quantity
      }, 0)
    },
    totalItems() {
      return this.cartItems.reduce((sum, item) => sum + item.quantity, 0)
    },
    totalMoney() {
      return formatEventPrice(this.totalAmount)
    }
  },
  async mounted() {
    await this.loadCart()
    this.loadUserData()
  },
  methods: {
    eventUsesSeatSelection,
    isValidPhone(phone) {
      const digits = String(phone || '').replace(/\D/g, '')
      if (digits.length === 10) return true
      if (digits.length === 11 && (digits.startsWith('7') || digits.startsWith('8'))) return true
      return false
    },
    async loadCart() {
      this.loading = true
      try {
        this.cartItems = await getCartItems()
      } catch (error) {
        console.error('Ошибка загрузки корзины:', error)
        if (error.response?.status === 401) {
          this.$router.push('/auth')
        }
      } finally {
        this.loading = false
      }
    },
    loadUserData() {
      const user = getStoredUser()
      if (user) {
        this.orderForm.name = user.userName || ''
        this.orderForm.email = user.email || ''
      }
    },
    async submitOrder() {
      this.error = ''
      this.submitting = true

      if (!this.isValidPhone(this.orderForm.phone)) {
        this.error = 'Введите корректный номер телефона'
        this.submitting = false
        return
      }

      try {
        const data = await createOrder(this.orderForm)
        
        if (data && data.id) {
          this.$root.$toast?.success(`Заказ оформлен! Номер #${data.id}`)
          this.$router.push('/my-tickets')
        } else {
          this.error = 'Неверный формат ответа от сервера'
        }
      } catch (error) {
        console.error('Ошибка оформления заказа:', error)
        if (error.response?.data?.message) {
          this.error = error.response.data.message
        } else {
          this.error = 'Ошибка оформления заказа. Попробуйте снова.'
        }
      } finally {
        this.submitting = false
      }
    },
    formatPrice(price) {
      return new Intl.NumberFormat('ru-RU').format(price)
    },
    unitPriceLabel(item) {
      return formatEventPrice(item.event?.price || 0)
    },
    lineMoney(item) {
      return formatEventPrice((item.event?.price || 0) * item.quantity)
    },
    setImageError(itemId) {
      this.imageErrors.add(itemId)
    },
    isImageError(itemId) {
      return this.imageErrors.has(itemId)
    },
    getEventImage(item) {
      return resolveEventImageUrl(item?.event?.imageUrl, item?.event?.id)
    }
  }
}
</script>

<style scoped>
.checkout-page {
  min-height: calc(100vh - 140px);
  padding-bottom: var(--space-6);
}

.checkout-page h1 {
  margin: 0 0 var(--space-5);
  font-size: clamp(1.5rem, 3vw, 2rem);
}

.checkout-form-section h2,
.checkout-summary-section h2 {
  margin: 0 0 var(--space-4);
  font-size: 1.125rem;
}

.loading {
  text-align: center;
  padding: var(--space-6);
  color: var(--text-secondary);
}

.empty-cart {
  text-align: center;
}

.empty-cart p {
  margin: 0 0 var(--space-4);
  color: var(--text-secondary);
}

.empty-icon {
  font-size: 2.5rem;
  margin-bottom: var(--space-3);
}

.checkout-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-5);
  align-items: start;
}


.checkout-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.form-group label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.form-group input,
.form-group textarea {
  min-height: 44px;
  padding: var(--space-3);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  font-size: 0.9375rem;
  font-family: inherit;
}

.form-group textarea {
  min-height: auto;
  resize: vertical;
}

.error-message {
  color: var(--danger);
  padding: var(--space-3);
  background: var(--danger-muted);
  border-radius: var(--radius-md);
  text-align: center;
  border: 1px solid #fecaca;
  font-size: 0.9375rem;
}

.order-items {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-bottom: var(--space-4);
  max-height: 360px;
  overflow-y: auto;
}

.order-item {
  display: flex;
  gap: var(--space-3);
  padding: var(--space-3);
  align-items: center;
}

.item-image {
  width: 72px;
  height: 72px;
  border-radius: var(--radius-sm);
  overflow: hidden;
  background: var(--surface-muted);
  flex-shrink: 0;
}

.item-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-image {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-placeholder {
  width: 100%;
  height: 100%;
  background: var(--primary-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  font-size: 1.2rem;
  font-weight: 700;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-details h3 {
  font-size: 0.9375rem;
  margin: 0 0 var(--space-1);
  color: var(--text-primary);
  line-height: 1.35;
}

.item-venue {
  margin: 0 0 2px;
  font-size: 0.875rem;
  font-weight: 700;
  color: var(--text-primary);
}

.item-addr {
  margin: 0 0 var(--space-2);
  font-size: 0.8125rem;
  color: var(--text-secondary);
  line-height: 1.4;
}

.item-quantity-price {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

.item-seats {
  margin: var(--space-2) 0 0;
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

.item-price {
  color: var(--accent-warm);
  font-weight: 700;
}

.item-total {
  font-size: 1rem;
  font-weight: 700;
  color: var(--primary);
  white-space: nowrap;
}

.order-summary {
  padding-top: var(--space-4);
  border-top: 1px solid var(--border);
}

.summary-row {
  display: flex;
  justify-content: space-between;
  padding: var(--space-2) 0;
  font-size: 0.9375rem;
  color: var(--text-secondary);
}

.summary-row.total {
  font-size: 1.125rem;
  font-weight: 700;
  color: var(--text-primary);
  border-top: 1px dashed var(--border);
  margin-top: var(--space-2);
  padding-top: var(--space-3);
}

.summary-row.total span:last-child {
  color: var(--primary);
}

@media (max-width: 968px) {
  .checkout-content {
    grid-template-columns: 1fr;
  }
}
</style>


