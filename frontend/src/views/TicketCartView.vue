<template>
  <div class="cart-page">
    <div class="container">
      <h1 class="ds-reveal">Корзина билетов</h1>
      <div v-if="loading" class="state">Загрузка...</div>
      <div v-else-if="cartItems.length === 0" class="state">
        Корзина пуста. <router-link to="/events">Открыть афишу</router-link>
      </div>
      <div v-else class="layout ds-reveal ds-reveal-delay-1">
        <div class="list">
          <div v-for="item in cartItems" :key="item.id" class="row ds-card-pro">
            <div class="info">
              <h3>{{ item.event?.title }}</h3>
              <p class="venue-line">{{ item.event?.venueName }}</p>
              <p v-if="item.event?.venueAddress" class="meta meta--addr">{{ item.event.venueAddress }}</p>
              <p class="meta">{{ formatDate(item.event?.eventDate) }} {{ formatTime(item.event?.eventTime) }}</p>
              <p v-if="item.selectedSeats?.length && eventUsesSeatSelection(item.event)" class="seats">
                Места: {{ item.selectedSeats.join(', ') }}
              </p>
            </div>
            <div v-if="!eventUsesSeatSelection(item.event)" class="controls">
              <button @click="updateQuantity(item.id, item.quantity - 1)" :disabled="item.quantity <= 1">−</button>
              <span>{{ item.quantity }}</span>
              <button @click="updateQuantity(item.id, item.quantity + 1)">+</button>
            </div>
            <div v-else class="qty-assigned">{{ item.quantity }} шт.</div>
            <div class="total">{{ lineTotal(item) }}</div>
            <button type="button" class="ds-btn ds-btn--ghost remove" @click="removeItem(item.id)">Удалить</button>
          </div>
        </div>
        <aside class="summary ds-panel">
          <h2 class="summary-title">Заказ</h2>
          <p class="summary-line">Билетов: <strong>{{ totalItems }}</strong></p>
          <p class="summary-total">Итого: <strong>{{ moneyTotal }}</strong></p>
          <div class="summary-actions">
            <button type="button" class="ds-btn ds-btn--ghost ds-btn--block" @click="clearCart">Очистить корзину</button>
            <button type="button" class="ds-btn ds-btn--primary ds-btn--block" @click="checkout">Перейти к оплате</button>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<script>
import { clearCartItems, getCartItems, removeCartItem, updateCartItem } from '@/services/cartService'
import { formatEventPrice } from '@/utils/ticketFormat'
import { eventUsesSeatSelection } from '@/utils/seating'

export default {
  name: 'TicketCartView',
  data() {
    return { cartItems: [], loading: true }
  },
  computed: {
    totalItems() {
      return this.cartItems.reduce((sum, item) => sum + item.quantity, 0)
    },
    totalAmount() {
      return this.cartItems.reduce((sum, item) => sum + (item.event?.price || 0) * item.quantity, 0)
    },
    moneyTotal() {
      return formatEventPrice(this.totalAmount)
    }
  },
  async mounted() {
    await this.loadCart()
  },
  methods: {
    eventUsesSeatSelection,
    async loadCart() {
      this.loading = true
      this.cartItems = await getCartItems()
      this.loading = false
    },
    async updateQuantity(itemId, newQuantity) {
      if (newQuantity < 1) return
      await updateCartItem(itemId, { eventId: 0, quantity: newQuantity })
      await this.loadCart()
    },
    async removeItem(itemId) {
      await removeCartItem(itemId)
      await this.loadCart()
    },
    async clearCart() {
      await clearCartItems()
      await this.loadCart()
    },
    checkout() {
      this.$router.push('/checkout')
    },
    formatPrice(price) {
      return new Intl.NumberFormat('ru-RU').format(price)
    },
    lineTotal(item) {
      return formatEventPrice((item.event?.price || 0) * item.quantity)
    },
    formatDate(date) {
      return date ? new Date(date).toLocaleDateString('ru-RU') : ''
    },
    formatTime(time) {
      return (time || '').slice(0, 5)
    }
  }
}
</script>

<style scoped>
.cart-page {
  min-height: calc(100vh - 140px);
  padding-bottom: var(--space-6);
}

.cart-page h1 {
  margin: 0 0 var(--space-5);
  font-size: clamp(1.5rem, 3vw, 2rem);
}

.layout {
  display: grid;
  grid-template-columns: 1fr minmax(260px, 300px);
  gap: var(--space-5);
  align-items: start;
}

.list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.row {
  padding: var(--space-4);
  display: grid;
  grid-template-columns: 1fr auto auto minmax(100px, auto);
  gap: var(--space-4);
  align-items: center;
}

.info h3 {
  font-size: 1rem;
  line-height: 1.35;
  margin: 0 0 var(--space-1);
}

.venue-line {
  margin: 0 0 2px;
  font-weight: 700;
  font-size: 0.9375rem;
}

.meta--addr {
  margin-bottom: var(--space-2);
}

.info p {
  margin: 0;
}

.meta {
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.seats {
  margin: var(--space-2) 0 0;
  font-size: 0.8125rem;
  color: var(--text-secondary);
}

.qty-assigned {
  font-weight: 700;
  font-size: 0.9375rem;
  color: var(--text-primary);
  justify-self: center;
}

.controls {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: var(--space-1);
  background: var(--surface-muted);
}

.controls button {
  width: 36px;
  height: 36px;
  border: none;
  background: var(--surface);
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.controls button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.controls span {
  min-width: 28px;
  text-align: center;
  font-weight: 700;
  font-size: 0.9375rem;
}

.total {
  font-weight: 700;
  font-size: 1.0625rem;
  color: var(--primary);
  justify-self: end;
}

.remove {
  justify-self: end;
}

.summary {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  height: fit-content;
  position: sticky;
  top: calc(var(--space-5) + 56px);
}

.summary-title {
  margin: 0;
  font-size: 1.0625rem;
}

.summary-line,
.summary-total {
  margin: 0;
  font-size: 0.9375rem;
  color: var(--text-secondary);
}

.summary-total {
  font-size: 1.125rem;
  color: var(--text-primary);
}

.summary-total strong {
  color: var(--primary);
}

.summary-actions {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-top: var(--space-2);
}

.state {
  text-align: center;
  color: var(--text-secondary);
  padding: var(--space-6);
  font-size: 0.9375rem;
}

@media (max-width: 960px) {
  .layout {
    grid-template-columns: 1fr;
  }

  .summary {
    position: static;
  }

  .row {
    grid-template-columns: 1fr;
    justify-items: stretch;
  }

  .total,
  .remove {
    justify-self: stretch;
  }

  .controls {
    justify-content: center;
  }
}
</style>

