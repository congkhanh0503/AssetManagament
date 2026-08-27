<template>
  <div class="glass-card top-holding-card">
    <div class="card-header">
      <div class="title-wrap">
        <div class="title-icon">🏆</div>
        <div>
          <h3 class="card-title">{{ $t('dashboard.top_holding_title') }}</h3>
          <p class="card-subtitle">{{ $t('dashboard.top_holding_subtitle') }}</p>
        </div>
      </div>

      <router-link to="/employees" class="btn-link-more">
        {{ $t('dashboard.view_all_employees') }}
      </router-link>
    </div>

    <div v-if="loading" class="widget-loading">
      <span class="loading-spinner"></span>
      <p>{{ $t('dashboard.loading_holding') }}</p>
    </div>

    <div v-else-if="employees.length === 0" class="empty-state">
      <div class="empty-icon">👥</div>
      <p>{{ $t('dashboard.no_holding_employees') }}</p>
    </div>

    <div v-else class="ranking-list">
      <div 
        v-for="emp in employees" 
        :key="emp.employeeID" 
        class="ranking-item"
        :class="`rank-${emp.rank}`"
      >
        <!-- 1. Rank Medal / Number -->
        <div class="rank-col">
          <div class="rank-badge" :class="`badge-rank-${emp.rank}`">
            <span v-if="emp.rank === 1">🥇</span>
            <span v-else-if="emp.rank === 2">🥈</span>
            <span v-else-if="emp.rank === 3">🥉</span>
            <span v-else>#{{ emp.rank }}</span>
          </div>
        </div>

        <!-- 2. Avatar & Thông tin nhân viên -->
        <div class="emp-main-info">
          <div class="emp-avatar" :style="{ background: getAvatarGradient(emp.fullName) }">
            {{ getInitials(emp.fullName) }}
          </div>
          <div class="emp-details">
            <div class="emp-name-row">
              <strong class="emp-name">{{ emp.fullName }}</strong>
              <span v-if="emp.englishName" class="emp-en-name">({{ emp.englishName }})</span>
              <span class="emp-code">{{ emp.employeeCode }}</span>
            </div>
            <div class="emp-meta-row">
              <span class="emp-dept" v-if="emp.departmentName">
                🏢 {{ emp.departmentName }}
              </span>
              <span class="emp-title" v-if="emp.title">
                💼 {{ emp.title }}
              </span>
              <span class="emp-phone" v-if="emp.phone">
                📞 {{ emp.phone }}
              </span>
            </div>
          </div>
        </div>

        <!-- 3. Danh sách thiết bị đang giữ -->
        <div class="emp-assets-col">
          <div class="assets-total-badge">
            <span class="box-icon">📦</span>
            <strong>{{ emp.totalAssetsCount }}</strong> {{ $t('dashboard.unit_devices') }}
          </div>

          <div class="asset-tags-list">
            <span 
              v-for="asset in emp.assetsList" 
              :key="asset.assetID" 
              class="asset-pill-tag"
              :class="getCategoryPillClass(asset.categoryName)"
              :title="`${asset.assetCode} - ${asset.assetName}`"
            >
              {{ getAssetIcon(asset.categoryName) }} {{ asset.assetCode }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/client'

const employees = ref([])
const loading = ref(false)

const fetchTopEmployees = async () => {
  loading.value = true
  try {
    const res = await dashboardApi.getTopHoldingEmployees(5)
    employees.value = res || []
  } catch (err) {
    console.error('Lỗi tải top nhân viên giữ thiết bị:', err)
  } finally {
    loading.value = false
  }
}

const getInitials = (name) => {
  if (!name) return 'NV'
  const parts = name.trim().split(' ')
  if (parts.length >= 2) {
    return (parts[parts.length - 2][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.slice(0, 2).toUpperCase()
}

const getAvatarGradient = (name) => {
  const colors = [
    'linear-gradient(135deg, #6366f1, #4338ca)',
    'linear-gradient(135deg, #06b6d4, #0891b2)',
    'linear-gradient(135deg, #10b981, #059669)',
    'linear-gradient(135deg, #f59e0b, #d97706)',
    'linear-gradient(135deg, #ec4899, #db2777)',
    'linear-gradient(135deg, #8b5cf6, #7c3aed)'
  ]
  let sum = 0
  for (let i = 0; i < (name || '').length; i++) {
    sum += name.charCodeAt(i)
  }
  return colors[sum % colors.length]
}

const getAssetIcon = (catName) => {
  if (!catName) return '📦'
  const c = catName.toLowerCase()
  if (c.includes('laptop') || c.includes('xách tay') || c.includes('macbook')) return '💻'
  if (c.includes('màn hình') || c.includes('monitor') || c.includes('display')) return '🖥️'
  if (c.includes('bàn phím') || c.includes('keyboard')) return '⌨️'
  if (c.includes('chuột') || c.includes('mouse')) return '🖱️'
  if (c.includes('pc') || c.includes('máy tính để bàn') || c.includes('desktop')) return '🖥️'
  return '🔌'
}

const getCategoryPillClass = (catName) => {
  if (!catName) return 'pill-other'
  const c = catName.toLowerCase()
  if (c.includes('laptop') || c.includes('xách tay') || c.includes('macbook')) return 'pill-laptop'
  if (c.includes('màn hình') || c.includes('monitor') || c.includes('display')) return 'pill-monitor'
  if (c.includes('bàn phím') || c.includes('keyboard')) return 'pill-keyboard'
  if (c.includes('chuột') || c.includes('mouse')) return 'pill-mouse'
  return 'pill-other'
}

defineExpose({
  fetchTopEmployees
})

onMounted(() => {
  fetchTopEmployees()
})
</script>

<style scoped>
.top-holding-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: #ffffff;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.title-wrap {
  display: flex;
  align-items: center;
  gap: 12px;
}

.title-icon {
  font-size: 1.6rem;
}

.card-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.card-subtitle {
  font-size: 0.8rem;
  color: var(--text-dim);
  margin: 2px 0 0 0;
}

.btn-link-more {
  font-size: 0.825rem;
  font-weight: 600;
  color: #4f46e5;
  text-decoration: none;
  transition: all 0.2s ease;
}

.btn-link-more:hover {
  color: #4338ca;
  text-decoration: underline;
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

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px 0;
  gap: 8px;
  color: var(--text-dim);
}

.empty-icon {
  font-size: 2rem;
}

.ranking-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ranking-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 18px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: var(--radius-md);
  transition: all 0.25s ease;
}

.ranking-item:hover {
  background: #ffffff;
  border-color: #cbd5e1;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transform: translateY(-1px);
}

/* Hiệu ứng Top 3 Rank */
.ranking-item.rank-1 {
  background: linear-gradient(180deg, #fffdf0, #ffffff);
  border-color: #fde047;
  border-left: 4px solid #eab308;
}

.ranking-item.rank-2 {
  background: linear-gradient(180deg, #f8fafc, #ffffff);
  border-color: #cbd5e1;
  border-left: 4px solid #94a3b8;
}

.ranking-item.rank-3 {
  background: linear-gradient(180deg, #fff7ed, #ffffff);
  border-color: #fdba74;
  border-left: 4px solid #f97316;
}

.rank-col {
  flex-shrink: 0;
}

.rank-badge {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.15rem;
  font-weight: 800;
}

.badge-rank-1 {
  background: #fef08a;
  box-shadow: 0 0 10px rgba(234, 179, 8, 0.35);
}

.badge-rank-2 {
  background: #e2e8f0;
  box-shadow: 0 0 10px rgba(148, 163, 184, 0.35);
}

.badge-rank-3 {
  background: #ffedd5;
  box-shadow: 0 0 10px rgba(249, 115, 22, 0.35);
}

.rank-badge:not(.badge-rank-1):not(.badge-rank-2):not(.badge-rank-3) {
  background: #f1f5f9;
  color: #64748b;
  font-size: 0.85rem;
  border: 1px solid #cbd5e1;
}

.emp-main-info {
  display: flex;
  align-items: center;
  gap: 14px;
  flex: 1;
  min-width: 200px;
}

.emp-avatar {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.95rem;
  font-weight: 700;
  flex-shrink: 0;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.12);
}

.emp-details {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.emp-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.emp-name {
  font-size: 0.95rem;
  color: #0f172a;
}

.emp-en-name {
  font-size: 0.8rem;
  color: #4f46e5;
  font-weight: 600;
}

.emp-code {
  font-size: 0.75rem;
  font-family: monospace;
  font-weight: 700;
  background: #e0e7ff;
  color: #4338ca;
  padding: 1px 6px;
  border-radius: 4px;
}

.emp-meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.775rem;
  color: var(--text-dim);
  flex-wrap: wrap;
}

.emp-dept {
  color: #475569;
  font-weight: 500;
}

.emp-phone {
  color: #059669;
  font-weight: 600;
}

.emp-assets-col {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}

.assets-total-badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  background: #eff6ff;
  color: #1d4ed8;
  border: 1px solid #bfdbfe;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 0.8rem;
}

.assets-total-badge strong {
  font-size: 0.95rem;
  color: #1e40af;
}

.asset-tags-list {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.asset-pill-tag {
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 7px;
  border-radius: 4px;
  border: 1px solid transparent;
  white-space: nowrap;
}

.pill-laptop {
  background: #ede9fe;
  color: #6d28d9;
  border-color: #ddd6fe;
}

.pill-monitor {
  background: #e0f2fe;
  color: #0369a1;
  border-color: #bae6fd;
}

.pill-keyboard {
  background: #ecfdf5;
  color: #047857;
  border-color: #a7f3d0;
}

.pill-mouse {
  background: #fdf4ff;
  color: #a21caf;
  border-color: #f5d0fe;
}

.pill-other {
  background: #f1f5f9;
  color: #475569;
  border-color: #e2e8f0;
}

@media (max-width: 768px) {
  .ranking-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  .emp-assets-col {
    align-items: flex-start;
    width: 100%;
  }
  .asset-tags-list {
    justify-content: flex-start;
  }
}
</style>
