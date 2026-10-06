<template>
  <Modal 
    :is-open="isOpen" 
    title="Import Danh Sách Tài Khoản Nhân Viên"
    subtitle="Cập nhật hàng loạt trạng thái tài khoản AD, QAD, ngày nghỉ việc từ file Excel"
    @close="handleClose"
    max-width="1000px"
    :z-index="10050"
  >
    <div class="account-import-content">
      <!-- 1. TẢI FILE MẪU CHUẨN -->
      <div class="template-bar">
        <div>
          <strong class="template-title">📋 File mẫu Danh sách Tài khoản (AD, QAD, Trạng thái)</strong>
          <p class="template-sub">
            Cột chuẩn: Name, Status, Termination date, AD account disabled?, QAD account disabled?, Note, Next steps...
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
            <strong>Kéo thả file Excel tài khoản vào đây hoặc bấm để chọn file</strong>
            <span class="text-dim">Hỗ trợ .xlsx, .xls, .csv (Ví dụ: danhsachtaikhoan.xlsx)</span>
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

      <!-- 3. TÙY CHỌN CẤU HÌNH QUY ƯỚC IMPORT -->
      <div v-if="previewItems.length > 0 && !isParsing" class="config-settings-card">
        <div class="config-title">⚙️ Tùy chọn quy ước cập nhật</div>
        <div class="config-grid">
          <!-- Quy ước cột AD & QAD -->
          <div class="config-item">
            <label class="config-label">Quy ước giá trị 'Y' và 'N' cho cả AD & QAD:</label>
            <div class="radio-options">
              <label class="radio-label">
                <input type="radio" value="Y_Is_Available" v-model="adMappingRule" />
                <span class="radio-custom"></span>
                <span>🟢 <strong>Y = Đã kích hoạt (Available)</strong> | N = Chưa kích hoạt (Disable)</span>
              </label>
              <label class="radio-label">
                <input type="radio" value="Y_Is_Disabled" v-model="adMappingRule" />
                <span class="radio-custom"></span>
                <span>🔴 <strong>Y = Đã khóa / Vô hiệu hóa (Disable)</strong> | N = Kích hoạt</span>
              </label>
            </div>
          </div>

          <!-- Tự động cập nhật nghỉ việc -->
          <div class="config-item checkboxes">
            <label class="checkbox-label">
              <input type="checkbox" v-model="updateTerminationDate" />
              <span>Cập nhật ngày nghỉ việc nếu có (Termination date)</span>
            </label>
            <label class="checkbox-label">
              <input type="checkbox" v-model="updateResignedStatus" />
              <span>Chuyển trạng thái sang "Nghỉ việc" nếu cột Status = 'Left'</span>
            </label>
          </div>
        </div>
      </div>

      <!-- 4. KHUNG PREVIEW ĐỐI CHIẾU DỮ LIỆU -->
      <template v-if="previewItems.length > 0 && !isParsing">
        <!-- Thanh tóm tắt số liệu -->
        <div class="summary-cards-row">
          <div class="summary-card total">
            <span class="num">{{ summary.total }}</span>
            <span class="lbl">Tổng tài khoản</span>
          </div>
          <div class="summary-card active-acc">
            <span class="num">{{ summary.qadActive }}</span>
            <span class="lbl">QAD Kích hoạt (Y)</span>
          </div>
          <div class="summary-card disabled-acc">
            <span class="num">{{ summary.qadDisabled }}</span>
            <span class="lbl">QAD Chưa kích hoạt (N)</span>
          </div>
          <div v-if="summary.left > 0 || summary.hasTermDate > 0" class="summary-card left-acc">
            <span class="num">{{ summary.left || summary.hasTermDate }}</span>
            <span class="lbl">Nghỉ việc / Có ngày nghỉ</span>
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
            :class="{ active: previewFilter === 'qadActive' }"
            @click="previewFilter = 'qadActive'"
          >
            QAD Kích hoạt ({{ previewItems.filter(i => i.isQadActive).length }})
          </button>
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'qadDisabled' }"
            @click="previewFilter = 'qadDisabled'"
          >
            QAD Chưa kích hoạt ({{ previewItems.filter(i => !i.isQadActive).length }})
          </button>
          <button 
            type="button" 
            class="tab-btn" 
            :class="{ active: previewFilter === 'left' }"
            @click="previewFilter = 'left'"
          >
            Nghỉ việc / Left ({{ previewItems.filter(i => i.isLeft).length }})
          </button>
        </div>

        <!-- Bảng danh sách Preview -->
        <div class="preview-table-container">
          <table class="preview-table">
            <thead>
              <tr>
                <th style="width: 50px; text-align: center;">#</th>
                <th style="min-width: 170px;">Tên trong Excel</th>
                <th style="min-width: 120px; text-align: center;">Trạng thái NV</th>
                <th style="min-width: 130px; text-align: center;">Ngày nghỉ việc</th>
                <th style="min-width: 130px; text-align: center;">Tài khoản AD</th>
                <th style="min-width: 130px; text-align: center;">Tài khoản QAD</th>
                <th style="min-width: 200px;">Ghi chú (Note)</th>
              </tr>
            </thead>
            <tbody>
              <tr 
                v-for="item in filteredItems" 
                :key="item.rowIndex"
                :class="{ 'row-left': item.isLeft }"
              >
                <td style="text-align: center;" class="text-dim">{{ item.rowIndex }}</td>
                <td>
                  <div class="name-cell">
                    <strong class="raw-name">{{ item.rawName }}</strong>
                    <span v-if="item.englishName" class="en-tag">({{ item.englishName }})</span>
                  </div>
                </td>
                <td style="text-align: center;">
                  <span 
                    class="status-pill"
                    :class="item.isLeft ? 'status-left' : 'status-active'"
                  >
                    {{ item.status }}
                  </span>
                </td>
                <td style="text-align: center;">
                  <span v-if="item.terminationDate" class="date-badge">
                    {{ formatDate(item.terminationDate) }}
                  </span>
                  <span v-else class="text-dim">---</span>
                </td>
                <td style="text-align: center;">
                  <span 
                    class="acc-badge"
                    :class="mappedAdStatus === 'Available' ? 'badge-available' : 'badge-disabled'"
                  >
                    {{ mappedAdStatus }}
                  </span>
                </td>
                <td style="text-align: center;">
                  <span 
                    class="acc-badge"
                    :class="item.isQadActive ? 'badge-available' : 'badge-disabled'"
                  >
                    {{ item.isQadActive ? 'Available' : 'Disable' }}
                  </span>
                </td>
                <td>
                  <span v-if="item.note" class="note-text" :title="item.note">{{ item.note }}</span>
                  <span v-else class="text-dim">---</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>

      <!-- 5. KẾT QUẢ IMPORT THÀNH CÔNG -->
      <div v-if="importResult" class="import-result-card">
        <div class="result-icon-box">🎉</div>
        <div class="result-details">
          <h4>Cập Nhật Trạng Thái Tài Khoản Thành Công!</h4>
          <p class="result-summary-text">
            Đã khớp và cập nhật thành công <strong>{{ importResult.updatedCount }}</strong> / {{ importResult.totalRows }} tài khoản nhân sự vào hệ thống.
          </p>
          <div v-if="importResult.notFoundCount > 0" class="not-found-warning">
            ⚠️ Có <strong>{{ importResult.notFoundCount }}</strong> tài khoản trong file Excel chưa khớp với nhân sự trong hệ thống:
            <div class="not-found-tags">
              <span v-for="(n, idx) in importResult.notFoundNames.slice(0, 15)" :key="idx" class="nf-tag">{{ n }}</span>
              <span v-if="importResult.notFoundNames.length > 15" class="nf-tag more">+{{ importResult.notFoundNames.length - 15 }} tài khoản khác</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL FOOTER -->
    <template #footer>
      <div class="modal-footer-actions">
        <button type="button" class="btn btn-secondary" @click="handleClose">
          {{ importResult ? 'Đóng' : 'Hủy bỏ' }}
        </button>

        <button 
          v-if="previewItems.length > 0 && !importResult"
          type="button" 
          class="btn btn-primary btn-submit-import"
          :disabled="submitting || isParsing"
          @click="handleSubmit"
        >
          <span v-if="submitting" class="spinner-small"></span>
          <span>🚀 Xác Nhận Cập Nhật ({{ previewItems.length }} Tài Khoản)</span>
        </button>
      </div>
    </template>
  </Modal>
