<template>
  <span :class="['badge', badgeClass]">
    <span class="badge-dot" :style="{ backgroundColor: dotColor }"></span>
    {{ displayLabel }}
  </span>
</template>

<script setup>
import { computed } from 'vue'
import { t } from '@/i18n'

const props = defineProps({
  status: {
    type: String,
    required: true,
    default: 'Available'
  },
  type: {
    type: String,
    default: 'asset' // 'asset' or 'account' or 'employee'
  }
})

const badgeClass = computed(() => {
  const s = props.status?.toLowerCase() || ''
  if (s === 'available' || s === 'active') return 'badge-available'
  if (s === 'in-use' || s === 'inuse') return 'badge-inuse'
  if (s === 'broken') return 'badge-broken'
  if (s === 'maintenance' || s === 'onleave') return 'badge-maintenance'
  if (s === 'disable' || s === 'disabled' || s === 'resigned') return 'badge-disabled'
  if (s === 'deleted' || s === 'disposed') return 'badge-deleted'
  return 'badge-disabled'
})

const dotColor = computed(() => {
  const s = props.status?.toLowerCase() || ''
  if (s === 'available' || s === 'active') return '#10b981'
  if (s === 'in-use' || s === 'inuse') return '#6366f1'
  if (s === 'broken') return '#ef4444'
  if (s === 'maintenance' || s === 'onleave') return '#f59e0b'
  if (s === 'disable' || s === 'disabled' || s === 'resigned') return '#9ca3af'
  if (s === 'deleted' || s === 'disposed') return '#f87171'
  return '#9ca3af'
})

const displayLabel = computed(() => {
  const s = props.status || ''
  if (props.type === 'asset') {
    switch (s) {
      case 'Available': return t('status.available')
      case 'In-Use': return t('status.in_use')
      case 'Broken': return t('status.broken')
      case 'Maintenance': return t('status.maintenance')
      case 'Disposed': return t('status.disposed')
      default: return s
    }
  } else if (props.type === 'account') {
    switch (s) {
      case 'Available': return t('status.account_available')
      case 'Disable': return t('status.account_disable')
      case 'Deleted': return t('status.account_deleted')
      default: return s
    }
  } else if (props.type === 'employee') {
    switch (s) {
      case 'Active': return t('status.emp_active')
      case 'Resigned': return t('status.emp_resigned')
      case 'Onboarding': return t('status.emp_onboarding')
      default: return s
    }
  }
  return s
})
</script>

<style scoped>
.badge-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  display: inline-block;
}
</style>
