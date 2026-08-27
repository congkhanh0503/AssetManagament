<template>
  <Modal 
    :is-open="isOpen" 
    title="Nhập Dữ Liệu Nhân Viên Từ File Excel / CSV"
    subtitle="Xem trước và tự động kiểm tra tính hợp lệ của dữ liệu trước khi lưu vào hệ thống"
    @close="$emit('close')"
    max-width="950px"
  >
    <div class="import-modal-content">
      <!-- Bước 1: Tải file mẫu -->
      <div class="import-step-card">
        <div class="step-num">1</div>
        <div class="step-body">
          <div class="step-title">Tải file Excel mẫu chuẩn</div>
          <p class="step-desc">Tải file mẫu về máy, điền danh sách nhân viên theo đúng các cột tiêu đề rồi tải lên hệ thống.</p>
          <button type="button" class="btn btn-secondary btn-sm" @click="$emit('download-template')">
            📥 Tải File Mẫu Excel (.xlsx)
          </button>
        </div>
      </div>

      <!-- Bước 2: Tải file lên -->
      <div class="import-step-card">
        <div class="step-num">2</div>
        <div class="step-body">
          <div class="step-title">Chọn hoặc kéo thả file Excel / CSV vào đây</div>
          <div 
            class="import-drop-zone"
            :class="{ 'dragging': isDragging, 'has-file': !!file }"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="onDrop"
            @click="triggerFileInput"
          >
            <input 
              type="file" 
              ref="fileInputRef" 
              accept=".xlsx, .xls, .csv" 
              style="display: none;" 
              @change="onFileSelected" 
            />
            <div v-if="!file" class="drop-zone-placeholder">
              <span class="drop-icon">📊</span>
              <p class="drop-text">Bấm vào đây hoặc kéo thả file <strong>.xlsx, .xls, .csv</strong> vào</p>
              <span class="drop-sub">Dung lượng tối đa 10MB</span>
            </div>
            <div v-else class="drop-zone-file">
              <span class="file-icon">📄</span>
              <div class="file-details">
                <strong class="file-name">{{ file.name }}</strong>
                <span class="file-size">{{ formatFileSize(file.size) }}</span>
              </div>
              <button type="button" class="btn-clear-file" @click.stop="clearFile" title="Xóa file chọn lại">✕</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Bước 3: Xem trước & Kiểm tra tính hợp lệ -->
      <!-- Bước 3: Xem trước & Kiểm tra tính hợp lệ -->
      <div v-if="parsedList.length > 0" class="import-step-card preview-step">
        <div class="step-num">3</div>
        <div class="step-body">
          <div class="preview-header-wrap">
            <div class="step-title">
              Xem trước & Kiểm tra tính hợp lệ của dữ liệu
            </div>

            <!-- Thanh thống kê số lượng -->
            <div class="validation-stats-bar">
              <div class="stat-pill total" :class="{ active: activeFilter === 'all' }" @click="activeFilter = 'all'">
                <span>Tổng số:</span>
                <strong>{{ parsedList.length }}</strong>
              </div>
              <div class="stat-pill valid" :class="{ active: activeFilter === 'valid' }" @click="activeFilter = 'valid'">
                <span>✅ Hợp lệ:</span>
                <strong>{{ validList.length }}</strong>
              </div>
              <div class="stat-pill error" :class="{ active: activeFilter === 'error' }" @click="activeFilter = 'error'">
                <span>❌ Bị lỗi:</span>
                <strong>{{ errorList.length }}</strong>
              </div>
              <div v-if="updateList.length > 0" class="stat-pill update" :class="{ active: activeFilter === 'update' }" @click="activeFilter = 'update'">
                <span>🔄 Cập nhật Email:</span>
                <strong>{{ updateList.length }}</strong>
              </div>
            </div>
          </div>

          <!-- Tùy chọn chế độ cập nhật Email cho nhân sự hiện tại -->
          <div class="import-option-box">
            <label class="checkbox-option-lbl">
              <input type="checkbox" v-model="updateExisting" />
              <span>🔄 <strong>Tự động cập nhật Email & Tên tiếng Anh</strong> nếu nhân viên đã tồn tại trong hệ thống</span>
            </label>
          </div>

          <!-- Bảng xem trước dữ liệu -->
          <div class="import-preview-table-wrap">
            <table class="import-preview-table">
              <thead>
                <tr>
                  <th style="width: 45px; text-align: center;">#</th>
                  <th style="width: 170px;">Hành Động / Trạng Thái</th>
                  <th style="width: 100px;">Mã NV</th>
                  <th style="min-width: 140px;">Họ và Tên</th>
                  <th style="min-width: 130px;">Tên Tiếng Anh</th>
                  <th style="min-width: 130px;">Phòng Ban</th>
                  <th style="min-width: 110px;">Chức Danh</th>
                  <th style="min-width: 150px;">Email (Liên hệ / Cty)</th>
                  <th style="width: 110px;">Số ĐT</th>
                  <th style="width: 100px;">Ngày Vào Làm</th>
                </tr>
              </thead>
              <tbody>
                <tr 
                  v-for="(item, idx) in displayedList" 
                  :key="idx" 
                  :class="{ 'row-error': !item.isValid, 'row-update': item.actionType === 'update' }"
                >
                  <td style="text-align: center; font-weight: 600; color: #64748b;">{{ item.rowIndex }}</td>
                  
                  <!-- Trạng thái validation & Hành động -->
                  <td>
                    <div v-if="!item.isValid" class="val-badge-wrap">
                      <span class="val-badge badge-err" v-for="(err, eIdx) in item.errors" :key="eIdx">
                        ❌ {{ err }}
                      </span>
                    </div>
                    <div v-else class="val-badge-wrap">
                      <span v-if="item.actionType === 'update'" class="val-badge badge-update">
                        🔄 Bổ sung Email
                      </span>
                      <span v-else class="val-badge badge-ok">
                        🆕 Thêm mới
                      </span>
                    </div>
                  </td>

                  <td><strong class="font-mono text-primary">{{ item.code || '---' }}</strong></td>
                  <td><strong>{{ item.name || '---' }}</strong></td>
                  <td><span v-if="item.englishName" style="color: #4f46e5; font-weight: 600;">{{ item.englishName }}</span><span v-else style="color: #94a3b8;">---</span></td>
                  <td>{{ item.deptName || '---' }}</td>
                  <td>{{ item.title || 'Nhân viên' }}</td>
                  <td>
                    <span v-if="item.email" class="email-preview-pill">
                      ✉️ {{ item.email }}
                    </span>
                    <span v-else style="color: #94a3b8; font-style: italic;">Chưa có email</span>
                  </td>
                  <td>{{ item.phone || '---' }}</td>
                  <td>{{ item.joinDate || '---' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-if="displayedList.length === 0" class="empty-filter-note">
            Không có dòng dữ liệu nào khớp với bộ lọc đang chọn.
          </div>

          <div class="preview-footer-note">
            <p v-if="errorList.length > 0" class="text-danger-note">
              * Lưu ý: Các dòng bị lỗi ({{ errorList.length }} dòng) sẽ tự động được bỏ qua. Sẽ xử lý <strong>{{ validList.length }} dòng hợp lệ</strong> (gồm {{ createList.length }} thêm mới và {{ updateList.length }} cập nhật).
            </p>
            <p v-else class="text-success-note">
              ✓ Toàn bộ {{ validList.length }} dòng dữ liệu đều hợp lệ! Sẵn sàng thêm mới {{ createList.length }} nhân sự và cập nhật email cho {{ updateList.length }} nhân sự hiện tại.
            </p>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="modal-actions-right" style="margin-top: 24px;">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
        <button 
          type="button" 
          class="btn btn-primary" 
          :disabled="validList.length === 0 || submitting"
          @click="submitValidRows"
        >
          <span v-if="submitting" class="loading-spinner"></span>
          🚀 Xác Nhận Nhập & Cập Nhật ({{ validList.length }})
        </button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'
import { parseSpreadsheetFile, getRowValue } from '@/utils/excelImport'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  submitting: { type: Boolean, default: false },
  employees: { type: Array, default: () => [] },
  departments: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'submit', 'download-template'])

