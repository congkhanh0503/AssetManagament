<template>
  <Modal 
    :is-open="isOpen" 
    title="Import Danh Sách Cấp Phát & Bàn Giao Thiết Bị"
    subtitle="Nạp danh sách phân bổ thiết bị cho nhân sự, tự động gán máy và tạo lịch sử bàn giao"
    @close="handleClose"
    max-width="1000px"
    :z-index="10050"
  >
    <div class="handover-import-content">
      <!-- 1. TẢI FILE MẪU CHUẨN -->
      <div class="template-bar">
        <div>
          <strong class="template-title">📋 File mẫu Cấp phát & Bàn giao (Laptop, PC, AIO)</strong>
          <p class="template-sub">
            Bao gồm các cột: Computer Name, S/N, Mã tài sản (VNIT#), Mã NV, Họ tên, Bộ phận, Tình trạng (ĐÃ CẤP / SPARE)...
          </p>
        </div>
        <div class="template-actions">
          <button type="button" class="btn btn-sm btn-primary" @click="downloadTemplate(true)">
            📥 Tải Mẫu Excel (.XLSX)
          </button>
          <button type="button" class="btn btn-sm btn-secondary" @click="downloadTemplate(false)">
            📄 Tải .CSV
          </button>
        </div>
      </div>

      <!-- 2. KHU VỰC KÉO THẢ / CHỌN FILE -->
      <div 
        class="dropzone-box"
        :class="{ 'is-dragging': isDragging, 'has-file': Boolean(selectedFile) }"
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @drop.prevent="handleDrop"
        @click="triggerFileInput"
      >
        <input 
          ref="fileInputRef" 
          type="file" 
          accept=".xlsx, .xls, .csv" 
          style="display: none;" 
          @change="handleFileChange"
        />

        <div v-if="!selectedFile" class="dropzone-placeholder">
          <div class="dropzone-icon">📁</div>
          <div class="dropzone-text">
            <strong>Kéo thả file Excel cấp phát vào đây hoặc bấm để chọn file</strong>
            <span class="text-dim">Hỗ trợ các định dạng .xlsx, .xls, .csv (Ví dụ: danh-sách-cấp-phát.xlsx)</span>
          </div>
        </div>

        <div v-else class="selected-file-info" @click.stop>
          <div class="file-icon-badge">📊</div>
          <div class="file-details">
            <strong class="file-name">{{ selectedFile.name }}</strong>
            <span class="file-size text-dim">{{ (selectedFile.size / 1024).toFixed(1) }} KB</span>
          </div>
          <button type="button" class="btn-remove-file" @click="clearFile" title="Chọn file khác">
            ✕
          </button>
        </div>
      </div>

      <!-- Trạng thái đang đọc file -->
      <div v-if="isParsing" class="parsing-loader">
        <span class="spinner-small"></span> Đang đọc và phân tích cấu trúc file Excel...
      </div>

      <!-- 3. KHUNG PREVIEW ĐỐI CHIẾU DỮ LIỆU -->
      <template v-if="previewItems.length > 0 && !isParsing">
        <!-- Thanh tóm tắt số liệu -->
        <div class="summary-cards-row">
          <div class="summary-card total">
            <span class="num">{{ summary.total }}</span>
            <span class="lbl">Tổng thiết bị</span>
          </div>
          <div class="summary-card assigned">
            <span class="num">{{ summary.assigned }}</span>
            <span class="lbl">Đã cấp cho nhân sự</span>
          </div>
          <div class="summary-card spare">
            <span class="num">{{ summary.spare }}</span>
            <span class="lbl">Dự phòng kho (SPARE)</span>
          </div>
          <div v-if="summary.invalid > 0" class="summary-card invalid">
            <span class="num">{{ summary.invalid }}</span>
            <span class="lbl">Dòng bỏ qua / Thiếu mã</span>
          </div>
        </div>

        <!-- Bộ lọc tab preview -->
        <div class="preview-filter-tabs">
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'all' }"
            @click="previewFilter = 'all'"
          >
            Tất cả ({{ previewItems.length }})
          </button>
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'assigned' }"
            @click="previewFilter = 'assigned'"
          >
            Đã cấp ({{ previewItems.filter(i => !i.isSpare).length }})
          </button>
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'spare' }"
            @click="previewFilter = 'spare'"
          >
            SPARE Kho ({{ previewItems.filter(i => i.isSpare).length }})
          </button>
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'auto' }"
            @click="previewFilter = 'auto'"
          >
            Line tự động ({{ previewItems.filter(i => isAutoLine(i)).length }})
          </button>
        </div>

        <!-- Bảng danh sách Preview -->
        <div class="preview-table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th style="width: 50px; text-align: center;">STT</th>
                <th>Computer Name</th>
                <th>S/N (Serial)</th>
                <th>Mã Tài Sản</th>
                <th>Mã NV Nhận</th>
                <th>Họ & Tên</th>
                <th>Bộ Phận</th>
                <th>Tình Trạng</th>
                <th>Account AD</th>
              </tr>
            </thead>
            <tbody>
              <tr 
                v-for="(item, idx) in filteredPreviewItems.slice(0, 100)" 
                :key="idx"
                :class="{ 'row-spare': item.isSpare, 'row-auto': isAutoLine(item) }"
              >
                <td style="text-align: center; color: var(--text-dim);">{{ idx + 1 }}</td>
                <td>
                  <strong class="font-mono text-primary">{{ item.assetCode || '---' }}</strong>
                </td>
                <td class="font-mono">{{ item.serialNumber || '---' }}</td>
                <td>
                  <span v-if="item.materialCode" class="tag-material">{{ item.materialCode }}</span>
                  <span v-else class="text-dim">---</span>
                </td>
                <td>
                  <strong v-if="item.employeeCode" class="font-mono" :class="{ 'text-warning': isAutoLine(item) }">
                    {{ item.employeeCode }}
                  </strong>
                  <span v-else class="text-dim">---</span>
                </td>
                <td>
                  <span>{{ item.employeeName || (item.isSpare ? 'Kho dự phòng' : '---') }}</span>
                </td>
                <td>
                  <span class="dept-badge">{{ item.departmentName || '---' }}</span>
                </td>
                <td>
                  <span 
                    class="status-tag"
                    :class="item.isSpare ? 'status-spare' : 'status-assigned'"
                  >
                    {{ item.status }}
                  </span>
                </td>
                <td class="font-mono text-dim" style="font-size: 0.8rem;">
                  {{ item.accountAd || '---' }}
                </td>
              </tr>
            </tbody>
          </table>

          <div v-if="filteredPreviewItems.length > 100" class="preview-overflow-notice">
            <span>... và {{ filteredPreviewItems.length - 100 }} dòng khác đang sẵn sàng nạp</span>
          </div>
        </div>
      </template>

      <!-- 4. KẾT QUẢ IMPORT NẾU HOÀN THÀNH -->
      <div v-if="importResult" class="import-result-box">
        <div class="result-header">
          <span class="icon">🎉</span>
          <h4>Hoàn Tất Cấp Phát Bàn Giao Thiết Bị!</h4>
        </div>
        <div class="result-stats">
          <div class="stat-item">
            <span>Tổng xử lý:</span>
            <strong>{{ importResult.totalRows }}</strong>
          </div>
          <div class="stat-item text-success">
            <span>Đã cấp phát thành công:</span>
            <strong>{{ importResult.assignedCount }}</strong>
          </div>
          <div class="stat-item text-primary">
            <span>Cập nhật SPARE kho:</span>
            <strong>{{ importResult.spareCount }}</strong>
          </div>
          <div v-if="importResult.skippedCount > 0" class="stat-item text-warning">
            <span>Bỏ qua / Lỗi:</span>
            <strong>{{ importResult.skippedCount }}</strong>
          </div>
        </div>

        <div v-if="importResult.errors && importResult.errors.length > 0" class="result-errors">
          <strong>Lưu ý:</strong>
          <ul>
            <li v-for="(err, eIdx) in importResult.errors.slice(0, 10)" :key="eIdx">{{ err }}</li>
            <li v-if="importResult.errors.length > 10">... và {{ importResult.errors.length - 10 }} lưu ý khác</li>
          </ul>
        </div>
      </div>

      <!-- FOOTER NÚT THAO TÁC -->
      <div class="modal-footer-actions">
        <button type="button" class="btn btn-secondary" @click="handleClose">
          {{ importResult ? 'Đóng' : 'Hủy Bỏ' }}
        </button>

        <button 
          v-if="!importResult"
          type="button" 
          class="btn btn-primary"
          :disabled="previewItems.length === 0 || isSubmitting || isParsing"
          @click="submitHandoverImport"
        >
          <span v-if="isSubmitting" class="spinner-small"></span>
          <span v-else>🚀 Xác Nhận Cấp Phát ({{ previewItems.length }} thiết bị)</span>
        </button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed } from 'vue'
