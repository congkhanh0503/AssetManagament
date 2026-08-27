<template>
  <div class="tab-content">
    <!-- 1. BỘ LỌC TÌM KIẾM & LỌC NGÀY VÀO LÀM -->
    <div class="glass-card filter-card">
      <div class="filter-controls-wrap">
        <!-- Ô tìm kiếm từ khóa -->
        <div class="form-group filter-search-group">
          <label class="form-label">{{ $t('employees.onboarding_search') }}</label>
          <div class="search-input-box">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="search-icon">
              <circle cx="11" cy="11" r="8"></circle>
              <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
            </svg>
            <input 
              type="text" 
              class="form-control form-control-with-icon" 
              :value="search" 
              :placeholder="$t('employees.filter_placeholder')" 
              @input="$emit('update:search', $event.target.value)"
            />
          </div>
        </div>

        <!-- Lọc phòng ban -->
        <div class="form-group filter-dept-group">
          <label class="form-label">{{ $t('employees.table_department') }}</label>
          <select 
            class="form-control" 
            :value="departmentId" 
            @change="$emit('update:departmentId', $event.target.value ? Number($event.target.value) : null)"
          >
            <option :value="null">{{ $t('employees.filter_all_depts') }}</option>
            <option v-for="dept in departments" :key="dept.departmentID" :value="dept.departmentID">
              {{ dept.departmentName }}
            </option>
          </select>
        </div>

        <!-- Lọc ngày dự kiến vào làm -->
        <div class="form-group filter-date-preset-group">
          <label class="form-label">{{ $t('employees.onboarding_period') }}</label>
          <select 
            class="form-control" 
            :value="dateFilterPreset"
            @change="$emit('update:dateFilterPreset', $event.target.value)"
          >
            <option value="all">{{ $t('employees.all_dates') }}</option>
            <option value="7days">{{ $t('employees.next_7days') }}</option>
            <option value="14days">{{ $t('employees.next_14days') }}</option>
            <option value="30days">{{ $t('employees.next_30days') }}</option>
            <option value="this_month">{{ $t('employees.this_month_preset') }}</option>
            <option value="next_month">{{ $t('employees.next_month_preset') }}</option>
            <option value="custom">{{ $t('employees.custom_range') }}</option>
          </select>
        </div>

        <!-- Cụm nút hành động nhanh -->
        <div class="filter-action-group">
          <button 
            type="button" 
            class="btn btn-secondary btn-action-onboarding btn-email-template" 
            @click="$emit('export-email-template')" 
          >
            {{ $t('employees.btn_export_email_template') }}
          </button>
          <button 
            type="button" 
            class="btn btn-secondary btn-action-onboarding" 
            @click="$emit('import')" 
          >
            📥 {{ $t('employees.btn_import') }}
          </button>
          <button 
            type="button" 
            class="btn btn-secondary btn-action-onboarding" 
            @click="$emit('export')" 
          >
            📊 {{ $t('employees.btn_export') }}
          </button>
          <button 
            type="button" 
            class="btn btn-primary btn-add-onboarding" 
            @click="$emit('create')"
          >
            ➕ {{ $t('common.add') }}
          </button>
        </div>
      </div>

      <!-- Khung tùy chọn ngày cụ thể (Từ ngày - Đến ngày) -->
      <div v-if="dateFilterPreset === 'custom'" class="custom-date-row">
        <div class="custom-date-col">
          <label class="custom-date-lbl">{{ $t('employees.from_date') }}</label>
          <input 
            type="date" 
            class="form-control form-control-sm" 
            :value="fromDate" 
            @input="$emit('update:fromDate', $event.target.value)"
          />
        </div>
        <div class="custom-date-col">
          <label class="custom-date-lbl">{{ $t('employees.to_date') }}</label>
          <input 
            type="date" 
            class="form-control form-control-sm" 
            :value="toDate" 
            @input="$emit('update:toDate', $event.target.value)"
          />
        </div>
        <button 
          v-if="fromDate || toDate" 
          type="button" 
          class="btn btn-sm btn-secondary btn-clear-date"
          @click="$emit('clear-custom-date')"
        >
          {{ $t('employees.clear_date_filter') }}
        </button>
      </div>
    </div>

    <!-- 2. BẢNG TOÀN BỘ DANH SÁCH NHÂN SỰ MỚI CHỜ ĐI LÀM -->
    <div class="glass-card table-container">
      <div v-if="loading" class="table-loading">
        <span class="loading-spinner"></span>
        <p>{{ $t('common.loading') }}</p>
      </div>

      <div v-else-if="employees.length === 0" class="empty-state">
        <div class="empty-icon-large">🌱</div>
        <h4>{{ $t('employees.empty_onboarding_title') }}</h4>
        <p>{{ $t('employees.empty_onboarding_desc') }}</p>
        <button type="button" class="btn btn-primary" style="margin-top: 12px;" @click="$emit('create')">
          ➕ {{ $t('employees.btn_add') }}
        </button>
      </div>

      <div v-else class="table-responsive">
        <table class="custom-table">
          <thead>
            <tr>
              <th style="width: 280px;">{{ $t('employees.table_employee_contact') }}</th>
              <th>{{ $t('employees.table_dept_title') }}</th>
              <th style="text-align: center; width: 190px;">{{ $t('employees.table_join_date') }}</th>
              <th style="text-align: center; width: 140px;">{{ $t('employees.table_email') }}</th>
              <th style="text-align: center; width: 150px;">{{ $t('employees.table_assets') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="emp in paginatedEmployees" 
              :key="emp.employeeID"
            >
              <!-- 1. Thông tin nhân viên & liên hệ -->
              <td>
                <div 
                  class="emp-profile-card clickable-profile" 
                  @click="$emit('edit', emp)"
                >
                  <!-- Avatar tròn viết tắt tên -->
                  <div class="emp-avatar-circle" :style="{ background: getAvatarBg(emp.fullName) }">
                    {{ getInitials(emp.fullName) }}
                  </div>

                  <!-- Khối thông tin chi tiết -->
                  <div class="emp-profile-body">
                    <div class="emp-name-row">
                      <strong class="emp-name clickable-name">{{ emp.fullName }}</strong>
                      <span v-if="emp.englishName" class="emp-en-name">({{ emp.englishName }})</span>
                      <span class="emp-code-badge" :class="{ 'no-code': !emp.employeeCode }">
                        {{ emp.employeeCode || $t('employees.no_code') }}
                      </span>
                    </div>

                    <div class="emp-contact-details">
                      <span v-if="emp.email" class="contact-pill email">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                          <polyline points="22,6 12,13 2,6"></polyline>
                        </svg>
                        <span class="contact-val">{{ emp.email }}</span>
                      </span>
                      <span v-if="emp.phone" class="contact-pill phone">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path>
                        </svg>
                        <span class="contact-val">{{ emp.phone }}</span>
                      </span>
                      <span v-if="!emp.email && !emp.phone" class="contact-empty">---</span>
                    </div>
                  </div>
                </div>
              </td>

              <!-- 2. Bộ phận & Chức danh -->
              <td>
                <div class="dept-cell-modern">
                  <div class="dept-text-main">{{ emp.departmentName }}</div>
                  <span class="title-badge-modern">{{ emp.title || 'Staff' }}</span>
                </div>
              </td>

              <!-- 3. Ngày vào làm & đếm ngược -->
              <td style="text-align: center;">
                <div class="onboarding-date-wrap">
                  <strong class="font-mono text-primary font-bold">{{ formatDate(emp.joinDate) }}</strong>
                  <span 
                    class="badge-countdown-pill" 
                    :class="{ 
                      'is-urgent': getDaysUntilJoin(emp.joinDate) <= 2, 
                      'is-warning': getDaysUntilJoin(emp.joinDate) > 2 && getDaysUntilJoin(emp.joinDate) <= 7 
                    }"
                  >
                    {{ $t('employees.days_remaining_join', { days: getDaysUntilJoin(emp.joinDate) }) }}
                  </span>
                </div>
              </td>

              <!-- 4. Email Công Ty -->
              <td 
                style="text-align: center;" 
                :class="{ 'cursor-pointer': !isHR, 'cursor-default': isHR }" 
                @click="!isHR && $emit('open-accounts', emp)" 
              >
                <StatusBadge :status="emp.email_Status" type="account" />
              </td>

              <!-- 5. Thiết bị cấp phát -->
              <td style="text-align: center;">
                <div style="display: flex; flex-direction: column; align-items: center; gap: 4px; justify-content: center;">
                  <button 
                    v-if="emp.holdingAssetCount > 0"
                    class="asset-count-badge-btn has-assets" 
                    @click="$emit('open-assets', emp)"
                  >
                    <span>{{ $t('employees.devices_assigned_count', { count: emp.holdingAssetCount }) }}</span>
                  </button>
                  <span 
                    v-if="emp.missingHandoverDocCount > 0" 
                    class="badge-missing-doc-mini"
                    @click.stop="$emit('open-assets', emp)"
                  >
                    ⚠️ {{ emp.missingHandoverDocCount }}
                  </span>
                  <button 
                    v-else-if="!isHR && emp.holdingAssetCount === 0"
                    class="asset-count-badge-btn no-assets" 
                    @click="$emit('open-assets', emp)"
                    title="Bấm để cấp phát thiết bị mới"
                  >
                    <span class="text-missing-device">➕ Cấp máy</span>
                  </button>
                  <span 
                    v-else-if="emp.holdingAssetCount === 0" 
                    class="asset-count-badge-btn no-assets cursor-default" 
                    title="Nhân sự mới chưa được bộ phận IT cấp máy"
                  >
                    <span style="color: #94a3b8; font-weight: 600;">Chưa cấp máy</span>
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Phân trang -->
      <Pagination 
        :current-page="currentPage" 
        :page-size="pageSize" 
        :total-items="employees.length" 
        @update:currentPage="$emit('update:currentPage', $event)"
        @update:pageSize="$emit('update:pageSize', $event)"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import StatusBadge from '@/components/common/StatusBadge.vue'
import Pagination from '@/components/common/Pagination.vue'

const props = defineProps({
  employees: { type: Array, required: true },
  departments: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  isHR: { type: Boolean, default: false },
  search: { type: String, default: '' },
  departmentId: { type: Number, default: null },
  dateFilterPreset: { type: String, default: 'all' },
  fromDate: { type: String, default: '' },
  toDate: { type: String, default: '' },
  currentPage: { type: Number, default: 1 },
  pageSize: { type: Number, default: 15 }
})

defineEmits([
  'update:search',
  'update:departmentId',
  'update:dateFilterPreset',
  'update:fromDate',
  'update:toDate',
  'clear-custom-date',
  'update:currentPage',
  'update:pageSize',
  'import',
  'export',
  'export-email-template',
  'create',
  'edit',
  'delete',
  'open-accounts',
  'open-assets',
  'open-history'
])

const paginatedEmployees = computed(() => {
  const start = (props.currentPage - 1) * props.pageSize
  return props.employees.slice(start, start + props.pageSize)
})

const getDaysUntilJoin = (joinDateStr) => {
  if (!joinDateStr) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const joinDate = new Date(joinDateStr)
  joinDate.setHours(0, 0, 0, 0)
  const diffTime = joinDate - today
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24))
}

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
.filter-controls-wrap {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  align-items: flex-end;
}

