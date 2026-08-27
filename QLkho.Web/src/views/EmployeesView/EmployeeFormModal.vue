<template>
  <Modal 
    :is-open="isOpen" 
    :title="isEdit ? $t('employees.modal_edit_title', { name: form.fullName }) : $t('employees.modal_create_title')"
    :subtitle="isEdit ? $t('employees.modal_edit_sub') : $t('employees.modal_create_sub')"
    @close="$emit('close')"
    max-width="700px"
  >
    <div v-if="isHR && isEdit && !isUpcomingJoin(form.joinDate)" class="hr-readonly-modal-banner">
      🔒 <strong>HR Access:</strong> Read-only mode for active working staff.
    </div>

    <form @submit.prevent="$emit('submit', form)">
      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_code') }} <span class="label-optional">(Optional)</span></label>
          <input 
            type="text" 
            class="form-control" 
            v-model="form.employeeCode" 
            placeholder="EMP001" 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_name') }} *</label>
          <input 
            type="text" 
            class="form-control" 
            v-model="form.fullName" 
            required 
            placeholder="Nguyen Van A" 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_english_name') }} <span class="label-optional">(Optional)</span></label>
          <input 
            type="text" 
            class="form-control" 
            v-model="form.englishName" 
            placeholder="David, Alex..." 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_department') }} *</label>
          <select 
            class="form-control" 
            v-model="form.departmentID" 
            required
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          >
            <option :value="null" disabled>{{ $t('employees.filter_all_depts') }}</option>
            <option v-for="d in departments" :key="d.departmentID" :value="d.departmentID">
              {{ d.departmentName }}
            </option>
          </select>
        </div>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_title') }}</label>
          <input 
            type="text" 
            class="form-control" 
            v-model="form.title" 
            placeholder="Software Engineer, Specialist..." 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_email') }}</label>
          <input 
            type="email" 
            class="form-control" 
            v-model="form.email" 
            placeholder="example@company.com" 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_phone') }}</label>
          <input 
            type="text" 
            class="form-control" 
            v-model="form.phone" 
            placeholder="0901234567" 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_join_date') }} *</label>
          <input 
            type="date" 
            class="form-control" 
            v-model="form.joinDate" 
            required 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
          <div v-if="isUpcomingJoin(form.joinDate)" class="onboarding-input-hint">
            🚀 <strong>{{ $t('employees.tab_onboarding') }}:</strong> {{ $t('employees.days_remaining_join', { days: getDaysUntilJoin(form.joinDate) }) }}
          </div>
        </div>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">{{ $t('employees.table_leave_date') }}</label>
          <input 
            type="date" 
            class="form-control" 
            v-model="form.leaveDate" 
            :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
          />
        </div>
      </div>

      <div v-if="isEdit" class="form-group">
        <label class="form-label">{{ $t('employees.table_status') }}</label>
        <select 
          class="form-control" 
          v-model="form.status" 
          :disabled="isHR && isEdit && !isUpcomingJoin(form.joinDate)"
        >
          <option value="Active">{{ $t('status.emp_active') }}</option>
          <option value="Resigned">{{ $t('status.emp_resigned') }}</option>
        </select>
      </div>

      <!-- Trạng thái 4 tài khoản hệ thống (Chỉ dành cho Admin / IT) -->
      <div v-if="isEdit && !isHR" class="emp-account-box-edit">
        <label class="form-label" style="color: #60a5fa; margin-bottom: 8px;">🔑 {{ $t('employees.table_accounts') }}</label>
        <div class="modal-grid-4">
          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label" style="font-size: 0.75rem;">QAD</label>
            <select class="form-control form-control-sm" v-model="form.qaD_Status">
              <option value="Available">Available</option>
              <option value="Disable">Disable</option>
              <option value="Deleted">Deleted</option>
            </select>
          </div>

          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label" style="font-size: 0.75rem;">OA</label>
            <select class="form-control form-control-sm" v-model="form.oA_Status">
              <option value="Available">Available</option>
              <option value="Disable">Disable</option>
              <option value="Deleted">Deleted</option>
            </select>
          </div>

          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label" style="font-size: 0.75rem;">Email</label>
            <select class="form-control form-control-sm" v-model="form.email_Status">
              <option value="Available">Available</option>
              <option value="Disable">Disable</option>
              <option value="Deleted">Deleted</option>
            </select>
          </div>

          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label" style="font-size: 0.75rem;">Active Directory</label>
            <select class="form-control form-control-sm" v-model="form.aD_Status">
              <option value="Available">Available</option>
              <option value="Disable">Disable</option>
              <option value="Deleted">Deleted</option>
            </select>
          </div>
        </div>
      </div>

      <div :class="isEdit ? 'modal-actions-between' : 'modal-actions-right'" style="margin-top: 20px;">
        <button 
          v-if="isEdit && (!isHR || isUpcomingJoin(form.joinDate))" 
          type="button" 
          class="btn btn-danger-outline" 
          @click="$emit('delete', form)"
        >
          🗑️ {{ $t('common.delete') }}
        </button>
        <span v-else-if="isEdit"></span>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="$emit('close')">
            {{ isHR && isEdit && !isUpcomingJoin(form.joinDate) ? $t('common.close') : $t('common.cancel') }}
          </button>
          <button 
            v-if="!isHR || !isEdit || isUpcomingJoin(form.joinDate)"
            type="submit" 
            class="btn btn-primary" 
            :disabled="submitting"
          >
            <span v-if="submitting" class="loading-spinner"></span>
            {{ isEdit ? $t('common.save') : $t('common.create') }}
          </button>
        </div>
      </div>
    </form>
  </Modal>
