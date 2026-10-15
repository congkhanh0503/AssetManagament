<template>
  <Modal
    :is-open="isOpen"
    :title="`Biên Bản Bàn Giao Thiết Bị: ${order?.orderCode || ''}`"
    :subtitle="`Bàn giao cho bộ phận ${deptCode}`"
    @close="$emit('close')"
    max-width="960px"
    :z-index="10050"
  >
    <div class="handover-preview-wrapper">
      <div id="print-handover-area" class="handover-document-form">
        <div class="h-doc-header-wrap">
          <div class="h-logo-box">
            <img :src="companyLogo" alt="Logo" class="h-company-logo" />
          </div>
          <div class="h-doc-title-box">
            <h2 class="h-doc-title-en">Confirm equipment delivery information</h2>
            <h3 class="h-doc-title-vn">Xác nhận thông tin cung cấp thiết bị</h3>
          </div>
          <div class="h-qr-box">
            <img v-if="qrCodeDataUrl" :src="qrCodeDataUrl" alt="QR Code" class="h-qr-img" />
            <div class="h-qr-caption">{{ qrCaption }}</div>
          </div>
        </div>

        <div class="h-doc-date-bar">
          <div class="h-date-item">
            <span class="h-date-lbl">Handover Date:</span>
            <strong class="h-date-val">{{ new Date().toLocaleDateString('vi-VN') }}</strong>
          </div>
          <div class="h-date-item">
            <span class="h-date-lbl">Order:</span>
            <span class="h-date-val">{{ order?.prCode ? `${order.orderCode} (PR: ${order.prCode})` : order?.orderCode }}</span>
          </div>
        </div>

        <div class="h-doc-section">
          <div class="h-section-header">EQUIPMENT SPECS</div>
          <table class="h-table-grid">
            <thead>
              <tr style="background: #f1f5f9; font-weight: bold;">
                <td style="text-align: center; width: 45px; font-weight: bold;">No.</td>
                <td style="width: 120px; font-weight: bold;">Type</td>
                <td style="font-weight: bold;">Model</td>
                <td style="width: 140px; font-weight: bold;">Serial No.</td>
                <td style="text-align: center; width: 70px; font-weight: bold;">Quantity</td>
                <td style="width: 160px; font-weight: bold;">Remark</td>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, idx) in rows" :key="row.key">
                <td style="text-align: center; font-weight: bold;">{{ idx + 1 }}</td>
                <td style="font-weight: 600;">{{ row.type }}</td>
                <td><strong>{{ row.model }}</strong></td>
                <td class="font-mono">{{ row.serial }}</td>
                <td style="text-align: center; font-weight: bold;">{{ row.quantity }}</td>
                <td>{{ row.remark }}</td>
              </tr>
              <tr v-if="rows.length === 0">
                <td colspan="6" style="text-align: center;">---</td>
              </tr>
              <tr>
                <td colspan="4" style="text-align: right; font-weight: bold;">Total</td>
                <td style="text-align: center; font-weight: bold;">{{ totalQuantity }}</td>
                <td></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="h-doc-section">
          <div class="h-section-header">DEPARTMENT INFORMATION</div>
          <table class="h-table-grid">
            <colgroup>
              <col style="width: 15%;" />
              <col style="width: 20%;" />
              <col style="width: 15%;" />
              <col style="width: 20%;" />
              <col style="width: 12%;" />
              <col style="width: 18%;" />
            </colgroup>
            <tbody>
              <tr>
                <td class="h-lbl">Department</td>
                <td class="h-val font-bold font-mono">{{ deptCode }}</td>
                <td class="h-lbl">Receiver</td>
                <td class="h-val font-bold text-uppercase" colspan="3">
                  <input type="text" class="h-inline-input font-bold" v-model="receiverName" list="dept-receiver-list" placeholder="Người nhận..." @change="onReceiverChange" />
                  <datalist id="dept-receiver-list">
                    <option v-for="e in deptEmployees" :key="e.employeeID" :value="e.fullName"></option>
                  </datalist>
                </td>
              </tr>
              <tr>
                <td class="h-lbl">Dept. Manager</td>
                <td class="h-val font-bold">{{ managerNameOnly }}</td>
                <td class="h-lbl">Receiver Email</td>
                <td class="h-val font-bold text-primary" colspan="3">
                  <input type="text" class="h-inline-input font-bold" v-model="receiverEmail" placeholder="Email người nhận..." />
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="h-doc-section">
          <div class="h-section-header">END USER AGREEMENT</div>
          <div class="h-agreement-box">
            <p class="h-agree-p">1. <strong>Department</strong> - confirms having received the correct quantity of equipment (including all corresponding accessories) listed above.</p>
            <p class="h-agree-p">2. <strong>Department</strong> - is responsible for the proper use and care of the equipment. Any damage caused by misuse, negligence, or external factors not covered by the manufacturer's warranty will be the responsibility of the department.</p>
            <div class="h-sub-rules" style="margin: 4px 0 4px 10px; font-size: 8pt; line-height: 1.5;">
              <div>• Spillage of liquids (e.g., water, coffee, etc.)</div>
              <div>• Physical damage due to dropping, impact, or improper handling</div>
              <div>• Unauthorized modifications or repairs</div>
              <div>• Exposure to extreme temperatures or environments</div>
            </div>
            <p class="h-agree-p">3. <strong>Department</strong> - takes the responsibility to return the equipment received as listed above when requested by the Company.</p>
            <p class="h-agree-p">4. <strong>Department</strong> - understands the above terms and commits to complying with them.</p>
          </div>
        </div>

        <table class="h-sign-table">
          <thead>
            <tr>
              <th class="h-sign-th">Delivery side</th>
              <th class="h-sign-th">Recipient</th>
              <th class="h-sign-th">Manager Confirm</th>
            </tr>
            <tr>
              <th class="h-sign-sub">IT confirm</th>
              <th class="h-sign-sub">Department Representative’s Signature</th>
              <th class="h-sign-sub">Manager Confirm</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="modal-actions-right no-print" style="margin-top: 20px; display: flex; justify-content: flex-end; gap: 8px;">
      <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
      <button type="button" class="btn btn-primary" @click="handlePrint">
        🖨️ In Biên Bản (A4 / PDF)
      </button>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { employeesApi } from '@/api/client'
