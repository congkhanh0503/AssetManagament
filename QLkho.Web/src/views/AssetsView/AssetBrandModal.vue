<template>
  <Modal 
    :is-open="isOpen" 
    title="Quản Lý Hãng Sản Xuất / Thương Hiệu"
    subtitle="Thêm mới, chỉnh sửa và quản lý danh mục hãng thiết bị trong hệ thống"
    @close="$emit('close')"
    max-width="780px"
  >
    <div class="brand-manager-content">
      <!-- Form Thêm / Chỉnh Sửa Hãng -->
      <div class="glass-card brand-form-box">
        <div class="brand-box-header">
          <span style="font-weight: 700; font-size: 0.9rem; color: #1e293b;">
            {{ editingBrandId ? '✏️ Chỉnh Sửa Thông Tin Hãng' : '➕ Thêm Hãng Sản Xuất Mới' }}
          </span>
          <button 
            v-if="editingBrandId" 
            type="button" 
            class="btn btn-sm btn-outline-secondary" 
            @click="resetForm"
          >
            Hủy sửa / Tạo mới
          </button>
        </div>

        <form @submit.prevent="handleSubmit" style="margin-top: 10px;">
          <div class="modal-grid-2">
            <div class="form-group" style="margin-bottom: 0;">
              <label class="form-label">Tên Hãng / Thương Hiệu *</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="form.brandName" 
                required 
                placeholder="Vd: Dell, Logitech, HP, Apple..." 
              />
            </div>

            <div class="form-group" style="margin-bottom: 0;">
              <label class="form-label">Quốc Gia / Xuất Xứ</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="form.originCountry" 
                placeholder="Mỹ, Thụy Sĩ, Đài Loan, Nhật Bản..." 
              />
            </div>
          </div>

          <div class="form-group" style="margin-top: 10px; margin-bottom: 0;">
            <label class="form-label">Mô Tả / Dòng Thiết Bị Chính</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="form.description" 
              placeholder="Laptop doanh nghiệp, chuột quang, switch mạng, máy in..." 
            />
          </div>

          <div style="margin-top: 12px; display: flex; justify-content: flex-end; gap: 8px;">
            <button type="submit" class="btn btn-primary btn-sm" :disabled="submitting">
              <span v-if="submitting" class="loading-spinner"></span>
              {{ editingBrandId ? '💾 Cập Nhật Hãng' : '➕ Thêm Hãng Vào Danh Mục' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Danh sách Hãng đang quản lý -->
      <div style="margin-top: 18px;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
          <strong style="font-size: 0.875rem; color: #0f172a;">
            Danh Sách Hãng Trong Hệ Thống ({{ brands.length }})
          </strong>
        </div>

        <div class="table-responsive" style="max-height: 320px; overflow-y: auto; border: 1px solid var(--border); border-radius: 8px;">
          <table class="data-table">
            <thead>
              <tr>
                <th style="width: 140px;">Tên Hãng</th>
                <th style="width: 120px;">Xuất Xứ</th>
                <th>Mô Tả Sản Phẩm</th>
                <th style="width: 110px; text-align: center;">Thiết Bị Dùng</th>
                <th style="width: 110px; text-align: center;">Thao Tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="brands.length === 0">
                <td colspan="5" style="text-align: center; color: #64748b; padding: 20px;">Chưa có hãng nào trong danh mục</td>
              </tr>
              <tr v-for="b in brands" :key="b.brandID">
                <td>
                  <strong style="color: #4f46e5; font-size: 0.9rem;">{{ b.brandName }}</strong>
                </td>
                <td>
                  <span v-if="b.originCountry" class="country-tag">{{ b.originCountry }}</span>
                  <span v-else class="text-dim">---</span>
                </td>
                <td>
                  <span class="text-dim" style="font-size: 0.8rem;">{{ b.description || '---' }}</span>
                </td>
                <td style="text-align: center;">
                  <span class="badge-asset-count" :class="{ 'has-assets': b.assetCount > 0 }">
                    {{ b.assetCount }} thiết bị
                  </span>
                </td>
                <td style="text-align: center;">
                  <div style="display: flex; gap: 6px; justify-content: center;">
                    <button 
                      type="button" 
                      class="btn-icon-brand edit" 
                      @click="startEdit(b)" 
                      title="Chỉnh sửa thông tin hãng"
                    >
                      ✏️ Sửa
                    </button>
                    <button 
                      type="button" 
                      class="btn-icon-brand delete" 
                      @click="$emit('delete', b)" 
                      title="Xóa hãng"
                    >
                      🗑️ Xóa
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="modal-actions-right" style="margin-top: 14px;">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { reactive, ref } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  brands: {
    type: Array,
    default: () => []
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'save', 'delete'])

const editingBrandId = ref(null)
const form = reactive({
  brandName: '',
  originCountry: '',
  description: ''
})

const resetForm = () => {
  editingBrandId.value = null
  form.brandName = ''
  form.originCountry = ''
  form.description = ''
}

const startEdit = (b) => {
  editingBrandId.value = b.brandID
  form.brandName = b.brandName
  form.originCountry = b.originCountry || ''
  form.description = b.description || ''
}

const handleSubmit = () => {
  if (!form.brandName.trim()) return
  emit('save', {
    id: editingBrandId.value,
    data: { ...form },
    callback: resetForm
  })
}
</script>

<style scoped>
.brand-manager-content {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.brand-form-box {
  padding: 16px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
}

.brand-box-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.country-tag {
  background: #eff6ff;
  color: #2563eb;
  border: 1px solid #bfdbfe;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}

.badge-asset-count {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 600;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 999px;
}

.badge-asset-count.has-assets {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
}

.btn-icon-brand {
  border: 1px solid var(--border);
  background: #ffffff;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-icon-brand.edit:hover {
  background: #eff6ff;
  border-color: #3b82f6;
  color: #1d4ed8;
}

.btn-icon-brand.delete:hover {
  background: #fef2f2;
  border-color: #ef4444;
  color: #b91c1c;
}
</style>
