<template>
  <Modal 
    :is-open="isOpen" 
    title="Lịch Sử Cấp Phát, Thu Hồi & Điều Chuyển Thiết Bị"
    subtitle="Theo dõi và tra cứu toàn bộ dòng luân chuyển tài sản IT theo tuần, tháng và tùy chọn thời gian"
    @close="$emit('close')"
    max-width="1050px"
  >
    <div class="history-modal-body">
      <!-- 1. Bộ Lọc Thời Gian & Thao Tác -->
      <div class="history-filters-bar">
        <!-- Filter Period Buttons -->
        <div class="period-toggle-group">
          <button 
            type="button"
            :class="['period-btn', { active: filters.period === 'week' }]"
            @click="setPeriod('week')"
          >
            Tuần Này (7 ngày)
          </button>
          <button 
            type="button"
            :class="['period-btn', { active: filters.period === 'month' }]"
            @click="setPeriod('month')"
          >
            Tháng Này (30 ngày)
          </button>
          <button 
            type="button"
            :class="['period-btn', { active: filters.period === 'last_month' }]"
            @click="setPeriod('last_month')"
          >
            Tháng Trước
          </button>
          <button 
            type="button"
            :class="['period-btn', { active: filters.period === 'all' }]"
            @click="setPeriod('all')"
          >
            Tất Cả
          </button>
          <button 
            type="button"
            :class="['period-btn', { active: filters.period === 'custom' }]"
            @click="setPeriod('custom')"
          >
            Tùy Chọn Ngày
          </button>
        </div>

        <div class="filter-controls-row">
          <!-- Custom Date Range -->
          <div v-if="filters.period === 'custom'" class="custom-date-inputs">
            <div class="date-input-wrap">
              <label>Từ ngày:</label>
              <input type="date" class="form-control form-control-sm" v-model="filters.fromDate" @change="fetchHistory" />
            </div>
            <div class="date-input-wrap">
              <label>Đến ngày:</label>
              <input type="date" class="form-control form-control-sm" v-model="filters.toDate" @change="fetchHistory" />
            </div>
          </div>

          <!-- Filter Action Type -->
          <div class="action-type-select-wrap">
            <select class="form-control form-control-sm" v-model="filters.actionType" @change="fetchHistory">
              <option value="">-- Tất cả thao tác --</option>
              <option value="Assign">Cấp Phát Mới</option>
              <option value="Return">Thu Hồi Về Kho</option>
              <option value="Transfer">Điều Chuyển Nhân Sự</option>
              <option value="SendMaintenance">Báo Hỏng / Đi Sửa</option>
            </select>
          </div>

          <!-- Search Box -->
          <div class="search-input-wrap">
            <input 
              type="text" 
              class="form-control form-control-sm" 
              v-model="filters.search" 
              placeholder="Tìm mã máy, tên máy, người nhận, người giao..." 
              @input="debounceFetch"
            />
            <button 
              v-if="filters.search" 
              type="button" 
              class="clear-search-btn" 
              @click="clearSearch"
              title="Xóa từ khóa tìm kiếm"
            >
              ✕
            </button>
          </div>

          <!-- Nút Xuất Excel Lịch Sử -->
          <button type="button" class="btn btn-sm btn-secondary" @click="exportHistoryCsv" title="Xuất toàn bộ lịch sử này ra Excel/CSV">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #34d399;">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
              <polyline points="7 10 12 15 17 10"></polyline>
              <line x1="12" y1="15" x2="12" y2="3"></line>
            </svg>
            Xuất Excel
          </button>
        </div>
      </div>

      <!-- 2. Thống Kê Tổng Hợp Trong Kỳ -->
      <div class="history-stats-banner">
        <div class="h-stat-box">
          <span class="h-stat-lbl">Tổng Giao Dịch:</span>
          <strong class="h-stat-val text-white">{{ histories.length }}</strong>
        </div>
        <div class="h-stat-box">
          <span class="h-stat-lbl">Cấp Phát Mới:</span>
          <strong class="h-stat-val text-green">{{ countByAction('Assign') }}</strong>
        </div>
        <div class="h-stat-box">
          <span class="h-stat-lbl">Thu Hồi Về Kho:</span>
          <strong class="h-stat-val text-yellow">{{ countByAction('Return') }}</strong>
        </div>
        <div class="h-stat-box">
          <span class="h-stat-lbl">Điều Chuyển:</span>
          <strong class="h-stat-val text-blue">{{ countByAction('Transfer') }}</strong>
        </div>
        <div class="h-stat-box">
          <span class="h-stat-lbl">Báo Hỏng / Đi Sửa:</span>
          <strong class="h-stat-val text-red">{{ countByAction('SendMaintenance') }}</strong>
        </div>
      </div>

      <!-- 3. Bảng Dữ Liệu Lịch Sử -->
      <div class="history-table-container">
        <div v-if="loading" class="table-loading">
          <span class="loading-spinner"></span>
          <p>Đang tải dữ liệu lịch sử cấp phát...</p>
        </div>

        <div v-else-if="histories.length === 0" class="empty-state" style="padding: 30px;">
          <p>Không tìm thấy bản ghi cấp phát / thu hồi nào trong <strong>{{ getPeriodLabel(filters.period) }}</strong>.</p>
          <div class="empty-quick-actions" style="margin-top: 10px; display: flex; gap: 8px; justify-content: center;">
            <button v-if="filters.period !== 'month'" type="button" class="btn btn-sm btn-secondary" @click="setPeriod('month')">
              📅 Xem Trong Tháng Này
            </button>
            <button v-if="filters.period !== 'all'" type="button" class="btn btn-sm btn-primary" @click="setPeriod('all')">
              🌐 Xem Toàn Bộ Lịch Sử
            </button>
          </div>
        </div>

        <div v-else>
          <div class="table-responsive history-table-scroll">
            <table class="data-table history-table">
              <thead>
                <tr>
                  <th>Thời Gian</th>
                  <th>Thao Tác</th>
                  <th>Thiết Bị</th>
                  <th>Bên Giao ➔ Bên Nhận</th>
                  <th>Vị Trí / Phòng Ban</th>
                  <th>Tình Trạng & Ghi Chú</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in paginatedHistories" :key="item.historyID">
                  <td>
                    <div class="time-main">{{ formatDate(item.actionDate) }}</div>
                    <div class="uploader-sub">Bởi: {{ item.createdBy || 'Admin' }}</div>
                  </td>

                  <td>
                    <span class="action-type-pill" :class="getActionClass(item.actionType)">
                      {{ item.actionTypeLabel }}
                    </span>
                  </td>

                  <td>
                    <div class="asset-code-row">
                      <span class="code-badge-mini">{{ item.assetCode }}</span>
                      <strong class="asset-name-text">{{ item.assetName }}</strong>
                    </div>
                    <div class="cat-brand-sub">{{ item.categoryName }} • {{ item.brand || '---' }}</div>
                  </td>

                  <td>
                    <!-- Assign: Từ Kho -> Nhân viên -->
                    <div v-if="item.actionType === 'Assign'" class="flow-cell">
                      <span class="flow-from">🏢 Kho IT</span>
                      <span class="flow-arrow">➔</span>
                      <span class="flow-to">👤 <strong>{{ item.toEmployeeName }}</strong> <span class="dim-code">({{ item.toEmployeeCode }})</span></span>
                    </div>

                    <!-- Return: Từ Nhân viên -> Kho -->
                    <div v-else-if="item.actionType === 'Return'" class="flow-cell">
                      <span class="flow-from">👤 {{ item.fromEmployeeName || 'Nhân viên' }}</span>
                      <span class="flow-arrow">➔</span>
                      <span class="flow-to">🏢 <strong>Kho IT</strong></span>
                    </div>

                    <!-- Transfer: Từ Nhân viên A -> Nhân viên B -->
                    <div v-else-if="item.actionType === 'Transfer'" class="flow-cell">
                      <span class="flow-from">👤 {{ item.fromEmployeeName }}</span>
                      <span class="flow-arrow">➔</span>
                      <span class="flow-to">👤 <strong>{{ item.toEmployeeName }}</strong></span>
                    </div>

                    <!-- Báo hỏng / Khác -->
                    <div v-else class="flow-cell">
                      <span class="flow-from">👤 {{ item.fromEmployeeName || 'Kho IT' }}</span>
                      <span class="flow-arrow">➔</span>
                      <span class="flow-to">🔧 Trung tâm bảo trì</span>
                    </div>
                  </td>

                  <td>
                    <div class="loc-text">📍 {{ item.toLocation || item.toDepartmentName || '---' }}</div>
                  </td>

                  <td>
                    <div class="cond-text">{{ item.conditionStatus || 'Bình thường' }}</div>
                    <div v-if="item.note" class="note-text">{{ item.note }}</div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Phân trang lịch sử -->
          <Pagination 
            v-model:currentPage="currentPage" 
            v-model:pageSize="pageSize" 
            :totalItems="histories.length" 
          />
        </div>
      </div>

      <div class="modal-actions-right" style="margin-top: 16px;">
        <button type="button" class="btn btn-secondary" @click="fetchHistory" :disabled="loading">
          🔄 Làm Mới
        </button>
        <button type="button" class="btn btn-primary" @click="$emit('close')">Đóng</button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { assetsApi } from '@/api/client'
