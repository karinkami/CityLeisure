export function buildYandexMapWidgetUrl(venue) {
  if (!venue) return null
  const lat = venue.latitude
  const lon = venue.longitude
  if (lat != null && lon != null && !Number.isNaN(Number(lat)) && !Number.isNaN(Number(lon))) {
    const la = Number(lat)
    const lo = Number(lon)
    return `https://yandex.ru/map-widget/v1/?ll=${lo}%2C${la}&z=16&pt=${lo},${la},pm2rdm`
  }
  const parts = [venue.address, venue.city, venue.name].filter(Boolean)
  if (parts.length === 0) return null
  return `https://yandex.ru/map-widget/v1/?z=14&text=${encodeURIComponent(parts.join(', '))}`
}

export function buildVenueMapUrl(venue) {
  if (!venue) return null
  if (venue.mapUrl) return venue.mapUrl
  const lat = venue.latitude
  const lon = venue.longitude
  if (lat != null && lon != null && !Number.isNaN(Number(lat)) && !Number.isNaN(Number(lon))) {
    return `https://yandex.ru/maps/?pt=${lon},${lat}&z=16&l=map`
  }
  const parts = [venue.address, venue.city].filter(Boolean)
  if (parts.length === 0) return null
  return `https://yandex.ru/maps/?text=${encodeURIComponent(parts.join(', '))}`
}

export function buildVenueOsmUrl(venue) {
  if (!venue) return null
  const lat = venue.latitude
  const lon = venue.longitude
  if (lat == null || lon == null || Number.isNaN(Number(lat)) || Number.isNaN(Number(lon))) return null
  const la = Number(lat)
  const lo = Number(lon)
  return `https://www.openstreetmap.org/?mlat=${la}&mlon=${lo}#map=16/${la}/${lo}`
}

export function venueFromEventDto(ev) {
  if (!ev) return null
  return {
    name: ev.venueName,
    address: ev.venueAddress,
    city: ev.venueCity,
    latitude: ev.venueLatitude,
    longitude: ev.venueLongitude,
    mapUrl: ev.venueMapUrl
  }
}
