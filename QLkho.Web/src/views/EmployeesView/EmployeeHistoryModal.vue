<template>
  <Modal 
    :is-open="isOpen" 
    :title="`Lịch Sử Biến Động: ${employee?.fullName || ''} (${employee?.employeeCode || ''})`"
    :subtitle="`Phòng ban: ${employee?.departmentName || ''} • Ngày vào làm: ${formatDate(employee?.joinDate)} • Trạng thái: ${employee?.status || 'Active'}`"
    @close="$emit('close')"
    max-width="850px"
  >
    <div v-if="loading" class="table-loading">
      <span class="loading-spinner"></span>
      <p>Đang tải lịch sử nhân sự...</p>
    </div>

    <div v-else-if="histories.length === 0" class="empty-state" style="padding: 30px;">
      <p>Chưa có bản ghi lịch sử nào được ghi nhận cho nhân sự này.</p>
    </div>

    <div v-else class="history-timeline-wrap" style="padding: 10px 0;">
      <div 
        v-for="item in histories" 
        :key="item.employeeHistoryID" 
        class="timeline-event-card"
        :class="`event-type-${item.actionType?.toLowerCase()}`"
      >
        <!-- Cột thời gian bên trái -->
        <div class="event-time-col">
          <span class="event-date">{{ formatDateFull(item.actionDate) }}</span>
          <span class="event-time">{{ formatTime(item.actionDate) }}</span>
        </div>

        <!-- Điểm mốc & icon -->
        <div class="event-node-col">
          <div class="event-node-icon" :class="`icon-${item.actionType}`">
            {{ getHistoryIcon(item.actionType) }}
          </div>
          <div class="event-line"></div>
        </div>

        <!-- Nội dung chi tiết sự kiện -->
        <div class="event-content-col">
          <div class="event-header-row">
            <div class="event-title-group">
              <span class="badge-history-type" :class="`badge-type-${item.actionType}`">
                {{ getHistoryTypeLabel(item.actionType) }}
              </span>
              <strong class="event-main-title">{{ item.title }}</strong>
            </div>
            <div class="event-actor-tag">
              👤 Thực hiện: <strong>{{ item.performedBy || 'Hệ Thống' }}</strong>
            </div>
          </div>

          <!-- Mô tả chi tiết -->
          <p class="event-description-text">{{ item.description }}</p>

          <!-- Thay đổi giá trị Trước -> Sau (Nếu có) -->
          <div v-if="item.oldValue || item.newValue" class="event-diff-box">
            <div v-if="item.oldValue" class="diff-item diff-old">
              <span class="diff-lbl">Trước:</span>
              <span class="diff-val">{{ item.oldValue }}</span>
            </div>
            <div v-if="item.oldValue && item.newValue" class="diff-arrow">➔</div>
            <div v-if="item.newValue" class="diff-item diff-new">
              <span class="diff-lbl">Sau:</span>
              <span class="diff-val">{{ item.newValue }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-actions-right" style="margin-top: 20px;">
      <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
    </div>
  </Modal>
</template>

<script setup>
import Modal from '@/components/common/Modal.vue'

defineProps({
  isOpen: { type: Boolean, default: false },
  employee: { type: Object, default: null },
  histories: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false }
})

defineEmits(['close'])

const getHistoryIcon = (type) => {
  switch (type) {
    case 'StartedWorking': return '💼'
    case 'OnboardingCreated': return '🚀'
    case 'UrgentMarked': return '🚨'
    case 'UrgentUnmarked': return '✕'
    case 'AssetAssigned': return '📦'
    case 'AssetReturned': return '📥'
    case 'AssetTransferred': return '🔄'
    case 'AccountUpdated': return '🔑'
    case 'ProfileUpdated': return '✏️'
    case 'Resigned': return '📁'
    default: return '🕒'
  }
}

const getHistoryTypeLabel = (type) => {
  switch (type) {
    case 'StartedWorking': return 'Chính Thức Đi Làm'
    case 'OnboardingCreated': return 'Tạo Hồ Sơ Mới'
    case 'UrgentMarked': return 'Báo Khẩn Cấp'
    case 'UrgentUnmarked': return 'Hủy Khẩn Cấp'
    case 'AssetAssigned': return 'Cấp Phát Thiết Bị'
    case 'AssetReturned': return 'Thu Hồi Thiết Bị'
    case 'AssetTransferred': return 'Điều Chuyển Thiết Bị'
    case 'AccountUpdated': return 'Tài Khoản Hệ Thống'
    case 'ProfileUpdated': return 'Cập Nhật Hồ Sơ'
    case 'Resigned': return 'Đã Nghỉ Việc'
    default: return 'Sự Kiện Nhân Sự'
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const formatDateFull = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const formatTime = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}
</script>
