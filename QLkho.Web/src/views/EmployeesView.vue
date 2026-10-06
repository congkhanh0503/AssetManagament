<template>
  <div class="employees-view">
    <!-- TOP HEADER BAR -->
    <div class="view-header">
      <div>
        <h1 class="page-title">{{ $t('employees.title') }}</h1>
        <p class="page-subtitle">{{ $t('employees.subtitle') }}</p>
      </div>
      <div class="header-actions">
        <!-- Nút Import Excel Nhân Sự -->
        <button type="button" class="btn btn-secondary" @click="isImportOpen = true">
          📥 {{ $t('employees.btn_import') }}
        </button>

        <!-- Nút Import Tài Khoản AD/QAD -->
        <button type="button" class="btn btn-secondary btn-account-import" @click="isAccountImportOpen = true">
          ⚡ Import Tài Khoản
        </button>

        <!-- Nút Xuất CSV Dropdown -->
        <div class="export-dropdown-wrapper" ref="exportDropdownRef">
          <button type="button" class="btn btn-secondary export-main-btn" @click="isExportMenuOpen = !isExportMenuOpen">
            📊 {{ exportButtonLabel }} <span class="caret-icon">▾</span>
          </button>
          <div v-if="isExportMenuOpen" class="export-dropdown-menu">
            <button type="button" class="export-menu-item active-type" @click="exportByType('active')">
              💼 <span>{{ $t('employees.btn_export_active') }}</span> <strong class="badge-count-pill">{{ activeEmployees.length }}</strong>
            </button>
            <button type="button" class="export-menu-item resigned-type" @click="exportByType('resigned')">
              📁 <span>{{ $t('employees.btn_export_resigned') }}</span> <strong class="badge-count-pill">{{ resignedEmployees.length }}</strong>
            </button>
            <button type="button" class="export-menu-item onboarding-type" @click="exportByType('onboarding')">
              🚀 <span>{{ $t('employees.btn_export_onboarding') }}</span> <strong class="badge-count-pill">{{ onboardingEmployees.length }}</strong>
            </button>
            <div class="export-menu-divider"></div>
            <button type="button" class="export-menu-item all-type" @click="exportByType('all')">
              🌐 <span>{{ $t('employees.btn_export_all') }}</span> <strong class="badge-count-pill">{{ employees.length }}</strong>
            </button>
          </div>
        </div>

        <!-- Nút Thêm Nhân Viên Mới -->
        <button type="button" class="btn btn-primary" @click="openCreateEmpModal">
          ➕ {{ $t('employees.btn_add') }}
        </button>
      </div>
    </div>

    <!-- NAVIGATION TABS -->
    <div class="tab-nav-bar">
      <!-- 1. Tab Đang Làm Việc -->
      <button 
        type="button" 
        class="tab-btn" 
        :class="{ active: currentTab === 'active' }"
        @click="currentTab = 'active'"
      >
        💼 {{ $t('employees.tab_active') }} ({{ activeEmployees.length }})
      </button>

      <!-- 2. Tab Chờ Đi Làm (Onboarding) -->
      <button 
        type="button" 
        class="tab-btn onboarding-tab" 
        :class="{ active: currentTab === 'onboarding' }"
        @click="currentTab = 'onboarding'"
      >
        🚀 {{ $t('employees.tab_onboarding') }} ({{ onboardingEmployees.length }})
      </button>

      <!-- 3. Tab Đã Nghỉ Việc -->
      <button 
        type="button" 
        class="tab-btn resigned-tab" 
        :class="{ active: currentTab === 'resigned' }"
        @click="currentTab = 'resigned'"
      >
        📁 {{ $t('employees.tab_resigned') }} ({{ resignedEmployees.length }})
      </button>

      <!-- 4. Tab Cảnh Báo Nghỉ Việc -->
      <button 
        type="button" 
        class="tab-btn alert-tab" 
        :class="{ active: currentTab === 'alerts' }"
        @click="currentTab = 'alerts'"
      >
        ⚠️ {{ $t('employees.tab_alerts') }} ({{ leaveAlerts.length }})
      </button>

      <!-- 5. Tab Cảnh Báo Chưa Có Biên Bản Bàn Giao -->
      <button 
        type="button" 
        class="tab-btn missing-doc-tab" 
        :class="{ active: currentTab === 'missing-handover' }"
        @click="currentTab = 'missing-handover'"
      >
        📑 {{ $t('employees.tab_missing_handover') }}
        <span v-if="totalMissingHandoverAssets > 0" class="tab-badge-count danger">
          {{ totalMissingHandoverAssets }}
        </span>
      </button>

      <!-- 6. Tab Lịch Sử Biến Động Chung -->
      <button 
        type="button" 
        class="tab-btn history-tab" 
        :class="{ active: currentTab === 'history' }"
        @click="openHistoryTab"
      >
        🕒 {{ $t('employees.tab_history') }}
      </button>
    </div>

    <!-- TAB 1 & 3: ĐANG LÀM VIỆC / ĐÃ NGHỈ VIỆC -->
    <EmployeeActiveTab 
      v-if="currentTab === 'active' || currentTab === 'resigned'"
      :current-tab="currentTab"
      :employees="currentDisplayedEmployees"
      :departments="departments"
      :loading="loading"
      :is-h-r="isHR"
      v-model:search="search"
      v-model:department-id="selectedDepartmentId"
      v-model:current-page="currentPage"
      v-model:page-size="pageSize"
      @edit="openEditEmpModal"
      @open-accounts="openAccountModal"
      @open-assets="openEmployeeAssetsModal"
      @open-history="openSingleEmpHistoryModal"
    />

    <!-- TAB 2: CHỜ ĐI LÀM (ONBOARDING) -->
    <EmployeeOnboardingTab 
      v-else-if="currentTab === 'onboarding'"
      :employees="filteredOnboardingEmployees"
      :departments="departments"
      :loading="loading"
      :is-h-r="isHR"
      v-model:search="onboardingSearch"
      v-model:department-id="selectedOnboardingDeptId"
      v-model:date-filter-preset="onboardingDatePreset"
      v-model:from-date="onboardingFromDate"
      v-model:to-date="onboardingToDate"
      @clear-custom-date="clearOnboardingCustomDate"
      v-model:current-page="onboardingCurrentPage"
      v-model:page-size="onboardingPageSize"
      @export-email-template="handleExportOnboardingEmailTemplate"
      @import="isImportOpen = true"
      @export="handleExportOnboardingCsv"
      @create="openCreateEmpModal"
      @edit="openEditEmpModal"
      @delete="handleDeleteEmployee"
      @open-accounts="openAccountModal"
      @open-assets="openEmployeeAssetsModal"
      @open-history="openSingleEmpHistoryModal"
    />

    <!-- TAB 4: CẢNH BÁO NGHỈ VIỆC -->
    <EmployeeAlertsTab 
      v-else-if="currentTab === 'alerts'"
      :leave-alerts="leaveAlerts"
      :loading-alerts="loadingAlerts"
      :is-h-r="isHR"
      @edit="openEditEmpModal"
      @open-assets="openEmployeeAssetsModal"
      @open-accounts="openAccountModal"
      @open-history="openSingleEmpHistoryModal"
    />

    <!-- TAB 5: CẢNH BÁO CHƯA CÓ BIÊN BẢN BÀN GIAO -->
    <EmployeeMissingHandoverTab 
      v-else-if="currentTab === 'missing-handover'"
      :alerts="missingHandoverAlerts"
      :departments="departments"
      :loading="loadingMissingHandover"
      @open-assets="openEmployeeAssetsModal"
      @refresh="fetchMissingHandoverAlerts"
    />

    <!-- TAB 6: LỊCH SỬ BIẾN ĐỘNG -->
    <EmployeeHistoryTab 
      v-else-if="currentTab === 'history'"
      :histories="historiesList"
      :employees="employees"
      :loading-history="loadingHistory"
      :filters="historyFilters"
      @update:search="onHistorySearchChange"
      @update:action-type="onHistoryActionTypeChange"
      @update:employee-id="onHistoryEmployeeIdChange"
    />

    <!-- MODAL 1: CẬP NHẬT 4 TÀI KHOẢN HỆ THỐNG -->
    <EmployeeAccountsModal 
      :is-open="isAccountModalOpen"
      :employee="selectedEmp"
      :submitting="submitting"
      @close="isAccountModalOpen = false"
      @submit="submitUpdateAccounts"
    />

    <!-- MODAL 2: THÊM MỚI / CHỈNH SỬA NHÂN VIÊN -->
    <EmployeeFormModal 
      :is-open="isEmpFormModalOpen"
      :is-edit="isEditEmp"
      :employee="selectedEmp"
      :departments="departments"
      :is-h-r="isHR"
      :submitting="submitting"
      @close="isEmpFormModalOpen = false"
      @submit="handleSaveEmployee"
      @delete="handleDeleteEmployee"
    />

    <!-- MODAL 3: XEM VÀ QUẢN LÝ THIẾT BỊ ĐƯỢC CẤP PHÁT -->
    <EmployeeAssetsModal 
      :is-open="isEmpAssetsOpen"
      :employee="selectedEmp"
      :emp-assets-data="empAssetsData"
      :loading="loadingEmpAssets"
      :is-h-r="isHR"
      :categories="categories"
      :employees="employees"
      :departments="departments"
      :warehouse-assets="warehouseAssets"
      :uploading-asset-id="uploadingAssetId"
      :submitting="submitting"
      @close="isEmpAssetsOpen = false"
      @assign="handleAssignAsset"
      @transfer="handleTransferAsset"
      @return="handleReturnAsset"
      @report-issue="handleReportIssueAsset"
      @print-single="openPrintSingleModal"
      @print-bulk-non-laptop="openPrintBulkNonLaptopModal"
      @view-pdf="openPdfViewer"
      @upload-pdf="handleUploadPdf"
      @delete-pdf="handleDeletePdf"
    />

    <!-- MODAL 4: DÒNG THỜI GIAN LỊCH SỬ CỦA 1 NHÂN SỰ -->
    <EmployeeHistoryModal 
      :is-open="isSingleEmpHistoryOpen"
      :employee="selectedHistoryEmp"
      :histories="singleEmpHistories"
      :loading="loadingSingleHistory"
      @close="isSingleEmpHistoryOpen = false"
    />

    <!-- MODAL 5: XEM TRƯỚC VÀ IN BIÊN BẢN BÀN GIAO THIẾT BỊ A4 -->
    <EmployeePrintHandoverModal 
      :is-open="isPrintHandoverOpen"
      :asset="selectedAssetForPrint"
      :employee="selectedEmp"
      :printing-equipment-list="printingEquipmentList"
      :is-bulk-equipment-print="isBulkEquipmentPrint"
      :categories="categories"
      :departments="departments"
      @close="isPrintHandoverOpen = false"
    />

    <!-- MODAL 6: IMPORT EXCEL / CSV NHÂN SỰ -->
    <EmployeeImportModal 
      :is-open="isImportOpen"
      :submitting="submitting"
      :employees="employees"
      :departments="departments"
      @close="isImportOpen = false"
      @submit="submitImportEmployees"
      @download-template="downloadTemplate"
    />

    <!-- MODAL 7: XEM TRỰC TIẾP FILE SCAN BIÊN BẢN PDF -->
    <EmployeePdfViewerModal 
      :is-open="isPdfViewerOpen"
      :document="viewingPdfDoc"
      @close="isPdfViewerOpen = false"
    />

    <!-- MODAL 8: IMPORT TÀI KHOẢN (AD, QAD, TRẠNG THÁI) -->
    <EmployeeAccountImportModal 
      :is-open="isAccountImportOpen"
      @close="isAccountImportOpen = false"
      @refresh="onAccountImportRefreshed"
    />

    <!-- TOAST NOTIFICATION COMPONENT -->
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
import { employeesApi, departmentsApi, assetsApi, categoriesApi, documentsApi } from '@/api/client'
import { getCurrentUser } from '@/api/auth'
import { exportToCsv } from '@/utils/exportCsv'
import { downloadEmployeeTemplate, exportOnboardingEmailTemplate, getRowValue, normalizeDateValue } from '@/utils/excelImport'