import Modal from '@/components/common/Modal.vue'
import Pagination from '@/components/common/Pagination.vue'
import { exportToCsv } from '@/utils/exportCsv'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  initialPeriod: {
    type: String,
    default: 'month'
  }
})

defineEmits(['close'])

const histories = ref([])
const loading = ref(false)

// Phân trang
const currentPage = ref(1)
const pageSize = ref(10)

const paginatedHistories = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return histories.value.slice(start, end)
})

const filters = reactive({
  period: 'month',
  fromDate: null,
  toDate: null,
  actionType: '',
  search: ''
})

watch(() => props.isOpen, (val) => {
  if (val) {
    if (props.initialPeriod) filters.period = props.initialPeriod
    fetchHistory()
  }
})

onMounted(() => {
  if (props.isOpen) {
    fetchHistory()
  }
})

watch(filters, () => {
  currentPage.value = 1
})

const getPeriodLabel = (p) => {
  switch (p) {
    case 'week': return 'Tuần này (7 ngày qua)'
    case 'month': return 'Tháng này (30 ngày qua)'
    case 'last_month': return 'Tháng trước'
    case 'custom': return 'Khoảng ngày tùy chọn'
    case 'all': return 'Toàn bộ thời gian'
    default: return p
  }
}

const setPeriod = (p) => {
  filters.period = p
  if (p !== 'custom') {
    filters.fromDate = null
    filters.toDate = null
    fetchHistory()
  }
}

