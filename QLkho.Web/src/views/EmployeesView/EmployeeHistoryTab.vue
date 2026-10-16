<template>
  <div class="tab-content history-tab-container">
    <!-- 1. BẢNG THỐNG KÊ NHANH (KPI SUMMARY CARDS) -->
    <div class="history-kpi-grid">
      <div 
        class="kpi-card" 
        :class="{ 'kpi-active': !filters.actionType }"
        @click="filterByType('')"
      >
        <div class="kpi-icon-wrap kpi-all">📊</div>
        <div class="kpi-info">
          <span class="kpi-num">{{ totalCount }}</span>
          <span class="kpi-label">Tổng Biến Động</span>
        </div>
      </div>

      <div 
        class="kpi-card" 
        :class="{ 'kpi-active': filters.actionType === 'AssetAssigned' }"
        @click="filterByType('AssetAssigned')"
      >
        <div class="kpi-icon-wrap kpi-assign">📦</div>
        <div class="kpi-info">
          <span class="kpi-num">{{ countAssigned }}</span>
          <span class="kpi-label">Cấp Phát Thiết Bị</span>
        </div>
      </div>

      <div 
        class="kpi-card" 
        :class="{ 'kpi-active': filters.actionType === 'AssetTransferred' }"
        @click="filterByType('AssetTransferred')"
      >
        <div class="kpi-icon-wrap kpi-transfer">🔄</div>
        <div class="kpi-info">
          <span class="kpi-num">{{ countTransferred }}</span>
          <span class="kpi-label">Điều Chuyển Thiết Bị</span>
        </div>
      </div>

      <div 
        class="kpi-card" 
        :class="{ 'kpi-active': filters.actionType === 'AssetReturned' }"
        @click="filterByType('AssetReturned')"
      >
        <div class="kpi-icon-wrap kpi-return">📥</div>
        <div class="kpi-info">
          <span class="kpi-num">{{ countReturned }}</span>
          <span class="kpi-label">Thu Hồi Thiết Bị</span>
        </div>
      </div>

      <div 
        class="kpi-card" 
        :class="{ 'kpi-active': filters.actionType === 'AccountUpdated' }"
        @click="filterByType('AccountUpdated')"
      >
        <div class="kpi-icon-wrap kpi-account">🔑</div>
        <div class="kpi-info">
          <span class="kpi-num">{{ countAccounts }}</span>
          <span class="kpi-label">Tài Khoản & Hồ Sơ</span>
        </div>
      </div>
    </div>

    <!-- 2. BỘ LỌC ĐA CHIỀU (ADVANCED FILTER TOOLBAR) -->
    <div class="glass-card filter-card">
      <div class="filter-top-row">
        <!-- Tìm kiếm -->
        <div class="form-group search-group" style="margin-bottom: 0;">
          <label class="form-label">🔍 Tìm kiếm sự kiện</label>
          <input 
            type="text" 
            class="form-control" 
            :value="filters.search" 
            placeholder="Tên nhân viên, mã NV, mã máy, số S/N, mô tả sự kiện..." 
            @input="$emit('update:search', $event.target.value)"
          />
        </div>

        <!-- Loại Biến Động -->
        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">🏷️ Loại Biến Động</label>
          <select 
            class="form-control" 
            :value="filters.actionType" 
            @change="$emit('update:actionType', $event.target.value)"
          >
            <option value="">-- Tất cả loại sự kiện --</option>
            <option value="AssetAssigned">📦 Cấp phát thiết bị</option>
            <option value="AssetTransferred">🔄 Điều chuyển thiết bị</option>
            <option value="AssetReturned">📥 Thu hồi thiết bị</option>
            <option value="AssetBroken">🛠️ Báo hỏng / Đi sửa</option>
            <option value="HandoverUploaded">📑 Tải biên bản bàn giao PDF</option>
            <option value="AccountUpdated">🔑 Tài khoản hệ thống</option>
            <option value="OnboardingCreated">🚀 Tạo hồ sơ mới</option>
            <option value="ProfileUpdated">✏️ Cập nhật hồ sơ</option>
            <option value="StartedWorking">💼 Chính thức đi làm</option>
            <option value="Resigned">📁 Đã nghỉ việc</option>
            <option value="UrgentMarked">🚨 Báo khẩn cấp</option>
            <option value="UrgentUnmarked">✕ Hủy khẩn cấp</option>
            <option value="ImportAccounts">📋 Đồng bộ tài khoản Excel</option>
          </select>
        </div>

        <!-- Lọc theo nhân sự -->
        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">👤 Lọc theo nhân sự</label>
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

      <!-- Dòng 2: Lọc thời gian + Nút thao tác -->
      <div class="filter-bottom-row">
        <div class="date-preset-group">
          <span class="preset-label">📅 Mốc thời gian:</span>
          <button 
            type="button" 
            class="btn-preset" 
            :class="{ active: filters.datePreset === 'all' }"
            @click="$emit('update:datePreset', 'all')"
          >
            Tất cả
          </button>
          <button 
            type="button" 
            class="btn-preset" 
            :class="{ active: filters.datePreset === 'today' }"
            @click="$emit('update:datePreset', 'today')"
          >
            Hôm nay
          </button>
          <button 
            type="button" 
            class="btn-preset" 
            :class="{ active: filters.datePreset === '7days' }"
            @click="$emit('update:datePreset', '7days')"
          >
            7 ngày qua
          </button>
          <button 
            type="button" 
            class="btn-preset" 
            :class="{ active: filters.datePreset === '30days' }"
            @click="$emit('update:datePreset', '30days')"
          >
            30 ngày qua
          </button>
        </div>

        <!-- Khoảng ngày tùy chọn -->
        <div class="date-custom-range">
          <input 
            type="date" 
            class="form-control date-input"
            :value="filters.fromDate"
            @change="$emit('update:fromDate', $event.target.value)"
            title="Từ ngày"
          />
          <span class="range-arrow">➔</span>
          <input 
            type="date" 
            class="form-control date-input"
            :value="filters.toDate"
            @change="$emit('update:toDate', $event.target.value)"
            title="Đến ngày"
          />
        </div>

        <!-- Nút hành động -->
        <div class="filter-actions-right">
          <button 
            type="button" 
            class="btn btn-secondary btn-icon-text"
            :disabled="loadingHistory"
            @click="$emit('refresh')"
            title="Tải lại dữ liệu lịch sử"
          >
            <span :class="{ 'spin-anim': loadingHistory }">🔄</span>
            <span>Làm Mới</span>
          </button>

          <button 
            type="button" 
            class="btn btn-success btn-icon-text"
            @click="$emit('export')"
            title="Xuất kết quả lọc ra file CSV"
          >
            <span>📥</span>
            <span>Xuất CSV</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 3. DÒNG THỜI GIAN LỊCH SỬ (TIMELINE) & PHÂN TRANG -->
    <div class="glass-card table-container" style="padding: 24px;">
      <!-- Thanh tóm tắt số lượng & chọn số dòng/trang -->
      <div class="history-list-header">
        <div class="list-count-badge">
          <span>Tìm thấy <strong>{{ filteredHistories.length }}</strong> sự kiện</span>
          <span v-if="filters.actionType" class="sub-filter-tag">
            • Lọc: {{ getHistoryTypeLabel(filters.actionType) }}
          </span>
        </div>

        <div class="page-size-select-wrap">
          <label class="ps-label">Hiển thị:</label>
          <select v-model="pageSize" class="form-control page-size-select">
            <option :value="15">15 sự kiện/trang</option>
            <option :value="30">30 sự kiện/trang</option>
            <option :value="50">50 sự kiện/trang</option>
            <option :value="100">100 sự kiện/trang</option>
          </select>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loadingHistory" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải dòng thời gian lịch sử nhân sự & thiết bị...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredHistories.length === 0" class="empty-state">
        <div class="empty-icon" style="font-size: 2.5rem; margin-bottom: 8px;">🔍</div>
        <p>Chưa có bản ghi lịch sử biến động nào phù hợp với bộ lọc hiện tại.</p>
        <button 
          v-if="filters.search || filters.actionType || filters.employeeId || filters.fromDate || filters.toDate" 
          type="button" 
          class="btn btn-secondary btn-sm"
          style="margin-top: 10px;"
          @click="resetFilters"
        >
          Xóa toàn bộ bộ lọc
        </button>
      </div>

      <!-- Danh sách Timeline -->
      <div v-else class="history-timeline-wrap">
        <div 
          v-for="item in paginatedHistories" 
          :key="item.employeeHistoryID" 
          class="timeline-event-card"
          :class="`event-type-${item.actionType?.toLowerCase()}`"
        >
          <!-- Cột thời gian bên trái -->
          <div class="event-time-col">
            <span class="event-date">{{ formatDateFull(item.actionDate) }}</span>
            <span class="event-time">{{ formatTime(item.actionDate) }}</span>
            <span v-if="isToday(item.actionDate)" class="badge-today">Hôm nay</span>
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

            <HistoryDiff :old-value="item.oldValue" :new-value="item.newValue" />
          </div>
        </div>
      </div>

      <!-- Thanh phân trang (Pagination Footer) -->
      <div v-if="totalPages > 1" class="history-pagination-footer">
        <div class="pagination-info">
          Hiển thị <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong> - 
          <strong>{{ Math.min(currentPage * pageSize, filteredHistories.length) }}</strong> 
          trong tổng số <strong>{{ filteredHistories.length }}</strong> sự kiện
        </div>

        <div class="pagination-buttons">
          <button 
            type="button" 
            class="btn btn-secondary btn-sm"
            :disabled="currentPage === 1"
            @click="currentPage--"
          >
            ◀ Trang trước
          </button>

          <span class="page-indicator">
            Trang <strong>{{ currentPage }}</strong> / {{ totalPages }}
          </span>

          <button 
            type="button" 
            class="btn btn-secondary btn-sm"
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
          >
            Trang sau ▶
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import HistoryDiff from '@/components/common/HistoryDiff.vue'

