<template>
  <div class="auth-page">
    <div class="container">
      <div class="auth-bg-orbs" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>
      <div class="auth-card ds-panel ds-reveal">
        <div class="auth-tabs">
          <button 
            @click="isLogin = true" 
            :class="['tab', { active: isLogin }]"
          >
            Вход
          </button>
          <button 
            @click="isLogin = false" 
            :class="['tab', { active: !isLogin }]"
          >
            Регистрация
          </button>
        </div>

        <h1>{{ isLogin ? 'Вход в систему' : 'Создание аккаунта' }}</h1>

        <form v-if="isLogin" @submit.prevent="handleLogin">
          <div class="form-group">
            <label>Email</label>
            <input 
              type="email" 
              v-model="loginForm.email" 
              required 
              placeholder="Введите email"
            />
          </div>
          <div class="form-group">
            <label>Пароль</label>
            <input 
              type="password" 
              v-model="loginForm.password" 
              required 
              placeholder="Введите пароль"
            />
          </div>
          <div v-if="error" class="error-message">{{ error }}</div>
          <div v-if="success" class="success-message">{{ success }}</div>
          <button type="submit" :disabled="loading" class="ds-btn ds-btn--primary ds-btn--block">
            {{ loading ? 'Вход...' : 'Войти' }}
          </button>
        </form>

        <form v-else @submit.prevent="handleRegister">
          <div class="form-group">
            <label>Имя пользователя</label>
            <input 
              type="text" 
              v-model="registerForm.userName" 
              required 
              placeholder="Введите имя"
            />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input 
              type="email" 
              v-model="registerForm.email" 
              required 
              placeholder="Введите email"
            />
          </div>
          <div class="form-group">
            <label>Пароль</label>
            <input 
              type="password" 
              v-model="registerForm.password" 
              required 
              placeholder="Минимум 6 символов"
              minlength="6"
            />
          </div>
          <div class="form-group">
            <label>Подтвердите пароль</label>
            <input
              type="password"
              v-model="registerForm.confirmPassword"
              required
              placeholder="Повторите пароль"
              minlength="6"
            />
          </div>
          <div v-if="error" class="error-message">{{ error }}</div>
          <button type="submit" :disabled="loading" class="ds-btn ds-btn--primary ds-btn--block">
            {{ loading ? 'Регистрация...' : 'Зарегистрироваться' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/auth'
import { fetchCurrentUser, loginUser, registerUser } from '@/services/authService'

export default {
  name: 'Auth',
  data() {
    return {
      isLogin: true,
      loginForm: {
        email: '',
        password: ''
      },
      registerForm: {
        userName: '',
        email: '',
        password: '',
        confirmPassword: ''
      },
      error: '',
      success: '',
      loading: false
    }
  },
  watch: {
    isLogin() {
      this.error = ''
      this.success = ''
    }
  },
  methods: {
    isValidEmail(email) {
      return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || '').trim())
    },
    async handleLogin() {
      this.error = ''
      this.success = ''
      this.loading = true
      useAuthStore().clear()

      if (!this.loginForm.email || !this.loginForm.password) {
        this.error = 'Заполните все поля'
        this.loading = false
        return
      }

      if (!this.isValidEmail(this.loginForm.email)) {
        this.error = 'Введите корректный email'
        this.loading = false
        return
      }

      if (String(this.loginForm.password).length < 6) {
        this.error = 'Пароль должен содержать минимум 6 символов'
        this.loading = false
        return
      }

      try {
        const authData = await loginUser({
          email: this.loginForm.email.trim(),
          password: this.loginForm.password
        })

        if (authData && authData.token && authData.user) {
          const store = useAuthStore()
          store.setSession(authData.token, authData.user)
          try {
            const me = await fetchCurrentUser()
            store.setSession(authData.token, me)
          } catch {
          }
          if (this.$root.$checkAuth) {
            this.$root.$checkAuth()
          }
          await this.$nextTick()
          this.$router.push('/profile')
        } else {
          this.error = 'Неверный формат ответа от сервера'
        }
      } catch (error) {
        console.error('Login error:', error)
        
        if (error.response) {
          this.error = error.response.data?.message || 'Ошибка входа. Проверьте email и пароль.'
        } else if (error.request) {
          this.error = 'Не удалось подключиться к серверу. Проверьте, что backend запущен.'
        } else {
          this.error = 'Ошибка при отправке запроса. Попробуйте снова.'
        }
      } finally {
        this.loading = false
      }
    },
    async handleRegister() {
      this.error = ''
      this.loading = true

      if (!this.registerForm.userName || !this.registerForm.email || !this.registerForm.password || !this.registerForm.confirmPassword) {
        this.error = 'Заполните все поля'
        this.loading = false
        return
      }

      if (String(this.registerForm.userName).trim().length < 2) {
        this.error = 'Имя пользователя должно содержать минимум 2 символа'
        this.loading = false
        return
      }

      if (this.registerForm.password.length < 6) {
        this.error = 'Пароль должен содержать минимум 6 символов'
        this.loading = false
        return
      }

      if (this.registerForm.password !== this.registerForm.confirmPassword) {
        this.error = 'Пароли не совпадают'
        this.loading = false
        return
      }

      if (!this.isValidEmail(this.registerForm.email)) {
        this.error = 'Введите корректный email'
        this.loading = false
        return
      }

      try {
        const data = await registerUser({
          userName: this.registerForm.userName.trim(),
          email: this.registerForm.email.trim(),
          password: this.registerForm.password
        })

        if (data) {
          this.isLogin = true
          this.success = 'Регистрация успешна! Войдите в систему.'
          this.$root.$toast?.success('Аккаунт успешно создан.')
          this.registerForm = {
            userName: '',
            email: '',
            password: '',
            confirmPassword: ''
          }
        } else {
          this.error = 'Ошибка регистрации. Попробуйте снова.'
        }
      } catch (error) {
        console.error('Register error:', error)
        
        if (error.response) {
          this.error = error.response.data?.message || 'Ошибка регистрации. Попробуйте снова.'
        } else if (error.request) {
          this.error = 'Не удалось подключиться к серверу. Проверьте, что backend запущен.'
        } else {
          this.error = 'Ошибка при отправке запроса. Попробуйте снова.'
        }
      } finally {
        this.loading = false
      }
    }
  }
}
</script>

