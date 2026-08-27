<template>
  <Modal 
    :is-open="isOpen" 
    :title="`Chi Tiết Thiết Bị: ${asset?.assetCode || ''}`"
    :subtitle="asset?.assetName || ''"
    @close="$emit('close')"
    max-width="750px"
  >
    <div v-if="loading" class="table-loading">
      <span class="loading-spinner"></span>
      <p>Đang tải chi tiết thiết bị & lịch sử...</p>
    </div>

    <div v-else-if="asset" class="detail-container">
      <!-- Control Actions Toolbar -->
      <div class="detail-toolbar-card">
        <div class="toolbar-label">Thao Tác & Quản Trị Thiết Bị:</div>
        <div class="toolbar-buttons-grid">
          <!-- Nút Cấp phát (Khi máy Available) -->
          <button 
            v-if="asset.status === 'Available'"
            type="button" 
            class="btn btn-sm btn-primary"
            @click="$emit('assign', asset)"
            title="Cấp phát thiết bị từ kho cho nhân viên"
          >
            🚀 Cấp Phát Thiết Bị
          </button>

          <!-- Nút Điều chuyển (Khi máy In-Use) -->
          <button 
            v-if="asset.status === 'In-Use'"
            type="button" 
            class="btn btn-sm btn-info-action"
            @click="$emit('transfer', asset)"
            title="Điều chuyển trực tiếp sang nhân sự khác"
          >
            🔄 Điều Chuyển
          </button>

          <!-- Nút Thu hồi (Khi máy In-Use) -->
          <button 
            v-if="asset.status === 'In-Use'"
            type="button" 
            class="btn btn-sm btn-secondary"
            @click="$emit('return', asset)"
            title="Thu hồi thiết bị về kho IT"
          >
            📥 Thu Hồi Về Kho
          </button>

          <!-- Nút Báo hỏng / Đi bảo trì -->
          <button 
            type="button" 
            class="btn btn-sm btn-warning-action"
            @click="$emit('report-issue', asset)"
            title="Ghi nhận lỗi hỏng / Gửi đi bảo hành bảo trì"
          >
            ⚠️ Báo Hỏng / Bảo Trì
          </button>

          <!-- Nút In Biên Bản (Khi máy In-Use) -->
          <button 
            v-if="asset.status === 'In-Use'"
            type="button" 
            class="btn btn-sm btn-success"
            @click="$emit('print', asset)"
            title="In biên bản bàn giao thiết bị A4 / PDF"
          >
            🖨️ In Biên Bản (A4)
          </button>

          <!-- Nút Sửa -->
          <button 
            type="button" 
            class="btn btn-sm btn-outline-primary"
            @click="$emit('edit', asset)"
            title="Chỉnh sửa thông số, mã, serial thiết bị"
          >
            ✏️ Sửa Thông Tin
          </button>

          <!-- Nút Xóa -->
          <button 
            type="button" 
            class="btn btn-sm btn-outline-danger"
            @click="$emit('delete', asset)"
            :disabled="asset.status === 'In-Use'"
            :title="asset.status === 'In-Use' ? 'Không thể xóa thiết bị đang được cấp phát' : 'Xóa vĩnh viễn thiết bị này khỏi hệ thống'"
          >
            🗑️ Xóa Thiết Bị
          </button>
        </div>
      </div>

      <!-- Asset Specs Box -->
      <div class="specs-grid">
        <div class="spec-item"><span class="spec-label">Host Name (Mã Máy):</span> <strong>{{ asset.assetCode }}</strong></div>
        <div class="spec-item"><span class="spec-label">Model:</span> <strong>{{ asset.assetName }}</strong></div>
        <div class="spec-item"><span class="spec-label">Loại Thiết Bị:</span> {{ asset.categoryName }}</div>
        <div class="spec-item"><span class="spec-label">Hãng:</span> {{ asset.brand || '---' }}</div>
        <div class="spec-item"><span class="spec-label">Service Tag (S/N):</span> <strong class="font-mono">{{ asset.serialNumber || '---' }}</strong></div>
        <div class="spec-item"><span class="spec-label">Asset Number (Mã VT):</span> {{ asset.materialCode || '---' }}</div>
        <div class="spec-item"><span class="spec-label">Nhà cung cấp:</span> {{ asset.supplierName || '---' }}</div>
        <div class="spec-item">
          <span class="spec-label">Trạng thái:</span> 
          <StatusBadge :status="asset.status" type="asset" />
        </div>
        <div class="spec-item full-width" v-if="asset.specifications">
          <span class="spec-label">⚙️ COMPUTER SPECS:</span> 
          <strong style="color: #0284c7;">{{ asset.specifications }}</strong>
        </div>
        <div class="spec-item full-width" v-if="asset.note">
          <span class="spec-label">📦 Phụ kiện & Ghi chú:</span> 
          <span style="color: #475569;">{{ asset.note }}</span>
        </div>
        <div class="spec-item full-width">
          <span class="spec-label">Người giữ & Vị trí hiện tại:</span> 
          <strong style="color: #059669;">
            {{ asset.holderName ? `${asset.holderName} (${asset.holderCode || ''}) - ${asset.holderDepartment || ''}` : asset.dynamicLocation }}
          </strong>
        </div>
      </div>

      <!-- History Timeline -->
      <div class="timeline-section">
        <h4 class="timeline-title">Lịch Sử Cấp Phát & Điều Chuyển</h4>
        
        <div v-if="!asset.histories || asset.histories.length === 0" class="empty-mini">
          Chưa có lịch sử bàn giao nào được ghi nhận cho thiết bị này.
        </div>

        <div v-else class="timeline-list">
          <div v-for="h in asset.histories" :key="h.historyID" class="timeline-item">
            <div class="timeline-dot"></div>
            <div class="timeline-content">
              <div class="timeline-header">
                <span class="action-type-badge" :class="(h.actionType || '').toLowerCase()">
                  {{ formatActionTypeLabel(h.actionType) }}
                </span>
                <span class="timeline-date">{{ formatDate(h.actionDate) }}</span>
              </div>

              <div class="timeline-desc">
                <span v-if="h.actionType === 'Assign'">
                  Bàn giao cho: <strong>{{ h.toEmployee }}</strong> (Tại: {{ h.toLocation }})
                </span>
                <span v-else-if="h.actionType === 'Return'">
                  Thu hồi từ: <strong>{{ h.fromEmployee || 'Nhân viên' }}</strong> về <strong>{{ h.toLocation }}</strong>
                </span>
                <span v-else-if="h.actionType === 'Transfer'">
                  Điều chuyển từ: <strong>{{ h.fromEmployee || 'Nhân viên' }}</strong> ➔ <strong>{{ h.toEmployee }}</strong>
                </span>
                <span v-else-if="h.actionType === 'SendMaintenance' || h.actionType === 'Report-Broken'">
                  Báo hỏng / Gửi sửa: <strong>{{ h.conditionStatus || h.note }}</strong>
                </span>
                <span v-else>
                  {{ h.note }}
                </span>
              </div>

              <div v-if="h.conditionStatus" class="timeline-condition">
                Tình trạng: {{ h.conditionStatus }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import Modal from '@/components/common/Modal.vue'
import StatusBadge from '@/components/common/StatusBadge.vue'

defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  asset: {
    type: Object,
    default: null
  },
  loading: {
    type: Boolean,
    default: false
  }
})

