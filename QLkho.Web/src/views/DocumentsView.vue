<template>
  <div class="documents-page">
    <!-- Top Bar -->
    <div class="page-topbar">
      <div>
        <h2 class="page-title">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="color: #ef4444;">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <polyline points="10 9 9 9 8 9"></polyline>
          </svg>
          {{ $t('documents.title') }}
        </h2>
        <p class="page-subtitle">{{ $t('documents.subtitle') }}</p>
      </div>

      <button class="btn btn-primary" @click="openUploadModal">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
          <polyline points="17 8 12 3 7 8"></polyline>
          <line x1="12" y1="3" x2="12" y2="15"></line>
        </svg>
        {{ $t('documents.btn_upload') }}
      </button>
    </div>

    <!-- Quick Stats Cards -->
    <div class="doc-stats-grid">
      <div class="glass-card stat-mini">
        <div class="stat-icon-wrap" style="background: rgba(99, 102, 241, 0.15); color: #818cf8;">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
          </svg>
        </div>
        <div>
          <div class="stat-mini-val">{{ documents.length }}</div>
          <div class="stat-mini-lbl">Tổng Số Tài Liệu</div>
        </div>
      </div>

      <div class="glass-card stat-mini">
        <div class="stat-icon-wrap" style="background: rgba(16, 185, 129, 0.15); color: #34d399;">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
            <circle cx="9" cy="7" r="4"></circle>
            <polyline points="16 11 18 13 22 9"></polyline>
          </svg>
        </div>
        <div>
          <div class="stat-mini-val">{{ countByType('HandoverReceipt') }}</div>
          <div class="stat-mini-lbl">Biên Bản Bàn Giao</div>
        </div>
      </div>

      <div class="glass-card stat-mini">
        <div class="stat-icon-wrap" style="background: rgba(245, 158, 11, 0.15); color: #fbbf24;">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
          </svg>
        </div>
        <div>
          <div class="stat-mini-val">{{ countByType('WarrantyReceipt') }}</div>
          <div class="stat-mini-lbl">Phiếu Bảo Hành</div>
        </div>
      </div>

      <div class="glass-card stat-mini">
        <div class="stat-icon-wrap" style="background: rgba(239, 68, 68, 0.15); color: #f87171;">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="1" y="4" width="22" height="16" rx="2" ry="2"></rect>
            <line x1="1" y1="10" x2="23" y2="10"></line>
          </svg>
        </div>
        <div>
          <div class="stat-mini-val">{{ countByType('Invoice') + countByType('Contract') }}</div>
          <div class="stat-mini-lbl">Hóa Đơn & Hợp Đồng</div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="glass-card filter-card">
      <div class="filter-grid">
        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Tìm kiếm hồ sơ</label>
          <input 
            type="text" 
            class="form-control" 
            v-model="filters.search" 
            placeholder="Tên tài liệu, tên file, mã thiết bị, nhân viên..." 
            @input="debounceFetch"
          />
        </div>

        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Phân loại tài liệu</label>
          <select class="form-control" v-model="filters.documentType" @change="fetchDocuments">
            <option value="">-- Tất cả loại hồ sơ --</option>
            <option value="HandoverReceipt">Biên Bản Bàn Giao</option>
            <option value="WarrantyReceipt">Phiếu Bảo Hành</option>
            <option value="Invoice">Hóa Đơn Mua Hàng</option>
            <option value="Contract">Hợp Đồng Mua Bán</option>
            <option value="UserManual">Hướng Dẫn Sử Dụng</option>
            <option value="Other">Tài Liệu Khác</option>
          </select>
        </div>

        <div class="form-group" style="margin-bottom: 0;">
          <label class="form-label">Liên kết Thiết Bị</label>
          <select class="form-control" v-model="filters.assetID" @change="fetchDocuments">
            <option :value="null">-- Tất cả thiết bị --</option>
            <option v-for="ast in assets" :key="ast.assetID" :value="ast.assetID">
              {{ ast.assetCode }} - {{ ast.assetName }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Documents Table -->
    <div class="glass-card table-container">
      <div v-if="loading" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải danh mục tài liệu & hồ sơ...</p>
      </div>

      <div v-else-if="documents.length === 0" class="empty-state">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" style="color: var(--text-dim); margin-bottom: 12px;">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
          <polyline points="14 2 14 8 20 8"></polyline>
          <line x1="12" y1="18" x2="12" y2="12"></line>
          <line x1="9" y1="15" x2="15" y2="15"></line>
        </svg>
        <p>Chưa có tài liệu hoặc hồ sơ PDF nào được lưu trữ.</p>
        <button class="btn btn-primary" style="margin-top: 10px;" @click="openUploadModal">
          Tải Lên Tài Liệu Đầu Tiên
        </button>
      </div>

      <div v-else>
        <div class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Tên Tài Liệu / File PDF</th>
                <th>Phân Loại</th>
                <th>Liên Kết Thiết Bị / Nhân Sự</th>
                <th>Dung Lượng</th>
                <th>Người Tải / Ngày Lưu</th>
                <th style="text-align: right;">Thao Tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="doc in paginatedDocs" :key="doc.documentID">
                <td>
                  <div class="doc-title-wrap">
                    <div class="pdf-icon-badge">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                        <polyline points="14 2 14 8 20 8"></polyline>
                      </svg>
                      <span>PDF</span>
                    </div>
                    <div>
                      <div class="doc-name" @click="previewPdf(doc)">{{ doc.documentName }}</div>
                      <div class="doc-file-sub">{{ doc.fileName }}</div>
                    </div>
                  </div>
                </td>

                <td>
                  <span class="doc-type-badge" :class="doc.documentType.toLowerCase()">
                    {{ doc.documentTypeLabel }}
                  </span>
                </td>

                <td>
                  <div v-if="doc.assetCode" class="link-item">
                    <span class="link-lbl">Thiết bị:</span>
                    <span class="code-badge-mini">{{ doc.assetCode }}</span>
                    <span class="link-txt">{{ doc.assetName }}</span>
                  </div>
                  <div v-if="doc.employeeName" class="link-item" style="margin-top: 2px;">
                    <span class="link-lbl">Nhân sự:</span>
                    <strong style="color: #0f172a;">{{ doc.employeeName }}</strong> ({{ doc.employeeCode }})
                  </div>
                  <span v-if="!doc.assetCode && !doc.employeeName" class="text-dim">---</span>
                </td>

                <td>
                  <span class="size-text">{{ doc.fileSizeFormatted }}</span>
                </td>

                <td>
                  <div class="date-text">{{ formatDate(doc.createdAt) }}</div>
                  <div class="uploader-text">Bởi: {{ doc.uploadedBy }}</div>
                </td>

                <td style="text-align: right;">
                  <div class="doc-actions">
                    <!-- Nút Xem trước trực tiếp -->
                    <button 
                      class="btn btn-sm btn-info-mini" 
                      @click="previewPdf(doc)"
                      title="Xem trước PDF trực tiếp"
                    >
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                        <circle cx="12" cy="12" r="3"></circle>
                      </svg>
                      Xem
                    </button>

                    <!-- Nút Tải về -->
                    <a 
                      :href="documentsApi.getDownloadUrl(doc.documentID)" 
                      target="_blank"
                      class="btn-icon" 
                      title="Tải file PDF về máy"
                      download
                    >
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                        <polyline points="7 10 12 15 17 10"></polyline>
                        <line x1="12" y1="15" x2="12" y2="3"></line>
                      </svg>
                    </a>

                    <!-- Nút Xóa -->
                    <button 
                      class="btn-icon btn-delete-icon" 
                      @click="deleteDocumentItem(doc)"
                      title="Xóa tài liệu này"
                    >
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <polyline points="3 6 5 6 21 6"></polyline>
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Phân trang -->
        <Pagination 
          v-model:currentPage="currentPage" 
          v-model:pageSize="pageSize" 
          :totalItems="documents.length" 
        />
      </div>
    </div>

    <!-- MODAL 1: IMPORT / UPLOAD TÀI LIỆU PDF -->
    <Modal 
      :is-open="isUploadOpen" 
      title="Import & Lưu Trữ Hồ Sơ / Tài Liệu PDF"
      subtitle="Hỗ trợ tải lên biên bản bàn giao scan, phiếu bảo hành, hóa đơn VAT hoặc hợp đồng mua sắm"
      @close="isUploadOpen = false"
      max-width="700px"
    >
      <form @submit.prevent="submitUploadDocument">
        <!-- Vùng Kéo Thả File -->
        <div 
          class="dropzone-box"
          :class="{ 'is-dragging': isDragging, 'has-file': !!selectedFile }"
          @dragover.prevent="isDragging = true"
          @dragleave.prevent="isDragging = false"
          @drop.prevent="handleFileDrop"
          @click="$refs.fileInputRef.click()"
        >
          <input 
            type="file" 
            ref="fileInputRef" 
            accept=".pdf,.docx,.doc,.xlsx,.xls,.png,.jpg,.jpeg" 
            style="display: none;" 
            @change="handleFileSelect"
          />

          <div v-if="!selectedFile" class="dropzone-content">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1; margin-bottom: 8px;">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
              <polyline points="17 8 12 3 7 8"></polyline>
              <line x1="12" y1="3" x2="12" y2="15"></line>
            </svg>
            <div class="dropzone-title">Kéo thả file PDF hoặc bấm vào đây để chọn file</div>
            <div class="dropzone-sub">Định dạng hỗ trợ: .PDF, .DOCX, .XLSX, Hình ảnh (Tối đa 25MB)</div>
          </div>

          <div v-else class="selected-file-info">
            <div class="file-icon-box">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
              </svg>
            </div>
            <div style="flex: 1;">
              <strong class="selected-name">{{ selectedFile.name }}</strong>
              <div class="selected-size">{{ formatBytes(selectedFile.size) }}</div>
            </div>
            <button type="button" class="btn-remove-file" @click.stop="selectedFile = null">✕ Bỏ chọn</button>
          </div>
        </div>

        <div class="modal-grid-2" style="margin-top: 16px;">
          <div class="form-group">
            <label class="form-label">Phân Loại Tài Liệu *</label>
            <select class="form-control" v-model="uploadForm.documentType" required @change="generateAutoDocName">
              <option value="HandoverReceipt">Biên Bản Bàn Giao (Scan chữ ký)</option>
              <option value="WarrantyReceipt">Phiếu Bảo Hành Chính Hãng</option>
              <option value="Invoice">Hóa Đơn Mua Hàng / VAT</option>
              <option value="Contract">Hợp Đồng Mua Bán / Cung Cấp</option>
              <option value="UserManual">Hướng Dẫn Sử Dụng / Kỹ Thuật</option>
              <option value="Other">Tài Liệu Khác</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Tên Hồ Sơ / Tài Liệu *</label>
            <input type="text" class="form-control" v-model="uploadForm.documentName" required placeholder="Vd: Biên bản bàn giao Laptop Dell i7 - Nguyễn Văn A" />
          </div>
        </div>

        <!-- 1. CHỌN NHÂN SỰ -->
        <div class="form-group" style="margin-top: 12px;">
          <div class="section-label-row">
            <label class="form-label" style="margin-bottom: 0;">1. Chọn Nhân Sự Bàn Giao / Sử Dụng Tài Liệu</label>
            <span v-if="uploadForm.employeeID" class="emp-selected-status-badge">
              ✓ Đã chọn nhân sự
            </span>
          </div>
          <EmployeeSelector 
            v-model="uploadForm.employeeID" 
            :employees="employees" 
            :departments="departments" 
            @change="handleEmployeeChange"
          />
        </div>

        <!-- 2. CHỌN THIẾT BỊ (ƯU TIÊN CÁC MÁY NHÂN VIÊN ĐÓ ĐANG NẮM GIỮ) -->
        <div class="form-group" style="margin-top: 14px;">
          <div class="asset-select-header-row">
            <label class="form-label" style="margin-bottom: 0;">
              2. Chọn Thiết Bị Gắn Với Tài Liệu
              <span v-if="selectedEmployeeObj && !useAllWarehouseAssets" class="emp-holding-hint">
                (📦 Đang hiển thị {{ employeeHoldingAssets.length }} máy của {{ selectedEmployeeObj.fullName }})
              </span>
            </label>
            <button 
              v-if="selectedEmployeeObj" 
              type="button" 
              class="btn-toggle-source" 
              @click="toggleWarehouseAssetSource"
            >
              {{ useAllWarehouseAssets ? '🔄 Chỉ xem máy nhân viên đang giữ' : '🌐 Xem toàn bộ kho' }}
            </button>
          </div>

          <div v-if="loadingHoldingAssets" class="loading-holding-hint">
            <span class="loading-spinner"></span>
            <span>Đang tải danh sách thiết bị nhân viên đang giữ...</span>
          </div>

          <AssetSelector 
            v-else
            v-model="uploadForm.assetID" 
            :assets="displayedAssetsForUpload" 
            :categories="categories" 
            @change="handleAssetChange"
          />

          <small v-if="selectedEmployeeObj && employeeHoldingAssets.length === 0 && !useAllWarehouseAssets" class="no-emp-asset-hint">
            💡 Nhân viên này hiện chưa giữ thiết bị nào. Bấm <strong>"Xem toàn bộ kho"</strong> phía trên để chọn máy từ kho hoặc chọn nhân viên khác.
          </small>
        </div>

        <div class="form-group" style="margin-top: 14px;">
          <label class="form-label">Mô Tả / Ghi Chú Hồ Sơ</label>
          <textarea class="form-control" rows="2" v-model="uploadForm.description" placeholder="Số hợp đồng, ngày ký, nội dung lưu ý..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isUploadOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting || !selectedFile">
            <span v-if="submitting" class="loading-spinner"></span>
            Import & Lưu Trữ
          </button>
        </div>
      </form>
    </Modal>

    <!-- MODAL 2: XEM TRƯỚC PDF (PDF PREVIEW) -->
    <Modal 
      :is-open="isPreviewOpen" 
      :title="`Xem Tài Liệu: ${previewDoc?.documentName || ''}`"
      :subtitle="previewDoc?.fileName || ''"
      @close="isPreviewOpen = false"
      max-width="900px"
    >
      <div class="pdf-viewer-container">
        <iframe 
          v-if="previewDoc" 
          :src="documentsApi.getFileUrl(previewDoc.filePath)" 
          class="pdf-iframe"
        ></iframe>
      </div>
      <div class="modal-actions-right">
        <a 
          :href="documentsApi.getDownloadUrl(previewDoc?.documentID)" 
          target="_blank" 
          class="btn btn-secondary"
          download
        >
          📥 Tải Về Máy
        </a>
        <button type="button" class="btn btn-primary" @click="isPreviewOpen = false">Đóng</button>
      </div>
    </Modal>

    <!-- Toast Notification -->
    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { documentsApi, assetsApi, employeesApi, departmentsApi, categoriesApi } from '@/api/client'
import Modal from '@/components/common/Modal.vue'
import Toast from '@/components/common/Toast.vue'
import Pagination from '@/components/common/Pagination.vue'
import EmployeeSelector from '@/components/common/EmployeeSelector.vue'
import AssetSelector from '@/components/common/AssetSelector.vue'

const documents = ref([])
const assets = ref([])
const employees = ref([])
const departments = ref([])
const categories = ref([])
const loading = ref(false)
const submitting = ref(false)
const isDragging = ref(false)

// State quản lý thiết bị của nhân viên đang chọn trong Upload Modal
const employeeHoldingAssets = ref([])
const loadingHoldingAssets = ref(false)
const useAllWarehouseAssets = ref(false)

const toastRef = ref(null)
const fileInputRef = ref(null)
const selectedFile = ref(null)

// Phân trang
const currentPage = ref(1)
const pageSize = ref(10)

const paginatedDocs = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return documents.value.slice(start, end)
})

