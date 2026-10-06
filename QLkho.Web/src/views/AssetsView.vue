<template>
  <div class="assets-view">
    <!-- TOP HEADER -->
    <div class="view-header">
      <div>
        <h1 class="page-title">{{ $t('assets.title') }}</h1>
        <p class="page-subtitle">{{ $t('assets.subtitle') }}</p>
      </div>

      <div class="header-actions">
        <!-- Nút Lịch Sử Bàn Giao Tuần/Tháng -->
        <button type="button" class="btn btn-secondary" @click="isHistoryModalOpen = true">
          🕒 {{ $t('employees.tab_history') }}
        </button>

        <!-- Nút Import Cấp Phát / Bàn Giao Hàng Loạt -->
        <button type="button" class="btn btn-primary" style="background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: white; border: none;" @click="isHandoverImportOpen = true">
          📋 Import Cấp Phát
        </button>

        <!-- Nút Import Excel -->
        <button type="button" class="btn btn-secondary" @click="isImportOpen = true">
          📥 {{ $t('assets.btn_import') }}
        </button>

        <!-- Nút Xuất CSV -->
        <button type="button" class="btn btn-secondary" @click="handleExportCsv">
          📊 {{ $t('assets.btn_export') }}
        </button>

        <!-- Nút Quản Lý Hãng -->
        <button type="button" class="btn btn-secondary" @click="isBrandManagerOpen = true">
          🏷️ {{ $t('assets.btn_brands') }} ({{ brands.length }})
        </button>

        <!-- Nút Cảnh Báo Bảo Hành -->
        <button type="button" class="btn btn-warning-outline" @click="openWarrantyModal">
          🛡️ {{ $t('assets.btn_warranty') }}
          <span v-if="warrantyAlerts.length > 0" class="badge-alert-count">
            {{ warrantyAlerts.length }}
          </span>
        </button>

        <!-- Nút Thêm Mới Thiết Bị -->
        <button type="button" class="btn btn-primary" @click="openCreateModal">
          ➕ {{ $t('assets.btn_add') }}
        </button>
      </div>
    </div>

    <!-- BỘ LỌC TÌM KIẾM & PHÂN LOẠI -->
    <AssetsFilterBar 
      :filters="filters"
      :categories="categories"
      :departments="departments"
      @update:search="filters.search = $event"
      @update:categoryID="filters.categoryID = $event"
      @update:status="filters.status = $event"
      @update:departmentID="filters.departmentID = $event"
    />

    <!-- BẢNG DANH SÁCH THIẾT BỊ GOM NHÓM & PHÂN TRANG -->
    <AssetsTable 
      :assets="filteredAssets"
      :loading="loading"
      @view-detail="openDetailModal"
    />

    <!-- 1. MODAL CHI TIẾT THIẾT BỊ & TIMELINE LỊCH SỬ -->
    <AssetDetailModal 
      :is-open="isDetailOpen"
      :asset="assetDetail"
      :loading="loadingDetail"
      @close="isDetailOpen = false"
      @assign="handleOpenAssignFromDetail"
      @transfer="handleOpenTransferFromDetail"
      @return="handleOpenReturnFromDetail"
      @report-issue="handleOpenReportIssueFromDetail"
      @print="handleOpenPrintFromDetail"
      @edit="handleOpenEditFromDetail"
      @delete="handleDeleteAsset"
    />

    <!-- 2. MODAL THÊM MỚI / CHỈNH SỬA THIẾT BỊ -->
    <AssetFormModal 
      :is-open="isCreateOpen || isEditOpen"
      :is-edit="isEditOpen"
      :form="isEditOpen ? editForm : createForm"
      :categories="categories"
      :suppliers="suppliers"
      :brands="brands"
      :existing-mouse-lot-info="existingMouseLotInfo"
      :existing-keyboard-lot-info="existingKeyboardLotInfo"
      :submitting="submitting"
      @close="isCreateOpen = false; isEditOpen = false"
      @submit="isEditOpen ? submitUpdateAsset() : submitCreateAsset()"
      @quick-cat="isQuickCatOpen = true"
      @quick-sup="isQuickSupOpen = true"
      @quick-brand="isQuickBrandOpen = true"
    />

    <!-- 3. MODAL CẤP PHÁT THIẾT BỊ (ASSIGN) -->
    <AssetAssignModal 
      :is-open="isAssignOpen"
      :asset="selectedAsset"
      :employees="activeEmployees"
      :departments="departments"
      :warehouse-mice="availableWarehouseMice"
      :warehouse-monitors="availableWarehouseMonitors"
      :warehouse-keyboards="availableWarehouseKeyboards"
      :submitting="submitting"
      @close="isAssignOpen = false"
      @submit="submitAssign"
    />

    <!-- 4. MODAL ĐIỀU CHUYỂN THIẾT BỊ (TRANSFER) -->
    <AssetTransferModal 
      :is-open="isTransferOpen"
      :asset="selectedAsset"
      :employees="activeEmployees"
      :departments="departments"
      :submitting="submitting"
      @close="isTransferOpen = false"
      @submit="submitTransfer"
    />

    <!-- 5. MODAL THU HỒI THIẾT BỊ (RETURN) -->
    <AssetReturnModal 
      :is-open="isReturnOpen"
      :asset="selectedAsset"
      :submitting="submitting"
      @close="isReturnOpen = false"
      @submit="submitReturn"
    />

    <!-- 6. MODAL BÁO HỎNG / BẢO TRÌ (REPORT ISSUE) -->
    <AssetReportIssueModal 
      :is-open="isReportIssueOpen"
      :asset="selectedAsset"
      :submitting="submitting"
      @close="isReportIssueOpen = false"
      @submit="submitReportIssue"
    />

    <!-- 7. MODAL IMPORT EXCEL THIẾT BỊ -->
    <AssetImportModal 
      :is-open="isImportOpen"
      :submitting="submitting"
      @close="isImportOpen = false"
      @import="submitImportAssets"
    />

    <!-- 7b. MODAL IMPORT CẤP PHÁT / BÀN GIAO THIẾT BỊ -->
    <AssetHandoverImportModal 
      :is-open="isHandoverImportOpen"
      @close="isHandoverImportOpen = false"
      @success="handleHandoverImportSuccess"
    />

    <!-- 8. MODAL IN BIÊN BẢN BÀN GIAO A4 -->
    <AssetPrintModal 
      :is-open="isPrintHandoverOpen"
      :asset="selectedAsset"
      :employees="activeEmployees"
      :categories="categories"
      :departments="departments"
      @close="isPrintHandoverOpen = false"
    />

    <!-- 9. MODAL CẢNH BÁO BẢO HÀNH -->
    <AssetWarrantyModal 
      :is-open="isWarrantyModalOpen"
      :loading="loadingWarranty"
      :alerts="warrantyAlerts"
      @close="isWarrantyModalOpen = false"
    />

    <!-- 10. MODAL QUẢN LÝ HÃNG SẢN XUẤT (BRAND CRUD) -->
    <AssetBrandModal 
      :is-open="isBrandManagerOpen"
      :brands="brands"
      :submitting="submittingBrand"
      @close="isBrandManagerOpen = false"
      @save="handleSaveBrand"
      @delete="handleDeleteBrand"
    />

    <!-- 11. MODAL LỊCH SỬ CẤP PHÁT TUẦN / THÁNG -->
    <HandoverHistoryModal 
      :is-open="isHistoryModalOpen" 
      initial-period="month"
      @close="isHistoryModalOpen = false" 
    />

    <!-- 12. MODAL TẠO NHANH LOẠI THIẾT BỊ -->
    <Modal 
      :is-open="isQuickCatOpen" 
      title="Tạo Nhanh Loại Thiết Bị"
      subtitle="Thêm nhóm chủng loại thiết bị mới vào hệ thống"
      @close="isQuickCatOpen = false"
      max-width="500px"
    >
      <form @submit.prevent="submitQuickCategory">
        <div class="form-group">
          <label class="form-label">Mã Loại *</label>
          <input type="text" class="form-control" v-model="quickCatForm.categoryCode" required placeholder="LT, PC, MN, PRN..." />
        </div>
        <div class="form-group">
          <label class="form-label">Tên Loại Thiết Bị *</label>
          <input type="text" class="form-control" v-model="quickCatForm.categoryName" required placeholder="Máy tính xách tay, Màn hình, Máy in..." />
        </div>
        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isQuickCatOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">Lưu Loại</button>
        </div>
      </form>
    </Modal>

    <!-- 13. MODAL TẠO NHANH NHÀ CUNG CẤP -->
    <Modal 
      :is-open="isQuickSupOpen" 
      title="Tạo Nhanh Nhà Cung Cấp"
      subtitle="Thêm đối tác cung cấp thiết bị mới"
      @close="isQuickSupOpen = false"
      max-width="500px"
    >
      <form @submit.prevent="submitQuickSupplier">
        <div class="form-group">
          <label class="form-label">Mã Nhà Cung Cấp *</label>
          <input type="text" class="form-control" v-model="quickSupForm.supplierCode" required placeholder="SUP-FPT, SUP-PHONGVU..." />
        </div>
        <div class="form-group">
          <label class="form-label">Tên Nhà Cung Cấp *</label>
          <input type="text" class="form-control" v-model="quickSupForm.supplierName" required placeholder="Công ty CP Bán Lẻ FPT..." />
        </div>
        <div class="form-group">
          <label class="form-label">Điện Thoại / Hotline</label>
          <input type="text" class="form-control" v-model="quickSupForm.phone" placeholder="0901234567" />
        </div>
        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isQuickSupOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">Lưu NCC</button>
        </div>
      </form>
    </Modal>

    <!-- 14. MODAL TẠO NHANH HÃNG SẢN XUẤT -->
    <Modal 
      :is-open="isQuickBrandOpen" 
      title="Thêm Nhanh Hãng Sản Xuất"
      subtitle="Thêm thương hiệu mới vào danh mục hệ thống"
      @close="isQuickBrandOpen = false"
      max-width="480px"
    >
      <form @submit.prevent="submitQuickBrand">
        <div class="form-group">
          <label class="form-label">Tên Hãng Sản Xuất *</label>
          <input type="text" class="form-control" v-model="quickBrandForm.brandName" required placeholder="Vd: Logitech, Dell, HP, Apple..." />
        </div>
        <div class="form-group">
          <label class="form-label">Quốc Gia / Xuất Xứ</label>
          <input type="text" class="form-control" v-model="quickBrandForm.originCountry" placeholder="Mỹ, Thụy Sĩ, Nhật Bản..." />
        </div>
        <div class="form-group">
          <label class="form-label">Mô Tả / Ghi Chú</label>
          <input type="text" class="form-control" v-model="quickBrandForm.description" placeholder="Chuột, bàn phím, laptop..." />
        </div>
        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isQuickBrandOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submittingBrand">
            <span v-if="submittingBrand" class="loading-spinner"></span>
            Lưu Hãng Mới
          </button>
        </div>
      </form>
    </Modal>

    <!-- TOAST THÔNG BÁO -->
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { assetsApi, categoriesApi, departmentsApi, employeesApi, suppliersApi, brandsApi } from '@/api/client'
import { getRowValue } from '@/utils/excelImport'
import Modal from '@/components/common/Modal.vue'
import Toast from '@/components/common/Toast.vue'
import HandoverHistoryModal from '@/components/assets/HandoverHistoryModal.vue'

