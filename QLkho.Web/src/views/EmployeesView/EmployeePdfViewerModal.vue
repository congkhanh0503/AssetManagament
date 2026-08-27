<template>
  <Modal 
    :is-open="isOpen" 
    :title="`Xem Biên Bản: ${document?.fileName || ''}`"
    subtitle="Biên bản bàn giao thiết bị được lưu trữ trong hệ thống"
    @close="$emit('close')"
    max-width="950px"
    :z-index="10050"
  >
    <div class="pdf-viewer-container">
      <div class="pdf-viewer-topbar">
        <div class="pdf-viewer-doc-title">
          <span class="pdf-viewer-icon">📄</span>
          <strong>{{ document?.fileName }}</strong>
          <span class="badge-pdf-size" v-if="document?.fileSizeFormatted">({{ document.fileSizeFormatted }})</span>
        </div>
        <div class="pdf-topbar-actions">
          <a 
            v-if="document" 
            :href="getPdfUrl(document.filePath)" 
            target="_blank" 
            class="btn btn-sm btn-secondary"
          >
            ↗ Mở Cửa Sổ Mới
          </a>
          <a 
            v-if="document" 
            :href="getPdfUrl(document.filePath)" 
            :download="document.fileName" 
            class="btn btn-sm btn-primary"
          >
            📥 Tải Về Máy
          </a>
        </div>
      </div>

      <div class="pdf-frame-wrapper">
        <iframe 
          v-if="document" 
          :src="getPdfUrl(document.filePath)" 
          class="pdf-iframe-view"
          title="PDF Preview"
        ></iframe>
      </div>
    </div>
  </Modal>
</template>

<script setup>
import Modal from '@/components/common/Modal.vue'

defineProps({
  isOpen: { type: Boolean, default: false },
  document: { type: Object, default: null }
})

defineEmits(['close'])

const getPdfUrl = (filePath) => {
  if (!filePath) return '#'
  if (filePath.startsWith('http://') || filePath.startsWith('https://')) return filePath
  return filePath.startsWith('/') ? filePath : `/${filePath}`
}
</script>
