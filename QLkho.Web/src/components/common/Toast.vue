<template>
  <Teleport to="body">
    <div class="toast-container">
      <TransitionGroup name="toast">
        <div 
          v-for="toast in toasts" 
          :key="toast.id" 
          :class="['toast-item', `toast-${toast.type}`]"
        >
          <div class="toast-icon">
            <span v-if="toast.type === 'success'">✓</span>
            <span v-else-if="toast.type === 'error'">✕</span>
            <span v-else>ℹ</span>
          </div>
          <div class="toast-content">
            <div class="toast-title">{{ toast.title }}</div>
            <div v-if="toast.message" class="toast-message">{{ toast.message }}</div>
          </div>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup>
import { ref } from 'vue'

const toasts = ref([])

const addToast = (title, message = '', type = 'success', duration = 3500) => {
  const id = Date.now() + Math.random()
  toasts.value.push({ id, title, message, type })
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }, duration)
}

const show = (title, type = 'success', message = '') => {
  addToast(title, message, type)
}

const success = (title, message = '') => addToast(title, message, 'success')
const error = (title, message = '') => addToast(title, message, 'error')
const info = (title, message = '') => addToast(title, message, 'info')
const warning = (title, message = '') => addToast(title, message, 'warning')

defineExpose({ addToast, show, success, error, info, warning })
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  gap: 12px;
  pointer-events: none;
}

.toast-item {
  min-width: 300px;
  max-width: 420px;
  background: #1e293b;
  border-radius: var(--radius-md);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.5), 0 0 0 1px rgba(255, 255, 255, 0.1);
  padding: 14px 18px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  pointer-events: auto;
  backdrop-filter: blur(12px);
}

.toast-icon {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 0.85rem;
  flex-shrink: 0;
}

.toast-success {
  border-left: 4px solid var(--success);
}
.toast-success .toast-icon {
  background: var(--success-bg);
  color: var(--success);
}

.toast-error {
  border-left: 4px solid var(--danger);
}
.toast-error .toast-icon {
  background: var(--danger-bg);
  color: var(--danger);
}

.toast-info {
  border-left: 4px solid var(--primary);
}
.toast-info .toast-icon {
  background: var(--primary-glow);
  color: var(--primary);
}

.toast-title {
  font-size: 0.9rem;
  font-weight: 600;
  color: #ffffff;
}

.toast-message {
  font-size: 0.8rem;
  color: var(--text-muted);
  margin-top: 2px;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(50px) scale(0.9);
}

.toast-leave-to {
  opacity: 0;
  transform: translateY(-20px) scale(0.9);
}
</style>
