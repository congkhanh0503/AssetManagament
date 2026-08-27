<template>
  <div class="glass-card table-card">
    <div v-if="loading" class="table-loading">
      <span class="loading-spinner"></span>
      <p>{{ $t('common.loading') }}</p>
    </div>

    <div v-else-if="displayAssetsList.length === 0" class="empty-state">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
        <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
        <line x1="8" y1="21" x2="16" y2="21"></line>
        <line x1="12" y1="17" x2="12" y2="21"></line>
      </svg>
      <p>{{ $t('assets.empty_list') }}</p>
    </div>

    <div v-else>
      <div class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 170px;">{{ $t('assets.table_host_name') }}</th>
              <th style="width: auto;">{{ $t('assets.table_specs') }}</th>
              <th style="width: 150px;">{{ $t('assets.table_serial_material') }}</th>
              <th style="width: 160px;">{{ $t('assets.table_status') }}</th>
              <th style="width: 210px;">{{ $t('assets.table_holder_location') }}</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="item in paginatedAssets" :key="item.isMouseGroup ? item.groupKey : (item.isKeyboardGroup ? item.groupKey : item.assetID)">
              <!-- 1. DÒNG NHÓM CHUỘT (GROUP ROW) -->
              <tr 
                v-if="item.isMouseGroup" 
                class="mouse-group-row clickable-row"
                @click="toggleMouseGroup(item.groupKey)"
              >
                <td>
                  <div class="mouse-group-code">
                    <button type="button" class="btn-toggle-group" :class="{ open: expandedMouseGroups.has(item.groupKey) }">
                      {{ expandedMouseGroups.has(item.groupKey) ? '▼' : '▶' }}
                    </button>
                    <span class="code-badge mouse-group-badge">
                      🖱️ {{ item.assetCode }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="asset-title-main" style="display: flex; align-items: center; gap: 8px;">
                    <span>{{ item.assetName }}</span>
                    <span class="mouse-total-badge">📦 {{ item.totalCount }} {{ $t('assets.unit_mouse') }}</span>
                  </div>
                  <div class="asset-specs-sub">
                    <span v-if="item.brand" class="brand-tag">{{ item.brand }}</span>
                    <span class="cat-tag">{{ item.categoryName || 'Mouse' }}</span>
                  </div>
                </td>
                <td>
                  <div class="sn-text" style="color: #64748b; font-size: 0.825rem;">{{ $t('assets.batch_entry') }}</div>
                  <div class="asset-sub">{{ $t('assets.no_sn_required') }}</div>
                </td>
                <td>
                  <div class="mouse-status-summary">
                    <span v-if="item.availableCount > 0" class="stat-pill-mini ok">
                      ✅ {{ $t('assets.stock_qty') }} {{ item.availableCount }}
                    </span>
                    <span v-if="item.inUseCount > 0" class="stat-pill-mini use">
                      👤 {{ $t('assets.in_use_qty') }} {{ item.inUseCount }}
                    </span>
                    <span v-if="item.brokenCount > 0" class="stat-pill-mini err">
                      ❌ {{ $t('assets.broken_qty') }} {{ item.brokenCount }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="location-badge-clean">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                      <circle cx="12" cy="10" r="3"></circle>
                    </svg>
                    <span>{{ item.warehouseLocation }}</span>
                  </div>
                </td>
              </tr>

              <!-- 2. DÒNG CON CHI TIẾT KHI MỞ RỘNG NHÓM CHUỘT -->
              <tr 
                v-else-if="item.isMouseSubItem" 
                class="mouse-sub-row clickable-row"
                @click="$emit('view-detail', item.assetID)"
              >
                <td style="padding-left: 28px;">
                  <span class="code-badge sub-code-badge">↳ {{ item.assetCode }}</span>
                </td>
                <td>
                  <div style="font-size: 0.85rem; font-weight: 600; color: #334155;">
                    {{ item.assetName }} <span style="color: #94a3b8; font-size: 0.75rem;">{{ $t('assets.sub_item_index', { index: item.subIndex }) }}</span>
                  </div>
                </td>
                <td>
                  <div class="sn-text">{{ item.serialNumber || $t('assets.batch_entry') }}</div>
                </td>
                <td>
                  <StatusBadge :status="item.status" type="asset" />
                </td>
                <td>
                  <div v-if="item.holderName" class="holder-cell">
                    <span class="holder-name">{{ item.holderName }}</span>
                    <span class="holder-code">({{ item.holderCode }})</span>
                    <div class="holder-dept-mini">{{ item.holderDepartment || '---' }}</div>
                  </div>
                  <div v-else class="location-badge-clean">
                    <span>{{ item.dynamicLocation }}</span>
                  </div>
                </td>
              </tr>

              <!-- 3. DÒNG NHÓM BÀN PHÍM (GROUP ROW) -->
              <tr 
                v-else-if="item.isKeyboardGroup" 
                class="mouse-group-row keyboard-group-row clickable-row"
                @click="toggleKeyboardGroup(item.groupKey)"
              >
                <td>
                  <div class="mouse-group-code">
                    <button type="button" class="btn-toggle-group" :class="{ open: expandedKeyboardGroups.has(item.groupKey) }">
                      {{ expandedKeyboardGroups.has(item.groupKey) ? '▼' : '▶' }}
                    </button>
                    <span class="code-badge keyboard-group-badge">
                      ⌨️ {{ item.assetCode }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="asset-title-main" style="display: flex; align-items: center; gap: 8px;">
                    <span>{{ item.assetName }}</span>
                    <span class="mouse-total-badge keyboard-total-badge">📦 {{ item.totalCount }} {{ $t('assets.unit_keyboard') }}</span>
                  </div>
                  <div class="asset-specs-sub">
                    <span v-if="item.brand" class="brand-tag">{{ item.brand }}</span>
                    <span class="cat-tag">{{ item.categoryName || 'Keyboard' }}</span>
                  </div>
                </td>
                <td>
                  <div class="sn-text" style="color: #64748b; font-size: 0.825rem;">{{ $t('assets.batch_entry') }}</div>
                  <div class="asset-sub">{{ $t('assets.no_sn_required') }}</div>
                </td>
                <td>
                  <div class="mouse-status-summary">
                    <span v-if="item.availableCount > 0" class="stat-pill-mini ok">
                      ✅ {{ $t('assets.stock_qty') }} {{ item.availableCount }}
                    </span>
                    <span v-if="item.inUseCount > 0" class="stat-pill-mini use">
                      👤 {{ $t('assets.in_use_qty') }} {{ item.inUseCount }}
                    </span>
                    <span v-if="item.brokenCount > 0" class="stat-pill-mini err">
                      ❌ {{ $t('assets.broken_qty') }} {{ item.brokenCount }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="location-badge-clean">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                      <circle cx="12" cy="10" r="3"></circle>
                    </svg>
                    <span>{{ item.warehouseLocation }}</span>
                  </div>
                </td>
              </tr>

              <!-- 4. DÒNG CON CHI TIẾT KHI MỞ RỘNG NHÓM BÀN PHÍM -->
              <tr 
                v-else-if="item.isKeyboardSubItem" 
                class="mouse-sub-row keyboard-sub-row clickable-row"
                @click="$emit('view-detail', item.assetID)"
              >
                <td style="padding-left: 28px;">
                  <span class="code-badge sub-code-badge" style="border-left: 2px solid #059669;">↳ {{ item.assetCode }}</span>
                </td>
                <td>
                  <div style="font-size: 0.85rem; font-weight: 600; color: #334155;">
                    {{ item.assetName }} <span style="color: #94a3b8; font-size: 0.75rem;">{{ $t('assets.sub_kb_index', { index: item.subIndex }) }}</span>
                  </div>
                </td>
                <td>
                  <div class="sn-text">{{ item.serialNumber || $t('assets.batch_entry') }}</div>
                </td>
                <td>
                  <StatusBadge :status="item.status" type="asset" />
                </td>
                <td>
                  <div v-if="item.holderName" class="holder-cell">
                    <span class="holder-name">{{ item.holderName }}</span>
                    <span class="holder-code">({{ item.holderCode }})</span>
                    <div class="holder-dept-mini">{{ item.holderDepartment || '---' }}</div>
                  </div>
                  <div v-else class="location-badge-clean">
                    <span>{{ item.dynamicLocation }}</span>
                  </div>
                </td>
              </tr>

              <!-- 5. CÁC THIẾT BỊ KHÁC (LAPTOP, PC, MÀN HÌNH...) -->
              <tr 
                v-else-if="!item.isMouseGroup && !item.isMouseSubItem && !item.isKeyboardGroup && !item.isKeyboardSubItem" 
                class="clickable-row" 
                @click="$emit('view-detail', item.assetID)" 
              >
                <td>
                  <span class="code-badge">{{ item.assetCode }}</span>
                </td>
                <td>
                  <div class="asset-title-main">{{ item.assetName }}</div>
                  <div class="asset-specs-sub">
                    <span v-if="item.brand" class="brand-tag">{{ item.brand }}</span>
                    <span v-if="item.categoryName" class="cat-tag">{{ item.categoryName }}</span>
                    <span v-if="item.specifications" class="spec-text-inline" :title="`Cấu hình: ${item.specifications}`">
                      ⚙️ {{ item.specifications }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="sn-text">{{ item.serialNumber || '---' }}</div>
                  <div v-if="item.materialCode" class="asset-sub">Mã VT: {{ item.materialCode }}</div>
                </td>
                <td>
                  <StatusBadge :status="item.status" type="asset" />
                </td>
                <td>
                  <div v-if="item.holderName" class="holder-cell">
                    <span class="holder-name">{{ item.holderName }}</span>
                    <span class="holder-code">({{ item.holderCode }})</span>
                    <div class="holder-dept-mini">{{ item.holderDepartment || '---' }}</div>
                  </div>
                  <div v-else class="location-badge-clean">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                      <circle cx="12" cy="10" r="3"></circle>
                    </svg>
                    <span>{{ item.dynamicLocation }}</span>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- Phân trang danh sách thiết bị -->
      <Pagination 
        v-model:currentPage="currentPage" 
        v-model:pageSize="pageSize" 
        :totalItems="displayAssetsList.length" 
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import StatusBadge from '@/components/common/StatusBadge.vue'
import Pagination from '@/components/common/Pagination.vue'

const props = defineProps({
  assets: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
})

defineEmits(['view-detail'])

const currentPage = ref(1)
const pageSize = ref(15)
const expandedMouseGroups = ref(new Set())
const expandedKeyboardGroups = ref(new Set())

const toggleMouseGroup = (groupKey) => {
  if (expandedMouseGroups.value.has(groupKey)) {
    expandedMouseGroups.value.delete(groupKey)
  } else {
    expandedMouseGroups.value.add(groupKey)
  }
}

const toggleKeyboardGroup = (groupKey) => {
  if (expandedKeyboardGroups.value.has(groupKey)) {
    expandedKeyboardGroups.value.delete(groupKey)
  } else {
    expandedKeyboardGroups.value.add(groupKey)
  }
}

const isMouseAsset = (a) => {
  if (!a) return false
  const catName = (a.categoryName || '').toLowerCase()
  const name = (a.assetName || '').toLowerCase()
  const code = (a.assetCode || '').toLowerCase()
  return catName.includes('chuột') || catName.includes('mouse') || 
         name.includes('chuột') || name.includes('mouse') ||
         code.includes('mou') || code.includes('mouse')
}

const isKeyboardAsset = (a) => {
  if (!a) return false
  const catName = (a.categoryName || '').toLowerCase()
  const name = (a.assetName || '').toLowerCase()
  const code = (a.assetCode || '').toLowerCase()
  return catName.includes('bàn phím') || catName.includes('ban phim') || catName.includes('keyboard') || catName.includes('phím') ||
         name.includes('bàn phím') || name.includes('ban phim') || name.includes('keyboard') ||
         code.includes('kb') || code.includes('key')
}

const displayAssetsList = computed(() => {
  const standaloneItems = []
  const mouseGroups = new Map()
  const keyboardGroups = new Map()

  // 1. Phân loại và gom nhóm Chuột & Bàn Phím theo Hãng
  for (const item of props.assets) {
    const itemUpdatedTime = item.updatedAt ? new Date(item.updatedAt).getTime() : 0
    if (isMouseAsset(item)) {
      const brandClean = (item.brand || 'Khác').trim().toLowerCase()
      const groupKey = brandClean
      if (!mouseGroups.has(groupKey)) {
        const brandCode = (item.brand || 'GEN').replace(/[^a-zA-Z0-9]/g, '').toUpperCase().slice(0, 5)
        mouseGroups.set(groupKey, {
          groupKey,
          isMouseGroup: true,
          assetID: item.assetID,
          assetCode: `MOU-${brandCode}`,
          assetName: item.brand ? `Chuột ${item.brand.trim()}` : (item.assetName || 'Chuột quang'),
          brand: item.brand || '',
          categoryName: item.categoryName || 'Chuột',
          warehouseLocation: item.warehouseLocation || 'Kho IT - Kệ Phụ Kiện',
          items: [],
          totalCount: 0,
          availableCount: 0,
          inUseCount: 0,
          brokenCount: 0,
          maintenanceCount: 0,
          latestUpdatedTime: itemUpdatedTime
        })
      }
      const group = mouseGroups.get(groupKey)
      group.items.push(item)
      group.totalCount++
      if (itemUpdatedTime > group.latestUpdatedTime) {
        group.latestUpdatedTime = itemUpdatedTime
      }
      if (item.status === 'Available') group.availableCount++
      else if (item.status === 'In-Use') group.inUseCount++
      else if (item.status === 'Broken') group.brokenCount++
      else if (item.status === 'Maintenance') group.maintenanceCount++
    } else if (isKeyboardAsset(item)) {
      const brandClean = (item.brand || 'Khác').trim().toLowerCase()
      const groupKey = 'kb__' + brandClean
      if (!keyboardGroups.has(groupKey)) {
        const brandCode = (item.brand || 'GEN').replace(/[^a-zA-Z0-9]/g, '').toUpperCase().slice(0, 5)
        keyboardGroups.set(groupKey, {
          groupKey,
          isKeyboardGroup: true,
          assetID: item.assetID,
          assetCode: `KB-${brandCode}`,
          assetName: item.brand ? `Bàn phím ${item.brand.trim()}` : (item.assetName || 'Bàn phím'),
          brand: item.brand || '',
          categoryName: item.categoryName || 'Bàn phím',
          warehouseLocation: item.warehouseLocation || 'Kho IT - Kệ Phụ Kiện',
          items: [],
          totalCount: 0,
          availableCount: 0,
          inUseCount: 0,
          brokenCount: 0,
          maintenanceCount: 0,
          latestUpdatedTime: itemUpdatedTime
        })
      }
      const group = keyboardGroups.get(groupKey)
      group.items.push(item)
      group.totalCount++
      if (itemUpdatedTime > group.latestUpdatedTime) {
        group.latestUpdatedTime = itemUpdatedTime
      }
      if (item.status === 'Available') group.availableCount++
      else if (item.status === 'In-Use') group.inUseCount++
      else if (item.status === 'Broken') group.brokenCount++
      else if (item.status === 'Maintenance') group.maintenanceCount++
    } else {
      standaloneItems.push({
        ...item,
        latestUpdatedTime: itemUpdatedTime
      })
    }
  }

  // 2. Định nghĩa trọng số ưu tiên trạng thái: Sẵn sàng (1) -> Đang cấp phát (2) -> Bảo trì (3) -> Bị hỏng (4)
  const getStatusPriority = (status) => {
    switch (status) {
      case 'Available': return 1
      case 'In-Use': return 2
      case 'Maintenance': return 3
      case 'Broken': return 4
      default: return 5
    }
  }

  const getGroupStatusPriority = (group) => {
    if (group.availableCount > 0) return 1
    if (group.inUseCount > 0) return 2
    if (group.maintenanceCount > 0) return 3
    if (group.brokenCount > 0) return 4
    return 5
  }

  // Gom các khối cấp 1 và sắp xếp:
  // 1. Nhóm thiết bị có số lượng (Chuột, Bàn phím lô) luôn lên ĐẦU BẢNG
  // 2. Sắp xếp theo Trạng thái (Sẵn sàng -> Đang cấp phát -> Bảo trì -> Hỏng)
  // 3. Số lượng lớn hơn lên trước -> Thời gian cập nhật mới nhất
  const topLevelEntries = [
    ...standaloneItems,
    ...Array.from(mouseGroups.values()),
    ...Array.from(keyboardGroups.values())
  ]

  topLevelEntries.sort((a, b) => {
    const isGroupA = (a.isMouseGroup || a.isKeyboardGroup) ? 1 : 0
    const isGroupB = (b.isMouseGroup || b.isKeyboardGroup) ? 1 : 0

    // 1. Ưu tiên các thiết bị có số lượng (lô phụ kiện) lên TRƯỚC
    if (isGroupA !== isGroupB) {
      return isGroupB - isGroupA
    }

    // 2. Sắp xếp theo Trạng thái (Sẵn sàng -> Đang cấp phát -> Bảo trì -> Hỏng)
    const priorityA = isGroupA ? getGroupStatusPriority(a) : getStatusPriority(a.status)
    const priorityB = isGroupB ? getGroupStatusPriority(b) : getStatusPriority(b.status)

    if (priorityA !== priorityB) {
      return priorityA - priorityB
    }

    // 3. Nếu cùng là nhóm lô: Nhóm có tổng số lượng lớn hơn đứng trước
    if (isGroupA && isGroupB) {
      if ((b.totalCount || 0) !== (a.totalCount || 0)) {
        return (b.totalCount || 0) - (a.totalCount || 0)
      }
    }

    // 4. Sắp xếp theo thời gian cập nhật mới nhất
    return (b.latestUpdatedTime || 0) - (a.latestUpdatedTime || 0)
  })

  // 3. Flatten danh sách và xen các dòng con nếu nhóm được mở rộng (expanded)
  const result = []
  for (const entry of topLevelEntries) {
    if (entry.isMouseGroup) {
      result.push(entry)
      if (expandedMouseGroups.value.has(entry.groupKey)) {
        // Sắp xếp các dòng con trong nhóm: Sẵn sàng -> Đang dùng -> Bảo trì -> Hỏng
        const sortedItems = [...entry.items].sort((a, b) => {
          const pA = getStatusPriority(a.status)
          const pB = getStatusPriority(b.status)
          if (pA !== pB) return pA - pB
          return (a.assetCode || '').localeCompare(b.assetCode || '')
        })

        sortedItems.forEach((subItem, sIdx) => {
          result.push({
            ...subItem,
            isMouseSubItem: true,
            subIndex: sIdx + 1,
            parentGroupKey: entry.groupKey
          })
        })
      }
    } else if (entry.isKeyboardGroup) {
      result.push(entry)
      if (expandedKeyboardGroups.value.has(entry.groupKey)) {
        const sortedItems = [...entry.items].sort((a, b) => {
          const pA = getStatusPriority(a.status)
          const pB = getStatusPriority(b.status)
          if (pA !== pB) return pA - pB
          return (a.assetCode || '').localeCompare(b.assetCode || '')
        })

        sortedItems.forEach((subItem, sIdx) => {
          result.push({
            ...subItem,
            isKeyboardSubItem: true,
            subIndex: sIdx + 1,
            parentGroupKey: entry.groupKey
          })
        })
      }
    } else {
      result.push(entry)
    }
  }

  return result
})

const paginatedAssets = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return displayAssetsList.value.slice(start, start + pageSize.value)
})
</script>

<style scoped>
.table-responsive {
  overflow-x: auto;
  width: 100%;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed; /* Cố định tuyệt đối độ rộng các cột, không bao giờ bị nhảy/biến dạng khi mở chi tiết */
}

.data-table th,
.data-table td {
  padding: 10px 14px;
  vertical-align: middle;
  border-bottom: 1px solid var(--border-color, #e2e8f0);
}

.data-table th {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  border-bottom: 2px solid #e2e8f0;
}

.clickable-row {
  cursor: pointer;
}

.code-badge {
  font-family: monospace;
  font-weight: 700;
  font-size: 0.85rem;
  color: #4f46e5;
  background: #eef2ff;
  padding: 3px 8px;
  border-radius: 6px;
  border: 1px solid #c7d2fe;
  cursor: pointer;
  transition: var(--transition);
  white-space: nowrap;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.code-badge:hover {
  background: #e0e7ff;
}

.asset-title-main {
  font-weight: 700;
  color: #0f172a;
  cursor: pointer;
  transition: var(--transition);
}

.asset-title-main:hover {
  color: var(--primary);
}

.asset-specs-sub {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  margin-top: 2px;
}

.brand-tag {
  background: #f1f5f9;
  color: #475569;
  border: 1px solid #e2e8f0;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 4px;
}

.cat-tag {
  background: #eff6ff;
  color: #2563eb;
  border: 1px solid #bfdbfe;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 4px;
}

.spec-text-inline {
  font-size: 0.75rem;
  color: #64748b;
  max-width: 320px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.asset-sub {
  font-size: 0.75rem;
  color: var(--text-dim);
  margin-top: 2px;
}

.sn-text {
  font-family: monospace;
  font-size: 0.8rem;
  color: var(--text-muted);
}

.holder-name {
  font-weight: 600;
  color: #0f172a;
}

.holder-code {
  font-size: 0.75rem;
  color: var(--text-dim);
  margin-left: 4px;
}

.holder-dept-mini {
  font-size: 0.75rem;
  color: var(--text-dim);
}

.location-badge-clean {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.8rem;
  color: #64748b;
}

/* Mouse Group Styling */
.mouse-group-row {
  background: #f8fafc;
  font-weight: 500;
  border-left: 3px solid #6366f1;
  cursor: pointer;
}

.mouse-group-row:hover {
  background: #f1f5f9;
}

.mouse-group-code {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
}

.btn-toggle-group {
  background: #eef2ff;
  border: 1px solid #c7d2fe;
  font-size: 0.7rem;
  color: #4f46e5;
  cursor: pointer;
  padding: 3px 6px;
  border-radius: 4px;
  line-height: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.btn-toggle-group:hover {
  background: #e0e7ff;
  border-color: #a5b4fc;
}

.mouse-group-badge {
  background: #e0e7ff !important;
  color: #4338ca !important;
  border-color: #a5b4fc !important;
  white-space: nowrap;
}

.mouse-total-badge {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #a7f3d0;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 1px 8px;
  border-radius: 999px;
  white-space: nowrap;
}

.keyboard-group-row {
  border-left: 3px solid #059669 !important;
}

.keyboard-group-badge {
  background: #ecfdf5 !important;
  color: #047857 !important;
  border-color: #a7f3d0 !important;
  white-space: nowrap;
}

.keyboard-total-badge {
  background: #f0fdf4;
  color: #15803d;
  border: 1px solid #bbf7d0;
  white-space: nowrap;
}

.keyboard-sub-row {
  border-left: 3px dashed #a7f3d0 !important;
}

.mouse-status-summary {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 4px;
  align-items: center;
}

.stat-pill-mini {
  display: inline-flex;
  align-items: center;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
  width: fit-content;
  white-space: nowrap;
}

.stat-pill-mini.ok {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #d1fae5;
}

.stat-pill-mini.use {
  background: #eff6ff;
  color: #2563eb;
  border: 1px solid #dbeafe;
}

.stat-pill-mini.err {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fee2e2;
}

.mouse-sub-row {
  background: #f8fafc;
  border-left: 3px dashed #cbd5e1;
}

.mouse-sub-row td {
  padding: 8px 14px !important;
  font-size: 0.825rem;
}

.mouse-sub-row:hover {
  background: #f1f5f9;
}

.mouse-group-row td {
  padding: 10px 14px !important;
}

.sub-code-badge {
  font-size: 0.75rem;
  padding: 2px 6px;
  background: #ffffff !important;
  color: #475569 !important;
  border-color: #cbd5e1 !important;
  white-space: nowrap;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);
}
</style>
