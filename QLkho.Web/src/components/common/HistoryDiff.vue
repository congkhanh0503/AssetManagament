<template>
  <div v-if="oldValue || newValue" class="event-diff-box">
    <!-- Dạng "Tên: giá trị": chỉ hiện các trường thực sự thay đổi -->
    <template v-if="changes">
      <div v-if="changes.length === 0" class="diff-item diff-same">
        <span class="diff-val">Không có giá trị nào thay đổi.</span>
      </div>
      <div v-for="c in changes" :key="c.key" class="diff-row">
        <span class="diff-key">{{ c.key }}</span>
        <span class="diff-item diff-old"><span class="diff-val">{{ c.oldVal || '---' }}</span></span>
        <span class="diff-arrow">➔</span>
        <span class="diff-item diff-new"><span class="diff-val">{{ c.newVal || '---' }}</span></span>
      </div>
    </template>

    <!-- Giá trị tự do: giữ cách hiển thị Trước ➔ Sau -->
    <template v-else>
      <div v-if="oldValue" class="diff-item diff-old">
        <span class="diff-lbl">Trước:</span>
        <span class="diff-val">{{ oldValue }}</span>
      </div>
      <div v-if="oldValue && newValue" class="diff-arrow">➔</div>
      <div v-if="newValue" class="diff-item diff-new">
        <span class="diff-lbl">Sau:</span>
        <span class="diff-val">{{ newValue }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  oldValue: { type: String, default: '' },
  newValue: { type: String, default: '' }
})

// "A: x | B: y" hoặc "A: x, B: y" -> [['A','x'],['B','y']]; null nếu không đúng dạng khóa-giá trị
const parsePairs = (text) => {
  if (!text) return null
  const parts = text.split(/\s*\|\s*|,\s*(?=[^,:]+:\s)/)
  const pairs = []
  for (const part of parts) {
    const m = part.match(/^([^:]+):\s*(.*)$/)
    if (!m) return null
    pairs.push([m[1].trim(), m[2].trim()])
  }
  return pairs
}

const changes = computed(() => {
  const o = parsePairs(props.oldValue)
  const n = parsePairs(props.newValue)
  if (!o || !n) return null
  const oldMap = new Map(o)
  const newMap = new Map(n)
  // Hai vế phải cùng bộ khóa thì mới so sánh theo từng trường
  if (oldMap.size !== newMap.size || [...oldMap.keys()].some(k => !newMap.has(k))) return null
  return [...newMap.entries()]
    .filter(([k, v]) => oldMap.get(k) !== v)
    .map(([key, newVal]) => ({ key, oldVal: oldMap.get(key), newVal }))
})
</script>

<style scoped>
.diff-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.diff-key {
  font-weight: 600;
  min-width: 90px;
}
.diff-same {
  opacity: 0.7;
}
</style>