// Modals
const isUploadOpen = ref(false)
const isPreviewOpen = ref(false)
const previewDoc = ref(null)

const filters = reactive({
  search: '',
  documentType: '',
  assetID: null
})

watch(filters, () => {
  currentPage.value = 1
})

const uploadForm = reactive({
  documentName: '',
  documentType: 'HandoverReceipt',
  assetID: null,
  employeeID: null,
  description: '',
  uploadedBy: 'Admin'
})

// Nhân viên đang chọn
const selectedEmployeeObj = computed(() => {
  if (!uploadForm.employeeID) return null
  return employees.value.find(e => e.employeeID === uploadForm.employeeID) || null
})

// Danh sách thiết bị hiển thị cho upload modal
const displayedAssetsForUpload = computed(() => {
  // Nếu đã chọn nhân viên và không bật chế độ xem toàn kho
  if (selectedEmployeeObj.value && !useAllWarehouseAssets.value) {
    return employeeHoldingAssets.value
  }
  return assets.value
})

// Xử lý khi chọn nhân sự trong Upload Modal
const handleEmployeeChange = async (emp) => {
  if (!emp) {
    employeeHoldingAssets.value = []
    useAllWarehouseAssets.value = false
    generateAutoDocName()
    return
  }

  loadingHoldingAssets.value = true
  useAllWarehouseAssets.value = false
  try {
    const res = await employeesApi.getAssets(emp.employeeID)
    employeeHoldingAssets.value = res?.assets || []
    
    // Nếu nhân viên có đúng 1 thiết bị, tự động chọn luôn thiết bị đó
    if (employeeHoldingAssets.value.length === 1) {
      uploadForm.assetID = employeeHoldingAssets.value[0].assetID
    } else if (employeeHoldingAssets.value.length > 0) {
      // Nếu thiết bị hiện tại không nằm trong danh sách máy của nhân viên này
      const exists = employeeHoldingAssets.value.some(a => a.assetID === uploadForm.assetID)
      if (!exists) {
        uploadForm.assetID = employeeHoldingAssets.value[0].assetID
      }
    } else {
      // Nhân viên chưa giữ máy nào
      uploadForm.assetID = null
    }
  } catch (err) {
    console.error('Lỗi lấy thiết bị nhân viên:', err)
  } finally {
    loadingHoldingAssets.value = false
    generateAutoDocName()
  }
}

