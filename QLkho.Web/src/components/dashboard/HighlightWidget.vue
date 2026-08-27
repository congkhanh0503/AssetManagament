<template>
  <div class="glass-card highlight-card">
    <div class="card-header">
      <div>
        <h3 class="card-title">{{ $t('dashboard.highlight_title') }}</h3>
        <p class="card-subtitle">{{ $t('dashboard.highlight_subtitle') }}</p>
      </div>

      <!-- Period Selector Toggle -->
      <div class="period-toggle">
        <button 
          :class="['period-btn', { active: period === 'week' }]"
          @click="changePeriod('week')"
        >
          {{ $t('dashboard.by_week') }}
        </button>
        <button 
          :class="['period-btn', { active: period === 'month' }]"
          @click="changePeriod('month')"
        >
          {{ $t('dashboard.by_month') }}
        </button>
      </div>
    </div>

    <div v-if="loading" class="widget-loading">
      <span class="loading-spinner"></span>
      <p>{{ $t('common.loading') }}</p>
    </div>

    <div v-else class="highlight-body">
      <!-- 3 Stat Summary Blocks (Lượt Cấp phát, Lượt Thu hồi, Lượt Báo hỏng/Bảo trì) -->
      <div class="stat-grid">
        <div class="stat-box assigned">
          <div class="stat-icon-circle">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"></line>
              <polyline points="19 12 12 19 5 12"></polyline>
            </svg>
          </div>
          <div class="stat-info">
            <span class="stat-label">{{ $t('dashboard.stat_assigned') }}</span>
            <div class="stat-num">{{ data.assignedCount || 0 }} <span class="unit">{{ $t('dashboard.unit_times') }}</span></div>
          </div>
        </div>

        <div class="stat-box returned">
          <div class="stat-icon-circle">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="19" x2="12" y2="5"></line>
              <polyline points="5 12 12 5 19 12"></polyline>
            </svg>
          </div>
          <div class="stat-info">
            <span class="stat-label">{{ $t('dashboard.stat_returned') }}</span>
            <div class="stat-num">{{ data.returnedCount || 0 }} <span class="unit">{{ $t('dashboard.unit_times') }}</span></div>
          </div>
        </div>

        <div class="stat-box broken">
          <div class="stat-icon-circle">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
              <line x1="12" y1="9" x2="12" y2="13"></line>
              <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
          </div>
          <div class="stat-info">
            <span class="stat-label">{{ $t('dashboard.stat_broken_maint') }}</span>
            <div class="stat-num">{{ data.brokenReportedCount || 0 }} <span class="unit">{{ $t('dashboard.unit_devices') }}</span></div>
          </div>
        </div>
      </div>

      <!-- Top Departments List -->
      <div class="top-departments-section">
        <div class="section-heading">
          <span>{{ $t('dashboard.top_dept_heading', { period: period === 'week' ? $t('dashboard.in_this_week') : $t('dashboard.in_this_month') }) }}</span>
        </div>

        <div v-if="!data.topAssignedDepartments || data.topAssignedDepartments.length === 0" class="empty-mini">
          {{ $t('dashboard.no_activity_period') }}
        </div>

        <div v-else class="dept-list">
          <div v-for="dept in data.topAssignedDepartments" :key="dept.departmentID" class="dept-item">
            <div class="dept-info">
              <span class="dept-name">{{ dept.departmentName }}</span>
              <span class="dept-count">{{ dept.assetCount }} {{ $t('dashboard.unit_machines') }} ({{ dept.percentage }}%)</span>
            </div>
            <div class="progress-bar-bg">
              <div class="progress-bar-fill" :style="{ width: `${dept.percentage}%` }"></div>
            </div>
          </div>
        </div>

        <!-- Nút Xem chi tiết lịch sử tuần/tháng -->
        <div class="history-quick-link-box">
          <button type="button" class="btn-history-link" @click="isHistoryOpen = true">
            <span>📜 {{ $t('dashboard.view_history_detail', { period: period === 'week' ? $t('dashboard.this_week') : $t('dashboard.this_month') }) }}</span>
            <span class="arrow-icon">➔</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Lịch Sử Cấp Phát -->
    <HandoverHistoryModal 
      :is-open="isHistoryOpen" 
      :initial-period="period" 
      @close="isHistoryOpen = false" 
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/client'
import HandoverHistoryModal from '@/components/assets/HandoverHistoryModal.vue'