let debounceTimer = null
const debounceFetch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    fetchHistory()
  }, 300)
}

const fetchHistory = async () => {
  loading.value = true
  try {
    const params = {
      period: filters.period,
      actionType: filters.actionType || undefined,
      search: filters.search || undefined,
      fromDate: filters.fromDate || undefined,
      toDate: filters.toDate || undefined
    }

    const res = await assetsApi.getHandoverHistory(params)
    histories.value = res || []
  } catch (err) {
    console.error('Lỗi tải lịch sử cấp phát:', err)
  } finally {
    loading.value = false
  }
}

const countByAction = (type) => {
  if (type === 'SendMaintenance' || type === 'Report-Broken') {
    return histories.value.filter(h => h.actionType === 'SendMaintenance' || h.actionType === 'Report-Broken').length
  }
  return histories.value.filter(h => h.actionType === type).length
}

const getActionClass = (type) => {
  return (type || '').toLowerCase()
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

const clearSearch = () => {
  filters.search = ''
  fetchHistory()
}

const exportHistoryCsv = () => {
  const columns = [
    { label: 'Thời Gian', field: (row) => formatDate(row.actionDate) },
    { label: 'Thao Tác', field: 'actionTypeLabel' },
    { label: 'Mã Tài Sản', field: 'assetCode' },
    { label: 'Tên Thiết Bị', field: 'assetName' },
    { label: 'Loại', field: 'categoryName' },
    { label: 'Người Giao', field: (row) => row.fromEmployeeName ? `${row.fromEmployeeName} (${row.fromEmployeeCode})` : 'Kho IT' },
    { label: 'Người Nhận', field: (row) => row.toEmployeeName ? `${row.toEmployeeName} (${row.toEmployeeCode})` : 'Kho IT' },
    { label: 'Vị Trí / Phòng Ban', field: 'toLocation' },
    { label: 'Tình Trạng', field: 'conditionStatus' },
    { label: 'Ghi Chú', field: 'note' },
    { label: 'Người Thực Hiện', field: 'createdBy' }
  ]
  exportToCsv(`Lich_Su_Cap_Phat_${filters.period}`, histories.value, columns)
}
</script>

<style scoped>
.history-modal-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 1. Filters Bar */
.history-filters-bar {
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: rgba(255, 255, 255, 0.02);
  padding: 14px 16px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.period-toggle-group {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.period-btn {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--border-color);
  color: var(--text-main);
  padding: 6px 14px;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
  font-weight: 500;
  cursor: pointer;
  transition: var(--transition);
}

.period-btn:hover {
  background: rgba(99, 102, 241, 0.15);
  border-color: var(--primary);
  color: #ffffff;
}

.period-btn.active {
  background: var(--primary);
  border-color: var(--primary);
  color: #ffffff;
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(99, 102, 241, 0.35);
}

.filter-controls-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.custom-date-inputs {
  display: flex;
  align-items: center;
  gap: 8px;
}

.date-input-wrap {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.775rem;
  color: var(--text-dim);
}

.action-type-select-wrap select {
  min-width: 180px;
}

.search-input-wrap {
  flex: 1;
  min-width: 220px;
  position: relative;
  display: flex;
  align-items: center;
}

.search-input-wrap input {
  padding-right: 28px;
}

.clear-search-btn {
  position: absolute;
  right: 8px;
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  font-size: 0.75rem;
  padding: 2px 4px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.clear-search-btn:hover {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
}


.form-control-sm {
  padding: 6px 12px;
  font-size: 0.8rem;
}

/* 2. Stats Banner */
.history-stats-banner {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 10px;
  background: #f8fafc;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  padding: 10px 14px;
}

.h-stat-box {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 10px;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: 4px;
}

.h-stat-lbl {
  font-size: 0.775rem;
  color: var(--text-dim);
}

.h-stat-val {
  font-size: 0.95rem;
  font-weight: 700;
}

.text-white { color: #0f172a; }
.text-green { color: #059669; }
.text-yellow { color: #d97706; }
.text-blue { color: #0284c7; }
.text-red { color: #dc2626; }

/* 3. Table */
.history-table-container {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.history-table-scroll {
  max-height: 400px;
  overflow-y: auto;
}

.history-table th {
  position: sticky;
  top: 0;
  background: #f1f5f9;
  z-index: 2;
  color: var(--text-muted);
}

.time-main {
  font-size: 0.825rem;
  font-weight: 600;
  color: #0f172a;
}

.uploader-sub {
  font-size: 0.7rem;
  color: var(--text-dim);
}

.action-type-pill {
  display: inline-block;
  font-size: 0.725rem;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 4px;
  text-transform: uppercase;
}

.action-type-pill.assign {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
}

.action-type-pill.return {
  background: #fffbeb;
  color: #d97706;
  border: 1px solid #fde68a;
}

.action-type-pill.transfer {
  background: #f0f9ff;
  color: #0284c7;
  border: 1px solid #bae6fd;
}

.action-type-pill.report-broken,
.action-type-pill.sendmaintenance {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.asset-code-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.code-badge-mini {
  font-family: monospace;
  font-size: 0.75rem;
  font-weight: 700;
  color: #4f46e5;
  background: #eef2ff;
  padding: 1px 5px;
  border-radius: 3px;
  border: 1px solid #c7d2fe;
}

.asset-name-text {
  font-size: 0.85rem;
  color: #0f172a;
  font-weight: 600;
}

.cat-brand-sub {
  font-size: 0.725rem;
  color: var(--text-dim);
  margin-top: 2px;
}

.flow-cell {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.825rem;
}

.flow-from {
  color: var(--text-dim);
}

.flow-arrow {
  color: #6366f1;
  font-weight: bold;
}

.flow-to {
  color: #0f172a;
  font-weight: 600;
}

.dim-code {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.loc-text {
  font-size: 0.8rem;
  color: #059669;
  font-weight: 600;
}

.cond-text {
  font-size: 0.8rem;
  color: #0f172a;
}

.note-text {
  font-size: 0.725rem;
  color: var(--text-muted);
  font-style: italic;
  margin-top: 2px;
}
</style>
