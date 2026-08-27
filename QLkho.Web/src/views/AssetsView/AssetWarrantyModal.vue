<template>
  <Modal 
    :is-open="isOpen" 
    title="Cảnh Báo Hạn Bảo Hành Thiết Bị"
    subtitle="Danh sách các thiết bị sắp hoặc đã hết hạn bảo hành để lên kế hoạch gia hạn / kiểm kê"
    @close="$emit('close')"
    max-width="850px"
  >
    <div v-if="loading" class="table-loading">
      <span class="loading-spinner"></span>
      <p>Đang kiểm tra dữ liệu bảo hành...</p>
    </div>

    <div v-else-if="alerts.length === 0" class="empty-state">
      <p style="color: #34d399; font-weight: 600;">✓ Không có thiết bị nào sắp hết hạn bảo hành trong 30 ngày tới!</p>
    </div>

    <div v-else class="warranty-list">
      <div v-for="w in sortedAlerts" :key="w.assetID" class="warranty-item-card">
        <div class="warranty-top">
          <div>
            <span class="code-badge">{{ w.assetCode }}</span>
            <strong class="warranty-name">{{ w.assetName }}</strong>
          </div>
          <span v-if="w.isExpired" class="badge-expired">ĐÃ HẾT HẠN</span>
          <span v-else class="badge-expiring">Còn {{ w.daysRemaining }} ngày</span>
        </div>

        <div class="warranty-meta">
          <div><span class="spec-label">Hạn bảo hành:</span> <strong>{{ formatDateOnly(w.warrantyExpireDate) }}</strong></div>
          <div><span class="spec-label">Nhà cung cấp:</span> {{ w.supplierName || '---' }}</div>
          <div><span class="spec-label">Người giữ:</span> {{ w.holderName || 'Trong kho' }}</div>
        </div>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { computed } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  loading: {
    type: Boolean,
    default: false
  },
  alerts: {
    type: Array,
    default: () => []
  }
})

defineEmits(['close'])

const sortedAlerts = computed(() => {
  return [...props.alerts].sort((a, b) => {
    if (!a.isExpired && b.isExpired) return -1
    if (a.isExpired && !b.isExpired) return 1
    if (!a.isExpired && !b.isExpired) {
      return (a.daysRemaining || 0) - (b.daysRemaining || 0)
    }
    return new Date(b.warrantyExpireDate) - new Date(a.warrantyExpireDate)
  })
})

const formatDateOnly = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
</script>

<style scoped>
.warranty-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 480px;
  overflow-y: auto;
  padding-right: 4px;
}

.warranty-item-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px 16px;
  transition: all 0.2s ease;
}

.warranty-item-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.warranty-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.warranty-name {
  margin-left: 8px;
  font-size: 0.95rem;
  color: #0f172a;
}

.badge-expired {
  background: #fee2e2;
  color: #dc2626;
  border: 1px solid #fca5a5;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 999px;
}

.badge-expiring {
  background: #fef3c7;
  color: #d97706;
  border: 1px solid #fcd34d;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 999px;
}

.warranty-meta {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 8px;
  font-size: 0.825rem;
  color: #475569;
  background: #f8fafc;
  padding: 8px 12px;
  border-radius: 6px;
}

.spec-label {
  color: #64748b;
  margin-right: 4px;
}

.code-badge {
  font-family: monospace;
  font-weight: 700;
  color: #4f46e5;
  background: #eef2ff;
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid #c7d2fe;
}
</style>
