<template>
  <div id="app">
    <nav class="navbar">
      <div class="container">
        <router-link to="/" class="logo">CityLeisure</router-link>
        <div class="nav-links">
          <div class="main-nav">
            <router-link to="/">Главная</router-link>
            <router-link to="/events">Афиша</router-link>
            <router-link to="/recommendations">Подбор</router-link>
            <router-link to="/about">О проекте</router-link>
          </div>
          <div class="user-nav">
            <template v-if="isAuthenticated">
              <router-link to="/tickets-cart" class="nav-link">
                Корзина билетов
              </router-link>
              <router-link to="/profile" class="nav-link">
                Личный кабинет
              </router-link>
            </template>
            <template v-else>
              <router-link to="/auth" class="nav-link nav-cta">
                Войти
              </router-link>
            </template>
          </div>
        </div>
      </div>
    </nav>
    <main class="main-content page-shell">
      <router-view />
    </main>
    <Toast ref="toast" />
    <ConfirmDialog ref="confirmDialog" />
    <footer class="footer">
      <div class="container">
        <p>&copy; 2026 CityLeisure. Сервис городского досуга.</p>
      </div>
    </footer>
  </div>
</template>

<script>
import { mapState, mapActions } from 'pinia'
import Toast from './components/Toast.vue'
import ConfirmDialog from './components/ConfirmDialog.vue'
import { useAuthStore } from '@/stores/auth'

export default {
  name: 'App',
  components: {
    Toast,
    ConfirmDialog
  },
  computed: {
    ...mapState(useAuthStore, ['isAuthenticated'])
  },
  methods: {
    ...mapActions(useAuthStore, { checkAuth: 'hydrate', clearSession: 'clear' }),
    handleLogout() {
      this.clearSession()
      this.$router.push('/')
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.$root.$toast = this.$refs.toast
      this.$root.$confirm = this.$refs.confirmDialog
      this.$root.$checkAuth = this.checkAuth
    })

    window.addEventListener('storage', this.checkAuth)
    window.addEventListener('auth-changed', this.checkAuth)
    this.checkAuth()
  },
  beforeUnmount() {
    window.removeEventListener('storage', this.checkAuth)
    window.removeEventListener('auth-changed', this.checkAuth)
  },
  watch: {
    '$route'() {
      this.checkAuth()
    }
  }
}
</script>