// Xử lý khi chọn thiết bị trong Upload Modal
const handleAssetChange = () => {
  generateAutoDocName()
}

// Toggle chế độ xem toàn kho hay chỉ máy nhân viên
const toggleWarehouseAssetSource = () => {
  useAllWarehouseAssets.value = !useAllWarehouseAssets.value
}

// Tự động tạo tên tài liệu thông minh
const generateAutoDocName = () => {
  const typeMap = {
    'HandoverReceipt': 'Biên bản bàn giao',
    'WarrantyReceipt': 'Phiếu bảo hành',
    'Invoice': 'Hóa đơn VAT',
    'Contract': 'Hợp đồng mua bán',
    'UserManual': 'Hướng dẫn kỹ thuật',
    'Other': 'Tài liệu'
  }
  const typeName = typeMap[uploadForm.documentType] || 'Biên bản'
  
  // Tìm thông tin máy và người
  let assetName = ''
  if (uploadForm.assetID) {
    const ast = assets.value.find(a => a.assetID === uploadForm.assetID) || 
                employeeHoldingAssets.value.find(a => a.assetID === uploadForm.assetID)
    if (ast) {
      assetName = `${ast.assetName} [${ast.assetCode}]`
    }
  }

  let empName = ''
  if (selectedEmployeeObj.value) {
    empName = selectedEmployeeObj.value.fullName
  }

  if (assetName && empName) {
    uploadForm.documentName = `${typeName} - ${assetName} - ${empName}`
  } else if (assetName) {
    uploadForm.documentName = `${typeName} - ${assetName}`
  } else if (empName) {
    uploadForm.documentName = `${typeName} - ${empName}`
  }
}

