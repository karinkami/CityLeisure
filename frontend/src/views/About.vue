<template>
  <div class="about">
    <div class="container about__frame">
      <header class="hero">
        <div class="hero__glow" aria-hidden="true" />
        <div class="hero__sparkles" aria-hidden="true">
          <span></span><span></span><span></span>
        </div>
        <div class="hero__content">
          <p class="hero__label ds-reveal">CityLeisure</p>
          <h1 class="hero__title ds-reveal ds-reveal-delay-1">Афиша и билеты без лишнего шума</h1>
          <p class="hero__text ds-reveal ds-reveal-delay-2">
            Мы собрали каталог мероприятий, корзину и личный кабинет в одном спокойном интерфейсе — чтобы вы
            быстрее оказались на событии, а не в настройках.
          </p>
          <div class="hero__chips ds-reveal ds-reveal-delay-3">
            <span class="chip">Фильтры по дате и цене</span>
            <span class="chip">Покупка онлайн</span>
            <span class="chip">История заказов</span>
          </div>
          <p class="hero__tagline ds-reveal ds-reveal-delay-3">Создано, чтобы выбор события ощущался как удовольствие.</p>
        </div>
      </header>

      <div class="bento">
        <article class="card card--wide ds-card-pro ds-reveal ds-reveal-delay-1">
          <div class="card__icon" aria-hidden="true">◇</div>
          <h2>Возможности</h2>
          <ul class="list">
            <li>Афиша с поиском, категориями, площадками и ценой</li>
            <li>Карточка события: дата, время, адрес, возрастной рейтинг</li>
            <li>Корзина и оформление заказа в несколько шагов</li>
            <li>Избранное и подбор по интересам</li>
            <li>Личный кабинет и «Мои билеты»</li>
          </ul>
        </article>

        <article class="card card--accent ds-card-pro ds-reveal ds-reveal-delay-2">
          <h2>Как это работает</h2>
          <ol class="steps">
            <li><span class="steps__n">1</span> Регистрация или вход</li>
            <li><span class="steps__n">2</span> Выбор события в афише</li>
            <li><span class="steps__n">3</span> Добавление билетов в корзину</li>
            <li><span class="steps__n">4</span> Оплата и билеты в кабинете</li>
          </ol>
        </article>

        <article class="card card--contact ds-card-pro ds-reveal ds-reveal-delay-3">
          <h2>Контакты</h2>
          <p class="contact-line"><span class="contact-k">Email</span> info@gorodafisha.ru</p>
          <p class="contact-line"><span class="contact-k">Телефон</span> +7 (800) 123-45-67</p>
          <p class="contact-line"><span class="contact-k">Офис</span> г. Казань, ул. Баумана, д. 1</p>
          <router-link to="/events" class="ds-btn ds-btn--secondary ds-btn--block contact-cta">Перейти в афишу</router-link>
        </article>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'About'
}
</script>

<style scoped>
.about {
  padding-bottom: var(--space-6);
  background:
    radial-gradient(800px 400px at 0% 0%, rgba(15, 118, 110, 0.08), transparent 50%),
    radial-gradient(600px 320px at 100% 20%, rgba(245, 158, 11, 0.07), transparent 45%),
    var(--bg);
}

.about__frame {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.hero {
  position: relative;
  border-radius: 20px;
  overflow: hidden;
  border: 1px solid var(--border);
  background: linear-gradient(125deg, #0f766e 0%, #0d5c56 42%, #134e4a 100%);
  color: #fff;
  box-shadow:
    0 1px 2px rgba(0, 0, 0, 0.06),
    0 28px 56px -28px rgba(15, 118, 110, 0.55);
  animation: about-rise 520ms ease both;
  isolation: isolate;
}

.hero::after {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  right: -70px;
  bottom: -120px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(251, 191, 36, 0.24), rgba(251, 191, 36, 0));
  pointer-events: none;
  z-index: 0;
}

.hero__glow {
  position: absolute;
  inset: -40% -20% auto -20%;
  height: 80%;
  background: radial-gradient(circle, rgba(255, 255, 255, 0.14) 0%, transparent 65%);
  pointer-events: none;
  animation: glow-float 9s ease-in-out infinite;
}
.hero__sparkles {
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}
.hero__sparkles span {
  position: absolute;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.75);
  box-shadow: 0 0 0 8px rgba(255, 255, 255, 0.07);
  animation: sparkle 3.8s ease-in-out infinite;
}
.hero__sparkles span:nth-child(1) {
  top: 20%;
  left: 62%;
}
.hero__sparkles span:nth-child(2) {
  top: 58%;
  right: 16%;
  animation-delay: 0.8s;
}
.hero__sparkles span:nth-child(3) {
  top: 34%;
  right: 30%;
  animation-delay: 1.4s;
}

