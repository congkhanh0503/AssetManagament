<template>
  <div class="employee-selector-root">
    <!-- 1. Trạng Thái ĐÃ CHỌN Nhân Viên -->
    <div v-if="selectedEmployee" class="selected-employee-card">
      <div class="selected-emp-main">
        <div class="selected-avatar">
          {{ getInitials(selectedEmployee.fullName) }}
        </div>
        <div class="selected-info">
          <div class="selected-name-row">
            <span class="selected-name">{{ selectedEmployee.fullName }}</span>
            <span class="selected-code-badge">{{ selectedEmployee.employeeCode }}</span>
            <span class="selected-dept-badge">{{ selectedEmployee.departmentName }}</span>
          </div>
          <div class="selected-meta-row">
            <span v-if="selectedEmployee.title" class="meta-item">💼 {{ selectedEmployee.title }}</span>
            <span v-if="selectedEmployee.email" class="meta-item">✉️ {{ selectedEmployee.email }}</span>
            <span v-if="selectedEmployee.phone" class="meta-item">📞 {{ selectedEmployee.phone }}</span>
            <span class="meta-item holding-count" :class="{ 'has-assets': selectedEmployee.holdingAssetCount > 0 }">
              📦 Đang giữ: <strong>{{ selectedEmployee.holdingAssetCount || 0 }} thiết bị</strong>
            </span>
          </div>
        </div>
      </div>
      <button type="button" class="btn-change-emp" @click="clearSelection" title="Chọn nhân viên khác">
        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
        </svg>
        <span>Đổi người nhận</span>
      </button>
    </div>

    <!-- 2. Trạng Thái CHƯA CHỌN: Hiển Thị Thanh Tìm Kiếm & Danh Sách Lọc -->
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
            placeholder="Gõ tên nhân viên, mã NV (EMP...), chức danh, email..."
            autofocus
          />
          <button v-if="searchTerm" type="button" class="btn-clear-search" @click="searchTerm = ''">✕</button>
        </div>

        <!-- Filter Phòng ban -->
        <div class="dept-filter-wrap">
          <select class="form-control form-control-sm picker-dept-select" v-model="filterDeptId">
            <option :value="null">-- Tất cả phòng ban --</option>
            <option v-for="dept in departments" :key="dept.departmentID" :value="dept.departmentID">
              {{ dept.departmentName }}
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
          Tất Cả ({{ employeesList.length }})
        </button>
        <button 
          type="button" 
          :class="['filter-pill', { active: quickFilter === 'no-asset' }]"
          @click="quickFilter = 'no-asset'"
        >
          ✨ Chưa Có Máy ({{ zeroAssetCount }})
        </button>
      </div>

      <!-- Danh Sách Nhân Viên Dạng Cuộn -->
      <div class="employees-scroll-list">
        <div v-if="filteredEmployees.length === 0" class="empty-emp-state">
          <p>Không tìm thấy nhân viên nào phù hợp với từ khóa "<strong>{{ searchTerm }}</strong>".</p>
        </div>

        <div 
          v-for="emp in filteredEmployees" 
          :key="emp.employeeID" 
          class="emp-select-item"
          :class="{ active: modelValue === emp.employeeID }"
          @click="selectEmployee(emp)"
        >
          <div class="emp-item-avatar">
            {{ getInitials(emp.fullName) }}
          </div>
          <div class="emp-item-info">
            <div class="emp-item-header">
              <span class="emp-item-name">{{ emp.fullName }}</span>
              <span class="emp-item-code">{{ emp.employeeCode }}</span>
              <span class="emp-item-dept">{{ emp.departmentName }}</span>
            </div>
            <div class="emp-item-sub">
              <span v-if="emp.title">{{ emp.title }} • </span>
              <span v-if="emp.email">{{ emp.email }}</span>
              <span v-else class="text-dim">Chưa cập nhật email</span>
            </div>
          </div>
          <div class="emp-item-badge">
            <span class="holding-tag" :class="{ 'tag-empty': (emp.holdingAssetCount || 0) === 0 }">
              {{ (emp.holdingAssetCount || 0) === 0 ? '✨ 0 máy' : `📦 ${emp.holdingAssetCount} máy` }}
            </span>
            <span class="btn-select-arrow">Chọn ➔</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: [Number, String, null],
    default: null
  },
  employees: {
    type: Array,
    default: () => []
  },
  departments: {
    type: Array,
    default: () => []
  },
  excludeEmployeeId: {
    type: [Number, String, null],
    default: null
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const searchTerm = ref('')
const filterDeptId = ref(null)
const quickFilter = ref('all')

const employeesList = computed(() => {
  if (!props.excludeEmployeeId) return props.employees || []
  return (props.employees || []).filter(e => e.employeeID !== props.excludeEmployeeId)
})

const zeroAssetCount = computed(() => {
  return employeesList.value.filter(e => (e.holdingAssetCount || 0) === 0).length
})

const selectedEmployee = computed(() => {
  if (!props.modelValue) return null
  return (props.employees || []).find(e => e.employeeID === props.modelValue) || null
})

const filteredEmployees = computed(() => {
  let list = employeesList.value

  // Lọc theo phòng ban
  if (filterDeptId.value) {
    list = list.filter(e => e.departmentID === filterDeptId.value)
  }

  // Quick Filter: Chưa có máy
  if (quickFilter.value === 'no-asset') {
    list = list.filter(e => (e.holdingAssetCount || 0) === 0)
  }

  // Lọc theo từ khóa tìm kiếm
  if (searchTerm.value.trim()) {
    const s = searchTerm.value.trim().toLowerCase()
    list = list.filter(e => 
      e.fullName.toLowerCase().includes(s) ||
      e.employeeCode.toLowerCase().includes(s) ||
      (e.departmentName && e.departmentName.toLowerCase().includes(s)) ||
      (e.title && e.title.toLowerCase().includes(s)) ||
      (e.email && e.email.toLowerCase().includes(s)) ||
      (e.phone && e.phone.includes(s))
    )
  }

  return list
})

const selectEmployee = (emp) => {
  emit('update:modelValue', emp.employeeID)
  emit('change', emp)
}

const clearSelection = () => {
  emit('update:modelValue', null)
  emit('change', null)
}

const getInitials = (name) => {
  if (!name) return 'NV'
  const parts = name.trim().split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}
</script>

<style scoped>
.employee-selector-root {
  width: 100%;
}

/* 1. Selected Employee Card */
.selected-employee-card {
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

.selected-emp-main {
  display: flex;
  align-items: center;
  gap: 14px;
}

.selected-avatar {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  background: linear-gradient(135deg, #059669 0%, #10b981 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.95rem;
  box-shadow: 0 4px 10px rgba(5, 150, 105, 0.25);
  flex-shrink: 0;
}

.selected-info {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.selected-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.selected-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
}

.selected-code-badge {
  font-family: monospace;
  font-size: 0.75rem;
  font-weight: 700;
  color: #4f46e5;
  background: #eef2ff;
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid #c7d2fe;
}

.selected-dept-badge {
  font-size: 0.75rem;
  color: #0284c7;
  background: #f0f9ff;
  padding: 2px 8px;
  border-radius: 9999px;
  font-weight: 600;
  border: 1px solid #bae6fd;
}

.selected-meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.775rem;
  color: var(--text-dim);
  flex-wrap: wrap;
}

.holding-count {
  color: #64748b;
}

.holding-count.has-assets strong {
  color: #0284c7;
}

.btn-change-emp {
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

.btn-change-emp:hover {
  background: #f1f5f9;
  border-color: var(--primary);
  color: var(--primary);
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

.picker-dept-select {
  background: #ffffff !important;
  border-color: #cbd5e1 !important;
  color: #0f172a !important;
}

.quick-filter-pills {
  display: flex;
  align-items: center;
  gap: 8px;
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
  background: var(--primary);
  border-color: var(--primary);
  color: #ffffff;
}

/* 3. Employees Scroll List */
.employees-scroll-list {
  max-height: 220px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-right: 4px;
}

.employees-scroll-list::-webkit-scrollbar {
  width: 5px;
}
.employees-scroll-list::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.emp-select-item {
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

.emp-select-item:hover {
  background: #f1f5f9;
  border-color: #93c5fd;
  transform: translateX(2px);
}

.emp-select-item.active {
  background: #eef2ff;
  border-color: var(--primary);
}

.emp-item-avatar {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, #64748b 0%, #475569 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.75rem;
  flex-shrink: 0;
}

.emp-item-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.emp-item-header {
  display: flex;
  align-items: center;
  gap: 6px;
}

.emp-item-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: #0f172a;
}

.emp-item-code {
  font-family: monospace;
  font-size: 0.7rem;
  color: #64748b;
}

.emp-item-dept {
  font-size: 0.725rem;
  color: #4f46e5;
  background: #eef2ff;
  padding: 1px 6px;
  border-radius: 4px;
}

.emp-item-sub {
  font-size: 0.725rem;
  color: var(--text-dim);
}

.emp-item-badge {
  display: flex;
  align-items: center;
  gap: 8px;
}

.holding-tag {
  font-size: 0.7rem;
  font-weight: 600;
  color: #0284c7;
  background: #f0f9ff;
  padding: 2px 6px;
  border-radius: 4px;
}

.holding-tag.tag-empty {
  color: #059669;
  background: #ecfdf5;
}

.btn-select-arrow {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--primary);
  opacity: 0;
  transition: opacity 0.15s ease;
}

.emp-select-item:hover .btn-select-arrow {
  opacity: 1;
}

.empty-emp-state {
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