let debounceTimer = null
const debounceFetch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    fetchDocuments()
  }, 300)
}

const fetchDocuments = async () => {
  loading.value = true
  try {
    const params = {}
    if (filters.search) params.search = filters.search
    if (filters.documentType) params.documentType = filters.documentType
    if (filters.assetID) params.assetId = filters.assetID

    const res = await documentsApi.getAll(params)
    documents.value = res || []
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải tài liệu', err.message, 'error')
  } finally {
    loading.value = false
  }
}

const fetchMetadata = async () => {
  try {
    const [asts, emps, depts, cats] = await Promise.all([
      assetsApi.getAll(),
      employeesApi.getAll({ status: 'Active' }),
      departmentsApi.getAll(),
      categoriesApi.getAll()
    ])
    assets.value = asts || []
    employees.value = emps || []
    departments.value = depts || []
    categories.value = cats || []
  } catch (err) {
    console.error('Lỗi tải metadata:', err)
  }
}

const countByType = (type) => {
  return documents.value.filter(d => d.documentType === type).length
}

// Drag and drop / File select
const handleFileSelect = (e) => {
  const file = e.target.files[0]
  if (file) processFile(file)
}

const handleFileDrop = (e) => {
  isDragging.value = false
  const file = e.dataTransfer.files[0]
  if (file) processFile(file)
}

