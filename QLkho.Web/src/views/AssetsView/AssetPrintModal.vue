<template>
  <Modal 
    :is-open="isOpen" 
    :title="`Biên Bản Bàn Giao Thiết Bị: ${asset?.assetCode || ''}`"
    :subtitle="isPrintingComputer ? 'Mẫu bàn giao máy tính (Sheet: BG-PC)' : 'Mẫu bàn giao thiết bị ngoại vi (Sheet: Equidment)'"
    @close="$emit('close')"
    max-width="960px"
    :z-index="10050"
  >
    <div class="handover-preview-wrapper">
      <div id="print-handover-area" class="handover-document-form">
        <!-- 1. Header Tiêu Đề Chính Có Logo Bên Góc Trái -->
        <div class="h-doc-header-wrap">
          <div class="h-logo-box">
            <img :src="companyLogo" alt="Logo" class="h-company-logo" />
          </div>
          <div class="h-doc-title-box">
            <h2 class="h-doc-title-en">Confirm equipment delivery information</h2>
            <h3 class="h-doc-title-vn">Xác nhận thông tin cung cấp thiết bị</h3>
          </div>
          <div class="h-header-spacer"></div>
        </div>

        <!-- 2. Hàng Ngày Tháng -->
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
              <tbody>
                <tr>
                  <td class="h-lbl">Host Name</td>
                  <td class="h-val font-bold text-primary">{{ asset?.assetCode || '---' }}</td>
                  <td class="h-lbl">Service Tag</td>
                  <td class="h-val font-mono">{{ asset?.serialNumber || 'N/A' }}</td>
                  <td class="h-lbl">Asset Number</td>
                  <td class="h-val font-mono">{{ asset?.materialCode || asset?.assetCode || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Model</td>
                  <td class="h-val font-bold">{{ asset?.assetName || '---' }}</td>
                  <td class="h-lbl">CPU</td>
                  <td class="h-val">{{ specs.cpu || 'Core Ultra 5-125U' }}</td>
                  <td class="h-lbl">RAM</td>
                  <td class="h-val">{{ specs.ram || '16 GB' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Disk</td>
                  <td class="h-val">{{ specs.disk || '512 GB SSD' }}</td>
                  <td class="h-lbl">OS</td>
                  <td class="h-val">{{ specs.os || 'Windows 11' }}</td>
                  <td class="h-lbl">Display</td>
                  <td class="h-val">{{ specs.display || '14-inch' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Keyboard</td>
                  <td class="h-val">{{ specs.keyboard || 'Theo máy (Built-in)' }}</td>
                  <td class="h-lbl">Mouse</td>
                  <td class="h-val font-bold">{{ isWithMouse ? '1' : 'N/A' }}</td>
                  <td class="h-lbl">Remark</td>
                  <td class="h-val">{{ specs.charger || 'N/A' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 4.1 BẢNG USER INFORMATION -->
          <div class="h-doc-section">
            <div class="h-section-header">USER INFORMATION</div>
            <table class="h-table-grid">
              <tbody>
                <tr>
                  <td class="h-lbl">Employee ID</td>
                  <td class="h-val font-bold font-mono">{{ holder?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase">{{ holder?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold">{{ holder?.departmentName || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary">{{ holder?.email || '---' }}</td>
                  <td class="h-lbl">Owner type</td>
                  <td class="h-val">Single</td>
                  <td class="h-lbl">Remark</td>
                  <td class="h-val">N/A</td>
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

        <!-- TRƯỜNG HỢP 2: MẪU THIẾT BỊ NGOẠI VI -->
        <template v-else>
          <!-- 3.2 BẢNG EQUIPMENT SPECS -->
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
                <tr>
                  <td style="text-align: center; font-weight: bold;">1</td>
                  <td style="font-weight: 600;">{{ asset?.categoryName || 'Equipment' }}</td>
                  <td><strong>{{ asset?.assetName || '---' }}</strong></td>
                  <td class="font-mono">{{ asset?.serialNumber || 'N/A' }}</td>
                  <td style="text-align: center; font-weight: bold;">1</td>
                  <td>{{ asset?.specifications || asset?.note || 'Hoạt động tốt' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 4.2 BẢNG USER INFORMATION -->
          <div class="h-doc-section">
            <div class="h-section-header">USER INFORMATION</div>
            <table class="h-table-grid">
              <tbody>
                <tr>
                  <td class="h-lbl">Employee ID</td>
                  <td class="h-val font-bold font-mono">{{ holder?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase">{{ holder?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold">{{ holder?.departmentName || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary">{{ holder?.email || '---' }}</td>
                  <td class="h-lbl">Owner type</td>
                  <td class="h-val">Single</td>
                  <td class="h-lbl">Remark</td>
                  <td class="h-val">N/A</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- 5.2 END USER AGREEMENT CHO THIẾT BỊ NGOẠI VI -->
          <div class="h-doc-section">
            <div class="h-section-header">END USER AGREEMENT</div>
            <div class="h-agreement-box">
              <p class="h-agree-p">1. <strong>End User</strong> - am provided with correct equipment (including all corresponding chargers) listed above.</p>
              <p class="h-agree-p">2. <strong>End User</strong> - am responsible for the proper use and care of the equipment. Any damage caused by misuse, negligence, or external factors not covered by the manufacturer's warranty will be the responsibility of me.</p>
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

        <!-- 6. BẢNG CHỮ KÝ 3 BÊN -->
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
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
              <td class="h-sign-cell"><div class="h-sign-space"></div></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="modal-actions-right no-print" style="margin-top: 20px; display: flex; justify-content: space-between; align-items: center;">
      <div v-if="isPrintingComputer" class="action-toggle-group">
        <button 
          type="button" 
          class="btn-toggle-mouse" 
          :class="{ active: isWithMouse }"
          @click="isWithMouse = !isWithMouse"
          title="Bấm để bật/tắt cấp kèm chuột"
        >
          <span class="btn-toggle-icon">{{ isWithMouse ? '✅' : '⬜' }}</span>
          <span>Cấp Kèm Chuột: <strong>{{ isWithMouse ? 'Có (Mouse = 1)' : 'Không (Mouse = N/A)' }}</strong></span>
        </button>
      </div>
      <div style="display: flex; gap: 8px; margin-left: auto;">
        <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
        <button type="button" class="btn btn-primary" @click="handlePrint">
          🖨️ In Biên Bản (A4 / PDF)
        </button>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'
import companyLogo from '@/components/assets/img/logo.png'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  asset: {
    type: Object,
    default: null
  },
  employees: {
    type: Array,
    default: () => []
  },
  categories: {
    type: Array,
    default: () => []
  }
})

defineEmits(['close'])

const isWithMouse = ref(false)

watch(() => props.asset, (newAsset) => {
  if (newAsset) {
    const noteStr = (newAsset.note || '').toLowerCase()
    isWithMouse.value = Boolean(noteStr.includes('chuột') || noteStr.includes('mouse'))
  }
}, { immediate: true })

const isPrintingComputer = computed(() => {
  if (!props.asset) return false
  const cat = props.categories.find(c => c.categoryID === props.asset.categoryID)
  const name = (cat?.categoryName || props.asset.categoryName || '').toLowerCase().trim()
  const code = (cat?.categoryCode || '').toLowerCase().trim()
  return name.includes('laptop') || name.includes('xách tay') || name.includes('notebook') || name.includes('macbook') || code.includes('nb') || code.includes('laptop')
})

const holder = computed(() => {
  if (!props.asset) return null
  const emp = props.employees.find(e => e.employeeID === props.asset.currentHolderID)
  return {
    employeeCode: props.asset.holderCode || emp?.employeeCode || '',
    fullName: props.asset.holderName || emp?.fullName || '',
    departmentName: props.asset.holderDepartment || emp?.departmentName || '',
    email: props.asset.holderEmail || emp?.email || ''
  }
})

const specs = computed(() => {
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

  const obj = {
    cpu: '',
    ram: '16 GB',
    disk: '512 GB SSD',
    os: 'Windows 11',
    display: '14-inch',
    keyboard: 'Theo máy (Built-in)',
    mouse: '1 Chuột quang',
    charger: 'Kèm củ sạc + Dây nguồn zin'
  }

  const specsStr = props.asset.specifications || ''
  if (specsStr) {
    const cpuM = specsStr.match(/CPU:\s*([^\|]+)/i) || specsStr.match(/(Core\s+i[3579][^\,\|]+|Ryzen\s+[3579][^\,\|]+|M[1234][^\,\|]*|Ultra\s+\d[^\,\|]*)/i)
    if (cpuM) obj.cpu = (cpuM[1] || cpuM[0]).trim()

    const ramM = specsStr.match(/RAM:\s*([^\|]+)/i) || specsStr.match(/(\d+\s*GB)/i)
    if (ramM) obj.ram = (ramM[1] || ramM[0]).trim()

    const diskM = specsStr.match(/Disk:\s*([^\|]+)/i) || specsStr.match(/(\d+\s*(GB|TB)(\s*SSD)?)/i)
    if (diskM) obj.disk = (diskM[1] || diskM[0]).trim()

    const osM = specsStr.match(/OS:\s*([^\|]+)/i) || specsStr.match(/(Windows\s*\d+(\s*Pro)?|macOS|Ubuntu)/i)
    if (osM) obj.os = (osM[1] || osM[0]).trim()

    const dispM = specsStr.match(/Display:\s*([^\|]+)/i) || specsStr.match(/(\d+(\.\d+)?-inch(\s*FHD)?)/i)
    if (dispM) obj.display = (dispM[1] || dispM[0]).trim()
  }

  const noteStr = props.asset.note || ''
  if (noteStr) {
    const kbM = noteStr.match(/Keyboard:\s*([^\|]+)/i)
    if (kbM) obj.keyboard = kbM[1].trim()
    const msM = noteStr.match(/Mouse:\s*([^\|]+)/i)
    if (msM) obj.mouse = msM[1].trim()
    const chgM = noteStr.match(/Charger:\s*([^\|]+)/i)
    if (chgM) obj.charger = chgM[1].trim()
  }

  return obj
})

const handlePrint = () => {
  window.print()
}
</script>

<style scoped>
.handover-preview-wrapper {
  background: #cbd5e1;
  padding: 16px;
  border-radius: 8px;
  overflow-x: auto;
  display: flex;
  justify-content: center;
}

.handover-document-form {
  background: #ffffff;
  width: 210mm;
  min-height: 297mm;
  padding: 15mm 20mm;
  color: #000000;
  font-family: 'Times New Roman', Times, serif;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
  box-sizing: border-box;
}

.h-doc-header-wrap {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.h-company-logo {
  height: 48px;
  object-fit: contain;
}

.h-doc-title-box {
  text-align: center;
  flex: 1;
}

.h-doc-title-en {
  font-size: 14pt;
  font-weight: bold;
  text-transform: uppercase;
  margin: 0;
}

.h-doc-title-vn {
  font-size: 11pt;
  font-weight: normal;
  font-style: italic;
  margin: 2px 0 0 0;
}

.h-header-spacer {
  width: 80px;
}

.h-doc-date-bar {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
  font-size: 9.5pt;
  border-bottom: 1px solid #000;
  padding-bottom: 4px;
}

.h-doc-section {
  margin-bottom: 12px;
}

.h-section-header {
  font-weight: bold;
  font-size: 10pt;
  background: #e2e8f0;
  padding: 3px 6px;
  border: 1px solid #000;
  border-bottom: none;
}

.h-table-grid {
  width: 100%;
  border-collapse: collapse;
  font-size: 9pt;
}

.h-table-grid td, .h-table-grid th {
  border: 1px solid #000;
  padding: 4px 6px;
}

.h-lbl {
  background: #f8fafc;
  font-weight: 600;
  width: 15%;
}

.h-val {
  width: 18%;
}

.h-agreement-box {
  border: 1px solid #000;
  padding: 8px 10px;
  font-size: 8.5pt;
  line-height: 1.4;
}

.h-agree-p {
  margin: 4px 0;
}

.h-sub-rules {
  margin-left: 12px;
}

.h-rule-row {
  display: flex;
  gap: 6px;
  margin: 2px 0;
}

.h-rule-tag {
  font-weight: bold;
  min-width: 65px;
}

.h-sign-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15px;
  text-align: center;
  font-size: 9.5pt;
}

.h-sign-th {
  border: 1px solid #000;
  font-weight: bold;
  padding: 4px;
  width: 33.33%;
  background: #f1f5f9;
}

.h-sign-sub {
  border: 1px solid #000;
  font-style: italic;
  font-weight: normal;
  padding: 2px;
}

.h-sign-cell {
  border: 1px solid #000;
  height: 80px;
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
</style>
