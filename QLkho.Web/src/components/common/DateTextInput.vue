<template>
  <div class="date-text-input">
    <input
      ref="textRef"
      type="text"
      inputmode="numeric"
      autocomplete="off"
      :class="['form-control', { 'is-invalid': invalid }]"
      :value="text"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      @input="onInput"
      @paste="onPaste"
      @blur="commit"
      @keydown.enter="commit"
    />
    <button
      type="button"
      class="date-picker-btn"
      tabindex="-1"
      :disabled="disabled"
      title="Chọn ngày từ lịch"
      @click="openPicker"
    >
      📅
    </button>
    <input
      ref="pickerRef"
      type="date"
      class="date-picker-native"
      tabindex="-1"
      aria-hidden="true"
      :value="modelValue ? String(modelValue).slice(0, 10) : ''"
      @change="onPick"
    />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: 'dd/mm/yyyy (có thể dán)' },
  required: { type: Boolean, default: false },
  disabled: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue', 'change'])

const textRef = ref(null)
const pickerRef = ref(null)
const text = ref('')
const invalid = ref(false)

const pad = (n) => String(n).padStart(2, '0')

// ISO yyyy-MM-dd -> dd/MM/yyyy
const toDisplay = (iso) => {
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso || '')
  return m ? `${m[3]}/${m[2]}/${m[1]}` : ''
}

const isRealDate = (y, mo, d) => {
  const dt = new Date(y, mo - 1, d)
  return dt.getFullYear() === y && dt.getMonth() === mo - 1 && dt.getDate() === d
}

/**
 * Nhận nhiều kiểu dán: Oct 12, 2026, 12/10/2026, 12-10-2026, 12.10.2026, 2026-10-12, 2026/10/12,
 * 12102026, 12/10/26, kèm giờ phía sau (12/10/2026 14:30) -> trả về yyyy-MM-dd hoặc null
 */
const MONTHS = ['jan', 'feb', 'mar', 'apr', 'may', 'jun', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec']

// Dạng chữ: "Oct 12, 2026", "October 12 2026", "12 Oct 2026", "12 October, 2026"
const parseMonthName = (raw) => {
  const str = String(raw || '').trim().replace(/,/g, ' ').replace(/\s+/g, ' ')
  let m = /^([A-Za-z]{3,9})\.? (\d{1,2})(?:st|nd|rd|th)? (\d{4})(?: .*)?$/.exec(str)
  let mon, d, y
  if (m) {
    mon = m[1]; d = +m[2]; y = +m[3]
  } else if ((m = /^(\d{1,2})(?:st|nd|rd|th)? ([A-Za-z]{3,9})\.? (\d{4})(?: .*)?$/.exec(str))) {
    d = +m[1]; mon = m[2]; y = +m[3]
  } else {
    return undefined
  }
  const idx = MONTHS.indexOf(mon.slice(0, 3).toLowerCase())
  if (idx < 0) return undefined
  return isRealDate(y, idx + 1, d) ? `${y}-${pad(idx + 1)}-${pad(d)}` : null
}

const parseToIso = (raw) => {
  const named = parseMonthName(raw)
  if (named !== undefined) return named
  const s = String(raw || '').trim().split(/[\sT]/)[0]
  if (!s) return ''
  let y, mo, d
  let m = /^(\d{4})[-/.](\d{1,2})[-/.](\d{1,2})$/.exec(s)
  if (m) {
    y = +m[1]; mo = +m[2]; d = +m[3]
  } else if ((m = /^(\d{1,2})[-/.](\d{1,2})[-/.](\d{4}|\d{2})$/.exec(s))) {
    d = +m[1]; mo = +m[2]; y = m[3].length === 2 ? 2000 + +m[3] : +m[3]
  } else if ((m = /^(\d{2})(\d{2})(\d{4})$/.exec(s))) {
    d = +m[1]; mo = +m[2]; y = +m[3]
  } else {
    return null
  }
  return isRealDate(y, mo, d) ? `${y}-${pad(mo)}-${pad(d)}` : null
}

watch(
  () => props.modelValue,
  (v) => {
    const parsed = parseToIso(text.value)
    // Chỉ đồng bộ lại ô nhập khi giá trị bên ngoài thật sự khác với thứ đang gõ
    if (parsed !== String(v || '').slice(0, 10)) {
      text.value = toDisplay(v)
      invalid.value = false
      textRef.value?.setCustomValidity('')
    }
  },
  { immediate: true }
)

const apply = (raw) => {
  const iso = parseToIso(raw)
  if (iso === null) {
    invalid.value = true
    textRef.value?.setCustomValidity('Ngày không hợp lệ. Dùng định dạng dd/mm/yyyy')
    return false
  }
  invalid.value = false
  textRef.value?.setCustomValidity('')
  text.value = toDisplay(iso)
  if (iso !== String(props.modelValue || '').slice(0, 10)) {
    emit('update:modelValue', iso)
    emit('change', iso)
  }
  return true
}

const onInput = (e) => {
  text.value = e.target.value
  // Gõ tới khi hợp lệ thì cập nhật luôn, chưa hợp lệ thì chờ blur
  if (!text.value.trim()) {
    apply('')
  } else if (parseToIso(text.value) !== null) {
    apply(text.value)
  } else {
    invalid.value = false
    textRef.value?.setCustomValidity('Ngày không hợp lệ. Dùng định dạng dd/mm/yyyy')
  }
}

const onPaste = (e) => {
  const pasted = e.clipboardData?.getData('text')
  if (pasted == null) return
  e.preventDefault()
  text.value = pasted.trim()
  apply(text.value)
}

const commit = () => {
  apply(text.value)
}

const openPicker = () => {
  const el = pickerRef.value
  if (!el) return
  if (typeof el.showPicker === 'function') el.showPicker()
  else el.click()
}

const onPick = (e) => {
  apply(e.target.value)
}
</script>

<style scoped>
.date-text-input {
  position: relative;
  display: flex;
  align-items: center;
}

.date-text-input .form-control {
  padding-right: 38px;
}

.date-text-input .is-invalid {
  border-color: #ef4444;
}

.date-picker-btn {
  position: absolute;
  right: 6px;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 1rem;
  padding: 2px 4px;
  line-height: 1;
}

.date-picker-btn:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.date-picker-native {
  position: absolute;
  right: 0;
  bottom: 0;
  width: 1px;
  height: 1px;
  opacity: 0;
  pointer-events: none;
}
</style>