const processFile = (file) => {
  selectedFile.value = file
  if (!uploadForm.documentName) {
    // Tự động gán tên tài liệu theo tên file (bỏ đuôi mở rộng)
    uploadForm.documentName = file.name.replace(/\.[^/.]+$/, "")
  }
}

const openUploadModal = () => {
  selectedFile.value = null
  uploadForm.documentName = ''
  uploadForm.documentType = 'HandoverReceipt'
  uploadForm.assetID = null
  uploadForm.employeeID = null
  uploadForm.description = ''
  employeeHoldingAssets.value = []
  useAllWarehouseAssets.value = false
  isUploadOpen.value = true
}

const submitUploadDocument = async () => {
  if (!selectedFile.value) {
    toastRef.value?.addToast('Cảnh báo', 'Vui lòng chọn file PDF để tải lên', 'error')
    return
  }

  submitting.value = true
  try {
    const formData = new FormData()
    formData.append('documentName', uploadForm.documentName)
    formData.append('documentType', uploadForm.documentType)
    formData.append('file', selectedFile.value)
    if (uploadForm.assetID) formData.append('assetID', uploadForm.assetID)
    if (uploadForm.employeeID) formData.append('employeeID', uploadForm.employeeID)
    if (uploadForm.description) formData.append('description', uploadForm.description)
    formData.append('uploadedBy', 'Admin')

    await documentsApi.upload(formData)
    toastRef.value?.addToast('Thành công', 'Đã lưu trữ tài liệu PDF thành công!', 'success')
    isUploadOpen.value = false
    fetchDocuments()
  } catch (err) {
    toastRef.value?.addToast('Lỗi tải lên', err.message, 'error')
  } finally {
    submitting.value = false
  }
}

