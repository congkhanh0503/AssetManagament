<template>
  <div class="employee-missing-handover-tab">
    <!-- BANNER TỔNG QUAN CẢNH BÁO -->
    <div class="missing-summary-banner" v-if="alerts.length > 0">
      <div class="summary-icon-box">
        <span class="summary-icon">⚠️</span>
      </div>
      <div class="summary-content">
        <div class="summary-title">Cảnh Báo Thiết Bị Chưa Import Biên Bản Bàn Giao</div>
        <p class="summary-desc">
          Hiện có <strong>{{ alerts.length }} nhân sự</strong> đang nắm giữ tổng cộng 
          <strong class="text-danger">{{ totalMissingAssets }} thiết bị</strong> nhưng chưa được upload file scan biên bản bàn giao (PDF) có chữ ký xác nhận lên hệ thống.
        </p>
      </div>
      <div class="summary-stats-pills">
        <div class="stat-badge danger">
          <span class="stat-num">{{ totalMissingAssets }}</span>
          <span class="stat-lbl">Thiết bị thiếu biên bản</span>
        </div>
        <div class="stat-badge warning">
          <span class="stat-num">{{ alerts.length }}</span>
          <span class="stat-lbl">Nhân sự liên quan</span>
        </div>
      </div>
    </div>

    <!-- FILTER & SEARCH BAR -->
    <div class="tab-filter-card">
      <div class="filter-controls-wrap">
        <!-- Ô tìm kiếm -->
        <div class="form-group filter-search-group">
          <label class="form-label">Tìm kiếm nhân sự / thiết bị</label>
          <div class="search-input-box">
            <span class="search-icon">🔍</span>
            <input 
              type="text" 
              class="form-control form-control-with-icon" 
              placeholder="Tìm theo tên, tiếng Anh, mã NV, tên máy, mã thiết bị..." 
              v-model="searchQuery"
            />
          </div>
        </div>

        <!-- Lọc phòng ban -->
        <div class="form-group filter-dept-group">
          <label class="form-label">Phòng ban</label>
          <select class="form-control" v-model="selectedDept">
            <option value="">Tất cả phòng ban</option>
            <option v-for="dept in departments" :key="dept.departmentID" :value="dept.departmentName">
              {{ dept.departmentName }}
            </option>
          </select>
        </div>

        <!-- Nút tải lại -->
        <div class="filter-action-group">
          <button type="button" class="btn btn-secondary btn-refresh" @click="$emit('refresh')">
            🔄 Tải Lại Cảnh Báo
          </button>
        </div>
      </div>
    </div>

    <!-- DANH SÁCH BẢNG CẢNH BÁO -->
    <div class="tab-table-card">
      <div v-if="loading" class="table-loading-state">
        <div class="loading-spinner"></div>
        <span>Đang kiểm tra biên bản bàn giao...</span>
      </div>

      <div v-else-if="filteredAlerts.length === 0" class="empty-state-card">
        <div class="empty-icon-large">🎉</div>
        <h3>Tuyệt Vời! Không Có Thiết Bị Nào Thiếu Biên Bản</h3>
        <p>Toàn bộ thiết bị đang cấp phát cho nhân viên đều đã được import đầy đủ file scan biên bản bàn giao (PDF) lên hệ thống.</p>
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="min-width: 220px;">Nhân Sự</th>
              <th style="min-width: 170px;">Phòng Ban & Chức Danh</th>
              <th style="min-width: 320px;">Thiết Bị Chưa Có Biên Bản Scan</th>
              <th style="width: 130px; text-align: center;">Số Lượng Thiếu</th>
              <th style="width: 150px; text-align: right;">Thao Tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="emp in filteredAlerts" :key="emp.employeeID" class="missing-row">
              <!-- Nhân sự -->
              <td>
                <div class="emp-profile-cell">
                  <div class="emp-avatar-sm" :style="{ backgroundColor: getAvatarBg(emp.fullName) }">
                    {{ getInitials(emp.fullName) }}
                  </div>
                  <div class="emp-profile-text">
                    <div class="emp-name-row">
                      <strong class="emp-name">{{ emp.fullName }}</strong>
                      <span v-if="emp.englishName" class="emp-en-name">({{ emp.englishName }})</span>
                    </div>
                    <div class="emp-meta-small">
                      <span class="emp-code">{{ emp.employeeCode || 'Chưa có mã' }}</span>
                      <span class="emp-status-dot" :class="emp.status.toLowerCase()">{{ emp.status === 'Resigned' ? 'Nghỉ việc' : 'Đang làm' }}</span>
                    </div>
                  </div>
                </div>
              </td>

              <!-- Phòng ban -->
              <td>
                <div class="dept-cell">
                  <div class="dept-text">{{ emp.departmentName }}</div>
                  <div class="title-text">{{ emp.title || 'Nhân viên' }}</div>
                </div>
              </td>

              <!-- Danh sách thiết bị chưa có biên bản -->
              <td>
                <div class="missing-assets-list">
                  <div 
                    v-for="asset in emp.missingAssets" 
                    :key="asset.assetID"
                    class="missing-asset-card"
                  >
                    <div class="asset-card-header">
                      <span class="asset-cat-tag">{{ asset.categoryName || 'Thiết bị' }}</span>
                      <strong class="asset-code-badge">{{ asset.assetCode }}</strong>
                    </div>
                    <div class="asset-name-text" :title="asset.assetName">{{ asset.assetName }}</div>
                    <div class="asset-meta-text" v-if="asset.brand || asset.serialNumber">
                      <span v-if="asset.brand">{{ asset.brand }}</span>
                      <span v-if="asset.brand && asset.serialNumber"> • </span>
                      <span v-if="asset.serialNumber">SN: {{ asset.serialNumber }}</span>
                    </div>
                    <div class="asset-assigned-date" v-if="asset.assignedDate">
                      📅 Cấp ngày: {{ formatDate(asset.assignedDate) }}
                    </div>
                  </div>
                </div>
              </td>

              <!-- Số lượng thiếu -->
              <td style="text-align: center;">
                <span class="missing-pill-badge">
                  ⚠️ Thiếu {{ emp.missingCount }} biên bản
                </span>
              </td>

              <!-- Thao tác -->
              <td style="text-align: right;">
                <div class="row-actions">
                  <button 
                    type="button" 
                    class="btn btn-sm btn-primary btn-upload-now"
                    @click="$emit('open-assets', emp)"
                    title="Mở hồ sơ để tải lên file PDF scan biên bản bàn giao"
                  >
                    📤 Upload PDF
                  </button>
                  <button 
                    type="button" 
                    class="btn btn-sm btn-secondary btn-icon-only"
                    @click="$emit('open-assets', emp)"
                    title="Xem chi tiết tài sản & In biên bản bàn giao"
                  >
                    👁️
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  alerts: { type: Array, required: true },
  departments: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false }
})

