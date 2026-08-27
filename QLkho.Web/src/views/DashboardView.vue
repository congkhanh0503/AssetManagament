<template>
  <div class="dashboard-page">
    <!-- Top Header Title & Actions -->
    <div class="dashboard-topbar">
      <div>
        <h2 class="page-title">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="color: #6366f1;">
            <rect x="3" y="3" width="7" height="7"></rect>
            <rect x="14" y="3" width="7" height="7"></rect>
            <rect x="14" y="14" width="7" height="7"></rect>
            <rect x="3" y="14" width="7" height="7"></rect>
          </svg>
          {{ $t('dashboard.title') }}
        </h2>
        <p class="page-subtitle">{{ $t('dashboard.subtitle') }}</p>
      </div>

      <button class="btn btn-secondary" @click="refreshAll" :disabled="loadingKpis">
        <svg :class="{ 'spin-icon': loadingKpis }" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="23 4 23 10 17 10"></polyline>
          <polyline points="1 20 1 14 7 14"></polyline>
          <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
        </svg>
        {{ $t('common.refresh') }}
      </button>
    </div>

    <!-- KPI Metric Cards Grid (6 Cards) -->
    <div class="kpi-grid">
      <!-- 1. Tổng thiết bị -->
      <KpiCard 
        :title="$t('dashboard.total_assets')" 
        :value="kpis.totalAssets" 
        :subtitle="$t('dashboard.total_assets_sub')"
        theme="indigo"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
            <line x1="8" y1="21" x2="16" y2="21"></line>
            <line x1="12" y1="17" x2="12" y2="21"></line>
          </svg>
        </template>
      </KpiCard>

      <!-- 2. Đang cấp phát -->
      <KpiCard 
        :title="$t('dashboard.in_use_assets')" 
        :value="kpis.inUseAssets" 
        :subtitle="$t('dashboard.in_use_rate', { percent: getPercent(kpis.inUseAssets, kpis.totalAssets) })"
        theme="purple"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
            <circle cx="9" cy="7" r="4"></circle>
          </svg>
        </template>
      </KpiCard>

      <!-- 3. Tồn kho sẵn sàng -->
      <KpiCard 
        :title="$t('dashboard.available_assets')" 
        :value="kpis.availableAssets" 
        :subtitle="$t('dashboard.available_assets_sub')"
        theme="emerald"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
            <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
            <line x1="12" y1="22.08" x2="12" y2="12"></line>
          </svg>
        </template>
      </KpiCard>

      <!-- 4. Hỏng hóc -->
      <KpiCard 
        :title="$t('dashboard.broken_assets')" 
        :value="kpis.brokenAssets" 
        :subtitle="$t('dashboard.broken_assets_sub')"
        theme="rose"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="15" y1="9" x2="9" y2="15"></line>
            <line x1="9" y1="9" x2="15" y2="15"></line>
          </svg>
        </template>
      </KpiCard>

      <!-- 5. Đang bảo trì -->
      <KpiCard 
        :title="$t('dashboard.maintenance_assets')" 
        :value="kpis.maintenanceAssets" 
        :subtitle="$t('dashboard.maintenance_assets_sub')"
        theme="amber"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"></path>
          </svg>
        </template>
      </KpiCard>

      <!-- 6. Cảnh báo nghỉ việc -->
      <KpiCard 
        :title="$t('dashboard.resigned_employees')" 
        :value="kpis.resignedEmployeesWithAlerts" 
        :subtitle="$t('dashboard.resigned_employees_sub')"
        theme="rose"
        :loading="loadingKpis"
      >
        <template #icon>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
            <line x1="12" y1="9" x2="12" y2="13"></line>
            <line x1="12" y1="17" x2="12.01" y2="17"></line>
          </svg>
        </template>
      </KpiCard>
    </div>

    <!-- Main Grid: Highlight Cấp/Trả Tuần/Tháng & Phân bổ chủng loại -->
    <div class="dashboard-widgets-grid">
      <!-- Widget Highlight Cấp phát / Thu hồi & Top bộ phận -->
      <HighlightWidget ref="highlightWidgetRef" />

      <!-- Widget Phân bổ chủng loại thiết bị (Categories) -->
      <div class="glass-card category-dist-card">
        <h3 class="card-title">{{ $t('dashboard.category_dist_title') }}</h3>
        <p class="card-subtitle">{{ $t('dashboard.category_dist_subtitle') }}</p>

        <div v-if="loadingCategories" class="loading-wrap">
          <span class="loading-spinner"></span>
        </div>

        <div v-else class="category-list">
          <div v-for="cat in categories" :key="cat.categoryID" class="category-item">
            <div class="cat-header">
              <span class="cat-name">{{ cat.categoryName }}</span>
              <span class="cat-badge">{{ cat.assetCount }} {{ $t('dashboard.unit_devices') }} ({{ cat.percentage }}%)</span>
            </div>
            <div class="cat-progress">
              <div class="cat-progress-bar" :style="{ width: `${cat.percentage}%` }"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Danh sách / Card Top 5 Nhân Viên Giữ Nhiều Thiết Bị Nhất -->
    <div class="top-holding-section-wrap">
      <TopHoldingEmployeesWidget ref="topHoldingWidgetRef" />
    </div>

    <!-- Biểu đồ Thống kê Hãng Bị Hỏng & Bảo Trì (Laptop riêng 1 chart, Thiết bị khác 1 chart) -->
    <div class="brand-defect-section-wrap">
      <BrandDefectChartsWidget ref="brandDefectWidgetRef" />
    </div>

    <!-- Highlight Thiết bị hỏng hóc & Sự cố -->
    <div class="broken-section">
      <BrokenAlertWidget ref="brokenWidgetRef" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/client'
