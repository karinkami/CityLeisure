export function formatEventPrice(price) {
  const n = Number(price)
  if (!Number.isFinite(n) || n === 0) return 'Бесплатно'
  return `${new Intl.NumberFormat('ru-RU').format(n)} ₽`
}
