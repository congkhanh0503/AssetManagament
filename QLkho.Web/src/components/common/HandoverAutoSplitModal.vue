<template>
  <div v-if="isOpen" class="modal-backdrop" @click.self="closeModal">
    <div class="glass-modal modal-lg">
      <div class="modal-header">
        <div class="modal-title-box">
          <div class="modal-icon purple">⚡</div>
          <div>
            <h2 class="modal-title">Tự Động Nhận Diện & Tách Biên Bản PDF</h2>
            <p class="modal-subtitle">Hệ thống tự động đọc mã QR trên từng trang scan, cắt file và gán vào đúng Nhân sự & Thiết bị</p>
          </div>
        </div>
        <button type="button" class="btn-close" @click="closeModal" :disabled="processing">✕</button>
      </div>

      <div class="modal-body-scroll">
        <!-- BƯỚC 1: KHUNG TẢI FILE NẾU CHƯA CÓ KẾT QUẢ -->
        <div v-if="!resultData">
          <div 
            class="dropzone-box" 
            :class="{ 'is-dragover': isDragging, 'has-file': !!selectedFile }"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="handleDrop"
            @click="triggerFileInput"
          >
            <input 
              type="file" 
              ref="fileInputRef" 
              accept=".pdf" 
              class="hidden-file-input" 
              @change="onFileSelected" 
            />

            <div v-if="!selectedFile" class="dropzone-empty">
              <div class="dropzone-icon">📑</div>
              <div class="dropzone-main-text">Kéo thả file PDF scan tổng vào đây</div>
              <div class="dropzone-sub-text">hoặc <span>bấm để chọn file từ máy tính</span> (chấp nhận file .PDF dung lượng tối đa 50MB)</div>
            </div>

            <div v-else class="dropzone-selected">
              <div class="dropzone-icon success">📄</div>
              <div class="file-info-col">
                <div class="file-name-text">{{ selectedFile.name }}</div>
                <div class="file-size-text">{{ formatFileSize(selectedFile.size) }}</div>
              </div>
              <button type="button" class="btn-remove-file" @click.stop="clearSelectedFile" title="Chọn file khác">✕</button>
            </div>
          </div>

          <!-- GHI CHÚ QUY TRÌNH QUÉT MÃ -->
          <div class="hint-card">
            <div class="hint-title">💡 Cách thức hoạt động:</div>
            <ul class="hint-list">
              <li>Bạn có thể gom <strong>hàng chục hoặc hàng trăm tờ biên bản A4</strong> đã ký của nhiều nhân viên rồi scan thành <strong>1 file PDF duy nhất</strong>.</li>
              <li>Hệ thống sẽ <strong>đọc mã QR</strong> ở góc trên bên phải từng trang để tự động phát hiện mã nhân viên và mã thiết bị.</li>
              <li>Tự động <strong>cắt tách file PDF</strong> thành từng biên bản riêng lẻ, lưu trữ vào mục Hồ sơ và gán vào đúng tài sản.</li>
              <li>Các tài sản tương ứng sẽ được tự động chuyển sang trạng thái <strong>"✅ Đã có biên bản"</strong> (xóa cảnh báo thiếu biên bản).</li>
            </ul>
          </div>
        </div>

        <!-- BƯỚC 2: BẢNG KẾT QUẢ SAU KHI XỬ LÝ XONG -->
        <div v-else class="result-section">
          <!-- THẺ TỔNG KẾT STATS -->
          <div class="stats-banner">
            <div class="stat-box blue">
              <div class="stat-num">{{ resultData.totalPages }}</div>
              <div class="stat-lbl">Tổng số trang</div>
            </div>
            <div class="stat-box green">
              <div class="stat-num">{{ resultData.successCount }}</div>
              <div class="stat-lbl">Tách & Gán thành công</div>
            </div>
            <div class="stat-box amber" v-if="resultData.failedCount > 0">
              <div class="stat-num">{{ resultData.failedCount }}</div>
              <div class="stat-lbl">Trang không có QR / Lỗi</div>
            </div>
          </div>

          <div class="result-msg-box" :class="resultData.success ? 'success' : 'warn'">
            {{ resultData.message }}
          </div>

          <!-- BẢNG CHI TIẾT TỪNG TRANG -->
          <div class="table-responsive" style="max-height: 340px; overflow-y: auto;">
            <table class="result-table">
              <thead>
                <tr>
                  <th style="width: 70px; text-align: center;">TRANG</th>
                  <th style="width: 140px;">MÃ QR NHẬN DIỆN</th>
                  <th>NHÂN VIÊN</th>
                  <th>THIẾT BỊ</th>
                  <th style="width: 140px; text-align: center;">TRẠNG THÁI</th>
                  <th style="width: 90px; text-align: right;">FILE TÁCH</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in resultData.items" :key="item.pageNumber" :class="{ 'row-fail': !item.success }">
                  <td style="text-align: center; font-weight: 700;">
                    Trang {{ item.pageNumber }}
                  </td>
                  <td>
                    <span v-if="item.rawQrContent" class="code-pill font-mono" :title="item.rawQrContent">
                      {{ item.rawQrContent.length > 20 ? item.rawQrContent.slice(0, 18) + '...' : item.rawQrContent }}
                    </span>
                    <span v-else class="text-muted" style="font-size: 0.75rem;">Không có QR</span>
                  </td>
                  <td>
                    <div v-if="item.employeeName || item.employeeCode" class="emp-cell">
                      <strong>{{ item.employeeName || item.employeeCode }}</strong>
                      <span v-if="item.employeeCode" class="emp-code-sub">({{ item.employeeCode }})</span>
                    </div>
                    <span v-else class="text-muted">---</span>
                  </td>
                  <td>
                    <div v-if="item.assetName || item.assetCode" class="asset-cell">
                      <strong>{{ item.assetCode }}</strong>
                      <span v-if="item.assetName" class="asset-name-sub">{{ item.assetName }}</span>
                    </div>
                    <span v-else class="text-muted">---</span>
                  </td>
                  <td style="text-align: center;">
                    <span v-if="item.success" class="badge-success-mini">
                      ✅ Thành công
                    </span>
                    <span v-else class="badge-fail-mini" :title="item.message">
                      ⚠️ {{ item.message || 'Chưa nhận diện' }}
                    </span>
                  </td>
                  <td style="text-align: right;">
                    <a 
                      v-if="item.filePath" 
                      :href="item.filePath" 
                      target="_blank" 
                      class="btn-view-pdf"
                      title="Xem file PDF đã tách"
                    >
                      👁️ Xem PDF
                    </a>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- FOOTER -->
      <div class="modal-footer">
        <template v-if="!resultData">
          <button type="button" class="btn btn-secondary" @click="closeModal" :disabled="processing">
            Đóng
          </button>
          <button 
            type="button" 
            class="btn btn-primary btn-start-split" 
            :disabled="!selectedFile || processing"
            @click="startAutoSplit"
          >
            <span v-if="processing" class="spinner-mini"></span>
            <span v-else>⚡</span>
            <span>{{ processing ? 'Đang phân tích & tách PDF...' : 'Bắt Đầu Nhận Diện & Tách File' }}</span>
          </button>
        </template>
        <template v-else>
          <button type="button" class="btn btn-secondary" @click="resetToUploadAgain">
            🔄 Tải file PDF khác
          </button>
          <button type="button" class="btn btn-success" @click="finishAndClose">
            ✅ Hoàn Tất & Làm Mới Dữ Liệu
          </button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { documentsApi } from '@/api/client'