// Import các sub-components đã tách nhỏ
import AssetsFilterBar from './AssetsView/AssetsFilterBar.vue'
import AssetsTable from './AssetsView/AssetsTable.vue'
import AssetDetailModal from './AssetsView/AssetDetailModal.vue'
import AssetFormModal from './AssetsView/AssetFormModal.vue'
import AssetAssignModal from './AssetsView/AssetAssignModal.vue'
import AssetTransferModal from './AssetsView/AssetTransferModal.vue'
import AssetReturnModal from './AssetsView/AssetReturnModal.vue'
import AssetReportIssueModal from './AssetsView/AssetReportIssueModal.vue'
import AssetImportModal from './AssetsView/AssetImportModal.vue'
import AssetHandoverImportModal from './AssetsView/AssetHandoverImportModal.vue'
import AssetPrintModal from './AssetsView/AssetPrintModal.vue'
import AssetWarrantyModal from './AssetsView/AssetWarrantyModal.vue'
import AssetBrandModal from './AssetsView/AssetBrandModal.vue'

// State dữ liệu chính
const assets = ref([])
const categories = ref([])
const departments = ref([])
const activeEmployees = ref([])
const suppliers = ref([])
const brands = ref([])
const warrantyAlerts = ref([])

const loading = ref(false)
const loadingDetail = ref(false)
const loadingWarranty = ref(false)
const submitting = ref(false)
const submittingBrand = ref(false)
const toastRef = ref(null)

