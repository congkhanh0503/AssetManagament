<template>
  <Modal 
    :is-open="isOpen" 
    title="Thu Hồi Thiết Bị Về Kho"
    :subtitle="`Thu hồi máy ${asset?.assetCode} từ ${asset?.holderName}`"
    @close="$emit('close')"
  >
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label class="form-label">Vị trí lưu kho sau khi thu hồi *</label>
        <input type="text" class="form-control" v-model="form.warehouseLocation" required placeholder="Vd: Kho IT - Kệ A1, Tủ Kính A2..." />
      </div>

      <div class="form-group" style="display: flex; align-items: center; gap: 10px;">
        <input type="checkbox" id="isBrokenCheck" v-model="form.isBroken" style="width: 18px; height: 18px; cursor: pointer;" />
        <label for="isBrokenCheck" style="color: #dc2626; font-weight: 600; cursor: pointer;">
          Thiết bị bị hỏng hóc / lỗi cần chuyển đi sửa chữa ngay
        </label>
      </div>

      <div class="form-group">
        <label class="form-label">Tình trạng thực tế lúc thu hồi</label>
        <input type="text" class="form-control" v-model="form.conditionStatus" placeholder="Vd: Máy nguyên vẹn, đầy đủ sạc..." />
      </div>

      <div class="form-group">
        <label class="form-label">Ghi chú thêm</label>
        <textarea class="form-control" rows="2" v-model="form.note" placeholder="Tình trạng bàn phím, pin, phụ kiện kèm theo..."></textarea>
      </div>

      <div class="modal-actions-right">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button type="submit" class="btn btn-primary" :disabled="submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          Xác Nhận Thu Hồi
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
  warehouseLocation: 'Kho IT - Kệ A1',
  isBroken: false,
  conditionStatus: 'Nguyên vẹn',
  note: ''
})

watch(() => props.isOpen, (open) => {
  if (open) {
    form.warehouseLocation = 'Kho IT - Kệ A1'
    form.isBroken = false
    form.conditionStatus = 'Nguyên vẹn'
    form.note = ''
  }
})

const handleSubmit = () => {
  emit('submit', { ...form })
}
</script>