import Toast from '@/components/common/Toast.vue'
import EmployeeActiveTab from './EmployeesView/EmployeeActiveTab.vue'
import EmployeeOnboardingTab from './EmployeesView/EmployeeOnboardingTab.vue'
import EmployeeAlertsTab from './EmployeesView/EmployeeAlertsTab.vue'
import EmployeeMissingHandoverTab from './EmployeesView/EmployeeMissingHandoverTab.vue'
import EmployeeHistoryTab from './EmployeesView/EmployeeHistoryTab.vue'
import EmployeeAccountsModal from './EmployeesView/EmployeeAccountsModal.vue'
import EmployeeFormModal from './EmployeesView/EmployeeFormModal.vue'
import EmployeeAssetsModal from './EmployeesView/EmployeeAssetsModal.vue'
import EmployeeHistoryModal from './EmployeesView/EmployeeHistoryModal.vue'
import EmployeePrintHandoverModal from './EmployeesView/EmployeePrintHandoverModal.vue'
import EmployeeImportModal from './EmployeesView/EmployeeImportModal.vue'
import EmployeePdfViewerModal from './EmployeesView/EmployeePdfViewerModal.vue'
import EmployeeAccountImportModal from './EmployeesView/EmployeeAccountImportModal.vue'

// Quản lý người dùng & vai trò
const currentUser = computed(() => getCurrentUser())
const isHR = computed(() => currentUser.value?.role === 'HR')

// Navigation state
const currentTab = ref(isHR.value ? 'onboarding' : 'active')
const toastRef = ref(null)

// Data state
const employees = ref([])
const leaveAlerts = ref([])
const missingHandoverAlerts = ref([])
const loadingMissingHandover = ref(false)
const departments = ref([])
const categories = ref([])
const warehouseAssets = ref([])
const loading = ref(false)
const loadingAlerts = ref(false)
const submitting = ref(false)

// Filters cho Tab Đang Làm Việc / Đã Nghỉ Việc
const search = ref('')
const selectedDepartmentId = ref(null)
const currentPage = ref(1)
const pageSize = ref(15)

// Filters cho Tab Chờ Đi Làm (Onboarding)
const onboardingSearch = ref('')
const selectedOnboardingDeptId = ref(null)
const onboardingDatePreset = ref('all')
const onboardingFromDate = ref('')
const onboardingToDate = ref('')
const onboardingCurrentPage = ref(1)
const onboardingPageSize = ref(15)

const clearOnboardingCustomDate = () => {
  onboardingFromDate.value = ''
  onboardingToDate.value = ''
}

// State Quản lý Lịch Sử Biến Động
const historiesList = ref([])
const loadingHistory = ref(false)
const historyFilters = reactive({
  search: '',
  actionType: '',
  employeeId: null
})

const isUpcomingJoin = (joinDateStr) => {
  if (!joinDateStr) return false
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const joinDate = new Date(joinDateStr)
  joinDate.setHours(0, 0, 0, 0)
  const diff = Math.ceil((joinDate - today) / (1000 * 60 * 60 * 24))
  return diff > 0
}

// 1. Phân loại danh sách nhân viên
const activeEmployees = computed(() => {
  return employees.value.filter(e => e.status !== 'Resigned' && !e.leaveDate && !isUpcomingJoin(e.joinDate))
})

const resignedEmployees = computed(() => {
  return employees.value.filter(e => e.status === 'Resigned' || (e.leaveDate && new Date(e.leaveDate) <= new Date()))
})

const onboardingEmployees = computed(() => {
  return employees.value
    .filter(e => e.status !== 'Resigned' && !e.leaveDate && isUpcomingJoin(e.joinDate))
    .sort((a, b) => new Date(a.joinDate) - new Date(b.joinDate))
})

const currentDisplayedEmployees = computed(() => {
  let list = currentTab.value === 'resigned' ? resignedEmployees.value : activeEmployees.value
  if (selectedDepartmentId.value) {
    list = list.filter(e => e.departmentID === selectedDepartmentId.value)
  }
  if (search.value.trim()) {
    const q = search.value.trim().toLowerCase()
    list = list.filter(e => 
      (e.fullName && e.fullName.toLowerCase().includes(q)) ||
      (e.englishName && e.englishName.toLowerCase().includes(q)) ||
      (e.employeeCode && e.employeeCode.toLowerCase().includes(q)) ||
      (e.email && e.email.toLowerCase().includes(q)) ||
      (e.phone && e.phone.toLowerCase().includes(q))
    )
  }
  return list
})

const filteredOnboardingEmployees = computed(() => {
  let list = onboardingEmployees.value
  if (selectedOnboardingDeptId.value) {
    list = list.filter(e => e.departmentID === selectedOnboardingDeptId.value)
  }
  if (onboardingSearch.value.trim()) {
    const q = onboardingSearch.value.trim().toLowerCase()
    list = list.filter(e => 
      (e.fullName && e.fullName.toLowerCase().includes(q)) ||
      (e.englishName && e.englishName.toLowerCase().includes(q)) ||
      (e.employeeCode && e.employeeCode.toLowerCase().includes(q)) ||
      (e.email && e.email.toLowerCase().includes(q)) ||
      (e.phone && e.phone.toLowerCase().includes(q))
    )
  }

  // Lọc theo ngày dự kiến vào làm
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  if (onboardingDatePreset.value === '7days') {
    const next7 = new Date(today)
    next7.setDate(next7.getDate() + 7)
    next7.setHours(23, 59, 59, 999)
    list = list.filter(e => {
      if (!e.joinDate) return false
      const d = new Date(e.joinDate)
      return d >= today && d <= next7
    })
  } else if (onboardingDatePreset.value === '14days') {
    const next14 = new Date(today)
    next14.setDate(next14.getDate() + 14)
    next14.setHours(23, 59, 59, 999)
    list = list.filter(e => {
      if (!e.joinDate) return false
      const d = new Date(e.joinDate)
      return d >= today && d <= next14
    })
  } else if (onboardingDatePreset.value === '30days') {
    const next30 = new Date(today)
    next30.setDate(next30.getDate() + 30)
    next30.setHours(23, 59, 59, 999)
    list = list.filter(e => {
      if (!e.joinDate) return false
      const d = new Date(e.joinDate)
      return d >= today && d <= next30
    })
  } else if (onboardingDatePreset.value === 'this_month') {
    const year = today.getFullYear()
    const month = today.getMonth()
    list = list.filter(e => {
      if (!e.joinDate) return false
      const d = new Date(e.joinDate)
      return d.getFullYear() === year && d.getMonth() === month
    })
  } else if (onboardingDatePreset.value === 'next_month') {
    const nextMonthDate = new Date(today.getFullYear(), today.getMonth() + 1, 1)
    const nextMonthYear = nextMonthDate.getFullYear()
    const nextMonth = nextMonthDate.getMonth()
    list = list.filter(e => {
      if (!e.joinDate) return false
      const d = new Date(e.joinDate)
      return d.getFullYear() === nextMonthYear && d.getMonth() === nextMonth
    })
  } else if (onboardingDatePreset.value === 'custom') {
    if (onboardingFromDate.value) {
      const from = new Date(onboardingFromDate.value)
      from.setHours(0, 0, 0, 0)
      list = list.filter(e => e.joinDate && new Date(e.joinDate) >= from)
    }
    if (onboardingToDate.value) {
      const to = new Date(onboardingToDate.value)
      to.setHours(23, 59, 59, 999)
      list = list.filter(e => e.joinDate && new Date(e.joinDate) <= to)
    }
  }

  return list
})