import Modal from '@/components/common/Modal.vue'
import { parseHandoverImportExcel, downloadHandoverTemplate } from '@/utils/excelImport'
import { assetsApi } from '@/api/client'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'success'])

const fileInputRef = ref(null)
const selectedFile = ref(null)
const isDragging = ref(false)
const isParsing = ref(false)
const isSubmitting = ref(false)
const previewItems = ref([])
const summary = ref({ total: 0, assigned: 0, spare: 0, invalid: 0 })
const previewFilter = ref('all')
const importResult = ref(null)

const triggerFileInput = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click()
  }
}

const handleFileChange = (e) => {
  const files = e.target.files
  if (files && files.length > 0) {
    processFile(files[0])
  }
}

const handleDrop = (e) => {
  isDragging.value = false
  const files = e.dataTransfer.files
  if (files && files.length > 0) {
    processFile(files[0])
  }
}

const processFile = async (file) => {
  selectedFile.value = file
  importResult.value = null
  isParsing.value = true

  try {
    const res = await parseHandoverImportExcel(file)
    previewItems.value = res.items || []
    summary.value = res.summary || { total: 0, assigned: 0, spare: 0, invalid: 0 }
  } catch (err) {
    console.error('Lỗi khi đọc file Excel:', err)
    alert('Không thể đọc file: ' + (err.message || 'File không đúng định dạng'))
    clearFile()
  } finally {
    isParsing.value = false
  }
}

