<template>
  <div class="tab-content">
    <!-- Cảnh báo chế độ chỉ xem cho HR -->
    <div v-if="isHR && currentTab === 'active'" class="hr-readonly-banner">
      🔒 <strong>Quyền Hạn Nhân Sự (HR):</strong> Bạn đang ở chế độ chỉ xem danh sách nhân viên đang làm việc. Nghiệp vụ chỉnh sửa thông tin, khóa tài khoản và bàn giao cấp phát thiết bị do Quản Trị Viên (Admin) và bộ phận IT phụ trách.
    </div>

    <!-- Filter Bar -->
    <div class="glass-card filter-card">
      <div class="filter-grid" style="grid-template-columns: 1fr 240px;">
        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">{{ currentTab === 'resigned' ? $t('employees.filter_search_resigned') : $t('employees.filter_search_active') }}</label>
          <input 
            type="text" 
            class="form-control" 
            :value="search" 
            :placeholder="$t('employees.filter_placeholder')" 
            @input="$emit('update:search', $event.target.value)"
          />
        </div>

        <div class="form-group" style="margin-bottom: 0;">
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
      </div>
    </div>

    <!-- Employees Table -->
    <div class="glass-card table-container">
      <div v-if="loading" class="table-loading">
        <span class="loading-spinner"></span>
        <p>{{ $t('common.loading') }}</p>
      </div>

      <div v-else-if="employees.length === 0" class="empty-state">
        <p v-if="currentTab === 'resigned'">{{ $t('employees.empty_resigned') }}</p>
        <p v-else>{{ $t('employees.empty_active') }}</p>
      </div>

      <div v-else class="table-responsive">
        <table class="custom-table">
          <thead>
            <tr>
              <th style="min-width: 280px; width: 32%;">{{ $t('employees.table_employee_contact') }}</th>
              <th style="min-width: 180px; width: 22%;">{{ $t('employees.table_dept_title') }}</th>
              <th v-if="currentTab === 'resigned'" style="text-align: center; width: 130px;">{{ $t('employees.table_leave_date') }}</th>
              <th style="text-align: center; width: 85px;">QAD</th>
              <th style="text-align: center; width: 85px;">OA</th>
              <th style="text-align: center; width: 85px;">Email</th>
              <th style="text-align: center; width: 85px;">AD</th>
              <th style="text-align: center; width: 130px;">{{ $t('employees.table_assets') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="emp in paginatedEmployees" :key="emp.employeeID" :class="{ 'row-resigned': currentTab === 'resigned' }">
              <td>
                <div 
                  class="emp-profile-card"
                  :class="{ 'clickable-profile': !isHR, 'cursor-default': isHR }" 
                  @click="!isHR && $emit('edit', emp)"
                >
                  <!-- Avatar tròn viết tắt tên -->
                  <div class="emp-avatar-circle" :style="{ background: getAvatarBg(emp.fullName) }">
                    {{ getInitials(emp.fullName) }}
                  </div>

                  <!-- Khối thông tin chi tiết -->
                  <div class="emp-profile-body">
                    <div class="emp-name-row">
                      <strong class="emp-name" :class="{ 'clickable-name': !isHR }">{{ emp.fullName }}</strong>
                      <span v-if="emp.englishName" class="emp-en-name">({{ emp.englishName }})</span>
                      <span class="emp-code-badge" :class="{ 'no-code': !emp.employeeCode }">{{ emp.employeeCode || $t('employees.no_code') }}</span>
                      <span v-if="emp.status === 'Resigned' || emp.leaveDate" class="badge-resigned-mini">{{ $t('employees.resigned_badge') }}</span>
                      <span v-if="!isHR" class="edit-hint-icon">✏️</span>
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
              <td>
                <div class="dept-cell-modern">
                  <div class="dept-text-main">{{ emp.departmentName }}</div>
                  <span class="title-badge-modern">{{ emp.title || 'Staff' }}</span>
                </div>
              </td>

              <!-- Cột ngày nghỉ việc (Dành riêng cho Tab Đã Nghỉ Việc) -->
              <td v-if="currentTab === 'resigned'" style="text-align: center;">
                <span class="leave-date-badge">{{ emp.leaveDate ? formatDate(emp.leaveDate) : $t('employees.resigned_badge') }}</span>
              </td>

              <!-- 4 Cột Tài khoản kèm theo -->
              <td 
                style="text-align: center;" 
                :class="{ 'cursor-pointer': !isHR, 'cursor-default': isHR }"
                @click="!isHR && $emit('open-accounts', emp)" 
              >
                <StatusBadge :status="emp.qaD_Status" type="account" />
              </td>
              <td 
                style="text-align: center;" 
                :class="{ 'cursor-pointer': !isHR, 'cursor-default': isHR }"
                @click="!isHR && $emit('open-accounts', emp)" 
              >
                <StatusBadge :status="emp.oA_Status" type="account" />
              </td>
              <td 
                style="text-align: center;" 
                :class="{ 'cursor-pointer': !isHR, 'cursor-default': isHR }"
                @click="!isHR && $emit('open-accounts', emp)" 
              >
                <StatusBadge :status="emp.email_Status" type="account" />
              </td>
              <td 
                style="text-align: center;" 
                :class="{ 'cursor-pointer': !isHR, 'cursor-default': isHR }"
                @click="!isHR && $emit('open-accounts', emp)" 
              >
                <StatusBadge :status="emp.aD_Status" type="account" />
              </td>

              <!-- Cột Thiết bị -->
              <td style="text-align: center;">
                <div class="emp-holding-cell" style="justify-content: center; flex-direction: column; gap: 4px; align-items: center;">
                  <button 
                    v-if="emp.holdingAssetCount > 0" 
                    type="button"
                    class="holding-pill active"
                    :class="{ 'holding-warning-pill': currentTab === 'resigned' }"
                    @click="$emit('open-assets', emp)"
                  >
                    <span class="holding-icon">📦</span>
                    <span class="holding-text">{{ $t('employees.holding_count', { count: emp.holdingAssetCount }) }}</span>
                  </button>
                  <span 
                    v-if="emp.missingHandoverDocCount > 0" 
                    class="badge-missing-doc-mini"
                    @click.stop="$emit('open-assets', emp)"
                  >
                    ⚠️ {{ emp.missingHandoverDocCount }}
                  </span>
                  <span v-else-if="currentTab === 'resigned'" class="returned-tag-mini">
                    ✓
                  </span>
                  <button 
                    v-else-if="!isHR && emp.holdingAssetCount === 0"
                    type="button"
                    class="holding-pill empty"
                    @click="$emit('open-assets', emp)"
                  >
                    <span class="holding-text">{{ $t('employees.no_assets_held') }}</span>
                    <span class="holding-btn-action">+</span>
                  </button>
                  <span v-else-if="emp.holdingAssetCount === 0" class="holding-pill empty cursor-default">
                    <span class="holding-text" style="color: #94a3b8;">{{ $t('employees.no_assets_held') }}</span>
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Phân trang nhân viên -->
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
  currentTab: { type: String, required: true },
  employees: { type: Array, required: true },
  departments: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  isHR: { type: Boolean, default: false },
  search: { type: String, default: '' },
  departmentId: { type: Number, default: null },
  currentPage: { type: Number, default: 1 },
  pageSize: { type: Number, default: 15 }
})

defineEmits([
  'update:search',
  'update:departmentId',
  'update:currentPage',
  'update:pageSize',
  'edit',
  'open-accounts',
  'open-assets',
  'open-history'
])

const paginatedEmployees = computed(() => {
  const start = (props.currentPage - 1) * props.pageSize
  return props.employees.slice(start, start + props.pageSize)
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
</style>
