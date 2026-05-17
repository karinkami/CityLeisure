export function eventUsesSeatSelection(event) {
  if (!event) return false
  const st = String(event.seatingType || 'general').toLowerCase()
  return st === 'assigned' && Number(event.price) > 0
}