const props = defineProps({
  histories: { type: Array, required: true },
  employees: { type: Array, default: () => [] },
  loadingHistory: { type: Boolean, default: false },
  filters: { type: Object, required: true }
})

const emit = defineEmits([
  'update:search',
  'update:actionType',
  'update:employeeId',
  'update:datePreset',
  'update:fromDate',
  'update:toDate',
  'refresh',
  'export'
])

// Pagination State
const currentPage = ref(1)
const pageSize = ref(15)

// Reset về trang 1 khi filters hoặc dữ liệu thay đổi
watch(() => [props.filters.search, props.filters.actionType, props.filters.employeeId, props.filters.fromDate, props.filters.toDate], () => {
  currentPage.value = 1
})

// Filter client-side dự phòng
const filteredHistories = computed(() => {
  return props.histories || []
})

const totalPages = computed(() => {
  const count = filteredHistories.value.length
  return Math.ceil(count / pageSize.value) || 1
})

const paginatedHistories = computed(() => {
  const list = filteredHistories.value
  const start = (currentPage.value - 1) * pageSize.value
  return list.slice(start, start + pageSize.value)
})

// KPI Counters
const totalCount = computed(() => props.histories.length)

const countAssigned = computed(() => {
  return props.histories.filter(h => h.actionType === 'AssetAssigned').length
})

