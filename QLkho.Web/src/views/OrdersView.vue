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
            <h1 class="page-title">Quản Lý Đơn Hàng & Nhập Kho</h1>
            <p class="page-subtitle">Khởi tạo đơn hàng, đối soát thiết bị nhận thực tế, cảnh báo thiếu hàng và tự động đồng bộ sang Kho tài sản</p>
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
          <span>Làm mới</span>
        </button>
        <button type="button" class="btn btn-primary btn-create-order" @click="openCreateModal">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19"></line>
            <line x1="5" y1="12" x2="19" y2="12"></line>
          </svg>
          <span>+ Tạo Đơn Hàng Mới</span>
        </button>
      </div>
    </div>

    <!-- Thống Kê KPI Cards -->
    <div class="kpi-grid">
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box blue">📦</div>
        <div>
          <div class="kpi-label">Tổng đơn hàng</div>
          <div class="kpi-val">{{ orders.length }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box amber">⏳</div>
        <div>
          <div class="kpi-label">Chờ nhận hàng</div>
          <div class="kpi-val">{{ kpiStats.pending }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box indigo">🚚</div>
        <div>
          <div class="kpi-label">Đang nhận hàng</div>
          <div class="kpi-val">{{ kpiStats.receiving }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card" :class="{ 'warning-highlight': kpiStats.shortage > 0 }">
        <div class="kpi-icon-box red">⚠️</div>
        <div>
          <div class="kpi-label">Vận chuyển thiếu</div>
          <div class="kpi-val" style="color: #dc2626;">{{ kpiStats.shortage }}</div>
        </div>
      </div>
      <div class="glass-card kpi-card">
        <div class="kpi-icon-box green">✅</div>
        <div>
          <div class="kpi-label">Đã hoàn tất</div>
          <div class="kpi-val" style="color: #059669;">{{ kpiStats.completed }}</div>
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
          placeholder="Tìm theo Mã đơn, Tên đơn hàng, Tên dự án, Nhà cung cấp..." 
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
          Tất cả ({{ orders.length }})
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Pending' }"
          @click="currentStatusFilter = 'Pending'"
        >
          Chờ nhận
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Receiving' }"
          @click="currentStatusFilter = 'Receiving'"
        >
          Đang nhận
        </button>
        <button 
          type="button" 
          class="pill-btn warn" 
          :class="{ active: currentStatusFilter === 'Shortage' }"
          @click="currentStatusFilter = 'Shortage'"
        >
          ⚠️ Vận chuyển thiếu
        </button>
        <button 
          type="button" 
          class="pill-btn" 
          :class="{ active: currentStatusFilter === 'Completed' }"
          @click="currentStatusFilter = 'Completed'"
        >
          Hoàn tất
        </button>
      </div>
    </div>

    <!-- Bảng Danh Sách Đơn Hàng -->
    <div class="glass-card table-wrapper">
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <span>Đang tải danh sách đơn hàng...</span>
      </div>

      <div v-else-if="filteredOrders.length === 0" class="empty-state">
        <div class="empty-icon">📦</div>
        <h3>Không tìm thấy đơn hàng nào</h3>
        <p>Bấm vào nút "+ Tạo Đơn Hàng Mới" để bắt đầu quy trình nhập thiết bị.</p>
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 130px;">MÃ ĐƠN HÀNG</th>
              <th style="width: 260px;">TÊN ĐƠN HÀNG</th>
              <th style="width: 170px;">DỰ ÁN</th>
              <th style="width: 130px;">NGÀY TẠO</th>
              <th style="width: 220px;">TIẾN ĐỘ NHẬN THIẾT BỊ</th>
              <th style="width: 140px;">TRẠNG THÁI</th>
              <th style="width: 170px; text-align: right;">THAO TÁC</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in filteredOrders" :key="order.orderID" class="order-row">
              <td>
                <span class="code-badge order-badge">{{ order.orderCode }}</span>
              </td>
              <td>
                <div class="order-title-cell">
                  <strong class="order-name-text">{{ order.orderName }}</strong>
                  <div class="order-sub-meta">
                    <span v-if="order.supplierName" class="sup-tag">🏢 {{ order.supplierName }}</span>
                    <span class="type-tag">📦 {{ order.itemTypesCount }} loại thiết bị</span>
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
                    <span><strong>{{ order.totalReceivedQuantity }}</strong> / {{ order.totalExpectedQuantity }} thiết bị</span>
                    <span class="percent-text">{{ getProgressPercent(order) }}%</span>
                  </div>
                  <div v-if="order.hasWarning" class="shortage-warning-badge">
                    ⚠️ Thiếu {{ order.missingQuantity }} máy (vận chuyển thiếu)
                  </div>
                </div>
              </td>
              <td>
                <span :class="getStatusBadgeClass(order.status)">
                  {{ getStatusText(order.status) }}
                </span>
              </td>
              <td style="text-align: right;">
                <div class="row-actions">
                  <button 
                    type="button" 
                    class="btn-action-primary" 
                    @click="openDetailModal(order.orderID)"
                    title="Nhập thiết bị hoặc đối soát đơn"
                  >
                    🔍 Nhập / Quét Thiết Bị
                  </button>
                  <button 
                    type="button" 
                    class="btn-action-danger" 
                    @click="confirmDeleteOrder(order)"
                    title="Xóa đơn hàng"
                  >
                    🗑️
                  </button>
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
              <h2 class="modal-title">Tạo Đơn Hàng Nhập Thiết Bị</h2>
              <p class="modal-subtitle">Bước 1: Khai báo thông tin đơn, dự án và danh sách các loại thiết bị cần mua</p>
            </div>
          </div>
          <button type="button" class="btn-close" @click="isCreateModalOpen = false">✕</button>
        </div>

        <form @submit.prevent="submitCreateOrder" class="modal-body">
          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">Tên đơn hàng <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.orderName" 
                required 
                placeholder="VD: Đơn mua 50 Laptop HP EliteBook và màn hình Dell" 
              />
            </div>

            <div class="form-group">
              <label class="form-label">Mã đơn hàng (PO Code)</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.orderCode" 
                placeholder="Tự động sinh (VD: PO-20261006-001)" 
              />
            </div>
          </div>

          <!-- Có thuộc dự án nào không? -->
          <div class="project-box">
            <label class="checkbox-label">
              <input type="checkbox" v-model="createForm.isProjectBased" />
              <span class="checkbox-text">Đơn hàng này thuộc Dự án cụ thể</span>
            </label>
            <div v-if="createForm.isProjectBased" class="project-input-wrap">
              <label class="form-label">Tên dự án <span class="req">*</span></label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.projectName" 
                required 
                placeholder="VD: Dự án Smart Factory Q4, Dự án Setup Chi nhánh mới..." 
              />
            </div>
          </div>

          <div class="form-grid-2">
            <div class="form-group">
              <label class="form-label">Nhà cung cấp</label>
              <select class="form-control" v-model="createForm.supplierID" @change="onSupplierChange">
                <option :value="null">-- Chọn nhà cung cấp (nếu có) --</option>
                <option v-for="sup in suppliersList" :key="sup.supplierID" :value="sup.supplierID">
                  {{ sup.supplierName }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Ghi chú đơn hàng</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="createForm.note" 
                placeholder="Ghi chú về tiến độ giao hàng, người phụ trách..." 
              />
            </div>
          </div>

          <!-- DANH SÁCH LOẠI THIẾT BỊ TRONG ĐƠN -->
          <div class="items-section">
            <div class="section-header-row">
              <div>
                <h3 class="section-title">Danh Sách Loại Thiết Bị Trong Đơn</h3>
                <p class="section-subtitle">Khai báo từng dòng thiết bị cần mua và số lượng dự kiến của mỗi loại</p>
              </div>
              <button type="button" class="btn btn-outline-primary btn-sm" @click="addCreateItemRow">
                + Thêm Loại Thiết Bị
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
                    title="Xóa loại này"
                  >
                    ✕ Xóa
                  </button>
                </div>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label class="form-label">Phân loại <span class="req">*</span></label>
                    <select class="form-control" v-model="it.categoryID" required>
                      <option :value="null">-- Chọn phân loại --</option>
                      <option v-for="cat in categoriesList" :key="cat.categoryID" :value="cat.categoryID">
                        {{ cat.categoryName }}
                      </option>
                    </select>
                  </div>

                  <div class="form-group">
                    <label class="form-label">Tên Model thiết bị <span class="req">*</span></label>
                    <input 
                      type="text" 
                      class="form-control" 
                      v-model="it.modelName" 
                      required 
                      placeholder="VD: HP EliteBook 640 G11, Dell P2422H..." 
                    />
                  </div>

                  <div class="form-group">
                    <label class="form-label">Hãng sản xuất</label>
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
                    <label class="form-label">Cấu hình chi tiết</label>
                    <input 
                      type="text" 
                      class="form-control" 
                      v-model="it.specifications" 
                      placeholder="VD: Core Ultra 5-125U | RAM: 16Gb | SSD: 512Gb | Màn hình: 14inch..." 
                    />
                  </div>

                  <div class="form-group">
                    <label class="form-label">Số lượng đặt <span class="req">*</span></label>
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
            <button type="button" class="btn btn-secondary" @click="isCreateModalOpen = false">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="submitting">
              <span v-if="submitting" class="spinner-mini"></span>
              <span>Lưu Đơn & Chuyển Sang Nhập Thiết Bị →</span>
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
                <h2 class="modal-title">{{ currentOrder.orderName }}</h2>
                <span :class="getStatusBadgeClass(currentOrder.status)">
                  {{ getStatusText(currentOrder.status) }}
                </span>
              </div>
              <p class="modal-subtitle">
                <span v-if="currentOrder.isProjectBased && currentOrder.projectName" class="proj-highlight">
                  🎯 Dự án: {{ currentOrder.projectName }}
                </span>
                <span v-if="currentOrder.supplierName"> | 🏢 Nhà cung cấp: {{ currentOrder.supplierName }}</span>
                | 📅 Ngày tạo: {{ formatDate(currentOrder.orderDate) }}
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
              <strong>CẢNH BÁO VẬN CHUYỂN THIẾU HÀNG:</strong>
              <div>
                Đơn hàng được đặt <strong>{{ currentOrder.totalExpectedQuantity }}</strong> thiết bị, nhưng thực tế mới nhận và quét được <strong>{{ currentOrder.totalReceivedQuantity }}</strong> thiết bị (<strong>Thiếu {{ currentOrder.missingQuantity }} máy</strong>).
              </div>
              <div class="alert-sub">
                👉 Bạn vẫn có thể nhấn nút <strong>"Đồng bộ vào Quản lý tài sản"</strong> bên dưới để đưa {{ currentOrder.totalReceivedQuantity }} máy đã nhận vào sử dụng trước. Đơn hàng sẽ tự động ghi nhận trạng thái <strong>Nhận một phần</strong> để theo dõi đợt hàng tiếp theo.
              </div>
            </div>
          </div>

          <!-- BẢNG ĐỐI SOÁT CÁC LOẠI THIẾT BỊ TRONG ĐƠN -->
          <div class="reconcile-card">
            <div class="reconcile-header">
              <h3>Bảng Đối Soát Số Lượng Từng Loại Thiết Bị</h3>
              <div class="reconcile-stat">
                Đã nhận: <strong style="color: #2563eb;">{{ currentOrder.totalReceivedQuantity }}</strong> / {{ currentOrder.totalExpectedQuantity }} máy
              </div>
            </div>

            <table class="sub-table">
              <thead>
                <tr>
                  <th>Loại Thiết Bị</th>
                  <th>Model</th>
                  <th>Cấu Hình</th>
                  <th style="width: 100px; text-align: center;">SL Đặt</th>
                  <th style="width: 100px; text-align: center;">Đã Nhận</th>
                  <th style="width: 130px; text-align: center;">Tình Trạng</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="it in currentOrder.items" :key="it.orderItemID" :class="{ 'row-shortage': it.isShortage }">
                  <td><span class="cat-pill">{{ it.categoryName || 'Thiết bị' }}</span></td>
                  <td><strong>{{ it.modelName }}</strong></td>
                  <td class="spec-cell-mini" :title="it.specifications">{{ it.specifications || '---' }}</td>
                  <td style="text-align: center; font-weight: 600;">{{ it.expectedQuantity }}</td>
                  <td style="text-align: center; font-weight: 700; color: #2563eb;">{{ it.receivedQuantity }}</td>
                  <td style="text-align: center;">
                    <span v-if="it.isShortage" class="status-badge-shortage">
                      ⚠️ Thiếu {{ it.missingQuantity }}
                    </span>
                    <span v-else class="status-badge-complete">
                      ✅ Đủ ({{ it.receivedQuantity }}/{{ it.expectedQuantity }})
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
                <h3 class="section-title">Nhập Thiết Bị Vào Đơn Hàng</h3>
                <p class="section-subtitle">Quét mã vạch hoặc nhập số Serial của từng máy được giao</p>
              </div>
              <div class="input-mode-tabs">
                <button 
                  type="button" 
                  class="mode-tab" 
                  :class="{ active: inputMode === 'single' }"
                  @click="inputMode = 'single'"
                >
                  ⚡ Nhập / Quét Từng Máy
                </button>
                <button 
                  type="button" 
                  class="mode-tab" 
                  :class="{ active: inputMode === 'bulk' }"
                  @click="inputMode = 'bulk'"
                >
                  📋 Dán Hàng Loạt Serial
                </button>
              </div>
            </div>

            <!-- CHẾ ĐỘ 1: NHẬP TỪNG MÁY -->
            <div v-if="inputMode === 'single'" class="single-input-box">
              <form @submit.prevent="submitSingleDevice" class="single-form-grid">
                <div class="form-group">
                  <label class="form-label">Chọn Loại Thiết Bị <span class="req">*</span></label>
                  <select class="form-control" v-model="singleDeviceForm.orderItemID" required>
                    <option v-for="it in currentOrder.items" :key="it.orderItemID" :value="it.orderItemID">
                      {{ it.modelName }} (Đã nhận: {{ it.receivedQuantity }}/{{ it.expectedQuantity }})
                    </option>
                  </select>
                </div>

                <div class="form-group">
                  <label class="form-label">Serial Number (S/N) <span class="req">*</span></label>
                  <input 
                    type="text" 
                    class="form-control highlight-input" 
                    v-model="singleDeviceForm.serialNumber" 
                    required 
                    placeholder="Quét mã vạch hoặc nhập Serial..." 
                    ref="serialInputRef"
                    autofocus
                  />
                </div>

                <div class="form-group">
                  <label class="form-label">Mã tài sản (để trống tự sinh)</label>
                  <input 
                    type="text" 
                    class="form-control" 
                    v-model="singleDeviceForm.assetCode" 
                    placeholder="VD: TTH-NB00256 (Tự động)" 
                  />
                </div>

                <div class="form-group btn-col">
                  <button type="submit" class="btn btn-primary btn-add-device" :disabled="submittingDevice">
                    <span v-if="submittingDevice" class="spinner-mini"></span>
                    <span>+ Thêm Máy</span>
                  </button>
                </div>
              </form>
            </div>

            <!-- CHẾ ĐỘ 2: DÁN HÀNG LOẠT SERIAL -->
            <div v-else class="bulk-input-box">
              <div class="form-group" style="margin-bottom: 12px;">
                <label class="form-label">Chọn Loại Thiết Bị Cần Nhập <span class="req">*</span></label>
                <select class="form-control" v-model="bulkDeviceForm.orderItemID" required>
                  <option v-for="it in currentOrder.items" :key="it.orderItemID" :value="it.orderItemID">
                    {{ it.modelName }} (Đã nhận: {{ it.receivedQuantity }}/{{ it.expectedQuantity }})
                  </option>
                </select>
              </div>

              <div class="form-group">
                <label class="form-label">Dán Danh Sách Serial Number (Mỗi dòng một Serial) <span class="req">*</span></label>
                <textarea 
                  class="form-control bulk-textarea" 
                  rows="5" 
                  v-model="bulkDeviceForm.serialText" 
                  placeholder="5CD5183JTG&#10;5CD5072DWT&#10;5CD5183KL9&#10;..."
                ></textarea>
                <span class="field-hint">Hệ thống sẽ tự động bóc tách các dòng và sinh mã tài sản tăng dần.</span>
              </div>

              <button 
                type="button" 
                class="btn btn-primary" 
                @click="submitBulkDevices" 
                :disabled="submittingDevice || !bulkDeviceForm.serialText.trim()"
                style="margin-top: 10px;"
              >
                <span v-if="submittingDevice" class="spinner-mini"></span>
                <span>Thêm Hàng Loạt Thiết Bị Vào Đơn</span>
              </button>
            </div>
          </div>

          <!-- DANH SÁCH THIẾT BỊ ĐÃ NHẬP VÀO ĐƠN -->
          <div class="devices-list-section">
            <div class="list-title-row">
              <h4>Danh Sách Thiết Bị Đã Nhập Vào Đơn ({{ currentOrder.devices.length }} máy)</h4>
              <span v-if="untransferredCount > 0" class="badge-pending-sync">
                ⚡ {{ untransferredCount }} máy chưa đồng bộ vào kho
              </span>
            </div>

            <div v-if="currentOrder.devices.length === 0" class="empty-devices">
              Chưa có thiết bị nào được quét/nhập vào đơn. Hãy sử dụng ô quét Serial bên trên để bắt đầu.
            </div>

            <div v-else class="table-responsive" style="max-height: 280px; overflow-y: auto;">
              <table class="sub-table device-grid-table">
                <thead>
                  <tr>
                    <th style="width: 40px;">#</th>
                    <th style="width: 140px;">MÃ TÀI SẢN</th>
                    <th style="width: 180px;">SERIAL NUMBER</th>
                    <th>TÊN THIẾT BỊ</th>
                    <th style="width: 150px;">ĐỒNG BỘ VÀO KHO</th>
                    <th style="width: 70px; text-align: center;">XÓA</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(dev, dIdx) in currentOrder.devices" :key="dev.deviceItemID">
                    <td>{{ dIdx + 1 }}</td>
                    <td><span class="code-badge mini">{{ dev.assetCode }}</span></td>
                    <td><strong class="sn-text-bold">{{ dev.serialNumber || '---' }}</strong></td>
                    <td>{{ dev.assetName }}</td>
                    <td>
                      <span v-if="dev.isTransferredToAsset" class="badge-synced">
                        ✅ Đã vào Kho Tài sản
                      </span>
                      <span v-else class="badge-unsynced">
                        ⏳ Chờ đồng bộ
                      </span>
                    </td>
                    <td style="text-align: center;">
                      <button 
                        type="button" 
                        class="btn-icon-del" 
                        @click="deleteDeviceItem(dev.deviceItemID)"
                        title="Xóa máy này khỏi đơn"
                      >
                        ✕
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- FOOTER: NÚT ĐỒNG BỘ SANG QUẢN LÝ TÀI SẢN -->
        <div class="modal-footer modal-footer-split">
          <div>
            <button type="button" class="btn btn-secondary" @click="isDetailModalOpen = false">Đóng</button>
          </div>

          <div class="footer-sync-actions">
            <button 
              type="button" 
              class="btn btn-success btn-sync-assets" 
              @click="syncOrderToAssets"
              :disabled="syncingAssets || currentOrder.devices.length === 0"
            >
              <span v-if="syncingAssets" class="spinner-mini"></span>
              <span v-else>🚀</span>
              <span>Đồng Bộ Vào Quản Lý Tài Sản ({{ untransferredCount }} máy mới)</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { ordersApi, categoriesApi, suppliersApi } from '@/api/client'

const router = useRouter()

// State danh sách đơn
const orders = ref([])
const loading = ref(false)
const searchKeyword = ref('')
const currentStatusFilter = ref('ALL')

// Dữ liệu danh mục & nhà cung cấp
const categoriesList = ref([])
const suppliersList = ref([])

// State Modal Bước 1 (Tạo đơn)
const isCreateModalOpen = ref(false)
const submitting = ref(false)
const createForm = reactive({
  orderName: '',
  orderCode: '',
  isProjectBased: false,
  projectName: '',
  supplierID: null,
  supplierName: '',
  note: '',
  items: []
})

// State Modal Bước 2 (Chi tiết & Quét thiết bị)
const isDetailModalOpen = ref(false)
const currentOrder = ref(null)
const inputMode = ref('single') // 'single' | 'bulk'
const submittingDevice = ref(false)
const syncingAssets = ref(false)
const serialInputRef = ref(null)

const singleDeviceForm = reactive({
  orderItemID: null,
  serialNumber: '',
  assetCode: '',
  warehouseLocation: 'Kho IT - Kệ A1'
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
  return { pending, receiving, completed, shortage }
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
      (o.projectName || '').toLowerCase().includes(k) ||
      (o.supplierName || '').toLowerCase().includes(k)
    )
  }

  return list
})

const untransferredCount = computed(() => {
  if (!currentOrder.value || !currentOrder.value.devices) return 0
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
    const [cRes, sRes] = await Promise.allSettled([
      categoriesApi.getAll(),
      suppliersApi.getAll()
    ])
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
  createForm.orderCode = ''
  createForm.isProjectBased = false
  createForm.projectName = ''
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
  if (!createForm.orderName.trim()) {
    alert('Vui lòng nhập tên đơn hàng!')
    return
  }

  if (createForm.isProjectBased && !createForm.projectName.trim()) {
    alert('Vui lòng nhập tên dự án!')
    return
  }

  submitting.value = true
  try {
    const payload = {
      orderName: createForm.orderName.trim(),
      orderCode: createForm.orderCode.trim() || undefined,
      isProjectBased: createForm.isProjectBased,
      projectName: createForm.isProjectBased ? createForm.projectName.trim() : undefined,
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
    alert('Lỗi tạo đơn hàng: ' + err.message)
  } finally {
    submitting.value = false
  }
}

// Mở Modal Bước 2: Quét/Nhập thiết bị
const openDetailModal = async (orderId) => {
  try {
    const detail = await ordersApi.getById(orderId)
    currentOrder.value = detail
    
    // Gán mặc định dòng thiết bị đầu tiên cho form nhập
    if (detail.items && detail.items.length > 0) {
      singleDeviceForm.orderItemID = detail.items[0].orderItemID
      bulkDeviceForm.orderItemID = detail.items[0].orderItemID
    }
    singleDeviceForm.serialNumber = ''
    singleDeviceForm.assetCode = ''
    bulkDeviceForm.serialText = ''

    isDetailModalOpen.value = true

    nextTick(() => {
      serialInputRef.value?.focus()
    })
  } catch (err) {
    alert('Lỗi mở chi tiết đơn hàng: ' + err.message)
  }
}

// Thêm từng thiết bị (Bước 2)
const submitSingleDevice = async () => {
  if (!singleDeviceForm.serialNumber.trim()) {
    alert('Vui lòng nhập số Serial!')
    return
  }

  submittingDevice.value = true
  try {
    await ordersApi.addDevice(currentOrder.value.orderID, {
      orderItemID: singleDeviceForm.orderItemID,
      serialNumber: singleDeviceForm.serialNumber.trim(),
      assetCode: singleDeviceForm.assetCode.trim() || undefined,
      warehouseLocation: singleDeviceForm.warehouseLocation
    })

    // Reset ô serial để quét tiếp máy tiếp theo
    singleDeviceForm.serialNumber = ''
    singleDeviceForm.assetCode = ''

    // Refresh lại chi tiết đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()

    nextTick(() => {
      serialInputRef.value?.focus()
    })
  } catch (err) {
    alert('Lỗi thêm thiết bị: ' + err.message)
  } finally {
    submittingDevice.value = false
  }
}

// Thêm hàng loạt thiết bị (Bước 2)
const submitBulkDevices = async () => {
  const lines = bulkDeviceForm.serialText
    .split('\n')
    .map(s => s.trim())
    .filter(Boolean)

  if (lines.length === 0) {
    alert('Vui lòng dán danh sách Serial Number!')
    return
  }

  submittingDevice.value = true
  try {
    await ordersApi.bulkAddDevices(currentOrder.value.orderID, {
      orderItemID: bulkDeviceForm.orderItemID,
      serialNumbers: lines,
      warehouseLocation: bulkDeviceForm.warehouseLocation
    })

    bulkDeviceForm.serialText = ''

    // Refresh lại chi tiết đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()
    alert(`Đã thêm thành công ${lines.length} thiết bị vào đơn!`)
  } catch (err) {
    alert('Lỗi thêm hàng loạt: ' + err.message)
  } finally {
    submittingDevice.value = false
  }
}

// Xóa thiết bị khỏi đơn
const deleteDeviceItem = async (deviceId) => {
  if (!confirm('Bạn có chắc muốn xóa thiết bị này khỏi đơn?')) return

  try {
    await ordersApi.removeDevice(currentOrder.value.orderID, deviceId)
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()
  } catch (err) {
    alert('Lỗi xóa thiết bị: ' + err.message)
  }
}

// ĐỒNG BỘ SANG BẢNG QUẢN LÝ TÀI SẢN (Bước 3)
const syncOrderToAssets = async () => {
  if (!confirm(`Xác nhận đồng bộ ${untransferredCount.value} thiết bị sang tab Quản lý Tài sản?`)) return

  syncingAssets.value = true
  try {
    const res = await ordersApi.syncToAssets(currentOrder.value.orderID)
    
    // Refresh lại chi tiết đơn & danh sách đơn
    const updated = await ordersApi.getById(currentOrder.value.orderID)
    currentOrder.value = updated
    await fetchOrders()

    const confirmGo = confirm(
      `${res.message || 'Đồng bộ thành công!'}\n\nBạn có muốn chuyển sang tab Quản lý Tài sản ngay bây giờ để xem danh sách tài sản vừa nhập không?`
    )
    if (confirmGo) {
      isDetailModalOpen.value = false
      router.push('/assets')
    }
  } catch (err) {
    alert('Lỗi đồng bộ vào tài sản: ' + err.message)
  } finally {
    syncingAssets.value = false
  }
}

// Xóa đơn hàng
const confirmDeleteOrder = async (order) => {
  if (!confirm(`Bạn có chắc muốn xóa đơn hàng "${order.orderName}" (${order.orderCode})?`)) return

  try {
    await ordersApi.delete(order.orderID)
    await fetchOrders()
  } catch (err) {
    alert('Lỗi xóa đơn hàng: ' + err.message)
  }
}

// Helpers định dạng
const formatDate = (d) => {
  if (!d) return '---'
  return new Date(d).toLocaleDateString('vi-VN')
}

const getProgressPercent = (order) => {
  if (!order.totalExpectedQuantity || order.totalExpectedQuantity === 0) return 0
  const pct = Math.round((order.totalReceivedQuantity / order.totalExpectedQuantity) * 100)
  return Math.min(100, pct)
}

const getStatusText = (status) => {
  switch (status) {
    case 'Pending': return 'Chờ nhận'
    case 'Receiving': return 'Đang nhận'
    case 'Partial': return 'Nhận một phần'
    case 'Completed': return 'Đã hoàn tất'
    case 'Cancelled': return 'Đã hủy'
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
})
</script>

<style scoped>
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
  overflow: hidden;
}

.table-responsive {
  overflow-x: auto;
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

.order-name-text {
  font-size: 0.9rem;
  color: #1e293b;
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
</style>