<style scoped>
.auth-page {
  min-height: calc(100vh - 140px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-6) 0;
  position: relative;
  overflow: hidden;
  background:
    radial-gradient(740px 340px at 10% 10%, rgba(14, 165, 233, 0.08), transparent 55%),
    radial-gradient(680px 320px at 92% 6%, rgba(16, 185, 129, 0.08), transparent 50%),
    var(--bg);
}

.auth-bg-orbs {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.auth-bg-orbs span {
  position: absolute;
  border-radius: 50%;
  filter: blur(1px);
  opacity: 0.5;
  animation: auth-float 10s ease-in-out infinite;
}

.auth-bg-orbs span:nth-child(1) {
  width: 220px;
  height: 220px;
  left: -70px;
  top: 16%;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.25), transparent 68%);
}

.auth-bg-orbs span:nth-child(2) {
  width: 180px;
  height: 180px;
  right: -50px;
  top: 8%;
  background: radial-gradient(circle, rgba(34, 197, 94, 0.22), transparent 68%);
  animation-delay: 1.2s;
}

.auth-bg-orbs span:nth-child(3) {
  width: 140px;
  height: 140px;
  right: 20%;
  bottom: -30px;
  background: radial-gradient(circle, rgba(245, 158, 11, 0.23), transparent 68%);
  animation-delay: 2s;
}

.auth-card {
  max-width: 440px;
  width: 100%;
  position: relative;
  z-index: 1;
  border: 1px solid rgba(15, 23, 42, 0.08);
  box-shadow: 0 28px 60px -36px rgba(2, 6, 23, 0.42);
  backdrop-filter: blur(6px);
  animation: auth-rise 460ms ease both;
}

.auth-tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-2);
  margin-bottom: var(--space-4);
}

.tab {
  padding: var(--space-3) var(--space-4);
  background: var(--surface-muted);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--text-secondary);
  transition: background 0.18s ease, border-color 0.18s ease, color 0.18s ease, transform 0.18s ease;
}

.tab:hover {
  color: var(--text-primary);
  border-color: #d1d5db;
  transform: translateY(-1px);
}

.tab.active {
  color: var(--primary);
  border-color: var(--primary);
  background: var(--primary-muted);
}

.auth-card h1 {
  text-align: center;
  margin: 0 0 var(--space-4);
  font-size: 1.25rem;
  background: linear-gradient(115deg, #0f172a 0%, #0f766e 52%, #2563eb 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
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
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;
}

.form-group input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: var(--focus-ring);
  background: #fff;
}

.error-message {
  color: var(--danger);
  margin-bottom: var(--space-3);
  padding: var(--space-3);
  background: var(--danger-muted);
  border: 1px solid #fecaca;
  border-radius: var(--radius-md);
  text-align: center;
  font-size: 0.9375rem;
}

.success-message {
  color: var(--primary);
  margin-bottom: var(--space-3);
  padding: var(--space-3);
  background: var(--primary-muted);
  border: 1px solid rgba(15, 118, 110, 0.2);
  border-radius: var(--radius-md);
  text-align: center;
  font-size: 0.9375rem;
}

@keyframes auth-rise {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.99);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes auth-float {
  0% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
  100% { transform: translateY(0); }
}

@media (prefers-reduced-motion: reduce) {
  .auth-card,
  .auth-bg-orbs span {
    animation: none;
  }
  .tab,
  .form-group input {
    transition: none;
  }
}
</style>


