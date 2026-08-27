<template>
  <div class="asset-selector-root">
    <!-- 1. Trạng Thái ĐÃ CHỌN Thiết Bị -->
    <div v-if="selectedAsset" class="selected-asset-card">
      <div class="selected-asset-main">
        <div class="selected-asset-icon">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
            <line x1="8" y1="21" x2="16" y2="21"></line>
            <line x1="12" y1="17" x2="12" y2="21"></line>
          </svg>
        </div>
        <div class="selected-asset-info">
          <div class="selected-asset-title-row">
            <span class="selected-asset-code">{{ selectedAsset.assetCode }}</span>
            <strong class="selected-asset-name">{{ selectedAsset.assetName }}</strong>
            <span class="selected-asset-cat">{{ selectedAsset.categoryName }}</span>
            <span v-if="selectedAsset.brand" class="selected-asset-brand">{{ selectedAsset.brand }}</span>
          </div>
          <div v-if="selectedAsset.specifications" class="selected-asset-specs">
            ⚙️ <strong>Cấu hình:</strong> {{ selectedAsset.specifications }}
          </div>
          <div class="selected-asset-meta-row">
            <span v-if="selectedAsset.serialNumber" class="meta-item">🏷️ S/N: <strong class="mono-text">{{ selectedAsset.serialNumber }}</strong></span>
            <span class="meta-item">📍 Vị trí kho: <strong style="color: #34d399;">{{ selectedAsset.warehouseLocation || 'Kho IT' }}</strong></span>
            <span v-if="selectedAsset.supplierName" class="meta-item">🏢 NCC: {{ selectedAsset.supplierName }}</span>
          </div>
        </div>
      </div>
      <button type="button" class="btn-change-asset" @click="clearSelection" title="Chọn thiết bị khác trong kho">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
        </svg>
        <span>Đổi thiết bị</span>
      </button>
    </div>

    <!-- 2. Trạng Thái CHƯA CHỌN: Thanh Tìm Kiếm, Lọc Loại Máy & Danh Sách Cuộn -->
    <div v-else class="selector-picker-box">
      <!-- Filter & Search Controls -->
      <div class="picker-search-bar">
        <div class="search-input-wrap">
          <svg class="search-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"></circle>
            <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
          </svg>
          <input 
            type="text" 
            class="form-control form-control-sm picker-input" 
            v-model="searchTerm" 
            placeholder="🔍 Gõ mã máy (AST-..), tên thiết bị, hãng, cấu hình (i5, 16GB), S/N..."
            autofocus
          />
          <button v-if="searchTerm" type="button" class="btn-clear-search" @click="searchTerm = ''">✕</button>
        </div>

        <!-- Filter Loại thiết bị -->
        <div class="cat-filter-wrap">
          <select class="form-control form-control-sm picker-cat-select" v-model="filterCatId">
            <option :value="null">-- Tất cả loại máy --</option>
            <option v-for="cat in categories" :key="cat.categoryID" :value="cat.categoryID">
              {{ cat.categoryName }}
            </option>
          </select>
        </div>
      </div>

      <!-- Quick Filter Pills -->
      <div class="quick-filter-pills">
        <button 
          type="button" 
          :class="['filter-pill', { active: quickFilter === 'all' }]"
          @click="quickFilter = 'all'"
        >
          Tất Cả Sẵn Sàng ({{ assetsList.length }})
        </button>
        <button 
          v-for="cat in popularCategories" 
          :key="cat.categoryID"
          type="button" 
          :class="['filter-pill', { active: filterCatId === cat.categoryID }]"
          @click="filterCatId = filterCatId === cat.categoryID ? null : cat.categoryID"
        >
          {{ cat.categoryName }} ({{ countByCategory(cat.categoryID) }})
        </button>
      </div>

      <!-- Danh Sách Thiết Bị Trong Kho Dạng Cuộn -->
      <div class="assets-scroll-list">
        <div v-if="filteredAssets.length === 0" class="empty-asset-state">
          <p v-if="assetsList.length === 0">⚠️ Hiện tại không có thiết bị nào ở trạng thái "Sẵn sàng" trong kho.</p>
          <p v-else>Không tìm thấy thiết bị nào phù hợp với từ khóa "<strong>{{ searchTerm }}</strong>".</p>
        </div>

        <div 
          v-for="a in filteredAssets" 
          :key="a.assetID" 
          class="asset-select-item"
          :class="{ active: modelValue === a.assetID }"
          @click="selectAsset(a)"
        >
          <div class="asset-item-icon">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
              <line x1="8" y1="21" x2="16" y2="21"></line>
              <line x1="12" y1="17" x2="12" y2="21"></line>
            </svg>
          </div>
          <div class="asset-item-info">
            <div class="asset-item-header">
              <span class="asset-item-code">{{ a.assetCode }}</span>
              <strong class="asset-item-name">{{ a.assetName }}</strong>
              <span class="asset-item-cat">{{ a.categoryName }}</span>
              <span v-if="a.brand" class="asset-item-brand">{{ a.brand }}</span>
            </div>
            <div v-if="a.specifications" class="asset-item-specs">
              ⚙️ {{ a.specifications }}
            </div>
            <div class="asset-item-sub">
              <span v-if="a.serialNumber">S/N: <strong class="mono-text">{{ a.serialNumber }}</strong> • </span>
              <span>📍 Vị trí: <strong>{{ a.warehouseLocation || 'Kho IT' }}</strong></span>
            </div>
          </div>
          <div class="asset-item-badge">
            <span class="ready-tag">Sẵn Sàng</span>
            <span class="btn-select-arrow">Chọn ➔</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: [Number, String, null],
    default: null
  },
  assets: {
    type: Array,
    default: () => []
  },
  categories: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const searchTerm = ref('')
