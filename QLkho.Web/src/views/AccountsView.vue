<template>
  <div class="accounts-view">
    <div class="view-header">
      <div>
        <h1 class="page-title">Quản Lý Tài Khoản Hệ Thống</h1>
        <p class="page-subtitle">QAD, OA, Email, AD, MFA, EV Mailbox, Migrate của từng nhân sự. Bấm vào trạng thái để bật / khóa nhanh.</p>
      </div>
      <div style="display: flex; gap: 8px;">
        <button type="button" class="btn btn-secondary" :disabled="filtered.length === 0" @click="exportExcel">📥 Export Excel ({{ filtered.length }})</button>
        <button type="button" class="btn btn-secondary" @click="isImportOpen = true">⚡ Import Tài Khoản</button>
      </div>
    </div>

    <div class="glass-card filter-card">
      <div class="filter-grid">
        <div class="form-group">
          <label class="form-label">Tìm nhân sự</label>
          <input v-model="search" type="text" class="form-control" placeholder="Tên, mã NV, email..." />
        </div>
        <div class="form-group">
          <label class="form-label">Phòng ban</label>
          <select v-model="departmentId" class="form-control">
            <option :value="null">Tất cả phòng ban</option>
            <option v-for="d in departments" :key="d.departmentID" :value="d.departmentID">{{ d.departmentName }}</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Nhân sự</label>
          <select v-model="empGroup" class="form-control">
            <option value="all">Tất cả</option>
            <option value="active">Đang làm việc</option>
            <option value="onboarding">Chờ đi làm</option>
            <option value="resigned">Đã nghỉ việc</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Loại tài khoản</label>
          <select v-model="accountType" class="form-control">
            <option value="any">Bất kỳ loại tài khoản</option>
            <option v-for="a in ACCOUNTS" :key="a.key" :value="a.key">{{ a.label }}</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Trạng thái</label>
          <select v-model="accountStatus" class="form-control">
            <option value="all">Tất cả</option>
            <option value="Available">Hoạt động</option>
            <option value="Disable">Đang khóa</option>
            <option value="Deleted">Đã xóa</option>
          </select>
        </div>
      </div>
      <div class="filter-summary">
        Hiển thị <strong>{{ filtered.length }}</strong> / {{ employees.length }} nhân sự
        <button v-if="hasFilter" type="button" class="btn-link" @click="resetFilters">Xóa bộ lọc</button>
      </div>
    </div>

    <div class="glass-card table-container">
      <div v-if="loading" class="table-loading"><span class="loading-spinner"></span></div>
      <div v-else-if="filtered.length === 0" class="empty-state"><p>Không có nhân sự nào khớp bộ lọc.</p></div>
      <div v-else class="table-responsive">
        <table class="custom-table">
          <thead>
            <tr>
              <th style="min-width: 260px;">Nhân sự</th>
              <th style="min-width: 160px;">Phòng ban</th>
              <th v-for="a in ACCOUNTS" :key="a.key" style="text-align: center; width: 100px;">{{ a.label }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="emp in pageItems" :key="emp.employeeID" :class="{ 'row-resigned': groupOf(emp) === 'resigned' }">
              <td>
                <strong>{{ emp.fullName }}</strong>
                <span v-if="emp.englishName" class="sub"> ({{ emp.englishName }})</span>
                <span class="code-pill">{{ emp.employeeCode || 'Chưa có mã' }}</span>
                <span v-if="groupOf(emp) === 'resigned'" class="group-tag resigned">Nghỉ việc</span>
                <span v-else-if="groupOf(emp) === 'onboarding'" class="group-tag onboarding">Chờ đi làm</span>
                <div class="sub">{{ emp.email || '---' }}</div>
              </td>
              <td>{{ emp.departmentName || '---' }}</td>
              <td v-for="a in ACCOUNTS" :key="a.key" style="text-align: center;">
                <button
                  type="button"
                  class="toggle-btn"
                  :disabled="savingId === emp.employeeID"
                  :title="emp[a.key] === 'Available' ? 'Bấm để khóa' : 'Bấm để kích hoạt'"
                  @click="toggleAccount(emp, a)"
                >
                  <StatusBadge :status="emp[a.key] || 'Disable'" type="account" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <Pagination
        :current-page="currentPage"
        :page-size="pageSize"
        :total-items="filtered.length"
        @update:currentPage="currentPage = $event"
        @update:pageSize="pageSize = $event"
      />
    </div>

    <EmployeeAccountImportModal :is-open="isImportOpen" @close="isImportOpen = false" @refresh="onImported" />
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { employeesApi, departmentsApi } from '@/api/client'
import StatusBadge from '@/components/common/StatusBadge.vue'
import Pagination from '@/components/common/Pagination.vue'
import Toast from '@/components/common/Toast.vue'
import { exportEmployeeAccounts } from '@/utils/excelImport'
import EmployeeAccountImportModal from './EmployeesView/EmployeeAccountImportModal.vue'

const ACCOUNTS = [
  { key: 'qaD_Status', label: 'QAD' },
  { key: 'oA_Status', label: 'OA' },
  { key: 'email_Status', label: 'Email' },
  { key: 'aD_Status', label: 'AD' },
  { key: 'mfa_Status', label: 'MFA' },
  { key: 'evMailbox_Status', label: 'EV Mailbox' },
  { key: 'migrate_Status', label: 'Migrate' }
]

const route = useRoute()
const toastRef = ref(null)
const employees = ref([])
const departments = ref([])
const loading = ref(false)
const savingId = ref(null)
const isImportOpen = ref(false)

const search = ref(String(route.query.search || ''))
const departmentId = ref(null)
const empGroup = ref('all')
const accountType = ref('any')
const accountStatus = ref('all')
const currentPage = ref(1)
const pageSize = ref(15)

const groupOf = (e) => {
  if (e.status === 'Resigned' || (e.leaveDate && new Date(e.leaveDate) <= new Date())) return 'resigned'
  if (e.joinDate && new Date(e.joinDate) > new Date()) return 'onboarding'
  return 'active'
}

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  const keys = accountType.value === 'any' ? ACCOUNTS.map(a => a.key) : [accountType.value]
  return employees.value.filter(e => {
    if (departmentId.value && e.departmentID !== departmentId.value) return false
    if (empGroup.value !== 'all' && groupOf(e) !== empGroup.value) return false
    if (accountStatus.value !== 'all' && !keys.some(k => (e[k] || 'Disable') === accountStatus.value)) return false
    if (q && ![e.fullName, e.englishName, e.employeeCode, e.email].some(v => v && String(v).toLowerCase().includes(q))) return false
    return true
  })
})

