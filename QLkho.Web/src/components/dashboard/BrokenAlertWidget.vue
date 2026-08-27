<template>
  <div class="glass-card broken-card">
    <div class="card-header">
      <div>
        <div class="alert-title-wrap">
          <span class="warning-icon">⚠️</span>
          <h3 class="card-title">{{ $t('dashboard.broken_alert_title') }}</h3>
        </div>
        <p class="card-subtitle">{{ $t('dashboard.broken_alert_subtitle') }}</p>
      </div>

      <div class="count-badge" v-if="brokenList.length > 0">
        {{ brokenList.length }} {{ $t('dashboard.unit_devices') }}
      </div>
    </div>

    <div v-if="loading" class="widget-loading">
      <span class="loading-spinner"></span>
      <p>{{ $t('dashboard.loading_broken') }}</p>
    </div>

    <div v-else-if="brokenList.length === 0" class="empty-success">
      <div class="success-icon">✓</div>
      <div class="success-text">{{ $t('dashboard.empty_broken_success') }}</div>
    </div>

    <div v-else class="broken-list">
      <div v-for="item in brokenList" :key="item.assetID" class="broken-item">
        <div class="item-left">
          <div class="asset-code-badge">{{ item.assetCode }}</div>
          <div class="item-main-info">
            <div class="asset-name">{{ item.assetName }}</div>
            <div class="asset-meta">
              <span>{{ $t('dashboard.category') }} <strong>{{ item.categoryName }}</strong></span>
              <span v-if="item.brand">{{ $t('dashboard.brand') }} <strong>{{ item.brand }}</strong></span>
              <span v-if="item.warehouseLocation">{{ $t('dashboard.location') }} <strong>{{ item.warehouseLocation }}</strong></span>
            </div>
            <div v-if="item.latestIssue" class="issue-box">
              <span class="issue-label">{{ $t('dashboard.reported_error') }}</span> {{ item.latestIssue }}
            </div>
          </div>
        </div>

        <div class="item-right">
          <StatusBadge :status="item.status" type="asset" />
          <span v-if="item.vendorName" class="vendor-tag">{{ $t('dashboard.repair_vendor') }} {{ item.vendorName }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/client'
import StatusBadge from '@/components/common/StatusBadge.vue'

const brokenList = ref([])
const loading = ref(false)

const fetchBrokenHighlights = async () => {
  loading.value = true
  try {
    const res = await dashboardApi.getBrokenHighlights()
    brokenList.value = res || []
  } catch (err) {
    console.error('Lỗi tải danh sách hỏng hóc:', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchBrokenHighlights()
})
</script>

<style scoped>
.broken-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  border-color: rgba(239, 68, 68, 0.2);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.alert-title-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
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

.count-badge {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 9999px;
}

.widget-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px 0;
  gap: 10px;
  color: var(--text-dim);
}

.empty-success {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 24px;
  background: rgba(16, 185, 129, 0.06);
  border: 1px dashed rgba(16, 185, 129, 0.25);
  border-radius: var(--radius-md);
}

.success-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--success-bg);
  color: var(--success);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}

.success-text {
  color: #34d399;
  font-size: 0.875rem;
  font-weight: 500;
}

.broken-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.broken-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 14px 16px;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  transition: var(--transition);
}

.broken-item:hover {
  background: #fef2f2;
  border-color: #fca5a5;
}

.item-left {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.asset-code-badge {
  background: #f1f5f9;
  color: #0f172a;
  font-family: monospace;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 4px 8px;
  border-radius: 6px;
  border: 1px solid var(--border-color);
  white-space: nowrap;
}

.asset-name {
  font-size: 0.9rem;
  font-weight: 700;
  color: #0f172a;
}

.asset-meta {
  display: flex;
  gap: 14px;
  font-size: 0.775rem;
  color: var(--text-dim);
  margin-top: 3px;
}

.asset-meta strong {
  color: var(--text-muted);
}

.issue-box {
  margin-top: 6px;
  font-size: 0.8rem;
  color: #b91c1c;
  background: #fee2e2;
  padding: 4px 8px;
  border-radius: 4px;
  border: 1px solid #fca5a5;
}

.issue-label {
  font-weight: 700;
  color: #dc2626;
}

.item-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}

.vendor-tag {
  font-size: 0.725rem;
  color: var(--text-dim);
}
</style>
