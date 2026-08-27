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
          <div class="h-header-spacer"></div>
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
                  <td class="h-lbl">Remark</td>
                  <td class="h-val">{{ assetSpecs.charger || 'N/A' }}</td>
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
                  <td class="h-val font-bold font-mono">{{ employee?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase">{{ employee?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold">{{ employee?.departmentName || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary">{{ employee?.email || '---' }}</td>
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
              <tbody>
                <tr>
                  <td class="h-lbl">Employee ID</td>
                  <td class="h-val font-bold font-mono">{{ employee?.employeeCode || '---' }}</td>
                  <td class="h-lbl">Full Name</td>
                  <td class="h-val font-bold text-uppercase">{{ employee?.fullName || '---' }}</td>
                  <td class="h-lbl">Department</td>
                  <td class="h-val font-bold">{{ employee?.departmentName || '---' }}</td>
                </tr>
                <tr>
                  <td class="h-lbl">Account/Email</td>
                  <td class="h-val font-bold text-primary">{{ employee?.email || '---' }}</td>
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
import { ref, computed, watch } from 'vue'
import Modal from '@/components/common/Modal.vue'
import companyLogo from '@/components/assets/img/logo.png'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  asset: { type: Object, default: null },
  employee: { type: Object, default: null },
  printingEquipmentList: { type: Array, default: () => [] },
  isBulkEquipmentPrint: { type: Boolean, default: false },
  categories: { type: Array, default: () => [] }
})

const emit = defineEmits(['close'])

const hasMouse = ref(false)

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    // Tự động kiểm tra nếu có ghi chú hoặc thông số có chuột thì bật true
    const noteStr = (props.asset?.note || '').toLowerCase()
    hasMouse.value = Boolean(noteStr.includes('chuột') || noteStr.includes('mouse'))
  }
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
</style>
