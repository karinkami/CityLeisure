<template>
  <div class="profile-page">
    <div class="container">
      <h1 class="page-title ds-reveal">Личный кабинет</h1>

      <div v-if="user" class="profile-card ds-panel ds-reveal ds-reveal-delay-1">
        <div class="profile-header">
          <h2>Здравствуйте, {{ user.userName }}</h2>
        </div>

        <div v-if="!isEditing" class="profile-info">
          <div class="info-item">
            <span class="label">Имя пользователя</span>
            <span class="value">{{ user.userName }}</span>
          </div>
          <div class="info-item">
            <span class="label">Email</span>
            <span class="value">{{ user.email }}</span>
          </div>
          <div class="info-item">
            <span class="label">Дата регистрации</span>
            <span class="value">{{ formatDate(user.createdAt) }}</span>
          </div>
          <div class="profile-actions">
            <div class="profile-actions__row ds-btn-group">
              <router-link to="/my-tickets" class="ds-btn ds-btn--primary">Мои билеты</router-link>
              <router-link to="/profile/favorites" class="ds-btn ds-btn--secondary">Избранное</router-link>
              <button type="button" class="ds-btn ds-btn--secondary" @click="startEdit">Редактировать</button>
            </div>
            <router-link
              v-if="isAdmin"
              to="/admin/events"
              class="ds-btn ds-btn--secondary ds-btn--block profile-actions__admin"
            >
              Админ: мероприятия
            </router-link>
            <button type="button" class="ds-btn ds-btn--danger ds-btn--block" @click="handleLogout">Выйти из аккаунта</button>
          </div>
        </div>

        <div v-else class="edit-form">
          <h3>Редактирование профиля</h3>
          <div class="form-group">
            <label>Имя пользователя</label>
            <input type="text" v-model="editForm.userName" />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input type="email" v-model="editForm.email" />
          </div>
          <div v-if="error" class="error-message">{{ error }}</div>
          <div v-if="success" class="success-message">{{ success }}</div>
          <div class="form-actions ds-btn-group ds-btn-group--end">
            <button type="button" class="ds-btn ds-btn--ghost" @click="cancelEdit">Отмена</button>
            <button type="button" class="ds-btn ds-btn--primary" :disabled="loading" @click="saveProfile">
              {{ loading ? 'Сохранение...' : 'Сохранить' }}
            </button>
          </div>
        </div>
      </div>

      <div v-else class="loading">Загрузка...</div>
    </div>
  </div>
</template>

<script>
import { getStoredUser } from '@/services/session'
import { getCurrentUserProfile, updateCurrentUserProfile } from '@/services/profileService'
import { useAuthStore } from '@/stores/auth'

export default {
  name: 'Profile',
  data() {
    return {
      user: null,
      isEditing: false,
      editForm: {
        userName: '',
        email: ''
      },
      error: '',
      success: '',
      loading: false
    }
  },
  computed: {
    isAdmin() {
      return String(this.user?.role || '').toLowerCase() === 'admin'
    }
  },
  async mounted() {
    await this.loadUser()
  },
  methods: {
    async loadUser() {
      const user = getStoredUser()
      if (user) {
        this.user = user
        try {
          const profile = await getCurrentUserProfile()
          this.user = { ...profile, createdAt: this.user.createdAt }
          useAuthStore().setUser(this.user)
        } catch (error) {
          console.error('Ошибка загрузки данных пользователя:', error)
        }
      } else {
        this.$router.push('/auth')
      }
    },
    startEdit() {
      this.editForm.userName = this.user.userName
      this.editForm.email = this.user.email
      this.isEditing = true
      this.error = ''
      this.success = ''
    },
    cancelEdit() {
      this.isEditing = false
      this.error = ''
      this.success = ''
    },
    async saveProfile() {
      this.error = ''
      this.success = ''

      if (!this.editForm.userName || !this.editForm.email) {
        this.error = 'Все поля обязательны для заполнения'
        return
      }

      if (!this.editForm.email.includes('@')) {
        this.error = 'Введите корректный email'
        return
      }

      this.loading = true

      try {
        const profile = await updateCurrentUserProfile({
          userName: this.editForm.userName.trim(),
          email: this.editForm.email.trim()
        })

        this.user = { ...this.user, ...profile }
        useAuthStore().setUser(this.user)

        this.success = 'Профиль успешно обновлен!'
        this.isEditing = false

        try {
          const updatedUser = await getCurrentUserProfile()
          this.user = { ...updatedUser, createdAt: this.user.createdAt }
          useAuthStore().setUser(this.user)
        } catch (e) {
          console.error('Ошибка обновления данных:', e)
        }

        setTimeout(() => {
          this.success = ''
        }, 3000)
      } catch (error) {
        this.error = error.response?.data?.message || 'Ошибка обновления профиля. Попробуйте снова.'
      } finally {
        this.loading = false
      }
    },
    handleLogout() {
      useAuthStore().clear()
      this.$router.push('/')
    },
    formatDate(dateString) {
      if (!dateString) return 'Не указано'
      const date = new Date(dateString)
      return date.toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    }
  }
}
</script>