const countTransferred = computed(() => {
  return props.histories.filter(h => h.actionType === 'AssetTransferred').length
})

const countReturned = computed(() => {
  return props.histories.filter(h => h.actionType === 'AssetReturned' || h.actionType === 'AssetBroken' || h.actionType === 'AssetMaintenance').length
})

const countAccounts = computed(() => {
  return props.histories.filter(h => h.actionType === 'AccountUpdated' || h.actionType === 'ProfileUpdated' || h.actionType === 'OnboardingCreated' || h.actionType === 'Create' || h.actionType === 'UpdateAccounts').length
})

const filterByType = (type) => {
  emit('update:actionType', type)
}

const resetFilters = () => {
  emit('update:search', '')
  emit('update:actionType', '')
  emit('update:employeeId', null)
  emit('update:datePreset', 'all')
}

const getHistoryIcon = (type) => {
  switch (type) {
    case 'StartedWorking': return '💼'
    case 'OnboardingCreated':
    case 'Create': return '🚀'
    case 'UrgentMarked': return '🚨'
    case 'UrgentUnmarked': return '✕'
    case 'AssetAssigned': return '📦'
    case 'AssetReturned': return '📥'
    case 'AssetTransferred': return '🔄'
    case 'AssetBroken': return '🛠️'
    case 'AssetMaintenance': return '⚙️'
    case 'HandoverUploaded':
    case 'UploadPDF': return '📑'
    case 'HandoverDeleted':
    case 'DeletePDF': return '🗑️'
    case 'AccountUpdated':
    case 'UpdateAccounts': return '🔑'
    case 'ProfileUpdated': return '✏️'
    case 'Resigned': return '📁'
    case 'ImportAccounts': return '📋'
    default: return '🕒'
  }
}

