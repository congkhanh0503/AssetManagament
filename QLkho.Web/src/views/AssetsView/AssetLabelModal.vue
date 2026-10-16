<template>
  <Modal
    :is-open="isOpen"
    :title="`In Tem Thiết Bị: ${asset?.assetCode || ''}`"
    subtitle="Tem 70 x 40 mm, mã QR chứa đầy đủ thông tin nhân sự và thiết bị"
    @close="$emit('close')"
    max-width="560px"
    :z-index="10050"
  >
    <div class="label-preview-wrapper">
      <div class="label-card">
        <img v-if="qrDataUrl" :src="qrDataUrl" alt="QR" class="label-qr" />
        <div class="label-info">
          <div class="label-row label-name">{{ info.holderName }}</div>
          <div class="label-row"><span class="k">Mã NV</span> {{ info.holderCode }}</div>
          <div class="label-row"><span class="k">Mã TS</span> <strong>{{ info.assetCode }}</strong></div>
          <div class="label-row"><span class="k">Model</span> {{ info.model }}</div>
          <div v-if="info.serial" class="label-row small"><span class="k">S/N</span> {{ info.serial }}</div>
          <div v-if="info.department" class="label-row small">{{ info.department }}</div>
        </div>
      </div>

      <div class="payload-box">
        <div class="payload-title">Nội dung hiện ra khi quét QR</div>
        <pre>{{ payload }}</pre>
      </div>
      <p class="label-note">
        Mã QR lưu nội dung tại thời điểm in. Nếu thiết bị đổi người dùng, in lại tem mới.
      </p>
    </div>

    <div class="modal-actions-right" style="margin-top: 16px; display: flex; gap: 8px; justify-content: flex-end;">
      <button type="button" class="btn btn-secondary" @click="$emit('close')">Đóng</button>
      <button type="button" class="btn btn-primary" :disabled="!qrDataUrl" @click="handlePrint">
        🏷️ In Tem
      </button>
    </div>
  </Modal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import QRCode from 'qrcode'
import Modal from '@/components/common/Modal.vue'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  asset: { type: Object, default: null }
})

defineEmits(['close'])

const qrDataUrl = ref('')

const info = computed(() => {
  const a = props.asset || {}
  return {
    holderName: a.holderName || 'Chưa cấp phát',
    holderCode: a.holderCode || '---',
    assetCode: a.assetCode || '---',
    model: a.assetName || '---',
    serial: a.serialNumber || '',
    department: a.holderDepartment || ''
  }
})

// Văn bản thuần: điện thoại quét bằng bất kỳ app nào cũng đọc được, không cần mạng hay đăng nhập
const payload = computed(() => {
  const i = info.value
  const lines = [
    `Nhân sự: ${i.holderName}`,
    `Mã NV: ${i.holderCode}`,
    `Mã tài sản: ${i.assetCode}`,
    `Model: ${i.model}`
  ]
  if (i.serial) lines.push(`S/N: ${i.serial}`)
  if (i.department) lines.push(`Phòng ban: ${i.department}`)
  return lines.join('\n')
})

watch(() => [props.isOpen, payload.value], async ([open]) => {
  if (!open || !props.asset) return
  try {
    qrDataUrl.value = await QRCode.toDataURL(payload.value, {
      width: 400,
      margin: 1,
      errorCorrectionLevel: 'M'
    })
  } catch (err) {
    console.warn('Lỗi sinh mã QR tem:', err)
    qrDataUrl.value = ''
  }
}, { immediate: true })

const escapeHtml = (s) => String(s).replace(/[&<>"']/g, (c) => (
  { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]
))

// In trong iframe riêng để @page 70x40mm không ảnh hưởng CSS in của các màn hình khác
const handlePrint = () => {
  const i = info.value
  const iframe = document.createElement('iframe')
  iframe.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;'
  document.body.appendChild(iframe)
  const doc = iframe.contentWindow.document
  doc.open()
  doc.write(`<!doctype html><html><head><meta charset="utf-8"><title>Tem ${escapeHtml(i.assetCode)}</title>
<style>
  @page { size: 70mm 40mm; margin: 0; }
  * { box-sizing: border-box; }
  html, body { margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; color: #000; }
  .label { width: 70mm; height: 40mm; padding: 2.5mm; display: flex; align-items: center; gap: 2.5mm; overflow: hidden; }
  .qr { width: 30mm; height: 30mm; flex: none; }
  .info { flex: 1; min-width: 0; font-size: 8pt; line-height: 1.25; }
  .name { font-size: 10pt; font-weight: 700; margin-bottom: 1mm; }
  .row { word-break: break-word; }
  .k { display: inline-block; min-width: 10mm; color: #444; }
  .small { font-size: 6.5pt; }
</style></head><body>
<div class="label">
  <img class="qr" src="${qrDataUrl.value}" />
  <div class="info">
    <div class="name">${escapeHtml(i.holderName)}</div>
    <div class="row"><span class="k">Mã NV</span>${escapeHtml(i.holderCode)}</div>
    <div class="row"><span class="k">Mã TS</span><b>${escapeHtml(i.assetCode)}</b></div>
    <div class="row"><span class="k">Model</span>${escapeHtml(i.model)}</div>
    ${i.serial ? `<div class="row small"><span class="k">S/N</span>${escapeHtml(i.serial)}</div>` : ''}
    ${i.department ? `<div class="row small">${escapeHtml(i.department)}</div>` : ''}
  </div>
</div>
</body></html>`)
  doc.close()

  const img = doc.querySelector('img')
  const doPrint = () => {
    iframe.contentWindow.focus()
    iframe.contentWindow.print()
    setTimeout(() => iframe.remove(), 1000)
  }
  if (img.complete) doPrint()
  else img.onload = doPrint
}
</script>

<style scoped>
.label-preview-wrapper {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.label-card {
  width: 70mm;
  height: 40mm;
  margin: 0 auto;
  padding: 2.5mm;
  display: flex;
  align-items: center;
  gap: 2.5mm;
  background: #ffffff;
  color: #000000;
  border: 1px dashed #94a3b8;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
  overflow: hidden;
}

.label-qr {
  width: 30mm;
  height: 30mm;
  flex: none;
}

.label-info {
  flex: 1;
  min-width: 0;
  font-size: 8pt;
  line-height: 1.25;
}

.label-name {
  font-size: 10pt;
  font-weight: 700;
  margin-bottom: 1mm;
}

.label-row {
  word-break: break-word;
}

.label-row.small {
  font-size: 6.5pt;
}

.k {
  display: inline-block;
  min-width: 10mm;
  color: #444444;
}

.payload-box {
  background: rgba(148, 163, 184, 0.12);
  border-radius: 8px;
  padding: 10px 12px;
}

.payload-title {
  font-size: 0.8rem;
  font-weight: 600;
  margin-bottom: 4px;
}

.payload-box pre {
  margin: 0;
  font-size: 0.85rem;
  white-space: pre-wrap;
  word-break: break-word;
}

.label-note {
  margin: 0;
  font-size: 0.8rem;
  opacity: 0.75;
}
</style>
