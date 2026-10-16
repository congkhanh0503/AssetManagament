<template>
  <div class="orders-page">
    <!-- Header Phân Hệ -->
    <div class="page-header-flex">
      <div>
        <div class="page-title-row">
          <div class="page-icon-wrapper">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
              <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
              <line x1="12" y1="22.08" x2="12" y2="12"></line>
            </svg>
          </div>
          <div>
            <h1 class="page-title">{{ $t('orders.title') }}</h1>
            <p class="page-subtitle">{{ $t('orders.subtitle') }}</p>
          </div>
        </div>
      </div>
      <div class="header-actions">
        <button type="button" class="btn btn-secondary" @click="fetchOrders">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="23 4 23 10 17 10"></polyline>
            <polyline points="1 20 1 14 7 14"></polyline>
            <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
          </svg>
          <span>{{ $t('orders.refresh') }}</span>
        </button>
        <button type="button" class="btn btn-primary btn-create-order" @click="openCreateModal">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19"></line>
            <line x1="5" y1="12" x2="19" y2="12"></line>
          </svg>
          <span>{{ $t('orders.btn_create_order') }}</span>
        </button>
      </div>
    </div>

    <!-- Thống Kê KPI Cards -->
    <div class="kpi-grid">
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box blue">📦</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_total') }}</div>
          <div class="kpi-val">{{ orders.length }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box amber">⏳</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_pending') }}</div>
          <div class="kpi-val">{{ kpiStats.pending }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box indigo">🚚</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_receiving') }}</div>
          <div class="kpi-val">{{ kpiStats.receiving }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card" :class="{ 'warning-highlight': kpiStats.shortage > 0 }">
        <div class="kpi-icon-box red">⚠️</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_shortage') }}</div>
          <div class="kpi-val" style="color: #dc2626;">{{ kpiStats.shortage }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box green">✅</div>
        <div>
          <div class="kpi-label">{{ $t('orders.status_completed') }}</div>
          <div class="kpi-val" style="color: #059669;">{{ kpiStats.completed }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card highlight-stock">
        <div class="kpi-icon-box emerald">🏢</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_warehouse') }}</div>
          <div class="kpi-val" style="color: #059669;">{{ kpiStats.totalInWarehouse }} <span class="kpi-unit">{{ $t('orders.unit_device') }}</span></div>
        </div>
      </div>
      <div class="glass-card kpi-card highlight-inuse">
        <div class="kpi-icon-box purple">👤</div>
        <div>
          <div class="kpi-label">{{ $t('orders.kpi_in_use') }}</div>
          <div class="kpi-val" style="color: #2563eb;">{{ kpiStats.totalInUse }} <span class="kpi-unit">{{ $t('orders.unit_device') }}</span></div>
        </div>
      </div>
    </div>

    <!-- Bộ Lọc & Tìm Kiếm -->
    <div class="glass-card filter-card">
      <div class="search-box">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"></circle>
          <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
        </svg>
        <input 
          type="text" 
          v-model="searchKeyword" 
          :placeholder="$t('orders.search_ph')" 
        />
        <button v-if="searchKeyword" class="btn-clear" @click="searchKeyword = ''">✕</button>
      </div>

      <div class="status-pills">
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'ALL' }"
          @click="currentStatusFilter = 'ALL'"
        >
          {{ $t('orders.filter_all', { count: orders.length }) }}
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Pending' }"
          @click="currentStatusFilter = 'Pending'"
        >
          {{ $t('orders.status_pending') }}
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Receiving' }"
          @click="currentStatusFilter = 'Receiving'"
        >
          {{ $t('orders.status_receiving') }}
        </button>
        <button 
          type="button" 
          class="pill-btn warn" 
          :class="{ active: currentStatusFilter === 'Shortage' }"
          @click="currentStatusFilter = 'Shortage'"
        >
          ⚠️ {{ $t('orders.filter_shortage') }}
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Completed' }"
          @click="currentStatusFilter = 'Completed'"
        >
          {{ $t('orders.filter_completed') }}
        </button>
      </div>
    </div>

    <!-- Bảng Danh Sách Đơn Hàng -->
    <div class="glass-card table-wrapper">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <span>{{ $t('orders.loading') }}</span>
      </div>

      <div v-else-if="filteredOrders.length === 0" class="empty-state">
        <div class="empty-icon">📦</div>
        <h3>{{ $t('orders.empty_title') }}</h3>
        <p>{{ $t('orders.empty_hint') }}</p>
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 130px;">{{ $t('orders.col_order_code') }}</th>
              <th style="width: 260px;">{{ $t('orders.col_pr_code') }}</th>
              <th style="width: 170px;">{{ $t('orders.col_project') }}</th>
              <th style="width: 130px;">{{ $t('orders.col_created') }}</th>
              <th style="width: 190px;">{{ $t('orders.col_progress') }}</th>
              <th style="width: 165px;">{{ $t('orders.col_stock_inuse') }}</th>
              <th style="width: 135px;">{{ $t('orders.col_status') }}</th>
              <th style="width: 140px; text-align: center;">{{ $t('orders.col_actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in filteredOrders" :key="order.orderID" class="order-row">
              <td>
                <span class="code-badge order-badge">{{ order.orderCode }}</span>
              </td>
              <td>
                <div class="order-title-cell">
                  <div class="order-name-row">
                    <strong class="order-name-text">
                      {{ order.prCode || order.orderName || $t('orders.no_pr') }}
                    </strong>
                    <span v-if="order.prCode && order.orderName && order.orderName !== order.prCode" class="pr-badge" :title="$t('orders.pr_code_prefix', { code: order.prCode })">
                      PR: {{ order.prCode }}
                    </span>
                  </div>
                  <div class="order-sub-meta">
                    <span v-if="order.departmentName" class="sup-tag" :title="$t('orders.title_dept_only', { dept: order.departmentName })">🏬 {{ $t('orders.ordered_by', { dept: order.departmentName }) }}</span>
                    <span v-if="order.supplierName" class="sup-tag">🏢 {{ order.supplierName }}</span>
                    <span class="type-tag">📦 {{ $t('orders.item_types_count', { count: order.itemTypesCount }) }}</span>
                  </div>
                  <div v-if="order.note" class="order-note-text" :title="order.note">📝 {{ order.note }}</div>
                </div>
              </td>
              <td>
                <div v-if="order.isProjectBased && order.projectName" class="project-pill">
                  <span class="proj-icon">🎯</span>
                  <span class="proj-name" :title="order.projectName">{{ order.projectName }}</span>
                </div>
                <span v-else class="text-muted" style="font-size: 0.8rem;">---</span>
              </td>
              <td>
                <div style="font-size: 0.825rem; font-weight: 500; color: #334155;">
                  {{ formatDate(order.orderDate) }}
                </div>
              </td>
              <td>
                <div class="progress-cell">
                  <div class="progress-bar-wrap">
                    <div 
                      class="progress-bar-fill" 
                      :style="{ width: getProgressPercent(order) + '%' }"
                      :class="{ 
                        'is-complete': order.totalReceivedQuantity >= order.totalExpectedQuantity,
                        'is-shortage': order.hasWarning
                      }"
                    ></div>
                  </div>
                  <div class="progress-text-row">
                    <span><strong>{{ order.totalReceivedQuantity }}</strong> / {{ order.totalExpectedQuantity }} {{ $t('orders.unit_device') }}</span>
                    <span class="percent-text">{{ getProgressPercent(order) }}%</span>
                  </div>
                  <div v-if="order.hasWarning" class="shortage-warning-badge">
                    ⚠️ {{ $t('orders.shortage_in_transit', { count: order.missingQuantity }) }}
                  </div>
                </div>
              </td>
              <td>
                <div class="allocation-badges">
                  <div class="alloc-badge stock" :title="$t('orders.alloc_stock_title')">
                    <span class="alloc-dot green"></span>
                    <span class="alloc-txt">{{ $t('orders.alloc_stock_label') }}</span>
                    <strong class="alloc-num">{{ order.inWarehouseCount || 0 }}</strong>
                  </div>
                  <div class="alloc-badge inuse" :title="$t('orders.alloc_inuse_title')">
                    <span class="alloc-dot blue"></span>
                    <span class="alloc-txt">{{ $t('orders.alloc_inuse_label') }}</span>
                    <strong class="alloc-num">{{ order.inUseCount || 0 }}</strong>
                  </div>
                </div>
              </td>
              <td>
                <span :class="getStatusBadgeClass(order.status)">
                  {{ getStatusText(order.status) }}
                </span>
              </td>
              <td style="text-align: center;">
                <div class="action-dropdown-wrapper">
                  <button 
                    type="button" 
                    class="btn-action-dropdown-toggle" 
                    :class="{ active: activeDropdownOrderId === order.orderID }"
                    @click="toggleActionDropdown(order.orderID, $event)"
                    :title="$t('orders.action_menu_title')"
                  >
                    <span class="btn-action-icon">⚙️</span>
                    <span class="btn-action-text">{{ $t('orders.action') }}</span>
                    <span class="dropdown-chevron">▾</span>
                  </button>

                  <!-- Menu Dropdown chứa trọn vẹn 4 thao tác CRUD -->
                  <div 
                    v-if="activeDropdownOrderId === order.orderID" 
                    class="action-dropdown-menu"
                    @click.stop
                  >
                    <!-- 1. Xem chi tiết / Quét nhận thiết bị (R - Read) -->
                    <button 
                      type="button" 
                      class="dropdown-item item-read" 
                      @click="handleActionRead(order)"
                    >
                      <span class="item-icon">🔍</span>
                      <div class="item-text-wrap">
                        <span class="item-title">{{ $t('orders.menu_view') }}</span>
                        <span class="item-desc">{{ $t('orders.menu_view_desc') }}</span>
                      </div>
                    </button>

                    <!-- 2. Sửa thông tin đơn hàng (U - Update) -->
                    <button 
                      type="button" 
                      class="dropdown-item item-update" 
                      @click="handleActionUpdate(order)"
                    >
                      <span class="item-icon">✏️</span>
                      <div class="item-text-wrap">
                        <span class="item-title">{{ $t('orders.edit_title') }}</span>
                        <span class="item-desc">{{ $t('orders.menu_edit_desc') }}</span>
                      </div>
                    </button>

                    <!-- 3. Thêm loại thiết bị cho đơn hàng này -->
                    <button 
                      type="button" 
                      class="dropdown-item item-create" 
                      @click="handleActionAddItem(order)"
                    >
                      <span class="item-icon">➕</span>
                      <div class="item-text-wrap">
                        <span class="item-title">{{ $t('orders.menu_add_item') }}</span>
                        <span class="item-desc">{{ $t('orders.menu_add_item_desc') }}</span>
                      </div>
                    </button>

                    <div class="dropdown-divider"></div>

                    <!-- 4. Xóa đơn hàng này (D - Delete) -->
                    <button 
                      type="button" 
                      class="dropdown-item item-delete" 
                      @click="handleActionDelete(order)"
                    >
                      <span class="item-icon">🗑️</span>
                      <div class="item-text-wrap">
                        <span class="item-title text-danger">{{ $t('orders.menu_delete') }}</span>
                        <span class="item-desc">{{ $t('orders.menu_delete_desc') }}</span>
                      </div>
                    </button>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ======================================================== -->
    <!-- MODAL BƯỚC 1: TẠO ĐƠN HÀNG MỚI (Tên, Mã, Dự án, Danh sách loại) -->
    <!-- ======================================================== -->
    <div v-if="isCreateModalOpen" class="modal-backdrop" @click.self="isCreateModalOpen = false">
      <div class="glass-modal modal-large">
        <div class="modal-header">
          <div class="modal-title-box">
            <div class="modal-icon blue">📦</div>
            <div>
              <h2 class="modal-title">{{ $t('orders.create_title') }}</h2>
              <p class="modal-subtitle">{{ $t('orders.create_subtitle') }}</p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isCreateModalOpen = false">✕</button>
        </div>

        <form @submit.prevent="submitCreateOrder" class="modal-body">
          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.pr_code_label') }} <span class="badge-optional">{{ $t('orders.optional_blank') }}</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.prCode" 
                :placeholder="$t('orders.pr_ph')" 
              />
            </div>
            <div class="form-group">
              <label class="form-label">{{ $t('orders.ordering_dept') }} <span class="badge-optional">{{ $t('orders.optional') }}</span></label>
              <select class="form-control" v-model="createForm.departmentID">
                <option :value="null">{{ $t('orders.shared_stock') }}</option>
                <option v-for="d in departmentsList" :key="d.departmentID" :value="d.departmentID">{{ d.departmentName }}</option>
              </select>
              <span class="field-hint-dept">{{ $t('orders.dept_hint') }}</span>
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.po_code_label') }}</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.orderCode" 
                :placeholder="$t('orders.po_ph')" 
              />
            </div>
          </div>

          <div class="form-group" style="margin-top: 10px;">
            <label class="form-label">{{ $t('orders.supplier') }}</label>
            <select class="form-control" v-model="createForm.supplierID" @change="onSupplierChange">
              <option :value="null">{{ $t('orders.supplier_ph') }}</option>
              <option v-for="sup in suppliersList" :key="sup.supplierID" :value="sup.supplierID">
                {{ sup.supplierName }}
              </option>
            </select>
          </div>

          <!-- Có thuộc dự án nào không? -->
          <div class="project-box">
            <label class="checkbox-label">
              <input type="checkbox" v-model="createForm.isProjectBased" />
              <span class="checkbox-text">{{ $t('orders.is_project') }}</span>
            </label>
            <div v-if="createForm.isProjectBased" class="project-input-wrap">
              <label class="form-label">{{ $t('orders.project_name') }} <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.projectName" 
                required 
                :placeholder="$t('orders.project_ph')" 
              />
            </div>
          </div>

          <div class="form-group" style="margin-top: 10px;">
            <label class="form-label">{{ $t('orders.order_note') }}</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="createForm.note" 
              :placeholder="$t('orders.note_ph')" 
            />
          </div>

          <!-- DANH SÁCH LOẠI THIẾT BỊ TRONG ĐƠN -->
          <div class="items-section">
            <div class="section-header-row">
              <div>
                <h3 class="section-title">{{ $t('orders.items_title') }}</h3>
                <p class="section-subtitle">{{ $t('orders.items_subtitle') }}</p>
              </div>
              <button type="button" class="btn btn-outline-primary btn-sm" @click="addCreateItemRow">
                {{ $t('orders.btn_add_item_plus') }}
              </button>
            </div>

            <div class="item-rows-container">
              <div v-for="(it, idx) in createForm.items" :key="idx" class="item-input-card">
                <div class="item-card-header">
                  <span class="item-idx-badge">#{{ idx + 1 }}</span>
                  <button 
                    v-if="createForm.items.length > 1" 
                    type="button" 
                    class="btn-remove-row" 
                    @click="removeCreateItemRow(idx)"
                    :title="$t('orders.remove_type_title')"
                  >
                    ✕ {{ $t('orders.delete') }}
                  </button>
                </div>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label class="form-label">{{ $t('orders.category') }} <span class="req">*</span></label>
                    <select class="form-control" v-model="it.categoryID" required>
                      <option :value="null">{{ $t('orders.category_ph') }}</option>
                      <option v-for="cat in categoriesList" :key="cat.categoryID" :value="cat.categoryID">
                        {{ cat.categoryName }}
                      </option>
                    </select>
                  </div>

                  <div class="form-group">
                    <label class="form-label">{{ $t('orders.model_name') }} <span class="req">*</span></label>
                    <input 
                      type="text" 
                      class="form-control" 
                      v-model="it.modelName" 
                      required 
                      :placeholder="$t('orders.model_ph')" 
                    />
                  </div>

                  <div class="form-group">
                    <label class="form-label">{{ $t('orders.manufacturer') }}</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      v-model="it.brand" 
                      placeholder="HP, Dell, Lenovo, Logitech..." 
                    />
                  </div>
                </div>

                <div class="form-grid-3" style="margin-top: 10px;">
                  <div class="form-group" style="grid-column: span 2;">
                    <label class="form-label">{{ $t('orders.spec_detail') }}</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      v-model="it.specifications" 
                      :placeholder="$t('orders.spec_ph')" 
                    />
                  </div>

                  <div class="form-group">
                    <label class="form-label">{{ $t('orders.qty_ordered') }} <span class="req">*</span></label>
                    <input 
                      type="number" 
                      class="form-control" 
                      v-model.number="it.expectedQuantity" 
                      min="1" 
                      required 
                      style="font-weight: 700; color: #2563eb;"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="isCreateModalOpen = false">{{ $t('orders.cancel') }}</button>
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              <span v-if="submitting" class="spinner-mini"></span>
              <span>{{ $t('orders.save_and_receive') }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- ======================================================== -->
    <!-- MODAL CHỈNH SỬA ĐƠN HÀNG (CẬP NHẬT THÔNG TIN ĐƠN HÀNG)   -->
    <!-- ======================================================== -->
    <div v-if="isEditModalOpen" class="modal-backdrop" @click.self="isEditModalOpen = false">
      <div class="glass-modal modal-large">
        <div class="modal-header">
          <div class="modal-title-box">
            <div class="modal-icon orange">✏️</div>
            <div>
              <h2 class="modal-title">{{ $t('orders.edit_title') }}</h2>
              <p class="modal-subtitle">{{ $t('orders.edit_subtitle') }}</p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isEditModalOpen = false">✕</button>
        </div>

        <form @submit.prevent="submitUpdateOrder" class="modal-body">
          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.pr_code_label') }} <span class="badge-optional">{{ $t('orders.optional_blank') }}</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.prCode" 
                :placeholder="$t('orders.pr_ph')" 
              />
            </div>
            <div class="form-group">
              <label class="form-label">{{ $t('orders.ordering_dept') }} <span class="badge-optional">{{ $t('orders.optional') }}</span></label>
              <select class="form-control" v-model="editForm.departmentID">
                <option :value="null">{{ $t('orders.shared_stock') }}</option>
                <option v-for="d in departmentsList" :key="d.departmentID" :value="d.departmentID">{{ d.departmentName }}</option>
              </select>
              <span class="field-hint-dept">{{ $t('orders.dept_hint') }}</span>
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.po_code_label') }}</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.orderCode" 
                disabled 
                style="background: #f1f5f9; cursor: not-allowed; font-weight: 700;"
                :title="$t('orders.order_code_locked')" 
              />
            </div>
          </div>

          <div class="form-grid-2" style="margin-top: 10px;">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.order_status') }}</label>
              <select class="form-control" v-model="editForm.status">
                <option value="Pending">🕒 {{ $t('orders.opt_pending') }}</option>
                <option value="Receiving">📥 {{ $t('orders.opt_receiving') }}</option>
                <option value="Partial">⚠️ {{ $t('orders.opt_partial') }}</option>
                <option value="Completed">✅ {{ $t('orders.opt_completed') }}</option>
                <option value="Cancelled">✕ {{ $t('orders.opt_cancelled') }}</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.supplier') }}</label>
              <select class="form-control" v-model="editForm.supplierID" @change="onEditSupplierChange">
                <option :value="null">{{ $t('orders.supplier_ph') }}</option>
                <option v-for="sup in suppliersList" :key="sup.supplierID" :value="sup.supplierID">
                  {{ sup.supplierName }}
                </option>
              </select>
            </div>
          </div>

          <!-- Thuộc dự án nào không? -->
          <div class="project-box" style="margin-top: 10px;">
            <label class="checkbox-label">
              <input type="checkbox" v-model="editForm.isProjectBased" />
              <span class="checkbox-text">{{ $t('orders.is_project') }}</span>
            </label>
            <div v-if="editForm.isProjectBased" class="project-input-wrap">
              <label class="form-label">{{ $t('orders.project_name') }} <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.projectName" 
                required 
                :placeholder="$t('orders.project_ph')" 
              />
            </div>
          </div>

          <div class="form-grid-2" style="margin-top: 10px;">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.expected_delivery') }}</label>
              <DateTextInput v-model="editForm.expectedDeliveryDate" />
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.order_note') }}</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.note" 
                :placeholder="$t('orders.note_ph')" 
              />
            </div>
          </div>

          <div class="modal-footer" style="margin-top: 20px;">
            <button type="button" class="btn btn-secondary" @click="isEditModalOpen = false">{{ $t('orders.cancel') }}</button>
            <button type="submit" class="btn btn-primary" :disabled="editSubmitting">
              <span v-if="editSubmitting" class="spinner-mini"></span>
              <span>💾 {{ $t('orders.save_changes') }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- ======================================================== -->
    <!-- MODAL BƯỚC 2: QUẢN LÝ & NHẬP THIẾT BỊ VÀO ĐƠN            -->
    <!-- (Đối soát số lượng, Cảnh báo thiếu, Quét Serial, Đồng bộ) -->
    <!-- ======================================================== -->
    <div v-if="isDetailModalOpen && currentOrder" class="modal-backdrop" @click.self="isDetailModalOpen = false">
      <div class="glass-modal modal-xl">
        <div class="modal-header">
          <div class="modal-title-box">
            <div class="modal-icon indigo">🔍</div>
            <div>
              <div class="title-code-row">
                <span class="code-badge order-badge">{{ currentOrder.orderCode }}</span>
                <h2 class="modal-title">{{ currentOrder.prCode ? $t('orders.pr_code_prefix', { code: currentOrder.prCode }) : (currentOrder.orderName || currentOrder.orderCode) }}</h2>
                <span :class="getStatusBadgeClass(currentOrder.status)">
                  {{ getStatusText(currentOrder.status) }}
                </span>
                <div class="modal-allocation-tags">
                  <span class="modal-alloc-tag stock" :title="$t('orders.modal_stock_title')">
                    🏢 {{ $t('orders.alloc_stock_label') }} <strong>{{ currentOrder.inWarehouseCount || 0 }}</strong>
                  </span>
                  <span class="modal-alloc-tag inuse" :title="$t('orders.modal_inuse_title')">
                    👤 {{ $t('orders.alloc_inuse_label') }} <strong>{{ currentOrder.inUseCount || 0 }}</strong>
                  </span>
                </div>
              </div>
              <p class="modal-subtitle">
                <span v-if="currentOrder.isProjectBased && currentOrder.projectName" class="proj-highlight">
                  🎯 {{ $t('orders.project_prefix') }} {{ currentOrder.projectName }}
                </span>
                <span v-if="currentOrder.supplierName"> | 🏢 {{ $t('orders.supplier_prefix') }} {{ currentOrder.supplierName }}</span>
                | 📅 {{ $t('orders.created_prefix') }} {{ formatDate(currentOrder.orderDate) }}
              </p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isDetailModalOpen = false">✕</button>
        </div>

        <div class="modal-body-scroll">
          <!-- BANNER CẢNH BÁO VẬN CHUYỂN THIẾU NẾU CÓ -->
          <div v-if="currentOrder.hasWarning" class="alert-banner-warning">
            <div class="alert-icon">⚠️</div>
            <div class="alert-content">
              <strong>{{ $t('orders.banner_title') }}</strong>
              <div v-html="$t('orders.banner_body', { expected: currentOrder.totalExpectedQuantity, received: currentOrder.totalReceivedQuantity, missing: currentOrder.missingQuantity })">
              </div>
              <div class="alert-sub" v-html="$t('orders.banner_sub', { received: currentOrder.totalReceivedQuantity })">
              </div>
            </div>
          </div>

          <!-- BẢNG ĐỐI SOÁT CÁC LOẠI THIẾT BỊ TRONG ĐƠN -->
          <div class="reconcile-card">
            <div class="reconcile-header">
              <h3>{{ $t('orders.recon_title') }}</h3>
              <div class="reconcile-stat">
                {{ $t('orders.received_label') }} <strong style="color: #2563eb;">{{ currentOrder.totalReceivedQuantity }}</strong> / {{ currentOrder.totalExpectedQuantity }} {{ $t('orders.unit_device') }}
                <span class="reconcile-sub-stat">
                  (🏢 {{ $t('orders.alloc_stock_label') }} <strong>{{ currentOrder.inWarehouseCount || 0 }}</strong> | 👤 {{ $t('orders.alloc_inuse_label') }} <strong>{{ currentOrder.inUseCount || 0 }}</strong>)
                </span>
              </div>
            </div>

            <table class="sub-table">
              <thead>
                <tr>
                  <th>{{ $t('orders.col_device_type') }}</th>
                  <th>Model</th>
                  <th>{{ $t('orders.col_spec') }}</th>
                  <th style="width: 80px; text-align: center;">{{ $t('orders.col_qty_ordered') }}</th>
                  <th style="width: 80px; text-align: center;">{{ $t('orders.col_received') }}</th>
                  <th style="width: 90px; text-align: center;">{{ $t('orders.col_in_stock') }}</th>
                  <th style="width: 90px; text-align: center;">{{ $t('orders.col_assigned') }}</th>
                  <th style="width: 120px; text-align: center;">{{ $t('orders.col_condition') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="it in currentOrder.items" :key="it.orderItemID" :class="{ 'row-shortage': it.isShortage }">
                  <td><span class="cat-pill">{{ it.categoryName || t('orders.default_category') }}</span></td>
                  <td><strong>{{ it.modelName }}</strong></td>
                  <td class="spec-cell-mini" :title="it.specifications">{{ it.specifications || '---' }}</td>
                  <td style="text-align: center; font-weight: 600;">{{ it.expectedQuantity }}</td>
                  <td style="text-align: center; font-weight: 700; color: #2563eb;">{{ it.receivedQuantity }}</td>
                  <td style="text-align: center;">
                    <span class="cell-pill stock">🏢 {{ it.inWarehouseCount || 0 }}</span>
                  </td>
                  <td style="text-align: center;">
                    <span class="cell-pill inuse">👤 {{ it.inUseCount || 0 }}</span>
                  </td>
                  <td style="text-align: center;">
                    <span v-if="it.isShortage" class="status-badge-shortage">
                      ⚠️ {{ $t('orders.short_n', { count: it.missingQuantity }) }}
                    </span>
                    <span v-else class="status-badge-complete">
                      ✅ {{ $t('orders.enough', { received: it.receivedQuantity, expected: it.expectedQuantity }) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- KHU VỰC NHẬP / QUÉT THIẾT BỊ VÀO ĐƠN -->
          <div class="device-input-section">
            <div class="device-input-header">
              <div>
                <h3 class="section-title">{{ $t('orders.receive_title') }}</h3>
                <p class="section-subtitle">{{ $t('orders.receive_subtitle') }}</p>
              </div>
              <div class="input-mode-tabs">
                <button 
                  type="button" 
                  class="mode-tab" 
                  :class="{ active: inputMode === 'single' }"
                  @click="inputMode = 'single'"
                >
                  ⚡ {{ $t('orders.mode_single') }}
                </button>
                <button 
                  type="button" 
                  class="mode-tab" 
                  :class="{ active: inputMode === 'bulk' }"
                  @click="inputMode = 'bulk'"
                >
                  📋 {{ $t('orders.mode_bulk') }}
                </button>
              </div>
            </div>

            <!-- CHẾ ĐỘ 1: NHẬP TỪNG MÁY -->
            <div v-if="inputMode === 'single'" class="single-input-box">
              <form @submit.prevent="submitSingleDevice" class="single-form-grid">
                <div class="form-group">
                  <label class="form-label">{{ $t('orders.select_type') }} <span class="req">*</span></label>
                  <select class="form-control" v-model="singleDeviceForm.orderItemID" required>
                    <option v-for="it in currentOrder.items" :key="it.orderItemID" :value="it.orderItemID">
                      {{ it.modelName }} {{ it.receivedQuantity >= it.expectedQuantity ? $t('orders.item_opt_full', { received: it.receivedQuantity, expected: it.expectedQuantity }) : $t('orders.item_opt_need', { received: it.receivedQuantity, expected: it.expectedQuantity }) }}
                    </option>
                  </select>
                </div>

                <div v-if="isSingleQtyItem" class="form-group">
                  <label class="form-label">{{ $t('orders.qty_no_sn') }} <span class="req">*</span></label>
                  <input 
                    type="number" 
                    class="form-control highlight-input" 
                    v-model.number="singleDeviceForm.quantity" 
                    min="1" 
                    :max="singleRemaining"
                    required 
                  />
                </div>

                <div v-else class="form-group">
                  <label class="form-label">Serial Number (S/N) <span class="req">*</span></label>
                  <input 
                    type="text" 
                    class="form-control highlight-input" 
                    v-model="singleDeviceForm.serialNumber" 
                    required 
                    :placeholder="$t('orders.serial_ph')" 
                    ref="serialInputRef"
                    autofocus
                  />
                </div>

                <div v-if="!isSingleQtyItem" class="form-group">
                  <label class="form-label">{{ $t('orders.asset_code_label') }}</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    v-model="singleDeviceForm.assetCode" 
                    :placeholder="$t('orders.asset_code_ph')" 
                  />
                </div>

                <div class="form-group btn-col">
                  <button type="submit" class="btn btn-primary btn-add-device" :disabled="submittingDevice">
                    <span v-if="submittingDevice" class="spinner-mini"></span>
                    <span>{{ $t('orders.btn_add_device') }}</span>
                  </button>
                </div>
              </form>
            </div>

            <!-- CHẾ ĐỘ 2: DÁN HÀNG LOẠT SERIAL -->
            <div v-else class="bulk-input-box">
              <div class="form-group" style="margin-bottom: 12px;">
                <label class="form-label">{{ $t('orders.select_type_bulk') }} <span class="req">*</span></label>
                <select class="form-control" v-model="bulkDeviceForm.orderItemID" required>
                  <option v-for="it in currentOrder.items" :key="it.orderItemID" :value="it.orderItemID">
                    {{ it.modelName }} {{ it.receivedQuantity >= it.expectedQuantity ? $t('orders.item_opt_full', { received: it.receivedQuantity, expected: it.expectedQuantity }) : $t('orders.item_opt_need', { received: it.receivedQuantity, expected: it.expectedQuantity }) }}
                  </option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label">{{ $t('orders.bulk_label') }} <span class="req">*</span></label>
                <textarea 
                  class="form-control bulk-textarea" 
                  rows="5" 
                  v-model="bulkDeviceForm.serialText" 
                  placeholder="5CD5183JTG&#10;5CD5072DWT&#10;5CD5183KL9&#10;..."
                ></textarea>
                <span class="field-hint">{{ $t('orders.bulk_hint') }}</span>
              </div>

              <button 
                type="button" 
                class="btn btn-primary" 
                @click="submitBulkDevices" 
                :disabled="submittingDevice || !bulkDeviceForm.serialText.trim()"
                style="margin-top: 10px;"
              >
                <span v-if="submittingDevice" class="spinner-mini"></span>
                <span>{{ $t('orders.btn_add_bulk') }}</span>
              </button>
            </div>
          </div>

          <!-- DANH SÁCH THIẾT BỊ ĐÃ NHẬP VÀO ĐƠN -->
          <div class="devices-list-section">
            <div class="list-title-row">
              <h4>{{ $t('orders.devices_list_title', { count: currentOrder.devices.length }) }}</h4>
              <span v-if="currentOrder.departmentID" class="badge-pending-sync" style="background: #eff6ff; color: #1d4ed8;">
                🏬 {{ $t('orders.dept_order_no_sync', { dept: currentOrder.departmentName }) }}
              </span>
              <span v-else-if="untransferredCount > 0" class="badge-pending-sync">
                ⚡ {{ $t('orders.pending_sync_count', { count: untransferredCount }) }}
              </span>
            </div>

            <div v-if="currentOrder.devices.length === 0" class="empty-devices">
              {{ $t('orders.no_devices') }}
            </div>

            <div v-else class="table-responsive" style="max-height: 280px; overflow-y: auto;">
              <table class="sub-table device-grid-table">
                <thead>
                  <tr>
                    <th style="width: 35px;">#</th>
                    <th style="width: 130px;">{{ $t('orders.col_asset_code') }}</th>
                    <th style="width: 160px;">SERIAL NUMBER</th>
                    <th>{{ $t('orders.col_device_name') }}</th>
                    <th style="width: 200px;">{{ $t('orders.col_state_location') }}</th>
                    <th style="width: 135px;">{{ $t('orders.col_sync_stock') }}</th>
                    <th style="width: 55px; text-align: center;">{{ $t('orders.col_delete') }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(dev, dIdx) in currentOrder.devices" :key="dev.deviceItemID">
                    <td>{{ dIdx + 1 }}</td>
                    <td><span class="code-badge mini">{{ dev.assetCode }}</span></td>
                    <td><strong class="sn-text-bold">{{ dev.serialNumber || '---' }}</strong></td>
                    <td>{{ dev.assetName }}</td>
                    <td>
                      <div v-if="dev.assetStatus === 'In-Use'" class="holder-info" :title="dev.currentHolderName">
                        <span class="alloc-dot blue"></span>
                        <span class="holder-name">👤 {{ dev.currentHolderName || t('orders.in_use_default') }}</span>
                        <span v-if="dev.currentHolderCode" class="holder-code">({{ dev.currentHolderCode }})</span>
                      </div>
                      <div v-else-if="dev.assetStatus === 'Broken'" class="holder-info text-danger">
                        <span class="alloc-dot red"></span>
                        <span>🛠️ {{ $t('orders.dev_broken') }}</span>
                      </div>
                      <div v-else-if="dev.assetStatus === 'Maintenance'" class="holder-info text-warning">
                        <span class="alloc-dot amber"></span>
                        <span>🔧 {{ $t('orders.dev_maintenance') }}</span>
                      </div>
                      <div v-else class="holder-info">
                        <span class="alloc-dot green"></span>
                        <span class="stock-loc">🏢 {{ $t('orders.in_stock_at', { loc: dev.warehouseLocation || 'Kho IT' }) }}</span>
                      </div>
                    </td>
                    <td>
                      <span v-if="dev.isTransferredToAsset" class="badge-synced">
                        ✅ {{ $t('orders.synced') }}
                      </span>
                      <span v-else-if="currentOrder.departmentID" class="badge-unsynced" style="background: #eff6ff; color: #1d4ed8;">
                        🏬 {{ $t('orders.dept_owned') }}
                      </span>
                      <span v-else class="badge-unsynced">
                        ⏳ {{ $t('orders.awaiting_sync') }}
                      </span>
                    </td>
                    <td style="text-align: center;">
                      <button 
                        type="button" 
                        class="btn-icon-del" 
                        @click="deleteDeviceItem(dev.deviceItemID)"
                        :title="$t('orders.remove_device_title')"
                      >
                        ✕
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        <!-- BIÊN BẢN BÀN GIAO CỦA ĐƠN BỘ PHẬN: IN + IMPORT LƯU TRỮ -->
        <div v-if="currentOrder.departmentID" class="order-docs-section">
          <div class="list-title-row">
            <h4>📄 {{ $t('orders.docs_title', { count: orderDocs.length }) }}</h4>
            <div style="display: flex; gap: 8px;">
              <button type="button" class="btn btn-secondary" @click="isPrintOpen = true" :disabled="!currentOrder.items?.length">
                🖨️ {{ $t('orders.btn_print_handover') }}
              </button>
              <button type="button" class="btn btn-primary" @click="orderDocInput?.click()" :disabled="uploadingDoc">
                <span v-if="uploadingDoc" class="spinner-mini"></span>
                <span>⬆️ {{ $t('orders.btn_import_doc') }}</span>
              </button>
              <input ref="orderDocInput" type="file" accept=".pdf,.png,.jpg,.jpeg,.doc,.docx,.xls,.xlsx" style="display: none;" @change="onOrderDocSelected" />
            </div>
          </div>
          <div v-if="orderDocs.length === 0" class="empty-devices">{{ $t('orders.no_docs') }}</div>
          <table v-else class="sub-table">
            <thead>
              <tr>
                <th>{{ $t('orders.doc_name') }}</th>
                <th style="width: 110px;">{{ $t('orders.doc_size') }}</th>
                <th style="width: 150px;">{{ $t('orders.doc_import_date') }}</th>
                <th style="width: 130px; text-align: center;">{{ $t('orders.action') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="doc in orderDocs" :key="doc.documentID">
                <td>{{ doc.documentName }}</td>
                <td>{{ doc.fileSizeFormatted }}</td>
                <td>{{ formatDate(doc.createdAt) }}</td>
                <td style="text-align: center;">
                  <a :href="documentsApi.getDownloadUrl(doc.documentID)" target="_blank" class="btn btn-secondary" style="padding: 2px 8px;">{{ $t('orders.view') }}</a>
                  <button type="button" class="btn-icon-del" @click="deleteOrderDoc(doc)" :title="$t('orders.delete_doc_title')">✕</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        </div>

        <!-- FOOTER: NÚT ĐỒNG BỘ SANG QUẢN LÝ TÀI SẢN -->
        <div class="modal-footer modal-footer-split">
          <div>
            <button type="button" class="btn btn-secondary" @click="isDetailModalOpen = false">{{ $t('orders.close') }}</button>
          </div>

          <div class="footer-sync-actions">
            <button 
              v-if="!currentOrder.departmentID"
              type="button" 
              class="btn btn-success btn-sync-assets" 
              @click="syncOrderToAssets"
              :disabled="syncingAssets || currentOrder.devices.length === 0"
            >
              <span v-if="syncingAssets" class="spinner-mini"></span>
              <span v-else>🚀</span>
              <span>{{ $t('orders.sync_btn', { count: untransferredCount }) }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <OrderDepartmentPrintModal
      :is-open="isPrintOpen"
      :order="currentOrder"
      :departments="departmentsList"
      @close="isPrintOpen = false"
    />

    <!-- ======================================================== -->
    <!-- MODAL BƯỚC 3: SỬA ĐƠN HÀNG (Cập nhật thông tin - U in CRUD) -->
    <!-- ======================================================== -->
    <div v-if="isEditModalOpen" class="modal-backdrop" @click.self="isEditModalOpen = false">
      <div class="glass-modal modal-md">
        <div class="modal-header">
          <div class="modal-title-box">
            <div class="modal-icon blue">✏️</div>
            <div>
              <h2 class="modal-title">{{ $t('orders.edit_title') }}</h2>
              <p class="modal-subtitle">{{ $t('orders.edit_subtitle_code', { code: editForm.orderCode }) }}</p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isEditModalOpen = false">✕</button>
        </div>

        <form @submit.prevent="submitUpdateOrder" class="modal-body">
          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.pr_code_label2') }} <span class="badge-optional">{{ $t('orders.optional') }}</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.prCode" 
                :placeholder="$t('orders.pr_ph')"
              />
            </div>
            <div class="form-group">
              <label class="form-label">{{ $t('orders.ordering_dept') }} <span class="badge-optional">{{ $t('orders.optional') }}</span></label>
              <select class="form-control" v-model="editForm.departmentID">
                <option :value="null">{{ $t('orders.shared_stock') }}</option>
                <option v-for="d in departmentsList" :key="d.departmentID" :value="d.departmentID">{{ d.departmentName }}</option>
              </select>
              <span class="field-hint-dept">{{ $t('orders.dept_hint') }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">{{ $t('orders.order_status') }}</label>
              <select class="form-control" v-model="editForm.status">
                <option value="Pending">{{ $t('orders.opt2_pending') }}</option>
                <option value="Receiving">{{ $t('orders.opt2_receiving') }}</option>
                <option value="Partial">{{ $t('orders.opt2_partial') }}</option>
                <option value="Completed">{{ $t('orders.opt_completed') }}</option>
                <option value="Cancelled">{{ $t('orders.opt2_cancelled') }}</option>
              </select>
            </div>
          </div>

          <div class="form-grid-2" style="margin-top: 10px;">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.supplier') }}</label>
              <select class="form-control" v-model="editForm.supplierID" @change="onEditSupplierChange">
                <option :value="null">{{ $t('orders.supplier_ph') }}</option>
                <option v-for="sup in suppliersList" :key="sup.supplierID" :value="sup.supplierID">
                  {{ sup.supplierName }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">{{ $t('orders.expected_delivery_short') }}</label>
              <DateTextInput v-model="editForm.expectedDeliveryDate" />
            </div>
          </div>

          <!-- Thuộc dự án -->
          <div class="project-box" style="margin-top: 12px;">
            <label class="checkbox-label">
              <input type="checkbox" v-model="editForm.isProjectBased" />
              <span class="checkbox-text">{{ $t('orders.is_project') }}</span>
            </label>
            <div v-if="editForm.isProjectBased" class="project-input-wrap">
              <label class="form-label">{{ $t('orders.project_name') }} <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="editForm.projectName" 
                required 
                :placeholder="$t('orders.project_ph_short')" 
              />
            </div>
          </div>

          <div class="form-group" style="margin-top: 10px;">
            <label class="form-label">{{ $t('orders.order_note') }}</label>
            <textarea 
              class="form-control" 
              rows="3" 
              v-model="editForm.note" 
              :placeholder="$t('orders.note_edit_ph')"
            ></textarea>
          </div>

          <div class="modal-footer" style="padding: 16px 0 0 0; margin-top: 16px; border-top: 1px solid #e2e8f0; display: flex; justify-content: flex-end; gap: 10px;">
            <button type="button" class="btn btn-secondary" @click="isEditModalOpen = false">{{ $t('orders.cancel') }}</button>
            <button type="submit" class="btn btn-primary" :disabled="editSubmitting">
              <span v-if="editSubmitting" class="spinner-mini"></span>
              <span>{{ $t('orders.save_changes') }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- ======================================================== -->
    <!-- MODAL THÊM LOẠI THIẾT BỊ CHO ĐƠN HÀNG (Add Item to Order) -->
    <!-- ======================================================== -->
    <div v-if="isAddItemModalOpen" class="modal-backdrop" @click.self="isAddItemModalOpen = false">
      <div class="glass-modal modal-md">
        <div class="modal-header">
          <div class="modal-title-box">
            <div class="modal-icon green">➕</div>
            <div>
              <h2 class="modal-title">{{ $t('orders.add_item_title') }}</h2>
              <p class="modal-subtitle">
                {{ $t('orders.order_label') }} <strong>{{ targetOrderForAddItem?.orderCode }}</strong>
                <span v-if="targetOrderForAddItem?.prCode"> (PR: {{ targetOrderForAddItem?.prCode }})</span>
              </p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isAddItemModalOpen = false">✕</button>
        </div>

        <form @submit.prevent="submitAddItem" class="modal-body">
          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.category_device') }} <span class="req">*</span></label>
              <select class="form-control" v-model="addItemForm.categoryID" required>
                <option :value="null">{{ $t('orders.category_ph') }}</option>
                <option v-for="cat in categoriesList" :key="cat.categoryID" :value="cat.categoryID">
                  {{ cat.categoryName }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.qty_expected_buy') }} <span class="req">*</span></label>
              <input 
                type="number" 
                class="form-control" 
                v-model.number="addItemForm.expectedQuantity" 
                min="1" 
                required 
                :placeholder="$t('orders.qty_ph')" 
              />
            </div>
          </div>

          <div class="form-grid-2" style="margin-top: 10px;">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.model_name') }} <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="addItemForm.modelName" 
                required 
                :placeholder="$t('orders.model_ph2')" 
              />
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.manufacturer') }}</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="addItemForm.brand" 
                :placeholder="$t('orders.manufacturer_ph')" 
              />
            </div>
          </div>

          <div class="form-group" style="margin-top: 10px;">
            <label class="form-label">{{ $t('orders.spec_tech') }}</label>
            <input 
              type="text" 
              class="form-control" 
              v-model="addItemForm.specifications" 
              :placeholder="$t('orders.spec_ph2')" 
            />
          </div>

          <div class="form-grid-2" style="margin-top: 10px;">
            <div class="form-group">
              <label class="form-label">{{ $t('orders.unit_price') }}</label>
              <input 
                type="number" 
                class="form-control" 
                v-model.number="addItemForm.unitPrice" 
                :placeholder="$t('orders.price_ph')" 
              />
            </div>

            <div class="form-group">
              <label class="form-label">{{ $t('orders.line_note') }}</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="addItemForm.note" 
                :placeholder="$t('orders.line_note_ph')" 
              />
            </div>
          </div>

          <div class="modal-footer" style="padding: 16px 0 0 0; margin-top: 16px; border-top: 1px solid #e2e8f0; display: flex; justify-content: flex-end; gap: 10px;">
            <button type="button" class="btn btn-secondary" @click="isAddItemModalOpen = false">{{ $t('orders.cancel') }}</button>
            <button type="submit" class="btn btn-success" :disabled="addItemSubmitting">
              <span v-if="addItemSubmitting" class="spinner-mini"></span>
              <span v-else>➕</span>
              <span>{{ $t('orders.btn_add_to_order') }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import DateTextInput from '@/components/common/DateTextInput.vue'
import OrderDepartmentPrintModal from './OrdersView/OrderDepartmentPrintModal.vue'
import { ref, reactive, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ordersApi, categoriesApi, suppliersApi, departmentsApi, documentsApi } from '@/api/client'
import { isQuantityAsset } from '@/utils/assetKind'
import { t, currentLocale } from '@/i18n'

const router = useRouter()

// State danh sách đơn
const orders = ref([])
const loading = ref(false)
const searchKeyword = ref('')
const currentStatusFilter = ref('ALL')

// Dropdown thao tác CRUD gom 4 nút thành 1
const activeDropdownOrderId = ref(null)

// Dữ liệu danh mục & nhà cung cấp
const categoriesList = ref([])
const suppliersList = ref([])
const departmentsList = ref([])

// State Modal Bước 1 (Tạo đơn - C in CRUD)
const isCreateModalOpen = ref(false)
const submitting = ref(false)
const createForm = reactive({
  orderName: '',
  prCode: '',
  orderCode: '',
  isProjectBased: false,
  projectName: '',
  departmentID: null,
  supplierID: null,
  supplierName: '',
  note: '',
  items: []
})

// State Modal Sửa đơn hàng (Update - U in CRUD)
const isEditModalOpen = ref(false)
const editSubmitting = ref(false)
const editForm = reactive({
  orderID: null,
  orderCode: '',
  orderName: '',
  prCode: '',
  isProjectBased: false,
  projectName: '',
  departmentID: null,
  supplierID: null,
  supplierName: '',
  expectedDeliveryDate: '',
  status: 'Pending',
  note: ''
})

// State Modal Thêm Loại Thiết Bị Cho Đơn Hàng (Add Item)
const isAddItemModalOpen = ref(false)
const addItemSubmitting = ref(false)
const targetOrderForAddItem = ref(null)
const addItemForm = reactive({
  categoryID: null,
  modelName: '',
  brand: '',
  specifications: '',
  expectedQuantity: 1,
  unitPrice: null,
  note: ''
})

// State Modal Bước 2 (Chi tiết & Quét thiết bị - R in CRUD)
const isDetailModalOpen = ref(false)
const currentOrder = ref(null)

// Biên bản bàn giao của đơn hàng bộ phận
const isPrintOpen = ref(false)
const orderDocs = ref([])
const uploadingDoc = ref(false)
const orderDocInput = ref(null)

const loadOrderDocs = async () => {
  orderDocs.value = []
  if (!currentOrder.value?.departmentID) return
  try {
    orderDocs.value = (await documentsApi.getAll({ orderId: currentOrder.value.orderID })) || []
  } catch (err) {
    console.warn('Lỗi tải biên bản đơn hàng:', err.message)
  }
}

const onOrderDocSelected = async (e) => {
  const file = e.target.files?.[0]
  e.target.value = ''
  if (!file || !currentOrder.value) return
  uploadingDoc.value = true
  try {
    const order = currentOrder.value
    const fd = new FormData()
    fd.append('file', file)
    fd.append('documentName', `Biên bản bàn giao ${order.orderCode} - ${order.departmentName || ''}`.trim())
    fd.append('documentType', 'HandoverReceipt')
    fd.append('orderID', order.orderID)
    fd.append('departmentID', order.departmentID)
    fd.append('uploadedBy', 'Admin')
    await documentsApi.upload(fd)
    await loadOrderDocs()
  } catch (err) {
    alert(t('orders.err_import_doc') + err.message)
  } finally {
    uploadingDoc.value = false
  }
}

const deleteOrderDoc = async (doc) => {
  if (!confirm(t('orders.confirm_delete_doc', { name: doc.documentName }))) return
  try {
    await documentsApi.delete(doc.documentID)
    await loadOrderDocs()
  } catch (err) {
    alert(t('orders.err_delete_doc') + err.message)
  }
}
const inputMode = ref('single') // 'single' | 'bulk'
const submittingDevice = ref(false)
const syncingAssets = ref(false)
const serialInputRef = ref(null)

const singleDeviceForm = reactive({
  orderItemID: null,
  serialNumber: '',
  quantity: 1,
  assetCode: '',
  warehouseLocation: 'Kho IT - Kệ A1'
})

const singleSelectedItem = computed(() =>
  currentOrder.value?.items?.find(it => it.orderItemID === singleDeviceForm.orderItemID)
)
const isSingleQtyItem = computed(() =>
  !!singleSelectedItem.value && isQuantityAsset(singleSelectedItem.value.modelName, singleSelectedItem.value.categoryName)
)
const singleRemaining = computed(() =>
  singleSelectedItem.value ? Math.max(0, singleSelectedItem.value.expectedQuantity - singleSelectedItem.value.receivedQuantity) : 0
)

// Khi chọn loại thiết bị theo số lượng, mặc định điền sẵn số lượng còn thiếu
watch(() => singleDeviceForm.orderItemID, () => {
  singleDeviceForm.quantity = Math.max(1, singleRemaining.value)
})

const bulkDeviceForm = reactive({
  orderItemID: null,
  serialText: '',
  warehouseLocation: 'Kho IT - Kệ A1'
})

// Tính toán KPI
const kpiStats = computed(() => {
  const pending = orders.value.filter(o => o.status === 'Pending').length
  const receiving = orders.value.filter(o => o.status === 'Receiving').length
  const completed = orders.value.filter(o => o.status === 'Completed').length
  const shortage = orders.value.filter(o => o.hasWarning).length
  const totalInWarehouse = orders.value.reduce((sum, o) => sum + (o.inWarehouseCount || 0), 0)
  const totalInUse = orders.value.reduce((sum, o) => sum + (o.inUseCount || 0), 0)
  return { pending, receiving, completed, shortage, totalInWarehouse, totalInUse }
})

// Lọc đơn hàng
const filteredOrders = computed(() => {
  let list = orders.value

  if (currentStatusFilter.value === 'Pending') {
    list = list.filter(o => o.status === 'Pending')
  } else if (currentStatusFilter.value === 'Receiving') {
    list = list.filter(o => o.status === 'Receiving')
  } else if (currentStatusFilter.value === 'Completed') {
    list = list.filter(o => o.status === 'Completed')
  } else if (currentStatusFilter.value === 'Shortage') {
    list = list.filter(o => o.hasWarning)
  }

  if (searchKeyword.value.trim()) {
    const k = searchKeyword.value.trim().toLowerCase()
    list = list.filter(o => 
      (o.orderCode || '').toLowerCase().includes(k) ||
      (o.orderName || '').toLowerCase().includes(k) ||
      (o.prCode || '').toLowerCase().includes(k) ||
      (o.projectName || '').toLowerCase().includes(k) ||
      (o.supplierName || '').toLowerCase().includes(k)
    )
  }

  return list
})

const untransferredCount = computed(() => {
  if (!currentOrder.value || !currentOrder.value.devices) return 0
  if (currentOrder.value.departmentID) return 0
  return currentOrder.value.devices.filter(d => !d.IsTransferredToAsset && !d.isTransferredToAsset).length
})

// Fetch danh sách đơn
const fetchOrders = async () => {
  loading.value = true
  try {
    const res = await ordersApi.getAll()
    orders.value = Array.isArray(res) ? res : (res?.data || [])
  } catch (err) {
    console.error('Lỗi tải danh sách đơn:', err)
  } finally {
    loading.value = false
  }
}

// Fetch Categories & Suppliers
const fetchMeta = async () => {
  try {
    const [cRes, sRes, dRes] = await Promise.allSettled([
      categoriesApi.getAll(),
      suppliersApi.getAll(),
      departmentsApi.getAll()
    ])
    if (dRes.status === 'fulfilled') departmentsList.value = dRes.value || []
    if (cRes.status === 'fulfilled') categoriesList.value = cRes.value || []
    if (sRes.status === 'fulfilled') suppliersList.value = sRes.value || []
  } catch (err) {
    console.warn('Lỗi tải meta:', err)
  }
}

// Mở Modal Bước 1
const openCreateModal = () => {
  // Reset form
  createForm.orderName = ''
  createForm.prCode = ''
  createForm.orderCode = ''
  createForm.isProjectBased = false
  createForm.projectName = ''
  createForm.departmentID = null
  createForm.supplierID = null
  createForm.supplierName = ''
  createForm.note = ''
  createForm.items = [
    {
      categoryID: categoriesList.value[0]?.categoryID || null,
      modelName: '',
      brand: '',
      specifications: '',
      expectedQuantity: 1
    }
  ]
  isCreateModalOpen.value = true
}

const addCreateItemRow = () => {
  createForm.items.push({
    categoryID: categoriesList.value[0]?.categoryID || null,
    modelName: '',
    brand: '',
    specifications: '',
    expectedQuantity: 1
  })
}

const removeCreateItemRow = (idx) => {
  createForm.items.splice(idx, 1)
}

const onSupplierChange = () => {
  const match = suppliersList.value.find(s => s.supplierID === createForm.supplierID)
  createForm.supplierName = match ? match.supplierName : ''
}

// Submit Bước 1: Tạo đơn
const submitCreateOrder = async () => {
  if (createForm.isProjectBased && !createForm.projectName.trim()) {
    alert(t('orders.msg_project_required'))
    return
  }

  submitting.value = true
  try {
    const pr = createForm.prCode?.trim() || ''
    const payload = {
      orderName: pr || undefined,
      prCode: pr || undefined,
      orderCode: createForm.orderCode?.trim() || undefined,
      isProjectBased: createForm.isProjectBased,
      projectName: createForm.isProjectBased ? createForm.projectName.trim() : undefined,
      departmentID: createForm.departmentID || undefined,
      supplierID: createForm.supplierID,
      supplierName: createForm.supplierName,
      note: createForm.note,
      items: createForm.items.map(it => ({
        categoryID: it.categoryID,
        modelName: it.modelName,
        brand: it.brand,
        specifications: it.specifications,
        expectedQuantity: it.expectedQuantity
      }))
    }

    const createdOrder = await ordersApi.create(payload)
    isCreateModalOpen.value = false
    await fetchOrders()

    // Tự động mở Modal Bước 2 để người dùng bắt đầu nhập thiết bị ngay
    if (createdOrder && createdOrder.orderID) {
      await openDetailModal(createdOrder.orderID)
    }
  } catch (err) {
    alert(t('orders.err_create_order') + err.message)
  } finally {
    submitting.value = false
  }
}

// Mở Modal Bước 2: Quét/Nhập thiết bị
const openDetailModal = async (orderId) => {
  try {
    const detail = await ordersApi.getById(orderId)
    currentOrder.value = detail
    
    // Gán mặc định dòng thiết bị còn thiếu đầu tiên cho form nhập (nếu đã đủ hết thì lấy dòng đầu)
    if (detail.items && detail.items.length > 0) {
      const shortageItem = detail.items.find(it => it.receivedQuantity < it.expectedQuantity) || detail.items[0]
      singleDeviceForm.orderItemID = shortageItem.orderItemID
      bulkDeviceForm.orderItemID = shortageItem.orderItemID
    }
    singleDeviceForm.serialNumber = ''
    singleDeviceForm.assetCode = ''
    bulkDeviceForm.serialText = ''

    isDetailModalOpen.value = true
    loadOrderDocs()

    nextTick(() => {
      serialInputRef.value?.focus()
    })
  } catch (err) {
    alert(t('orders.err_open_detail') + err.message)
  }
}

// Âm thanh phản hồi khi quét mã vạch
const playBeep = (isSuccess = true) => {
  try {
    const AudioContext = window.AudioContext || window.webkitAudioContext
    if (!AudioContext) return
    const ctx = new AudioContext()
    const osc = ctx.createOscillator()
    const gain = ctx.createGain()
    osc.connect(gain)
    gain.connect(ctx.destination)
    osc.type = isSuccess ? 'sine' : 'sawtooth'
    osc.frequency.setValueAtTime(isSuccess ? 880 : 200, ctx.currentTime)
    gain.gain.setValueAtTime(0.2, ctx.currentTime)
    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + (isSuccess ? 0.12 : 0.4))
    osc.start()
    osc.stop(ctx.currentTime + (isSuccess ? 0.12 : 0.4))
  } catch {
    // ignore audio errors
  }
}

// Thêm từng thiết bị (Bước 2)
const submitSingleDevice = async () => {
  if (submittingDevice.value) return // Chặn gửi 2 lần liên tiếp

  if (isSingleQtyItem.value) {
    const qty = Number(singleDeviceForm.quantity)
    if (!Number.isInteger(qty) || qty < 1) {
      alert(t('orders.msg_qty_invalid'))
      return
    }
    if (qty > singleRemaining.value) {
      playBeep(false)
      alert(t('orders.alert_over_remaining', { model: singleSelectedItem.value.modelName, remaining: singleRemaining.value, qty: qty }))
      return
    }
    submittingDevice.value = true
    try {
      await ordersApi.addQuantityDevices(currentOrder.value.orderID, {
        orderItemID: singleDeviceForm.orderItemID,
        quantity: qty,
        warehouseLocation: singleDeviceForm.warehouseLocation
      })
      playBeep(true)
      currentOrder.value = await ordersApi.getById(currentOrder.value.orderID)
      await fetchOrders()
      const next = currentOrder.value.items?.find(it => it.receivedQuantity < it.expectedQuantity)
      if (next) {
        singleDeviceForm.orderItemID = next.orderItemID
        bulkDeviceForm.orderItemID = next.orderItemID
      }
      singleDeviceForm.quantity = Math.max(1, singleRemaining.value)
    } catch (err) {
      playBeep(false)
      alert(t('orders.err_add_device') + err.message)
    } finally {
      submittingDevice.value = false
    }
    return
  }

  const rawSerial = singleDeviceForm.serialNumber?.trim()
  if (!rawSerial) {
    alert(t('orders.msg_serial_required'))
    nextTick(() => serialInputRef.value?.focus())
    return
  }

  // 1. Chặn ngay tại Client nếu dòng thiết bị này đã nhận đủ số lượng
  const selectedItem = currentOrder.value?.items?.find(it => it.orderItemID === singleDeviceForm.orderItemID)
  if (selectedItem && selectedItem.receivedQuantity >= selectedItem.expectedQuantity) {
    playBeep(false) // Âm cảnh báo lỗi
    alert(t('orders.alert_full', { model: selectedItem.modelName, received: selectedItem.receivedQuantity, expected: selectedItem.expectedQuantity }))
    singleDeviceForm.serialNumber = ''
    nextTick(() => serialInputRef.value?.focus())
    return
  }

  // 2. Chặn ngay tại Client: Kiểm tra trùng mã Serial với các thiết bị đã có trong đơn hàng này
  const duplicateDevice = currentOrder.value?.devices?.find(
    d => d.serialNumber && d.serialNumber.trim().toLowerCase() === rawSerial.toLowerCase()
  )

  if (duplicateDevice) {
    playBeep(false) // Âm cảnh báo lỗi
    alert(t('orders.alert_dup_serial', { serial: rawSerial, asset: duplicateDevice.assetCode || t('orders.unassigned') }))
    singleDeviceForm.serialNumber = ''
    nextTick(() => {
      serialInputRef.value?.focus()
    })
    return
  }

  submittingDevice.value = true
  try {
    await ordersApi.addDevice(currentOrder.value.orderID, {
      orderItemID: singleDeviceForm.orderItemID,
      serialNumber: rawSerial,
      assetCode: singleDeviceForm.assetCode?.trim() || undefined,
      warehouseLocation: singleDeviceForm.warehouseLocation
    })

    playBeep(true) // Tiếng tít xác nhận đã quét thành công

    // Reset ô serial để quét tiếp máy tiếp theo
    singleDeviceForm.serialNumber = ''
    singleDeviceForm.assetCode = ''

    // Refresh lại chi tiết đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()

    // Tự động chuyển sang loại thiết bị còn thiếu tiếp theo nếu loại này đã đủ máy
    const currentItem = updated.items?.find(it => it.orderItemID === singleDeviceForm.orderItemID)
    if (currentItem && currentItem.receivedQuantity >= currentItem.expectedQuantity) {
      const nextShortageItem = updated.items?.find(it => it.receivedQuantity < it.expectedQuantity)
      if (nextShortageItem) {
        singleDeviceForm.orderItemID = nextShortageItem.orderItemID
        bulkDeviceForm.orderItemID = nextShortageItem.orderItemID
      }
    }

    nextTick(() => {
      serialInputRef.value?.focus()
    })
  } catch (err) {
    playBeep(false) // Âm trầm cảnh báo lỗi (VD: trùng serial trong kho)
    alert(t('orders.err_add_device') + err.message)
    singleDeviceForm.serialNumber = ''
    nextTick(() => {
      serialInputRef.value?.focus()
    })
  } finally {
    submittingDevice.value = false
  }
}

// Thêm hàng loạt thiết bị (Bước 2)
const submitBulkDevices = async () => {
  if (submittingDevice.value) return

  const lines = bulkDeviceForm.serialText
    .split('\n')
    .map(s => s.trim())
    .filter(Boolean)

  if (lines.length === 0) {
    alert(t('orders.msg_paste_serials'))
    return
  }

  // Kiểm tra số lượng còn thiếu của dòng này
  const selectedItem = currentOrder.value?.items?.find(it => it.orderItemID === bulkDeviceForm.orderItemID)
  if (selectedItem) {
    const remaining = selectedItem.expectedQuantity - selectedItem.receivedQuantity
    if (remaining <= 0) {
      playBeep(false)
      alert(t('orders.alert_bulk_full', { model: selectedItem.modelName, received: selectedItem.receivedQuantity, expected: selectedItem.expectedQuantity }))
      return
    }
    if (lines.length > remaining) {
      playBeep(false)
      alert(t('orders.alert_bulk_over', { model: selectedItem.modelName, remaining: remaining, received: selectedItem.receivedQuantity, expected: selectedItem.expectedQuantity, count: lines.length }))
      return
    }
  }

  // 1. Kiểm tra trùng lặp trong chính danh sách dán vào
  const lowerMap = new Map()
  const internalDups = []
  for (const sn of lines) {
    const lower = sn.toLowerCase()
    if (lowerMap.has(lower)) {
      if (!internalDups.includes(sn)) internalDups.push(sn)
    } else {
      lowerMap.set(lower, sn)
    }
  }

  if (internalDups.length > 0) {
    playBeep(false)
    alert(t('orders.alert_internal_dups', { list: internalDups.join('\n- ') }))
    return
  }

  // 2. Kiểm tra trùng với các máy đã có trong đơn hàng
  const currentSerials = new Set(
    (currentOrder.value?.devices || [])
      .map(d => d.serialNumber?.trim().toLowerCase())
      .filter(Boolean)
  )

  const dupsWithCurrentOrder = lines.filter(sn => currentSerials.has(sn.toLowerCase()))
  if (dupsWithCurrentOrder.length > 0) {
    playBeep(false)
    alert(t('orders.alert_dups_in_order', { list: dupsWithCurrentOrder.join('\n- ') }))
    return
  }

  submittingDevice.value = true
  try {
    await ordersApi.bulkAddDevices(currentOrder.value.orderID, {
      orderItemID: bulkDeviceForm.orderItemID,
      serialNumbers: lines,
      warehouseLocation: bulkDeviceForm.warehouseLocation
    })

    playBeep(true)
    bulkDeviceForm.serialText = ''

    // Refresh lại chi tiết đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()
    alert(t('orders.alert_bulk_ok', { count: lines.length }))
  } catch (err) {
    playBeep(false)
    alert(t('orders.err_bulk_add') + err.message)
  } finally {
    submittingDevice.value = false
  }
}

// Xóa thiết bị khỏi đơn
const deleteDeviceItem = async (deviceId) => {
  if (!confirm(t('orders.confirm_delete_device'))) return

  try {
    await ordersApi.removeDevice(currentOrder.value.orderID, deviceId)
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()
  } catch (err) {
    alert(t('orders.err_delete_device') + err.message)
  }
}

// ĐỒNG BỘ SANG BẢNG QUẢN LÝ TÀI SẢN (Bước 3)
const syncOrderToAssets = async () => {
  if (!confirm(t('orders.confirm_sync', { count: untransferredCount.value }))) return

  syncingAssets.value = true
  try {
    const res = await ordersApi.syncToAssets(currentOrder.value.orderID)
    
    // Refresh lại chi tiết đơn & danh sách đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()

    const confirmGo = confirm(
      `${res.message || t('orders.sync_success')}\n\n${t('orders.sync_goto_confirm')}`
    )
    if (confirmGo) {
      isDetailModalOpen.value = false
      router.push('/assets')
    }
  } catch (err) {
    alert(t('orders.err_sync') + err.message)
  } finally {
    syncingAssets.value = false
  }
}

// Xóa đơn hàng
// Xử lý Dropdown Thao Tác (Gom 4 nút CRUD vào 1)
const toggleActionDropdown = (orderId, event) => {
  event?.stopPropagation()
  activeDropdownOrderId.value = activeDropdownOrderId.value === orderId ? null : orderId
}

const closeDropdowns = () => {
  activeDropdownOrderId.value = null
}

const handleActionRead = (order) => {
  closeDropdowns()
  openDetailModal(order.orderID)
}

const handleActionUpdate = (order) => {
  closeDropdowns()
  openEditModal(order)
}

const handleActionAddItem = (order) => {
  closeDropdowns()
  targetOrderForAddItem.value = order
  addItemForm.categoryID = categoriesList.value[0]?.categoryID || null
  addItemForm.modelName = ''
  addItemForm.brand = ''
  addItemForm.specifications = ''
  addItemForm.expectedQuantity = 1
  addItemForm.unitPrice = null
  addItemForm.note = ''
  isAddItemModalOpen.value = true
}

const submitAddItem = async () => {
  if (!targetOrderForAddItem.value) return
  if (!addItemForm.modelName?.trim()) {
    alert(t('orders.msg_model_required'))
    return
  }
  if (addItemForm.expectedQuantity < 1) {
    alert(t('orders.msg_qty_positive'))
    return
  }

  addItemSubmitting.value = true
  try {
    const cat = categoriesList.value.find(c => c.categoryID === addItemForm.categoryID)
    const payload = {
      categoryID: addItemForm.categoryID,
      categoryName: cat?.categoryName || undefined,
      modelName: addItemForm.modelName.trim(),
      brand: addItemForm.brand?.trim() || undefined,
      specifications: addItemForm.specifications?.trim() || undefined,
      expectedQuantity: addItemForm.expectedQuantity,
      unitPrice: addItemForm.unitPrice ? Number(addItemForm.unitPrice) : undefined,
      note: addItemForm.note?.trim() || undefined
    }

    await ordersApi.addItem(targetOrderForAddItem.value.orderID, payload)
    isAddItemModalOpen.value = false
    await fetchOrders()

    // Nếu đang mở Modal Bước 2 của chính đơn này thì reload lại dữ liệu
    if (currentOrder.value && currentOrder.value.orderID === targetOrderForAddItem.value.orderID) {
      const updated = await ordersApi.getById(targetOrderForAddItem.value.orderID)
      currentOrder.value = updated
    }

    alert(t('orders.alert_item_added', { model: payload.modelName, count: payload.expectedQuantity }))
  } catch (err) {
    alert(t('orders.err_add_item') + err.message)
  } finally {
    addItemSubmitting.value = false
  }
}

const handleActionDelete = (order) => {
  closeDropdowns()
  confirmDeleteOrder(order)
}

// Modal Chỉnh Sửa Đơn Hàng (U in CRUD)
const openEditModal = (order) => {
  editForm.orderID = order.orderID
  editForm.orderCode = order.orderCode || ''
  editForm.orderName = order.orderName || ''
  editForm.prCode = order.prCode || ''
  editForm.isProjectBased = !!order.isProjectBased
  editForm.projectName = order.projectName || ''
  editForm.departmentID = order.departmentID || null
  editForm.supplierID = order.supplierID || null
  editForm.supplierName = order.supplierName || ''
  editForm.expectedDeliveryDate = order.expectedDeliveryDate ? order.expectedDeliveryDate.substring(0, 10) : ''
  editForm.status = order.status || 'Pending'
  editForm.note = order.note || ''
  isEditModalOpen.value = true
}

const onEditSupplierChange = () => {
  const match = suppliersList.value.find(s => s.supplierID === editForm.supplierID)
  editForm.supplierName = match ? match.supplierName : ''
}

const submitUpdateOrder = async () => {
  if (editForm.isProjectBased && !editForm.projectName.trim()) {
    alert(t('orders.msg_project_required'))
    return
  }

  editSubmitting.value = true
  try {
    const pr = editForm.prCode?.trim() || ''
    const payload = {
      prCode: pr || null,
      orderName: pr || editForm.orderCode || 'Đơn hàng',
      isProjectBased: editForm.isProjectBased,
      projectName: editForm.isProjectBased ? editForm.projectName.trim() : null,
      departmentID: editForm.departmentID || null,
      supplierID: editForm.supplierID,
      supplierName: editForm.supplierName || null,
      expectedDeliveryDate: editForm.expectedDeliveryDate ? new Date(editForm.expectedDeliveryDate).toISOString() : null,
      status: editForm.status,
      note: editForm.note?.trim() || null
    }

    await ordersApi.update(editForm.orderID, payload)
    isEditModalOpen.value = false
    await fetchOrders()
    alert(t('orders.msg_update_ok'))
  } catch (err) {
    alert(t('orders.err_update_order') + err.message)
  } finally {
    editSubmitting.value = false
  }
}

// Xóa đơn hàng
const confirmDeleteOrder = async (order) => {
  if (!confirm(t('orders.confirm_delete_order', { name: order.orderName, code: order.orderCode }))) return

  try {
    await ordersApi.delete(order.orderID)
    await fetchOrders()
  } catch (err) {
    alert(t('orders.err_delete_order') + err.message)
  }
}

// Helpers định dạng
const formatDate = (d) => {
  if (!d) return '---'
  return new Date(d).toLocaleDateString(currentLocale.value === 'en' ? 'en-GB' : 'vi-VN')
}

const getProgressPercent = (order) => {
  if (!order.totalExpectedQuantity || order.totalExpectedQuantity === 0) return 0
  const pct = Math.round((order.totalReceivedQuantity / order.totalExpectedQuantity) * 100)
  return Math.min(100, pct)
}

const getStatusText = (status) => {
  switch (status) {
    case 'Pending': return t('orders.status_pending')
    case 'Receiving': return t('orders.status_receiving')
    case 'Partial': return t('orders.status_partial')
    case 'Completed': return t('orders.status_completed')
    case 'Cancelled': return t('orders.status_cancelled')
    default: return status || 'Pending'
  }
}

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'Pending': return 'stat-pill gray'
    case 'Receiving': return 'stat-pill blue'
    case 'Partial': return 'stat-pill amber'
    case 'Completed': return 'stat-pill green'
    case 'Cancelled': return 'stat-pill red'
    default: return 'stat-pill'
  }
}

onMounted(() => {
  fetchOrders()
  fetchMeta()
  window.addEventListener('click', closeDropdowns)
})

onUnmounted(() => {
  window.removeEventListener('click', closeDropdowns)
})
</script>

<style scoped>
.order-docs-section {
  margin-top: 16px;
}

.orders-page {
  padding: 24px;
  max-width: 1600px;
  margin: 0 auto;
}

.page-header-flex {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}

.page-title-row {
  display: flex;
  align-items: center;
  gap: 14px;
}

.page-icon-wrapper {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: linear-gradient(135deg, #0284c7, #0369a1);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(2, 132, 199, 0.25);
}

.page-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 4px 0;
}

.page-subtitle {
  font-size: 0.85rem;
  color: #64748b;
  margin: 0;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-create-order {
  background: linear-gradient(135deg, #0284c7, #2563eb);
  color: white;
  border: none;
  font-weight: 700;
  padding: 10px 18px;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.25);
}

.btn-create-order:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
}

/* KPI Cards */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.glass-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.kpi-card {
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 14px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
}

.kpi-card.warning-highlight {
  border-color: #fca5a5;
  background: #fff5f5;
}

.kpi-icon-box {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

.kpi-icon-box.blue { background: #e0f2fe; }
.kpi-icon-box.amber { background: #fef3c7; }
.kpi-icon-box.indigo { background: #e0e7ff; }
.kpi-icon-box.red { background: #fee2e2; }
.kpi-icon-box.green { background: #dcfce7; }

.kpi-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  margin-bottom: 2px;
}

.kpi-val {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.2;
}

/* Filter Card */
.filter-card {
  padding: 14px 18px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 14px;
}

.search-box {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #f8fafc;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 8px 14px;
  width: 380px;
  max-width: 100%;
}

.search-box input {
  border: none;
  background: transparent;
  outline: none;
  width: 100%;
  font-size: 0.85rem;
  color: #0f172a;
}

.btn-clear {
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  font-size: 0.85rem;
}

.status-pills {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.pill-btn {
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  cursor: pointer;
  transition: all 0.15s;
}

.pill-btn:hover {
  background: #e2e8f0;
}

.pill-btn.active {
  background: #2563eb;
  color: #fff;
  border-color: #2563eb;
}

.pill-btn.warn.active {
  background: #dc2626;
  border-color: #dc2626;
}

/* Table */
.table-wrapper {
  padding: 0;
  overflow: visible;
}

.table-responsive {
  overflow-x: auto;
  min-height: 280px;
  padding-bottom: 70px;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  font-size: 0.775rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
  text-align: left;
}

.data-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.order-row:hover {
  background: #f8fafc;
}

.code-badge {
  font-family: monospace;
  font-weight: 700;
  font-size: 0.8rem;
  padding: 3px 8px;
  border-radius: 6px;
  display: inline-block;
}

.order-badge {
  background: #e0f2fe;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.order-title-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.order-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.order-name-text {
  font-size: 0.9rem;
  color: #1e293b;
}

.pr-badge {
  font-family: monospace;
  font-size: 0.75rem;
  font-weight: 700;
  color: #7c3aed;
  background: #f5f3ff;
  border: 1px solid #ddd6fe;
  padding: 2px 7px;
  border-radius: 5px;
  display: inline-flex;
  align-items: center;
  box-shadow: 0 1px 2px rgba(124, 58, 237, 0.08);
}

.pr-badge-large {
  font-family: monospace;
  font-size: 0.825rem;
  font-weight: 700;
  color: #6d28d9;
  background: #ede9fe;
  border: 1px solid #c4b5fd;
  padding: 3px 10px;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
}

.field-hint-dept {
  display: block;
  margin-top: 4px;
  font-size: 0.75rem;
  color: #64748b;
}

.badge-optional {
  font-size: 0.725rem;
  font-weight: 500;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: 4px;
}

.order-sub-meta {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.sup-tag, .type-tag {
  font-size: 0.725rem;
  background: #f1f5f9;
  color: #475569;
  padding: 1px 6px;
  border-radius: 4px;
  border: 1px solid #e2e8f0;
}

.order-note-text {
  font-size: 0.75rem;
  color: #64748b;
  max-width: 320px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.project-pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  background: #f5f3ff;
  border: 1px solid #ddd6fe;
  color: #6d28d9;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  max-width: 160px;
}

.proj-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.progress-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.progress-bar-wrap {
  width: 100%;
  height: 8px;
  background: #e2e8f0;
  border-radius: 999px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #3b82f6, #2563eb);
  border-radius: 999px;
  transition: width 0.3s;
}

.progress-bar-fill.is-complete {
  background: linear-gradient(90deg, #10b981, #059669);
}

.progress-bar-fill.is-shortage {
  background: linear-gradient(90deg, #f59e0b, #d97706);
}

.progress-text-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  color: #64748b;
}

.shortage-warning-badge {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
  display: inline-block;
  margin-top: 2px;
}

.stat-pill {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 999px;
  display: inline-block;
}

.stat-pill.gray { background: #f1f5f9; color: #475569; }
.stat-pill.blue { background: #e0f2fe; color: #0284c7; }
.stat-pill.amber { background: #fef3c7; color: #d97706; }
.stat-pill.green { background: #dcfce7; color: #059669; }
.stat-pill.red { background: #fee2e2; color: #dc2626; }

.row-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}

.btn-action-primary {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1d4ed8;
  font-weight: 600;
  font-size: 0.775rem;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.btn-action-primary:hover {
  background: #dbeafe;
}

.btn-action-danger {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  padding: 6px 10px;
  border-radius: 6px;
  cursor: pointer;
}

.btn-action-danger:hover {
  background: #fee2e2;
}

/* Modals */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.glass-modal {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  width: 100%;
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

.modal-large { max-width: 800px; }
.modal-xl { max-width: 1050px; }

.modal-header {
  padding: 18px 24px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
}

.modal-title-box {
  display: flex;
  align-items: center;
  gap: 12px;
}

.modal-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

.modal-icon.blue { background: #e0f2fe; color: #0284c7; }
.modal-icon.indigo { background: #e0e7ff; color: #4338ca; }

.modal-title {
  font-size: 1.15rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
}

.modal-subtitle {
  font-size: 0.8rem;
  color: #64748b;
  margin: 2px 0 0 0;
}

.btn-close {
  background: none;
  border: none;
  font-size: 1.25rem;
  color: #94a3b8;
  cursor: pointer;
}

.modal-body {
  padding: 24px;
  overflow-y: auto;
}

.modal-body-scroll {
  padding: 24px;
  overflow-y: auto;
  max-height: calc(90vh - 140px);
}

.form-grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 16px;
}

.form-grid-3 {
  display: grid;
  grid-template-columns: 1fr 1.5fr 1fr;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 0.825rem;
  font-weight: 600;
  color: #334155;
}

.req { color: #ef4444; }

.form-control {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 9px 12px;
  font-size: 0.875rem;
  color: #0f172a;
  outline: none;
  transition: border-color 0.15s;
}

.form-control:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.project-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px;
  margin-bottom: 16px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.875rem;
  color: #1e293b;
}

.project-input-wrap {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed #cbd5e1;
}

/* Items Section */
.items-section {
  margin-top: 20px;
  border-top: 1px solid #e2e8f0;
  padding-top: 18px;
}

.section-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.section-title {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.section-subtitle {
  font-size: 0.775rem;
  color: #64748b;
  margin: 2px 0 0 0;
}

.item-rows-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 280px;
  overflow-y: auto;
  padding-right: 4px;
}

.item-input-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 14px;
}

.item-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.item-idx-badge {
  background: #e2e8f0;
  color: #334155;
  font-weight: 700;
  font-size: 0.75rem;
  padding: 2px 8px;
  border-radius: 4px;
}

.btn-remove-row {
  background: none;
  border: none;
  color: #ef4444;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.modal-footer {
  padding: 16px 24px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  background: #f8fafc;
}

.modal-footer-split {
  justify-content: space-between;
}

/* Modal 2 (Chi tiết & Quét) Styles */
.title-code-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.proj-highlight {
  color: #6d28d9;
  font-weight: 600;
}

.alert-banner-warning {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 10px;
  padding: 14px 18px;
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
}

.alert-banner-warning .alert-icon {
  font-size: 1.5rem;
}

.alert-banner-warning .alert-content {
  font-size: 0.85rem;
  color: #92400e;
  line-height: 1.5;
}

.alert-sub {
  margin-top: 6px;
  font-size: 0.8rem;
  color: #78350f;
}

.reconcile-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 20px;
}

.reconcile-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.reconcile-header h3 {
  font-size: 0.9rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.reconcile-stat {
  font-size: 0.85rem;
  color: #64748b;
}

.sub-table {
  width: 100%;
  border-collapse: collapse;
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

.sub-table th {
  background: #f1f5f9;
  font-size: 0.75rem;
  font-weight: 700;
  color: #475569;
  padding: 8px 12px;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
}

.sub-table td {
  padding: 8px 12px;
  font-size: 0.825rem;
  border-bottom: 1px solid #f1f5f9;
  color: #1e293b;
}

.row-shortage {
  background: #fffdf5;
}

.cat-pill {
  background: #e0f2fe;
  color: #0369a1;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}

.status-badge-shortage {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
  font-size: 0.725rem;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
}

.status-badge-complete {
  background: #ecfdf5;
  color: #059669;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}

/* Device input section */
.device-input-section {
  background: #ffffff;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  padding: 18px;
  margin-bottom: 20px;
}

.device-input-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
  flex-wrap: wrap;
  gap: 10px;
}

.input-mode-tabs {
  display: flex;
  background: #f1f5f9;
  padding: 3px;
  border-radius: 8px;
}

.mode-tab {
  background: transparent;
  border: none;
  font-size: 0.8rem;
  font-weight: 600;
  color: #64748b;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
}

.mode-tab.active {
  background: #ffffff;
  color: #2563eb;
  box-shadow: 0 2px 4px rgba(0,0,0,0.06);
}

.single-form-grid {
  display: grid;
  grid-template-columns: 1.5fr 1.5fr 1fr auto;
  gap: 12px;
  align-items: flex-end;
}

.highlight-input {
  border-color: #3b82f6 !important;
  background: #eff6ff;
  font-family: monospace;
  font-weight: 700;
}

.btn-add-device {
  padding: 9px 16px;
  font-weight: 700;
}

.bulk-textarea {
  font-family: monospace;
  font-size: 0.85rem;
  line-height: 1.4;
}

.field-hint {
  font-size: 0.725rem;
  color: #64748b;
  margin-top: 4px;
}

/* Devices List */
.devices-list-section {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 16px;
}

.list-title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.list-title-row h4 {
  font-size: 0.875rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

.badge-pending-sync {
  background: #fef3c7;
  color: #b45309;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 6px;
}

.empty-devices {
  padding: 24px;
  text-align: center;
  color: #94a3b8;
  font-size: 0.85rem;
}

.sn-text-bold {
  font-family: monospace;
  color: #0f172a;
}

.badge-synced {
  background: #ecfdf5;
  color: #059669;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}

.badge-unsynced {
  background: #fef3c7;
  color: #92400e;
  font-size: 0.725rem;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}

.btn-icon-del {
  background: none;
  border: none;
  color: #ef4444;
  cursor: pointer;
  font-weight: 700;
}

.btn-sync-assets {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border: none;
  padding: 10px 20px;
  font-weight: 700;
  font-size: 0.9rem;
  border-radius: 8px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.25);
  transition: all 0.2s;
}

.btn-sync-assets:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(16, 185, 129, 0.35);
}

.btn-sync-assets:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Allocation & In-Warehouse / In-Use Badges */
.kpi-unit {
  font-size: 0.8rem;
  font-weight: 500;
  color: #64748b;
}

.kpi-icon-box.emerald {
  background: #d1fae5;
  color: #059669;
}

.kpi-icon-box.purple {
  background: #ede9fe;
  color: #7c3aed;
}

.kpi-card.highlight-stock {
  border-color: #a7f3d0;
  background: linear-gradient(to bottom right, #ffffff, #f0fdf4);
}

.kpi-card.highlight-inuse {
  border-color: #bfdbfe;
  background: linear-gradient(to bottom right, #ffffff, #eff6ff);
}

.allocation-badges {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.alloc-badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.775rem;
  line-height: 1.2;
}

.alloc-badge.stock {
  background: #ecfdf5;
  border: 1px solid #a7f3d0;
  color: #065f46;
}

.alloc-badge.inuse {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1e40af;
}

.alloc-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  display: inline-block;
  flex-shrink: 0;
}

.alloc-dot.green { background: #10b981; }
.alloc-dot.blue { background: #3b82f6; }
.alloc-dot.red { background: #ef4444; }
.alloc-dot.amber { background: #f59e0b; }

.alloc-badge.stock .alloc-txt { color: #047857; font-size: 0.725rem; }
.alloc-badge.inuse .alloc-txt { color: #1d4ed8; font-size: 0.725rem; }

.alloc-num {
  font-weight: 800;
  font-size: 0.825rem;
}

.modal-allocation-tags {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-left: 8px;
}

.modal-alloc-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.775rem;
  font-weight: 600;
}

.modal-alloc-tag.stock {
  background: #dcfce7;
  color: #15803d;
  border: 1px solid #86efac;
}

.modal-alloc-tag.inuse {
  background: #dbeafe;
  color: #1e40af;
  border: 1px solid #93c5fd;
}

.reconcile-sub-stat {
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  margin-left: 6px;
}

.cell-pill {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.775rem;
  font-weight: 700;
}

.cell-pill.stock {
  background: #ecfdf5;
  color: #047857;
  border: 1px solid #a7f3d0;
}

.cell-pill.inuse {
  background: #eff6ff;
  color: #1d4ed8;
  border: 1px solid #bfdbfe;
}

.holder-info {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
}

.holder-name {
  font-weight: 600;
  color: #1e40af;
}

.holder-code {
  font-size: 0.75rem;
  color: #64748b;
}

.stock-loc {
  color: #059669;
  font-weight: 500;
}

.modal-md {
  max-width: 640px;
}

/* ======================================== */
/* ACTION DROPDOWN: GOM 4 NÚT CRUD VÀO 1   */
/* ======================================== */
.action-dropdown-wrapper {
  position: relative;
  display: inline-block;
  text-align: left;
}

.btn-action-dropdown-toggle {
  background: #ffffff;
  border: 1px solid #cbd5e1;
  color: #1e293b;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.btn-action-dropdown-toggle:hover {
  background: #f8fafc;
  border-color: #94a3b8;
  color: #0f172a;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
}

.btn-action-dropdown-toggle.active {
  background: #eff6ff;
  border-color: #3b82f6;
  color: #1d4ed8;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.btn-action-icon {
  font-size: 0.85rem;
  line-height: 1;
}

.dropdown-chevron {
  font-size: 0.75rem;
  color: #64748b;
  transition: transform 0.2s;
  line-height: 1;
}

.btn-action-dropdown-toggle.active .dropdown-chevron {
  transform: rotate(180deg);
  color: #1d4ed8;
}

.action-dropdown-menu {
  position: absolute;
  right: 0;
  top: calc(100% + 6px);
  z-index: 1050;
  min-width: 260px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  box-shadow: 0 12px 28px -4px rgba(15, 23, 42, 0.15), 0 4px 12px -2px rgba(15, 23, 42, 0.08);
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  animation: dropdownFadeIn 0.15s ease-out;
}

@keyframes dropdownFadeIn {
  from {
    opacity: 0;
    transform: translateY(-4px) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.action-dropdown-menu .dropdown-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 8px 10px;
  border: none;
  background: transparent;
  border-radius: 6px;
  cursor: pointer;
  text-align: left;
  transition: all 0.15s;
  color: #1e293b;
}

.action-dropdown-menu .dropdown-item:hover {
  background: #f1f5f9;
}

.action-dropdown-menu .item-read:hover {
  background: #eff6ff;
  color: #1d4ed8;
}

.action-dropdown-menu .item-update:hover {
  background: #fefce8;
  color: #a16207;
}

.action-dropdown-menu .item-create:hover {
  background: #f0fdf4;
  color: #15803d;
}

.action-dropdown-menu .item-delete:hover {
  background: #fef2f2;
  color: #dc2626;
}

.action-dropdown-menu .item-icon {
  font-size: 1.05rem;
  flex-shrink: 0;
  width: 24px;
  text-align: center;
}

.action-dropdown-menu .item-text-wrap {
  display: flex;
  flex-direction: column;
  gap: 1px;
  overflow: hidden;
}

.action-dropdown-menu .item-title {
  font-size: 0.825rem;
  font-weight: 600;
  line-height: 1.25;
}

.action-dropdown-menu .item-desc {
  font-size: 0.72rem;
  color: #64748b;
  line-height: 1.2;
}

.action-dropdown-menu .dropdown-divider {
  height: 1px;
  background: #e2e8f0;
  margin: 4px 6px;
}
</style>
