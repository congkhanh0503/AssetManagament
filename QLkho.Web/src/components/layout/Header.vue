<template>
  <header class="header">
    <div class="header-left">
    </div>

    <div class="header-right">
      <!-- Language Switcher Pill -->
      <LanguageSwitcher variant="pills" />

      <div class="api-badge">
        <span class="pulse-icon"></span>
        <span>REST API: <strong style="color: #6366f1;">{{ $t('header.api_active') }}</strong></span>
      </div>

      <!-- User Profile & Dropdown -->
      <div class="user-profile-wrapper">
        <div class="user-profile" @click="isUserMenuOpen = !isUserMenuOpen">
          <div class="user-avatar">{{ userInitials }}</div>
          <div class="user-meta">
            <div class="user-name">{{ currentUser.fullName || 'Admin' }}</div>
            <div class="user-role" :class="{ 'hr-role-badge': currentUser.role === 'HR' }">
              {{ currentUser.role === 'HR' ? $t('auth.role_hr') : $t('auth.role_admin') }}
            </div>
          </div>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="chevron-icon">
            <polyline points="6 9 12 15 18 9"></polyline>
          </svg>
        </div>

        <!-- User Dropdown Menu -->
        <div v-if="isUserMenuOpen" class="user-dropdown-menu">
          <div class="dropdown-header">
            <strong>{{ currentUser.fullName || 'Administrator' }}</strong>
            <span>@{{ currentUser.username || 'admin' }}</span>
          </div>
          <div class="dropdown-divider"></div>
          <button type="button" class="dropdown-item" @click="openChangePwdModal">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
            </svg>
            <span>{{ $t('auth.change_password') }}</span>
          </button>
          <div class="dropdown-divider"></div>
          <button type="button" class="dropdown-item logout" @click="handleLogout">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
              <polyline points="16 17 21 12 16 7"></polyline>
              <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
            <span>{{ $t('auth.logout') }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Đổi Mật Khẩu -->
    <Modal 
      :is-open="isChangePwdOpen" 
      :title="$t('auth.change_password_title')"
      :subtitle="$t('auth.change_password_subtitle')"
      @close="isChangePwdOpen = false"
      max-width="450px"
    >
      <form @submit.prevent="submitChangePassword">
        <div v-if="pwdError" class="pwd-error-alert">{{ pwdError }}</div>
        <div v-if="pwdSuccess" class="pwd-success-alert">{{ pwdSuccess }}</div>

        <div class="form-group">
          <label class="form-label">{{ $t('auth.current_password') }} *</label>
          <input type="password" class="form-control" v-model="pwdForm.currentPassword" required :placeholder="$t('auth.current_password')" />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('auth.new_password') }} * ({{ $t('auth.password_min_length') }})</label>
          <input type="password" class="form-control" v-model="pwdForm.newPassword" minlength="6" required :placeholder="$t('auth.new_password')" />
        </div>

        <div class="form-group">
          <label class="form-label">{{ $t('auth.confirm_new_password') }} *</label>
          <input type="password" class="form-control" v-model="pwdForm.confirmPassword" minlength="6" required :placeholder="$t('auth.confirm_new_password')" />
        </div>

        <div class="modal-actions-right" style="margin-top: 20px;">
          <button type="button" class="btn btn-secondary" @click="isChangePwdOpen = false">{{ $t('common.cancel') }}</button>
          <button type="submit" class="btn btn-primary" :disabled="pwdSubmitting">
            <span v-if="pwdSubmitting" class="loading-spinner"></span>
            {{ $t('common.save') }}
          </button>
        </div>
      </form>
    </Modal>
  </header>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { authApi, getCurrentUser, clearSession } from '@/api/auth'
import { t } from '@/i18n'
import Modal from '@/components/common/Modal.vue'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'

defineEmits(['global-search'])
const router = useRouter()

const isUserMenuOpen = ref(false)
const currentUser = ref(getCurrentUser())

const userInitials = computed(() => {
  const name = currentUser.value?.fullName || 'Admin'
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
})