const file = ref(null)
const rawRows = ref([])
const isDragging = ref(false)
const fileInputRef = ref(null)
const activeFilter = ref('all') // 'all', 'valid', 'error', 'warning'

const triggerFileInput = () => {
  fileInputRef.value?.click()
}

const onFileSelected = async (e) => {
  const selected = e.target.files?.[0]
  if (selected) {
    await processFile(selected)
  }
}

const onDrop = async (e) => {
  isDragging.value = false
  const dropped = e.dataTransfer?.files?.[0]
  if (dropped) {
    await processFile(dropped)
  }
}

const processFile = async (f) => {
  file.value = f
  try {
    const data = await parseSpreadsheetFile(f)
    const list = Array.isArray(data) ? data : (data?.rows || [])
    rawRows.value = list
    activeFilter.value = 'all'
    if (list.length === 0) {
      alert('Không tìm thấy dòng dữ liệu nào trong file bảng tính.')
    }
  } catch (err) {
    alert('Lỗi đọc file: ' + err.message)
    clearFile()
  }
}

const clearFile = () => {
  file.value = null
  rawRows.value = []
  activeFilter.value = 'all'
  if (fileInputRef.value) fileInputRef.value.value = ''
}

watch(() => props.isOpen, (open) => {
  if (!open) {
    clearFile()
  }
})

