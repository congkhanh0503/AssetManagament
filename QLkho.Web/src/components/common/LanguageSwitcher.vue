<template>
  <div class="lang-switcher" :class="[variant, { 'is-dropdown-open': isOpen }]">
    <!-- Nút dạng Compact / Pill Switcher (Dùng cho Header, Sidebar, Login) -->
    <div v-if="variant === 'pills'" class="lang-pills">
      <button 
        type="button" 
        class="lang-pill-btn" 
        :class="{ active: currentLocale === 'vi' }"
        @click="switchLang('vi')"
        title="Chuyển sang Tiếng Việt"
      >
        <span class="flag-icon">🇻🇳</span>
        <span class="lang-text">VN</span>
      </button>
      <button 
        type="button" 
        class="lang-pill-btn" 
        :class="{ active: currentLocale === 'en' }"
        @click="switchLang('en')"
        title="Switch to English"
      >
        <span class="flag-icon">🇬🇧</span>
        <span class="lang-text">EN</span>
      </button>
    </div>

    <!-- Nút dạng Toggle Button đơn giản -->
    <button 
      v-else-if="variant === 'toggle'"
      type="button" 
      class="lang-toggle-btn"
      @click="toggleLang"
      :title="currentLocale === 'vi' ? 'Switch to English' : 'Chuyển sang Tiếng Việt'"
    >
      <span class="flag-icon">{{ currentLocale === 'vi' ? '🇻🇳' : '🇬🇧' }}</span>
      <span class="lang-code">{{ currentLocale.toUpperCase() }}</span>
    </button>

    <!-- Nút dạng Dropdown Selector -->
    <div v-else class="lang-dropdown-wrapper">
      <button 
        type="button" 
        class="lang-dropdown-trigger" 
        @click="isOpen = !isOpen"
      >
        <span class="flag-icon">{{ currentLocale === 'vi' ? '🇻🇳' : '🇬🇧' }}</span>
        <span class="lang-label">{{ currentLocale === 'vi' ? 'Tiếng Việt' : 'English' }}</span>
        <svg class="chevron-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="6 9 12 15 18 9"></polyline>
        </svg>
      </button>

      <div v-if="isOpen" class="lang-dropdown-menu">
        <button 
          type="button" 
          class="lang-menu-item" 
          :class="{ active: currentLocale === 'vi' }"
          @click="selectLang('vi')"
        >
          <span class="flag-icon">🇻🇳</span>
          <span class="lang-name">Tiếng Việt</span>
          <span v-if="currentLocale === 'vi'" class="check-mark">✓</span>
        </button>
        <button 
          type="button" 
          class="lang-menu-item" 
          :class="{ active: currentLocale === 'en' }"
          @click="selectLang('en')"
        >
          <span class="flag-icon">🇬🇧</span>
          <span class="lang-name">English</span>
          <span v-if="currentLocale === 'en'" class="check-mark">✓</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { currentLocale, setLocale } from '@/i18n'

const props = defineProps({
  variant: {
    type: String,
    default: 'pills', // 'pills' | 'toggle' | 'dropdown'
    validator: (v) => ['pills', 'toggle', 'dropdown'].includes(v)
  }
})

const isOpen = ref(false)

const switchLang = (lang) => {
  setLocale(lang)
}

const toggleLang = () => {
  setLocale(currentLocale.value === 'vi' ? 'en' : 'vi')
}

const selectLang = (lang) => {
  setLocale(lang)
  isOpen.value = false
}

const handleClickOutside = (e) => {
  if (!e.target.closest('.lang-switcher')) {
    isOpen.value = false
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
.lang-switcher {
  display: inline-flex;
  align-items: center;
  user-select: none;
}

/* 1. PILLS VARIANT */
.lang-pills {
  display: flex;
  background: #f1f5f9;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 9999px;
  padding: 2px;
  gap: 2px;
}

.lang-pill-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 9999px;
  border: none;
  background: transparent;
  color: #64748b;
  font-size: 0.775rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.lang-pill-btn:hover {
  color: #1e293b;
}

.lang-pill-btn.active {
  background: #ffffff;
  color: #4f46e5;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.flag-icon {
  font-size: 0.95rem;
  line-height: 1;
}

/* 2. TOGGLE VARIANT */
.lang-toggle-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #f8fafc;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 8px;
  color: #334155;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
}

.lang-toggle-btn:hover {
  background: #eff6ff;
  border-color: #3b82f6;
  color: #2563eb;
}

/* 3. DROPDOWN VARIANT */
.lang-dropdown-wrapper {
  position: relative;
}

.lang-dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  background: #ffffff;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 8px;
  color: #334155;
  font-size: 0.825rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.lang-dropdown-trigger:hover {
  border-color: #cbd5e1;
  background: #f8fafc;
}

.lang-dropdown-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  min-width: 140px;
  background: #ffffff;
  border: 1px solid var(--border-color, #e2e8f0);
  border-radius: 8px;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
  padding: 4px;
  z-index: 100;
}

.lang-menu-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  border: none;
  background: transparent;
  border-radius: 6px;
  color: #334155;
  font-size: 0.825rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s;
  text-align: left;
}

.lang-menu-item:hover {
  background: #f1f5f9;
}

.lang-menu-item.active {
  color: #4f46e5;
  font-weight: 700;
  background: #eef2ff;
}

.check-mark {
  margin-left: auto;
  color: #4f46e5;
  font-size: 0.85rem;
}
</style>