.filter-search-group {
  flex: 1;
  min-width: 240px;
  margin-bottom: 0;
}

.filter-dept-group {
  width: 200px;
  margin-bottom: 0;
}

.filter-date-preset-group {
  width: 180px;
  margin-bottom: 0;
}

.filter-action-group {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 0;
  margin-left: auto;
}

.btn-action-onboarding,
.btn-add-onboarding {
  white-space: nowrap;
  height: 38px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.85rem;
}

.btn-email-template {
  background: #eff6ff;
  color: #2563eb;
  border-color: #bfdbfe;
}

.btn-email-template:hover {
  background: #dbeafe;
  border-color: #93c5fd;
  color: #1d4ed8;
}

.custom-date-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed var(--border-color);
  flex-wrap: wrap;
}

.custom-date-col {
  display: flex;
  align-items: center;
  gap: 8px;
}

.custom-date-lbl {
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  white-space: nowrap;
}

.btn-clear-date {
  font-size: 0.775rem;
  padding: 4px 10px;
}

.emp-en-name {
  font-size: 0.8rem;
  color: var(--primary);
  font-weight: 600;
  white-space: nowrap;
}

.emp-code-badge.no-code {
  background: #f1f5f9;
  color: #94a3b8;
  border: 1px dashed #cbd5e1;
  font-style: italic;
}

.badge-missing-doc-mini {
  font-size: 0.7rem;
  font-weight: 700;
  color: #c2410c;
  background: #fff7ed;
  border: 1px solid #fed7aa;
  padding: 1px 6px;
  border-radius: 4px;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.2s ease;
}

.badge-missing-doc-mini:hover {
  background: #ffedd5;
  border-color: #f97316;
  color: #9a3412;
}

.empty-icon-large {
  font-size: 2.5rem;
  margin-bottom: 8px;
}

.action-btn-group {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.btn-icon-action {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  border: 1px solid var(--border-color);
  background: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-icon-action:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
  transform: translateY(-1px);
}

@media (max-width: 992px) {
  .filter-controls-wrap {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 600px) {
  .filter-controls-wrap {
    grid-template-columns: 1fr;
  }
}
</style>
