<template>
  <div class="status-chips" v-if="chips.length">
    <span v-for="c in chips" :key="c.key" :class="['status-chip', `status-chip--${c.tone}`]">{{ c.label }}</span>
  </div>
</template>

<script>
function startOfDay(d) {
  const x = new Date(d)
  x.setHours(0, 0, 0, 0)
  return x
}

export default {
  name: 'EventStatusChips',
  props: {
    eventItem: { type: Object, required: true }
  },
  computed: {
    chips() {
      const e = this.eventItem
      const out = []
      const avail = Number(e.availableTickets)
      const day = startOfDay(new Date())
      const evDay = startOfDay(new Date(e.eventDate))
      const diffDays = Math.round((evDay - day) / (24 * 60 * 60 * 1000))

      if (avail === 0) out.push({ key: 'sold', label: 'Sold out', tone: 'sold' })
      else if (avail > 0 && avail <= 15) out.push({ key: 'low', label: 'Мало мест', tone: 'low' })
      if (diffDays >= 0 && diffDays <= 2 && evDay >= day) out.push({ key: 'soon', label: 'Скоро', tone: 'soon' })

      return out
    }
  }
}
</script>

<style scoped>
.status-chips {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.status-chip {
  font-size: 0.6875rem;
  font-weight: 700;
  padding: 2px var(--space-2);
  border-radius: 999px;
  border: 1px solid var(--border);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.status-chip--free {
  background: var(--primary-muted);
  color: var(--primary);
  border-color: rgba(15, 118, 110, 0.35);
}

.status-chip--sold {
  background: #f4f4f5;
  color: #52525b;
  border-color: #d4d4d8;
}

.status-chip--low {
  background: #fffbeb;
  color: #b45309;
  border-color: #fde68a;
}

.status-chip--soon {
  background: #eff6ff;
  color: #1d4ed8;
  border-color: #bfdbfe;
}
</style>
