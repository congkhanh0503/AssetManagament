<template>
  <div v-if="totalPages > 1 || totalItems > 10" class="pagination-container">
    <div class="pagination-info">
      {{ $t('common.pagination_info', { from: fromItem, to: toItem, total: totalItems }) }}
    </div>

    <div class="pagination-controls">
      <!-- Chọn số lượng trên trang -->
      <div class="page-size-selector">
        <label class="size-label">{{ $t('common.page_display') }}</label>
        <select 
          :value="pageSize" 
          @change="$emit('update:pageSize', Number($event.target.value))"
          class="page-size-select"
        >
          <option v-for="size in pageSizeOptions" :key="size" :value="size">
            {{ $t('common.per_page', { size }) }}
          </option>
        </select>
      </div>

      <!-- Nút Điều hướng trang -->
      <div class="page-buttons">
        <button 
          class="page-btn prev-next" 
          :disabled="currentPage <= 1"
          @click="$emit('update:currentPage', currentPage - 1)"
          title="Trang trước"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="15 18 9 12 15 6"></polyline>
          </svg>
        </button>

        <template v-for="page in visiblePages" :key="page">
          <span v-if="page === '...'" class="page-dots">...</span>
          <button 
            v-else
            :class="['page-btn', { active: page === currentPage }]"
            @click="$emit('update:currentPage', page)"
          >
            {{ page }}
          </button>
        </template>

        <button 
          class="page-btn prev-next" 
          :disabled="currentPage >= totalPages"
          @click="$emit('update:currentPage', currentPage + 1)"
          title="Trang sau"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="9 18 15 12 9 6"></polyline>
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  currentPage: {
    type: Number,
    default: 1
  },
  pageSize: {
    type: Number,
    default: 10
  },
  totalItems: {
    type: Number,
    default: 0
  },
  pageSizeOptions: {
    type: Array,
    default: () => [10, 20, 50, 100]
  }
})

defineEmits(['update:currentPage', 'update:pageSize'])

const totalPages = computed(() => Math.ceil(props.totalItems / props.pageSize) || 1)

const fromItem = computed(() => {
  if (props.totalItems === 0) return 0
  return (props.currentPage - 1) * props.pageSize + 1
})

const toItem = computed(() => {
  return Math.min(props.currentPage * props.pageSize, props.totalItems)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = props.currentPage

  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    pages.push(1)
    if (current > 3) pages.push('...')
    
    const start = Math.max(2, current - 1)
    const end = Math.min(total - 1, current + 1)
    for (let i = start; i <= end; i++) {
      pages.push(i)
    }

    if (current < total - 2) pages.push('...')
    pages.push(total)
  }

  return pages
})
</script>

<style scoped>
.pagination-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 20px;
  background: #ffffff;
  border-top: 1px solid var(--border-color);
  flex-wrap: wrap;
}

.pagination-info {
  font-size: 0.825rem;
  color: var(--text-dim);
}

.pagination-info strong {
  color: #0f172a;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.page-size-selector {
  display: flex;
  align-items: center;
  gap: 8px;
}

.size-label {
  font-size: 0.8rem;
  color: var(--text-dim);
}

.page-size-select {
  background: #ffffff;
  border: 1px solid #cbd5e1;
  color: #0f172a;
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
  outline: none;
  cursor: pointer;
}

.page-buttons {
  display: flex;
  align-items: center;
  gap: 4px;
}

.page-btn {
  min-width: 32px;
  height: 32px;
  padding: 0 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: #0f172a;
  font-size: 0.825rem;
  font-weight: 500;
  cursor: pointer;
  transition: var(--transition);
}

.page-btn:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #cbd5e1;
  color: var(--primary);
}

.page-btn.active {
  background: var(--primary);
  border-color: var(--primary);
  color: #ffffff;
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.35);
}

.page-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.page-dots {
  color: var(--text-dim);
  padding: 0 4px;
  font-size: 0.825rem;
}
</style>