const props = defineProps({
  isOpen: { type: Boolean, default: false }
})

const emit = defineEmits(['close', 'success'])

const fileInputRef = ref(null)
const selectedFile = ref(null)
const isDragging = ref(false)
const processing = ref(false)
const resultData = ref(null)

const triggerFileInput = () => {
  if (processing.value) return
  fileInputRef.value?.click()
}

const onFileSelected = (e) => {
  const file = e.target.files?.[0]
  if (file) {
    validateAndSetFile(file)
  }
}

const handleDrop = (e) => {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) {
    validateAndSetFile(file)
  }
}

const validateAndSetFile = (file) => {
  if (!file.name.toLowerCase().endsWith('.pdf')) {
    alert('Vui lòng chỉ chọn file định dạng PDF (.pdf)!')
    return
  }
  selectedFile.value = file
}

const clearSelectedFile = () => {
  selectedFile.value = null
  if (fileInputRef.value) fileInputRef.value.value = ''
}

const formatFileSize = (bytes) => {
  if (!bytes) return '0 B'
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

const startAutoSplit = async () => {
  if (!selectedFile.value) return

  processing.value = true
  try {
    const formData = new FormData()
    formData.append('pdfFile', selectedFile.value)
    formData.append('uploadedBy', 'Auto-Scan')

    const res = await documentsApi.autoSplitHandover(formData)
    resultData.value = res.data || res
  } catch (err) {
    alert('Lỗi xử lý file PDF: ' + err.message)
  } finally {
    processing.value = false
  }
}

const resetToUploadAgain = () => {
  resultData.value = null
  selectedFile.value = null
  if (fileInputRef.value) fileInputRef.value.value = ''
}

const closeModal = () => {
  if (processing.value) return
  emit('close')
}

const finishAndClose = () => {
  emit('success')
  emit('close')
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  z-index: 10080;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.glass-modal {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 860px;
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

.modal-header {
  padding: 18px 24px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
}

.modal-title-box {
  display: flex;
  align-items: center;
  gap: 12px;
}

.modal-icon.purple {
  background: #ede9fe;
  color: #7c3aed;
  width: 42px;
  height: 42px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

.modal-title {
  font-size: 1.15rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
}

.modal-subtitle {
  font-size: 0.8rem;
  color: #64748b;
  margin: 2px 0 0 0;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.2rem;
  color: #94a3b8;
  cursor: pointer;
  padding: 4px;
}

.modal-body-scroll {
  padding: 24px;
  overflow-y: auto;
  max-height: calc(90vh - 140px);
}

/* Dropzone */
.dropzone-box {
  border: 2px dashed #cbd5e1;
  border-radius: 14px;
  background: #f8fafc;
  padding: 32px 20px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.dropzone-box:hover, .dropzone-box.is-dragover {
  border-color: #7c3aed;
  background: #f5f3ff;
}

.dropzone-box.has-file {
  border-style: solid;
  border-color: #a78bfa;
  background: #faf5ff;
}

.hidden-file-input {
  display: none;
}

.dropzone-icon {
  font-size: 2.5rem;
  margin-bottom: 10px;
}

.dropzone-main-text {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 4px;
}

.dropzone-sub-text {
  font-size: 0.825rem;
  color: #64748b;
}

.dropzone-sub-text span {
  color: #7c3aed;
  font-weight: 600;
  text-decoration: underline;
}

.dropzone-selected {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 12px;
}

.file-info-col {
  flex: 1;
  text-align: left;
}

.file-name-text {
  font-weight: 700;
  font-size: 0.95rem;
  color: #0f172a;
}

.file-size-text {
  font-size: 0.775rem;
  color: #64748b;
  margin-top: 2px;
}

.btn-remove-file {
  background: #fee2e2;
  border: 1px solid #fecaca;
  color: #dc2626;
  border-radius: 50%;
  width: 28px;
  height: 28px;
  font-size: 0.8rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Hint Card */
.hint-card {
  margin-top: 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 14px 18px;
}

.hint-title {
  font-size: 0.85rem;
  font-weight: 700;
  color: #334155;
  margin-bottom: 6px;
}

.hint-list {
  margin: 0;
  padding-left: 20px;
  font-size: 0.8rem;
  color: #64748b;
  line-height: 1.6;
}

/* Stats Banner */
.stats-banner {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
  gap: 12px;
  margin-bottom: 16px;
}

.stat-box {
  padding: 12px;
  border-radius: 10px;
  text-align: center;
  border: 1px solid #e2e8f0;
}

.stat-box.blue { background: #eff6ff; border-color: #bfdbfe; color: #1e40af; }
.stat-box.green { background: #ecfdf5; border-color: #a7f3d0; color: #065f46; }
.stat-box.amber { background: #fffbeb; border-color: #fde68a; color: #92400e; }

.stat-num {
  font-size: 1.4rem;
  font-weight: 800;
  line-height: 1.2;
}

.stat-lbl {
  font-size: 0.725rem;
  font-weight: 600;
  text-transform: uppercase;
  margin-top: 2px;
}

.result-msg-box {
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  margin-bottom: 14px;
}

.result-msg-box.success {
  background: #dcfce7;
  color: #15803d;
  border: 1px solid #86efac;
}

.result-msg-box.warn {
  background: #fef3c7;
  color: #b45309;
  border: 1px solid #fde68a;
}

/* Result Table */
.result-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.8rem;
}

.result-table th {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  padding: 8px 12px;
  border-bottom: 2px solid #e2e8f0;
  text-align: left;
}

.result-table td {
  padding: 10px 12px;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.result-table tr.row-fail {
  background: #fffdf5;
}

.code-pill {
  background: #f1f5f9;
  color: #475569;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.75rem;
}

.emp-cell, .asset-cell {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.emp-code-sub, .asset-name-sub {
  font-size: 0.725rem;
  color: #64748b;
}

.badge-success-mini {
  background: #ecfdf5;
  color: #059669;
  font-weight: 700;
  font-size: 0.725rem;
  padding: 2px 6px;
  border-radius: 4px;
  display: inline-block;
}

.badge-fail-mini {
  background: #fef2f2;
  color: #dc2626;
  font-weight: 600;
  font-size: 0.725rem;
  padding: 2px 6px;
  border-radius: 4px;
  display: inline-block;
  max-width: 130px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.btn-view-pdf {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #2563eb;
  padding: 4px 8px;
  border-radius: 6px;
  text-decoration: none;
  font-size: 0.75rem;
  font-weight: 600;
  transition: all 0.15s;
}

.btn-view-pdf:hover {
  background: #dbeafe;
}

.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  background: #f8fafc;
}

.btn-start-split {
  background: linear-gradient(135deg, #7c3aed, #6d28d9);
  color: white;
  border: none;
  padding: 10px 18px;
  font-weight: 700;
  border-radius: 8px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  box-shadow: 0 4px 12px rgba(124, 58, 237, 0.25);
  transition: all 0.2s;
}

.btn-start-split:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(124, 58, 237, 0.35);
}

.btn-start-split:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-mini {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  display: inline-block;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