</template>

<script setup>
import { ref, computed } from 'vue'
import Modal from '@/components/common/Modal.vue'
import { parseAccountImportExcel, downloadAccountTemplate } from '@/utils/excelImport'
import { employeesApi } from '@/api/client'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'refresh'])

// File & State
const fileInputRef = ref(null)
const selectedFile = ref(null)
const isDragging = ref(false)
const isParsing = ref(false)
const submitting = ref(false)
const previewItems = ref([])
const summary = ref({ total: 0, active: 0, left: 0, qadActive: 0, qadDisabled: 0, hasTermDate: 0 })
const previewFilter = ref('all')
const importResult = ref(null)

// Config Settings
const adMappingRule = ref('Y_Is_Available') // 'Y_Is_Available' hoặc 'Y_Is_Disabled'
const updateTerminationDate = ref(true)
const updateResignedStatus = ref(true)

const mappedAdStatus = computed(() => {
  return adMappingRule.value === 'Y_Is_Available' ? 'Available' : 'Disable'
})

// Trigger chọn file
const triggerFileInput = () => {
  if (fileInputRef.value) {
    fileInputRef.value.click()
  }
}

// Xử lý kéo thả
const handleDrop = async (e) => {
  isDragging.value = false
  const files = e.dataTransfer.files
  if (files && files.length > 0) {
    await processFile(files[0])
  }
}

