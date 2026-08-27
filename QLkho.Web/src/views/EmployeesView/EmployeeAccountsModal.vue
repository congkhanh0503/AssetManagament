<template>
  <Modal 
    :is-open="isOpen" 
    :title="$t('employees.modal_accounts_title')"
    :subtitle="$t('employees.modal_accounts_sub', { name: employee?.fullName, code: employee?.employeeCode })"
    @close="$emit('close')"
  >
    <form @submit.prevent="$emit('submit', form)">
      <div class="account-control-list">
        <!-- QAD ERP -->
        <div class="account-row">
          <div class="account-info">
            <span class="account-title">1. ERP / QAD</span>
            <span class="account-sub">Enterprise Resource Planning & Production ERP</span>
          </div>
          <select class="form-control status-select" v-model="form.qaD_Status">
            <option value="Available">Available (Active)</option>
            <option value="Disable">Disable (Locked)</option>
            <option value="Deleted">Deleted</option>
          </select>
        </div>

        <!-- OA -->
        <div class="account-row">
          <div class="account-info">
            <span class="account-title">2. Office Automation (OA)</span>
            <span class="account-sub">Digital Workflow & Online Approval</span>
          </div>
          <select class="form-control status-select" v-model="form.oA_Status">
            <option value="Available">Available (Active)</option>
            <option value="Disable">Disable (Locked)</option>
            <option value="Deleted">Deleted</option>
          </select>
        </div>

        <!-- Email -->
        <div class="account-row">
          <div class="account-info">
            <span class="account-title">3. Corporate Email</span>
            <span class="account-sub">Microsoft 365 / Google Workspace</span>
          </div>
          <select class="form-control status-select" v-model="form.email_Status">
            <option value="Available">Available (Active)</option>
            <option value="Disable">Disable (Locked)</option>
            <option value="Deleted">Deleted</option>
          </select>
        </div>

        <!-- Active Directory -->
        <div class="account-row">
          <div class="account-info">
            <span class="account-title">4. Active Directory (AD)</span>
            <span class="account-sub">Windows Domain Login & Workstation Auth</span>
          </div>
          <select class="form-control status-select" v-model="form.aD_Status">
            <option value="Available">Available (Active)</option>
            <option value="Disable">Disable (Locked)</option>
            <option value="Deleted">Deleted</option>
          </select>
        </div>
      </div>

      <div class="modal-actions-right" style="margin-top: 24px;">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">{{ $t('common.cancel') }}</button>
        <button type="submit" class="btn btn-primary" :disabled="submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          {{ $t('employees.save_accounts') }}
        </button>
      </div>
    </form>
  </Modal>
</template>

<script setup>
import { reactive, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  employee: { type: Object, default: null },
  submitting: { type: Boolean, default: false }
})

defineEmits(['close', 'submit'])

const form = reactive({
  qaD_Status: 'Disable',
  oA_Status: 'Disable',
  email_Status: 'Disable',
  aD_Status: 'Disable'
})

const populateAccounts = () => {
  if (props.isOpen && props.employee) {
    const emp = props.employee
    form.qaD_Status = emp.qaD_Status || 'Disable'
    form.oA_Status = emp.oA_Status || 'Disable'
    form.email_Status = emp.email_Status || 'Disable'
    form.aD_Status = emp.aD_Status || 'Disable'
  }
}

watch([() => props.isOpen, () => props.employee], () => {
  populateAccounts()
}, { immediate: true, deep: true })
</script>