import QRCode from 'qrcode'
import Modal from '@/components/common/Modal.vue'
import companyLogo from '@/components/assets/img/logo.png'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  order: { type: Object, default: null },
  departments: { type: Array, default: () => [] }
})

defineEmits(['close'])

const receiverName = ref('')
const receiverEmail = ref('')
const employees = ref([])
const qrCodeDataUrl = ref('')

const dept = computed(() => props.departments.find(d => d.departmentID === props.order?.departmentID) || null)
const deptCode = computed(() => dept.value?.departmentCode || props.order?.departmentName || '---')
const deptName = computed(() => dept.value?.departmentName || props.order?.departmentName || '---')

// Trưởng bộ phận lưu dạng "Tên/email"
const managerRaw = computed(() => dept.value?.managerName || '')
const managerNameOnly = computed(() => managerRaw.value.split('/')[0].trim() || '---')
const deptEmployees = computed(() =>
  employees.value.filter(e => e.departmentID === props.order?.departmentID && e.fullName && e.status !== 'Resigned')
)

// Chọn tên nhân sự trong phòng ban -> tự điền email
const onReceiverChange = () => {
  const emp = deptEmployees.value.find(e => e.fullName === receiverName.value)
  if (emp?.email) receiverEmail.value = emp.email
}

const managerEmail = computed(() => managerRaw.value.includes('/') ? managerRaw.value.split('/').slice(1).join('/').trim() : '')

// Thiết bị không có serial -> tính theo Quantity của từng dòng trong đơn
const rows = computed(() => {
  const order = props.order
  if (!order) return []
  return (order.items || []).map(it => {
    const serials = (order.devices || [])
      .filter(d => d.orderItemID === it.orderItemID && d.serialNumber)
      .map(d => d.serialNumber)
    return {
      key: it.orderItemID,
      type: it.categoryName || 'Equipment',
      model: it.modelName,
      serial: serials.length ? serials.join(', ') : 'N/A',
      quantity: it.receivedQuantity > 0 ? it.receivedQuantity : it.expectedQuantity,
      remark: it.specifications || it.note || 'Hoạt động tốt'
    }
  })
})

const totalQuantity = computed(() => rows.value.reduce((sum, r) => sum + Number(r.quantity || 0), 0))

const qrCaption = computed(() => `OD-${props.order?.orderCode || ''}`)

const generateQrCode = async () => {
  const today = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  try {
    qrCodeDataUrl.value = await QRCode.toDataURL(`OD|${props.order?.orderCode || ''}|${deptCode.value}|${today}`, {
      width: 90,
      margin: 1,
      errorCorrectionLevel: 'M'
    })
  } catch (err) {
    console.warn('Lỗi sinh mã QR:', err)
  }
}

watch(() => props.isOpen, async (open) => {
  if (open) {
    receiverName.value = managerNameOnly.value === '---' ? '' : managerNameOnly.value
    receiverEmail.value = managerEmail.value
    if (employees.value.length === 0) {
      try { employees.value = (await employeesApi.getAll()) || [] } catch (e) { /* bỏ qua, vẫn nhập tay được */ }
    }
    generateQrCode()
  }
})

const handlePrint = () => {
  window.print()
}
</script>

<style scoped>
.handover-preview-wrapper {
  background: #cbd5e1;
  padding: 12px;
  border-radius: 8px;
  overflow-x: auto;
  overflow-y: auto;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  width: 100%;
  box-sizing: border-box;
}

