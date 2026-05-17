<template>
  <div class="tickets-page">
    <div class="container">
      <h1 class="ds-reveal">Мои билеты</h1>
      <div v-if="loading" class="state">Загрузка...</div>
      <div v-else-if="orders.length === 0" class="state">Пока нет купленных билетов</div>
      <div v-else class="orders">
        <article v-for="order in orders" :key="order.id" class="order-block ds-reveal">
          <header class="order-head">
            <div>
              <span class="order-id">Заказ №{{ order.id }}</span>
              <span class="order-meta">{{ formatDate(order.createdAt) }}</span>
            </div>
            <span class="order-sum">Итого: <strong>{{ orderMoney(order) }}</strong></span>
          </header>

          <div class="etickets">
            <div v-for="item in order.orderItems" :key="item.id" class="eticket-wrap">
              <div
                v-for="n in Math.max(1, item.quantity)"
                :key="`${item.id}-${n}`"
                class="eticket"
                :class="{ 'eticket--multi': item.quantity > 1 }"
              >
                <div class="eticket__stub" aria-hidden="true" />
                <div class="eticket__body">
                  <div class="eticket__brand">CityLeisure</div>
                  <p class="eticket__title">{{ item.event?.title || 'Мероприятие' }}</p>
                  <div class="eticket__row">
                    <span class="eticket__k">Дата и время</span>
                    <span class="eticket__v">{{ formatEventWhen(item) }}</span>
                  </div>
                  <div class="eticket__row">
                    <span class="eticket__k">Площадка</span>
                    <span class="eticket__v">{{ item.event?.venueName || '—' }}</span>
                  </div>
                  <div v-if="item.event?.venueAddress" class="eticket__row">
                    <span class="eticket__k">Адрес</span>
                    <span class="eticket__v">{{ item.event.venueAddress }}</span>
                  </div>
                  <div v-if="seatLine(item, n)" class="eticket__row">
                    <span class="eticket__k">Место</span>
                    <span class="eticket__v">{{ seatLine(item, n) }}</span>
                  </div>
                  <div class="eticket__row eticket__row--status">
                    <span class="eticket__k">Статус заказа</span>
                    <span class="eticket__status" :class="'eticket__status--' + statusTone(order.status)">{{ statusLabel(order.status) }}</span>
                  </div>
                  <div v-if="item.quantity > 1" class="eticket__hint">Билет {{ n }} из {{ item.quantity }}</div>
                </div>
              </div>
            </div>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script>
import { getUserOrders } from '@/services/ordersService'
import { formatEventPrice } from '@/utils/ticketFormat'
export default {
  name: 'MyTicketsView',
  data() {
    return { orders: [], loading: true }
  },
  async mounted() {
    try {
      this.orders = await getUserOrders()
    } finally {
      this.loading = false
    }
  },
  methods: {
    unitLabel(item) {
      return formatEventPrice(item.price)
    },
    orderMoney(order) {
      return formatEventPrice(order.totalAmount)
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long', year: 'numeric' })
    },
    formatEventWhen(item) {
      const ev = item.event
      if (!ev?.eventDate) return '—'
      const d = new Date(ev.eventDate).toLocaleDateString('ru-RU', { weekday: 'short', day: 'numeric', month: 'long', year: 'numeric' })
      const t = (ev.eventTime || '').toString().slice(0, 5)
      return t ? `${d}, ${t}` : d
    },
    seatLine(item, indexInQty) {
      const labels = item.seatLabels
      if (!labels?.length) return ''
      if (labels.length === 1) return labels[0]
      return labels[indexInQty - 1] || labels[0]
    },
    statusLabel(s) {
      const m = {
        pending: 'Ожидает оплаты',
        paid: 'Оплачен',
        completed: 'Завершён',
        cancelled: 'Отменён'
      }
      return m[String(s || '').toLowerCase()] || s || '—'
    },
    statusTone(s) {
      const k = String(s || '').toLowerCase()
      if (k === 'paid' || k === 'completed') return 'ok'
      if (k === 'cancelled') return 'bad'
      return 'wait'
    }
  }
}
</script>

<style scoped>
.tickets-page {
  min-height: calc(100vh - 140px);
  padding-bottom: var(--space-6);
}

.tickets-page h1 {
  margin: 0 0 var(--space-5);
  font-size: clamp(1.5rem, 3vw, 2rem);
}

.state {
  text-align: center;
  color: var(--text-secondary);
  padding: var(--space-6);
  font-size: 0.9375rem;
}

.orders {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.order-block {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--surface);
  padding: var(--space-4);
}

.order-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-4);
  flex-wrap: wrap;
  margin-bottom: var(--space-4);
  padding-bottom: var(--space-3);
  border-bottom: 1px solid var(--border);
}

.order-id {
  font-weight: 700;
  font-size: 1rem;
  display: block;
}

.order-meta {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.order-sum {
  font-size: 0.9375rem;
  color: var(--text-secondary);
}

.order-sum strong {
  color: var(--primary);
  font-size: 1.0625rem;
}

.etickets {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.eticket-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.eticket {
  display: block;
  gap: 0;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #0f7668;
  background: linear-gradient(165deg, #f0fdfa 0%, #ffffff 40%, #ecfdf5 100%);
  box-shadow: 0 12px 32px -18px rgba(15, 118, 110, 0.45);
  position: relative;
}

.eticket__stub {
  position: absolute;
  left: 0;
  top: 50%;
  width: 14px;
  height: 14px;
  margin-top: -7px;
  margin-left: -7px;
  border-radius: 50%;
  background: var(--surface);
  border: 1px solid #0f7668;
  z-index: 1;
}

.eticket__body {
  padding: var(--space-4) var(--space-5);
  min-width: 0;
  position: relative;
}

.eticket__body::after {
  content: '';
  position: absolute;
  top: 10px;
  bottom: 10px;
  right: 0;
  width: 2px;
  background: repeating-linear-gradient(
    to bottom,
    rgba(15, 118, 110, 0.45) 0 6px,
    transparent 6px 11px
  );
  pointer-events: none;
}

.eticket__brand {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: #0f7668;
  margin-bottom: var(--space-2);
}

.eticket__title {
  margin: 0 0 var(--space-3);
  font-size: 1.125rem;
  font-weight: 700;
  line-height: 1.3;
  color: var(--text-primary);
}

.eticket__row {
  display: grid;
  grid-template-columns: 110px 1fr;
  gap: var(--space-2);
  font-size: 0.875rem;
  margin-bottom: var(--space-2);
}

.eticket__k {
  color: var(--text-secondary);
  font-weight: 600;
}

.eticket__v {
  color: var(--text-primary);
}

.eticket__row--status {
  margin-top: var(--space-2);
  padding-top: var(--space-2);
  border-top: 1px dashed rgba(15, 118, 110, 0.25);
}

.eticket__status {
  font-weight: 700;
  font-size: 0.8125rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.eticket__status--ok {
  color: #047857;
}

.eticket__status--wait {
  color: #b45309;
}

.eticket__status--bad {
  color: #b91c1c;
}

.eticket__hint {
  margin: var(--space-2) 0 0;
  font-size: 0.75rem;
  color: var(--text-secondary);
}

@media (max-width: 640px) {
  .eticket__row {
    grid-template-columns: 1fr;
  }
}
</style>
