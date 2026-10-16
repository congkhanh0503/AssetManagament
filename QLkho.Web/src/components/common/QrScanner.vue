<template>
  <div class="qr-scanner">
    <div v-if="errorMessage" class="scanner-error">{{ errorMessage }}</div>
    <div v-else class="scanner-frame">
      <video ref="videoRef" playsinline muted class="scanner-video"></video>
      <div class="scanner-aim"></div>
    </div>
    <canvas ref="canvasRef" class="scanner-canvas"></canvas>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import jsQR from 'jsqr'
import { t } from '@/i18n'

const emit = defineEmits(['detect', 'error'])

const videoRef = ref(null)
const canvasRef = ref(null)
const errorMessage = ref('')

let stream = null
let timer = null
let lastText = ''
let lastAt = 0

// Cùng một mã trong khoảng này coi là một lần quét, tránh ghi nhận lặp khi giữ camera trước tem
const REPEAT_WINDOW_MS = 3000

const stop = () => {
  if (timer) clearInterval(timer)
  timer = null
  if (stream) stream.getTracks().forEach((t) => t.stop())
  stream = null
}

const tick = () => {
  const video = videoRef.value
  const canvas = canvasRef.value
  if (!video || !canvas || video.readyState < 2 || !video.videoWidth) return

  // Thu nhỏ khung hình để giải mã nhanh trên điện thoại
  const scale = Math.min(1, 640 / video.videoWidth)
  const w = Math.round(video.videoWidth * scale)
  const h = Math.round(video.videoHeight * scale)
  canvas.width = w
  canvas.height = h
  const ctx = canvas.getContext('2d', { willReadFrequently: true })
  ctx.drawImage(video, 0, 0, w, h)
  const img = ctx.getImageData(0, 0, w, h)
  const code = jsQR(img.data, w, h, { inversionAttempts: 'dontInvert' })
  if (!code || !code.data) return

  const now = Date.now()
  if (code.data === lastText && now - lastAt < REPEAT_WINDOW_MS) return
  lastText = code.data
  lastAt = now
  if (navigator.vibrate) navigator.vibrate(60)
  emit('detect', code.data)
}

onMounted(async () => {
  if (!window.isSecureContext || !navigator.mediaDevices?.getUserMedia) {
    errorMessage.value = t('inventory.cam_https')
    emit('error', errorMessage.value)
    return
  }
  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: { ideal: 'environment' } },
      audio: false
    })
    const video = videoRef.value
    video.srcObject = stream
    await video.play()
    timer = setInterval(tick, 200)
  } catch (err) {
    errorMessage.value = err?.name === 'NotAllowedError'
      ? t('inventory.cam_denied')
      : t('inventory.cam_fail', { error: err?.message || err })
    emit('error', errorMessage.value)
  }
})

onBeforeUnmount(stop)
</script>

<style scoped>
.qr-scanner {
  width: 100%;
}

.scanner-frame {
  position: relative;
  width: 100%;
  max-width: 480px;
  margin: 0 auto;
  aspect-ratio: 1 / 1;
  background: #000;
  border-radius: 12px;
  overflow: hidden;
}

.scanner-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.scanner-aim {
  position: absolute;
  inset: 18%;
  border: 3px solid rgba(34, 197, 94, 0.9);
  border-radius: 14px;
  box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.35);
  pointer-events: none;
}

.scanner-canvas {
  display: none;
}

.scanner-error {
  padding: 14px;
  border-radius: 10px;
  background: #fef2f2;
  color: #b91c1c;
  font-size: 0.9rem;
  line-height: 1.4;
}
</style>