const pageItems = computed(() => filtered.value.slice((currentPage.value - 1) * pageSize.value, currentPage.value * pageSize.value))

const hasFilter = computed(() =>
  search.value || departmentId.value || empGroup.value !== 'all' || accountType.value !== 'any' || accountStatus.value !== 'all'
)
const resetFilters = () => {
  search.value = ''
  departmentId.value = null
  empGroup.value = 'all'
  accountType.value = 'any'
  accountStatus.value = 'all'
}

watch([search, departmentId, empGroup, accountType, accountStatus, pageSize], () => { currentPage.value = 1 })

const fetchData = async () => {
  loading.value = true
  try {
    const [emps, depts] = await Promise.all([employeesApi.getAll(), departmentsApi.getAll()])
    employees.value = emps || []
    departments.value = depts || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải dữ liệu', err.message, 'error')
  } finally {
    loading.value = false
  }
}

// Bấm: đang hoạt động -> khóa, còn lại (khóa / đã xóa) -> hoạt động
const toggleAccount = async (emp, account) => {
  if (savingId.value) return
  const next = emp[account.key] === 'Available' ? 'Disable' : 'Available'
  savingId.value = emp.employeeID
  try {
    await employeesApi.updateAccounts(emp.employeeID, {
      qaD_Status: emp.qaD_Status || 'Disable',
      oA_Status: emp.oA_Status || 'Disable',
      email_Status: emp.email_Status || 'Disable',
      aD_Status: emp.aD_Status || 'Disable',
      mfa_Status: emp.mfa_Status || 'Disable',
      evMailbox_Status: emp.evMailbox_Status || 'Disable',
      migrate_Status: emp.migrate_Status || 'Disable',
      [account.key]: next
    })
    emp[account.key] = next
    toastRef.value?.addToast(
      next === 'Available' ? 'Đã kích hoạt' : 'Đã khóa',
      `${account.label} của ${emp.fullName}`,
      'success'
    )
  } catch (err) {
    toastRef.value?.addToast('Lỗi cập nhật tài khoản', err.message, 'error')
  } finally {
    savingId.value = null
  }
}

// Chỉ xuất các nhân sự đang khớp bộ lọc (toàn bộ, không chỉ trang hiện tại)
const exportExcel = async () => {
  try {
    const today = new Date().toISOString().slice(0, 10)
    await exportEmployeeAccounts(filtered.value, ACCOUNTS, `Tai_Khoan_Nhan_Su_${today}.xlsx`)
  } catch (err) {
    toastRef.value?.addToast('Lỗi xuất Excel', err.message, 'error')
  }
}

const onImported = () => {
  fetchData()
  toastRef.value?.addToast('Thành công', 'Đã cập nhật trạng thái tài khoản từ file Excel!', 'success')
}

onMounted(fetchData)
</script>

<style scoped>
.view-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; margin-bottom: 20px; }
.filter-card { padding: 16px; margin-bottom: 16px; }
.filter-grid { display: grid; grid-template-columns: 1.4fr repeat(4, 1fr); gap: 12px; }
.filter-grid .form-group { margin-bottom: 0; }
.filter-summary { margin-top: 12px; font-size: 0.85rem; color: var(--text-dim); display: flex; gap: 12px; align-items: center; }
.btn-link { background: none; border: none; color: #4f46e5; cursor: pointer; font-weight: 600; }
.sub { font-size: 0.78rem; color: var(--text-dim); }
.code-pill { margin-left: 8px; font-family: monospace; font-size: 0.75rem; background: #eef2ff; color: #4f46e5; padding: 1px 6px; border-radius: 4px; }
.group-tag { margin-left: 6px; font-size: 0.7rem; padding: 1px 6px; border-radius: 9999px; font-weight: 600; }
.group-tag.resigned { background: #fee2e2; color: #b91c1c; }
.group-tag.onboarding { background: #e0f2fe; color: #0369a1; }
.row-resigned td { opacity: 0.8; }
.toggle-btn { background: none; border: none; cursor: pointer; padding: 4px; border-radius: 9999px; }
.toggle-btn:hover:not(:disabled) { filter: brightness(0.95); transform: scale(1.04); }
.toggle-btn:disabled { cursor: wait; opacity: 0.6; }
@media (max-width: 1100px) { .filter-grid { grid-template-columns: 1fr 1fr; } }
</style>
