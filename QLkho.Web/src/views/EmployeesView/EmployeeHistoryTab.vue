<template>
  <div class="tab-content">
    <!-- Bộ lọc lịch sử -->
    <div class="glass-card filter-card">
      <div class="filter-grid" style="grid-template-columns: 1fr 220px 240px;">
        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Tìm kiếm sự kiện</label>
          <input 
            type="text" 
            class="form-control" 
            :value="filters.search" 
            placeholder="Tên nhân viên, mã NV, mô tả sự kiện..." 
            @input="$emit('update:search', $event.target.value)"
          />
        </div>

        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Loại Biến Động</label>
          <select 
            class="form-control" 
            :value="filters.actionType" 
            @change="$emit('update:actionType', $event.target.value)"
          >
            <option value="">-- Tất cả loại sự kiện --</option>
            <option value="StartedWorking">💼 Chính thức đi làm</option>
            <option value="OnboardingCreated">🚀 Tạo hồ sơ mới</option>
            <option value="UrgentMarked">🚨 Báo khẩn cấp</option>
            <option value="UrgentUnmarked">✕ Hủy khẩn cấp</option>
            <option value="AssetAssigned">📦 Cấp phát thiết bị</option>
            <option value="AssetReturned">📥 Thu hồi thiết bị</option>
            <option value="AccountUpdated">🔑 Tài khoản hệ thống</option>
            <option value="ProfileUpdated">✏️ Cập nhật hồ sơ</option>
            <option value="Resigned">📁 Đã nghỉ việc</option>
          </select>
        </div>

        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Lọc theo nhân sự</label>
          <select 
            class="form-control" 
            :value="filters.employeeId" 
            @change="$emit('update:employeeId', $event.target.value ? Number($event.target.value) : null)"
          >
            <option :value="null">-- Toàn bộ nhân sự --</option>
            <option v-for="emp in employees" :key="emp.employeeID" :value="emp.employeeID">
              {{ emp.fullName }} ({{ emp.employeeCode }})
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Dòng thời gian lịch sử (Timeline) -->
    <div class="glass-card table-container" style="padding: 24px;">
      <div v-if="loadingHistory" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải dòng thời gian lịch sử nhân sự...</p>
      </div>

      <div v-else-if="histories.length === 0" class="empty-state">
        <p>Chưa có bản ghi lịch sử biến động nào phù hợp với bộ lọc.</p>
      </div>

      <div v-else class="history-timeline-wrap">
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

            <!-- Nhân sự liên quan -->
            <div class="event-emp-chip">
              <span class="event-emp-avatar">👤</span>
              <span class="event-emp-name">{{ item.employeeName }}</span>
              <span class="event-emp-code">({{ item.employeeCode }})</span>
              <span class="dot-sep">•</span>
              <span class="event-emp-dept">{{ item.departmentName || '---' }}</span>
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
    </div>
  </div>
</template>

<script setup>
defineProps({
  histories: { type: Array, required: true },
  employees: { type: Array, default: () => [] },
  loadingHistory: { type: Boolean, default: false },
  filters: { type: Object, required: true }
})

defineEmits([
  'update:search',
  'update:actionType',
  'update:employeeId'
])

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
