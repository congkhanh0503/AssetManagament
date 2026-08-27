<template>
  <aside class="sidebar">
    <!-- Brand Logo -->
    <div class="brand-container">
      <div class="brand-logo">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="2" y="7" width="20" height="14" rx="2" ry="2"></rect>
          <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"></path>
        </svg>
      </div>
      <div class="brand-info">
        <h1 class="brand-name">AssetsOps</h1>
        <span class="brand-tag">Enterprise IT</span>
      </div>
    </div>

    <!-- Navigation Menu -->
    <nav class="nav-menu">
      <!-- DÀNH CHO ADMIN: HIỂN THỊ ĐẦY ĐỦ TẤT CẢ MENU -->
      <template v-if="!isHR">
        <div class="nav-section-title">{{ $t('sidebar.section_overview') }}</div>
        
        <RouterLink to="/" class="nav-item" active-class="active">
          <div class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="7" height="7"></rect>
              <rect x="14" y="3" width="7" height="7"></rect>
              <rect x="14" y="14" width="7" height="7"></rect>
              <rect x="3" y="14" width="7" height="7"></rect>
            </svg>
          </div>
          <span>{{ $t('sidebar.dashboard') }}</span>
        </RouterLink>

        <RouterLink to="/assets" class="nav-item" active-class="active">
          <div class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
              <line x1="8" y1="21" x2="16" y2="21"></line>
              <line x1="12" y1="17" x2="12" y2="21"></line>
            </svg>
          </div>
          <span>{{ $t('sidebar.assets') }}</span>
        </RouterLink>

        <RouterLink to="/documents" class="nav-item" active-class="active">
          <div class="nav-icon" style="color: #ef4444;">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
            </svg>
          </div>
          <span>{{ $t('sidebar.documents') }}</span>
        </RouterLink>

        <div class="nav-section-title" style="margin-top: 10px;">{{ $t('sidebar.section_management') }}</div>

        <RouterLink to="/employees" class="nav-item" active-class="active">
          <div class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
          </div>
          <span>{{ $t('sidebar.employees') }}</span>
        </RouterLink>

        <RouterLink to="/departments" class="nav-item" active-class="active">
          <div class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
              <polyline points="9 22 9 12 15 12 15 22"></polyline>
            </svg>
          </div>
          <span>{{ $t('sidebar.departments') }}</span>
        </RouterLink>

        <RouterLink to="/categories-suppliers" class="nav-item" active-class="active">
          <div class="nav-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"></path>
              <line x1="7" y1="7" x2="7.01" y2="7"></line>
            </svg>
          </div>
          <span>{{ $t('sidebar.categories_suppliers') }}</span>
        </RouterLink>
      </template>

      <!-- DÀNH CHO ROLE NHÂN SỰ (HR): CHỈ HIỂN THỊ DUY NHẤT QUẢN LÝ NHÂN SỰ -->
      <template v-else>
        <div class="nav-section-title">{{ $t('sidebar.hr_section_title') }}</div>

        <RouterLink to="/employees" class="nav-item" active-class="active">
          <div class="nav-icon" style="color: #6366f1;">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
          </div>
          <span>{{ $t('sidebar.employees') }}</span>
        </RouterLink>

        <div class="hr-restricted-notice">
          <div class="hr-notice-badge">🔒 Quyền hạn: HR</div>
          <p>{{ $t('sidebar.hr_restricted_notice') }}</p>
        </div>
      </template>
    </nav>

    <!-- Bottom System Status, Lang & Logout -->
    <div class="sidebar-footer">
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <span style="font-size: 0.75rem; font-weight: 700; color: #64748b; text-transform: uppercase;">Ngôn Ngữ / Language:</span>
        <LanguageSwitcher variant="pills" />
      </div>

      <button type="button" class="btn-sidebar-logout" @click="handleLogout">
        <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
          <polyline points="16 17 21 12 16 7"></polyline>
          <line x1="21" y1="12" x2="9" y2="12"></line>
        </svg>
        <span>{{ $t('auth.logout') }} ({{ isHR ? 'HR' : 'Admin' }})</span>
      </button>

      <div class="system-status-card">
        <div class="status-indicator">
          <span class="pulse-dot"></span>
          <span class="status-text">{{ $t('sidebar.backend_status') }}</span>
        </div>
        <div class="status-sub">{{ $t('sidebar.db_connected') }}</div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { authApi, getCurrentUser } from '@/api/auth'
import { t } from '@/i18n'
import LanguageSwitcher from '@/components/common/LanguageSwitcher.vue'

const router = useRouter()
const currentUser = computed(() => getCurrentUser())
const isHR = computed(() => currentUser.value?.role === 'HR')

const handleLogout = () => {
  if (confirm(t('auth.logout_confirm'))) {
    authApi.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.sidebar {
  width: 270px;
  background: #ffffff;
  border-right: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  height: 100vh;
  position: sticky;
  top: 0;
  z-index: 50;
  box-shadow: 1px 0 3px rgba(0, 0, 0, 0.02);
}

.brand-container {
  padding: 22px 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid var(--border-color);
}

.brand-logo {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px var(--primary-glow);
}

.brand-name {
  font-size: 1.1rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.01em;
}

.brand-tag {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--primary);
  letter-spacing: 0.05em;
}

.nav-menu {
  padding: 20px 14px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  overflow-y: auto;
}

.nav-section-title {
  font-size: 0.725rem;
  font-weight: 700;
  color: var(--text-dim);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  padding: 10px 12px 6px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: var(--radius-md);
  color: var(--text-muted);
  text-decoration: none;
  font-size: 0.885rem;
  font-weight: 500;
  transition: var(--transition);
}

.nav-item:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.nav-item.active {
  background: rgba(79, 70, 229, 0.08);
  color: var(--primary);
  font-weight: 700;
  border-left: 3px solid var(--primary);
}

.nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  color: currentColor;
}

.sidebar-footer {
  padding: 16px;
  border-top: 1px solid var(--border-color);
  display: flex;
  flex-direction: column;
  gap: 10px;
  background: #ffffff;
}

.btn-sidebar-logout {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  padding: 8px 12px;
  border-radius: var(--radius-sm);
  font-size: 0.825rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-sidebar-logout:hover {
  background: #fee2e2;
  border-color: #f87171;
  color: #b91c1c;
  transform: translateY(-1px);
}

.system-status-card {
  background: #f8fafc;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  padding: 10px 14px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
}

.pulse-dot {
  width: 8px;
  height: 8px;
  background-color: var(--success);
  border-radius: 50%;
  box-shadow: 0 0 6px rgba(5, 150, 105, 0.5);
}

.status-text {
  font-size: 0.8rem;
  font-weight: 600;
  color: #1e293b;
}

.status-sub {
  font-size: 0.725rem;
  color: var(--text-dim);
  margin-top: 2px;
  padding-left: 16px;
}

.hr-restricted-notice {
  margin-top: 20px;
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: var(--radius-md);
  padding: 12px 14px;
}

.hr-notice-badge {
  font-size: 0.75rem;
  font-weight: 800;
  color: #1d4ed8;
  margin-bottom: 6px;
  text-transform: uppercase;
}

.hr-restricted-notice p {
  font-size: 0.775rem;
  color: #475569;
  line-height: 1.45;
  margin: 0;
}
</style>
