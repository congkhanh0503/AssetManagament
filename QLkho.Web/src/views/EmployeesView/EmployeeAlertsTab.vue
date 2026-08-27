<template>
  <div class="tab-content">
    <div class="glass-card table-container">
      <div class="alert-info-banner">
        ⚠️ <strong>{{ $t('employees.tab_alerts') }}:</strong> {{ $t('dashboard.resigned_employees_sub') }}
      </div>

      <div v-if="loadingAlerts" class="table-loading">
        <span class="loading-spinner"></span>
        <p>{{ $t('common.loading') }}</p>
      </div>

      <div v-else-if="leaveAlerts.length === 0" class="empty-state">
        <p>🎉 {{ $t('employees.empty_resigned') }}</p>
      </div>

      <div v-else class="table-responsive">
        <table class="custom-table">
          <thead>
            <tr>
              <th>{{ $t('employees.table_employee_contact') }}</th>
              <th>{{ $t('employees.table_dept_title') }}</th>
              <th style="text-align: center;">{{ $t('employees.table_leave_date') }}</th>
              <th style="text-align: center;">{{ $t('employees.table_assigned_devices') }}</th>
              <th style="text-align: center;">{{ $t('employees.table_accounts') }}</th>
              <th style="text-align: right;">{{ $t('common.actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="emp in leaveAlerts" :key="emp.employeeID">
              <td>
                <div class="emp-profile-cell">
                  <div class="emp-name-row">
                    <strong 
                      class="emp-name" 
                      :class="{ 'clickable-name': !isHR }"
                      @click="!isHR && $emit('edit', emp)"
                    >
                      {{ emp.fullName }}
                    </strong>
                    <span v-if="emp.englishName" class="emp-en-name" style="color: #ef4444; font-weight: 600; font-size: 0.8rem;">({{ emp.englishName }})</span>
                    <span class="emp-code">{{ emp.employeeCode || $t('employees.no_code') }}</span>
                    <span 
                      v-if="!isHR" 
                      class="edit-hint-icon" 
                      @click="$emit('edit', emp)" 
                    >
                      ✏️
                    </span>
                  </div>
                </div>
              </td>
              <td>
                <div class="dept-cell">
                  <div class="dept-text">{{ emp.departmentName }}</div>
                  <div class="title-text">{{ emp.title || 'Staff' }}</div>
                </div>
              </td>
              <td style="text-align: center;">
                <span class="leave-date-badge danger">{{ formatDate(emp.leaveDate) }}</span>
              </td>
              <td style="text-align: center;">
                <button 
                  v-if="emp.holdingAssetCount > 0"
                  type="button" 
                  class="holding-pill warning"
                  @click="$emit('open-assets', emp)"
                >
                  {{ $t('employees.unreturned_warning', { count: emp.holdingAssetCount }) }}
                </button>
                <span v-else class="returned-tag">
                  {{ $t('employees.all_returned') }}
                </span>
              </td>
              <td style="text-align: center;">
                <button 
                  v-if="hasActiveAccounts(emp)"
                  type="button" 
                  class="account-warning-badge-btn"
                  @click="!isHR && $emit('open-accounts', emp)"
                  :class="{ 'cursor-default': isHR, 'cursor-pointer': !isHR }"
                >
                  {{ $t('employees.active_accounts_count', { count: countActiveAccounts(emp) }) }}
                </button>
                <span v-else class="accounts-disabled-tag">
                  {{ $t('employees.all_disabled') }}
                </span>
              </td>
              <td style="text-align: right;">
                <div class="row-action-btns">
                  <button 
                    v-if="!isHR"
                    type="button" 
                    class="btn btn-sm btn-danger" 
                    @click="$emit('open-assets', emp)"
                  >
                    📥 Thu Hồi Máy
                  </button>
                  <button 
                    v-if="!isHR"
                    type="button" 
                    class="btn btn-sm btn-secondary" 
                    @click="$emit('open-accounts', emp)"
                  >
                    🔒 Khóa TK
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
defineProps({
  leaveAlerts: { type: Array, required: true },
  loadingAlerts: { type: Boolean, default: false },
  isHR: { type: Boolean, default: false }
})

defineEmits([
  'open-assets',
  'open-accounts',
  'open-history',
  'edit'
])

const formatDate = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const isAccountActive = (status) => {
  if (!status) return false
  const s = String(status).trim().toLowerCase()
  return s !== 'disable' && s !== 'deleted' && s !== ''
}

const hasActiveAccounts = (emp) => {
  const qad = emp.qad_Status || emp.qaD_Status
  const oa = emp.oa_Status || emp.oA_Status
  const email = emp.email_Status
  const ad = emp.ad_Status || emp.aD_Status

  return isAccountActive(qad) || isAccountActive(oa) || isAccountActive(email) || isAccountActive(ad)
}

const countActiveAccounts = (emp) => {
  const qad = emp.qad_Status || emp.qaD_Status
  const oa = emp.oa_Status || emp.oA_Status
  const email = emp.email_Status
  const ad = emp.ad_Status || emp.aD_Status

  let count = 0
  if (isAccountActive(qad)) count++
  if (isAccountActive(oa)) count++
  if (isAccountActive(email)) count++
  if (isAccountActive(ad)) count++
  return count
}

</script>

<style scoped>
.emp-name-row {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.clickable-name {
  cursor: pointer;
  color: var(--primary, #4f46e5);
  transition: color 0.15s ease;
}

.clickable-name:hover {
  text-decoration: underline;
  color: var(--primary-dark, #4338ca);
}

.edit-hint-icon {
  font-size: 0.8rem;
  opacity: 0.5;
  cursor: pointer;
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.edit-hint-icon:hover {
  opacity: 1;
  transform: scale(1.2);
}

.row-action-btns {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}
</style>