defineEmits(['open-assets', 'refresh'])

const searchQuery = ref('')
const selectedDept = ref('')

const totalMissingAssets = computed(() => {
  return props.alerts.reduce((sum, item) => sum + (item.missingCount || 0), 0)
})

const filteredAlerts = computed(() => {
  let list = props.alerts || []

  if (selectedDept.value) {
    list = list.filter(e => e.departmentName === selectedDept.value)
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(e => {
      const matchEmp = (
        (e.fullName && e.fullName.toLowerCase().includes(q)) ||
        (e.englishName && e.englishName.toLowerCase().includes(q)) ||
        (e.employeeCode && e.employeeCode.toLowerCase().includes(q)) ||
        (e.departmentName && e.departmentName.toLowerCase().includes(q))
      )
      if (matchEmp) return true

      // Tìm trong danh sách thiết bị con
      const matchAsset = (e.missingAssets || []).some(a => 
        (a.assetCode && a.assetCode.toLowerCase().includes(q)) ||
        (a.assetName && a.assetName.toLowerCase().includes(q)) ||
        (a.categoryName && a.categoryName.toLowerCase().includes(q)) ||
        (a.brand && a.brand.toLowerCase().includes(q)) ||
        (a.serialNumber && a.serialNumber.toLowerCase().includes(q))
      )
      return matchAsset
    })
  }

  return list
})

const formatDate = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const getInitials = (name) => {
  if (!name) return 'NV'
  const parts = name.trim().split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

const getAvatarBg = (name) => {
  if (!name) return '#e0e7ff'
  const colors = [
    '#e0e7ff', '#fce7f3', '#dcfce7', '#fef3c7', 
    '#ccfbf1', '#ede9fe', '#ffedd5', '#e0f2fe'
  ]
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  const idx = Math.abs(hash) % colors.length
  return colors[idx]
}
</script>

<style scoped>
.employee-missing-handover-tab {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* BANNER */
.missing-summary-banner {
  background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%);
  border: 1px solid #fed7aa;
  border-radius: var(--radius-lg);
  padding: 18px 24px;
  display: flex;
  align-items: center;
  gap: 18px;
  box-shadow: 0 2px 8px rgba(249, 115, 22, 0.08);
}

.summary-icon-box {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: #ffedd5;
  border: 2px solid #fdba74;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
  flex-shrink: 0;
}

.summary-content {
  flex: 1;
}

.summary-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #9a3412;
  margin-bottom: 4px;
}

