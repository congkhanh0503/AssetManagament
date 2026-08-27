<template>
  <Modal 
    :is-open="isOpen" 
    title="Import Danh Sách Thiết Bị Từ File Excel / CSV"
    subtitle="Nạp hàng loạt thiết bị vào kho nhanh chóng từ file bảng tính Excel/CSV"
    @close="$emit('close')"
    max-width="900px"
  >
    <div class="import-modal-content">
      <!-- BỘ CHUYỂN TAB CHẾ ĐỘ IMPORT -->
      <div class="import-mode-tabs">
        <button 
          type="button" 
          class="import-mode-tab-btn" 
          :class="{ active: importAssetTab === 'computer' }"
          @click="switchImportTab('computer')"
        >
          <span class="tab-icon">💻</span>
          <div class="tab-texts">
            <span class="tab-main-title">Import Laptop & Máy Tính (PC)</span>
            <span class="tab-sub-title">Bao gồm CPU, RAM, Disk, OS, Display, Sạc...</span>
          </div>
        </button>

        <button 
          type="button" 
          class="import-mode-tab-btn" 
          :class="{ active: importAssetTab === 'peripheral' }"
          @click="switchImportTab('peripheral')"
        >
          <span class="tab-icon">🖱️</span>
          <div class="tab-texts">
            <span class="tab-main-title">Import Thiết Bị Ngoại Vi & Khác</span>
            <span class="tab-sub-title">Chuột, Màn hình, Bàn phím, Máy in, Switch...</span>
          </div>
        </button>
      </div>

      <!-- Bước 1: Tải file mẫu theo từng loại -->
      <div class="template-download-box" style="margin-top: 14px;">
        <div>
          <strong v-if="importAssetTab === 'computer'">1. File mẫu cho Máy Tính (Laptop / PC)</strong>
          <strong v-else>1. File mẫu cho Thiết Bị Ngoại Vi & Khác (Chuột, Màn hình, Máy in...)</strong>
          <p class="text-dim" style="font-size: 0.8rem; margin-top: 2px;">
            {{ importAssetTab === 'computer' ? 'Mẫu chứa đầy đủ các cột thông số phần cứng máy tính chuẩn xác.' : 'Mẫu tinh gọn với các cột thông số thiết bị ngoại vi, không cần CPU/RAM.' }}
          </p>
        </div>
        
        <div style="display: flex; gap: 8px; flex-wrap: wrap;">
          <template v-if="importAssetTab === 'computer'">
            <button type="button" class="btn btn-sm btn-primary" @click="downloadComputerTemplate(true)">
              📥 Tải Mẫu Excel (.XLSX)
            </button>
            <button type="button" class="btn btn-sm btn-secondary" @click="downloadComputerTemplate(false)">
              📄 Tải .CSV
            </button>
          </template>

          <template v-else>
            <button type="button" class="btn btn-sm btn-primary" @click="downloadPeripheralTemplate(true)">
              📥 Tải Mẫu Excel (.XLSX)
            </button>
            <button type="button" class="btn btn-sm btn-secondary" @click="downloadPeripheralTemplate(false)">
              📄 Tải .CSV
            </button>
          </template>
        </div>
      </div>

      <!-- Bước 2: Kéo thả file -->
      <div 
        class="dropzone-box"
        :class="{ 'is-dragging': isDragging, 'has-file': !!file }"
        style="margin-top: 14px;"
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @drop.prevent="handleDrop"
        @click="$refs.fileInputRef.click()"
      >
        <input 
          type="file" 
          ref="fileInputRef" 
          accept=".xlsx,.xls,.csv,.txt" 
          style="display: none;" 
          @change="handleFileSelect"
        />

        <div v-if="!file" class="dropzone-content">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" style="color: #60a5fa; margin-bottom: 6px;">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
            <polyline points="17 8 12 3 7 8"></polyline>
            <line x1="12" y1="3" x2="12" y2="15"></line>
          </svg>
          <div class="dropzone-title">
            {{ importAssetTab === 'computer' ? 'Kéo thả file Excel (.xlsx, .xls) hoặc CSV Laptop/PC vào đây' : 'Kéo thả file Excel (.xlsx, .xls) hoặc CSV Thiết Bị Khác vào đây' }}
          </div>
          <div class="dropzone-sub">Hỗ trợ định dạng Excel (.xlsx, .xls) và CSV UTF-8</div>
        </div>

        <div v-else class="selected-file-info">
          <div class="file-icon-box" style="color: #60a5fa;">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
            </svg>
          </div>
          <div style="flex: 1;">
            <strong class="selected-name">{{ file.name }}</strong>
            <div class="selected-size">{{ rows.length }} dòng dữ liệu được nhận diện</div>
          </div>
          <button type="button" class="btn-remove-file" @click.stop="clearFile">✕ Chọn file khác</button>
        </div>
      </div>

      <!-- Preview Table -->
      <div v-if="rows.length > 0" class="import-preview-section">
        <div class="preview-header">
          <strong>
            Xem Trước Dữ Liệu ({{ rows.length }} bản ghi) - Chế độ: {{ importAssetTab === 'computer' ? 'Laptop & PC' : 'Thiết Bị Khác' }}
          </strong>
          <span class="badge-preview-count">Hợp lệ</span>
        </div>

        <div class="table-responsive preview-table-wrap">
          <!-- Bảng Xem Trước Cho Laptop / PC -->
          <table v-if="importAssetTab === 'computer'" class="data-table preview-table">
            <thead>
              <tr>
                <th>Mã Máy</th>
                <th>Tên Model</th>
                <th>Loại</th>
                <th>Hãng</th>
                <th>CPU</th>
                <th>RAM</th>
                <th>Disk</th>
                <th>S/N</th>
                <th>Ngày Mua</th>
                <th>Hạn BH</th>
                <th>Vị Trí Kho</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, idx) in rows.slice(0, 5)" :key="idx">
                <td><span class="code-badge-mini">{{ getRowValue(row, ['Host Name', 'Mã Máy', 'Mã Thiết Bị', 'Asset Code', 'Mã Tài Sản', 'Code']) }}</span></td>
                <td><strong>{{ getRowValue(row, ['Model', 'Tên Thiết Bị', 'Tên Máy', 'Asset Name', 'Model Name', 'Name']) }}</strong></td>
                <td>{{ getRowValue(row, ['Loại Thiết Bị', 'Loại', 'Category', 'Loại máy']) || 'Laptop' }}</td>
                <td>{{ getRowValue(row, ['Hãng', 'Thương Hiệu', 'Brand']) || '---' }}</td>
                <td>{{ getRowValue(row, ['CPU', 'Vi Xử Lý', 'Chip']) || '---' }}</td>
                <td>{{ getRowValue(row, ['RAM', 'Bộ Nhớ']) || '---' }}</td>
                <td>{{ getRowValue(row, ['Disk', 'Ổ Cứng', 'SSD', 'HDD']) || '---' }}</td>
                <td>{{ getRowValue(row, ['Service Tag', 'Số Serial', 'Serial Number', 'S/N', 'Serial']) || '---' }}</td>
                <td><span class="date-badge">{{ getRowValue(row, ['Ngày Mua', 'Purchase Date', 'Ngay Mua', 'Purchase']) || '---' }}</span></td>
                <td><span class="date-badge">{{ getRowValue(row, ['Hạn Bảo Hành', 'Warranty Expire', 'Warranty', 'Han Bao Hanh']) || '---' }}</span></td>
                <td>{{ getRowValue(row, ['Vị Trí Kho', 'Vị Trí', 'Kho', 'Warehouse Location']) || 'Kho IT - Kệ A1' }}</td>
              </tr>
            </tbody>
          </table>

          <!-- Bảng Xem Trước Cho Thiết Bị Khác -->
          <table v-else class="data-table preview-table">
            <thead>
              <tr>
                <th>Mã Thiết Bị</th>
                <th>Tên Thiết Bị</th>
                <th>Loại Thiết Bị</th>
                <th>Thương Hiệu</th>
                <th>Thông Số / Đặc Điểm</th>
                <th>Số Serial</th>
                <th>Ngày Mua</th>
                <th>Hạn BH</th>
                <th>Vị Trí Kho</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, idx) in rows.slice(0, 5)" :key="idx">
                <td><span class="code-badge-mini">{{ getRowValue(row, ['Mã Thiết Bị', 'Mã Tài Sản', 'Asset Code', 'Host Name', 'Code']) }}</span></td>
                <td><strong>{{ getRowValue(row, ['Tên Thiết Bị', 'Model', 'Tên Máy', 'Asset Name', 'Name']) }}</strong></td>
                <td>{{ getRowValue(row, ['Loại Thiết Bị', 'Loại', 'Category']) || 'Chuột' }}</td>
                <td>{{ getRowValue(row, ['Thương Hiệu', 'Hãng', 'Brand']) || '---' }}</td>
                <td><span class="text-dim" style="font-size: 0.75rem;">{{ getRowValue(row, ['Thông Số Kỹ Thuật', 'Thông số', 'Specifications', 'Cấu Hình', 'Đặc Điểm']) || '---' }}</span></td>
                <td>{{ getRowValue(row, ['Số Serial', 'Serial Number', 'S/N', 'Serial', 'Service Tag']) || '---' }}</td>
                <td><span class="date-badge">{{ getRowValue(row, ['Ngày Mua', 'Purchase Date', 'Ngay Mua', 'Purchase']) || '---' }}</span></td>
                <td><span class="date-badge">{{ getRowValue(row, ['Hạn Bảo Hành', 'Warranty Expire', 'Warranty', 'Han Bao Hanh']) || '---' }}</span></td>
                <td>{{ getRowValue(row, ['Vị Trí Kho', 'Vị Trí', 'Kho', 'Warehouse Location']) || 'Kho IT - Kệ A1' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="rows.length > 5" class="preview-more-hint">
          ... và {{ rows.length - 5 }} thiết bị khác sẽ được nạp vào kho.
        </div>
      </div>

      <div class="modal-actions-right">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Hủy</button>
        <button 
          type="button" 
          class="btn btn-primary" 
          :disabled="rows.length === 0 || submitting"
          @click="handleSubmit"
        >
          <span v-if="submitting" class="loading-spinner"></span>
          Xác Nhận Nạp {{ rows.length }} Thiết Bị Vào Kho
        </button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref } from 'vue'
import Modal from '@/components/common/Modal.vue'
import { parseSpreadsheetFile, downloadComputerTemplate, downloadPeripheralTemplate, getRowValue } from '@/utils/excelImport'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  submitting: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'import'])

