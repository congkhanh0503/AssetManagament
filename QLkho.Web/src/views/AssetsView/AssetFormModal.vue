<template>
  <Modal 
    :is-open="isOpen" 
    :title="isEdit ? `Chỉnh Sửa Thiết Bị: ${form.assetCode}` : 'Thêm Mới Thiết Bị Vào Kho'"
    :subtitle="isEdit ? 'Cập nhật chi tiết thông số phần cứng khớp với Biên Bản Bàn Giao' : 'Khai báo đầy đủ thông số kỹ thuật và phụ kiện khớp với Biên Bản Bàn Giao Laptop'"
    @close="$emit('close')"
    max-width="780px"
  >
    <form @submit.prevent="handleSubmit">
      <!-- TRƯỜNG HỢP 1: THIẾT BỊ LÀ CHUỘT HOẶC BÀN PHÍM (PHỤ KIỆN LÔ HOẶC SỬA GỌN) -->
      <div v-if="isMouseOrKeyboard" class="mouse-quick-section">
        <div class="form-section-title">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1;">
            <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
            <line x1="8" y1="21" x2="16" y2="21"></line>
            <line x1="12" y1="17" x2="12" y2="21"></line>
          </svg>
          1. THÔNG TIN {{ isKeyboard ? 'BÀN PHÍM' : 'CHUỘT' }} {{ isEdit ? '' : 'NHẬP KHO' }}
        </div>

        <!-- Banner Tự Động Gom Vào Lô Chuột Hiện Có (Khi tạo mới) -->
        <div v-if="!isEdit && isMouse && existingMouseLotInfo" class="lot-hint-box">
          <div class="hint-icon">💡</div>
          <div class="hint-body">
            <strong>Đã có sẵn lô Chuột {{ existingMouseLotInfo.brand }} trong kho:</strong>
            <div>
              Hiện đang có <b>{{ existingMouseLotInfo.currentCount }} con</b> ({{ existingMouseLotInfo.availableCount }} sẵn sàng trong kho, {{ existingMouseLotInfo.inUseCount }} đang cấp phát).
              <br />
              Chuột mới sẽ được <b>tự động gom vào lô này</b> với số thứ tự tiếp theo từ <b>#{{ existingMouseLotInfo.nextIndex }}</b> (Mã: <code>{{ existingMouseLotInfo.previewNextCode }}</code>)!
            </div>
          </div>
        </div>

        <!-- Banner Tự Động Gom Vào Lô Bàn Phím Hiện Có (Khi tạo mới) -->
        <div v-if="!isEdit && isKeyboard && existingKeyboardLotInfo" class="lot-hint-box keyboard-theme">
          <div class="hint-icon">💡</div>
          <div class="hint-body">
            <strong style="color: #065f46;">Đã có sẵn lô Bàn phím {{ existingKeyboardLotInfo.brand }} trong kho:</strong>
            <div>
              Hiện đang có <b>{{ existingKeyboardLotInfo.currentCount }} chiếc</b> ({{ existingKeyboardLotInfo.availableCount }} sẵn sàng trong kho, {{ existingKeyboardLotInfo.inUseCount }} đang cấp phát).
              <br />
              Bàn phím mới sẽ được <b>tự động gom vào lô này</b> với số thứ tự tiếp theo từ <b>#{{ existingKeyboardLotInfo.nextIndex }}</b> (Mã: <code style="background: #d1fae5; color: #047857;">{{ existingKeyboardLotInfo.previewNextCode }}</code>)!
            </div>
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Loại Thiết Bị *</label>
            <select class="form-control" v-model="form.categoryID" required>
              <option v-for="cat in categories" :key="cat.categoryID" :value="cat.categoryID">
                {{ cat.categoryName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
              <label class="form-label" style="margin-bottom: 0;">Hãng Sản Xuất (Brand) *</label>
              <button type="button" class="btn-link-action" @click="$emit('quick-brand')">+ Thêm Hãng</button>
            </div>
            <input 
              type="text" 
              class="form-control" 
              list="brands-datalist-modal" 
              v-model="form.brand" 
              required 
              :placeholder="isKeyboard ? 'Logitech, Dell, Fuhlen, DareU, HP...' : 'Logitech, Dell, Genius, DareU...'" 
            />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Tên {{ isKeyboard ? 'Bàn Phím' : 'Chuột' }} / Model *</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="form.assetName" 
              required 
              :placeholder="isKeyboard ? 'Vd: Bàn phím Logitech K120, Bàn phím Dell KB216...' : 'Vd: Chuột quang Logitech B170, Chuột Dell MS116...'" 
            />
          </div>

          <!-- Khi Tạo mới -> Nhập Số lượng; Khi Sửa -> Mã Asset Code -->
          <div v-if="!isEdit" class="form-group">
            <label class="form-label">Số Lượng Nhập Kho (Chiếc) *</label>
            <input type="number" class="form-control" v-model.number="form.quantity" min="1" max="500" required placeholder="Vd: 1, 10, 50, 100..." />
          </div>

          <div v-else class="form-group">
            <label class="form-label">Mã Quản Lý (Asset Code) *</label>
            <input type="text" class="form-control" v-model="form.assetCode" required />
          </div>
        </div>

        <div class="form-section-title" style="margin-top: 10px;">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #059669;">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
            <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
            <line x1="12" y1="22.08" x2="12" y2="12"></line>
          </svg>
          2. VỊ TRÍ KHO & TRẠNG THÁI
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Vị Trí Lưu Kho</label>
            <input type="text" class="form-control" v-model="form.warehouseLocation" :placeholder="isKeyboard ? 'Kho IT - Kệ Bàn Phím' : 'Kho IT - Kệ Phụ Kiện'" />
          </div>

          <div class="form-group">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
              <label class="form-label" style="margin-bottom: 0;">Nhà Cung Cấp</label>
              <button type="button" class="btn-link-action" @click="$emit('quick-sup')">+ Thêm NCC</button>
            </div>
            <select class="form-control" v-model="form.supplierID">
              <option :value="null">-- Không chọn --</option>
              <option v-for="sup in suppliers" :key="sup.supplierID" :value="sup.supplierID">
                {{ sup.supplierName }}
              </option>
            </select>
          </div>
        </div>

        <div v-if="isEdit" class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Trạng Thái Thiết Bị</label>
            <div v-if="form.status === 'In-Use'" class="inuse-state-box">
              <span style="font-size: 0.825rem; color: #0f172a; font-weight: 600;">
                Đang cấp cho: <strong style="color: #2563eb;">{{ form.holderName || 'Nhân viên' }}</strong>
              </span>
            </div>
            <select v-else class="form-control" v-model="form.status">
              <option value="Available">Sẵn sàng (Available)</option>
              <option value="Maintenance">Đang bảo trì (Maintenance)</option>
              <option value="Broken">Bị hỏng (Broken)</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Ghi Chú Kèm Theo</label>
            <input type="text" class="form-control" v-model="form.note" placeholder="Vd: Chuột USB, Kèm pin..." />
          </div>
        </div>

        <div v-else class="form-group">
          <label class="form-label">Ghi Chú Kèm Theo (Tùy chọn)</label>
          <input type="text" class="form-control" v-model="form.note" :placeholder="isKeyboard ? 'Vd: Bàn phím có dây cổng USB...' : 'Vd: Chuột quang USB, Kèm pin AA...'" />
        </div>
      </div>

      <!-- TRƯỜNG HỢP 2: THIẾT BỊ KHÁC (LAPTOP / PC / MÀN HÌNH...) -> FORM ĐẦY ĐỦ -->
      <div v-else class="standard-section">
        <!-- 1. THÔNG TIN ĐỊNH DANH -->
        <div class="form-section-title">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1;">
            <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
            <line x1="8" y1="21" x2="16" y2="21"></line>
            <line x1="12" y1="17" x2="12" y2="21"></line>
          </svg>
          1. THÔNG TIN ĐỊNH DANH (IDENTIFIERS)
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Host Name (Mã Máy / Tên Máy) *</label>
            <input type="text" class="form-control" v-model="form.assetCode" required placeholder="Vd: TTH-NB0065, AST-LT-010" />
          </div>

          <div class="form-group">
            <label class="form-label">Model (Tên Model Thiết Bị) *</label>
            <input type="text" class="form-control" v-model="form.assetName" required placeholder="Vd: HP Elitebook 640 G11, Dell Latitude 5430..." />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Service Tag / Số Serial (S/N) *</label>
            <input type="text" class="form-control" v-model="form.serialNumber" required placeholder="Vd: 5CD5183KKV, 5CD534DZP5..." />
          </div>

          <div class="form-group">
            <label class="form-label">Asset Number (Mã Tài Sản / Mã Vật Tư)</label>
            <input type="text" class="form-control" v-model="form.materialCode" placeholder="Vd: VT-HP-640, AST0065" />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
              <label class="form-label" style="margin-bottom: 0;">Loại Thiết Bị *</label>
              <button type="button" class="btn-link-action" @click="$emit('quick-cat')">+ Thêm loại</button>
            </div>
            <select class="form-control" v-model="form.categoryID" required>
              <option :value="null" disabled>-- Chọn loại thiết bị --</option>
              <option v-for="cat in categories" :key="cat.categoryID" :value="cat.categoryID">
                {{ cat.categoryName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
              <label class="form-label" style="margin-bottom: 0;">Hãng Sản Xuất (Brand)</label>
              <button type="button" class="btn-link-action" @click="$emit('quick-brand')">+ Thêm Hãng</button>
            </div>
            <input 
              type="text" 
              class="form-control" 
              list="brands-datalist-modal" 
              v-model="form.brand" 
              placeholder="Chọn hoặc nhập hãng (HP, Dell, Apple...)" 
            />
          </div>
        </div>

        <!-- 2. COMPUTER SPECS (CHO LAPTOP / PC) -->
        <div v-if="isComputer" class="computer-specs-block">
          <div class="form-section-title" style="margin-top: 10px;">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #0284c7;">
              <rect x="4" y="4" width="16" height="16" rx="2" ry="2"></rect>
              <rect x="9" y="9" width="6" height="6"></rect>
              <line x1="9" y1="1" x2="9" y2="4"></line>
              <line x1="15" y1="1" x2="15" y2="4"></line>
              <line x1="9" y1="20" x2="9" y2="23"></line>
              <line x1="15" y1="20" x2="15" y2="23"></line>
              <line x1="20" y1="9" x2="23" y2="9"></line>
              <line x1="20" y1="14" x2="23" y2="14"></line>
              <line x1="1" y1="9" x2="4" y2="9"></line>
              <line x1="1" y1="14" x2="4" y2="14"></line>
            </svg>
            2. COMPUTER SPECS (THÔNG SỐ PHẦN CỨNG)
          </div>

          <div class="modal-grid-3">
            <div class="form-group">
              <label class="form-label">CPU (Vi Xử Lý) *</label>
              <input type="text" class="form-control" v-model="form.cpu" :required="isComputer" placeholder="Vd: Core Ultra 5-125U, Core i5-1240P..." />
            </div>

            <div class="form-group">
              <label class="form-label">RAM *</label>
              <select class="form-control" v-model="form.ram">
                <option value="8 GB">8 GB</option>
                <option value="16 GB">16 GB</option>
                <option value="32 GB">32 GB</option>
                <option value="64 GB">64 GB</option>
                <option value="4 GB">4 GB</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Disk (Ổ Cứng) *</label>
              <select class="form-control" v-model="form.disk">
                <option value="256 GB SSD">256 GB SSD</option>
                <option value="512 GB SSD">512 GB SSD</option>
                <option value="1 TB SSD">1 TB SSD</option>
                <option value="2 TB SSD">2 TB SSD</option>
                <option value="128 GB SSD">128 GB SSD</option>
              </select>
            </div>
          </div>

          <div class="modal-grid-2">
            <div class="form-group">
              <label class="form-label">OS (Hệ Điều Hành) *</label>
              <select class="form-control" v-model="form.os">
                <option value="Windows 11">Windows 11</option>
                <option value="Windows 11 Pro">Windows 11 Pro</option>
                <option value="Windows 10 Pro">Windows 10 Pro</option>
                <option value="macOS Sonoma">macOS</option>
                <option value="Ubuntu Linux">Ubuntu Linux</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Display (Màn Hình) *</label>
              <select class="form-control" v-model="form.display">
                <option value="Không có (N/A)">Không có (N/A - Cho PC)</option>
                <option value="14-inch">14-inch (Laptop)</option>
                <option value="15.6-inch">15.6-inch (Laptop)</option>
                <option value="13.3-inch">13.3-inch (Laptop)</option>
                <option value="16-inch">16-inch (Laptop)</option>
                <option value="24-inch">24-inch</option>
                <option value="27-inch">27-inch</option>
              </select>
            </div>
          </div>
        </div>

        <div v-else class="other-specs-block">
          <div class="form-group" style="margin-top: 10px;">
            <label class="form-label">Thông Số Kỹ Thuật / Đặc Điểm (Tùy chọn)</label>
            <input type="text" class="form-control" v-model="form.specifications" placeholder="Vd: 27-inch 4K IPS 144Hz, In 2 mặt tự động..." />
          </div>
        </div>

        <!-- 3. PHỤ KIỆN & QUẢN LÝ LƯU KHO -->
        <div class="form-section-title" style="margin-top: 10px;">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #059669;">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
            <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
            <line x1="12" y1="22.08" x2="12" y2="12"></line>
          </svg>
          3. PHỤ KIỆN & QUẢN LÝ LƯU KHO
        </div>

        <div v-if="isComputer" class="form-group">
          <label class="form-label">Củ Sạc & Ghi Chú Kèm Theo</label>
          <input type="text" class="form-control" v-model="form.charger" placeholder="Kèm củ sạc + Dây nguồn zin" />
        </div>

        <div v-else class="form-group">
          <label class="form-label">Ghi Chú Phụ Kiện / Tình Trạng Kèm Theo</label>
          <input type="text" class="form-control" v-model="form.note" placeholder="Vd: Kèm cáp nguồn, cáp HDMI, chân đế..." />
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px;">
              <label class="form-label" style="margin-bottom: 0;">Nhà Cung Cấp</label>
              <button type="button" class="btn-link-action" @click="$emit('quick-sup')">+ Thêm NCC</button>
            </div>
            <select class="form-control" v-model="form.supplierID">
              <option :value="null">-- Không chọn --</option>
              <option v-for="sup in suppliers" :key="sup.supplierID" :value="sup.supplierID">
                {{ sup.supplierName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Vị Trí Lưu Kho</label>
            <input type="text" class="form-control" v-model="form.warehouseLocation" placeholder="Kho IT - Kệ A1" />
          </div>
        </div>

        <div v-if="isEdit" class="form-group">
          <label class="form-label">Trạng Thái Thiết Bị</label>
          <div v-if="form.status === 'In-Use'" class="inuse-state-box">
            <span style="font-size: 0.825rem; color: #0f172a; font-weight: 600;">
              Đang cấp cho: <strong style="color: #2563eb;">{{ form.holderName || 'Nhân viên' }}</strong>
            </span>
          </div>
          <select v-else class="form-control" v-model="form.status">
            <option value="Available">Sẵn sàng (Available)</option>
            <option value="Maintenance">Đang bảo trì (Maintenance)</option>
            <option value="Broken">Bị hỏng (Broken)</option>
          </select>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Ngày Mua</label>
            <input type="date" class="form-control" v-model="form.purchaseDate" />
          </div>

          <div class="form-group">
            <label class="form-label">Hạn Bảo Hành</label>
            <input type="date" class="form-control" v-model="form.warrantyExpireDate" />
          </div>
        </div>
      </div>

      <div class="modal-actions-right" style="margin-top: 20px;">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button type="submit" class="btn btn-primary" :disabled="submitting">
          <span v-if="submitting" class="loading-spinner"></span>
          {{ isEdit ? 'Cập Nhật Thiết Bị' : 'Lưu Thiết Bị Vào Kho' }}
        </button>
      </div>
    </form>

    <datalist id="brands-datalist-modal">
      <option v-for="b in brands" :key="b.brandID" :value="b.brandName">
        {{ b.brandName }} {{ b.originCountry ? `(${b.originCountry})` : '' }}
      </option>
    </datalist>
  </Modal>
</template>

<script setup>
import { computed } from 'vue'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  isEdit: {
    type: Boolean,
    default: false
  },
  form: {
    type: Object,
    required: true
  },
  categories: {
    type: Array,
    default: () => []
  },
  suppliers: {
    type: Array,
    default: () => []
  },
  brands: {
    type: Array,
    default: () => []
  },
  existingMouseLotInfo: {
    type: Object,
    default: null
  },
  existingKeyboardLotInfo: {
    type: Object,
    default: null
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'submit', 'quick-cat', 'quick-sup', 'quick-brand'])

const currentCategory = computed(() => {
  return props.categories.find(c => c.categoryID === props.form.categoryID)
})

const isMouse = computed(() => {
  const catName = (currentCategory.value?.categoryName || '').toLowerCase()
  const name = (props.form.assetName || '').toLowerCase()
  return catName.includes('chuột') || catName.includes('mouse') || name.includes('chuột') || name.includes('mouse')
})

const isKeyboard = computed(() => {
  const catName = (currentCategory.value?.categoryName || '').toLowerCase()
  const name = (props.form.assetName || '').toLowerCase()
  return catName.includes('bàn phím') || catName.includes('keyboard') || name.includes('bàn phím') || name.includes('keyboard')
})

const isMouseOrKeyboard = computed(() => isMouse.value || isKeyboard.value)

const isComputer = computed(() => {
  const catName = (currentCategory.value?.categoryName || '').toLowerCase()
  const catCode = (currentCategory.value?.categoryCode || '').toLowerCase()
  return catName.includes('laptop') || catName.includes('máy tính') || catName.includes('notebook') || 
         catName.includes('desktop') || catName.includes('pc') || catCode.includes('lt') || catCode.includes('pc') || catCode.includes('dt')
})

const handleSubmit = () => {
  emit('submit')
}
</script>

<style scoped>
.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.modal-grid-3 {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 12px;
}

.form-section-title {
  font-size: 0.825rem;
  font-weight: 700;
  color: #334155;
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 14px 0 10px 0;
  text-transform: uppercase;
}

.lot-hint-box {
  background: #eff6ff;
  border-left: 4px solid #3b82f6;
  border-radius: 6px;
  padding: 10px 12px;
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
  font-size: 0.8rem;
  color: #1e3a8a;
}

.lot-hint-box.keyboard-theme {
  background: #ecfdf5;
  border-left-color: #059669;
  color: #065f46;
}

.hint-icon {
  font-size: 1.25rem;
}

.inuse-state-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  padding: 8px 12px;
}

.btn-link-action {
  background: none;
  border: none;
  color: #4f46e5;
  font-size: 0.775rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
}

.btn-link-action:hover {
  text-decoration: underline;
}
</style>
