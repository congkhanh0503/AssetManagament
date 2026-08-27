<template>
  <div class="brand-defect-section">
    <div class="section-header">
      <div class="header-left">
        <span class="header-icon">📊</span>
        <div>
          <h3 class="section-title">{{ $t('dashboard.brand_defect_title') }}</h3>
          <p class="section-subtitle">
            {{ $t('dashboard.brand_defect_subtitle', { period: selectedMonthLabel }) }}
          </p>
        </div>
      </div>

      <div class="header-right-controls">
        <!-- Bộ lọc tháng -->
        <div class="month-filter-group">
          <label class="month-filter-lbl">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="16" y1="2" x2="16" y2="6"></line>
              <line x1="8" y1="2" x2="8" y2="6"></line>
              <line x1="3" y1="10" x2="21" y2="10"></line>
            </svg>
            <span>{{ $t('dashboard.stat_period') }}</span>
          </label>
          <select 
            class="month-select" 
            v-model="selectedMonth" 
            @change="fetchStats"
            :disabled="loading"
          >
            <option value="all">{{ $t('dashboard.all_months') }}</option>
            <option 
              v-for="m in availableMonths" 
              :key="m" 
              :value="m"
            >
              {{ $t('dashboard.month_prefix') }} {{ formatMonth(m) }}
            </option>
          </select>
        </div>

        <div class="header-legend">
          <span class="legend-item"><span class="legend-dot broken"></span> {{ $t('dashboard.legend_broken') }}</span>
          <span class="legend-item"><span class="legend-dot maintenance"></span> {{ $t('dashboard.legend_maintenance') }}</span>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-state glass-card">
      <span class="loading-spinner"></span>
      <p>{{ $t('common.loading') }}</p>
    </div>

    <div v-else class="charts-grid-2">
      <!-- BIỂU ĐỒ CỘT 1: LAPTOP THEO HÃNG -->
      <div class="glass-card chart-card laptop-chart-card">
        <div class="card-top">
          <div class="card-title-group">
            <div class="chart-type-icon laptop-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
                <line x1="8" y1="21" x2="16" y2="21"></line>
                <line x1="12" y1="17" x2="12" y2="21"></line>
              </svg>
            </div>
            <div>
              <h4 class="card-heading">{{ $t('dashboard.laptop_chart_title') }}</h4>
              <span class="card-subheading">{{ $t('dashboard.laptop_chart_sub', { period: selectedMonthLabel }) }}</span>
            </div>
          </div>

          <div class="total-badge" :class="{ 'has-defect': stats.totalLaptopDefects > 0 }">
            {{ $t('dashboard.devices_with_defect', { count: stats.totalLaptopDefects }) }}
          </div>
        </div>

        <div v-if="stats.laptopStats.length === 0" class="empty-clean">
          <div class="empty-icon">✓</div>
          <div class="empty-text">{{ $t('dashboard.no_laptop_defects') }}</div>
        </div>

        <div v-else class="column-chart-container">
          <!-- Trục Y và Grid lines -->
          <div class="chart-grid-lines">
            <div class="grid-line" v-for="tick in maxLaptopTicks" :key="tick">
              <span class="grid-label">{{ tick }}</span>
            </div>
            <div class="grid-line base-line">
              <span class="grid-label">0</span>
            </div>
          </div>

          <!-- Các cột biểu đồ -->
          <div class="columns-wrapper">
            <div 
              v-for="item in stats.laptopStats" 
              :key="item.brand" 
              class="column-group"
            >
              <!-- Cột đôi: Hỏng & Bảo trì -->
              <div class="bars-pair">
                <!-- Cột Hỏng (Đỏ) -->
                <div class="bar-col-wrap">
                  <span v-if="item.brokenCount > 0" class="bar-val-label broken">{{ item.brokenCount }}</span>
                  <div 
                    class="bar-pillar broken"
                    :style="{ height: getBarHeight(item.brokenCount, maxLaptopVal) }"
                    :title="`${item.brand}: ${item.brokenCount} ${$t('dashboard.legend_broken')}`"
                  ></div>
                </div>

                <!-- Cột Bảo trì (Vàng) -->
                <div class="bar-col-wrap">
                  <span v-if="item.maintenanceCount > 0" class="bar-val-label maintenance">{{ item.maintenanceCount }}</span>
                  <div 
                    class="bar-pillar maintenance"
                    :style="{ height: getBarHeight(item.maintenanceCount, maxLaptopVal) }"
                    :title="`${item.brand}: ${item.maintenanceCount} ${$t('dashboard.legend_maintenance')}`"
                  ></div>
                </div>
              </div>

              <!-- Tên Hãng phía dưới -->
              <div class="column-x-label">
                <span class="brand-x-name" :title="item.brand">{{ item.brand }}</span>
                <span class="brand-x-total">Tổng: {{ item.totalCount }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- BIỂU ĐỒ CỘT 2: THIẾT BỊ KHÁC THEO HÃNG -->
      <div class="glass-card chart-card other-chart-card">
        <div class="card-top">
          <div class="card-title-group">
            <div class="chart-type-icon other-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="4" y="4" width="16" height="16" rx="2" ry="2"></rect>
                <rect x="9" y="9" width="6" height="6"></rect>
                <line x1="9" y1="1" x2="9" y2="4"></line>
                <line x1="15" y1="1" x2="15" y2="4"></line>
                <line x1="9" y1="20" x2="9" y2="23"></line>
                <line x1="15" y1="20" x2="15" y2="23"></line>
                <line x1="20" y1="9" x2="23" y2="9"></line>
                <line x1="20" y1="14" x2="23" y2="14"></line>
                <line x1="1" y1="9" x2="4" y2="9"></line>
                <line x1="1" y1="14" x2="4" y2="14"></line>
              </svg>
            </div>
            <div>
              <h4 class="card-heading">{{ $t('dashboard.peripheral_chart_title') }}</h4>
              <span class="card-subheading">{{ $t('dashboard.peripheral_chart_sub', { period: selectedMonthLabel }) }}</span>
            </div>
          </div>

          <div class="total-badge" :class="{ 'has-defect': stats.totalOtherDefects > 0 }">
            {{ $t('dashboard.devices_with_defect', { count: stats.totalOtherDefects }) }}
          </div>
        </div>

        <div v-if="stats.otherDeviceStats.length === 0" class="empty-clean">
          <div class="empty-icon">✓</div>
          <div class="empty-text">{{ $t('dashboard.no_peripheral_defects') }}</div>
        </div>

        <div v-else class="column-chart-container">
          <!-- Trục Y và Grid lines -->
          <div class="chart-grid-lines">
            <div class="grid-line" v-for="tick in maxOtherTicks" :key="tick">
              <span class="grid-label">{{ tick }}</span>
            </div>
            <div class="grid-line base-line">
              <span class="grid-label">0</span>
            </div>
          </div>

          <!-- Các cột biểu đồ -->
          <div class="columns-wrapper">
            <div 
              v-for="item in stats.otherDeviceStats" 
              :key="item.brand" 
              class="column-group"
            >
              <!-- Cột đôi: Hỏng & Bảo trì -->
              <div class="bars-pair">
                <!-- Cột Hỏng (Đỏ) -->
                <div class="bar-col-wrap">
                  <span v-if="item.brokenCount > 0" class="bar-val-label broken">{{ item.brokenCount }}</span>
                  <div 
                    class="bar-pillar broken"
                    :style="{ height: getBarHeight(item.brokenCount, maxOtherVal) }"
                    :title="`${item.brand}: ${item.brokenCount} thiết bị bị hỏng`"
                  ></div>
                </div>

                <!-- Cột Bảo trì (Vàng) -->
                <div class="bar-col-wrap">
                  <span v-if="item.maintenanceCount > 0" class="bar-val-label maintenance">{{ item.maintenanceCount }}</span>
                  <div 
                    class="bar-pillar maintenance"
                    :style="{ height: getBarHeight(item.maintenanceCount, maxOtherVal) }"
                    :title="`${item.brand}: ${item.maintenanceCount} thiết bị đang bảo trì`"
                  ></div>
                </div>
              </div>

              <!-- Tên Hãng phía dưới -->
              <div class="column-x-label">
                <span class="brand-x-name" :title="item.brand">{{ item.brand }}</span>
                <span class="brand-x-total">Tổng: {{ item.totalCount }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { dashboardApi } from '@/api/client'

const selectedMonth = ref('all')
const availableMonths = ref([])

const stats = ref({
  laptopStats: [],
  otherDeviceStats: [],
  totalLaptopDefects: 0,
  totalOtherDefects: 0
})

const loading = ref(false)

const formatMonth = (mStr) => {
  if (!mStr || mStr === 'all') return 'Tất cả'
  const parts = mStr.split('-')
  if (parts.length === 2) return `${parts[1]}/${parts[0]}`
  return mStr
}

const selectedMonthLabel = computed(() => {
  if (selectedMonth.value === 'all') return 'Tất Cả Thời Gian'
  return `Tháng ${formatMonth(selectedMonth.value)}`
})

// Tính giá trị max để scale chiều cao cột
const maxLaptopVal = computed(() => {
  if (!stats.value.laptopStats || stats.value.laptopStats.length === 0) return 5
  let max = 0
  stats.value.laptopStats.forEach(item => {
    max = Math.max(max, item.brokenCount, item.maintenanceCount)
  })
  return max <= 4 ? 5 : Math.ceil(max * 1.25)
})

const maxLaptopTicks = computed(() => {
  const max = maxLaptopVal.value
  const mid = Math.round(max / 2)
  return [max, mid]
})

const maxOtherVal = computed(() => {
  if (!stats.value.otherDeviceStats || stats.value.otherDeviceStats.length === 0) return 5
  let max = 0
  stats.value.otherDeviceStats.forEach(item => {
    max = Math.max(max, item.brokenCount, item.maintenanceCount)
  })
  return max <= 4 ? 5 : Math.ceil(max * 1.25)
})

const maxOtherTicks = computed(() => {
  const max = maxOtherVal.value
  const mid = Math.round(max / 2)
  return [max, mid]
})

const getBarHeight = (val, max) => {
  if (!val || val <= 0) return '0px'
  const percent = Math.max(8, (val / max) * 100)
  return `${percent}%`
}

const fetchStats = async () => {
  loading.value = true
  try {
    const res = await dashboardApi.getBrandDefectStats(selectedMonth.value)
    if (res) {
      stats.value = {
        laptopStats: res.laptopStats || [],
        otherDeviceStats: res.otherDeviceStats || [],
        totalLaptopDefects: res.totalLaptopDefects || 0,
        totalOtherDefects: res.totalOtherDefects || 0
      }
      if (res.availableMonths && res.availableMonths.length > 0) {
        availableMonths.value = res.availableMonths
      }
    }
  } catch (err) {
    console.error('Lỗi tải thống kê hãng hỏng & bảo trì theo tháng:', err)
  } finally {
    loading.value = false
  }
}

defineExpose({
  fetchStats
})

onMounted(() => {
  fetchStats()
})
</script>

<style scoped>
.brand-defect-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-top: 6px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-icon {
  font-size: 1.5rem;
}

.section-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.section-subtitle {
  font-size: 0.8rem;
  color: var(--text-dim);
  margin: 2px 0 0 0;
}

.header-right-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.month-filter-group {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #ffffff;
  padding: 4px 10px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.month-filter-lbl {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  cursor: default;
}

.month-select {
  padding: 4px 10px;
  border-radius: 6px;
  border: 1px solid #cbd5e1;
  background: #f8fafc;
  font-size: 0.825rem;
  font-weight: 700;
  color: #1e293b;
  cursor: pointer;
  outline: none;
  transition: all 0.2s ease;
}

.month-select:hover {
  background: #ffffff;
  border-color: #94a3b8;
}

.month-select:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.2);
}

.header-legend {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #ffffff;
  padding: 6px 14px;
  border-radius: 999px;
  border: 1px solid var(--border-color);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
}

.legend-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.legend-dot.broken {
  background: #ef4444;
  box-shadow: 0 0 6px rgba(239, 68, 68, 0.5);
}

.legend-dot.maintenance {
  background: #f59e0b;
  box-shadow: 0 0 6px rgba(245, 158, 11, 0.5);
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 0;
  gap: 10px;
  color: var(--text-dim);
}

.charts-grid-2 {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(360px, 1fr));
  gap: 18px;
}