const getHistoryTypeLabel = (type) => {
  switch (type) {
    case 'StartedWorking': return 'Chính Thức Đi Làm'
    case 'OnboardingCreated':
    case 'Create': return 'Tạo Hồ Sơ Mới'
    case 'UrgentMarked': return 'Báo Khẩn Cấp'
    case 'UrgentUnmarked': return 'Hủy Khẩn Cấp'
    case 'AssetAssigned': return 'Cấp Phát Thiết Bị'
    case 'AssetReturned': return 'Thu Hồi Thiết Bị'
    case 'AssetTransferred': return 'Điều Chuyển Thiết Bị'
    case 'AssetBroken': return 'Báo Hỏng Thiết Bị'
    case 'AssetMaintenance': return 'Bảo Trì Thiết Bị'
    case 'HandoverUploaded':
    case 'UploadPDF': return 'Tải Biên Bản PDF'
    case 'HandoverDeleted':
    case 'DeletePDF': return 'Gỡ Biên Bản PDF'
    case 'AccountUpdated':
    case 'UpdateAccounts': return 'Tài Khoản Hệ Thống'
    case 'ProfileUpdated': return 'Cập Nhật Hồ Sơ'
    case 'Resigned': return 'Đã Nghỉ Việc'
    case 'ImportAccounts': return 'Đồng Bộ Tài Khoản Excel'
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

const isToday = (dateStr) => {
  if (!dateStr) return false
  const d = new Date(dateStr)
  const now = new Date()
  return d.toDateString() === now.toDateString()
}
</script>

<style scoped>
.history-tab-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 1. KPI Cards */
.history-kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 14px;
}

.kpi-card {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 14px 18px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}

.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
  border-color: #cbd5e1;
}

.kpi-card.kpi-active {
  border-color: #3b82f6;
  background: #f0f7ff;
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
}

.kpi-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.35rem;
  flex-shrink: 0;
}