<style scoped>
.profile-page {
  min-height: calc(100vh - 140px);
  padding: var(--space-5) 0 var(--space-6);
  background:
    radial-gradient(760px 340px at 8% -6%, rgba(99, 102, 241, 0.08), transparent 55%),
    radial-gradient(720px 320px at 92% 4%, rgba(16, 185, 129, 0.08), transparent 54%),
    var(--bg);
}

.page-title {
  text-align: center;
  margin: 0 0 var(--space-5);
  font-size: clamp(1.5rem, 3vw, 2rem);
  background: linear-gradient(120deg, #0f172a 0%, #0f766e 50%, #2563eb 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  animation: profile-rise 420ms ease both;
}

.profile-card {
  max-width: 640px;
  margin: 0 auto;
  border: 1px solid rgba(15, 23, 42, 0.08);
  box-shadow: 0 28px 60px -36px rgba(2, 6, 23, 0.38);
  animation: profile-rise 520ms ease both;
}

.profile-header {
  margin-bottom: var(--space-5);
  padding-bottom: var(--space-4);
  border-bottom: 1px solid var(--border);
  position: relative;
}

.profile-header::after {
  content: '';
  position: absolute;
  left: 0;
  bottom: -1px;
  width: 140px;
  height: 2px;
  background: linear-gradient(90deg, #0f766e, #2563eb);
  border-radius: 999px;
}

.profile-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
}

.profile-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-4);
  background: var(--surface-muted);
  border-radius: var(--radius-md);
  border: 1px solid var(--border);
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.info-item:hover {
  transform: translateY(-2px);
  border-color: rgba(15, 118, 110, 0.22);
  box-shadow: 0 14px 26px -24px rgba(2, 6, 23, 0.45);
}

.label {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.value {
  color: var(--text-primary);
  font-size: 1rem;
  font-weight: 600;
  text-align: right;
}

.profile-actions {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-top: var(--space-2);
}

.profile-actions__row {
  width: 100%;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-2);
}

.profile-actions__row .ds-btn {
  width: 100%;
  min-width: 0;
  justify-content: center;
}

.profile-actions__admin {
  margin-top: var(--space-1);
}

.loading {
  text-align: center;
  padding: var(--space-6);
  font-size: 1rem;
  color: var(--text-secondary);
}

.edit-form {
  margin-top: var(--space-2);
}

.edit-form h3 {
  margin: 0 0 var(--space-4);
  font-size: 1.125rem;
}

.form-group {
  margin-bottom: var(--space-4);
}

.form-group label {
  display: block;
  margin-bottom: var(--space-2);
  color: var(--text-secondary);
  font-weight: 600;
  font-size: 0.875rem;
}

.form-group input {
  width: 100%;
  min-height: 44px;
  padding: 0 var(--space-3);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  font-size: 1rem;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
}

.form-group input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
}

.form-actions {
  margin-top: var(--space-5);
}

.error-message {
  color: var(--danger);
  margin-bottom: var(--space-3);
  padding: var(--space-3);
  background: var(--danger-muted);
  border-radius: var(--radius-md);
  border: 1px solid #fecaca;
  font-size: 0.9375rem;
}

.success-message {
  color: var(--primary);
  margin-bottom: var(--space-3);
  padding: var(--space-3);
  background: var(--primary-muted);
  border-radius: var(--radius-md);
  border: 1px solid rgba(15, 118, 110, 0.2);
  font-size: 0.9375rem;
}

@keyframes profile-rise {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 520px) {
  .profile-actions__row {
    grid-template-columns: 1fr;
  }

  .profile-actions__row .ds-btn {
    width: 100%;
  }

  .info-item {
    flex-direction: column;
    align-items: flex-start;
  }

  .value {
    text-align: left;
  }
}

@media (prefers-reduced-motion: reduce) {
  .page-title,
  .profile-card {
    animation: none;
  }
  .info-item,
  .form-group input {
    transition: none;
  }
}
</style>
