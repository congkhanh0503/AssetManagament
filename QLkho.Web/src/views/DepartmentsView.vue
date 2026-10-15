<template>
  <div class="departments-page">
    <div class="page-topbar">
      <div>
        <h2 class="page-title">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1;">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
            <polyline points="9 22 9 12 15 12 15 22"></polyline>
          </svg>
          {{ $t('departments.title') }}
        </h2>
        <p class="page-subtitle">{{ $t('departments.subtitle') }}</p>
      </div>

      <button v-if="activeTab === 'departments'" class="btn btn-primary" @click="isCreateOpen = true">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="5" x2="12" y2="19"></line>
          <line x1="5" y1="12" x2="19" y2="12"></line>
        </svg>
        {{ $t('departments.btn_add') }}
      </button>
    </div>

    <div class="dept-tabs">
      <button type="button" class="dept-tab" :class="{ active: activeTab === 'departments' }" @click="activeTab = 'departments'">
        {{ $t('departments.tab_departments') }}
      </button>
      <button type="button" class="dept-tab" :class="{ active: activeTab === 'managers' }" @click="activeTab = 'managers'">
        {{ $t('departments.tab_managers') }} ({{ managers.length }})
      </button>
    </div>

    <!-- Tab Manager -->
    <div v-if="activeTab === 'managers'" class="glass-card manager-tab">
      <form class="manager-add-row" @submit.prevent="submitAddManager">
        <select class="form-control" v-model="newManagerEmployeeId" required>
          <option value="">{{ $t('departments.manager_pick_employee') }}</option>
          <option v-for="e in employeesForManager" :key="e.employeeID" :value="e.employeeID">
            {{ e.fullName }}{{ e.email ? ' / ' + e.email : '' }}{{ e.title ? ' - ' + e.title : '' }}
          </option>
        </select>
        <button type="submit" class="btn btn-primary" :disabled="submitting || !newManagerEmployeeId">
          {{ $t('departments.manager_add') }}
        </button>
      </form>

      <div v-if="managers.length === 0" class="field-hint" style="padding: 16px 0;">{{ $t('departments.manager_empty') }}</div>
      <table v-else class="dept-sub-table">
        <thead>
          <tr>
            <th>{{ $t('departments.manager_col_name') }}</th>
            <th>Email</th>
            <th style="width: 80px;"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in managers" :key="m.managerID">
            <td><strong>{{ m.fullName }}</strong></td>
            <td>{{ m.email || '---' }}</td>
            <td style="text-align: right;">
              <button type="button" class="btn-icon-mini btn-delete-icon" @click="deleteManager(m)" :title="$t('departments.delete_tooltip')">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Departments Cards Grid -->
    <div v-else-if="loading" class="table-loading">
      <span class="loading-spinner"></span>
      <p>{{ $t('common.loading') }}</p>
    </div>

    <div v-else class="dept-cards-grid">
      <div v-for="dept in departments" :key="dept.departmentID" class="glass-card dept-card">
        <div class="dept-card-top">
          <span class="dept-code-tag">{{ dept.departmentCode }}</span>
          <div class="dept-actions-top">
            <button class="btn-icon-mini" @click="openEditDeptModal(dept)" :title="$t('departments.edit_tooltip')">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
              </svg>
            </button>
            <button class="btn-icon-mini btn-delete-icon" @click="deleteDeptItem(dept)" :title="$t('departments.delete_tooltip')">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"></polyline>
                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
              </svg>
            </button>
          </div>
        </div>

        <h3 class="dept-title">{{ dept.departmentName }}</h3>
        <p class="dept-desc">{{ dept.description || $t('departments.no_desc') }}</p>

        <div class="dept-meta-grid">
          <div class="meta-box">
            <span class="meta-label">{{ $t('departments.table_manager') }}:</span>
            <strong class="meta-value">{{ dept.managerName || $t('departments.unassigned') }}</strong>
            <template v-if="dept.subManagers && dept.subManagers.length">
              <span class="meta-label" style="display: block; margin-top: 6px;">{{ $t('departments.sub_managers') }}:</span>
              <strong v-for="m in dept.subManagers" :key="m" class="meta-value" style="display: block;">{{ m }}</strong>
            </template>
          </div>

          <div class="meta-box meta-box-clickable" @click="goToEmployeesPage(dept)" title="Nhấn để xem danh sách nhân sự bộ phận này">
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <span class="meta-label">{{ $t('departments.table_employee_count') }}:</span>
              <span class="meta-hint-icon">➔</span>
            </div>
            <strong class="meta-value" style="color: #6366f1;">{{ dept.employeeCount }} {{ $t('departments.unit_members') }}</strong>
          </div>

          <div class="meta-box meta-box-clickable" style="grid-column: span 2;" @click="openDeptAssetsModal(dept)" title="Nhấn để xem danh sách thiết bị đang cấp phát của bộ phận này">
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <span class="meta-label">{{ $t('departments.table_asset_count') }}:</span>
              <span class="meta-hint-action">👁️ Xem chi tiết ➔</span>
            </div>
            <strong class="meta-value" style="color: #059669; font-size: 0.95rem;">{{ dept.assignedAssetCount || 0 }} {{ $t('departments.unit_assets') }}</strong>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Thêm Bộ Phận -->
    <Modal 
      :is-open="isCreateOpen" 
      :title="$t('departments.modal_add_title')"
      :subtitle="$t('departments.modal_add_sub')"
      @close="isCreateOpen = false"
    >
      <form @submit.prevent="submitCreateDept">
        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_code') }} *</label>
          <input type="text" class="form-control" v-model="createForm.departmentCode" required placeholder="MARKETING, QC, IT..." />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_name') }} *</label>
          <input type="text" class="form-control" v-model="createForm.departmentName" required placeholder="Marketing & Communications" />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_manager') }}</label>
          <select class="form-control" v-model="createForm.managerName">
            <option value="">{{ $t('departments.unassigned') }}</option>
            <option v-for="name in mainManagerOptions(createForm.managerName)" :key="name" :value="name">{{ name }}</option>
          </select>
          <span class="field-hint">Chọn từ danh sách ở tab Manager.</span>
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.desc_label') }}</label>
          <textarea class="form-control" rows="3" v-model="createForm.description" placeholder="..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isCreateOpen = false">{{ $t('common.cancel') }}</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            {{ $t('departments.save_dept') }}
          </button>
        </div>
      </form>
    </Modal>

    <!-- Modal Sửa Bộ Phận -->
    <Modal 
      :is-open="isEditOpen" 
      :title="$t('departments.modal_edit_title', { name: editForm.departmentName })"
      :subtitle="$t('departments.modal_edit_sub')"
      @close="isEditOpen = false"
    >
      <form @submit.prevent="submitUpdateDept">
        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_code') }} *</label>
          <input type="text" class="form-control" v-model="editForm.departmentCode" required />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_name') }} *</label>
          <input type="text" class="form-control" v-model="editForm.departmentName" required />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.table_manager') }}</label>
          <select class="form-control" v-model="editForm.managerName">
            <option value="">{{ $t('departments.unassigned') }}</option>
            <option v-for="name in mainManagerOptions(editForm.managerName)" :key="name" :value="name">{{ name }}</option>
          </select>
          <span class="field-hint">Chọn từ danh sách ở tab Manager. Đổi trưởng bộ phận sẽ cập nhật manager cho toàn bộ nhân sự phòng ban.</span>
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.sub_managers') }}</label>
          <div v-if="subManagerChoices.length === 0" class="field-hint">Chưa có nhân sự khác trong phòng ban để chọn.</div>
          <div v-else class="sub-manager-list">
            <label v-for="name in subManagerChoices" :key="name" class="sub-manager-item">
              <input type="checkbox" :value="name" v-model="editForm.subManagers" />
              <span>{{ name }}</span>
            </label>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('departments.desc_label') }}</label>
          <textarea class="form-control" rows="3" v-model="editForm.description"></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isEditOpen = false">{{ $t('common.cancel') }}</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            {{ $t('departments.update_dept') }}
          </button>
        </div>
      </form>
    </Modal>

    <!-- Modal Xem Danh Sách Thiết Bị Đang Cấp Phát Của Phòng Ban -->
    <Modal 
      :is-open="isDeptAssetsModalOpen" 
      :title="'Thiết Bị Của Bộ Phận: ' + (activeDept?.departmentName || '')"
      :subtitle="'Đang cấp phát ' + (deptAssetsList.length) + ' thiết bị cho nhân sự thuộc bộ phận này'"
      @close="isDeptAssetsModalOpen = false"
    >
      <div v-if="loadingDeptAssets" class="table-loading" style="padding: 30px;">
        <span class="loading-spinner"></span>
        <p>Đang tải danh sách thiết bị...</p>
      </div>

      <div v-else-if="deptAssetsList.length === 0" class="empty-state" style="padding: 28px; text-align: center;">
        <div style="font-size: 2.5rem; margin-bottom: 8px;">📦</div>
        <p style="color: #64748b; font-weight: 500; font-size: 0.9rem;">Bộ phận này hiện chưa có thiết bị nào được cấp phát trong hệ thống.</p>
      </div>

      <div v-else class="dept-assets-container">
        <div class="table-responsive" style="max-height: 400px; overflow-y: auto;">
          <table class="dept-sub-table">
            <thead>
              <tr>
                <th style="width: 120px;">MÃ TÀI SẢN</th>
                <th>TÊN THIẾT BỊ</th>
                <th style="width: 140px;">SERIAL NUMBER</th>
                <th style="width: 170px;">NGƯỜI ĐANG GIỮ</th>
                <th style="width: 110px; text-align: center;">TRẠNG THÁI</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="asset in deptAssetsList" :key="asset.assetID">
                <td>
                  <strong class="code-tag-blue">{{ asset.assetCode }}</strong>
                </td>
                <td>
                  <div style="font-weight: 600; color: #1e293b;">{{ asset.assetName }}</div>
                  <div style="font-size: 0.75rem; color: #64748b;">{{ asset.categoryName || asset.brand || '' }}</div>
                </td>
                <td>
                  <span style="font-family: monospace; font-size: 0.8rem; color: #475569;">
                    {{ asset.serialNumber || '---' }}
                  </span>
                </td>
                <td>
                  <div v-if="asset.holderName" style="color: #1e40af; font-weight: 600; font-size: 0.825rem;">
                    👤 {{ asset.holderName }}
                    <span v-if="asset.holderCode" style="color: #64748b; font-size: 0.725rem; display: block;">({{ asset.holderCode }})</span>
                  </div>
                  <span v-else style="color: #94a3b8; font-size: 0.8rem;">---</span>
                </td>
                <td style="text-align: center;">
                  <span class="status-pill-green">
                    {{ asset.status === 'In-Use' ? 'Đang cấp phát' : asset.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="modal-actions-right" style="margin-top: 20px; display: flex; justify-content: space-between; align-items: center;">
        <button 
          v-if="activeDept" 
          type="button" 
          class="btn btn-secondary" 
          @click="goToAssetsPage(activeDept)"
          style="display: inline-flex; align-items: center; gap: 6px; font-size: 0.825rem;"
        >
          <span>🔍 Mở bộ lọc trong Kho Tài Sản ➔</span>
        </button>
        <button type="button" class="btn btn-primary" @click="isDeptAssetsModalOpen = false">
          {{ $t('common.close') || 'Đóng' }}
        </button>
      </div>
    </Modal>

    <!-- Toast Notification -->
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { departmentsApi, assetsApi, employeesApi, managersApi } from '@/api/client'
import Modal from '@/components/common/Modal.vue'
import Toast from '@/components/common/Toast.vue'

const router = useRouter()

const departments = ref([])
const employees = ref([])

// Chỉ nhân sự đang làm việc thuộc đúng phòng ban; giữ lại tên hiện tại dù không còn trong danh sách
// Giá trị lưu dạng "Tên/email" (vd: Bob Liu/Bob.Liu@amphenol-cbs.com)
const managerLabel = (e) => (e.email ? `${e.fullName}/${e.email}` : e.fullName)

const managerOptions = (deptId, current) => {
  const names = deptId
    ? employees.value
        .filter(e => e.departmentID === deptId && e.status !== 'Resigned' && !(e.leaveDate && new Date(e.leaveDate) <= new Date()))
        .filter(e => e.fullName)
        .map(managerLabel)
    : []
  const list = [...new Set(names)].sort((a, b) => a.localeCompare(b, 'vi'))
  return current && !list.includes(current) ? [current, ...list] : list
}

// Trưởng bộ phận: chọn từ danh sách Manager (không phụ thuộc phòng ban); giữ lại giá trị hiện tại dù không còn trong danh sách
const mainManagerOptions = (current) => {
  const list = [...new Set(managers.value.map(managerLabel))].sort((a, b) => a.localeCompare(b, 'vi'))
  return current && !list.includes(current) ? [current, ...list] : list
}

// Dữ liệu cũ chỉ lưu tên -> đổi sang "Tên/email" nếu tìm thấy nhân sự trùng tên trong phòng ban
const normalizeManager = (deptId, current) => {
  if (!current || current.includes('/')) return current || ''
  const emp = employees.value.find(e => e.departmentID === deptId && e.fullName === current)
  return emp ? managerLabel(emp) : current
}

// Manager nhỏ: nhân sự trong phòng ban, trừ manager chính; giữ lại các lựa chọn đã lưu
const subManagerChoices = computed(() => {
  const opts = managerOptions(editForm.departmentID, '').filter(n => n !== editForm.managerName)
  return [...new Set([...editForm.subManagers, ...opts])].filter(n => n !== editForm.managerName)
})

const activeTab = ref('departments')
const managers = ref([])
const newManagerEmployeeId = ref('')

const employeesForManager = computed(() => {
  const taken = new Set(managers.value.map(m => m.employeeID))
  return employees.value
    .filter(e => e.fullName && !taken.has(e.employeeID) && e.status !== 'Resigned')
    .sort((a, b) => a.fullName.localeCompare(b.fullName, 'vi'))
})

const fetchManagers = async () => {
  try {
    managers.value = (await managersApi.getAll()) || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải manager', err.message, 'error')
  }
}

const submitAddManager = async () => {
  submitting.value = true
  try {
    await managersApi.create({ employeeID: newManagerEmployeeId.value })
    newManagerEmployeeId.value = ''
    await fetchManagers()
    toastRef.value?.addToast('Thành công', 'Đã thêm manager!', 'success')
  } catch (err) {
    toastRef.value?.addToast('Lỗi thêm manager', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const deleteManager = async (m) => {
  if (!confirm(`Xóa "${m.fullName}" khỏi danh sách Manager? Các phòng ban đang dùng người này vẫn giữ nguyên.`)) return
  try {
    await managersApi.delete(m.managerID)
    await fetchManagers()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa manager', err.message, 'error')
  }
}

const fetchEmployees = async () => {
  try {
    const res = await employeesApi.getAll()
    employees.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải nhân sự', err.message, 'error')
  }
}
const loading = ref(false)
const submitting = ref(false)
const isCreateOpen = ref(false)
const isEditOpen = ref(false)

// State xem danh sách thiết bị của phòng ban
const isDeptAssetsModalOpen = ref(false)
const loadingDeptAssets = ref(false)
const activeDept = ref(null)
const deptAssetsList = ref([])

const toastRef = ref(null)

const createForm = reactive({
  departmentCode: '',
  departmentName: '',
  managerName: '',
  description: ''
})

const editForm = reactive({
  departmentID: null,
  departmentCode: '',
  departmentName: '',
  managerName: '',
  subManagers: [],
  description: ''
})

const fetchDepartments = async () => {
  loading.value = true
  try {
    const res = await departmentsApi.getAll()
    departments.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải phòng ban', err.message, 'error')
  } finally {
    loading.value = false
  }
}

const submitCreateDept = async () => {
  submitting.value = true
  try {
    await departmentsApi.create(createForm)
    toastRef.value?.addToast('Thành công', 'Đã thêm bộ phận mới!', 'success')
    isCreateOpen.value = false
    createForm.departmentCode = ''
    createForm.departmentName = ''
    createForm.managerName = ''
    createForm.description = ''
    fetchDepartments()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tạo bộ phận', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const openEditDeptModal = (dept) => {
  editForm.departmentID = dept.departmentID
  editForm.departmentCode = dept.departmentCode
  editForm.departmentName = dept.departmentName
  editForm.managerName = normalizeManager(dept.departmentID, dept.managerName)
  editForm.subManagers = (dept.subManagers || []).map(m => normalizeManager(dept.departmentID, m))
  editForm.description = dept.description || ''
  isEditOpen.value = true
}

const submitUpdateDept = async () => {
  submitting.value = true
  try {
    await departmentsApi.update(editForm.departmentID, editForm)
    toastRef.value?.addToast('Thành công', 'Đã cập nhật bộ phận!', 'success')
    isEditOpen.value = false
    fetchDepartments()
    fetchEmployees()
  } catch (err) {
    toastRef.value?.addToast('Lỗi cập nhật', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const deleteDeptItem = async (dept) => {
  if (dept.employeeCount > 0) {
    toastRef.value?.addToast('Cảnh báo', `Không thể xóa bộ phận "${dept.departmentName}" đang có ${dept.employeeCount} nhân viên!`, 'error')
    return
  }
  if (!confirm(`Bạn có chắc chắn muốn xóa bộ phận "${dept.departmentName} (${dept.departmentCode})" không?`)) {
    return
  }
  try {
    await departmentsApi.delete(dept.departmentID)
    toastRef.value?.addToast('Thành công', `Đã xóa bộ phận ${dept.departmentName}!`, 'success')
    fetchDepartments()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa bộ phận', err.message, 'error')
  }
}

// Mở modal xem danh sách thiết bị của bộ phận
const openDeptAssetsModal = async (dept) => {
  activeDept.value = dept
  isDeptAssetsModalOpen.value = true
  loadingDeptAssets.value = true
  deptAssetsList.value = []
  try {
    const res = await assetsApi.getAll({ departmentID: dept.departmentID })
    deptAssetsList.value = Array.isArray(res) ? res : []
  } catch (err) {
    toastRef.value?.addToast('Lỗi', 'Không thể nạp danh sách thiết bị: ' + err.message, 'error')
  } finally {
    loadingDeptAssets.value = false
  }
}

// Chuyển sang trang Kho Tài Sản có lọc sẵn theo bộ phận
const goToAssetsPage = (dept) => {
  isDeptAssetsModalOpen.value = false
  router.push({ path: '/assets', query: { departmentID: dept.departmentID } })
}

// Chuyển sang trang Nhân Sự có lọc sẵn theo bộ phận
const goToEmployeesPage = (dept) => {
  router.push({ path: '/employees', query: { departmentId: dept.departmentID } })
}

onMounted(() => {
  fetchDepartments()
  fetchEmployees()
  fetchManagers()
})
</script>

<style scoped>
.dept-tabs {
  display: flex;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
}

.dept-tab {
  padding: 8px 16px;
  border: none;
  background: none;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  border-bottom: 2px solid transparent;
}

.dept-tab.active {
  color: #6366f1;
  border-bottom-color: #6366f1;
}

.manager-tab {
  padding: 16px;
}

.manager-add-row {
  display: flex;
  gap: 10px;
  margin-bottom: 14px;
}

.manager-add-row select {
  flex: 1;
}

.sub-manager-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 180px;
  overflow-y: auto;
  padding: 8px 10px;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
}

.sub-manager-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.875rem;
  cursor: pointer;
}

.departments-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.dept-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.dept-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.dept-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.dept-code-tag {
  font-family: monospace;
  font-size: 0.8rem;
  font-weight: 800;
  color: #818cf8;
  background: rgba(99, 102, 241, 0.15);
  padding: 3px 10px;
  border-radius: 4px;
  letter-spacing: 0.05em;
}

.status-dot-active {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--success);
  box-shadow: 0 0 6px rgba(5, 150, 105, 0.4);
}

.dept-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: #0f172a;
}

.dept-desc {
  font-size: 0.825rem;
  color: var(--text-dim);
  margin-top: 4px;
  line-height: 1.4;
  min-height: 38px;
}

.dept-meta-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-top: 16px;
  padding-top: 14px;
  border-top: 1px solid var(--border-color);
}

.meta-box {
  background: #f8fafc;
  border: 1px solid var(--border-color);
  padding: 10px;
  border-radius: var(--radius-sm);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.meta-label {
  font-size: 0.725rem;
  color: var(--text-dim);
  text-transform: uppercase;
  font-weight: 600;
}

.meta-value {
  font-size: 0.875rem;
  color: #0f172a;
  font-weight: 700;
}

.modal-actions-right {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.dept-actions-top {
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-icon-mini {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-dim);
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: var(--transition);
}

.btn-icon-mini:hover {
  color: var(--primary);
  border-color: var(--primary);
  background: #eef2ff;
}

.btn-icon-mini.btn-delete-icon:hover {
  color: #dc2626;
  border-color: #fca5a5;
  background: #fef2f2;
}

/* Interactivity cho Meta Box */
.meta-box-clickable {
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}

.meta-box-clickable:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
  transform: translateY(-1px);
  box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
}

.meta-hint-action {
  font-size: 0.72rem;
  color: #059669;
  font-weight: 700;
  opacity: 0.85;
}

.meta-box-clickable:hover .meta-hint-action {
  opacity: 1;
  text-decoration: underline;
}

.meta-hint-icon {
  font-size: 0.72rem;
  color: #818cf8;
  font-weight: 700;
}

/* Bảng thiết bị trong Modal */
.dept-sub-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.825rem;
}

.dept-sub-table th {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  padding: 10px 12px;
  border-bottom: 2px solid #e2e8f0;
  text-align: left;
}

.dept-sub-table td {
  padding: 11px 12px;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.dept-sub-table tr:hover td {
  background: #f8fafc;
}

.code-tag-blue {
  font-family: monospace;
  font-size: 0.8rem;
  color: #0284c7;
  background: #e0f2fe;
  padding: 2px 7px;
  border-radius: 5px;
  border: 1px solid #bae6fd;
  display: inline-block;
}

.status-pill-green {
  background: #dcfce7;
  color: #059669;
  border: 1px solid #a7f3d0;
  padding: 3px 9px;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 700;
  display: inline-block;
}
</style>