// Đăng Xuất
const handleLogout = () => {
  if (confirm(t('auth.logout_confirm'))) {
    authApi.logout()
    isUserMenuOpen.value = false
    router.push('/login')
  }
}

// Modal Đổi Mật Khẩu
const isChangePwdOpen = ref(false)
const pwdSubmitting = ref(false)
const pwdError = ref('')
const pwdSuccess = ref('')

const pwdForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const openChangePwdModal = () => {
  isUserMenuOpen.value = false
  pwdForm.currentPassword = ''
  pwdForm.newPassword = ''
  pwdForm.confirmPassword = ''
  pwdError.value = ''
  pwdSuccess.value = ''
  isChangePwdOpen.value = true
}

const submitChangePassword = async () => {
  if (pwdForm.newPassword !== pwdForm.confirmPassword) {
    pwdError.value = t('auth.password_not_match')
    return
  }

  pwdSubmitting.value = true
  pwdError.value = ''
  pwdSuccess.value = ''

  try {
    await authApi.changePassword({
      currentPassword: pwdForm.currentPassword,
      newPassword: pwdForm.newPassword
    }, currentUser.value.username || 'admin')

    pwdSuccess.value = t('auth.change_pwd_success')
    setTimeout(() => {
      isChangePwdOpen.value = false
    }, 1500)
  } catch (err) {
    pwdError.value = err.message || t('auth.invalid_credentials')
  } finally {
    pwdSubmitting.value = false
  }
}

// Click outside to close dropdown
const handleClickOutside = (e) => {
  if (!e.target.closest('.user-profile-wrapper')) {
    isUserMenuOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.header {
  height: 70px;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--border-color);
  padding: 0 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 40;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.02);
}

.search-box {
  position: relative;
  width: 380px;
}

.search-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-dim);
}

.search-input {
  width: 100%;
  padding: 9px 14px 9px 40px;
  background: #f1f5f9;
  border: 1px solid var(--border-color);
  border-radius: 9999px;
  color: var(--text-main);
  font-family: inherit;
  font-size: 0.875rem;
  outline: none;
  transition: var(--transition);
}

.search-input:focus {
  background: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 20px;
}

.api-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #eef2ff;
  padding: 6px 14px;
  border-radius: 9999px;
  border: 1px solid #c7d2fe;
  font-size: 0.785rem;
  color: var(--primary);
}

.pulse-icon {
  width: 6px;
  height: 6px;
  background: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 6px rgba(16, 185, 129, 0.5);
}

/* User Profile & Dropdown */
.user-profile-wrapper {
  position: relative;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  padding: 6px 10px;
  border-radius: 10px;
  transition: background 0.2s;
  user-select: none;
}

.user-profile:hover {
  background: #f1f5f9;
}

.user-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.3);
}

.user-name {
  font-size: 0.875rem;
  font-weight: 700;
  color: #0f172a;
}

.user-role {
  font-size: 0.725rem;
  color: var(--text-dim);
}

.chevron-icon {
  color: var(--text-dim);
  transition: transform 0.2s;
}

.user-dropdown-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 220px;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05);
  padding: 6px;
  z-index: 100;
  animation: fadeIn 0.15s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-6px); }
  to { opacity: 1; transform: translateY(0); }
}

.dropdown-header {
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
}

.dropdown-header strong {
  color: #0f172a;
  font-size: 0.85rem;
}

.dropdown-header span {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.dropdown-divider {
  height: 1px;
  background: var(--border-color);
  margin: 4px 0;
}

.dropdown-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: var(--radius-sm);
  background: transparent;
  border: none;
  color: var(--text-main);
  font-size: 0.825rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s;
  text-align: left;
}

.dropdown-item:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.dropdown-item.logout {
  color: #dc2626;
}

.dropdown-item.logout:hover {
  background: rgba(239, 68, 68, 0.15);
  color: #fca5a5;
}

/* Password Change Alerts */
.pwd-error-alert {
  background: rgba(239, 68, 68, 0.15);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 0.825rem;
  margin-bottom: 12px;
}

.pwd-success-alert {
  background: rgba(16, 185, 129, 0.15);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #6ee7b7;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 0.825rem;
  margin-bottom: 12px;
}
</style>