</template>

<script setup>
import { reactive, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  isEdit: { type: Boolean, default: false },
  employee: { type: Object, default: null },
  departments: { type: Array, default: () => [] },
  isHR: { type: Boolean, default: false },
  submitting: { type: Boolean, default: false }
})

defineEmits(['close', 'submit', 'delete'])

const form = reactive({
  employeeID: null,
  employeeCode: '',
  fullName: '',
  englishName: '',
  departmentID: null,
  title: '',
  email: '',
  phone: '',
  joinDate: '',
  leaveDate: '',
  status: 'Active',
  qaD_Status: 'Disable',
  oA_Status: 'Disable',
  email_Status: 'Disable',
  aD_Status: 'Disable'
})

const populateForm = () => {
  if (props.isOpen && props.employee && props.isEdit) {
    const emp = props.employee
    form.employeeID = emp.employeeID
    form.employeeCode = emp.employeeCode || ''
    form.fullName = emp.fullName || ''
    form.englishName = emp.englishName || ''
    form.departmentID = emp.departmentID || null
    form.title = emp.title || ''
    form.email = emp.email || ''
    form.phone = emp.phone || ''
    form.joinDate = emp.joinDate ? emp.joinDate.split('T')[0] : ''
    form.leaveDate = emp.leaveDate ? emp.leaveDate.split('T')[0] : ''
    form.status = emp.status || 'Active'
    form.qaD_Status = emp.qaD_Status || emp.qad_Status || emp.QAD_Status || 'Disable'
    form.oA_Status = emp.oA_Status || emp.oa_Status || emp.OA_Status || 'Disable'
    form.email_Status = emp.email_Status || emp.Email_Status || 'Disable'
    form.aD_Status = emp.aD_Status || emp.ad_Status || emp.AD_Status || 'Disable'
  } else if (props.isOpen && !props.isEdit) {
    form.employeeID = null
    form.employeeCode = ''
    form.fullName = ''
    form.englishName = ''
    form.departmentID = null
    form.title = ''
    form.email = ''
    form.phone = ''
    form.joinDate = new Date().toISOString().split('T')[0]
    form.leaveDate = ''
    form.status = 'Active'
    form.qaD_Status = 'Disable'
    form.oA_Status = 'Disable'
    form.email_Status = 'Disable'
    form.aD_Status = 'Disable'
  }
}

watch([() => props.isOpen, () => props.employee, () => props.isEdit], () => {
  populateForm()
}, { immediate: true, deep: true })

const getDaysUntilJoin = (joinDateStr) => {
  if (!joinDateStr) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const joinDate = new Date(joinDateStr)
  joinDate.setHours(0, 0, 0, 0)
  const diffTime = joinDate - today
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24))
}

const isUpcomingJoin = (joinDateStr) => {
  const days = getDaysUntilJoin(joinDateStr)
  return days !== null && days > 0
}
</script>

<style scoped>
.label-optional {
  font-size: 0.75rem;
  color: var(--text-dim);
  font-weight: normal;
  margin-left: 4px;
}

.field-hint {
  display: block;
  font-size: 0.725rem;
  color: #64748b;
  margin-top: 4px;
}

.hr-readonly-modal-banner {
  background: #fffbeb;
  color: #b45309;
  border: 1px solid #fde68a;
  padding: 10px 14px;
  border-radius: var(--radius-md);
  margin-bottom: 16px;
  font-size: 0.85rem;
}
</style>
