<template>
  <Modal 
    :is-open="isOpen" 
    title="Cấp Phát Thiết Bị Cho Nhân Viên"
    :subtitle="`Giao thiết bị ${asset?.assetCode} từ kho cho nhân sự sử dụng`"
    @close="$emit('close')"
    max-width="680px"
  >
    <!-- Khung Thông Tin Thiết Bị Xuất Kho -->
    <div v-if="asset" class="assign-device-preview">
      <div class="device-badge-icon assign-theme">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
          <line x1="8" y1="21" x2="16" y2="21"></line>
          <line x1="12" y1="17" x2="12" y2="21"></line>
        </svg>
      </div>
      <div class="device-main-info">
        <div class="device-title-row">
          <span class="device-code-tag">{{ asset.assetCode }}</span>
          <strong class="device-name-text">{{ asset.assetName }}</strong>
          <span v-if="asset.brand" class="brand-tag">{{ asset.brand }}</span>
        </div>
        <div v-if="asset.specifications" class="device-specs-row">
          ⚙️ <span>{{ asset.specifications }}</span>
        </div>
        <div class="device-meta-row">
          <span>🏷️ Serial: <strong class="font-mono">{{ asset.serialNumber || '---' }}</strong></span>
          <span>📍 Vị trí kho: <strong>{{ asset.warehouseLocation || 'Kho IT' }}</strong></span>
        </div>
      </div>
    </div>

    <form @submit.prevent="handleSubmit" style="margin-top: 14px;">
      <div class="form-group">
        <label class="form-label">Chọn Nhân Viên Nhận Bàn Giao *</label>
        <EmployeeSelector 
          v-model="form.toEmployeeID" 
          :employees="employees" 
          :departments="departments" 
        />
      </div>

      <!-- 1. Chọn Chuột Kèm Theo Trong Kho (Khi cấp phát Laptop) -->
      <div v-if="isLaptop" class="form-group accessory-block mouse-block">
        <label class="form-label" style="color: #4f46e5; display: flex; align-items: center; justify-content: space-between;">
          <span>🖱️ Chọn Chuột Kèm Theo Trong Kho (Tùy chọn)</span>
          <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">
            {{ warehouseMice.length }} chuột sẵn có
          </span>
        </label>
        <select class="form-control" v-model="form.mouseAssetID">
          <option :value="null">-- Không cấp chuột kèm theo --</option>
          <option v-for="m in warehouseMice" :key="m.assetID" :value="m.assetID">
            🖱️ {{ m.assetCode }} - {{ m.assetName }} ({{ m.brand || '---' }}) {{ m.serialNumber ? `- S/N: ${m.serialNumber}` : '' }}
          </option>
        </select>
        <small class="text-dim" style="font-size: 0.75rem; margin-top: 4px; display: block;">
          💡 Hệ thống sẽ tự động bàn giao cả Laptop và Chuột này cho nhân viên.
        </small>
      </div>

      <!-- 2. Chọn Phụ Kiện Kèm Theo (Khi cấp phát Máy Tính Để Bàn / PC) -->
      <div v-if="isDesktop" style="display: flex; flex-direction: column; gap: 12px; margin-top: 14px;">
        <!-- 2.1 Chọn Màn Hình -->
        <div class="form-group accessory-block monitor-block">
          <label class="form-label" style="color: #0284c7; display: flex; align-items: center; justify-content: space-between;">
            <span>🖥️ 1. Chọn Màn Hình Kèm Theo (Tùy chọn)</span>
            <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">
              {{ warehouseMonitors.length }} màn hình sẵn có
            </span>
          </label>
          <select class="form-control" v-model="form.monitorAssetID">
            <option :value="null">-- Không cấp màn hình kèm theo --</option>
            <option v-for="mon in warehouseMonitors" :key="mon.assetID" :value="mon.assetID">
              🖥️ {{ mon.assetCode }} - {{ mon.assetName }} ({{ mon.brand || '---' }}) {{ mon.serialNumber ? `- S/N: ${mon.serialNumber}` : '' }}
            </option>
          </select>
        </div>

        <!-- 2.2 Chọn Bàn Phím -->
        <div class="form-group accessory-block keyboard-block">
          <label class="form-label" style="color: #059669; display: flex; align-items: center; justify-content: space-between;">
            <span>⌨️ 2. Chọn Bàn Phím Kèm Theo (Tùy chọn)</span>
            <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">
              {{ warehouseKeyboards.length }} bàn phím sẵn có
            </span>
          </label>
          <select class="form-control" v-model="form.keyboardAssetID">
            <option :value="null">-- Không cấp bàn phím kèm theo --</option>
            <option v-for="kb in warehouseKeyboards" :key="kb.assetID" :value="kb.assetID">
              ⌨️ {{ kb.assetCode }} - {{ kb.assetName }} ({{ kb.brand || '---' }}) {{ kb.serialNumber ? `- S/N: ${kb.serialNumber}` : '' }}
            </option>
          </select>
        </div>

        <!-- 2.3 Chọn Chuột -->
        <div class="form-group accessory-block mouse-block">
          <label class="form-label" style="color: #4f46e5; display: flex; align-items: center; justify-content: space-between;">
            <span>🖱️ 3. Chọn Chuột Kèm Theo (Tùy chọn)</span>
            <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">
              {{ warehouseMice.length }} chuột sẵn có
            </span>
          </label>
          <select class="form-control" v-model="form.mouseAssetID">
            <option :value="null">-- Không cấp chuột kèm theo --</option>
            <option v-for="m in warehouseMice" :key="m.assetID" :value="m.assetID">
              🖱️ {{ m.assetCode }} - {{ m.assetName }} ({{ m.brand || '---' }}) {{ m.serialNumber ? `- S/N: ${m.serialNumber}` : '' }}
            </option>
          </select>
        </div>
      </div>

      <div class="form-group" style="margin-top: 14px;">
        <label class="form-label">Tình trạng máy lúc bàn giao</label>
        <input type="text" class="form-control" v-model="form.conditionStatus" placeholder="Vd: Máy mới 100%, đầy đủ phụ kiện..." />
      </div>

      <div class="form-group">
        <label class="form-label">Ghi chú bàn giao</label>
        <textarea class="form-control" rows="2" v-model="form.note" placeholder="Thông tin bàn giao, phụ kiện cấp phát thêm..."></textarea>
      </div>

      <div class="modal-actions-right">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button type="submit" class="btn btn-primary" :disabled="!form.toEmployeeID || submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          Xác Nhận Bàn Giao
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
  warehouseMice: {
    type: Array,
    default: () => []
  },
  warehouseMonitors: {
    type: Array,
    default: () => []
  },
  warehouseKeyboards: {
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
  conditionStatus: 'Máy hoạt động tốt',
  note: '',
  mouseAssetID: null,
  monitorAssetID: null,
  keyboardAssetID: null
})