// Modal States
const selectedEmp = ref(null)
const isAccountModalOpen = ref(false)
const isEmpFormModalOpen = ref(false)
const isEditEmp = ref(false)

const isEmpAssetsOpen = ref(false)
const empAssetsData = ref(null)
const loadingEmpAssets = ref(false)
const uploadingAssetId = ref(null)

const isSingleEmpHistoryOpen = ref(false)
const selectedHistoryEmp = ref(null)
const singleEmpHistories = ref([])
const loadingSingleHistory = ref(false)

const isPrintHandoverOpen = ref(false)
const selectedAssetForPrint = ref(null)
const printingEquipmentList = ref([])
const isBulkEquipmentPrint = ref(false)

const isImportOpen = ref(false)
const isAccountImportOpen = ref(false)
const isPdfViewerOpen = ref(false)
const viewingPdfDoc = ref(null)

// API Fetching
const fetchEmployees = async () => {
  loading.value = true
  try {
    const data = await employeesApi.getAll()
    employees.value = data || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải danh sách nhân viên', err.message, 'error')
  } finally {
    loading.value = false
  }
}

const fetchLeaveAlerts = async () => {
  loadingAlerts.value = true
  try {
    const data = await employeesApi.getLeaveAlerts()
    leaveAlerts.value = data || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải cảnh báo nghỉ việc', err.message, 'error')
  } finally {
    loadingAlerts.value = false
  }
}

const totalMissingHandoverAssets = computed(() => {
  return missingHandoverAlerts.value.reduce((sum, item) => sum + (item.missingCount || 0), 0)
})

const fetchMissingHandoverAlerts = async () => {
  loadingMissingHandover.value = true
  try {
    const data = await employeesApi.getMissingHandoverAlerts()
    missingHandoverAlerts.value = data || []
  } catch (err) {
    console.error('Lỗi tải cảnh báo thiếu biên bản:', err)
  } finally {
    loadingMissingHandover.value = false
  }
}

const fetchDepartments = async () => {
  try {
    const data = await departmentsApi.getAll()
    departments.value = data || []
  } catch {}
}

const fetchCategories = async () => {
  try {
    const data = await categoriesApi.getAll()
    categories.value = data || []
  } catch {}
}

const fetchWarehouseAssets = async () => {
  try {
    const data = await assetsApi.getAll()
    warehouseAssets.value = data || []
  } catch {}
}

const fetchHistories = async () => {
  loadingHistory.value = true
  try {
    const params = {}
    if (historyFilters.search) params.search = historyFilters.search
    if (historyFilters.actionType) params.actionType = historyFilters.actionType
    if (historyFilters.employeeId) params.employeeId = historyFilters.employeeId
    const res = await employeesApi.getHistories(params)
    historiesList.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải lịch sử', err.message, 'error')
  } finally {
    loadingHistory.value = false
  }
}

const openHistoryTab = () => {
  currentTab.value = 'history'
  fetchHistories()
}

let historyTimer = null
const onHistorySearchChange = (val) => {
  historyFilters.search = val
  if (historyTimer) clearTimeout(historyTimer)
  historyTimer = setTimeout(fetchHistories, 300)
}
const onHistoryActionTypeChange = (val) => {
  historyFilters.actionType = val
  fetchHistories()
}
const onHistoryEmployeeIdChange = (val) => {
  historyFilters.employeeId = val
  fetchHistories()
}

// Handlers for Modals
const openAccountModal = (emp) => {
  if (isHR.value) {
    toastRef.value?.addToast('Giới Hạn Quyền Hạn (HR)', 'Nhân sự HR không có quyền thay đổi trạng thái tài khoản hệ thống (QAD, OA, Email, AD). Nghiệp vụ này do Quản trị viên (Admin) và bộ phận IT phụ trách.', 'warning')
    return
  }
  selectedEmp.value = emp
  isAccountModalOpen.value = true
}

