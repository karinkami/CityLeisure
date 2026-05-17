<template>
  <TransitionGroup name="toast" tag="div" class="toast-container">
    <div
      v-for="toast in toasts"
      :key="toast.id"
      :class="['toast', `toast-${toast.type}`]"
    >
      <span class="toast-icon">{{ toast.icon }}</span>
      <span class="toast-message">{{ toast.message }}</span>
      <button @click="removeToast(toast.id)" class="toast-close">×</button>
    </div>
  </TransitionGroup>
</template>

<script>
export default {
  name: 'Toast',
  data() {
    return {
      toasts: []
    }
  },
  methods: {
    show(message, type = 'info', duration = 3000) {
      const icons = {
        success: '✅',
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️'
      }
      
      const toast = {
        id: Date.now() + Math.random(),
        message,
        type,
        icon: icons[type] || icons.info
      }
      
      this.toasts.push(toast)
      
      if (duration > 0) {
        setTimeout(() => {
          this.removeToast(toast.id)
        }, duration)
      }
      
      return toast.id
    },
    removeToast(id) {
      const index = this.toasts.findIndex(t => t.id === id)
      if (index > -1) {
        this.toasts.splice(index, 1)
      }
    },
    success(message, duration) {
      return this.show(message, 'success', duration)
    },
    error(message, duration) {
      return this.show(message, 'error', duration)
    },
    warning(message, duration) {
      return this.show(message, 'warning', duration)
    },
    info(message, duration) {
      return this.show(message, 'info', duration)
    }
  }
}
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: 400px;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-4);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.1);
  pointer-events: auto;
  animation: slideIn 0.3s ease-out;
  min-width: 280px;
}

.toast-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.toast-message {
  flex: 1;
  font-size: 0.9375rem;
  color: var(--text-primary);
  line-height: 1.45;
}

.toast-close {
  background: none;
  border: none;
  font-size: 1.35rem;
  line-height: 1;
  color: var(--text-secondary);
  cursor: pointer;
  padding: var(--space-1);
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  transition: background 0.15s ease, color 0.15s ease;
  flex-shrink: 0;
}

.toast-close:hover {
  background: var(--surface-muted);
  color: var(--text-primary);
}

.toast-success {
  border-left: 4px solid var(--primary);
  background: var(--surface);
}

.toast-error {
  border-left: 4px solid var(--danger);
  background: var(--surface);
}

.toast-warning {
  border-left: 4px solid var(--accent-warm);
  background: var(--surface);
}

.toast-info {
  border-left: 4px solid #9ca3af;
  background: var(--surface);
}

@keyframes slideIn {
  from {
    transform: translateX(400px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.toast-leave-active {
  transition: all 0.3s ease-in;
}

.toast-leave-to {
  transform: translateX(400px);
  opacity: 0;
}

@media (max-width: 768px) {
  .toast-container {
    right: 10px;
    left: 10px;
    max-width: none;
  }
  
  .toast {
    min-width: auto;
  }
}
</style>