.handover-document-form {
  background: #ffffff;
  width: 100%;
  max-width: 210mm;
  min-height: auto;
  padding: 10mm 14mm;
  color: #000000;
  font-family: 'Times New Roman', Times, serif;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
  box-sizing: border-box;
}

.h-doc-header-wrap {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.h-company-logo {
  height: 44px;
  object-fit: contain;
}

.h-doc-title-box {
  text-align: center;
  flex: 1;
}

.h-doc-title-en {
  font-size: 13.5pt;
  font-weight: bold;
  text-transform: uppercase;
  margin: 0;
  letter-spacing: 0.02em;
}

.h-doc-title-vn {
  font-size: 10.5pt;
  font-weight: normal;
  font-style: italic;
  margin: 1px 0 0 0;
}

.h-header-spacer {
  width: 80px;
}

.h-doc-date-bar {
  display: flex;
  justify-content: space-between;
  margin-bottom: 6px;
  font-size: 9pt;
  border-bottom: 1.5px solid #000;
  padding-bottom: 3px;
}

.h-doc-section {
  margin-bottom: 6px;
}

.h-section-header {
  font-weight: bold;
  font-size: 9.5pt;
  background: #e2e8f0;
  padding: 2.5px 6px;
  border: 1px solid #000;
  border-bottom: none;
}

.h-table-grid {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
  font-size: 8.5pt;
  margin-bottom: 0;
}

.h-table-grid td, .h-table-grid th {
  border: 1px solid #000000;
  padding: 2.5px 5px;
  vertical-align: middle;
  height: 23px;
  box-sizing: border-box;
}

.h-lbl {
  background: #f8fafc;
  font-weight: 700;
  color: #000000;
  font-size: 8pt;
  white-space: normal;
  text-align: left;
}

.h-lbl-compact {
  font-size: 7.25pt !important;
  line-height: 1.15 !important;
  padding: 1.5px 3px !important;
  letter-spacing: -0.02em;
  white-space: normal !important;
  word-break: break-word !important;
  overflow-wrap: anywhere !important;
}

.h-val {
  background: #ffffff;
  color: #000000;
  font-size: 8.5pt;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: normal;
  word-break: break-word;
  line-height: 1.2;
}

.h-inline-input {
  width: 100%;
  border: 1px dashed #cbd5e1;
  background: #f8fafc;
  padding: 1px 4px;
  font-size: 8.5pt;
  font-family: inherit;
  color: #0f172a;
  border-radius: 2px;
  outline: none;
  box-sizing: border-box;
  height: 20px;
}

.h-inline-input:focus {
  border-color: #2563eb;
  background: #ffffff;
}

.h-agreement-box {
  border: 1px solid #000;
  padding: 5px 8px;
  font-size: 8pt;
  line-height: 1.35;
}

.h-agree-p {
  margin: 2px 0;
}

.h-sub-rules {
  margin-left: 10px;
  margin-top: 2px;
  margin-bottom: 2px;
}

.h-rule-row {
  display: flex;
  gap: 4px;
  margin: 1px 0;
}

.h-rule-tag {
  font-weight: bold;
  min-width: 60px;
}

.h-sign-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 6px;
  text-align: center;
  font-size: 9pt;
}

.h-sign-th {
  border: 1px solid #000;
  font-weight: bold;
  padding: 3px;
  width: 33.33%;
  background: #f1f5f9;
}

.h-sign-sub {
  border: 1px solid #000;
  font-style: italic;
  font-weight: normal;
  padding: 2px;
  font-size: 8pt;
}

.h-sign-cell {
  border: 1px solid #000;
  height: 60px;
  vertical-align: bottom;
}

.btn-toggle-mouse {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1d4ed8;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 0.85rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-toggle-mouse.active {
  background: #dbeafe;
  border-color: #3b82f6;
  font-weight: bold;
}

@media print {
  .handover-preview-wrapper {
    background: transparent !important;
    padding: 0 !important;
  }

  .handover-document-form {
    box-shadow: none !important;
    padding: 0 !important;
    width: 100% !important;
    min-height: auto !important;
  }

  .h-table-grid {
    table-layout: fixed !important;
    font-size: 8.5pt !important;
  }
  
  .h-table-grid td, .h-table-grid th {
    border: 1px solid #000000 !important;
    padding: 2px 4px !important;
  }

  .h-lbl {
    background: #f8fafc !important;
    -webkit-print-color-adjust: exact !important;
    print-color-adjust: exact !important;
  }

  .h-inline-input {
    border: none !important;
    background: transparent !important;
    padding: 0 !important;
    font-size: 8.5pt !important;
    color: #000000 !important;
    box-shadow: none !important;
    height: auto !important;
  }
}

/* QR Code Box */
.h-qr-box {
  width: 90px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.h-qr-img {
  width: 72px;
  height: 72px;
  display: block;
}

.h-qr-caption {
  font-size: 6.5pt;
  font-weight: 700;
  font-family: monospace;
  color: #0f172a;
  margin-top: 1px;
  text-align: center;
  white-space: nowrap;
}
</style>