const formatFileSize = (bytes) => {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const updateExisting = ref(true)

// Phân tích và kiểm tra tính hợp lệ (Validation)
const parsedList = computed(() => {
  if (!rawRows.value || !rawRows.value.length) return []

  const existingCodes = new Set((props.employees || []).map(e => (e.employeeCode || '').trim().toLowerCase()))
  const seenCodesInFile = new Map()

  return rawRows.value.map((row, index) => {
    const code = getRowValue(row, ['mã nhân viên', 'ma nhan vien', 'manhanvien', 'mã nv', 'ma nv', 'code', 'employeecode']).trim()
    const name = getRowValue(row, ['họ và tên', 'ho va ten', 'họ tên', 'ho ten', 'hoten', 'tên', 'ten', 'fullname', 'name']).trim()
    const englishName = getRowValue(row, ['tên tiếng anh', 'ten tieng anh', 'englishname', 'english name', 'tentienganh', 'en name', 'enname']).trim()
    const deptName = getRowValue(row, ['phòng ban', 'phong ban', 'phongban', 'department', 'bộ phận', 'bo phan']).trim()
    const title = getRowValue(row, ['chức danh', 'chuc danh', 'chucdanh', 'chức vụ', 'chuc vu', 'title', 'position']).trim()
    
    // Trích xuất địa chỉ email (ưu tiên cột nhập email, không lấy nhầm cột trạng thái)
    const rawEmail = getRowValue(row, [
      'email (địa chỉ email mới)*',
      'email (địa chỉ email mới)',
      'email (địa chỉ email)',
      'email (điền email mới tại đây)*',
      'email (điền email mới tại đây)',
      'email (dien email moi tai day)',
      'địa chỉ email',
      'dia chi email',
      'email liên hệ',
      'email cá nhân',
      'thư điện tử',
      'thu dien tu',
      'email',
      'mail'
    ]).trim()

    // Chặn tuyệt đối nếu giá trị là từ khóa trạng thái (Available, Disable, v.v.)
    const statusWords = ['available', 'disable', 'active', 'deleted', 'hoạt động', 'bật', 'khóa', 'đã xóa', 'chưa có', '---']
    const email = (rawEmail && !statusWords.includes(rawEmail.toLowerCase())) ? rawEmail : ''

    const phone = getRowValue(row, ['số điện thoại', 'so dien thoai', 'sdt', 'phone', 'điện thoại', 'dien thoai']).trim()
    let joinDate = getRowValue(row, ['ngày vào làm', 'ngay vao lam', 'ngayvaolam', 'ngày vào', 'ngay vao', 'joindate', 'ngày làm việc']).trim()

    if (joinDate && joinDate.includes('T')) {
      joinDate = joinDate.split('T')[0]
    }

    const errors = []
    const warnings = []

    // 1. Kiểm tra bắt buộc: Họ và Tên
    if (!name) {
      errors.push('Thiếu Họ và Tên')
    }

    // 2. Tìm kiếm xem nhân sự đã có trong hệ thống chưa
    const matchEmp = (props.employees || []).find(e => {
      if (code && e.employeeCode && e.employeeCode.trim().toLowerCase() === code.toLowerCase()) return true
      if (!code && e.fullName && e.fullName.trim().toLowerCase() === name.toLowerCase()) return true
      return false
    })

    const isExisting = Boolean(matchEmp)
    const isUpdate = isExisting && updateExisting.value

    // Kiểm tra trùng mã trong cùng 1 file
    if (code) {
      const lowerCode = code.toLowerCase()
      if (seenCodesInFile.has(lowerCode)) {
        errors.push(`Trùng mã với dòng ${seenCodesInFile.get(lowerCode)} trong file`)
      } else {
        seenCodesInFile.set(lowerCode, index + 1)
      }
      if (existingCodes.has(lowerCode) && !updateExisting.value) {
        errors.push('Mã NV đã tồn tại (Chưa bật chế độ cập nhật)')
      }
    }

    // 3. Kiểm tra email hợp lệ (nếu có)
    if (email && (!email.includes('@') || !email.includes('.'))) {
      warnings.push('Email chưa đúng định dạng')
    }

    const isValid = errors.length === 0
    const hasWarning = warnings.length > 0
    const actionType = isUpdate ? 'update' : 'create'

    return {
      rowIndex: index + 1,
      rawRow: { ...row, email, englishName, updateExisting: updateExisting.value },
      code,
      name,
      englishName,
      deptName,
      title: title || 'Nhân viên',
      email,
      phone,
      joinDate: joinDate || new Date().toISOString().split('T')[0],
      actionType,
      matchedEmpId: matchEmp?.employeeID || null,
      isValid,
      hasWarning,
      errors,
      warnings
    }
  })
})

const validList = computed(() => parsedList.value.filter(i => i.isValid))
const errorList = computed(() => parsedList.value.filter(i => !i.isValid))
const createList = computed(() => validList.value.filter(i => i.actionType === 'create'))
const updateList = computed(() => validList.value.filter(i => i.actionType === 'update'))

const displayedList = computed(() => {
  if (activeFilter.value === 'valid') return validList.value
  if (activeFilter.value === 'error') return errorList.value
  if (activeFilter.value === 'update') return updateList.value
  return parsedList.value
})

const submitValidRows = () => {
  const payloadRows = validList.value.map(item => ({
    ...item.rawRow,
    _matchedEmpId: item.matchedEmpId,
    _actionType: item.actionType
  }))
  emit('submit', payloadRows)
}
</script>

<style scoped>
.import-option-box {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: var(--radius-md);
  padding: 10px 14px;
  margin-bottom: 14px;
}

.checkbox-option-lbl {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  font-size: 0.875rem;
  color: #166534;
  user-select: none;
}

.checkbox-option-lbl input[type="checkbox"] {
  width: 17px;
  height: 17px;
  accent-color: #16a34a;
  cursor: pointer;
}

.stat-pill.update {
  background: #fdf2f8;
  color: #9d174d;
  border-color: #fbcfe8;
}

.stat-pill.update:hover,
.stat-pill.update.active {
  background: #fce7f3;
  border-color: #db2777;
}

.val-badge.badge-update {
  background: #fdf2f8;
  color: #be185d;
  border: 1px solid #fbcfe8;
}

.email-preview-pill {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: #eff6ff;
  color: #1d4ed8;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
}

.row-update {
  background-color: #faf5ff !important;
}
.import-modal-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.import-step-card {
  display: flex;
  gap: 14px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
}

.step-num {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--primary, #2563eb);
  color: #ffffff;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  flex-shrink: 0;
}

.step-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 0;
}