const filterCatId = ref(null)
const quickFilter = ref('all')

const assetsList = computed(() => {
  return props.assets || []
})

const selectedAsset = computed(() => {
  if (!props.modelValue) return null
  return (props.assets || []).find(a => a.assetID === props.modelValue) || null
})

const popularCategories = computed(() => {
  if (!props.categories) return []
  return props.categories.filter(c => countByCategory(c.categoryID) > 0).slice(0, 4)
})

const countByCategory = (catId) => {
  return assetsList.value.filter(a => a.categoryID === catId).length
}

const filteredAssets = computed(() => {
  let list = assetsList.value

  // Lọc theo loại thiết bị
  if (filterCatId.value) {
    list = list.filter(a => a.categoryID === filterCatId.value)
  }

  // Lọc theo từ khóa tìm kiếm
  if (searchTerm.value.trim()) {
    const s = searchTerm.value.trim().toLowerCase()
    list = list.filter(a => 
      a.assetCode.toLowerCase().includes(s) ||
      a.assetName.toLowerCase().includes(s) ||
      (a.categoryName && a.categoryName.toLowerCase().includes(s)) ||
      (a.brand && a.brand.toLowerCase().includes(s)) ||
      (a.specifications && a.specifications.toLowerCase().includes(s)) ||
      (a.serialNumber && a.serialNumber.toLowerCase().includes(s)) ||
      (a.warehouseLocation && a.warehouseLocation.toLowerCase().includes(s))
    )
  }

  return list
})

const selectAsset = (asset) => {
  emit('update:modelValue', asset.assetID)
  emit('change', asset)
}

const clearSelection = () => {
  emit('update:modelValue', null)
  emit('change', null)
}
</script>

<style scoped>
.asset-selector-root {
  width: 100%;
}

/* 1. Selected Asset Card */
.selected-asset-card {
  background: #f0fdf4;
  border: 1.5px solid #86efac;
  border-radius: var(--radius-md);
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  animation: fadeIn 0.2s ease-out;
}

.selected-asset-main {
  display: flex;
  align-items: center;
  gap: 14px;
}