const clearFile = () => {
  selectedFile.value = null
  previewItems.value = []
  summary.value = { total: 0, assigned: 0, spare: 0, invalid: 0 }
  importResult.value = null
  if (fileInputRef.value) fileInputRef.value.value = ''
}

const isAutoLine = (item) => {
  const code = (item.employeeCode || '').toUpperCase()
  return code === 'AUTO' || code === 'RICHARD' || (!item.employeeCode && !item.isSpare)
}

const filteredPreviewItems = computed(() => {
  if (previewFilter.value === 'assigned') {
    return previewItems.value.filter(i => !i.isSpare)
  }
  if (previewFilter.value === 'spare') {
    return previewItems.value.filter(i => i.isSpare)
  }
  if (previewFilter.value === 'auto') {
    return previewItems.value.filter(i => isAutoLine(i))
  }
  return previewItems.value
})

const downloadTemplate = (asXlsx) => {
  downloadHandoverTemplate(asXlsx)
}

const submitHandoverImport = async () => {
  if (previewItems.value.length === 0) return
  isSubmitting.value = true

  try {
    const payload = previewItems.value.map(item => ({
      assetCode: item.assetCode,
      serialNumber: item.serialNumber,
      materialCode: item.materialCode,
      employeeCode: item.employeeCode,
      employeeName: item.employeeName,
      departmentName: item.departmentName,
      status: item.status,
      accountAd: item.accountAd,
      specifications: item.specifications,
      note: item.isSpare ? 'Máy dự phòng trong kho' : 'Cấp phát theo danh sách phân bổ'
    }))

    const res = await assetsApi.importHandover(payload)
    importResult.value = res.data || res

    emit('success', importResult.value)
  } catch (err) {
    console.error('Lỗi khi import cấp phát:', err)
    alert('Lỗi import: ' + (err.response?.data?.message || err.message || 'Không thể cấp phát'))
  } finally {
    isSubmitting.value = false
  }
}

const handleClose = () => {
  if (importResult.value) {
    clearFile()
  }
  emit('close')
}
</script>

