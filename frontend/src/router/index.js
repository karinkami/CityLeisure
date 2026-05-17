import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Home from '../views/Home.vue'
import EventListView from '../views/EventListView.vue'
import About from '../views/About.vue'
import Auth from '../views/Auth.vue'
import Profile from '../views/Profile.vue'
import RecommendationsView from '../views/RecommendationsView.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/events',
    name: 'Events',
    component: EventListView
  },
  {
    path: '/recommendations',
    name: 'Recommendations',
    component: RecommendationsView
  },
  {
    path: '/events/:id',
    name: 'EventDetail',
    component: () => import('../views/EventDetailsView.vue')
  },
  {
    path: '/catalog',
    redirect: '/events'
  },
  {
    path: '/about',
    name: 'About',
    component: About
  },
  {
    path: '/auth',
    name: 'Auth',
    component: Auth
  },
  {
    path: '/login',
    redirect: '/auth'
  },
  {
    path: '/register',
    redirect: '/auth'
  },
  {
    path: '/profile/favorites',
    name: 'ProfileFavorites',
    component: () => import('../views/FavoritesView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: Profile,
    meta: { requiresAuth: true }
  },
  {
    path: '/tickets-cart',
    name: 'TicketsCart',
    component: () => import('../views/TicketCartView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/checkout',
    name: 'Checkout',
    component: () => import('../views/Checkout.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/my-tickets',
    name: 'MyTickets',
    component: () => import('../views/MyTicketsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/admin/events',
    name: 'AdminEvents',
    component: () => import('../views/AdminEventsView.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/cart',
    redirect: '/tickets-cart'
  },
  {
    path: '/orders',
    redirect: '/my-tickets'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0, left: 0 }
  }
})

router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    next('/auth')
  } else if (to.meta.requiresAdmin && !auth.isAdmin) {
    next('/')
  } else {
    next()
  }
})

export default router
