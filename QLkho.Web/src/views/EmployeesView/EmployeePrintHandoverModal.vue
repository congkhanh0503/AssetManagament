<template>
  <Modal 
    :is-open="isOpen" 
    :title="isBulkEquipmentPrint ? `Biên Bản Bàn Giao Thiết Bị Ngoại Vi (${printingEquipmentList.length} món)` : `Biên Bản Bàn Giao: ${asset?.assetCode || ''}`"
    :subtitle="isPrintingComputer ? 'Mẫu bàn giao máy tính (Sheet: BG-PC)' : 'Mẫu bàn giao thiết bị ngoại vi (Sheet: Equidment)'"
    @close="$emit('close')"
    max-width="960px"
    :z-index="10050"
  >
    <div class="handover-preview-wrapper">
      <div id="print-handover-area" class="handover-document-form">
        <!-- 1. Header Tiêu Đề Chính Có Logo Bên Góc Trái (Cân Xứng 3 Cột) -->
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

        <!-- 2. Hàng Ngày Tháng (Handover Date / Return Date) -->
        <div class="h-doc-date-bar">
          <div class="h-date-item">
            <span class="h-date-lbl">Handover Date:</span>
            <strong class="h-date-val">{{ new Date().toLocaleDateString('vi-VN') }}</strong>
          </div>
          <div class="h-date-item">
            <span class="h-date-lbl">Return date:</span>
            <span class="h-date-val">---</span>
          </div>
        </div>

        <!-- TRƯỜNG HỢP 1: MẪU MÁY TÍNH (COMPUTER SPECS) -->
        <template v-if="isPrintingComputer">
          <!-- 3.1 BẢNG COMPUTER SPECS -->
          <div class="h-doc-section">
            <div class="h-section-header">COMPUTER SPECS</div>
            <table class="h-table-grid">
              <colgroup>
                <col style="width: 14%;" />
                <col style="width: 20%;" />
                <col style="width: 14%;" />
                <col style="width: 20%;" />
                <col style="width: 13%;" />
                <col style="width: 19%;" />
              </colgroup>
              <tbody>
                <tr>
                  <td class="h-lbl">Host Name</td>
                  <td class="h-val font-bold text-primary">{{ asset?.assetCode || '---' }}</td>
                  <td class="h-lbl">Serial No.</td>
                  <td class="h-val font-mono">{{ asset?.serialNumber || 'N/A' }}</td>
                  <td class="h-lbl">Asset Number</td>
                  <td class="h-val font-mono">{{ asset?.materialCode || asset?.assetCode || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Model</td>
                  <td class="h-val font-bold">{{ asset?.assetName || '---' }}</td>
                  <td class="h-lbl">CPU</td>
                  <td class="h-val">{{ assetSpecs.cpu || 'Core Ultra 5-125U' }}</td>
                  <td class="h-lbl">RAM</td>
                  <td class="h-val">{{ assetSpecs.ram || '16 GB' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Disk</td>
                  <td class="h-val">{{ assetSpecs.disk || '512 GB SSD' }}</td>
                  <td class="h-lbl">OS</td>
                  <td class="h-val">{{ assetSpecs.os || 'Windows 11' }}</td>
                  <td class="h-lbl">Display</td>
                  <td class="h-val">{{ assetSpecs.display || '14-inch' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Keyboard</td>
                  <td class="h-val">{{ assetSpecs.keyboard || 'Theo máy (Built-in)' }}</td>
                  <td class="h-lbl">Mouse</td>
                  <td class="h-val font-bold">{{ hasMouse ? '1' : 'N/A' }}</td>
                  <td class="h-lbl">Bag</td>
                  <td class="h-val font-bold">1</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 4.1 BẢNG USER INFORMATION -->
          <div class="h-doc-section">
            <div class="h-section-header">USER INFORMATION</div>
            <table class="h-table-grid">
              <colgroup>
                <col style="width: 13%;" />
                <col style="width: 20%;" />
                <col style="width: 17%;" />
                <col style="width: 18%;" />
                <col style="width: 13%;" />
                <col style="width: 19%;" />
              </colgroup>
              <tbody>
                <tr>
                  <td class="h-lbl">Employee ID</td>
                  <td class="h-val font-bold font-mono">{{ employee?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase" :title="employee?.fullName">{{ employee?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold font-mono" :title="employeeDepartmentDisplay">{{ employeeDepartmentDisplay }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary" :title="employee?.email">{{ employee?.email || '---' }}</td>
                  <td class="h-lbl h-lbl-compact">Account/OA/VPN/<br />PC Login</td>
                  <td class="h-val">
                    <input type="text" class="h-inline-input" v-model="accountInfo" placeholder="Nhập tài khoản..." />
                  </td>
                  <td class="h-lbl">Password</td>
                  <td class="h-val font-mono">
                    <input type="text" class="h-inline-input font-mono font-bold" v-model="passwordInfo" />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 5.1 END USER AGREEMENT CHO MÁY TÍNH -->
          <div class="h-doc-section">
            <div class="h-section-header">END USER AGREEMENT</div>
            <div class="h-agreement-box">
              <p class="h-agree-p">1. <strong>End User</strong> - have carried out the required backup action of data as I am solely responsible for my data.</p>
              <p class="h-agree-p">2. <strong>End User</strong> - am provided with correct computer and hardwares (including all corresponding chargers) listed above.</p>
              <p class="h-agree-p">3. <strong>End User</strong> - understand and agree to the hardware/software compatibility concerns that is mentioned below:</p>
              
              <div class="h-sub-rules">
                <div class="h-rule-row">
                  <span class="h-rule-tag">Hardware:</span>
                  <span>No changing without permission in all hardware parts such as CPU, RAM, Disk, Wifi adapter, internal mouse, keyboard…</span>
                </div>
                <div class="h-rule-row">
                  <span class="h-rule-tag">Software:</span>
                  <div class="h-rule-list">
                    <div>• No illegal installation and usage of softwares.</div>
                    <div>• Cracked/ Patched softwares are prohibited.</div>
                    <div>• Additional softwares must be reviewed by local IT department to be installed.</div>
                  </div>
                </div>
              </div>

              <p class="h-agree-p">4. In the event that the supplied equipment is lost or damaged, the End User shall bear full responsibility and shall compensate the Company for the repair costs or replace it with another machine of the same model and specifications.</p>
              <p class="h-agree-p">5. <strong>End User</strong> - take the responsibility to return all the hardwares that I received as listed above by the end of my last working day.</p>
              <p class="h-agree-p">6. <strong>End User</strong> - understand the above terms and commit to complying with them.</p>
            </div>
          </div>
        </template>

        <!-- TRƯỜNG HỢP 2: MẪU THIẾT BỊ NGOẠI VI (EQUIPMENT SPECS - CHUỘT, PHÍM, MÀN HÌNH...) -->
        <template v-else>
          <!-- 3.2 BẢNG EQUIPMENT SPECS DẠNG DANH SÁCH -->
          <div class="h-doc-section">
            <div class="h-section-header">EQUIPMENT SPECS</div>
            <table class="h-table-grid">
              <thead>
                <tr style="background: #f1f5f9; font-weight: bold;">
                  <td style="text-align: center; width: 45px; font-weight: bold;">No.</td>
                  <td style="width: 140px; font-weight: bold;">Type</td>
                  <td style="font-weight: bold;">Model</td>
                  <td style="width: 160px; font-weight: bold;">Serial No.</td>
                  <td style="text-align: center; width: 70px; font-weight: bold;">Quantity</td>
                  <td style="width: 180px; font-weight: bold;">Remark</td>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, idx) in displayEquipmentList" :key="idx">
                  <td style="text-align: center; font-weight: bold;">{{ idx + 1 }}</td>
                  <td style="font-weight: 600;">{{ item.categoryName || 'Equipment' }}</td>
                  <td><strong>{{ item.assetName || '---' }}</strong></td>
                  <td class="font-mono">{{ item.serialNumber || 'N/A' }}</td>
                  <td style="text-align: center; font-weight: bold;">1</td>
                  <td>{{ item.specifications || item.note || 'Hoạt động tốt' }}</td>
                </tr>
                <tr v-if="displayEquipmentList.length === 0">
                  <td colspan="6" style="text-align: center; color: #64748b; padding: 12px;">
                    Chưa có thông tin thiết bị
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 4.2 BẢNG USER INFORMATION -->
          <div class="h-doc-section">
            <div class="h-section-header">USER INFORMATION</div>
            <table class="h-table-grid">
              <colgroup>
                <col style="width: 13%;" />
                <col style="width: 20%;" />
                <col style="width: 17%;" />
                <col style="width: 18%;" />
                <col style="width: 13%;" />
                <col style="width: 19%;" />
              </colgroup>
              <tbody>
                <tr>
                  <td class="h-lbl">Employee ID</td>
                  <td class="h-val font-bold font-mono">{{ employee?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase" :title="employee?.fullName">{{ employee?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold font-mono" :title="employeeDepartmentDisplay">{{ employeeDepartmentDisplay }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary" :title="employee?.email">{{ employee?.email || '---' }}</td>
                  <td class="h-lbl h-lbl-compact">Account/OA/VPN/<br />PC Login</td>
                  <td class="h-val">
                    <input type="text" class="h-inline-input" v-model="accountInfo" placeholder="Nhập tài khoản..." />
                  </td>
                  <td class="h-lbl">Password</td>
                  <td class="h-val font-mono">
                    <input type="text" class="h-inline-input font-mono font-bold" v-model="passwordInfo" />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 5.2 END USER AGREEMENT CHO THIẾT BỊ NGOẠI VI -->
          <div class="h-doc-section">
            <div class="h-section-header">END USER AGREEMENT</div>
            <div class="h-agreement-box">
              <p class="h-agree-p">1. <strong>End User</strong> - am provided with correct equipment (including all corresponding chargers) listed above.</p>
              <p class="h-agree-p">2. <strong>End User</strong> - am responsible for the proper use and care of the equipment. Any damage caused by misuse, negligence, or external factors not covered by the manufacturer's warranty will be the responsibility of me. This includes, but is not limited to:</p>
              <div class="h-sub-rules" style="margin: 4px 0 4px 10px; font-size: 8pt; line-height: 1.5;">
                <div>• Spillage of liquids (e.g., water, coffee, etc.)</div>
                <div>• Physical damage due to dropping, impact, or improper handling</div>
                <div>• Unauthorized modifications or repairs</div>
                <div>• Exposure to extreme temperatures or environments</div>
              </div>
              <p class="h-agree-p">4. <strong>End User</strong> - take the responsibility to return the equipment that I received as listed above by the end of my last working day.</p>
              <p class="h-agree-p">5. <strong>End User</strong> - understand the above terms and commit to complying with them.</p>
            </div>
          </div>
        </template>

        <!-- 6. BẢNG CHỮ KÝ 3 BÊN (CHUẨN VIỀN KÍN EXCEL) -->
        <table class="h-sign-table">
          <thead>
            <tr>
              <th class="h-sign-th">Delivery side</th>
              <th class="h-sign-th">Recipient</th>
              <th class="h-sign-th">Manager Confirm</th>
            </tr>
            <tr>
              <th class="h-sign-sub">IT confirm</th>
              <th class="h-sign-sub">User’s Signature</th>
              <th class="h-sign-sub">Manager Confirm</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td class="h-sign-cell">
                <div class="h-sign-space"></div>
              </td>
              <td class="h-sign-cell">
                <div class="h-sign-space"></div>
              </td>
              <td class="h-sign-cell">
                <div class="h-sign-space"></div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- THANH NÚT HÀNH ĐỘNG DƯỚI CÙNG (KHÔNG IN) -->
      <div class="handover-action-bar no-print">
        <!-- Nút Tùy Chọn Cấp Kèm Chuột Cho Laptop -->
        <div v-if="isPrintingComputer" class="action-toggle-group">
          <button 
            type="button" 
            class="btn-toggle-mouse" 
            :class="{ active: hasMouse }"
            @click="hasMouse = !hasMouse"
            title="Bấm để bật/tắt cấp kèm chuột"
          >
            <span class="btn-toggle-icon">{{ hasMouse ? '✅' : '⬜' }}</span>
            <span>Cấp Kèm Chuột: <strong>{{ hasMouse ? 'Có (Mouse = 1)' : 'Không (Mouse = N/A)' }}</strong></span>
          </button>
        </div>

        <div style="display: flex; gap: 8px; margin-left: auto;">
          <button type="button" class="btn btn-secondary" @click="$emit('close')">
            Đóng
          </button>
          <button type="button" class="btn btn-primary" @click="handlePrintNative">
            🖨️ In Biên Bản (A4 / PDF)
          </button>
        </div>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import QRCode from 'qrcode'
import Modal from '@/components/common/Modal.vue'
import companyLogo from '@/components/assets/img/logo.png'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  asset: { type: Object, default: null },
  employee: { type: Object, default: null },
  printingEquipmentList: { type: Array, default: () => [] },
  isBulkEquipmentPrint: { type: Boolean, default: false },
  categories: { type: Array, default: () => [] },
  departments: { type: Array, default: () => [] }
})

const emit = defineEmits(['close'])

const hasMouse = ref(false)
const accountInfo = ref('')
const passwordInfo = ref('@9e7w9qS@KTF')
const qrCodeDataUrl = ref('')

const qrCaption = computed(() => {
  const empCode = props.employee?.employeeCode || 'EMP'
  const astCode = props.asset?.assetCode || (props.printingEquipmentList?.[0]?.assetCode) || 'EQ'
  return `HB-${empCode}-${astCode}`
})

const generateQrCode = async () => {
  const empCode = props.employee?.employeeCode || ''
  const astCode = props.asset?.assetCode || (props.printingEquipmentList?.[0]?.assetCode) || ''
  const today = new Date().toISOString().slice(0, 10).replace(/-/g, '')
  
  // Format token nhận diện: HB|{empCode}|{astCode}|{today}
  const payload = `HB|${empCode}|${astCode}|${today}`
  try {
    qrCodeDataUrl.value = await QRCode.toDataURL(payload, {
      width: 90,
      margin: 1,
      errorCorrectionLevel: 'M',
      color: {
        dark: '#000000',
        light: '#ffffff'
      }
    })
  } catch (err) {
    console.warn('Lỗi sinh mã QR:', err)
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    generateQrCode()
    nextTick(() => {
      const modalBodies = document.querySelectorAll('.modal-body')
      modalBodies.forEach(b => { b.scrollTop = 0 })
      const previewWrappers = document.querySelectorAll('.handover-preview-wrapper')
      previewWrappers.forEach(p => { p.scrollTop = 0 })
    })

    // Tự động kiểm tra nếu có ghi chú hoặc thông số có chuột thì bật true
    const noteStr = (props.asset?.note || '').toLowerCase()
    hasMouse.value = Boolean(noteStr.includes('chuột') || noteStr.includes('mouse'))

    if (props.employee?.email && !accountInfo.value) {
      accountInfo.value = props.employee.email.split('@')[0]
    }
    if (!passwordInfo.value) {
      passwordInfo.value = '@9e7w9qS@KTF'
    }
  }
})

const employeeDepartmentDisplay = computed(() => {
  if (!props.employee) return '---'
  if (props.employee.departmentCode) return props.employee.departmentCode
  if (props.asset?.holderDepartmentCode) return props.asset.holderDepartmentCode
  
  const dept = props.departments.find(d => 
    (props.employee.departmentID && d.departmentID === props.employee.departmentID) ||
    (props.employee.departmentName && (
      d.departmentName?.toLowerCase() === props.employee.departmentName?.toLowerCase() ||
      d.departmentCode?.toLowerCase() === props.employee.departmentName?.toLowerCase()
    ))
  )
  return dept?.departmentCode || props.employee.departmentCode || props.employee.departmentName || '---'
})

const isLaptopCategory = (categoryId, categoryName) => {
  const cat = props.categories.find(c => c.categoryID === categoryId)
  const name = (cat?.categoryName || categoryName || '').toLowerCase().trim()
  const code = (cat?.categoryCode || '').toLowerCase().trim()
  return name.includes('laptop') || 
         name.includes('xách tay') || 
         name.includes('xachtay') || 
         name.includes('notebook') || 
         name.includes('macbook') || 
         code.includes('laptop') || 
         code.includes('nb')
}

const isPrintingComputer = computed(() => {
  if (props.isBulkEquipmentPrint) return false
  if (!props.asset) return false
  return isLaptopCategory(props.asset.categoryID, props.asset.categoryName)
})

const displayEquipmentList = computed(() => {
  if (props.isBulkEquipmentPrint && props.printingEquipmentList && props.printingEquipmentList.length > 0) {
    return props.printingEquipmentList
  }
  if (props.asset) {
    return [props.asset]
  }
  if (props.printingEquipmentList && props.printingEquipmentList.length > 0) {
    return props.printingEquipmentList
  }
  return []
})

const parseSpecsIntoForm = (specStr, noteStr, targetForm) => {
  if (specStr) {
    const cpuM = specStr.match(/CPU:\s*([^\|]+)/i)
    if (cpuM) targetForm.cpu = cpuM[1].trim()
    const ramM = specStr.match(/RAM:\s*([^\|]+)/i)
    if (ramM) targetForm.ram = ramM[1].trim()
    const diskM = specStr.match(/(?:Disk|Ổ cứng|SSD|HDD):\s*([^\|]+)/i)
    if (diskM) targetForm.disk = diskM[1].trim()
    const osM = specStr.match(/OS:\s*([^\|]+)/i)
    if (osM) targetForm.os = osM[1].trim()
    const dispM = specStr.match(/(?:Display|Màn hình):\s*([^\|]+)/i)
    if (dispM) targetForm.display = dispM[1].trim()
  }
  if (noteStr) {
    const kbM = noteStr.match(/Keyboard:\s*([^\|]+)/i)
    if (kbM) targetForm.keyboard = kbM[1].trim()
    const msM = noteStr.match(/Mouse:\s*([^\|]+)/i)
    if (msM) targetForm.mouse = msM[1].trim()
    const chgM = noteStr.match(/Charger:\s*([^\|]+)/i)
    if (chgM) targetForm.charger = chgM[1].trim()
  }
}

const assetSpecs = computed(() => {
  if (!props.asset) {
    return {
      cpu: 'Core Ultra 5-125U',
      ram: '16 GB',
      disk: '512 GB SSD',
      os: 'Windows 11',
      display: '14-inch',
      keyboard: 'Theo máy (Built-in)',
      mouse: 'Theo máy / N/A',
      charger: 'Kèm củ sạc + Dây nguồn zin'
    }
  }
  const obj = {}
  parseSpecsIntoForm(props.asset.specifications, props.asset.note, obj)
  return obj
})

const handlePrintNative = () => {
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

.action-toggle-group {
  display: flex;
  align-items: center;
}

.btn-toggle-mouse {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  background: #f8fafc;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.85rem;
  color: #334155;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
}

.btn-toggle-mouse:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.btn-toggle-mouse.active {
  background: #eff6ff;
  border-color: #3b82f6;
  color: #1d4ed8;
  font-weight: 600;
}

.btn-toggle-icon {
  font-size: 1rem;
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