<style scoped>
.handover-import-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Template bar */
.template-bar {
  background: var(--bg-hover);
  border: 1px solid var(--border-color);
  padding: 12px 16px;
  border-radius: 8px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.template-title {
  font-size: 0.95rem;
  color: var(--text-color);
}

.template-sub {
  font-size: 0.8rem;
  color: var(--text-dim);
  margin: 2px 0 0 0;
}

.template-actions {
  display: flex;
  gap: 8px;
}

/* Dropzone */
.dropzone-box {
  border: 2px dashed var(--border-color);
  background: var(--card-bg);
  border-radius: 12px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.dropzone-box:hover,
.dropzone-box.is-dragging {
  border-color: #4f46e5;
  background: rgba(79, 70, 229, 0.04);
}

.dropzone-box.has-file {
  border-style: solid;
  border-color: #10b981;
  background: rgba(16, 185, 129, 0.04);
  padding: 14px 20px;
}

.dropzone-icon {
  font-size: 2.2rem;
  margin-bottom: 6px;
}

.dropzone-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.selected-file-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.file-icon-badge {
  font-size: 1.8rem;
}

.file-details {
  flex: 1;
  text-align: left;
  display: flex;
  flex-direction: column;
}

.file-name {
  font-size: 0.95rem;
  color: var(--text-color);
}

.btn-remove-file {
  background: transparent;
  border: none;
  font-size: 1.2rem;
  color: var(--text-dim);
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 4px;
}

.btn-remove-file:hover {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

/* Loader */
.parsing-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px;
  color: #4f46e5;
  font-weight: 500;
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid currentColor;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
  display: inline-block;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Summary cards */
.summary-cards-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
}

.summary-card {
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background: var(--card-bg);
  display: flex;
  flex-direction: column;
}

.summary-card .num {
  font-size: 1.4rem;
  font-weight: bold;
}

.summary-card .lbl {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.summary-card.total .num { color: var(--text-color); }
.summary-card.assigned .num { color: #10b981; }
.summary-card.spare .num { color: #4f46e5; }
.summary-card.invalid .num { color: #ef4444; }

/* Filter tabs */
.preview-filter-tabs {
  display: flex;
  gap: 6px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 6px;
}

.tab-btn {
  background: transparent;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 0.85rem;
  color: var(--text-dim);
  cursor: pointer;
  transition: all 0.15s ease;
}

.tab-btn:hover {
  background: var(--bg-hover);
  color: var(--text-color);
}

.tab-btn.active {
  background: #4f46e5;
  color: #ffffff;
  font-weight: 500;
}

/* Preview table */
.preview-table-container {
  max-height: 380px;
  overflow-y: auto;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.tag-material {
  background: rgba(99, 102, 241, 0.1);
  color: #4f46e5;
  padding: 2px 6px;
  border-radius: 4px;
  font-family: monospace;
  font-size: 0.8rem;
  font-weight: bold;
}

.dept-badge {
  font-size: 0.8rem;
  background: var(--bg-hover);
  padding: 2px 6px;
  border-radius: 4px;
}

.status-tag {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 12px;
}

.status-assigned {
  background: rgba(16, 185, 129, 0.15);
  color: #059669;
}

.status-spare {
  background: rgba(79, 70, 229, 0.15);
  color: #4f46e5;
}

.row-spare {
  background: rgba(79, 70, 229, 0.02);
}

.row-auto {
  background: rgba(245, 158, 11, 0.04);
}

.preview-overflow-notice {
  text-align: center;
  padding: 8px;
  font-size: 0.8rem;
  color: var(--text-dim);
  background: var(--bg-hover);
  border-top: 1px solid var(--border-color);
}

/* Result box */
.import-result-box {
  background: rgba(16, 185, 129, 0.06);
  border: 1px solid #10b981;
  border-radius: 10px;
  padding: 16px;
}

.result-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
}

.result-header .icon {
  font-size: 1.4rem;
}

.result-header h4 {
  margin: 0;
  color: #059669;
}

.result-stats {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: 12px;
}

.stat-item {
  display: flex;
  gap: 6px;
  font-size: 0.9rem;
}

.result-errors {
  font-size: 0.8rem;
  color: #ef4444;
  background: rgba(239, 68, 68, 0.05);
  padding: 8px 12px;
  border-radius: 6px;
}

.result-errors ul {
  margin: 4px 0 0 16px;
  padding: 0;
}

.modal-footer-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 10px;
  padding-top: 12px;
  border-top: 1px solid var(--border-color);
}
</style>