import KpiCard from '@/components/dashboard/KpiCard.vue'
import HighlightWidget from '@/components/dashboard/HighlightWidget.vue'
import BrokenAlertWidget from '@/components/dashboard/BrokenAlertWidget.vue'
import BrandDefectChartsWidget from '@/components/dashboard/BrandDefectChartsWidget.vue'
import TopHoldingEmployeesWidget from '@/components/dashboard/TopHoldingEmployeesWidget.vue'

const kpis = ref({
  totalAssets: 0,
  inUseAssets: 0,
  availableAssets: 0,
  brokenAssets: 0,
  maintenanceAssets: 0,
  disposedAssets: 0,
  resignedEmployeesWithAlerts: 0
})

const categories = ref([])
const loadingKpis = ref(false)
const loadingCategories = ref(false)

const highlightWidgetRef = ref(null)
const brokenWidgetRef = ref(null)
const brandDefectWidgetRef = ref(null)
const topHoldingWidgetRef = ref(null)

const getPercent = (part, total) => {
  if (!total || total === 0) return 0
  return Math.round((part / total) * 100)
}

const fetchKpis = async () => {
  loadingKpis.value = true
  try {
    const res = await dashboardApi.getKpis()
    if (res) kpis.value = res
  } catch (err) {
    console.error('Lỗi tải KPIs:', err)
  } finally {
    loadingKpis.value = false
  }
}

const fetchCategoryDistribution = async () => {
  loadingCategories.value = true
  try {
    const res = await dashboardApi.getCategoryDistribution()
    categories.value = res || []
  } catch (err) {
    console.error('Lỗi tải phân bổ loại thiết bị:', err)
  } finally {
    loadingCategories.value = false
  }
}

const refreshAll = () => {
  fetchKpis()
  fetchCategoryDistribution()
  if (highlightWidgetRef.value?.fetchHighlights) highlightWidgetRef.value.fetchHighlights()
  if (brokenWidgetRef.value?.fetchBrokenHighlights) brokenWidgetRef.value.fetchBrokenHighlights()
  if (brandDefectWidgetRef.value?.fetchStats) brandDefectWidgetRef.value.fetchStats()
  if (topHoldingWidgetRef.value?.fetchTopEmployees) topHoldingWidgetRef.value.fetchTopEmployees()
}

onMounted(() => {
  refreshAll()
})
</script>

<style scoped>
.dashboard-page {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.dashboard-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.dashboard-widgets-grid {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  gap: 20px;
}

@media (max-width: 1024px) {
  .dashboard-widgets-grid {
    grid-template-columns: 1fr;
  }
}

.category-dist-card {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.card-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #0f172a;
}

.card-subtitle {
  font-size: 0.8rem;
  color: var(--text-dim);
}

.loading-wrap {
  display: flex;
  justify-content: center;
  padding: 40px 0;
}

.category-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-top: 6px;
}

.category-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.cat-header {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
}

.cat-name {
  color: var(--text-main);
  font-weight: 600;
}

.cat-badge {
  color: #7c3aed;
  font-weight: 700;
}

.cat-progress {
  width: 100%;
  height: 6px;
  background: #e2e8f0;
  border-radius: 999px;
  overflow: hidden;
}

.cat-progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #7c3aed 0%, #a855f7 100%);
  border-radius: 999px;
  transition: width 0.6s ease;
}

.broken-section {
  margin-top: 4px;
}

.spin-icon {
  animation: spin 0.8s linear infinite;
}
</style>
