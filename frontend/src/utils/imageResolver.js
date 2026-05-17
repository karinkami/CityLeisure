import { EVENT_FALLBACK_IMAGE_PATHS, EVENT_POSTER_FILES, PLACEHOLDER_URL_PATTERN } from '@/config/eventImages'

export function usesAutomaticPoster(imageUrl) {
  if (imageUrl == null || String(imageUrl).trim() === '') return true
  return PLACEHOLDER_URL_PATTERN.test(String(imageUrl))
}

export function getFallbackPosterFileName(eventId) {
  const safeId = Number(eventId) || 0
  const index = Math.abs(safeId) % EVENT_FALLBACK_IMAGE_PATHS.length
  return EVENT_POSTER_FILES[index]
}

export function resolveEventImageUrl(imageUrl, eventId = 0) {
  if (imageUrl) {
    if (PLACEHOLDER_URL_PATTERN.test(String(imageUrl))) {
      const safeId = Number(eventId) || 0
      const index = Math.abs(safeId) % EVENT_FALLBACK_IMAGE_PATHS.length
      return EVENT_FALLBACK_IMAGE_PATHS[index]
    }
    if (/^(https?:)?\/\//i.test(imageUrl) || imageUrl.startsWith('/')) {
      return imageUrl
    }
    return `/images/events/${imageUrl}`
  }

  const safeId = Number(eventId) || 0
  const index = Math.abs(safeId) % EVENT_FALLBACK_IMAGE_PATHS.length
  return EVENT_FALLBACK_IMAGE_PATHS[index]
}
