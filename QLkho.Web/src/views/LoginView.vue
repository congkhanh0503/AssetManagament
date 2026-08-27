<template>
  <div class="login-page-container">
    <!-- Language Switcher ở góc trên bên phải -->
    <div class="login-top-bar">
      <LanguageSwitcher variant="pills" />
    </div>

    <!-- Animated background glowing orbs -->
    <div class="bg-glow-orb orb-1"></div>
    <div class="bg-glow-orb orb-2"></div>
    <div class="bg-glow-orb orb-3"></div>
    <div class="login-card-wrap">
      <div class="login-card">
        <!-- Logo & Header -->
        <div class="login-header">
          <div class="brand-badge-icon">
            <svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
              <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
              <line x1="12" y1="22.08" x2="12" y2="12"></line>
            </svg>
          </div>
          <h1 class="login-title">{{ $t('auth.login_title') }}</h1>
          <p class="login-subtitle">{{ $t('auth.login_subtitle') }}</p>
        </div>

        <!-- Form Đăng Nhập -->
        <form @submit.prevent="handleLogin" class="login-form">
          <!-- Alert thông báo lỗi nếu có -->
          <div v-if="errorMessage" class="login-error-banner">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Input Username -->
          <div class="form-group">
            <label class="login-label">{{ $t('auth.username') }}</label>
            <div class="input-icon-wrapper">
              <span class="field-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                  <circle cx="12" cy="7" r="4"></circle>
                </svg>
              </span>
              <input 
                type="text" 
                class="login-input" 
                v-model="loginForm.username" 
                required 
                :placeholder="$t('auth.username_placeholder')"
                autocomplete="username"
                :disabled="submitting"
              />
            </div>
          </div>

          <!-- Input Password -->
          <div class="form-group">
            <label class="login-label">{{ $t('auth.password') }}</label>
            <div class="input-icon-wrapper">
              <span class="field-icon">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                </svg>
              </span>
              <input 
                :type="showPassword ? 'text' : 'password'" 
                class="login-input" 
                v-model="loginForm.password" 
                required 
                :placeholder="$t('auth.password_placeholder')"
                autocomplete="current-password"
                :disabled="submitting"
              />
              <button 
                type="button" 
                class="btn-toggle-password" 
                @click="showPassword = !showPassword"
                tabindex="-1"
                title="Ẩn/Hiện mật khẩu"
              >
                <svg v-if="!showPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M1 PRECISION 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                  <circle cx="12" cy="12" r="3"></circle>
                </svg>
                <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                  <line x1="1" y1="1" x2="23" y2="23"></line>
                </svg>
              </button>
            </div>
          </div>

          <!-- Nút Ghi nhớ & Quên mật khẩu -->
          <div class="login-options-row">
            <label class="remember-label">
              <input type="checkbox" v-model="rememberMe" />
              <span>{{ $t('auth.remember_me') }}</span>
            </label>
          </div>

          <!-- Submit Button -->
          <button type="submit" class="btn-login-submit" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner-btn"></span>
            <span v-else>{{ $t('auth.login_button') }}</span>
          </button>
        </form>

        <!-- Footer Info -->
        <div class="login-footer">
          <span>© 2026 {{ $t('common.system_name') }} - {{ $t('common.system_tag') }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import { t } from '@/i18n'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'

const router = useRouter()

const loginForm = reactive({
  username: '',
  password: ''
})

const showPassword = ref(false)
const rememberMe = ref(true)
const submitting = ref(false)
const errorMessage = ref('')

// Xử lý Đăng nhập
const handleLogin = async () => {
  if (!loginForm.username || !loginForm.password) {
    errorMessage.value = t('auth.invalid_credentials')
    return
  }

  submitting.value = true
  errorMessage.value = ''

  try {
    const res = await authApi.login({
      username: loginForm.username,
      password: loginForm.password
    })

    if (res && res.token) {
      if (res.role === 'HR') {
        router.push('/employees')
      } else {
        router.push('/')
      }
    }
  } catch (err) {
    errorMessage.value = err.message || t('auth.invalid_credentials')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.login-page-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8fafc;
  background-image: radial-gradient(at 0% 0%, rgba(79, 70, 229, 0.08) 0, transparent 50%),
                    radial-gradient(at 100% 100%, rgba(59, 130, 246, 0.08) 0, transparent 50%);
  position: relative;
  overflow: hidden;
  padding: 24px;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
}

.login-top-bar {
  position: absolute;
  top: 20px;
  right: 24px;
  z-index: 50;
}

/* Glowing Background Orbs */
.bg-glow-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  pointer-events: none;
  opacity: 0.25;
  animation: floatOrb 12s ease-in-out infinite alternate;
}

.orb-1 {
  width: 480px;
  height: 480px;
  background: radial-gradient(circle, #818cf8 0%, #c7d2fe 70%);
  top: -100px;
  left: -100px;
}

.orb-2 {
  width: 520px;
  height: 520px;
  background: radial-gradient(circle, #38bdf8 0%, #bae6fd 70%);
  bottom: -150px;
  right: -100px;
  animation-delay: -4s;
}

.orb-3 {
  width: 350px;
  height: 350px;
  background: radial-gradient(circle, #c084fc 0%, #e9d5ff 70%);
  top: 45%;
  right: 25%;
  animation-delay: -8s;
}

@keyframes floatOrb {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(40px, 30px) scale(1.1); }
  100% { transform: translate(-30px, 50px) scale(0.95); }
}

.login-card-wrap {
  width: 100%;
  max-width: 460px;
  position: relative;
  z-index: 10;
}

.login-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  padding: 40px 36px;
  box-shadow: 0 20px 45px -10px rgba(0, 0, 0, 0.08), 0 8px 10px -6px rgba(0, 0, 0, 0.04);
}

.login-header {
  text-align: center;
  margin-bottom: 28px;
}

.brand-badge-icon {
  width: 64px;
  height: 64px;
  background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
  border-radius: 16px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  margin-bottom: 16px;
  box-shadow: 0 10px 25px -5px rgba(79, 70, 229, 0.35);
}

.login-title {
  font-size: 1.35rem;
  font-weight: 800;
  letter-spacing: 0.5px;
  color: #0f172a;
  margin-bottom: 6px;
}

.login-subtitle {
  font-size: 0.85rem;
  color: #64748b;
  line-height: 1.4;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.login-error-banner {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  padding: 10px 14px;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 600;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.login-label {
  font-size: 0.825rem;
  font-weight: 600;
  color: #475569;
}

.input-icon-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.field-icon {
  position: absolute;
  left: 14px;
  color: #94a3b8;
  display: flex;
  align-items: center;
  pointer-events: none;
}

.login-input {
  width: 100%;
  background: #ffffff;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 12px 14px 12px 42px;
  color: #0f172a;
  font-size: 0.95rem;
  font-family: inherit;
  outline: none;
  transition: all 0.2s ease;
}

.login-input:focus {
  background: rgba(255, 255, 255, 0.9);
  border-color: #6366f1;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.25);
}

.btn-toggle-password {
  position: absolute;
  right: 12px;
  background: transparent;
  border: none;
  color: #64748b;
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
  transition: color 0.2s;
}

.btn-toggle-password:hover {
  color: #cbd5e1;
}

.login-options-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 0.8rem;
  color: #64748b;
}

.remember-label {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.remember-label input {
  accent-color: #4f46e5;
}

.default-badge-hint {
  color: #4f46e5;
}

.btn-login-submit {
  width: 100%;
  padding: 13px 20px;
  background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
  border: none;
  border-radius: 10px;
  color: #ffffff;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 4px 14px 0 rgba(79, 70, 229, 0.35);
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-login-submit:hover:not(:disabled) {
  background: linear-gradient(135deg, #4338ca 0%, #2563eb 100%);
  box-shadow: 0 6px 20px rgba(79, 70, 229, 0.45);
  transform: translateY(-1px);
}

.btn-login-submit:active:not(:disabled) {
  transform: translateY(0);
}

.btn-login-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loading-spinner-btn {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Quick Admin Credentials */
.quick-admin-box {
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px dashed var(--border-color);
}

.quick-admin-title {
  font-size: 0.725rem;
  font-weight: 700;
  color: #64748b;
  letter-spacing: 0.5px;
  margin-bottom: 10px;
}

.quick-accounts-stack {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.btn-quick-fill {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 10px;
  padding: 8px 12px;
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: left;
}

.btn-quick-fill:hover {
  background: #dcfce7;
  border-color: #059669;
  transform: translateY(-1px);
}

.btn-quick-fill.hr-quick-btn {
  background: #eff6ff;
  border-color: #93c5fd;
}

.btn-quick-fill.hr-quick-btn:hover {
  background: #dbeafe;
  border-color: #3b82f6;
}

.quick-account-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.tag-role {
  background: #dcfce7;
  color: #059669;
  font-size: 0.7rem;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid #86efac;
}

.tag-role.tag-role-hr {
  background: #dbeafe;
  color: #1d4ed8;
  border-color: #93c5fd;
}

.tag-creds {
  font-size: 0.775rem;
  color: #0f172a;
}

.btn-quick-action {
  font-size: 0.725rem;
  font-weight: 700;
  color: #4f46e5;
}

.login-footer {
  margin-top: 24px;
  text-align: center;
  font-size: 0.75rem;
  color: #94a3b8;
}
</style>