// Bộ lọc
const filters = reactive({
  search: '',
  categoryID: '',
  status: '',
  departmentID: ''
})

// Modal Open States
const isDetailOpen = ref(false)
const isCreateOpen = ref(false)
const isEditOpen = ref(false)
const isAssignOpen = ref(false)
const isTransferOpen = ref(false)
const isReturnOpen = ref(false)
const isReportIssueOpen = ref(false)
const isImportOpen = ref(false)
const isHandoverImportOpen = ref(false)
const isPrintHandoverOpen = ref(false)
const isWarrantyModalOpen = ref(false)
const isBrandManagerOpen = ref(false)
const isHistoryModalOpen = ref(false)
const isQuickCatOpen = ref(false)
const isQuickSupOpen = ref(false)
const isQuickBrandOpen = ref(false)

const selectedAsset = ref(null)
const assetDetail = ref(null)

// Forms
const createForm = reactive({
  assetCode: '',
  assetName: '',
  categoryID: null,
  brand: '',
  serialNumber: '',
  materialCode: '',
  cpu: 'Core Ultra 5-125U',
  ram: '16 GB',
  disk: '512 GB SSD',
  os: 'Windows 11',
  display: '14-inch',
  charger: 'Kèm củ sạc + Dây nguồn zin',
  specifications: '',
  note: '',
  supplierID: null,
  warehouseLocation: 'Kho IT - Kệ A1',
  purchaseDate: '',
  warrantyExpireDate: '',
  quantity: 1
})

const editForm = reactive({
  assetID: null,
  assetCode: '',
  assetName: '',
  categoryID: null,
  brand: '',
  serialNumber: '',
  materialCode: '',
  cpu: '',
  ram: '16 GB',
  disk: '512 GB SSD',
  os: 'Windows 11',
  display: '14-inch',
  charger: '',
  specifications: '',
  note: '',
  supplierID: null,
  warehouseLocation: '',
  status: 'Available',
  holderName: '',
  purchaseDate: '',
  warrantyExpireDate: ''
})

const quickCatForm = reactive({ categoryCode: '', categoryName: '' })
const quickSupForm = reactive({ supplierCode: '', supplierName: '', phone: '' })
const quickBrandForm = reactive({ brandName: '', originCountry: '', description: '' })

// -------------------------------------------------------------
// HELPER FUNCTIONS & COMPUTED
// -------------------------------------------------------------
const isMouseAsset = (a) => {
  if (!a) return false
  const catName = (a.categoryName || '').toLowerCase()
  const name = (a.assetName || '').toLowerCase()
  const code = (a.assetCode || '').toLowerCase()
  return catName.includes('chuột') || catName.includes('mouse') || name.includes('chuột') || name.includes('mouse') || code.includes('mou') || code.includes('mouse')
}

const isKeyboardAsset = (a) => {
  if (!a) return false
  const catName = (a.categoryName || '').toLowerCase()
  const name = (a.assetName || '').toLowerCase()
  const code = (a.assetCode || '').toLowerCase()
  return catName.includes('bàn phím') || catName.includes('keyboard') || name.includes('bàn phím') || name.includes('keyboard') || code.includes('kb') || code.includes('key')
}

const isMonitorAsset = (a) => {
  if (!a) return false
  const catName = (a.categoryName || '').toLowerCase()
  const name = (a.assetName || '').toLowerCase()
  const code = (a.assetCode || '').toLowerCase()
  return catName.includes('màn hình') || catName.includes('monitor') || name.includes('màn hình') || name.includes('monitor') || code.includes('mn') || code.includes('mon')
}

// Phụ kiện sẵn có trong kho
const availableWarehouseMice = computed(() => {
  return assets.value.filter(a => a.status === 'Available' && isMouseAsset(a))
})

const availableWarehouseMonitors = computed(() => {
  return assets.value.filter(a => a.status === 'Available' && isMonitorAsset(a))
})

const availableWarehouseKeyboards = computed(() => {
  return assets.value.filter(a => a.status === 'Available' && isKeyboardAsset(a))
})

// Lọc danh sách thiết bị
const filteredAssets = computed(() => {
  return assets.value.filter(a => {
    if (filters.search) {
      const q = filters.search.toLowerCase().trim()
      const match = (a.assetCode && a.assetCode.toLowerCase().includes(q)) ||
                    (a.assetName && a.assetName.toLowerCase().includes(q)) ||
                    (a.serialNumber && a.serialNumber.toLowerCase().includes(q)) ||
                    (a.materialCode && a.materialCode.toLowerCase().includes(q)) ||
                    (a.holderName && a.holderName.toLowerCase().includes(q)) ||
                    (a.holderCode && a.holderCode.toLowerCase().includes(q)) ||
                    (a.specifications && a.specifications.toLowerCase().includes(q)) ||
                    (a.brand && a.brand.toLowerCase().includes(q))
      if (!match) return false
    }

    if (filters.categoryID && a.categoryID !== Number(filters.categoryID)) return false
    if (filters.status && a.status !== filters.status) return false
    if (filters.departmentID && a.departmentID !== Number(filters.departmentID)) return false

    return true
  })
})

// Thông tin lô chuột & phím hiện có khi thêm mới
const existingMouseLotInfo = computed(() => {
  if (!createForm.brand || !createForm.brand.trim()) return null
  const targetBrand = createForm.brand.trim().toLowerCase()
  const matched = assets.value.filter(a => isMouseAsset(a) && (a.brand || '').trim().toLowerCase() === targetBrand)
  if (matched.length === 0) return null

  let maxNum = 0
  const brandCode = (createForm.brand.trim() || 'GEN').replace(/[^a-zA-Z0-9]/g, '').toUpperCase().slice(0, 5)
  for (const item of matched) {
    const code = item.assetCode || ''
    const parts = code.split('-')
    const lastPart = parts[parts.length - 1]
    const num = parseInt(lastPart, 10)
    if (!isNaN(num) && num > maxNum) maxNum = num
  }

  const nextIndex = maxNum + 1
  return {
    brand: createForm.brand.trim(),
    currentCount: matched.length,
    availableCount: matched.filter(m => m.status === 'Available').length,
    inUseCount: matched.filter(m => m.status === 'In-Use').length,
    nextIndex,
    previewNextCode: `MOU-${brandCode}-${String(nextIndex).padStart(2, '0')}`
  }
})