watch(() => props.isOpen, (open) => {
  if (open) {
    form.toEmployeeID = null
    form.conditionStatus = 'Máy hoạt động tốt'
    form.note = ''
    form.mouseAssetID = null
    form.monitorAssetID = null
    form.keyboardAssetID = null
  }
})

const isLaptop = computed(() => {
  if (!props.asset) return false
  const cat = (props.asset.categoryName || '').toLowerCase()
  const code = (props.asset.assetCode || '').toLowerCase()
  const name = (props.asset.assetName || '').toLowerCase()
  if (code.includes('dt') || code.includes('pc') || name.includes('optiplex') || name.includes('thinkcentre') || name.includes('prodesk') || name.includes('elitedesk')) {
    return false
  }
  return cat.includes('laptop') || cat.includes('xách tay') || cat.includes('notebook') || code.includes('nb')
})

const isDesktop = computed(() => {
  if (!props.asset) return false
  const cat = (props.asset.categoryName || '').toLowerCase()
  const code = (props.asset.assetCode || '').toLowerCase()
  const name = (props.asset.assetName || '').toLowerCase()
  return cat.includes('để bàn') || cat.includes('desktop') || cat.includes('pc') || code.includes('dt') || code.includes('pc') || name.includes('optiplex') || name.includes('thinkcentre') || name.includes('prodesk') || name.includes('elitedesk')
})

const handleSubmit = () => {
  if (!form.toEmployeeID) return
  emit('submit', { ...form })
}
</script>

<style scoped>
.assign-device-preview {
  display: flex;
  gap: 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 12px;
}

.device-badge-icon {
  width: 44px;
  height: 44px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eff6ff;
  color: #2563eb;
  flex-shrink: 0;
}

.device-main-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.device-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.device-code-tag {
  font-family: monospace;
  font-weight: bold;
  color: #4f46e5;
  background: #eef2ff;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.8rem;
}

.device-specs-row {
  font-size: 0.775rem;
  color: #0284c7;
}

.device-meta-row {
  display: flex;
  gap: 16px;
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 2px;
}

.accessory-block {
  padding: 12px;
  border-radius: 8px;
  margin-bottom: 0;
}

.mouse-block {
  background: rgba(99, 102, 241, 0.05);
  border: 1px solid rgba(99, 102, 241, 0.2);
}

.monitor-block {
  background: rgba(14, 165, 233, 0.06);
  border: 1px solid rgba(14, 165, 233, 0.25);
}

.keyboard-block {
  background: rgba(16, 185, 129, 0.06);
  border: 1px solid rgba(16, 185, 129, 0.25);
}
</style>