const period = ref('month')
const loading = ref(false)
const isHistoryOpen = ref(false)
const data = ref({
  assignedCount: 0,
  returnedCount: 0,
  topAssignedDepartments: []
})

const fetchHighlights = async () => {
  loading.value = true
  try {
    const res = await dashboardApi.getHighlights(period.value)
    data.value = res || { assignedCount: 0, returnedCount: 0, topAssignedDepartments: [] }
  } catch (err) {
    console.error('Lỗi lấy dữ liệu highlight:', err)
  } finally {
    loading.value = false
  }
}

const changePeriod = (newPeriod) => {
  if (period.value === newPeriod) return
  period.value = newPeriod
  fetchHighlights()
}

onMounted(() => {
  fetchHighlights()
})
</script>

<style scoped>
.highlight-card {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.card-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: #0f172a;
}

.card-subtitle {
  font-size: 0.8rem;
  color: var(--text-dim);
  margin-top: 2px;
}

.period-toggle {
  display: flex;
  background: #f1f5f9;
  padding: 3px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
}

.period-btn {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-family: inherit;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 6px 14px;
  border-radius: 6px;
  cursor: pointer;
  transition: var(--transition);
}

.period-btn.active {
  background: #ffffff;
  color: var(--primary);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.widget-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 0;
  gap: 10px;
  color: var(--text-dim);
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
}

.stat-box {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  transition: all 0.2s ease;
}

.stat-box:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
}

.stat-box.assigned {
  background: #f0fdf4;
  border-color: #bbf7d0;
}

.stat-box.assigned .stat-icon-circle {
  background: #dcfce7;
  color: #059669;
}

.stat-box.returned {
  background: #eef2ff;
  border-color: #c7d2fe;
}

.stat-box.returned .stat-icon-circle {
  background: #e0e7ff;
  color: #4f46e5;
}

.stat-box.broken {
  background: #fef2f2;
  border-color: #fecaca;
}

.stat-box.broken .stat-icon-circle {
  background: #fee2e2;
  color: #dc2626;
}

.stat-icon-circle {
  width: 42px;
  height: 42px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-label {
  font-size: 0.775rem;
  color: var(--text-muted);
  font-weight: 600;
}

.stat-num {
  font-size: 1.55rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.2;
}

.unit {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--text-dim);
}

.top-departments-section {
  margin-top: 10px;
}

.section-heading {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 12px;
}

.dept-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.dept-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.dept-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
}

.dept-name {
  color: var(--text-main);
  font-weight: 600;
}

.dept-count {
  color: #4f46e5;
  font-weight: 700;
}

.progress-bar-bg {
  width: 100%;
  height: 7px;
  background: #e2e8f0;
  border-radius: 999px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #4f46e5 0%, #3b82f6 100%);
  border-radius: 999px;
  transition: width 0.6s ease;
}

.empty-mini {
  text-align: center;
  padding: 16px;
  font-size: 0.825rem;
  color: var(--text-dim);
  background: #f8fafc;
  border-radius: var(--radius-sm);
}

.history-quick-link-box {
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px dashed var(--border-color);
}

.btn-history-link {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #eef2ff;
  border: 1px solid #c7d2fe;
  color: #4f46e5;
  font-size: 0.825rem;
  font-weight: 600;
  padding: 8px 14px;
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: var(--transition);
}

.btn-history-link:hover {
  background: #e0e7ff;
  border-color: var(--primary);
  color: #4338ca;
  transform: translateY(-1px);
}

.arrow-icon {
  font-size: 0.9rem;
  transition: transform 0.2s;
}

.btn-history-link:hover .arrow-icon {
  transform: translateX(4px);
}
</style>
