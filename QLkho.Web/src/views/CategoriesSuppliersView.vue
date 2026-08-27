<template>
  <div class="cat-sup-page">
    <div class="page-topbar">
      <div>
        <h2 class="page-title">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1;">
            <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"></path>
            <line x1="7" y1="7" x2="7.01" y2="7"></line>
          </svg>
          {{ $t('categories_suppliers.title') }}
        </h2>
        <p class="page-subtitle">{{ $t('categories_suppliers.subtitle') }}</p>
      </div>

      <!-- Top Switch Tabs & Action Button -->
      <div class="top-actions">
        <div class="tab-toggle">
          <button 
            :class="['tab-btn', { active: activeTab === 'categories' }]"
            @click="activeTab = 'categories'"
          >
            {{ $t('categories_suppliers.tab_categories') }} ({{ categories.length }})
          </button>
          <button 
            :class="['tab-btn', { active: activeTab === 'suppliers' }]"
            @click="activeTab = 'suppliers'"
          >
            {{ $t('categories_suppliers.tab_suppliers') }} ({{ suppliers.length }})
          </button>
        </div>

        <button 
          v-if="activeTab === 'categories'" 
          class="btn btn-primary"
          @click="openCreateCatModal"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19"></line>
            <line x1="5" y1="12" x2="19" y2="12"></line>
          </svg>
          {{ $t('categories_suppliers.btn_add_cat') }}
        </button>

        <button 
          v-else 
          class="btn btn-primary"
          @click="openCreateSupModal"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19"></line>
            <line x1="5" y1="12" x2="19" y2="12"></line>
          </svg>
          {{ $t('categories_suppliers.btn_add_sup') }}
        </button>
      </div>
    </div>

    <!-- TAB 1: PHÂN LOẠI THIẾT BỊ -->
    <div v-if="activeTab === 'categories'" class="tab-body">
      <div v-if="loadingCats" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải danh sách phân loại...</p>
      </div>

      <div v-else class="cards-grid">
        <div v-for="cat in categories" :key="cat.categoryID" class="glass-card item-card">
          <div class="card-top">
            <span class="code-badge">{{ cat.categoryCode }}</span>
            <div class="card-top-right">
              <span class="count-pill">{{ cat.assetCount }} thiết bị</span>
              <button class="btn-icon-mini" @click="openEditCatModal(cat)" title="Chỉnh sửa loại này">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                </svg>
              </button>
              <button class="btn-icon-mini btn-delete-icon" @click="deleteCatItem(cat)" title="Xóa loại này">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </div>
          </div>

          <h3 class="item-title">{{ cat.categoryName }}</h3>
          <p class="item-desc">{{ cat.description || 'Chưa có mô tả chi tiết cho loại này.' }}</p>
        </div>
      </div>
    </div>

    <!-- TAB 2: NHÀ CUNG CẤP -->
    <div v-else class="tab-body">
      <div v-if="loadingSups" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải danh sách nhà cung cấp...</p>
      </div>

      <div v-else class="cards-grid">
        <div v-for="sup in suppliers" :key="sup.supplierID" class="glass-card item-card">
          <div class="card-top">
            <span class="code-badge" style="color: #34d399; background: rgba(16, 185, 129, 0.12); border-color: rgba(16, 185, 129, 0.25);">
              {{ sup.supplierCode }}
            </span>
            <div class="card-top-right">
              <span class="count-pill">{{ sup.suppliedAssetCount }} máy</span>
              <button class="btn-icon-mini" @click="openEditSupModal(sup)" title="Chỉnh sửa nhà cung cấp">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                </svg>
              </button>
              <button class="btn-icon-mini btn-delete-icon" @click="deleteSupItem(sup)" title="Xóa nhà cung cấp">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="3 6 5 6 21 6"></polyline>
                  <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
              </button>
            </div>
          </div>

          <h3 class="item-title">{{ sup.supplierName }}</h3>

          <div class="sup-meta-list">
            <div class="meta-row">
              <span class="meta-label">Người liên hệ:</span>
              <span class="meta-value">{{ sup.contactPerson || '---' }}</span>
            </div>
            <div class="meta-row">
              <span class="meta-label">Điện thoại:</span>
              <span class="meta-value">{{ sup.phone || '---' }}</span>
            </div>
            <div class="meta-row">
              <span class="meta-label">Email:</span>
              <span class="meta-value">{{ sup.email || '---' }}</span>
            </div>
            <div class="meta-row">
              <span class="meta-label">Địa chỉ:</span>
              <span class="meta-value">{{ sup.address || '---' }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL 1: TẠO MỚI PHÂN LOẠI -->
    <Modal 
      :is-open="isCatModalOpen" 
      title="Thêm Mới Phân Loại Thiết Bị"
      subtitle="Khai báo danh mục loại tài sản mới (Laptop, PC, Thiết bị mạng...)"
      @close="isCatModalOpen = false"
    >
      <form @submit.prevent="submitCreateCategory">
        <div class="form-group">
          <label class="form-label">Mã Loại *</label>
          <input type="text" class="form-control" v-model="catForm.categoryCode" required placeholder="Vd: LAPTOP, MONITOR, TABLET..." />
        </div>

        <div class="form-group">
          <label class="form-label">Tên Loại Thiết Bị *</label>
          <input type="text" class="form-control" v-model="catForm.categoryName" required placeholder="Vd: Máy tính xách tay (Laptop)" />
        </div>

        <div class="form-group">
          <label class="form-label">Mô Tả</label>
          <textarea class="form-control" rows="3" v-model="catForm.description" placeholder="Mô tả phạm vi sử dụng..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isCatModalOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Lưu Phân Loại
          </button>
        </div>
      </form>
    </Modal>

    <!-- MODAL 2: SỬA PHÂN LOẠI -->
    <Modal 
      :is-open="isEditCatOpen" 
      :title="`Chỉnh Sửa Loại: ${editCatForm.categoryName}`"
      subtitle="Cập nhật mã, tên và mô tả phân loại"
      @close="isEditCatOpen = false"
    >
      <form @submit.prevent="submitUpdateCategory">
        <div class="form-group">
          <label class="form-label">Mã Loại *</label>
          <input type="text" class="form-control" v-model="editCatForm.categoryCode" required />
        </div>

        <div class="form-group">
          <label class="form-label">Tên Loại Thiết Bị *</label>
          <input type="text" class="form-control" v-model="editCatForm.categoryName" required />
        </div>

        <div class="form-group">
          <label class="form-label">Mô Tả</label>
          <textarea class="form-control" rows="3" v-model="editCatForm.description"></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isEditCatOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Cập Nhật Phân Loại
          </button>
        </div>
      </form>
    </Modal>

    <!-- MODAL 3: TẠO MỚI NHÀ CUNG CẤP -->
    <Modal 
      :is-open="isSupModalOpen" 
      title="Thêm Mới Nhà Cung Cấp / Bảo Hành"
      subtitle="Khai báo đối tác phân phối thiết bị & trung tâm bảo dưỡng"
      @close="isSupModalOpen = false"
    >
      <form @submit.prevent="submitCreateSupplier">
        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Mã Nhà Cung Cấp *</label>
            <input type="text" class="form-control" v-model="supForm.supplierCode" required placeholder="Vd: SUP-FPT, SUP-DELL..." />
          </div>
          <div class="form-group">
            <label class="form-label">Tên Nhà Cung Cấp *</label>
            <input type="text" class="form-control" v-model="supForm.supplierName" required placeholder="Vd: FPT Information System" />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Người Liên Hệ</label>
            <input type="text" class="form-control" v-model="supForm.contactPerson" placeholder="Vd: Nguyễn Văn A" />
          </div>
          <div class="form-group">
            <label class="form-label">Số Điện Thoại</label>
            <input type="text" class="form-control" v-model="supForm.phone" placeholder="0901234567" />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Email Liên Hệ</label>
          <input type="email" class="form-control" v-model="supForm.email" placeholder="contact@supplier.com" />
        </div>

        <div class="form-group">
          <label class="form-label">Địa Chỉ</label>
          <input type="text" class="form-control" v-model="supForm.address" placeholder="Tòa nhà, Đường, Quận, Thành phố..." />
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isSupModalOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Lưu Nhà Cung Cấp
          </button>
        </div>
      </form>
    </Modal>

    <!-- MODAL 4: SỬA NHÀ CUNG CẤP -->
    <Modal 
      :is-open="isEditSupOpen" 
      :title="`Chỉnh Sửa NCC: ${editSupForm.supplierName}`"
      subtitle="Cập nhật thông tin đối tác cung cấp / bảo hành"
      @close="isEditSupOpen = false"
    >
      <form @submit.prevent="submitUpdateSupplier">
        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Mã Nhà Cung Cấp *</label>
            <input type="text" class="form-control" v-model="editSupForm.supplierCode" required />
          </div>
          <div class="form-group">
            <label class="form-label">Tên Nhà Cung Cấp *</label>
            <input type="text" class="form-control" v-model="editSupForm.supplierName" required />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Người Liên Hệ</label>
            <input type="text" class="form-control" v-model="editSupForm.contactPerson" />
          </div>
          <div class="form-group">
            <label class="form-label">Số Điện Thoại</label>
            <input type="text" class="form-control" v-model="editSupForm.phone" />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Email Liên Hệ</label>
          <input type="email" class="form-control" v-model="editSupForm.email" />
        </div>

        <div class="form-group">
          <label class="form-label">Địa Chỉ</label>
          <input type="text" class="form-control" v-model="editSupForm.address" />
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isEditSupOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Cập Nhật Nhà Cung Cấp
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
import { categoriesApi, suppliersApi } from '@/api/client'
import Modal from '@/components/common/Modal.vue'
import Toast from '@/components/common/Toast.vue'

const activeTab = ref('categories') // 'categories' or 'suppliers'
const categories = ref([])
const suppliers = ref([])
const loadingCats = ref(false)
const loadingSups = ref(false)
const submitting = ref(false)

const isCatModalOpen = ref(false)
const isEditCatOpen = ref(false)
const isSupModalOpen = ref(false)
const isEditSupOpen = ref(false)

const toastRef = ref(null)

const catForm = reactive({
  categoryCode: '',
  categoryName: '',
  description: ''
})

const editCatForm = reactive({
  categoryID: null,
  categoryCode: '',
  categoryName: '',
  description: ''
})

const supForm = reactive({
  supplierCode: '',
  supplierName: '',
  contactPerson: '',
  phone: '',
  email: '',
  address: ''
})

const editSupForm = reactive({
  supplierID: null,
  supplierCode: '',
  supplierName: '',
  contactPerson: '',
  phone: '',
  email: '',
  address: ''
})

const fetchCategories = async () => {
  loadingCats.value = true
  try {
    const res = await categoriesApi.getAll()
    categories.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải loại thiết bị', err.message, 'error')
  } finally {
    loadingCats.value = false
  }
}

const fetchSuppliers = async () => {
  loadingSups.value = true
  try {
    const res = await suppliersApi.getAll()
    suppliers.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải nhà cung cấp', err.message, 'error')
  } finally {
    loadingSups.value = false
  }
}

// Category CRUD
const openCreateCatModal = () => {
  catForm.categoryCode = ''
  catForm.categoryName = ''
  catForm.description = ''
  isCatModalOpen.value = true
}

const submitCreateCategory = async () => {
  submitting.value = true
  try {
    await categoriesApi.create(catForm)
    toastRef.value?.addToast('Thành công', 'Đã thêm phân loại thiết bị mới!', 'success')
    isCatModalOpen.value = false
    fetchCategories()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tạo phân loại', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const openEditCatModal = (cat) => {
  editCatForm.categoryID = cat.categoryID
  editCatForm.categoryCode = cat.categoryCode
  editCatForm.categoryName = cat.categoryName
  editCatForm.description = cat.description || ''
  isEditCatOpen.value = true
}

const submitUpdateCategory = async () => {
  submitting.value = true
  try {
    await categoriesApi.update(editCatForm.categoryID, editCatForm)
    toastRef.value?.addToast('Thành công', 'Đã cập nhật loại thiết bị!', 'success')
    isEditCatOpen.value = false
    fetchCategories()
  } catch (err) {
    toastRef.value?.addToast('Lỗi cập nhật', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const deleteCatItem = async (cat) => {
  if (cat.assetCount > 0) {
    toastRef.value?.addToast('Cảnh báo', `Không thể xóa loại "${cat.categoryName}" đang có ${cat.assetCount} thiết bị trực thuộc!`, 'error')
    return
  }
  if (!confirm(`Bạn có chắc muốn xóa loại thiết bị "${cat.categoryName} (${cat.categoryCode})" không?`)) {
    return
  }
  try {
    await categoriesApi.delete(cat.categoryID)
    toastRef.value?.addToast('Thành công', `Đã xóa loại ${cat.categoryName}!`, 'success')
    fetchCategories()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa loại thiết bị', err.message, 'error')
  }
}

// Supplier CRUD
const openCreateSupModal = () => {
  supForm.supplierCode = ''
  supForm.supplierName = ''
  supForm.contactPerson = ''
  supForm.phone = ''
  supForm.email = ''
  supForm.address = ''
  isSupModalOpen.value = true
}

const submitCreateSupplier = async () => {
  submitting.value = true
  try {
    await suppliersApi.create(supForm)
    toastRef.value?.addToast('Thành công', 'Đã thêm nhà cung cấp mới!', 'success')
    isSupModalOpen.value = false
    fetchSuppliers()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tạo nhà cung cấp', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const openEditSupModal = (sup) => {
  editSupForm.supplierID = sup.supplierID
  editSupForm.supplierCode = sup.supplierCode
  editSupForm.supplierName = sup.supplierName
  editSupForm.contactPerson = sup.contactPerson || ''
  editSupForm.phone = sup.phone || ''
  editSupForm.email = sup.email || ''
  editSupForm.address = sup.address || ''
  isEditSupOpen.value = true
}

const submitUpdateSupplier = async () => {
  submitting.value = true
  try {
    await suppliersApi.update(editSupForm.supplierID, editSupForm)
    toastRef.value?.addToast('Thành công', 'Đã cập nhật nhà cung cấp!', 'success')
    isEditSupOpen.value = false
    fetchSuppliers()
  } catch (err) {
    toastRef.value?.addToast('Lỗi cập nhật', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const deleteSupItem = async (sup) => {
  if (sup.suppliedAssetCount > 0) {
    toastRef.value?.addToast('Cảnh báo', `Không thể xóa nhà cung cấp "${sup.supplierName}" đang có ${sup.suppliedAssetCount} máy liên kết!`, 'error')
    return
  }
  if (!confirm(`Bạn có chắc muốn xóa nhà cung cấp "${sup.supplierName} (${sup.supplierCode})" không?`)) {
    return
  }
  try {
    await suppliersApi.delete(sup.supplierID)
    toastRef.value?.addToast('Thành công', `Đã xóa nhà cung cấp ${sup.supplierName}!`, 'success')
    fetchSuppliers()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa nhà cung cấp', err.message, 'error')
  }
}

onMounted(() => {
  fetchCategories()
  fetchSuppliers()
})
</script>

<style scoped>
.cat-sup-page {
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

.top-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.tab-toggle {
  display: flex;
  background: #f1f5f9;
  padding: 3px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.tab-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-family: inherit;
  font-size: 0.825rem;
  font-weight: 600;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  transition: var(--transition);
}

.tab-btn.active {
  background: #ffffff;
  color: var(--primary);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.item-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.card-top-right {
  display: flex;
  align-items: center;
  gap: 6px;
}

.code-badge {
  font-family: monospace;
  font-size: 0.8rem;
  font-weight: 800;
  color: #4f46e5;
  background: #eef2ff;
  padding: 3px 10px;
  border-radius: 4px;
  border: 1px solid #c7d2fe;
}

.count-pill {
  font-size: 0.775rem;
  color: #4f46e5;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 999px;
  font-weight: 600;
}

.btn-icon-mini {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  color: var(--text-dim);
  width: 26px;
  height: 26px;
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

.item-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: #0f172a;
}

.item-desc {
  font-size: 0.825rem;
  color: var(--text-dim);
  margin-top: 4px;
  line-height: 1.4;
  min-height: 38px;
}

.sup-meta-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid var(--border-color);
}

.meta-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.825rem;
}

.meta-label {
  color: var(--text-dim);
}

.meta-value {
  color: #0f172a;
  font-weight: 600;
}

.table-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 0;
  gap: 12px;
  color: var(--text-dim);
}

.modal-actions-right {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}
</style>