const existingKeyboardLotInfo = computed(() => {
  if (!createForm.brand || !createForm.brand.trim()) return null
  const targetBrand = createForm.brand.trim().toLowerCase()
  const matched = assets.value.filter(a => isKeyboardAsset(a) && (a.brand || '').trim().toLowerCase() === targetBrand)
  if (matched.length === 0) return null

  let maxNum = 0
  const brandCode = (createForm.brand.trim() || 'GEN').replace(/[^a-zA-Z0-9]/g, '').toUpperCase().slice(0, 5)
  for (const item of matched) {
    const code = item.assetCode || ''
    const parts = code.split('-')
    const lastPart = parts[parts.length - 1]
    const num = parseInt(lastPart, 10)
    if (!isNaN(num) && num > maxNum) maxNum = num
  }

  const nextIndex = maxNum + 1
  return {
    brand: createForm.brand.trim(),
    currentCount: matched.length,
    availableCount: matched.filter(m => m.status === 'Available').length,
    inUseCount: matched.filter(m => m.status === 'In-Use').length,
    nextIndex,
    previewNextCode: `KB-${brandCode}-${String(nextIndex).padStart(2, '0')}`
  }
})

// -------------------------------------------------------------
// API CALLS & CRUD ACTIONS
// -------------------------------------------------------------
const calculateWarrantyAlerts = () => {
  const now = new Date()
  warrantyAlerts.value = assets.value
    .filter(a => a.warrantyExpireDate)
    .map(a => {
      const exp = new Date(a.warrantyExpireDate)
      const diffDays = Math.ceil((exp - now) / (1000 * 60 * 60 * 24))
      return {
        assetID: a.assetID,
        assetCode: a.assetCode,
        assetName: a.assetName,
        supplierName: a.supplierName,
        holderName: a.holderName,
        warrantyExpireDate: a.warrantyExpireDate,
        daysRemaining: diffDays,
        isExpired: diffDays < 0
      }
    })
    .filter(a => a.daysRemaining <= 30)
}

const fetchAssets = async () => {
  loading.value = true
  try {
    const res = await assetsApi.getAll()
    assets.value = res || []
    calculateWarrantyAlerts()
  } catch (err) {
    toastRef.value?.show('Lỗi tải danh sách thiết bị: ' + err.message, 'error')
  } finally {
    loading.value = false
  }
}

const fetchMetadata = async () => {
  try {
    const [cRes, dRes, eRes, sRes, bRes] = await Promise.all([
      categoriesApi.getAll(),
      departmentsApi.getAll(),
      employeesApi.getAll({ status: 'Active' }),
      suppliersApi.getAll(),
      brandsApi.getAll()
    ])
    categories.value = cRes || []
    departments.value = dRes || []
    activeEmployees.value = eRes || []
    suppliers.value = sRes || []
    brands.value = bRes || []

    if (categories.value.length > 0 && !createForm.categoryID) {
      createForm.categoryID = categories.value[0].categoryID
    }
  } catch (err) {
    console.error('Lỗi nạp metadata:', err)
  }
}

const openWarrantyModal = () => {
  calculateWarrantyAlerts()
  isWarrantyModalOpen.value = true
}

const openDetailModal = async (assetID) => {
  isDetailOpen.value = true
  loadingDetail.value = true
  try {
    const res = await assetsApi.getById(assetID)
    assetDetail.value = res
    selectedAsset.value = res
  } catch (err) {
    toastRef.value?.show('Lỗi tải chi tiết thiết bị: ' + err.message, 'error')
  } finally {
    loadingDetail.value = false
  }
}

const openCreateModal = () => {
  createForm.assetCode = ''
  createForm.assetName = ''
  createForm.categoryID = categories.value.length > 0 ? categories.value[0].categoryID : null
  createForm.brand = ''
  createForm.serialNumber = ''
  createForm.materialCode = ''
  createForm.cpu = 'Core Ultra 5-125U'
  createForm.ram = '16 GB'
  createForm.disk = '512 GB SSD'
  createForm.os = 'Windows 11'
  createForm.display = '14-inch'
  createForm.charger = 'Kèm củ sạc + Dây nguồn zin'
  createForm.specifications = ''
  createForm.note = ''
  createForm.supplierID = null
  createForm.warehouseLocation = 'Kho IT - Kệ A1'
  createForm.purchaseDate = ''
  createForm.warrantyExpireDate = ''
  createForm.quantity = 1
  isCreateOpen.value = true
}

// Chuyển tiếp hành động từ Detail Modal
const handleOpenAssignFromDetail = (asset) => {
  selectedAsset.value = asset
  isDetailOpen.value = false
  isAssignOpen.value = true
}

const handleOpenTransferFromDetail = (asset) => {
  selectedAsset.value = asset
  isDetailOpen.value = false
  isTransferOpen.value = true
}

const handleOpenReturnFromDetail = (asset) => {
  selectedAsset.value = asset
  isDetailOpen.value = false
  isReturnOpen.value = true
}

const handleOpenReportIssueFromDetail = (asset) => {
  selectedAsset.value = asset
  isDetailOpen.value = false
  isReportIssueOpen.value = true
}

const handleOpenPrintFromDetail = (asset) => {
  selectedAsset.value = asset
  isPrintHandoverOpen.value = true
}

const handleOpenEditFromDetail = (asset) => {
  editForm.assetID = asset.assetID
  editForm.assetCode = asset.assetCode
  editForm.assetName = asset.assetName
  editForm.categoryID = asset.categoryID
  editForm.brand = asset.brand || ''
  editForm.serialNumber = asset.serialNumber || ''
  editForm.materialCode = asset.materialCode || ''
  editForm.specifications = asset.specifications || ''
  editForm.note = asset.note || ''
  editForm.supplierID = asset.supplierID || null
  editForm.warehouseLocation = asset.warehouseLocation || ''
  editForm.status = asset.status || 'Available'
  editForm.holderName = asset.holderName || ''
  editForm.purchaseDate = asset.purchaseDate ? asset.purchaseDate.split('T')[0] : ''
  editForm.warrantyExpireDate = asset.warrantyExpireDate ? asset.warrantyExpireDate.split('T')[0] : ''

  // Parse specs nếu là máy tính
  if (asset.specifications) {
    const specsStr = asset.specifications
    const cpuM = specsStr.match(/CPU:\s*([^\|]+)/i)
    if (cpuM) editForm.cpu = cpuM[1].trim()
    const ramM = specsStr.match(/RAM:\s*([^\|]+)/i)
    if (ramM) editForm.ram = ramM[1].trim()
    const diskM = specsStr.match(/Disk:\s*([^\|]+)/i)
    if (diskM) editForm.disk = diskM[1].trim()
    const osM = specsStr.match(/OS:\s*([^\|]+)/i)
    if (osM) editForm.os = osM[1].trim()
    const dispM = specsStr.match(/Display:\s*([^\|]+)/i)
    if (dispM) editForm.display = dispM[1].trim()
  }

  isDetailOpen.value = false
  isEditOpen.value = true
}