defineEmits(['close', 'assign', 'transfer', 'return', 'report-issue', 'print', 'edit', 'delete'])

const formatDate = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

const formatActionTypeLabel = (actionType) => {
  const act = (actionType || '').trim().toLowerCase()
  if (act === 'assign') return 'Cấp phát'
  if (act === 'return') return 'Thu hồi'
  if (act === 'transfer') return 'Điều chuyển'
  if (act === 'sendmaintenance' || act === 'report-broken' || act === 'report_broken' || act === 'broken') return 'Báo hỏng / Đi sửa'
  if (act === 'maintenance') return 'Bảo trì'
  return actionType || 'Thao tác'
}
</script>


<style scoped>
.detail-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.detail-toolbar-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 12px 14px;
}

.toolbar-label {
  font-size: 0.775rem;
  font-weight: 700;
  color: #64748b;
  margin-bottom: 8px;
  text-transform: uppercase;
}

.toolbar-buttons-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.specs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px;
  font-size: 0.85rem;
}

.spec-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.spec-item.full-width {
  grid-column: 1 / -1;
  border-top: 1px solid #f1f5f9;
  padding-top: 6px;
  margin-top: 2px;
}

.spec-label {
  font-size: 0.75rem;
  color: #64748b;
}

.timeline-section {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px;
}

.timeline-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 12px 0;
}

.empty-mini {
  font-size: 0.8rem;
  color: #94a3b8;
  font-style: italic;
  text-align: center;
  padding: 10px;
}

.timeline-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  position: relative;
  padding-left: 16px;
  border-left: 2px solid #e2e8f0;
  margin-left: 8px;
}

.timeline-item {
  position: relative;
}

.timeline-dot {
  width: 10px;
  height: 10px;
  background: #3b82f6;
  border-radius: 50%;
  position: absolute;
  left: -22px;
  top: 5px;
}

.timeline-content {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 10px 12px;
}

.timeline-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.action-type-badge {
  font-size: 0.725rem;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.action-type-badge.assign { background: #eff6ff; color: #2563eb; }
.action-type-badge.return { background: #fef2f2; color: #dc2626; }
.action-type-badge.transfer { background: #f0fdf4; color: #16a34a; }

.timeline-date {
  font-size: 0.725rem;
  color: #94a3b8;
}

.timeline-desc {
  font-size: 0.825rem;
  color: #334155;
}

.timeline-condition {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 4px;
  font-style: italic;
}

.btn-info-action {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1d4ed8;
}

.btn-info-action:hover {
  background: #dbeafe;
}

.btn-warning-action {
  background: #fffbeb;
  border: 1px solid #fde68a;
  color: #b45309;
}

.btn-warning-action:hover {
  background: #fef3c7;
}
</style>