const importAssetTab = ref('computer')
const file = ref(null)
const rows = ref([])
const isDragging = ref(false)
const fileInputRef = ref(null)

const switchImportTab = (tab) => {
  if (importAssetTab.value === tab) return
  importAssetTab.value = tab
  clearFile()
}

const clearFile = () => {
  file.value = null
  rows.value = []
  if (fileInputRef.value) fileInputRef.value.value = ''
}

const handleFileSelect = async (e) => {
  const f = e.target.files[0]
  if (f) await processFile(f)
}

const handleDrop = async (e) => {
  isDragging.value = false
  const f = e.dataTransfer.files[0]
  if (f) await processFile(f)
}

const processFile = async (f) => {
  try {
    const { rows: parsedRows } = await parseSpreadsheetFile(f)
    if (!parsedRows || parsedRows.length === 0) {
      alert('File không có dòng dữ liệu hợp lệ nào!')
      return
    }
    file.value = f
    rows.value = parsedRows
  } catch (err) {
    alert('Lỗi đọc file: ' + err.message)
  }
}

const handleSubmit = () => {
  if (rows.value.length === 0) return
  emit('import', {
    mode: importAssetTab.value,
    rows: rows.value,
    callback: clearFile
  })
}
</script>

<style scoped>
.import-modal-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.import-mode-tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.import-mode-tab-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  text-align: left;
  transition: all 0.2s ease;
}

