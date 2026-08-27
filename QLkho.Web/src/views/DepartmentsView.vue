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

      <button class="btn btn-primary" @click="isCreateOpen = true">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="5" x2="12" y2="19"></line>
          <line x1="5" y1="12" x2="19" y2="12"></line>
        </svg>
        {{ $t('departments.btn_add') }}
      </button>
    </div>

    <!-- Departments Cards Grid -->
    <div v-if="loading" class="table-loading">
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
          </div>

          <div class="meta-box">
            <span class="meta-label">{{ $t('departments.table_employee_count') }}:</span>
            <strong class="meta-value" style="color: #818cf8;">{{ dept.employeeCount }} {{ $t('departments.unit_members') }}</strong>
          </div>

          <div class="meta-box" style="grid-column: span 2;">
            <span class="meta-label">{{ $t('departments.table_asset_count') }}:</span>
            <strong class="meta-value" style="color: #34d399;">{{ dept.assignedAssetCount }} {{ $t('departments.unit_assets') }}</strong>
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
          <input type="text" class="form-control" v-model="createForm.managerName" placeholder="Nguyen Van A, John Doe..." />
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
          <input type="text" class="form-control" v-model="editForm.managerName" placeholder="Nguyen Van A..." />
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

    <!-- Toast Notification -->
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { departmentsApi } from '@/api/client'
import Modal from '@/components/common/Modal.vue'
import Toast from '@/components/common/Toast.vue'

const departments = ref([])
const loading = ref(false)
const submitting = ref(false)
const isCreateOpen = ref(false)
const isEditOpen = ref(false)

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
  editForm.managerName = dept.managerName || ''
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

onMounted(() => {
  fetchDepartments()
})
</script>

<style scoped>
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
</style>