const handleDeleteAsset = async (asset) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa thiết bị ${asset.assetCode} (${asset.assetName}) khỏi hệ thống?`)) return
  try {
    await assetsApi.delete(asset.assetID)
    toastRef.value?.show(`Đã xóa thiết bị ${asset.assetCode} thành công!`, 'success')
    isDetailOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi xóa thiết bị: ' + (err.response?.data?.message || err.message), 'error')
  }
}

// Submit Create Asset
const submitCreateAsset = async () => {
  submitting.value = true
  try {
    const cat = categories.value.find(c => c.categoryID === createForm.categoryID)
    const catNameLower = (cat?.categoryName || '').toLowerCase()
    const nameLower = (createForm.assetName || '').toLowerCase()

    const isMouse = catNameLower.includes('chuột') || catNameLower.includes('mouse') || nameLower.includes('chuột') || nameLower.includes('mouse')
    const isKb = catNameLower.includes('bàn phím') || catNameLower.includes('keyboard') || nameLower.includes('bàn phím') || nameLower.includes('keyboard')
    const isComp = catNameLower.includes('laptop') || catNameLower.includes('máy tính') || catNameLower.includes('pc') || catNameLower.includes('desktop')

    if (isMouse || isKb) {
      const createdList = await assetsApi.createAccessoryLot({
        categoryID: createForm.categoryID,
        brand: createForm.brand || 'GEN',
        assetName: createForm.assetName || null,
        quantity: Math.max(1, parseInt(createForm.quantity, 10) || 1),
        supplierID: createForm.supplierID || null,
        warehouseLocation: createForm.warehouseLocation || null,
        note: createForm.note || null
      })
      toastRef.value?.show(`Đã nhập thành công ${createdList.length} ${isKb ? 'bàn phím' : 'chuột'} ${createForm.brand || ''} vào kho!`, 'success')
    } else {
      const payload = {
        assetCode: createForm.assetCode.trim(),
        assetName: createForm.assetName.trim(),
        categoryID: createForm.categoryID,
        brand: createForm.brand?.trim() || null,
        serialNumber: createForm.serialNumber?.trim() || null,
        materialCode: createForm.materialCode?.trim() || null,
        specs: isComp ? {
          cpu: createForm.cpu,
          ram: createForm.ram,
          disk: createForm.disk,
          os: createForm.os,
          display: createForm.display,
          charger: createForm.charger
        } : null,
        supplierID: createForm.supplierID || null,
        warehouseLocation: createForm.warehouseLocation?.trim() || 'Kho IT - Kệ A1',
        purchaseDate: createForm.purchaseDate || null,
        warrantyExpireDate: createForm.warrantyExpireDate || null,
        status: 'Available',
        note: createForm.note?.trim() || null
      }
      const created = await assetsApi.create(payload)
      toastRef.value?.show(`Đã thêm mới thiết bị [${created.assetCode}] ${created.assetName} vào kho thành công!`, 'success')
    }

    // Tự động xóa bộ lọc tìm kiếm để người dùng thấy ngay thiết bị mới vừa thêm
    filters.search = ''
    filters.categoryID = ''
    filters.status = ''
    filters.departmentID = ''

    isCreateOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi thêm thiết bị: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Update Asset
const submitUpdateAsset = async () => {
  submitting.value = true
  try {
    const payload = {
      assetID: editForm.assetID,
      assetCode: editForm.assetCode.trim(),
      assetName: editForm.assetName.trim(),
      categoryID: editForm.categoryID,
      brand: editForm.brand?.trim() || null,
      serialNumber: editForm.serialNumber?.trim() || null,
      materialCode: editForm.materialCode?.trim() || null,
      specifications: editForm.specifications?.trim() || `CPU: ${editForm.cpu} | RAM: ${editForm.ram} | Disk: ${editForm.disk} | OS: ${editForm.os} | Display: ${editForm.display}`,
      note: editForm.note?.trim() || (editForm.charger ? `Charger: ${editForm.charger}` : null),
      supplierID: editForm.supplierID || null,
      warehouseLocation: editForm.warehouseLocation?.trim() || 'Kho IT',
      purchaseDate: editForm.purchaseDate || null,
      warrantyExpireDate: editForm.warrantyExpireDate || null,
      status: editForm.status
    }
    await assetsApi.update(editForm.assetID, payload)
    toastRef.value?.show(`Cập nhật thiết bị ${payload.assetCode} thành công!`, 'success')
    isEditOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi cập nhật thiết bị: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Assign (Cấp phát trọn gói ACID)
const submitAssign = async (formData) => {
  submitting.value = true
  try {
    await assetsApi.bundleAssign({
      mainAssetID: selectedAsset.value.assetID,
      toEmployeeID: formData.toEmployeeID,
      mouseAssetID: formData.mouseAssetID || null,
      monitorAssetID: formData.monitorAssetID || null,
      keyboardAssetID: formData.keyboardAssetID || null,
      conditionStatus: formData.conditionStatus || 'Hoạt động tốt',
      note: formData.note || null
    })

    toastRef.value?.show('Đã bàn giao thiết bị cho nhân viên thành công!', 'success')
    isAssignOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi cấp phát: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Transfer
const submitTransfer = async (formData) => {
  submitting.value = true
  try {
    await assetsApi.transfer(selectedAsset.value.assetID, {
      toEmployeeID: formData.toEmployeeID,
      conditionStatus: formData.conditionStatus,
      note: formData.note
    })
    toastRef.value?.show('Đã điều chuyển thiết bị thành công!', 'success')
    isTransferOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi điều chuyển: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Return
const submitReturn = async (formData) => {
  submitting.value = true
  try {
    await assetsApi.return(selectedAsset.value.assetID, {
      warehouseLocation: formData.warehouseLocation,
      isBroken: formData.isBroken,
      conditionStatus: formData.conditionStatus,
      note: formData.note
    })
    toastRef.value?.show('Đã thu hồi thiết bị về kho thành công!', 'success')
    isReturnOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi thu hồi: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Report Issue
const submitReportIssue = async (formData) => {
  submitting.value = true
  try {
    await assetsApi.reportIssue(selectedAsset.value.assetID, {
      issueDescription: formData.issueDescription,
      status: formData.newStatus || formData.status || 'Broken',
      newStatus: formData.newStatus,
      vendorName: formData.vendorName,
      estimatedCost: formData.estimatedCost ? Number(formData.estimatedCost) : null,
      expectedReturnDate: formData.expectedReturnDate || null,
      warehouseLocation: formData.warehouseLocation || null,
      note: formData.note || null
    })
    const isMaint = (formData.newStatus || formData.status) === 'Maintenance'
    toastRef.value?.show(isMaint ? 'Đã chuyển thiết bị sang trạng thái Bảo Trì thành công!' : 'Đã ghi nhận báo hỏng thiết bị thành công!', 'success')
    isReportIssueOpen.value = false
    await fetchAssets()
  } catch (err) {
    toastRef.value?.show('Lỗi báo sự cố: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Submit Import Assets
const submitImportAssets = async ({ mode, rows, callback }) => {
  submitting.value = true
  try {
    const itemsToCreate = []
    
    const matchCategory = (inputCatStr, assetCode = '', assetName = '') => {
      if (!categories.value || categories.value.length === 0) return 1
      const s = (inputCatStr || '').toLowerCase().trim()
      const code = (assetCode || '').toLowerCase().trim()
      const name = (assetName || '').toLowerCase().trim()

      // 1. Kiểm tra PC / Desktop
      if (
        s.includes('để bàn') || s.includes('de ban') || s.includes('desktop') || s.includes('pc') || s.includes('máy bàn') || s.includes('may ban') ||
        code.includes('dt') || code.includes('pc') || name.includes('optiplex') || name.includes('thinkcentre') || name.includes('prodesk') || name.includes('elitedesk')
      ) {
        const pcCat = categories.value.find(c => c.categoryCode === 'PC' || c.categoryName.toLowerCase().includes('để bàn') || c.categoryName.toLowerCase().includes('desktop'))
        if (pcCat) return pcCat.categoryID
      }

      // 2. Kiểm tra Laptop
      if (
        s.includes('laptop') || s.includes('xách tay') || s.includes('xach tay') || s.includes('notebook') || s.includes('macbook') ||
        code.includes('nb') || code.includes('lt') || name.includes('elitebook') || name.includes('latitude') || name.includes('thinkpad') || name.includes('vostro') || name.includes('inspiron')
      ) {
        const ltCat = categories.value.find(c => c.categoryCode === 'LAPTOP' || c.categoryName.toLowerCase().includes('laptop') || c.categoryName.toLowerCase().includes('xách tay'))
        if (ltCat) return ltCat.categoryID
      }

      // 3. Kiểm tra Màn hình
      if (s.includes('màn hình') || s.includes('man hinh') || s.includes('monitor') || code.includes('mn') || code.includes('mon')) {
        const monCat = categories.value.find(c => c.categoryCode === 'MONITOR' || c.categoryName.toLowerCase().includes('màn hình') || c.categoryName.toLowerCase().includes('monitor'))
        if (monCat) return monCat.categoryID
      }

      // 4. Kiểm tra Chuột
      if (s.includes('chuột') || s.includes('chuot') || s.includes('mouse') || code.includes('mou')) {
        const mouCat = categories.value.find(c => c.categoryCode === 'MOUSE' || c.categoryName.toLowerCase().includes('chuột'))
        if (mouCat) return mouCat.categoryID
      }

      // 5. Kiểm tra Bàn phím
      if (s.includes('bàn phím') || s.includes('ban phim') || s.includes('keyboard') || code.includes('kb')) {
        const kbCat = categories.value.find(c => c.categoryCode === 'KEYBOARD' || c.categoryName.toLowerCase().includes('bàn phím'))
        if (kbCat) return kbCat.categoryID
      }

      const directMatch = categories.value.find(c => c.categoryName.toLowerCase() === s || c.categoryCode.toLowerCase() === s)
      if (directMatch) return directMatch.categoryID

      const includesMatch = categories.value.find(c => c.categoryName.toLowerCase().includes(s) || s.includes(c.categoryName.toLowerCase()))
      if (includesMatch) return includesMatch.categoryID

      return categories.value[0]?.categoryID || 1
    }

    for (const row of rows) {
      if (mode === 'computer') {
        const assetCode = getRowValue(row, ['Computer Name', 'ComputerName', 'Host Name', 'Mã Máy', 'Mã Thiết Bị', 'Asset Code', 'Mã Tài Sản', 'Code'])
        const assetName = getRowValue(row, ['Model', 'Tên Thiết Bị', 'Tên Máy', 'Asset Name', 'Model Name', 'Name'])
        if (!assetCode && !assetName) continue

        let brand = getRowValue(row, ['Hãng', 'Thương Hiệu', 'Brand'])
        if (!brand && assetName) {
          const m = assetName.toLowerCase()
          if (m.includes('hp')) brand = 'HP'
          else if (m.includes('dell')) brand = 'Dell'
          else if (m.includes('lenovo')) brand = 'Lenovo'
          else if (m.includes('asus')) brand = 'ASUS'
          else if (m.includes('aoc')) brand = 'AOC'
          else if (m.includes('advantech')) brand = 'Advantech'
          else if (m.includes('ximagtek')) brand = 'Ximagtek'
        }

        const serialNumber = getRowValue(row, ['SN', 'Service Tag', 'Số Serial', 'Serial Number', 'S/N', 'Serial'])
        const materialCode = getRowValue(row, ['Asset Number', 'Mã Tài Sản', 'Mã Vật Tư'])
        const rawSpecs = getRowValue(row, ['Thông Số Kỹ Thuật', 'Thông số', 'Specifications', 'Cấu Hình', 'Đặc Điểm'])
        const cpu = getRowValue(row, ['CPU', 'Vi Xử Lý', 'Chip']) || 'Core Ultra'
        const ram = getRowValue(row, ['RAM', 'Bộ Nhớ']) || '16 GB'
        const disk = getRowValue(row, ['Disk', 'Ổ Cứng', 'SSD', 'HDD']) || '512 GB SSD'
        const os = getRowValue(row, ['OS', 'Hệ Điều Hành']) || 'Windows 11'
        const display = getRowValue(row, ['Display', 'Màn Hình']) || '14-inch'
        const charger = getRowValue(row, ['Remark', 'Sạc', 'Củ Sạc']) || 'Kèm củ sạc + Dây nguồn'
        const warehouseLocation = getRowValue(row, ['Vị Trí Kho', 'Vị Trí', 'Kho', 'Warehouse Location']) || 'Kho IT'
        const catStr = getRowValue(row, ['Loại Thiết Bị', 'Loại', 'Category', 'Loại máy'])
        const supplierName = getRowValue(row, ['Nhà Cung Cấp', 'Nhà cung cấp', 'Nha Cung Cap', 'Supplier', 'Supplier Name', 'NCC', 'Ncc'])
        const categoryID = matchCategory(catStr, assetCode, assetName)

        const purchaseDateStr = getRowValue(row, ['Ngày Mua', 'Purchase Date', 'Ngay Mua', 'Purchase'])
        const warrantyStr = getRowValue(row, ['Hạn Bảo Hành', 'Warranty Expire', 'Warranty', 'Han Bao Hanh'])

        itemsToCreate.push({
          assetCode: assetCode || `AST-${Date.now().toString().slice(-6)}`,
          assetName: assetName || 'Laptop Doanh Nghiệp',
          categoryName: catStr || null,
          supplierName: supplierName || null,
          categoryID,
          brand: brand || null,
          serialNumber: serialNumber || null,
          materialCode: materialCode || null,
          specifications: rawSpecs || `CPU: ${cpu} | RAM: ${ram} | Disk: ${disk} | OS: ${os} | Display: ${display}`,
          note: `Charger: ${charger}`,
          warehouseLocation,
          status: 'Available',
          purchaseDate: purchaseDateStr ? new Date(purchaseDateStr).toISOString() : null,
          warrantyExpireDate: warrantyStr ? new Date(warrantyStr).toISOString() : null
        })
      } else {
        const assetCode = getRowValue(row, ['Computer Name', 'ComputerName', 'Mã Thiết Bị', 'Mã Tài Sản', 'Asset Code', 'Host Name', 'Code'])
        const assetName = getRowValue(row, ['Tên Thiết Bị', 'Model', 'Tên Máy', 'Asset Name', 'Name'])
        if (!assetCode && !assetName) continue

        const brand = getRowValue(row, ['Thương Hiệu', 'Hãng', 'Brand'])
        const serialNumber = getRowValue(row, ['Số Serial', 'Serial Number', 'S/N', 'Serial', 'Service Tag'])
        const materialCode = getRowValue(row, ['Mã Vật Tư', 'Asset Number', 'Mã Tài Sản'])
        const specifications = getRowValue(row, ['Thông Số Kỹ Thuật', 'Thông số', 'Specifications', 'Cấu Hình', 'Đặc Điểm'])
        const note = getRowValue(row, ['Ghi Chú', 'Phụ Kiện', 'Remark', 'Tình Trạng'])
        const warehouseLocation = getRowValue(row, ['Vị Trí Kho', 'Vị Trí', 'Kho', 'Warehouse Location']) || 'Kho IT - Kệ Phụ Kiện'
        const catStr = getRowValue(row, ['Loại Thiết Bị', 'Loại', 'Category'])
        const supplierName = getRowValue(row, ['Nhà Cung Cấp', 'Nhà cung cấp', 'Nha Cung Cap', 'Supplier', 'Supplier Name', 'NCC', 'Ncc'])
        const categoryID = matchCategory(catStr, assetCode, assetName)

        const purchaseDateStr2 = getRowValue(row, ['Ngày Mua', 'Purchase Date', 'Ngay Mua', 'Purchase'])
        const warrantyStr2 = getRowValue(row, ['Hạn Bảo Hành', 'Warranty Expire', 'Warranty', 'Han Bao Hanh'])

        itemsToCreate.push({
          assetCode: assetCode || `AST-${Date.now().toString().slice(-6)}`,
          assetName: assetName || 'Thiết bị ngoại vi',
          categoryName: catStr || null,
          supplierName: supplierName || null,
          categoryID,
          brand: brand || null,
          serialNumber: serialNumber || null,
          materialCode: materialCode || null,
          specifications: specifications || null,
          note: note || null,
          warehouseLocation,
          status: 'Available',
          purchaseDate: purchaseDateStr2 ? new Date(purchaseDateStr2).toISOString() : null,
          warrantyExpireDate: warrantyStr2 ? new Date(warrantyStr2).toISOString() : null
        })
      }
    }

    if (itemsToCreate.length === 0) {
      toastRef.value?.show('Không có dòng dữ liệu hợp lệ nào để nạp!', 'error')
      return
    }

    await assetsApi.importBulk(itemsToCreate)
    toastRef.value?.show(`Đã nạp thành công ${itemsToCreate.length} thiết bị vào kho!`, 'success')
    isImportOpen.value = false
    if (callback) callback()
    
    // Tải lại danh sách thiết bị, danh mục loại thiết bị và nhà cung cấp
    await Promise.all([
      fetchAssets(),
      (async () => {
        try {
          const [cRes, sRes] = await Promise.all([
            categoriesApi.getAll(),
            suppliersApi.getAll()
          ])
          categories.value = cRes || []
          suppliers.value = sRes || []
        } catch (_) {}
      })()
    ])
  } catch (err) {
    toastRef.value?.show('Lỗi nạp danh sách thiết bị: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

// Handler khi Import Cấp Phát thành công
const handleHandoverImportSuccess = async (result) => {
  toastRef.value?.show(`Đã cấp phát thành công ${result.assignedCount} thiết bị, cập nhật ${result.spareCount} máy dự phòng!`, 'success')
  await fetchAssets()
}

// Brand CRUD Handlers
const handleSaveBrand = async ({ id, data, callback }) => {
  submittingBrand.value = true
  try {
    if (id) {
      await brandsApi.update(id, data)
      toastRef.value?.show(`Đã cập nhật hãng ${data.brandName} thành công!`, 'success')
    } else {
      await brandsApi.create(data)
      toastRef.value?.show(`Đã thêm hãng ${data.brandName} vào danh mục thành công!`, 'success')
    }
    if (callback) callback()
    const bRes = await brandsApi.getAll()
    brands.value = bRes || []
  } catch (err) {
    toastRef.value?.show('Lỗi lưu thông tin hãng: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submittingBrand.value = false
  }
}

const handleDeleteBrand = async (b) => {
  if (b.assetCount > 0) {
    alert(`Không thể xóa hãng "${b.brandName}" vì đang có ${b.assetCount} thiết bị trong hệ thống sử dụng thương hiệu này!`)
    return
  }
  if (!confirm(`Bạn có chắc chắn muốn xóa hãng "${b.brandName}" khỏi danh mục không?`)) return
  try {
    await brandsApi.delete(b.brandID)
    toastRef.value?.show(`Đã xóa hãng "${b.brandName}" khỏi danh mục!`, 'success')
    const bRes = await brandsApi.getAll()
    brands.value = bRes || []
  } catch (err) {
    toastRef.value?.show('Lỗi xóa hãng: ' + (err.response?.data?.message || err.message), 'error')
  }
}

// Quick Modals Handlers
const submitQuickCategory = async () => {
  submitting.value = true
  try {
    const res = await categoriesApi.create({ ...quickCatForm })
    toastRef.value?.show('Đã tạo loại thiết bị mới thành công!', 'success')
    isQuickCatOpen.value = false
    quickCatForm.categoryCode = ''
    quickCatForm.categoryName = ''
    const cRes = await categoriesApi.getAll()
    categories.value = cRes || []
    if (res?.categoryID) {
      createForm.categoryID = res.categoryID
      editForm.categoryID = res.categoryID
    }
  } catch (err) {
    toastRef.value?.show('Lỗi tạo loại: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

const submitQuickSupplier = async () => {
  submitting.value = true
  try {
    const res = await suppliersApi.create({ ...quickSupForm })
    toastRef.value?.show('Đã tạo nhà cung cấp mới thành công!', 'success')
    isQuickSupOpen.value = false
    quickSupForm.supplierCode = ''
    quickSupForm.supplierName = ''
    quickSupForm.phone = ''
    const sRes = await suppliersApi.getAll()
    suppliers.value = sRes || []
    if (res?.supplierID) {
      createForm.supplierID = res.supplierID
      editForm.supplierID = res.supplierID
    }
  } catch (err) {
    toastRef.value?.show('Lỗi tạo NCC: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submitting.value = false
  }
}

const submitQuickBrand = async () => {
  submittingBrand.value = true
  try {
    const res = await brandsApi.create({ ...quickBrandForm })
    toastRef.value?.show(`Đã thêm hãng "${quickBrandForm.brandName}" thành công!`, 'success')
    isQuickBrandOpen.value = false
    createForm.brand = quickBrandForm.brandName
    editForm.brand = quickBrandForm.brandName
    quickBrandForm.brandName = ''
    quickBrandForm.originCountry = ''
    quickBrandForm.description = ''
    const bRes = await brandsApi.getAll()
    brands.value = bRes || []
  } catch (err) {
    toastRef.value?.show('Lỗi tạo hãng: ' + (err.response?.data?.message || err.message), 'error')
  } finally {
    submittingBrand.value = false
  }
}

// Xuất CSV
const handleExportCsv = () => {
  if (filteredAssets.value.length === 0) {
    toastRef.value?.show('Không có dữ liệu thiết bị để xuất CSV!', 'warning')
    return
  }

  const headers = ['Mã Thiết Bị', 'Tên Model', 'Loại Thiết Bị', 'Hãng SX', 'Số Serial (S/N)', 'Mã Tài Sản / VT', 'Thông Số Kỹ Thuật', 'Phụ Kiện', 'Trạng Thái', 'Người Đang Giữ', 'Mã Nhân Viên', 'Phòng Ban', 'Vị Trí Kho', 'Nhà Cung Cấp', 'Ngày Mua', 'Hạn Bảo Hành']

  const rows = filteredAssets.value.map(a => [
    `"${(a.assetCode || '').replace(/"/g, '""')}"`,
    `"${(a.assetName || '').replace(/"/g, '""')}"`,
    `"${(a.categoryName || '').replace(/"/g, '""')}"`,
    `"${(a.brand || '').replace(/"/g, '""')}"`,
    `"${(a.serialNumber || '').replace(/"/g, '""')}"`,
    `"${(a.materialCode || '').replace(/"/g, '""')}"`,
    `"${(a.specifications || '').replace(/"/g, '""')}"`,
    `"${(a.note || '').replace(/"/g, '""')}"`,
    `"${(a.status || '').replace(/"/g, '""')}"`,
    `"${(a.holderName || '').replace(/"/g, '""')}"`,
    `"${(a.holderCode || '').replace(/"/g, '""')}"`,
    `"${(a.holderDepartment || '').replace(/"/g, '""')}"`,
    `"${(a.warehouseLocation || '').replace(/"/g, '""')}"`,
    `"${(a.supplierName || '').replace(/"/g, '""')}"`,
    `"${a.purchaseDate ? a.purchaseDate.split('T')[0] : ''}"`,
    `"${a.warrantyExpireDate ? a.warrantyExpireDate.split('T')[0] : ''}"`
  ])

  const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\r\n')
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', `Danh_Sach_Thiet_Bi_${new Date().toISOString().slice(0, 10)}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
  toastRef.value?.show(`Đã xuất ${filteredAssets.value.length} thiết bị ra file CSV thành công!`, 'success')
}

// -------------------------------------------------------------
// LIFECYCLE HOOKS
// -------------------------------------------------------------
onMounted(async () => {
  await Promise.all([fetchAssets(), fetchMetadata()])
})
</script>

<style scoped>
.assets-view {
  padding-bottom: 30px;
}

.view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.page-subtitle {
  font-size: 0.875rem;
  color: #64748b;
  margin: 4px 0 0 0;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-warning-outline {
  background: #fffbeb;
  border: 1px solid #fde68a;
  color: #b45309;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.btn-warning-outline:hover {
  background: #fef3c7;
}

.badge-alert-count {
  background: #ef4444;
  color: #ffffff;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 999px;
}
</style>