.summary-desc {
  font-size: 0.875rem;
  color: #7c2d12;
  margin: 0;
  line-height: 1.45;
}

.summary-stats-pills {
  display: flex;
  gap: 10px;
  flex-shrink: 0;
}

.stat-badge {
  padding: 8px 14px;
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 100px;
}

.stat-badge.danger {
  background: #fee2e2;
  border: 1px solid #fca5a5;
  color: #991b1b;
}

.stat-badge.warning {
  background: #fef3c7;
  border: 1px solid #fcd34d;
  color: #92400e;
}

.stat-badge .stat-num {
  font-size: 1.25rem;
  font-weight: 800;
  line-height: 1;
}

.stat-badge .stat-lbl {
  font-size: 0.725rem;
  font-weight: 600;
  margin-top: 4px;
  white-space: nowrap;
}

/* FILTER */
.tab-filter-card {
  background: var(--card-bg, #ffffff);
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: var(--radius-lg);
  padding: 16px;
}

.filter-controls-wrap {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  align-items: flex-end;
}

.filter-search-group {
  flex: 1;
  min-width: 260px;
  margin-bottom: 0;
}

.filter-dept-group {
  width: 220px;
  margin-bottom: 0;
}

.filter-action-group {
  margin-bottom: 0;
  margin-left: auto;
}

.btn-refresh {
  height: 38px;
  white-space: nowrap;
}

.search-input-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 12px;
  color: #94a3b8;
  pointer-events: none;
}

.form-control-with-icon {
  padding-left: 36px;
}

/* TABLE */
.tab-table-card {
  background: var(--card-bg, #ffffff);
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.missing-row:hover {
  background-color: #fffbf5 !important;
}

.emp-profile-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.emp-avatar-sm {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  color: #334155;
  flex-shrink: 0;
}

.emp-profile-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.emp-name-row {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.emp-name {
  font-size: 0.95rem;
  color: #0f172a;
}

.emp-en-name {
  font-size: 0.8rem;
  color: var(--primary, #4f46e5);
  font-weight: 600;
}

.emp-meta-small {
  display: flex;
  align-items: center;
  gap: 8px;
}

.emp-code {
  font-size: 0.75rem;
  font-family: monospace;
  font-weight: 700;
  background: #f1f5f9;
  color: #475569;
  padding: 1px 6px;
  border-radius: 4px;
}

.emp-status-dot {
  font-size: 0.725rem;
  color: #16a34a;
  font-weight: 500;
}

.emp-status-dot.resigned {
  color: #ef4444;
}

.dept-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.dept-text {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
}

.title-text {
  font-size: 0.775rem;
  color: #64748b;
}

/* CARDS THIẾT BỊ THIẾU BIÊN BẢN */
.missing-assets-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 4px 0;
}

.missing-asset-card {
  background: #fff8f5;
  border: 1px solid #fed7aa;
  border-radius: var(--radius-md, 8px);
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 3px;
  transition: all 0.2s ease;
}

.missing-asset-card:hover {
  border-color: #f97316;
  box-shadow: 0 2px 6px rgba(249, 115, 22, 0.1);
}

.asset-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.asset-cat-tag {
  font-size: 0.7rem;
  font-weight: 700;
  color: #c2410c;
  background: #ffedd5;
  padding: 1px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.asset-code-badge {
  font-size: 0.75rem;
  font-family: monospace;
  font-weight: 700;
  color: #ea580c;
}

.asset-name-text {
  font-size: 0.85rem;
  font-weight: 600;
  color: #0f172a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.asset-meta-text {
  font-size: 0.75rem;
  color: #64748b;
}

.asset-assigned-date {
  font-size: 0.725rem;
  color: #9a3412;
  font-weight: 500;
}

.missing-pill-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
  white-space: nowrap;
}

.row-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}

.btn-upload-now {
  white-space: nowrap;
  font-size: 0.8rem;
  padding: 6px 12px;
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
  border-color: #ea580c;
}

.btn-upload-now:hover {
  background: linear-gradient(135deg, #ea580c 0%, #c2410c 100%);
}

.btn-icon-only {
  width: 32px;
  height: 32px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.empty-state-card {
  padding: 48px 24px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}

.empty-icon-large {
  font-size: 3rem;
}

.empty-state-card h3 {
  font-size: 1.15rem;
  font-weight: 700;
  color: #15803d;
  margin: 0;
}

.empty-state-card p {
  font-size: 0.875rem;
  color: #64748b;
  max-width: 480px;
  margin: 0;
}
</style>
