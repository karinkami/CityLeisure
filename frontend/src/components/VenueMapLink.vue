<template>
  <div v-if="yandexHref || osmHref" class="venue-map-link">
    <a
      v-if="yandexHref"
      :href="yandexHref"
      class="venue-map-link__a"
      target="_blank"
      rel="noopener noreferrer"
    >
      {{ primaryLabel }}
    </a>
    <a
      v-if="osmHref"
      :href="osmHref"
      class="venue-map-link__a venue-map-link__a--muted"
      target="_blank"
      rel="noopener noreferrer"
    >
      OSM
    </a>
  </div>
</template>

<script>
import { buildVenueMapUrl, buildVenueOsmUrl } from '@/utils/mapLink'

export default {
  name: 'VenueMapLink',
  props: {
    venue: {
      type: Object,
      default: null
    },
    primaryLabel: {
      type: String,
      default: 'На карте'
    }
  },
  computed: {
    yandexHref() {
      return buildVenueMapUrl(this.venue)
    },
    osmHref() {
      return buildVenueOsmUrl(this.venue)
    }
  }
}
</script>

<style scoped>
.venue-map-link {
  display: inline-flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.35rem 0.65rem;
  margin-top: 2px;
}

.venue-map-link__a {
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--primary);
  text-decoration: none;
}

.venue-map-link__a:hover {
  text-decoration: underline;
}

.venue-map-link__a--muted {
  font-weight: 600;
  color: var(--text-secondary);
}
</style>
