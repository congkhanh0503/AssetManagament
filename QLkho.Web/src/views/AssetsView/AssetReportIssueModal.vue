<template>
  <Modal 
    :is-open="isOpen" 
    title="Ghi Nhận Sự Cố & Báo Hỏng Thiết Bị"
    :subtitle="`Ghi nhận tình trạng lỗi cho thiết bị: ${asset?.assetCode} (${asset?.assetName})`"
    @close="$emit('close')"
  >
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label class="form-label">Mô Tả Chi Tiết Sự Cố / Lỗi Thiết Bị *</label>
        <textarea 
          class="form-control" 
          rows="3" 
          v-model="form.issueDescription" 
          required 
          placeholder="Vd: Màn hình bị sọc ngang, hỏng bàn phím phím space, máy không lên nguồn..."
        ></textarea>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">Chuyển Trạng Thái Thiết Bị</label>
          <select class="form-control" v-model="form.newStatus">
            <option value="Broken">Bị hỏng (Broken)</option>
            <option value="Maintenance">Đang gửi đi bảo hành / sửa chữa (Maintenance)</option>
          </select>
        </div>

        <div class="form-group">
          <label class="form-label">Đơn Vị Bảo Hành / Sửa Chữa</label>
          <input type="text" class="form-control" v-model="form.vendorName" placeholder="Vd: FPT Services, Dell Care..." />
        </div>
      </div>

      <div class="modal-grid-2">
        <div class="form-group">
          <label class="form-label">Chi Phí Dự Kiến (VNĐ)</label>
          <input type="number" class="form-control" v-model="form.estimatedCost" placeholder="0" />
        </div>

        <div class="form-group">
          <label class="form-label">Ngày Dự Kiến Trả</label>
          <input type="date" class="form-control" v-model="form.expectedReturnDate" />
        </div>
      </div>

      <div class="modal-actions-right">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button type="submit" class="btn btn-danger" :disabled="!form.issueDescription.trim() || submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          Xác Nhận Báo Hỏng
        </button>
      </div>
    </form>
  </Modal>
</template>

<script setup>
import { reactive, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  asset: {
    type: Object,
    default: null
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'submit'])

const form = reactive({
  issueDescription: '',
  newStatus: 'Broken',
  vendorName: '',
  estimatedCost: null,
  expectedReturnDate: ''
})

watch(() => props.isOpen, (open) => {
  if (open) {
    form.issueDescription = ''
    form.newStatus = 'Broken'
    form.vendorName = ''
    form.estimatedCost = null
    form.expectedReturnDate = ''
  }
})

const handleSubmit = () => {
  if (!form.issueDescription.trim()) return
  emit('submit', { ...form })
}
</script>

<style scoped>
.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
</style>