// Xử lý chọn file từ input
const handleFileChange = async (e) => {
  const files = e.target.files
  if (files && files.length > 0) {
    await processFile(files[0])
  }
}

// Phân tích file
const processFile = async (file) => {
  selectedFile.value = file
  isParsing.value = true
  importResult.value = null
  try {
    const { items, summary: s } = await parseAccountImportExcel(file)
    previewItems.value = items
    summary.value = s
  } catch (err) {
    console.error('Lỗi khi đọc file Excel tài khoản:', err)
    alert('Không thể đọc file Excel. Vui lòng kiểm tra định dạng file: ' + (err.message || ''))
  } finally {
    isParsing.value = false
  }
}

// Xóa file chọn lại
const clearFile = () => {
  selectedFile.value = null
  previewItems.value = []
  importResult.value = null
  summary.value = { total: 0, active: 0, left: 0, qadActive: 0, qadDisabled: 0, hasTermDate: 0 }
  if (fileInputRef.value) fileInputRef.value.value = ''
}

// Tải file mẫu
const downloadTemplate = (asXlsx) => {
  downloadAccountTemplate(asXlsx)
}

// Lọc dữ liệu xem trước
const filteredItems = computed(() => {
  if (previewFilter.value === 'qadActive') {
    return previewItems.value.filter(i => i.isQadActive)
  }
  if (previewFilter.value === 'qadDisabled') {
    return previewItems.value.filter(i => !i.isQadActive)
  }
  if (previewFilter.value === 'left') {
    return previewItems.value.filter(i => i.isLeft)
  }
  return previewItems.value
})

const formatDate = (d) => {
  if (!d) return ''
  const date = new Date(d)
  if (isNaN(date.getTime())) return d
  return date.toLocaleDateString('vi-VN')
}

// Đóng modal
const handleClose = () => {
  if (importResult.value) {
    emit('refresh')
  }
  clearFile()
  emit('close')
}

// Gửi dữ liệu cập nhật
const handleSubmit = async () => {
  if (previewItems.value.length === 0) return
  submitting.value = true
  try {
    const payload = {
      items: previewItems.value.map(i => ({
        rawName: i.rawName,
        status: i.status,
        terminationDate: i.terminationDate ? String(i.terminationDate).trim() : null,
        adDisabled: i.adDisabled,
        qadDisabled: i.qadDisabled,
        note: i.note,
        nextSteps: i.nextSteps
      })),
      adMappingRule: adMappingRule.value,
      updateTerminationDate: updateTerminationDate.value,
      updateResignedStatus: updateResignedStatus.value
    }

    const res = await employeesApi.importAccounts(payload)
    importResult.value = res.data || res
    emit('refresh')
  } catch (err) {
    console.error('Lỗi cập nhật tài khoản:', err)
    alert('Có lỗi xảy ra khi cập nhật tài khoản: ' + (err.response?.data?.message || err.message))
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.account-import-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: calc(85vh - 120px);
  overflow-y: auto;
  padding-right: 4px;
}

/* Template bar */
.template-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(79, 70, 229, 0.05);
  border: 1px solid rgba(79, 70, 229, 0.15);
  border-radius: 10px;
  padding: 12px 16px;
  gap: 16px;
}

.template-title {
  color: #4338ca;
  font-size: 0.95rem;
}

.template-sub {
  color: #64748b;
  font-size: 0.82rem;
  margin: 2px 0 0 0;
}

.template-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

/* Dropzone */
.dropzone-box {
  border: 2px dashed #cbd5e1;
  border-radius: 12px;
  padding: 24px 16px;
  text-align: center;
  cursor: pointer;
  background: #f8fafc;
  transition: all 0.2s ease;
}

.dropzone-box:hover, .dropzone-box.is-dragging {
  border-color: #4f46e5;
  background: rgba(79, 70, 229, 0.04);
}

.dropzone-box.has-file {
  padding: 14px 18px;
  background: #ffffff;
  border-style: solid;
  border-color: #e2e8f0;
}