.step-title {
  font-size: 0.95rem;
  font-weight: 700;
  color: #1e293b;
}

.step-desc {
  font-size: 0.85rem;
  color: #64748b;
  margin: 0;
  line-height: 1.4;
}

/* Drop Zone */
.import-drop-zone {
  border: 2px dashed #cbd5e1;
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  background: #ffffff;
  transition: all 0.2s ease;
}

.import-drop-zone:hover,
.import-drop-zone.dragging {
  border-color: var(--primary, #2563eb);
  background: #eff6ff;
}

.drop-icon {
  font-size: 2rem;
  display: block;
  margin-bottom: 6px;
}

.drop-text {
  font-size: 0.9rem;
  color: #334155;
  margin: 0 0 4px;
}

.drop-sub {
  font-size: 0.75rem;
  color: #94a3b8;
}

.drop-zone-file {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #f1f5f9;
  padding: 10px 16px;
  border-radius: 6px;
}

.file-icon {
  font-size: 1.5rem;
}

.file-details {
  flex: 1;
  text-align: left;
  display: flex;
  flex-direction: column;
}

.file-name {
  font-size: 0.9rem;
  color: #0f172a;
}

.file-size {
  font-size: 0.75rem;
  color: #64748b;
}

.btn-clear-file {
  background: #fee2e2;
  color: #dc2626;
  border: none;
  border-radius: 50%;
  width: 26px;
  height: 26px;
  cursor: pointer;
  font-weight: bold;
}

/* Stats Bar */
.preview-header-wrap {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.validation-stats-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.stat-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  padding: 4px 10px;
  border-radius: 6px;
  background: #ffffff;
  border: 1px solid #cbd5e1;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
}

.stat-pill:hover {
  transform: translateY(-1px);
}

.stat-pill.active {
  font-weight: 700;
  border-color: currentColor;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}

.stat-pill.total { color: #334155; }
.stat-pill.total.active { background: #f1f5f9; }

.stat-pill.valid { color: #059669; }
.stat-pill.valid.active { background: #ecfdf5; border-color: #059669; }

.stat-pill.error { color: #dc2626; }
.stat-pill.error.active { background: #fef2f2; border-color: #dc2626; }

.stat-pill.warning { color: #d97706; }
.stat-pill.warning.active { background: #fffbeb; border-color: #d97706; }

/* Preview Table */
.import-preview-table-wrap {
  max-height: 280px;
  overflow-y: auto;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  background: #ffffff;
}

.import-preview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.825rem;
  text-align: left;
}

.import-preview-table th {
  background: #f1f5f9;
  color: #475569;
  font-weight: 700;
  padding: 8px 10px;
  position: sticky;
  top: 0;
  border-bottom: 1px solid #cbd5e1;
  white-space: nowrap;
}

.import-preview-table td {
  padding: 8px 10px;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.import-preview-table tr.row-error {
  background: #fff1f2;
}

.import-preview-table tr.row-warning {
  background: #fffbeb;
}

.val-badge-wrap {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.val-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
  white-space: nowrap;
}

.val-badge.badge-ok {
  background: #ecfdf5;
  color: #059669;
}

.val-badge.badge-err {
  background: #fee2e2;
  color: #dc2626;
}

.val-badge.badge-warn {
  background: #fef3c7;
  color: #d97706;
}

.empty-filter-note {
  padding: 20px;
  text-align: center;
  color: #94a3b8;
  font-style: italic;
  font-size: 0.85rem;
}

.preview-footer-note {
  margin-top: 4px;
  font-size: 0.8rem;
}

.text-danger-note {
  color: #dc2626;
  font-weight: 600;
  margin: 0;
}

.text-success-note {
  color: #059669;
  font-weight: 600;
  margin: 0;
}
</style>
