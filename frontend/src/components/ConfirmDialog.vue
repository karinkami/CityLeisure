<template>
  <Transition name="modal">
    <div v-if="visible" class="modal-overlay" @click.self="handleCancel">
      <div class="modal-dialog">
        <div class="modal-header">
          <h3>{{ title }}</h3>
        </div>
        <div class="modal-body">
          <p>{{ message }}</p>
        </div>
        <div class="modal-footer ds-btn-group ds-btn-group--end">
          <button type="button" class="ds-btn ds-btn--secondary" @click="handleCancel">Отмена</button>
          <button type="button" class="ds-btn ds-btn--primary" @click="handleConfirm">{{ confirmText }}</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script>
export default {
  name: 'ConfirmDialog',
  data() {
    return {
      visible: false,
      title: 'Подтверждение',
      message: '',
      confirmText: 'Подтвердить',
      resolve: null
    }
  },
  methods: {
    show(title, message, confirmText = 'Подтвердить') {
      this.title = title
      this.message = message
      this.confirmText = confirmText
      this.visible = true
      
      return new Promise((resolve) => {
        this.resolve = (result) => {
          this.visible = false
          resolve(result)
          setTimeout(() => {
            this.reset()
          }, 200)
        }
      })
    },
    handleConfirm() {
      if (this.resolve) {
        this.resolve(true)
      }
    },
    handleCancel() {
      if (this.resolve) {
        this.resolve(false)
      }
    },
    reset() {
      this.title = 'Подтверждение'
      this.message = ''
      this.confirmText = 'Подтвердить'
      this.resolve = null
    }
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10001;
  backdrop-filter: blur(4px);
}

.modal-dialog {
  background: var(--surface);
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  box-shadow: 0 20px 50px rgba(15, 23, 42, 0.18);
  max-width: 440px;
  width: 90%;
  max-height: 90vh;
  overflow: hidden;
  animation: scaleIn 0.2s ease-out;
}

@keyframes scaleIn {
  from {
    transform: scale(0.9);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

.modal-header {
  padding: var(--space-5);
  border-bottom: 1px solid var(--border);
}

.modal-header h3 {
  margin: 0;
  font-size: 1.125rem;
  color: var(--text-primary);
  font-weight: 700;
}

.modal-body {
  padding: var(--space-5);
}

.modal-body p {
  margin: 0;
  font-size: 0.9375rem;
  color: var(--text-secondary);
  line-height: 1.55;
}

.modal-footer {
  padding: var(--space-4) var(--space-5);
  border-top: 1px solid var(--border);
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-dialog,
.modal-leave-active .modal-dialog {
  transition: transform 0.2s;
}

.modal-enter-from .modal-dialog,
.modal-leave-to .modal-dialog {
  transform: scale(0.9);
}
</style>