const submitUpdateAccounts = async (formData) => {
  if (isHR.value) return
  submitting.value = true
  try {
    await employeesApi.updateAccounts(selectedEmp.value.employeeID, formData)
    toastRef.value?.addToast('Thành công', `Đã cập nhật trạng thái tài khoản cho ${selectedEmp.value.fullName}!`, 'success')
    isAccountModalOpen.value = false
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi cập nhật tài khoản', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const onAccountImportRefreshed = () => {
  fetchEmployees()
  fetchLeaveAlerts()
  toastRef.value?.addToast('Thành công', 'Đã cập nhật trạng thái tài khoản nhân sự từ file Excel!', 'success')
}

const openCreateEmpModal = () => {
  selectedEmp.value = null
  isEditEmp.value = false
  isEmpFormModalOpen.value = true
}

const openEditEmpModal = (emp) => {
  const fullEmp = employees.value.find(e => e.employeeID === emp.employeeID)
  selectedEmp.value = fullEmp ? { ...fullEmp, ...emp } : { ...emp }
  isEditEmp.value = true
  isEmpFormModalOpen.value = true
}

const handleSaveEmployee = async (formData) => {
  submitting.value = true
  try {
    const payload = {
      employeeID: formData.employeeID,
      employeeCode: (formData.employeeCode || '').trim(),
      fullName: (formData.fullName || '').trim(),
      englishName: formData.englishName ? formData.englishName.trim() : null,
      departmentID: formData.departmentID ? parseInt(formData.departmentID) : null,
      title: formData.title ? formData.title.trim() : null,
      email: formData.email ? formData.email.trim() : null,
      phone: formData.phone ? formData.phone.trim() : null,
      joinDate: formData.joinDate ? formData.joinDate : new Date().toISOString().split('T')[0],
      leaveDate: formData.leaveDate && formData.leaveDate.trim() ? formData.leaveDate : null,
      qad_Status: formData.qaD_Status || formData.qad_Status || formData.QAD_Status || 'Disable',
      oa_Status: formData.oA_Status || formData.oa_Status || formData.OA_Status || 'Disable',
      email_Status: formData.email_Status || formData.Email_Status || 'Disable',
      ad_Status: formData.aD_Status || formData.ad_Status || formData.AD_Status || 'Disable',
      status: formData.status || 'Active'
    }

    if (isEditEmp.value) {
      await employeesApi.update(formData.employeeID, payload)
      toastRef.value?.addToast('Thành công', `Đã cập nhật hồ sơ ${formData.fullName}!`, 'success')
    } else {
      await employeesApi.create(payload)
      toastRef.value?.addToast('Thành công', `Đã thêm nhân viên mới ${formData.fullName}!`, 'success')
    }
    isEmpFormModalOpen.value = false
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi lưu nhân viên', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const handleDeleteEmployee = async (emp) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa nhân viên "${emp.fullName}" (${emp.employeeCode})?`)) return
  submitting.value = true
  try {
    await employeesApi.delete(emp.employeeID)
    toastRef.value?.addToast('Thành công', `Đã xóa nhân viên ${emp.fullName}!`, 'success')
    isEmpFormModalOpen.value = false
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa nhân viên', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const openEmployeeAssetsModal = async (emp) => {
  selectedEmp.value = emp
  isEmpAssetsOpen.value = true
  loadingEmpAssets.value = true
  try {
    const data = await employeesApi.getAssets(emp.employeeID)
    empAssetsData.value = data
    fetchWarehouseAssets()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải thiết bị nhân viên', err.message, 'error')
  } finally {
    loadingEmpAssets.value = false
  }
}

const handleAssignAsset = async (assignData) => {
  submitting.value = true
  try {
    const toEmpId = selectedEmp.value.employeeID
    await assetsApi.bundleAssign({
      mainAssetID: assignData.assetID,
      toEmployeeID: toEmpId,
      mouseAssetID: assignData.mouseAssetID || null,
      monitorAssetID: assignData.monitorAssetID || null,
      keyboardAssetID: assignData.keyboardAssetID || null,
      conditionStatus: assignData.conditionStatus || 'Hoạt động tốt',
      note: assignData.note || `Cấp phát thiết bị cho ${selectedEmp.value.fullName}`
    })

    toastRef.value?.addToast('Thành công', `Đã cấp phát thiết bị cho ${selectedEmp.value.fullName}!`, 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi cấp phát thiết bị', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const handleTransferAsset = async ({ asset, toEmployeeID, conditionStatus, note }) => {
  submitting.value = true
  try {
    await assetsApi.transfer({
      assetID: asset.assetID,
      toEmployeeID: toEmployeeID,
      conditionStatus: conditionStatus || 'Hoạt động bình thường',
      note: note || `Điều chuyển thiết bị ${asset.assetName}`
    })
    toastRef.value?.addToast('Thành công', `Đã điều chuyển thiết bị ${asset.assetName}!`, 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi điều chuyển thiết bị', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const handleReturnAsset = async ({ asset, warehouseLocation, isBroken, conditionStatus, note }) => {
  submitting.value = true
  try {
    await assetsApi.return(asset.assetID, {
      warehouseLocation,
      isBroken,
      conditionStatus,
      note
    })
    toastRef.value?.addToast('Thành công', `Đã thu hồi thiết bị ${asset.assetName} về kho!`, 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi thu hồi thiết bị', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const handleReportIssueAsset = async ({ asset, issueDescription, status, vendorName, estimatedCost, expectedReturnDate }) => {
  submitting.value = true
  try {
    await assetsApi.reportIssue(asset.assetID, {
      issueDescription,
      status,
      vendorName,
      estimatedCost,
      expectedReturnDate
    })
    toastRef.value?.addToast('Thành công', `Đã cập nhật báo hỏng cho ${asset.assetName}!`, 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchLeaveAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi báo hỏng thiết bị', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const openSingleEmpHistoryModal = async (emp) => {
  selectedHistoryEmp.value = emp
  isSingleEmpHistoryOpen.value = true
  loadingSingleHistory.value = true
  singleEmpHistories.value = []
  try {
    const res = await employeesApi.getEmployeeHistories(emp.employeeID)
    singleEmpHistories.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải lịch sử', err.message, 'error')
  } finally {
    loadingSingleHistory.value = false
  }
}

const openPrintSingleModal = (asset) => {
  selectedAssetForPrint.value = asset
  isBulkEquipmentPrint.value = false
  isPrintHandoverOpen.value = true
}

const openPrintBulkNonLaptopModal = (assetIds) => {
  if (!empAssetsData.value?.assets) return
  printingEquipmentList.value = empAssetsData.value.assets.filter(a => assetIds.includes(a.assetID))
  isBulkEquipmentPrint.value = true
  selectedAssetForPrint.value = null
  isPrintHandoverOpen.value = true
}

const openPdfViewer = (doc) => {
  viewingPdfDoc.value = doc
  isPdfViewerOpen.value = true
}

const handleUploadPdf = async ({ assetId, file }) => {
  uploadingAssetId.value = assetId
  try {
    const formData = new FormData()
    formData.append('File', file)
    await employeesApi.uploadHandoverPdf(selectedEmp.value.employeeID, assetId, formData)
    toastRef.value?.addToast('Thành công', 'Đã tải lên biên bản bàn giao PDF!', 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchMissingHandoverAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải lên PDF', err.message, 'error')
  } finally {
    uploadingAssetId.value = null
  }
}

const handleDeletePdf = async (asset) => {
  if (!confirm(`Bạn có chắc muốn xóa file PDF biên bản "${asset.handoverDocument?.fileName}"?`)) return
  try {
    await employeesApi.deleteHandoverPdf(selectedEmp.value.employeeID, asset.assetID)
    toastRef.value?.addToast('Thành công', 'Đã xóa file PDF biên bản!', 'success')
    openEmployeeAssetsModal(selectedEmp.value)
    fetchEmployees()
    fetchMissingHandoverAlerts()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa PDF', err.message, 'error')
  }
}

const submitImportEmployees = async (rows) => {
  submitting.value = true
  let createdCount = 0
  let updatedCount = 0
  let errorCount = 0

  for (const row of rows) {
    const code = getRowValue(row, ['mã nhân viên', 'ma nhan vien', 'manhanvien', 'mã nv', 'ma nv', 'code', 'employeecode']).trim()
    let name = getRowValue(row, ['họ và tên', 'ho va ten', 'họ tên', 'ho ten', 'hoten', 'tên', 'ten', 'fullname', 'name']).trim()
    
    const englishName = (row.englishName !== undefined && row.englishName !== null) 
      ? String(row.englishName).trim() 
      : getRowValue(row, ['tên tiếng anh (english name)', 'tên tiếng anh', 'ten tieng anh', 'englishname', 'english name', 'tentienganh', 'en name', 'enname', 'họ và tên_2']).trim()
    
    if (!name && englishName) {
      name = englishName
    }
    if (!name) continue
    
    const deptName = getRowValue(row, ['bộ phận', 'bo phan', 'phòng ban', 'phong ban', 'phongban', 'department', 'dept']).replace(/\s+/g, ' ').trim()
    let deptId = null
    if (deptName) {
      let matchDept = departments.value.find(d => 
        d.departmentName.toLowerCase().trim() === deptName.toLowerCase() ||
        (d.departmentCode && d.departmentCode.toLowerCase().trim() === deptName.toLowerCase())
      )
      if (!matchDept) {
        matchDept = departments.value.find(d => 
          d.departmentName.toLowerCase().includes(deptName.toLowerCase()) ||
          deptName.toLowerCase().includes(d.departmentName.toLowerCase())
        )
      }
      if (matchDept) {
        deptId = matchDept.departmentID
      } else {
        // Tự động tạo Phòng Ban mới nếu chưa có trong hệ thống
        try {
          const deptCode = deptName.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').toUpperCase().replace(/[^A-Z0-9]+/g, '_').slice(0, 20) || 'DEPT'
          const newDept = await departmentsApi.create({
            departmentName: deptName,
            departmentCode: deptCode,
            description: 'Tự động tạo từ Import Excel Nhân sự'
          })
          if (newDept && newDept.departmentID) {
            deptId = newDept.departmentID
            departments.value.push(newDept)
          }
        } catch (_) {
          deptId = departments.value[0]?.departmentID || 1
        }
      }
    }
    if (!deptId) {
      deptId = departments.value[0]?.departmentID || 1
    }

    const title = getRowValue(row, ['title', 'chức danh', 'chuc danh', 'chucdanh', 'chức vụ', 'chuc vu', 'position']).trim()
    
    // Trạng thái nhân sự: ON -> Active, OFF -> Resigned
    let empStatus = 'Active'
    if (row.status) {
      empStatus = row.status
    } else {
      const rawStatus = getRowValue(row, ['status', 'trạng thái', 'trang thai']).trim().toUpperCase()
      if (rawStatus === 'OFF' || rawStatus === 'RESIGNED' || rawStatus === 'ĐÃ NGHỈ' || rawStatus === 'NGHI VIEC') {
        empStatus = 'Resigned'
      }
    }

    // Trích xuất địa chỉ email (chặn tuyệt đối từ khóa trạng thái như Available)
    const rawEmail = (row.email !== undefined && row.email !== null) ? String(row.email).trim() : getRowValue(row, [
      'email (địa chỉ email mới)*',
      'email (địa chỉ email mới)',
      'email (địa chỉ email)',
      'email (điền email mới tại đây)*',
      'email (điền email mới tại đây)',
      'email (dien email moi tai day)',
      'địa chỉ email',
      'dia chi email',
      'email liên hệ',
      'email cá nhân',
      'thư điện tử',
      'thu dien tu',
      'email',
      'mail'
    ]).trim()

    const statusWords = ['available', 'disable', 'active', 'deleted', 'hoạt động', 'bật', 'khóa', 'đã xóa', 'chưa có', '---']
    const email = (rawEmail && !statusWords.includes(rawEmail.toLowerCase())) ? rawEmail : ''

    const phone = getRowValue(row, ['số điện thoại', 'so dien thoai', 'sdt', 'phone', 'điện thoại', 'dien thoai']).trim()
    const rawJoinDate = getRowValue(row, ['ngày vào làm', 'ngay vao lam', 'ngayvaolam', 'ngày vào', 'ngay vao', 'joindate', 'ngày làm việc']).trim()
    const joinDate = normalizeDateValue(rawJoinDate) || new Date().toISOString().split('T')[0]

    const parseAccountStatus = (val, defaultVal = 'Disable') => {
      if (!val) return defaultVal
      const v = String(val).trim().toLowerCase()
      if (v === 'available' || v === 'active' || v === 'hoạt động' || v === 'bật' || v === 'có' || v === 'yes') return 'Available'
      if (v === 'deleted' || v === 'đã xóa' || v === 'xóa') return 'Deleted'
      return 'Disable'
    }

    const rawEmailStatus = getRowValue(row, ['trạng thái email cty', 'trạng thái email', 'email_status', 'email status', 'tài khoản email'])
    // Nếu có địa chỉ email thì mặc định kích hoạt Available (trừ khi có cột trạng thái chỉ định khác)
    const emailStatus = rawEmailStatus ? parseAccountStatus(rawEmailStatus) : (email ? 'Available' : 'Disable')

    // Nhận diện tài khoản Windows / Active Directory (Account)
    const rawAccount = getRowValue(row, ['account', 'tài khoản', 'tai khoan', 'tài khoản ad', 'ad account']).trim()
    const hasRealAccount = rawAccount && rawAccount !== 'AP\\' && rawAccount !== '---'
    const rawAdStatus = getRowValue(row, ['active directory', 'ad', 'ad_status', 'tài khoản windows'])
    const adStatus = rawAdStatus ? parseAccountStatus(rawAdStatus) : (hasRealAccount ? 'Available' : 'Disable')

    // Tìm xem nhân sự đã có sẵn trong danh sách chưa
    let existingEmp = null
    if (row._matchedEmpId) {
      existingEmp = employees.value.find(e => e.employeeID === row._matchedEmpId)
    }
    if (!existingEmp && code) {
      existingEmp = employees.value.find(e => e.employeeCode && e.employeeCode.trim().toLowerCase() === code.toLowerCase())
    }
    if (!existingEmp && name) {
      existingEmp = employees.value.find(e => e.fullName.trim().toLowerCase() === name.toLowerCase() && (deptId ? e.departmentID === deptId : true))
    }

    // A. NẾU NHÂN SỰ ĐÃ CÓ TRONG HỆ THỐNG & ĐƯỢC PHÉP CẬP NHẬT
    if (existingEmp && (row.updateExisting || row._actionType === 'update')) {
      try {
        const updatePayload = {
          employeeID: existingEmp.employeeID,
          employeeCode: code || existingEmp.employeeCode || '',
          fullName: name || existingEmp.fullName,
          englishName: englishName || existingEmp.englishName || null,
          departmentID: deptId || existingEmp.departmentID || 1,
          title: title || existingEmp.title || 'Nhân viên',
          email: email || existingEmp.email || null,
          phone: phone || existingEmp.phone || null,
          joinDate: joinDate || existingEmp.joinDate,
          leaveDate: existingEmp.leaveDate,
          qad_Status: existingEmp.qaD_Status || 'Disable',
          oa_Status: existingEmp.oA_Status || 'Disable',
          email_Status: email ? 'Available' : (existingEmp.email_Status || 'Disable'),
          ad_Status: adStatus !== 'Disable' ? adStatus : (existingEmp.ad_Status || 'Disable'),
          status: empStatus || existingEmp.status || 'Active'
        }
        await employeesApi.update(existingEmp.employeeID, updatePayload)
        updatedCount++
      } catch {
        errorCount++
      }
    } 
    // B. NẾU LÀ NHÂN SỰ MỚI
    else {
      const payload = {
        employeeCode: code,
        fullName: name,
        englishName: englishName || null,
        departmentID: deptId || 1,
        title: title || 'Nhân viên',
        email: email || null,
        phone: phone || null,
        joinDate: joinDate,
        status: empStatus,
        qaD_Status: parseAccountStatus(getRowValue(row, ['tài khoản qad', 'tai khoan qad', 'qad', 'qad_status'])),
        oA_Status: parseAccountStatus(getRowValue(row, ['tài khoản oa', 'tai khoan oa', 'oa', 'oa_status'])),
        email_Status: emailStatus,
        aD_Status: adStatus
      }

      try {
        await employeesApi.create(payload)
        createdCount++
      } catch {
        errorCount++
      }
    }
  }

  isImportOpen.value = false
  submitting.value = false

  const resultMsg = []
  if (createdCount > 0) resultMsg.push(`Thêm mới ${createdCount} nhân sự`)
  if (updatedCount > 0) resultMsg.push(`Cập nhật Email & TT cho ${updatedCount} nhân sự hiện tại`)
  if (errorCount > 0) resultMsg.push(`Lỗi/Bỏ qua: ${errorCount}`)

  toastRef.value?.show(
    resultMsg.length > 0 ? resultMsg.join(' | ') : 'Không có thay đổi nào', 
    (createdCount > 0 || updatedCount > 0) ? 'success' : (errorCount > 0 ? 'error' : 'warning')
  )
  await fetchEmployees()
  try {
    const dRes = await departmentsApi.getAll()
    departments.value = dRes || []
  } catch (_) {}
}

const downloadTemplate = (isXlsx = true) => {
  downloadEmployeeTemplate(isXlsx)
}

const isExportMenuOpen = ref(false)
const exportDropdownRef = ref(null)

const exportButtonLabel = computed(() => {
  if (currentTab.value === 'resigned') return 'Xuất DS Đã Nghỉ Việc'
  if (currentTab.value === 'onboarding') return 'Xuất DS Chờ Đi Làm'
  if (currentTab.value === 'active') return 'Xuất DS Đang Làm Việc'
  return 'Xuất CSV'
})

// Đóng menu export khi click ra ngoài
const handleDocumentClick = (e) => {
  if (exportDropdownRef.value && !exportDropdownRef.value.contains(e.target)) {
    isExportMenuOpen.value = false
  }
}
onMounted(() => {
  document.addEventListener('click', handleDocumentClick)
})
onUnmounted(() => {
  document.removeEventListener('click', handleDocumentClick)
})

const exportByType = (type) => {
  isExportMenuOpen.value = false
  const targetType = type || (currentTab.value === 'resigned' ? 'resigned' : (currentTab.value === 'onboarding' ? 'onboarding' : 'active'))

  if (targetType === 'resigned') {
    const columns = [
      { label: 'Mã Nhân Viên', field: (r) => r.employeeCode || 'Chưa có mã' },
      { label: 'Họ Và Tên', field: 'fullName' },
      { label: 'Tên Tiếng Anh', field: 'englishName' },
      { label: 'Phòng Ban', field: 'departmentName' },
      { label: 'Chức Danh', field: 'title' },
      { label: 'Email', field: 'email' },
      { label: 'Số Điện Thoại', field: 'phone' },
      { label: 'Ngày Vào Làm', field: (r) => r.joinDate ? r.joinDate.split('T')[0] : '' },
      { label: 'Ngày Nghỉ Việc', field: (r) => r.leaveDate ? r.leaveDate.split('T')[0] : 'Đã nghỉ việc' },
      { label: 'Tài Khoản QAD', field: (r) => r.qaD_Status || 'Disable' },
      { label: 'Tài Khoản OA', field: (r) => r.oA_Status || 'Disable' },
      { label: 'Email Cty', field: (r) => r.email_Status || 'Disable' },
      { label: 'Active Directory', field: (r) => r.aD_Status || 'Disable' },
      { 
        label: 'Tình Trạng Thiết Bị', 
        field: (r) => (r.holdingAssetCount || 0) === 0 ? 'Đã thu hồi hết' : `Còn giữ ${r.holdingAssetCount} thiết bị` 
      },
      { label: 'Số Thiết Bị Còn Giữ', field: (r) => r.holdingAssetCount || 0 },
      { label: 'Danh Sách Thiết Bị Chưa Thu Hồi', field: (r) => (r.holdingAssetNames || []).join(' ; ') }
    ]
    exportToCsv('Danh_Sach_Nhan_Vien_Da_Nghi_Viec', resignedEmployees.value, columns)
  } else if (targetType === 'active') {
    const columns = [
      { label: 'Mã Nhân Viên', field: (r) => r.employeeCode || 'Chưa có mã' },
      { label: 'Họ Và Tên', field: 'fullName' },
      { label: 'Tên Tiếng Anh', field: 'englishName' },
      { label: 'Phòng Ban', field: 'departmentName' },
      { label: 'Chức Danh', field: 'title' },
      { label: 'Email', field: 'email' },
      { label: 'Số Điện Thoại', field: 'phone' },
      { label: 'Ngày Vào Làm', field: (r) => r.joinDate ? r.joinDate.split('T')[0] : '' },
      { label: 'Tài Khoản QAD', field: (r) => r.qaD_Status || 'Disable' },
      { label: 'Tài Khoản OA', field: (r) => r.oA_Status || 'Disable' },
      { label: 'Email Cty', field: (r) => r.email_Status || 'Disable' },
      { label: 'Active Directory', field: (r) => r.aD_Status || 'Disable' },
      { label: 'Số Thiết Bị Đang Giữ', field: (r) => r.holdingAssetCount || 0 },
      { label: 'Danh Sách Thiết Bị Đang Dùng', field: (r) => (r.holdingAssetNames || []).join(' ; ') }
    ]
    exportToCsv('Danh_Sach_Nhan_Vien_Dang_Lam_Viec', activeEmployees.value, columns)
  } else if (targetType === 'onboarding') {
    const columns = [
      { label: 'Mã Nhân Viên', field: (r) => r.employeeCode || 'Chưa có mã' },
      { label: 'Họ Và Tên', field: 'fullName' },
      { label: 'Tên Tiếng Anh', field: 'englishName' },
      { label: 'Phòng Ban', field: 'departmentName' },
      { label: 'Chức Danh', field: (r) => r.title || 'Nhân viên mới' },
      { label: 'Email Liên Hệ', field: 'email' },
      { label: 'Số Điện Thoại', field: 'phone' },
      { label: 'Ngày Dự Kiến Vào Làm', field: (r) => r.joinDate ? r.joinDate.split('T')[0] : '' },
      { label: 'Email Công Ty', field: (r) => r.email_Status || 'Disable' },
      { label: 'Trạng Thái Cấp Máy', field: (r) => (r.holdingAssetCount || 0) > 0 ? `Đã cấp ${r.holdingAssetCount} thiết bị` : 'Chưa cấp máy' },
      { label: 'Danh Sách Thiết Bị Đã Cấp', field: (r) => (r.holdingAssetNames || []).join(' ; ') }
    ]
    exportToCsv('Danh_Sach_Nhan_Su_Cho_Di_Lam', onboardingEmployees.value, columns)
  } else {
    // Xuất toàn bộ
    const columns = [
      { label: 'Mã Nhân Viên', field: (r) => r.employeeCode || 'Chưa có mã' },
      { label: 'Họ Và Tên', field: 'fullName' },
      { label: 'Tên Tiếng Anh', field: 'englishName' },
      { label: 'Phòng Ban', field: 'departmentName' },
      { label: 'Chức Danh', field: 'title' },
      { label: 'Email', field: 'email' },
      { label: 'Số Điện Thoại', field: 'phone' },
      { label: 'Ngày Vào Làm', field: (r) => r.joinDate ? r.joinDate.split('T')[0] : '' },
      { label: 'Ngày Nghỉ Việc', field: (r) => r.leaveDate ? r.leaveDate.split('T')[0] : '' },
      { label: 'Trạng Thái', field: (r) => (r.status === 'Resigned' || r.leaveDate) ? 'Đã nghỉ việc' : (r.joinDate && new Date(r.joinDate) > new Date() ? 'Chờ đi làm' : 'Đang làm việc') },
      { label: 'Tài Khoản QAD', field: (r) => r.qaD_Status || 'Disable' },
      { label: 'Tài Khoản OA', field: (r) => r.oA_Status || 'Disable' },
      { label: 'Email Cty', field: (r) => r.email_Status || 'Disable' },
      { label: 'Active Directory', field: (r) => r.aD_Status || 'Disable' },
      { label: 'Số Thiết Bị Đang Giữ', field: (r) => r.holdingAssetCount || 0 },
      { label: 'Danh Sách Thiết Bị', field: (r) => (r.holdingAssetNames || []).join(' ; ') }
    ]
    exportToCsv('Danh_Sach_Toan_Bo_Nhan_Vien', employees.value, columns)
  }
}

const handleExportCsv = () => {
  exportByType()
}

const handleExportOnboardingCsv = () => {
  const columns = [
    { label: 'Mã Nhân Viên', field: (r) => r.employeeCode || 'Chưa có mã' },
    { label: 'Họ Và Tên', field: 'fullName' },
    { label: 'Tên Tiếng Anh', field: 'englishName' },
    { label: 'Phòng Ban', field: 'departmentName' },
    { label: 'Chức Danh', field: (r) => r.title || 'Nhân viên mới' },
    { label: 'Email Liên Hệ', field: 'email' },
    { label: 'Số Điện Thoại', field: 'phone' },
    { 
      label: 'Ngày Dự Kiến Vào Làm', 
      field: (r) => r.joinDate ? r.joinDate.split('T')[0] : '' 
    },
    { 
      label: 'Số Ngày Còn Lại', 
      field: (r) => {
        if (!r.joinDate) return ''
        const today = new Date()
        today.setHours(0, 0, 0, 0)
        const joinDate = new Date(r.joinDate)
        joinDate.setHours(0, 0, 0, 0)
        const diff = Math.ceil((joinDate - today) / (1000 * 60 * 60 * 24))
        return diff > 0 ? `Còn ${diff} ngày` : 'Hôm nay'
      }
    },
    { label: 'Email Công Ty', field: (r) => r.email_Status || 'Disable' },
    { label: 'Thiết Bị Cấp Phát', field: (r) => r.holdingAssetCount > 0 ? `Đã cấp ${r.holdingAssetCount} thiết bị` : 'Chưa cấp máy' }
  ]

  const dataToExport = filteredOnboardingEmployees.value && filteredOnboardingEmployees.value.length > 0 
    ? filteredOnboardingEmployees.value 
    : onboardingEmployees.value

  exportToCsv('Danh_Sach_Nhan_Su_Cho_Di_Lam', dataToExport, columns)
}

const handleExportOnboardingEmailTemplate = async () => {
  const data = filteredOnboardingEmployees.value && filteredOnboardingEmployees.value.length > 0 
    ? filteredOnboardingEmployees.value 
    : onboardingEmployees.value

  if (!data || data.length === 0) {
    toastRef.value?.addToast('Thông báo', 'Không có nhân sự mới nào trong danh sách hiện tại.', 'warning')
    return
  }

  await exportOnboardingEmailTemplate(data, true)
  toastRef.value?.addToast('Thành công', `Đã xuất file Excel mẫu điền Email cho ${data.length} nhân sự mới!`, 'success')
}

onMounted(() => {
  fetchEmployees()
  fetchLeaveAlerts()
  fetchMissingHandoverAlerts()
  fetchDepartments()
  fetchCategories()
})
</script>

<style>
/* CSS DÙNG CHUNG CHO EMPLOYEES VIEW VÀ CÁC SUB-COMPONENTS */
.employees-view {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.header-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.tab-nav-bar {
  display: flex;
  gap: 8px;
  background: var(--bg-surface-alt, #f8fafc);
  padding: 4px;
  border-radius: var(--radius-md, 8px);
  border: 1px solid var(--border-color, #e2e8f0);
  flex-wrap: wrap;
}

.tab-btn {
  background: transparent;
  border: none;
  color: var(--text-muted, #64748b);
  font-family: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab-btn.active {
  background: #ffffff;
  color: var(--primary, #2563eb);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-btn.onboarding-tab.active {
  background: #eff6ff;
  color: #2563eb;
  border: 1px solid #bfdbfe;
}

.tab-btn.resigned-tab.active {
  background: #fff7ed;
  color: #ea580c;
  border: 1px solid #fed7aa;
}

.tab-btn.alert-tab.active {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.tab-btn.history-tab.active {
  background: #f5f3ff;
  color: #7c3aed;
  border: 1px solid #ddd6fe;
}

.badge-urgent-dot {
  background: #dc2626;
  color: #ffffff;
  font-size: 0.7rem;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 999px;
  margin-left: 6px;
  animation: pulse-urgent 1.5s infinite;
}

.hr-readonly-banner {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-left: 4px solid #3b82f6;
  color: #1e40af;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 0.85rem;
  line-height: 1.45;
  margin-bottom: 16px;
}

.hr-readonly-modal-banner {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-left: 4px solid #f59e0b;
  color: #92400e;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 0.825rem;
  line-height: 1.45;
  margin-bottom: 16px;
}

.cursor-default {
  cursor: default !important;
}

.cursor-pointer {
  cursor: pointer !important;
}

@keyframes pulse-urgent {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.08); opacity: 0.85; }
  100% { transform: scale(1); opacity: 1; }
}

/* KHỐI KHẨN CẤP */
.urgent-onboarding-section {
  background: linear-gradient(135deg, rgba(254, 242, 242, 0.95) 0%, rgba(255, 237, 213, 0.9) 100%);
  border: 2px solid #fca5a5;
  border-radius: var(--radius-lg, 12px);
  padding: 20px 24px;
  box-shadow: 0 10px 25px -5px rgba(239, 68, 68, 0.15);
  margin-bottom: 20px;
}

.urgent-section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 16px;
}

.urgent-header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.urgent-pulse-icon {
  font-size: 1.8rem;
  animation: pulse-urgent 1.2s infinite;
}

.urgent-title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 800;
  color: #991b1b;
  letter-spacing: 0.02em;
}

.urgent-subtitle {
  margin: 2px 0 0 0;
  font-size: 0.825rem;
  color: #7f1d1d;
}

.urgent-count-badge {
  font-size: 0.8rem;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 999px;
  background: #ffffff;
  border: 1px solid #fca5a5;
  color: #dc2626;
}

.urgent-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(360px, 1fr));
  gap: 16px;
}

.urgent-emp-card {
  background: #ffffff;
  border: 1.5px solid #fecaca;
  border-radius: var(--radius-md, 8px);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.08);
}

.urgent-card-badge-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.badge-urgent-tag {
  background: #fee2e2;
  color: #dc2626;
  font-size: 0.725rem;
  font-weight: 800;
  padding: 2px 8px;
  border-radius: 4px;
  border: 1px solid #fca5a5;
}

.badge-countdown-tag {
  background: #fef3c7;
  color: #b45309;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
}

.btn-unmark-urgent {
  background: #f1f5f9;
  border: 1px solid #cbd5e1;
  color: #64748b;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-unmark-urgent:hover {
  background: #fee2e2;
  color: #dc2626;
  border-color: #fca5a5;
}

.urgent-emp-main {
  display: flex;
  align-items: center;
  gap: 12px;
}

.urgent-avatar-box {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: #eff6ff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
}

.urgent-emp-name {
  font-size: 1rem;
  color: #0f172a;
}

.urgent-emp-code {
  font-family: monospace;
  font-size: 0.8rem;
  color: #64748b;
  margin-left: 6px;
}

.badge-dept-tag {
  background: #eff6ff;
  color: #2563eb;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 4px;
  border: 1px solid #bfdbfe;
}

.urgent-status-summary {
  display: flex;
  flex-direction: column;
  gap: 4px;
  background: #f8fafc;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 0.8rem;
}

.urgent-stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.badge-device-ready {
  color: #059669;
  font-weight: 700;
}

.badge-device-missing {
  color: #dc2626;
  font-weight: 700;
}

.urgent-accounts-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  font-size: 0.8rem;
}

.urgent-acc-pills {
  display: flex;
  gap: 4px;
}

.acc-mini-pill {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 3px;
}

.acc-mini-pill.is-active {
  background: #dcfce7;
  color: #15803d;
}

.acc-mini-pill.is-pending {
  background: #f1f5f9;
  color: #94a3b8;
}

.urgent-card-actions {
  display: flex;
  gap: 8px;
  margin-top: 4px;
}

.urgent-empty-tip {
  display: flex;
  gap: 12px;
  background: #ffffff;
  padding: 14px 18px;
  border-radius: 8px;
  border: 1px dashed #fca5a5;
  align-items: center;
}

.urgent-empty-icon {
  font-size: 1.5rem;
}

.urgent-empty-text p {
  margin: 2px 0 0 0;
  font-size: 0.8rem;
  color: #64748b;
}

/* BẢNG & PROFILE CELLS */
.emp-profile-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

/* Modern Profile Card in Table */
.emp-profile-card {
  display: flex;
  align-items: center;
  gap: 12px;
  text-align: left;
}

.clickable-profile {
  cursor: pointer;
  padding: 4px 6px;
  margin: -4px -6px;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.clickable-profile:hover {
  background: rgba(37, 99, 235, 0.05);
}

.emp-avatar-circle {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 0.85rem;
  color: #1e293b;
  flex-shrink: 0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  border: 1.5px solid #ffffff;
}

.emp-profile-body {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.emp-name-row {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  line-height: 1.2;
}

.emp-name {
  font-size: 0.925rem;
  font-weight: 700;
  color: #0f172a;
  white-space: nowrap;
}

.clickable-name:hover {
  color: var(--primary, #2563eb);
}

.emp-code-badge {
  font-family: monospace;
  font-size: 0.725rem;
  font-weight: 700;
  background: #f1f5f9;
  color: #475569;
  padding: 1.5px 6px;
  border-radius: 4px;
  border: 1px solid #e2e8f0;
}

.edit-hint-icon {
  font-size: 0.75rem;
  opacity: 0.6;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.clickable-profile:hover .edit-hint-icon {
  opacity: 1;
  transform: scale(1.15);
}

.badge-resigned-mini {
  background: #fee2e2;
  color: #dc2626;
  font-size: 0.7rem;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 4px;
}

.badge-urgent-tag-sm {
  background: #dc2626;
  color: #ffffff;
  font-size: 0.675rem;
  font-weight: 800;
  padding: 1.5px 6px;
  border-radius: 4px;
  animation: pulse-urgent 1.5s infinite;
}

.emp-contact-details {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.contact-pill {
  display: inline-flex;
  align-items: center;
  gap: 4.5px;
  font-size: 0.75rem;
  color: #475569;
  background: #f8fafc;
  padding: 2px 7px;
  border-radius: 5px;
  border: 1px solid #e2e8f0;
  white-space: nowrap;
  transition: all 0.15s ease;
}

.contact-pill svg {
  color: #64748b;
  flex-shrink: 0;
}

.contact-pill.email:hover {
  color: #2563eb;
  background: #eff6ff;
  border-color: #bfdbfe;
}

.contact-pill.phone:hover {
  color: #059669;
  background: #ecfdf5;
  border-color: #a7f3d0;
}

.contact-val {
  max-width: 180px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.contact-empty {
  font-size: 0.75rem;
  color: #94a3b8;
  font-style: italic;
}

/* Modern Department & Title Cell */
.dept-cell-modern {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
}

.dept-text-main {
  font-size: 0.85rem;
  font-weight: 600;
  color: #1e293b;
  line-height: 1.35;
}

.title-badge-modern {
  font-size: 0.725rem;
  font-weight: 600;
  color: #475569;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 4px;
  border: 1px solid #e2e8f0;
  display: inline-block;
  white-space: nowrap;
}

.badge-countdown-pill {
  font-size: 0.725rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 999px;
  background: #eff6ff;
  color: #2563eb;
  margin-top: 2px;
}

.badge-countdown-pill.is-urgent {
  background: #fef2f2;
  color: #dc2626;
}

.badge-countdown-pill.is-warning {
  background: #fffbeb;
  color: #d97706;
}

.btn-urgent-toggle {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 6px;
  border: 1px solid #cbd5e1;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-urgent-toggle.mark {
  background: #ffffff;
  color: #dc2626;
  border-color: #fca5a5;
}

.btn-urgent-toggle.mark:hover {
  background: #fef2f2;
}

.btn-urgent-toggle.unmark {
  background: #f1f5f9;
  color: #64748b;
}

.badge-priority-urgent {
  font-size: 0.725rem;
  font-weight: 800;
  color: #dc2626;
  background: #fef2f2;
  padding: 2px 8px;
  border-radius: 4px;
  margin-bottom: 4px;
  display: inline-block;
}

.holding-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.775rem;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 999px;
  border: 1px solid transparent;
  cursor: pointer;
}

.holding-pill.active {
  background: #eff6ff;
  color: #2563eb;
  border-color: #bfdbfe;
}

.holding-pill.warning {
  background: #fff7ed;
  color: #ea580c;
  border-color: #fed7aa;
}

.holding-pill.empty {
  background: #f8fafc;
  color: #64748b;
  border-color: #e2e8f0;
}

.returned-tag-mini, .returned-tag {
  color: #059669;
  font-weight: 700;
  font-size: 0.775rem;
}

.account-warning-badge-btn {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 6px;
}

.accounts-disabled-tag {
  color: #64748b;
  font-size: 0.775rem;
}

.leave-date-badge {
  font-family: monospace;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 4px;
  background: #f8fafc;
}

.leave-date-badge.danger {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.row-action-btns {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

/* TIMELINE LỊCH SỬ BIẾN ĐỘNG */
.history-timeline-wrap {
  display: flex;
  flex-direction: column;
  gap: 16px;
  position: relative;
}

.timeline-event-card {
  display: grid;
  grid-template-columns: 110px 42px 1fr;
  gap: 14px;
  position: relative;
}

.event-time-col {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: flex-start;
  padding-top: 6px;
}

.event-date {
  font-size: 0.825rem;
  font-weight: 800;
  color: #334155;
  font-family: monospace;
}

.event-time {
  font-size: 0.725rem;
  color: #64748b;
  font-family: monospace;
}

.event-node-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

.event-node-icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #ffffff;
  border: 2px solid #cbd5e1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  z-index: 2;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.event-node-icon.icon-StartedWorking {
  border-color: #10b981;
  background: #ecfdf5;
}

.event-node-icon.icon-OnboardingCreated {
  border-color: #3b82f6;
  background: #eff6ff;
}

.event-node-icon.icon-UrgentMarked {
  border-color: #ef4444;
  background: #fef2f2;
  animation: pulse-urgent 1.5s infinite;
}

.event-line {
  flex: 1;
  width: 2px;
  background: #e2e8f0;
  margin-top: 4px;
  margin-bottom: -16px;
}

.timeline-event-card:last-child .event-line {
  display: none;
}

.event-content-col {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: var(--radius-md, 8px);
  padding: 14px 18px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.event-header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 8px;
  flex-wrap: wrap;
}

.event-title-group {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.event-main-title {
  font-size: 0.925rem;
  color: #0f172a;
  font-weight: 700;
}

.event-actor-tag {
  font-size: 0.75rem;
  color: #64748b;
  background: #f8fafc;
  padding: 2px 8px;
  border-radius: 4px;
  border: 1px solid #e2e8f0;
}

.badge-history-type {
  font-size: 0.7rem;
  font-weight: 800;
  padding: 2px 8px;
  border-radius: 4px;
  text-transform: uppercase;
}

.badge-type-StartedWorking { background: #ecfdf5; color: #059669; border: 1px solid #a7f3d0; }
.badge-type-OnboardingCreated { background: #eff6ff; color: #2563eb; border: 1px solid #bfdbfe; }
.badge-type-UrgentMarked { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
.badge-type-UrgentUnmarked { background: #f8fafc; color: #64748b; border: 1px solid #cbd5e1; }
.badge-type-AssetAssigned { background: #f5f3ff; color: #7c3aed; border: 1px solid #ddd6fe; }
.badge-type-AssetReturned { background: #fff7ed; color: #ea580c; border: 1px solid #fed7aa; }
.badge-type-AccountUpdated { background: #ecfeff; color: #0891b2; border: 1px solid #a5f3fc; }
.badge-type-ProfileUpdated { background: #f1f5f9; color: #475569; border: 1px solid #cbd5e1; }
.badge-type-Resigned { background: #fef2f2; color: #b91c1c; border: 1px solid #fca5a5; }

.event-emp-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 999px;
  padding: 3px 10px;
  font-size: 0.775rem;
  margin-bottom: 8px;
}

.event-emp-name {
  font-weight: 700;
  color: #1e293b;
}

.event-description-text {
  font-size: 0.85rem;
  color: #334155;
  line-height: 1.5;
  margin: 0 0 8px 0;
}

.event-diff-box {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #f8fafc;
  border: 1px dashed #cbd5e1;
  border-radius: 6px;
  padding: 6px 12px;
  font-size: 0.775rem;
  margin-top: 6px;
}

.diff-old .diff-val {
  color: #dc2626;
  text-decoration: line-through;
  background: #fef2f2;
  padding: 1px 6px;
  border-radius: 4px;
}

.diff-new .diff-val {
  color: #16a34a;
  font-weight: 700;
  background: #f0fdf4;
  padding: 1px 6px;
  border-radius: 4px;
}

/* THIẾT BỊ MODAL CARDS */
.emp-assets-header-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  margin-bottom: 16px;
}

.emp-asset-card-enhanced {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px 18px;
  margin-bottom: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.emp-asset-card-enhanced.is-selected-for-print {
  border-color: #6366f1;
  background: #f5f3ff;
}

.asset-card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.asset-left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.code-badge {
  font-family: monospace;
  font-weight: 700;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
}

.asset-specs-grid-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 8px;
  font-size: 0.8rem;
  background: #f8fafc;
  padding: 8px 12px;
  border-radius: 6px;
}

.bulk-select-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  padding: 10px 16px;
  border-radius: 8px;
  margin-bottom: 14px;
}

.pdf-attached-box {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fef2f2;
  border: 1px solid #fecaca;
  padding: 8px 12px;
  border-radius: 6px;
}

.pdf-file-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.pdf-filename {
  font-size: 0.825rem;
  color: #1e293b;
}

.pdf-action-btns {
  display: flex;
  gap: 6px;
}

.btn-pdf-act {
  font-size: 0.75rem;
  padding: 3px 8px;
  border-radius: 4px;
  border: 1px solid #cbd5e1;
  background: #ffffff;
  cursor: pointer;
  text-decoration: none;
  color: #334155;
}

.pdf-empty-upload-box {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
  border: 1px dashed #cbd5e1;
  padding: 8px 12px;
  border-radius: 6px;
}

.asset-item-actions-bar {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  border-top: 1px dashed #e2e8f0;
  padding-top: 10px;
}

.btn-asset-act {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.775rem;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 6px;
  border: 1px solid #cbd5e1;
  background: #ffffff;
  cursor: pointer;
}

.btn-asset-act.print { color: #4f46e5; border-color: #c7d2fe; }
.btn-asset-act.transfer { color: #0284c7; border-color: #bae6fd; }
.btn-asset-act.return { color: #ea580c; border-color: #fed7aa; }
.btn-asset-act.broken { color: #dc2626; border-color: #fecaca; }

/* CSS IN PHIẾU BÀN GIAO A4 */
.handover-preview-wrapper {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: 80vh;
  overflow-y: auto;
}

.handover-document-form {
  background: #ffffff;
  color: #000000;
  padding: 20px 18px 20px 32px;
  border: 1px solid #e2e8f0;
  font-family: Arial, Helvetica, sans-serif;
}

.h-doc-header-wrap {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.h-company-logo {
  height: 48px;
  object-fit: contain;
}

.h-doc-title-box {
  text-align: center;
  flex: 1;
}

.h-doc-title-en {
  font-size: 13pt;
  font-weight: 900;
  text-transform: uppercase;
  margin: 0;
}

.h-doc-title-vn {
  font-size: 10pt;
  font-weight: bold;
  color: #475569;
  margin: 2px 0 0 0;
}

.h-doc-date-bar {
  display: flex;
  justify-content: space-between;
  border-bottom: 2px solid #000000;
  padding-bottom: 6px;
  margin-bottom: 12px;
  font-size: 9pt;
}

.h-doc-section {
  margin-bottom: 12px;
}

.h-section-header {
  background: #f1f5f9;
  border: 1px solid #000000;
  border-bottom: none;
  font-weight: 900;
  font-size: 10pt;
  padding: 5px 10px;
}

.h-table-grid {
  width: 100%;
  border-collapse: collapse;
  font-size: 9pt;
}

.h-table-grid td {
  border: 1px solid #000000;
  padding: 4px 8px;
}

.h-lbl {
  background: #f8fafc;
  font-weight: bold;
  width: 16%;
}

.h-agreement-box {
  border: 1px solid #000000;
  padding: 8px 12px;
  font-size: 8pt;
  line-height: 1.4;
}

.h-sign-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
  text-align: center;
  border: 1px solid #000000;
}

.h-sign-th {
  border: 1px solid #000000;
  padding: 4px;
  font-size: 9.5pt;
  font-weight: bold;
  background: #f1f5f9;
}

.h-sign-sub {
  border: 1px solid #000000;
  padding: 2px;
  font-size: 8pt;
  color: #475569;
}

.h-sign-cell {
  border: 1px solid #000000;
  height: 70px;
  vertical-align: bottom;
  padding: 6px;
}

.handover-action-bar {
  display: flex;
  justify-content: space-between;
  border-top: 1px solid #e2e8f0;
  padding-top: 12px;
}

/* PDF VIEWER */
.pdf-viewer-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.pdf-viewer-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
  padding: 10px 14px;
  border-radius: 8px;
}

.pdf-frame-wrapper iframe {
  width: 100%;
  height: 600px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
}

/* MODAL GRIDS */
.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.modal-grid-4 {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.modal-actions-between {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-actions-right {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.account-control-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.account-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  padding: 12px 16px;
  border-radius: 8px;
}

.account-info {
  display: flex;
  flex-direction: column;
}

.account-title {
  font-weight: 700;
  color: #1e293b;
}

.account-sub {
  font-size: 0.775rem;
  color: #64748b;
}

.status-select {
  width: 180px;
}

.emp-account-box-edit {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px;
  margin-bottom: 16px;
}

.onboarding-input-hint {
  font-size: 0.775rem;
  color: #2563eb;
  margin-top: 4px;
}

/* IMPORT MODAL */
.import-modal-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.import-step-card {
  display: flex;
  gap: 14px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 16px;
}

.step-num {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--primary, #2563eb);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  flex-shrink: 0;
}

.step-body {
  flex: 1;
}

.step-title {
  font-weight: 700;
  margin-bottom: 4px;
}

.step-desc {
  font-size: 0.8rem;
  color: #64748b;
  margin: 0 0 10px 0;
}

.import-drop-zone {
  border: 2px dashed #cbd5e1;
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  background: #ffffff;
  transition: all 0.2s ease;
}

.import-drop-zone.dragging {
  border-color: var(--primary, #2563eb);
  background: #eff6ff;
}

.import-preview-table-wrap {
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: #ffffff;
  margin-top: 8px;
}

.import-preview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.775rem;
}

.import-preview-table th, .import-preview-table td {
  padding: 6px 10px;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
}

.import-preview-table th {
  background: #f8fafc;
  position: sticky;
  top: 0;
}

/* EXPORT DROPDOWN MENU */
.export-dropdown-wrapper {
  position: relative;
  display: inline-block;
}

.export-main-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 600;
}

.caret-icon {
  font-size: 0.75rem;
  opacity: 0.8;
  margin-left: 2px;
}

.export-dropdown-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  background: #ffffff;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 8px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.12), 0 4px 10px rgba(0, 0, 0, 0.06);
  min-width: 260px;
  padding: 6px;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 2px;
  animation: fadeInDown 0.15s ease-out;
}

@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-6px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.export-menu-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  width: 100%;
  padding: 8px 12px;
  background: transparent;
  border: none;
  border-radius: 6px;
  font-size: 0.825rem;
  font-weight: 500;
  color: #1e293b;
  cursor: pointer;
  text-align: left;
  transition: all 0.15s ease;
}

.export-menu-item:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.export-menu-item.active-type:hover {
  background: #ecfdf5;
  color: #059669;
}

.export-menu-item.resigned-type:hover {
  background: #fff1f2;
  color: #e11d48;
}

.export-menu-item.onboarding-type:hover {
  background: #eff6ff;
  color: #2563eb;
}

.badge-count-pill {
  font-size: 0.725rem;
  background: #f1f5f9;
  color: #475569;
  padding: 2px 7px;
  border-radius: 12px;
  font-weight: 700;
}

.export-menu-item:hover .badge-count-pill {
  background: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.export-menu-divider {
  height: 1px;
  background: #e2e8f0;
  margin: 4px 0;
}
</style>
