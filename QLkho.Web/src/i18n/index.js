import { ref, computed } from 'vue'
import vi from './vi'
import en from './en'

const messages = { vi, en }

const savedLocale = localStorage.getItem('app_locale') || 'vi'
export const currentLocale = ref(savedLocale)

/**
 * Hàm dịch chuỗi theo key dạng dot-notation (vd: 'sidebar.assets', 'common.save')
 * Hỗ trợ params thay thế {name}, {count}...
 */
export function t(key, params = {}) {
  if (!key) return ''
  const locale = currentLocale.value || 'vi'
  const dict = messages[locale] || messages.vi

  const keys = key.split('.')
  let val = dict

  for (const k of keys) {
    if (val && typeof val === 'object' && k in val) {
      val = val[k]
    } else {
      // Fallback sang tiếng Việt nếu không tìm thấy key trong locale hiện tại
      val = getFallback(key)
      break
    }
  }

  if (typeof val !== 'string') {
    return key
  }

  // Thay thế tham số {param}
  let text = val
  for (const [pKey, pVal] of Object.entries(params)) {
    text = text.replace(new RegExp(`{${pKey}}`, 'g'), String(pVal))
  }

  return text
}

function getFallback(key) {
  const keys = key.split('.')
  let val = messages.vi
  for (const k of keys) {
    if (val && typeof val === 'object' && k in val) {
      val = val[k]
    } else {
      return key
    }
  }
  return typeof val === 'string' ? val : key
}

export function setLocale(lang) {
  if (lang === 'vi' || lang === 'en') {
    currentLocale.value = lang
    localStorage.setItem('app_locale', lang)
    document.documentElement.setAttribute('lang', lang)
  }
}

export function getLocale() {
  return currentLocale.value
}

export const isEnglish = computed(() => currentLocale.value === 'en')
export const isVietnamese = computed(() => currentLocale.value === 'vi')

export function useI18n() {
  return {
    t,
    locale: currentLocale,
    setLocale,
    getLocale,
    isEnglish,
    isVietnamese
  }
}

// Vue plugin để inject $t toàn cục
export default {
  install(app) {
    app.config.globalProperties.$t = t
    app.provide('i18n', {
      t,
      locale: currentLocale,
      setLocale
    })
  }
}