.chart-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 20px;
  background: #ffffff;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  transition: all 0.25s ease;
}

.chart-card:hover {
  box-shadow: var(--shadow-md);
  border-color: #cbd5e1;
}

.laptop-chart-card {
  border-top: 4px solid #6366f1;
}

.other-chart-card {
  border-top: 4px solid #06b6d4;
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.card-title-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.chart-type-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.laptop-icon {
  background: #e0e7ff;
  color: #4f46e5;
}

.other-icon {
  background: #cffafe;
  color: #0891b2;
}

.card-heading {
  font-size: 1rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.card-subheading {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.total-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 999px;
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
  white-space: nowrap;
}

.total-badge.has-defect {
  background: #fef2f2;
  color: #dc2626;
  border-color: #fecaca;
}

.empty-clean {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 36px 16px;
  background: rgba(16, 185, 129, 0.05);
  border: 1px dashed rgba(16, 185, 129, 0.25);
  border-radius: var(--radius-md);
}

.empty-icon {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #d1fae5;
  color: #059669;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  font-weight: bold;
}

.empty-text {
  font-size: 0.825rem;
  font-weight: 500;
  color: #059669;
}

/* Biểu đồ Cột (Column Chart) Styles */
.column-chart-container {
  position: relative;
  height: 230px;
  display: flex;
  align-items: flex-end;
  padding-left: 28px;
  padding-bottom: 34px;
  margin-top: 10px;
}

.chart-grid-lines {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 34px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  pointer-events: none;
}

.grid-line {
  position: relative;
  width: 100%;
  height: 1px;
  background: rgba(226, 232, 240, 0.7);
}

.grid-line.base-line {
  background: #cbd5e1;
  height: 2px;
}

.grid-label {
  position: absolute;
  left: 0;
  top: -8px;
  font-size: 0.7rem;
  color: #94a3b8;
  font-weight: 600;
  font-family: monospace;
}

.columns-wrapper {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-end;
  justify-content: space-around;
  width: 100%;
  height: 100%;
  padding: 0 10px;
}

.column-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  justify-content: flex-end;
  min-width: 50px;
  max-width: 80px;
  flex: 1;
}

.bars-pair {
  display: flex;
  align-items: flex-end;
  gap: 6px;
  height: 100%;
  width: 100%;
  justify-content: center;
}

.bar-col-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  height: 100%;
  width: 18px;
}