.selected-asset-icon {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  background: linear-gradient(135deg, #059669 0%, #10b981 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(5, 150, 105, 0.25);
  flex-shrink: 0;
}

.selected-asset-info {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.selected-asset-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.selected-asset-code {
  font-family: monospace;
  font-size: 0.775rem;
  font-weight: 700;
  color: #4f46e5;
  background: #eef2ff;
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid #c7d2fe;
}

.selected-asset-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
}

.selected-asset-cat {
  font-size: 0.75rem;
  color: #0284c7;
  background: #f0f9ff;
  padding: 2px 8px;
  border-radius: 9999px;
  font-weight: 600;
  border: 1px solid #bae6fd;
}

.selected-asset-brand {
  font-size: 0.725rem;
  color: var(--text-dim);
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 4px;
}

.selected-asset-specs {
  font-size: 0.8rem;
  color: #0284c7;
}

.selected-asset-meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.775rem;
  color: var(--text-dim);
  flex-wrap: wrap;
}

.mono-text {
  font-family: monospace;
  color: #1e293b;
}

.btn-change-asset {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #ffffff;
  border: 1px solid #cbd5e1;
  color: var(--text-main);
  padding: 6px 12px;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition);
  white-space: nowrap;
}

.btn-change-asset:hover {
  background: #f1f5f9;
  border-color: #059669;
  color: #059669;
}

/* 2. Picker Box */
.selector-picker-box {
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: #f8fafc;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.picker-search-bar {
  display: grid;
  grid-template-columns: 1fr 180px;
  gap: 8px;
}

.search-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 10px;
  color: var(--text-dim);
  pointer-events: none;
}

.picker-input {
  padding-left: 32px !important;
  padding-right: 28px !important;
  background: #ffffff !important;
  border-color: #cbd5e1 !important;
  color: #0f172a !important;
}

.btn-clear-search {
  position: absolute;
  right: 8px;
  background: transparent;
  border: none;
  color: var(--text-dim);
  cursor: pointer;
  font-size: 0.8rem;
}

.picker-cat-select {
  background: #ffffff !important;
  border-color: #cbd5e1 !important;
  color: #0f172a !important;
}

.quick-filter-pills {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.filter-pill {
  background: #ffffff;
  border: 1px solid var(--border-color);
  color: var(--text-dim);
  padding: 3px 10px;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition);
}

.filter-pill:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.filter-pill.active {
  background: #059669;
  border-color: #059669;
  color: #ffffff;
}

/* 3. Assets Scroll List */
.assets-scroll-list {
  max-height: 230px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-right: 4px;
}

.assets-scroll-list::-webkit-scrollbar {
  width: 5px;
}
.assets-scroll-list::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.asset-select-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 12px;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-sm);
  cursor: pointer;
  transition: all 0.15s ease;
}

.asset-select-item:hover {
  background: #f1f5f9;
  border-color: #86efac;
  transform: translateX(2px);
}

.asset-select-item.active {
  background: #ecfdf5;
  border-color: #059669;
}

.asset-item-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, #059669 0%, #10b981 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  flex-shrink: 0;
}

.asset-item-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.asset-item-header {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.asset-item-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: #0f172a;
}

.asset-item-code {
  font-family: monospace;
  font-size: 0.725rem;
  font-weight: 700;
  color: #4f46e5;
}

.asset-item-cat {
  font-size: 0.725rem;
  color: #0284c7;
  background: #f0f9ff;
  padding: 1px 6px;
  border-radius: 4px;
}

.asset-item-brand {
  font-size: 0.7rem;
  color: var(--text-dim);
}

.asset-item-specs {
  font-size: 0.75rem;
  color: #0284c7;
}

.asset-item-sub {
  font-size: 0.725rem;
  color: var(--text-dim);
}

.asset-item-badge {
  display: flex;
  align-items: center;
  gap: 8px;
}

.ready-tag {
  font-size: 0.7rem;
  font-weight: 600;
  color: #059669;
  background: #ecfdf5;
  padding: 2px 6px;
  border-radius: 4px;
}

.btn-select-arrow {
  font-size: 0.75rem;
  font-weight: 600;
  color: #059669;
  opacity: 0;
  transition: opacity 0.15s ease;
}

.asset-select-item:hover .btn-select-arrow {
  opacity: 1;
}

.empty-asset-state {
  padding: 20px;
  text-align: center;
  font-size: 0.8rem;
  color: var(--text-dim);
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-4px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