.import-mode-tab-btn:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.import-mode-tab-btn.active {
  background: #eff6ff;
  border-color: #3b82f6;
}

.tab-icon {
  font-size: 1.5rem;
}

.tab-texts {
  display: flex;
  flex-direction: column;
}

.tab-main-title {
  font-weight: 700;
  font-size: 0.9rem;
  color: #1e293b;
}

.tab-sub-title {
  font-size: 0.75rem;
  color: #64748b;
}

.template-download-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 12px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.dropzone-box {
  border: 2px dashed #cbd5e1;
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
  background: #fcfdfe;
}

.dropzone-box:hover, .dropzone-box.is-dragging {
  border-color: #3b82f6;
  background: #eff6ff;
}

.dropzone-box.has-file {
  border-style: solid;
  border-color: #bfdbfe;
  background: #f0fdf4;
}

.dropzone-title {
  font-weight: 600;
  font-size: 0.9rem;
  color: #334155;
  margin-top: 4px;
}

.dropzone-sub {
  font-size: 0.775rem;
  color: #64748b;
  margin-top: 2px;
}

.selected-file-info {
  display: flex;
  align-items: center;
  gap: 12px;
  text-align: left;
}

.selected-name {
  color: #0f172a;
  font-size: 0.9rem;
}

.selected-size {
  font-size: 0.775rem;
  color: #16a34a;
  font-weight: 600;
}

.btn-remove-file {
  background: none;
  border: 1px solid #fca5a5;
  color: #dc2626;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  cursor: pointer;
}

.btn-remove-file:hover {
  background: #fee2e2;
}

.import-preview-section {
  margin-top: 10px;
}

.preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  font-size: 0.85rem;
}

.badge-preview-count {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
  font-size: 0.725rem;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 4px;
}

.preview-table-wrap {
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: 6px;
}

.preview-table th, .preview-table td {
  padding: 6px 10px;
  font-size: 0.775rem;
}

.code-badge-mini {
  font-family: monospace;
  font-weight: bold;
  background: #eef2ff;
  color: #4f46e5;
  padding: 1px 4px;
  border-radius: 3px;
}

.date-badge {
  font-size: 0.72rem;
  color: #0369a1;
  background: #e0f2fe;
  padding: 1px 5px;
  border-radius: 3px;
  white-space: nowrap;
}

.preview-more-hint {
  text-align: center;
  font-size: 0.775rem;
  color: #64748b;
  margin-top: 4px;
  font-style: italic;
}
</style>
