<template>
  <div class="glass-card filter-card">
    <div class="filter-grid">
      <div class="form-group">
        <label class="form-label">{{ $t('assets.filter_search') }}</label>
        <input 
          type="text" 
          class="form-control" 
          :value="filters.search"
          @input="$emit('update:search', $event.target.value)"
          :placeholder="$t('assets.filter_search_placeholder')" 
        />
      </div>

      <div class="form-group">
        <label class="form-label">{{ $t('assets.filter_category') }}</label>
        <select 
          class="form-control" 
          :value="filters.categoryID"
          @change="$emit('update:categoryID', $event.target.value)"
        >
          <option value="">{{ $t('assets.filter_all_categories') }}</option>
          <option v-for="c in categories" :key="c.categoryID" :value="c.categoryID">
            {{ c.categoryName }}
          </option>
        </select>
      </div>

      <div class="form-group">
        <label class="form-label">{{ $t('assets.filter_status') }}</label>
        <select 
          class="form-control" 
          :value="filters.status"
          @change="$emit('update:status', $event.target.value)"
        >
          <option value="">{{ $t('assets.filter_all_statuses') }}</option>
          <option value="Available">{{ $t('status.available') }}</option>
          <option value="In-Use">{{ $t('status.in_use') }}</option>
          <option value="Maintenance">{{ $t('status.maintenance') }}</option>
          <option value="Broken">{{ $t('status.broken') }}</option>
        </select>
      </div>

      <div class="form-group">
        <label class="form-label">{{ $t('assets.filter_department') }}</label>
        <select 
          class="form-control" 
          :value="filters.departmentID"
          @change="$emit('update:departmentID', $event.target.value)"
        >
          <option value="">{{ $t('assets.filter_all_departments') }}</option>
          <option v-for="d in departments" :key="d.departmentID" :value="d.departmentID">
            {{ d.departmentName }}
          </option>
        </select>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  filters: {
    type: Object,
    required: true
  },
  categories: {
    type: Array,
    default: () => []
  },
  departments: {
    type: Array,
    default: () => []
  }
})

defineEmits(['update:search', 'update:categoryID', 'update:status', 'update:departmentID'])
</script>

<style scoped>
.filter-card {
  margin-bottom: 24px;
  padding: 16px 20px;
}

.filter-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}
</style>