.hero__content {
  position: relative;
  padding: clamp(var(--space-5), 6vw, var(--space-6));
  max-width: 720px;
  z-index: 1;
}

.hero__label {
  margin: 0 0 var(--space-3);
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  opacity: 0.85;
}

.hero__title {
  margin: 0 0 var(--space-3);
  font-size: clamp(1.75rem, 4vw, 2.5rem);
  font-weight: 800;
  line-height: 1.1;
  letter-spacing: -0.03em;
  text-wrap: balance;
  background: linear-gradient(120deg, #ffffff 0%, #ccfbf1 50%, #fde68a 100%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.hero__text {
  margin: 0 0 var(--space-4);
  font-size: 1.0625rem;
  line-height: 1.6;
  opacity: 0.92;
  max-width: 52ch;
  text-wrap: pretty;
}
.hero__tagline {
  margin: var(--space-3) 0 0;
  font-size: 0.95rem;
  letter-spacing: 0.01em;
  opacity: 0.95;
}

.hero__chips {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

.chip {
  font-size: 0.75rem;
  font-weight: 700;
  padding: var(--space-2) var(--space-3);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.22);
  transition: transform 0.2s ease, background-color 0.2s ease;
}

.chip:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.18);
}

.bento {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  grid-template-areas:
    'wide accent'
    'wide contact';
  gap: var(--space-4);
}

.card {
  padding: var(--space-5);
  transition: transform 0.24s ease, box-shadow 0.24s ease, border-color 0.24s ease;
  animation: about-rise 480ms ease both;
  border: 1px solid rgba(15, 23, 42, 0.08);
}

.card:hover {
  transform: translateY(-3px);
  box-shadow: 0 20px 34px -26px rgba(2, 6, 23, 0.34);
}

.card--wide {
  grid-area: wide;
}

.card--accent {
  grid-area: accent;
  background: linear-gradient(165deg, #fffbeb 0%, #ffffff 55%);
  position: relative;
  overflow: hidden;
}
.card--accent::after {
  content: '';
  position: absolute;
  width: 180px;
  height: 180px;
  right: -40px;
  top: -60px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(245, 158, 11, 0.2), transparent 72%);
  pointer-events: none;
}

.card--contact {
  grid-area: contact;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  background: linear-gradient(180deg, #ffffff 0%, #f9fafb 100%);
}

.card__icon {
  font-size: 1.5rem;
  color: var(--primary);
  margin-bottom: var(--space-2);
  opacity: 0.9;
}

.card h2 {
  margin: 0 0 var(--space-4);
  font-size: 1.2rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}

.list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.list li {
  position: relative;
  padding-left: var(--space-5);
  font-size: 0.9375rem;
  color: var(--text-secondary);
  line-height: 1.5;
}

.list li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0.55em;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--primary);
  opacity: 0.85;
}

.steps {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.steps li {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  font-weight: 600;
  font-size: 0.9375rem;
  color: var(--text-primary);
}

.steps__n {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8125rem;
  font-weight: 800;
  background: rgba(245, 158, 11, 0.2);
  color: #b45309;
  border: 1px solid rgba(245, 158, 11, 0.35);
  transition: transform 0.2s ease, background-color 0.2s ease;
}

.steps li:hover .steps__n {
  transform: scale(1.08);
  background: rgba(245, 158, 11, 0.26);
}

.contact-line {
  margin: 0;
  font-size: 0.9375rem;
  color: var(--text-primary);
  line-height: 1.5;
}

.contact-k {
  display: inline-block;
  min-width: 4.5rem;
  margin-right: var(--space-2);
  font-size: 0.75rem;
  font-weight: 800;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--text-secondary);
}

.contact-cta {
  margin-top: auto;
}

@media (max-width: 900px) {
  .bento {
    grid-template-columns: 1fr;
    grid-template-areas:
      'wide'
      'accent'
      'contact';
  }
}

@keyframes about-rise {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes glow-float {
  0% {
    transform: translateY(0) scale(1);
    opacity: 0.85;
  }
  50% {
    transform: translateY(8px) scale(1.03);
    opacity: 1;
  }
  100% {
    transform: translateY(0) scale(1);
    opacity: 0.85;
  }
}
@keyframes sparkle {
  0% {
    transform: scale(0.8);
    opacity: 0.3;
  }
  50% {
    transform: scale(1.15);
    opacity: 1;
  }
  100% {
    transform: scale(0.8);
    opacity: 0.3;
  }
}

@media (prefers-reduced-motion: reduce) {
  .hero,
  .card {
    animation: none;
  }
  .hero__glow {
    animation: none;
  }
  .hero__sparkles span {
    animation: none;
  }
  .chip,
  .card,
  .steps__n {
    transition: none;
  }
}
</style>
