<template>
  <div class="glass-card kpi-card" :class="`theme-${theme}`">
    <div class="kpi-top">
      <span class="kpi-title">{{ title }}</span>
      <div class="kpi-icon-wrap">
        <slot name="icon"></slot>
      </div>
    </div>

    <div class="kpi-main">
      <div class="kpi-value">
        <span v-if="loading" class="loading-spinner"></span>
        <span v-else>{{ value }}</span>
      </div>
      <div v-if="subtitle" class="kpi-subtitle">
        {{ subtitle }}
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  },
  value: {
    type: [Number, String],
    default: 0
  },
  subtitle: {
    type: String,
    default: ''
  },
  theme: {
    type: String,
    default: 'indigo' // 'indigo', 'emerald', 'amber', 'rose', 'purple'
  },
  loading: {
    type: Boolean,
    default: false
  }
})
</script>

<style scoped>
.kpi-card {
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 125px;
}

.kpi-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.kpi-title {
  font-size: 0.825rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.kpi-icon-wrap {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.kpi-main {
  margin-top: 14px;
}

.kpi-value {
  font-size: 2rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.03em;
  line-height: 1;
}

.kpi-subtitle {
  font-size: 0.775rem;
  color: var(--text-dim);
  margin-top: 6px;
}

/* Themes */
.theme-indigo .kpi-icon-wrap {
  background: #eef2ff;
  color: #4f46e5;
}
.theme-indigo:hover {
  border-color: #c7d2fe;
}

.theme-emerald .kpi-icon-wrap {
  background: #ecfdf5;
  color: #059669;
}
.theme-emerald:hover {
  border-color: #a7f3d0;
}

.theme-amber .kpi-icon-wrap {
  background: #fffbeb;
  color: #d97706;
}
.theme-amber:hover {
  border-color: #fde68a;
}

.theme-rose .kpi-icon-wrap {
  background: #fef2f2;
  color: #dc2626;
}
.theme-rose:hover {
  border-color: #fecaca;
}

.theme-purple .kpi-icon-wrap {
  background: #f5f3ff;
  color: #7c3aed;
}
.theme-purple:hover {
  border-color: #ddd6fe;
}
</style>