.kpi-all { background: #f1f5f9; color: #475569; }
.kpi-assign { background: #f5f3ff; color: #7c3aed; }
.kpi-transfer { background: #e0f2fe; color: #0284c7; }
.kpi-return { background: #fff7ed; color: #ea580c; }
.kpi-account { background: #ecfeff; color: #0891b2; }

.kpi-info {
  display: flex;
  flex-direction: column;
}

.kpi-num {
  font-size: 1.35rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.2;
}

.kpi-label {
  font-size: 0.775rem;
  color: #64748b;
  font-weight: 600;
  margin-top: 2px;
}

/* 2. Filter Toolbar */
.filter-card {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
}

.filter-top-row {
  display: grid;
  grid-template-columns: 1fr 240px 240px;
  gap: 14px;
}

.filter-bottom-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
  flex-wrap: wrap;
  padding-top: 12px;
  border-top: 1px dashed #e2e8f0;
}

.date-preset-group {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.preset-label {
  font-size: 0.8rem;
  font-weight: 700;
  color: #475569;
  margin-right: 4px;
}

.btn-preset {
  padding: 5px 12px;
  font-size: 0.785rem;
  font-weight: 600;
  border: 1px solid #cbd5e1;
  background: #f8fafc;
  color: #475569;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-preset:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
  color: #1e293b;
}

.btn-preset.active {
  background: #3b82f6;
  color: #ffffff;
  border-color: #3b82f6;
  box-shadow: 0 2px 6px rgba(59, 130, 246, 0.25);
}

.date-custom-range {
  display: flex;
  align-items: center;
  gap: 6px;
}

.date-input {
  width: 140px;
  padding: 5px 8px;
  font-size: 0.8rem;
}

.range-arrow {
  color: #94a3b8;
  font-size: 0.8rem;
}

.filter-actions-right {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: auto;
}

.btn-icon-text {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.825rem;
  font-weight: 600;
  padding: 7px 14px;
}

.spin-anim {
  display: inline-block;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 3. Header & Pagination */
.history-list-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f1f5f9;
}

.list-count-badge {
  font-size: 0.85rem;
  color: #475569;
}

.sub-filter-tag {
  color: #3b82f6;
  font-weight: 700;
  margin-left: 6px;
}

.page-size-select-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
}

.ps-label {
  font-size: 0.8rem;
  color: #64748b;
}

.page-size-select {
  width: 150px;
  padding: 4px 8px;
  font-size: 0.8rem;
}

.badge-today {
  font-size: 0.65rem;
  font-weight: 800;
  color: #dc2626;
  background: #fef2f2;
  border: 1px solid #fecaca;
  padding: 1px 5px;
  border-radius: 4px;
  margin-top: 4px;
  text-transform: uppercase;
}

/* Event Types Badges & Icons bổ trợ */
.badge-type-AssetTransferred { background: #e0f2fe; color: #0284c7; border: 1px solid #bae6fd; }
.badge-type-AssetBroken { background: #fee2e2; color: #b91c1c; border: 1px solid #fca5a5; }
.badge-type-AssetMaintenance { background: #fef3c7; color: #d97706; border: 1px solid #fde68a; }
.badge-type-HandoverUploaded, .badge-type-UploadPDF { background: #f0fdf4; color: #16a34a; border: 1px solid #bbf7d0; }
.badge-type-HandoverDeleted, .badge-type-DeletePDF { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
.badge-type-ImportAccounts { background: #f3e8ff; color: #9333ea; border: 1px solid #e9d5ff; }

.event-node-icon.icon-AssetTransferred { border-color: #0284c7; background: #e0f2fe; }
.event-node-icon.icon-AssetBroken { border-color: #ef4444; background: #fee2e2; }
.event-node-icon.icon-AssetMaintenance { border-color: #f59e0b; background: #fef3c7; }
.event-node-icon.icon-HandoverUploaded, .event-node-icon.icon-UploadPDF { border-color: #16a34a; background: #f0fdf4; }
.event-node-icon.icon-HandoverDeleted, .event-node-icon.icon-DeletePDF { border-color: #991b1b; background: #fef2f2; }
.event-node-icon.icon-AccountUpdated, .event-node-icon.icon-UpdateAccounts { border-color: #0891b2; background: #ecfeff; }
.event-node-icon.icon-ProfileUpdated { border-color: #64748b; background: #f1f5f9; }
.event-node-icon.icon-Resigned { border-color: #b91c1c; background: #fef2f2; }
.event-node-icon.icon-ImportAccounts { border-color: #9333ea; background: #f3e8ff; }

/* Pagination Footer */
.history-pagination-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 24px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
  flex-wrap: wrap;
  gap: 12px;
}

.pagination-info {
  font-size: 0.825rem;
  color: #64748b;
}

.pagination-buttons {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-indicator {
  font-size: 0.825rem;
  color: #334155;
}
</style>