const previewPdf = (doc) => {
  previewDoc.value = doc
  isPreviewOpen.value = true
}

const deleteDocumentItem = async (doc) => {
  if (!confirm(`Bạn có chắc muốn xóa hồ sơ "${doc.documentName}" không?`)) return
  try {
    await documentsApi.delete(doc.documentID)
    toastRef.value?.addToast('Thành công', 'Đã xóa tài liệu!', 'success')
    fetchDocuments()
  } catch (err) {
    toastRef.value?.addToast('Lỗi xóa tài liệu', err.message, 'error')
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const formatBytes = (bytes) => {
  if (!bytes) return '0 B'
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`
}

onMounted(() => {
  fetchDocuments()
  fetchMetadata()
})
</script>

<style scoped>
.documents-page {
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

/* Mini Stats */
.doc-stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-mini {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px 20px;
}

.stat-icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-mini-val {
  font-size: 1.3rem;
  font-weight: 700;
  color: #0f172a;
}

.stat-mini-lbl {
  font-size: 0.8rem;
  color: var(--text-dim);
}

.filter-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 16px;
}

.doc-title-wrap {
  display: flex;
  align-items: center;
  gap: 12px;
}

.pdf-icon-badge {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: rgba(239, 68, 68, 0.15);
  color: #f87171;
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: var(--radius-sm);
  padding: 4px 6px;
  font-size: 0.65rem;
  font-weight: 800;
}

.doc-name {
  font-weight: 700;
  color: #0f172a;
  cursor: pointer;
  transition: var(--transition);
}

.doc-name:hover {
  color: var(--primary);
}

.doc-file-sub {
  font-size: 0.75rem;
  color: var(--text-dim);
  margin-top: 2px;
}

.doc-type-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 4px;
}

.doc-type-badge.handoverreceipt {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
}

.doc-type-badge.warrantyreceipt {
  background: #fffbeb;
  color: #d97706;
  border: 1px solid #fde68a;
}

.doc-type-badge.invoice, .doc-type-badge.contract {
  background: #eef2ff;
  color: #4f46e5;
  border: 1px solid #c7d2fe;
}

.doc-type-badge.usermanual, .doc-type-badge.other {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.link-item {
  font-size: 0.8rem;
}

.link-lbl {
  color: var(--text-dim);
  margin-right: 4px;
}

.code-badge-mini {
  font-family: monospace;
  font-size: 0.75rem;
  color: #4f46e5;
  background: #eef2ff;
  padding: 1px 4px;
  border-radius: 3px;
  margin-right: 4px;
  border: 1px solid #c7d2fe;
}

.link-txt {
  color: var(--text-main);
}

.size-text {
  font-family: monospace;
  font-size: 0.8rem;
  color: var(--text-dim);
}

.date-text {
  font-size: 0.8rem;
  color: #0f172a;
  font-weight: 500;
}

.uploader-text {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.doc-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}

/* Dropzone */
.dropzone-box {
  border: 2px dashed #cbd5e1;
  background: #f8fafc;
  border-radius: var(--radius-md);
  padding: 24px;
  text-align: center;
  cursor: pointer;
  transition: var(--transition);
}

.dropzone-box:hover, .dropzone-box.is-dragging {
  border-color: var(--primary);
  background: #eef2ff;
}

.dropzone-title {
  font-weight: 700;
  color: #0f172a;
  font-size: 0.95rem;
}

.dropzone-sub {
  font-size: 0.8rem;
  color: var(--text-dim);
  margin-top: 4px;
}

.selected-file-info {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #f1f5f9;
  padding: 12px 16px;
  border-radius: var(--radius-sm);
  text-align: left;
}

.file-icon-box {
  color: #dc2626;
}

.selected-name {
  color: #0f172a;
  font-size: 0.9rem;
  font-weight: 600;
}

.selected-size {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.btn-remove-file {
  background: none;
  border: none;
  color: #dc2626;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-remove-file:hover {
  text-decoration: underline;
}

.pdf-viewer-container {
  width: 100%;
  height: 600px;
  background: #f8fafc;
  border-radius: var(--radius-md);
  overflow: hidden;
  border: 1px solid var(--border-color);
}

.pdf-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

.modal-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.modal-actions-right {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

/* Upload Modal Enhanced Selectors */
.section-label-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.emp-selected-status-badge {
  font-size: 0.725rem;
  font-weight: 600;
  color: #059669;
  background: #ecfdf5;
  padding: 2px 8px;
  border-radius: 9999px;
  border: 1px solid #a7f3d0;
}

.asset-select-header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
  flex-wrap: wrap;
  gap: 8px;
}

.emp-holding-hint {
  font-size: 0.75rem;
  color: #0284c7;
  font-weight: normal;
  margin-left: 4px;
}

.btn-toggle-source {
  background: #f1f5f9;
  border: 1px solid #cbd5e1;
  color: #4f46e5;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-toggle-source:hover {
  background: #eef2ff;
  border-color: var(--primary);
  color: #4338ca;
}

.loading-holding-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px;
  background: #f8fafc;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  font-size: 0.8rem;
  color: var(--text-dim);
}

.no-emp-asset-hint {
  display: block;
  margin-top: 6px;
  color: #fbbf24;
  font-size: 0.75rem;
  line-height: 1.4;
}
</style>
