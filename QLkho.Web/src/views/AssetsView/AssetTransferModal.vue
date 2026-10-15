<template>
  <Modal 
    :is-open="isOpen" 
    title="Điều Chuyển Thiết Bị Trực Tiếp"
    :subtitle="`Chuyển giao máy ${asset?.assetCode} từ ${asset?.holderName} sang nhân sự khác`"
    @close="$emit('close')"
  >
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label class="form-label">Người đang sử dụng hiện tại</label>
        <input type="text" class="form-control" :value="asset?.holderName || '---'" disabled />
      </div>

      <div class="form-group">
        <label class="form-label">Chọn Nhân Viên Tiếp Nhận Thiết Bị *</label>
        <EmployeeSelector 
          v-model="form.toEmployeeID" 
          :employees="availableEmployees" 
          :departments="departments" 
        />
      </div>

      <div class="form-group" v-if="form.toEmployeeID">
        <label class="form-label">Tài khoản máy tính (AD Account) của người nhận</label>
        <input 
          type="text" 
          class="form-control" 
          style="font-family: monospace; font-weight: 600;" 
          v-model="form.newAccount" 
          placeholder="VD: AP\Bob.Liu" 
        />
        <small class="text-muted" style="font-size: 0.75rem; margin-top: 3px; display: block;">
          💡 Hệ thống tự động gợi ý theo email nhân sự và cập nhật thông tin máy tính
        </small>
      </div>

      <div class="form-group">
        <label class="form-label">Tình trạng thực tế lúc điều chuyển</label>
        <input type="text" class="form-control" v-model="form.conditionStatus" placeholder="Vd: Máy hoạt động bình thường..." />
      </div>

      <div class="form-group">
        <label class="form-label">Lý do điều chuyển / Ghi chú</label>
        <textarea class="form-control" rows="2" v-model="form.note" placeholder="Vd: Bàn giao lại do đổi vị trí công việc..."></textarea>
      </div>

      <div class="modal-actions-right">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button type="submit" class="btn btn-primary" :disabled="!form.toEmployeeID || submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          Xác Nhận Điều Chuyển
        </button>
      </div>
    </form>
  </Modal>
</template>

<script setup>
import { reactive, computed, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'
import EmployeeSelector from '@/components/common/EmployeeSelector.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  asset: {
    type: Object,
    default: null
  },
  employees: {
    type: Array,
    default: () => []
  },
  departments: {
    type: Array,
    default: () => []
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'submit'])

const form = reactive({
  toEmployeeID: null,
  newAccount: '',
  conditionStatus: 'Máy hoạt động tốt',
  note: ''
})

watch(() => props.isOpen, (open) => {
  if (open) {
    form.toEmployeeID = null
    form.newAccount = ''
    form.conditionStatus = 'Máy hoạt động tốt'
    form.note = ''
  }
})

watch(() => form.toEmployeeID, (newEmpId) => {
  if (newEmpId) {
    const match = props.employees.find(e => e.employeeID === newEmpId)
    if (match) {
      if (match.email) {
        const prefix = match.email.split('@')[0].trim()
        form.newAccount = `AP\\${prefix}`
      } else if (match.englishName) {
        form.newAccount = `AP\\${match.englishName.trim()}`
      } else if (match.employeeCode) {
        form.newAccount = `AP\\${match.employeeCode.trim()}`
      }
    }
  } else {
    form.newAccount = ''
  }
})

const availableEmployees = computed(() => {
  if (!props.asset) return props.employees
  const deptId = props.asset.orderDepartmentID
  return props.employees.filter(e =>
    e.employeeID !== props.asset.currentHolderID && (!deptId || e.departmentID === deptId)
  )
})

const handleSubmit = () => {
  if (!form.toEmployeeID) return
  emit('submit', { ...form })
}
</script>