.bar-val-label {
  font-size: 0.7rem;
  font-weight: 700;
  margin-bottom: 3px;
  line-height: 1;
}

.bar-val-label.broken {
  color: #dc2626;
}

.bar-val-label.maintenance {
  color: #d97706;
}

.bar-pillar {
  width: 100%;
  border-radius: 4px 4px 0 0;
  transition: height 0.5s cubic-bezier(0.34, 1.56, 0.64, 1), transform 0.2s ease;
  cursor: pointer;
}

.bar-pillar:hover {
  transform: scaleY(1.05);
  filter: brightness(1.1);
}

.bar-pillar.broken {
  background: linear-gradient(180deg, #f87171, #ef4444);
  box-shadow: 0 -2px 6px rgba(239, 68, 68, 0.25);
}

.bar-pillar.maintenance {
  background: linear-gradient(180deg, #fbbf24, #f59e0b);
  box-shadow: 0 -2px 6px rgba(245, 158, 11, 0.25);
}

.column-x-label {
  position: absolute;
  bottom: -32px;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  text-align: center;
}

.brand-x-name {
  font-size: 0.775rem;
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 70px;
}

.brand-x-total {
  font-size: 0.675rem;
  color: #64748b;
  white-space: nowrap;
}

@media (max-width: 768px) {
  .charts-grid-2 {
    grid-template-columns: 1fr;
  }
}
</style>