.dropzone-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.dropzone-icon {
  font-size: 2.2rem;
}

.dropzone-text strong {
  display: block;
  font-size: 0.95rem;
  color: #1e293b;
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
}

.file-name {
  display: block;
  font-size: 0.95rem;
  color: #0f172a;
}

.file-size {
  font-size: 0.8rem;
}

.btn-remove-file {
  background: none;
  border: none;
  color: #94a3b8;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
}

.btn-remove-file:hover {
  color: #ef4444;
  background: #fee2e2;
}

.parsing-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #4f46e5;
  font-weight: 500;
  padding: 12px;
}

/* Config settings card */
.config-settings-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 14px 16px;
}

.config-title {
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 12px;
  font-size: 0.92rem;
}

.config-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

@media (max-width: 768px) {
  .config-grid {
    grid-template-columns: 1fr;
  }
}

.config-label {
  display: block;
  font-size: 0.82rem;
  font-weight: 600;
  color: #475569;
  margin-bottom: 8px;
}

.radio-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  color: #334155;
  cursor: pointer;
}

.checkboxes {
  display: flex;
  flex-direction: column;
  gap: 10px;
  justify-content: center;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  color: #334155;
  cursor: pointer;
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
  display: flex;
  flex-direction: column;
  border: 1px solid #e2e8f0;
  background: #ffffff;
}

.summary-card .num {
  font-size: 1.3rem;
  font-weight: 700;
  color: #0f172a;
}

.summary-card .lbl {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 2px;
}

.summary-card.active-acc {
  background: #f0fdf4;
  border-color: #bbf7d0;
}
.summary-card.active-acc .num {
  color: #16a34a;
}

.summary-card.disabled-acc {
  background: #fef2f2;
  border-color: #fecaca;
}
.summary-card.disabled-acc .num {
  color: #dc2626;
}

.summary-card.left-acc {
  background: #fff7ed;
  border-color: #fed7aa;
}
.summary-card.left-acc .num {
  color: #ea580c;
}

/* Filter tabs */
.preview-filter-tabs {
  display: flex;
  gap: 8px;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 8px;
  overflow-x: auto;
}

.tab-btn {
  background: none;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 0.85rem;
  color: #64748b;
  cursor: pointer;
  white-space: nowrap;
  font-weight: 500;
}

.tab-btn:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.tab-btn.active {
  background: #4f46e5;
  color: #ffffff;
}

/* Preview Table */
.preview-table-container {
  max-height: 380px;
  overflow-y: auto;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #ffffff;
}

.preview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.preview-table th {
  position: sticky;
  top: 0;
  background: #f8fafc;
  color: #475569;
  font-weight: 600;
  padding: 10px 12px;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
  z-index: 1;
}

.preview-table td {
  padding: 8px 12px;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
  vertical-align: middle;
}

.preview-table tr:hover td {
  background: #f8fafc;
}

.preview-table tr.row-left td {
  background: #fffbf5;
}

.name-cell {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.raw-name {
  color: #0f172a;
}

.en-tag {
  color: #4f46e5;
  font-size: 0.8rem;
  font-weight: 500;
}

.status-pill {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-active {
  background: #dcfce7;
  color: #15803d;
}

.status-left {
  background: #fee2e2;
  color: #b91c1c;
}

.date-badge {
  font-family: monospace;
  font-size: 0.8rem;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 4px;
}

.acc-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.badge-available {
  background: #dcfce7;
  color: #15803d;
}

.badge-disabled {
  background: #f1f5f9;
  color: #64748b;
}

.note-text {
  display: inline-block;
  max-width: 220px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #475569;
}

/* Result Card */
.import-result-card {
  display: flex;
  gap: 16px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 10px;
  padding: 16px;
}

.result-icon-box {
  font-size: 2rem;
}

.result-details h4 {
  margin: 0 0 4px 0;
  color: #15803d;
  font-size: 1rem;
}

.result-summary-text {
  margin: 0;
  font-size: 0.88rem;
  color: #166534;
}

.not-found-warning {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed #bbf7d0;
  font-size: 0.82rem;
  color: #991b1b;
}

.not-found-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 6px;
}

.nf-tag {
  background: #fee2e2;
  color: #991b1b;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
}

.nf-tag.more {
  background: #fecaca;
  font-weight: 600;
}

/* Modal footer */
.modal-footer-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  width: 100%;
}

.btn-submit-import {
  display: flex;
  align-items: center;
  gap: 8px;
}

.spinner-small {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.text-dim {
  color: #94a3b8;
}
</style>
