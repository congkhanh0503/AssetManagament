<template>
  <div>
    <!-- MODAL CHÍNH: XEM DANH SÁCH THIẾT BỊ ĐƯỢC CẤP PHÁT CỦA NHÂN VIÊN -->
    <Modal 
      :is-open="isOpen" 
      :title="`Thiết Bị Cấp Cho: ${employee?.fullName || ''} (${employee?.employeeCode || ''})`"
      :subtitle="`Phòng ban: ${employee?.departmentName || ''} • Tổng số: ${empAssetsData?.totalHeld || 0} thiết bị đang nắm giữ`"
      @close="$emit('close')"
      max-width="950px"
    >
      <div v-if="loading" class="table-loading">
        <span class="loading-spinner"></span>
        <p>Đang tải danh sách thiết bị...</p>
      </div>

      <div v-else>
        <!-- Banner cảnh báo phân quyền HR trong Modal Thiết Bị -->
        <div v-if="isHR" class="hr-readonly-modal-banner">
          🔒 <strong>Quyền Hạn Nhân Sự (HR):</strong> Bạn đang ở chế độ xem danh sách thiết bị. Nghiệp vụ cấp phát mới, điều chuyển, thu hồi và báo hỏng thiết bị do Quản Trị Viên (Admin) và bộ phận IT phụ trách.
        </div>

        <!-- Header Actions Bar trong Modal -->
        <div class="emp-assets-header-bar">
          <div class="emp-summary-badge">
            <span>👤 Nhân sự: <strong>{{ employee?.fullName }}</strong> ({{ employee?.title || 'Nhân viên' }})</span>
            <span class="badge-dept-tag">{{ employee?.departmentName }}</span>
          </div>
          <div style="display: flex; gap: 8px; align-items: center;" v-if="!isHR">
            <button type="button" class="btn btn-sm btn-primary" @click="openAssignModal">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                <line x1="12" y1="5" x2="12" y2="19"></line>
                <line x1="5" y1="12" x2="19" y2="12"></line>
              </svg>
              ➕ Cấp Phát Mới
            </button>
          </div>
        </div>

        <!-- Thanh Chọn Nhanh & In Gộp Thiết Bị Không Phải Laptop (Chỉ dành cho Admin/IT) -->
        <div v-if="!isHR && nonLaptopAssets.length > 0" class="bulk-select-bar">
          <label class="bulk-check-label">
            <input 
              type="checkbox" 
              :checked="isAllNonLaptopSelected" 
              @change="toggleSelectAllNonLaptop" 
              class="custom-chk-asset"
            />
            <span><strong>Chọn tất cả thiết bị không phải laptop</strong> ({{ nonLaptopAssets.length }} thiết bị)</span>
          </label>
          <div class="bulk-bar-right">
            <span class="badge-bulk-info" v-if="selectedAssetIdsForPrint.length > 0">
              Đã chọn <strong>{{ selectedAssetIdsForPrint.length }}</strong>/{{ nonLaptopAssets.length }} thiết bị
            </span>
            <button 
              type="button" 
              class="btn btn-sm btn-primary-gradient" 
              @click="$emit('print-bulk-non-laptop', selectedAssetIdsForPrint)"
              :disabled="selectedAssetIdsForPrint.length === 0"
              title="In biên bản bàn giao gộp các thiết bị đã chọn trên 1 tờ A4"
            >
              📄 In Biên Bản Gộp ({{ selectedAssetIdsForPrint.length }} món)
            </button>
          </div>
        </div>

        <div v-if="!empAssetsData?.assets || empAssetsData.assets.length === 0" class="empty-state" style="padding: 30px;">
          <p>Nhân viên này hiện chưa được bàn giao thiết bị nào.</p>
          <button v-if="!isHR" type="button" class="btn btn-primary" style="margin-top: 10px;" @click="openAssignModal">
            ➕ Cấp Phát Thiết Bị Ngay
          </button>
        </div>

        <div v-else class="emp-assets-list">
          <div 
            v-for="asset in empAssetsData.assets" 
            :key="asset.assetID" 
            class="emp-asset-card-enhanced"
            :class="{ 'is-selected-for-print': selectedAssetIdsForPrint.includes(asset.assetID) }"
          >
            <div class="asset-card-top">
              <div class="asset-left">
                <!-- Checkbox Slot Cố Định (Chỉ dành cho Admin/IT khi in gộp) -->
                <div v-if="!isHR" class="asset-checkbox-wrap">
                  <input 
                    v-if="!isLaptopCategory(asset.categoryID, asset.categoryName)"
                    type="checkbox" 
                    :value="asset.assetID" 
                    v-model="selectedAssetIdsForPrint" 
                    class="custom-chk-asset"
                    title="Tích chọn để in gộp vào biên bản thiết bị"
                  />
                  <span v-else class="laptop-icon-slot" title="Laptop cá nhân - In biên bản riêng">💻</span>
                </div>

                <span class="code-badge">{{ asset.assetCode }}</span>
                <strong class="asset-name-main">{{ asset.assetName }}</strong>
                <span class="asset-brand-tag">{{ asset.brand || asset.categoryName }}</span>
                <span 
                  v-if="!isLaptopCategory(asset.categoryID, asset.categoryName)" 
                  class="badge-can-bulk" 
                  title="Thiết bị này có thể in gộp chung trên 1 biên bản"
                >
                  🧩 Có thể in gộp
                </span>
                <span v-else class="badge-laptop-only" title="Laptop cá nhân in biên bản riêng">
                  Mẫu BG-PC
                </span>
              </div>
              <StatusBadge :status="asset.status" type="asset" />
            </div>

            <!-- Cấu hình máy tính / specs -->
            <div v-if="asset.specifications" class="asset-specs-highlight">
              ⚙️ <strong>Cấu hình:</strong> {{ asset.specifications }}
            </div>

            <div class="asset-specs-grid-row">
              <div class="spec-col"><span class="spec-lbl">Loại thiết bị:</span> <strong>{{ asset.categoryName }}</strong></div>
              <div class="spec-col"><span class="spec-lbl">Số S/N:</span> <span class="serial-text">{{ asset.serialNumber || '---' }}</span></div>
              <div class="spec-col"><span class="spec-lbl">Ngày cấp:</span> <span style="color: #a5b4fc;">{{ formatDate(asset.assignedDate) }}</span></div>
              <div class="spec-col" v-if="asset.supplierName"><span class="spec-lbl">Nhà cung cấp:</span> {{ asset.supplierName }}</div>
            </div>

            <div v-if="asset.note" class="asset-note-box">
              <span class="spec-lbl">Ghi chú máy:</span> {{ asset.note }}
            </div>

            <!-- KHỐI BIÊN BẢN BÀN GIAO PDF RIÊNG CHO TỪNG THIẾT BỊ -->
            <div class="asset-handover-pdf-section">
              <!-- Trường hợp ĐÃ CÓ biên bản PDF đính kèm -->
              <div v-if="asset.handoverDocument" class="pdf-attached-box">
                <div class="pdf-file-info">
                  <div class="pdf-badge-icon">
                    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: #ef4444;">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                      <polyline points="14 2 14 8 20 8"></polyline>
                      <line x1="9" y1="15" x2="15" y2="15"></line>
                    </svg>
                    <span class="pdf-tag">PDF</span>
                  </div>
                  <div class="pdf-meta">
                    <strong class="pdf-filename" :title="asset.handoverDocument.fileName">
                      {{ asset.handoverDocument.fileName }}
                    </strong>
                    <div class="pdf-sub-meta">
                      <span class="pdf-size">{{ asset.handoverDocument.fileSizeFormatted || 'PDF' }}</span>
                      <span class="dot-separator">•</span>
                      <span>{{ formatDate(asset.handoverDocument.createdAt) }}</span>
                    </div>
                  </div>
                </div>

                <div class="pdf-action-btns">
                  <!-- Nút Xem Trực Tiếp PDF -->
                  <button 
                    type="button" 
                    class="btn-pdf-act view"
                    @click="$emit('view-pdf', asset.handoverDocument)"
                    title="Xem trực tiếp biên bản PDF"
                  >
                    👁️ Xem PDF
                  </button>

                  <!-- Nút Tải Về PDF -->
                  <a 
                    :href="getPdfUrl(asset.handoverDocument.filePath)" 
                    target="_blank" 
                    :download="asset.handoverDocument.fileName"
                    class="btn-pdf-act download"
                    title="Tải file PDF về máy tính"
                  >
                    📥 Tải về
                  </a>

                  <!-- Nút Thay File PDF Khác (Chỉ dành cho Admin/IT) -->
                  <button 
                    v-if="!isHR"
                    type="button" 
                    class="btn-pdf-act replace"
                    @click="triggerPdfFileInput(asset.assetID)"
                    :disabled="uploadingAssetId === asset.assetID"
                    title="Thay thế bằng file scan PDF mới"
                  >
                    🔄 {{ uploadingAssetId === asset.assetID ? '...' : 'Thay file' }}
                  </button>

                  <!-- Nút Xóa File PDF (Chỉ dành cho Admin/IT) -->
                  <button 
                    v-if="!isHR"
                    type="button" 
                    class="btn-pdf-act delete"
                    @click="$emit('delete-pdf', asset)"
                    title="Xóa file PDF này"
                  >
                    🗑️
                  </button>
                </div>
              </div>

              <!-- Trường hợp CHƯA CÓ biên bản PDF -->
              <div v-else class="pdf-empty-upload-box">
                <div class="pdf-empty-left">
                  <span class="pdf-empty-icon">📄</span>
                  <div class="pdf-empty-texts">
                    <span class="pdf-empty-title">Biên bản bàn giao thiết bị (PDF)</span>
                    <span class="pdf-empty-sub">Chưa có file scan / PDF ký nhận cho thiết bị này</span>
                  </div>
                </div>

                <button 
                  v-if="!isHR"
                  type="button" 
                  class="btn-upload-pdf-action"
                  @click="triggerPdfFileInput(asset.assetID)"
                  :disabled="uploadingAssetId === asset.assetID"
                  title="Chọn file PDF để đính kèm vào thiết bị này"
                >
                  <span v-if="uploadingAssetId === asset.assetID" class="loading-spinner-sm"></span>
                  <span v-else>📎</span>
                  <span>{{ uploadingAssetId === asset.assetID ? 'Đang nạp...' : 'Import PDF' }}</span>
                </button>
                <span v-else class="pdf-tag" style="background: #f1f5f9; color: #64748b; font-size: 0.75rem;">
                  Chưa nạp PDF
                </span>
              </div>

              <!-- Input File Ẩn Riêng Cho Từng Thiết Bị -->
              <input 
                v-if="!isHR"
                type="file" 
                :ref="el => setPdfInputRef(asset.assetID, el)" 
                accept=".pdf,application/pdf" 
                style="display: none;" 
                @change="handlePdfSelected($event, asset.assetID)"
              />
            </div>

            <!-- THANH THAO TÁC NGHIỆP VỤ TRỰC TIẾP TRÊN THIẾT BỊ (CHỈ DÀNH CHO ADMIN / IT) -->
            <div v-if="!isHR" class="asset-item-actions-bar">
              <button 
                type="button" 
                class="btn-asset-act print"
                @click="$emit('print-single', asset)"
                title="In Biên Bản Bàn Giao (A4 / PDF) chuẩn form cho thiết bị này"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="6 9 6 2 18 2 18 9"></polyline>
                  <path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"></path>
                  <rect x="6" y="14" width="12" height="8"></rect>
                </svg>
                <span>In Biên Bản</span>
              </button>

              <button 
                type="button" 
                class="btn-asset-act transfer"
                @click="openTransferModal(asset)"
                title="Điều chuyển thiết bị này sang cho nhân viên khác"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="17 1 21 5 17 9"></polyline>
                  <path d="M3 11V9a4 4 0 0 1 4-4h14"></path>
                  <polyline points="7 23 3 19 7 15"></polyline>
                  <path d="M21 13v2a4 4 0 0 1-4 4H3"></path>
                </svg>
                <span>Điều Chuyển</span>
              </button>

              <button 
                type="button" 
                class="btn-asset-act return"
                @click="openReturnModal(asset)"
                title="Thu hồi thiết bị này về lại Kho IT"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="9 14 4 9 9 4"></polyline>
                  <path d="M20 20v-7a4 4 0 0 0-4-4H4"></path>
                </svg>
                <span>Thu Hồi Về Kho</span>
              </button>

              <button 
                type="button" 
                class="btn-asset-act broken"
                @click="openReportIssueModal(asset)"
                title="Báo hỏng hoặc gửi đi bảo trì/sửa chữa thiết bị này"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                  <line x1="12" y1="9" x2="12" y2="13"></line>
                  <line x1="12" y1="17" x2="12.01" y2="17"></line>
                </svg>
                <span>Báo Hỏng / Đi Sửa</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </Modal>

    <!-- SUB-MODAL 1: CẤP PHÁT THIẾT BỊ MỚI (ASSIGN) -->
    <Modal 
      :is-open="isAssignOpen" 
      title="Cấp Phát Thiết Bị Từ Kho"
      :subtitle="`Chọn thiết bị sẵn sàng trong kho để bàn giao cho ${employee?.fullName}`"
      @close="isAssignOpen = false"
      max-width="700px"
    >
      <div v-if="employee" class="emp-recipient-preview">
        <div class="emp-recipient-avatar">{{ getInitials(employee.fullName) }}</div>
        <div class="emp-recipient-info">
          <div class="emp-recipient-name-row">
            <span class="emp-recipient-label">👤 Người tiếp nhận:</span>
            <strong class="emp-recipient-name">{{ employee.fullName }}</strong>
            <span class="emp-recipient-code">{{ employee.employeeCode }}</span>
            <span class="emp-recipient-dept">{{ employee.departmentName }}</span>
          </div>
          <div class="emp-recipient-meta">
            <span v-if="employee.title">💼 Chức vụ: <strong>{{ employee.title }}</strong></span>
            <span>📦 Đang giữ: <strong>{{ empAssetsData?.totalHeld || 0 }} thiết bị</strong></span>
          </div>
        </div>
      </div>

      <form @submit.prevent="submitAssign" style="margin-top: 14px;">
        <div class="form-group">
          <label class="form-label">Chọn Thiết Bị Sẵn Sàng Trong Kho (Available) *</label>
          <AssetSelector 
            v-model="assignForm.assetID" 
            :assets="availableWarehouseAssets" 
            :categories="categories" 
          />
        </div>

        <!-- Chọn Chuột Kèm Theo Khi Cấp Laptop -->
        <div v-if="isAssignLaptop" class="form-group" style="margin-top: 14px; background: rgba(99, 102, 241, 0.05); padding: 12px; border-radius: var(--radius-md); border: 1px solid rgba(99, 102, 241, 0.2);">
          <label class="form-label" style="color: #4f46e5; display: flex; align-items: center; justify-content: space-between;">
            <span>🖱️ Chọn Chuột Kèm Theo Trong Kho (Tùy chọn)</span>
            <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">
              {{ availableWarehouseMice.length }} chuột sẵn có
            </span>
          </label>
          <select class="form-control" v-model="assignForm.mouseAssetID">
            <option :value="null">-- Không cấp chuột kèm theo --</option>
            <option v-for="m in availableWarehouseMice" :key="m.assetID" :value="m.assetID">
              🖱️ {{ m.assetCode }} - {{ m.assetName }} ({{ m.brand || '---' }}) {{ m.serialNumber ? `- S/N: ${m.serialNumber}` : '' }}
            </option>
          </select>
        </div>

        <!-- Chọn Phụ Kiện Khi Cấp Máy Tính Để Bàn (PC) -->
        <div v-if="isAssignDesktop" style="display: flex; flex-direction: column; gap: 12px; margin-top: 14px;">
          <div class="form-group" style="background: rgba(14, 165, 233, 0.06); padding: 12px; border-radius: var(--radius-md); border: 1px solid rgba(14, 165, 233, 0.25); margin-bottom: 0;">
            <label class="form-label" style="color: #0284c7; display: flex; align-items: center; justify-content: space-between;">
              <span>🖥️ 1. Chọn Màn Hình Kèm Theo (Tùy chọn)</span>
              <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">{{ availableWarehouseMonitors.length }} màn hình sẵn có</span>
            </label>
            <select class="form-control" v-model="assignForm.monitorAssetID">
              <option :value="null">-- Không cấp màn hình kèm theo --</option>
              <option v-for="mon in availableWarehouseMonitors" :key="mon.assetID" :value="mon.assetID">
                🖥️ {{ mon.assetCode }} - {{ mon.assetName }} ({{ mon.brand || '---' }}) {{ mon.serialNumber ? `- S/N: ${mon.serialNumber}` : '' }}
              </option>
            </select>
          </div>

          <div class="form-group" style="background: rgba(16, 185, 129, 0.06); padding: 12px; border-radius: var(--radius-md); border: 1px solid rgba(16, 185, 129, 0.25); margin-bottom: 0;">
            <label class="form-label" style="color: #059669; display: flex; align-items: center; justify-content: space-between;">
              <span>⌨️ 2. Chọn Bàn Phím Kèm Theo (Tùy chọn)</span>
              <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">{{ availableWarehouseKeyboards.length }} bàn phím sẵn có</span>
            </label>
            <select class="form-control" v-model="assignForm.keyboardAssetID">
              <option :value="null">-- Không cấp bàn phím kèm theo --</option>
              <option v-for="kb in availableWarehouseKeyboards" :key="kb.assetID" :value="kb.assetID">
                ⌨️ {{ kb.assetCode }} - {{ kb.assetName }} ({{ kb.brand || '---' }}) {{ kb.serialNumber ? `- S/N: ${kb.serialNumber}` : '' }}
              </option>
            </select>
          </div>

          <div class="form-group" style="background: rgba(99, 102, 241, 0.06); padding: 12px; border-radius: var(--radius-md); border: 1px solid rgba(99, 102, 241, 0.25); margin-bottom: 0;">
            <label class="form-label" style="color: #4f46e5; display: flex; align-items: center; justify-content: space-between;">
              <span>🖱️ 3. Chọn Chuột Kèm Theo (Tùy chọn)</span>
              <span style="font-size: 0.75rem; font-weight: normal; color: var(--text-dim);">{{ availableWarehouseMice.length }} chuột sẵn có</span>
            </label>
            <select class="form-control" v-model="assignForm.mouseAssetID">
              <option :value="null">-- Không cấp chuột kèm theo --</option>
              <option v-for="m in availableWarehouseMice" :key="m.assetID" :value="m.assetID">
                🖱️ {{ m.assetCode }} - {{ m.assetName }} ({{ m.brand || '---' }}) {{ m.serialNumber ? `- S/N: ${m.serialNumber}` : '' }}
              </option>
            </select>
          </div>
        </div>

        <div class="form-group" style="margin-top: 14px;">
          <label class="form-label">Tình trạng máy lúc bàn giao</label>
          <input type="text" class="form-control" v-model="assignForm.conditionStatus" placeholder="Vd: Mới 100%, Hoạt động tốt..." />
        </div>

        <div class="form-group">
          <label class="form-label">Ghi chú cấp phát</label>
          <textarea class="form-control" rows="2" v-model="assignForm.note" placeholder="Lý do cấp phát, phụ kiện kèm theo..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isAssignOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting || !assignForm.assetID">
            <span v-if="submitting" class="loading-spinner"></span>
            Xác Nhận Cấp Phát
          </button>
        </div>
      </form>
    </Modal>

    <!-- SUB-MODAL 2: ĐIỀU CHUYỂN (TRANSFER) -->
    <Modal 
      :is-open="isTransferOpen" 
      title="Điều Chuyển Thiết Bị"
      :subtitle="`Chuyển máy [${selectedOpAsset?.assetCode}] ${selectedOpAsset?.assetName} từ ${employee?.fullName} sang nhân sự khác`"
      @close="isTransferOpen = false"
      max-width="680px"
    >
      <form @submit.prevent="submitTransfer">
        <div class="form-group">
          <label class="form-label">Chọn Nhân Viên Tiếp Nhận Mới *</label>
          <EmployeeSelector 
            v-model="transferForm.toEmployeeID" 
            :employees="employees" 
            :departments="departments" 
            :exclude-employee-id="employee?.employeeID"
          />
        </div>

        <div class="form-group" style="margin-top: 14px;">
          <label class="form-label">Tình trạng máy khi bàn giao</label>
          <input type="text" class="form-control" v-model="transferForm.conditionStatus" placeholder="Vd: Hoạt động bình thường, máy sạch đẹp..." />
        </div>

        <div class="form-group">
          <label class="form-label">Lý do điều chuyển / Ghi chú</label>
          <textarea class="form-control" rows="2" v-model="transferForm.note" placeholder="Chuyển dự án, thay đổi phân công nhiệm vụ..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isTransferOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting || !transferForm.toEmployeeID">
            <span v-if="submitting" class="loading-spinner"></span>
            Xác Nhận Điều Chuyển
          </button>
        </div>
      </form>
    </Modal>

    <!-- SUB-MODAL 3: THU HỒI VỀ KHO (RETURN) -->
    <Modal 
      :is-open="isReturnOpen" 
      title="Thu Hồi Thiết Bị Về Kho IT"
      :subtitle="`Thu hồi máy [${selectedOpAsset?.assetCode}] ${selectedOpAsset?.assetName} từ ${employee?.fullName}`"
      @close="isReturnOpen = false"
      max-width="500px"
    >
      <form @submit.prevent="submitReturn">
        <div class="form-group">
          <label class="form-label">Vị trí lưu kho sau khi thu hồi *</label>
          <input type="text" class="form-control" v-model="returnForm.warehouseLocation" required placeholder="Vd: Kho IT - Kệ A1, Tủ A2..." />
        </div>

        <div class="form-group" style="display: flex; align-items: center; gap: 10px;">
          <input type="checkbox" id="isBrokenCheck" v-model="returnForm.isBroken" style="width: 18px; height: 18px; cursor: pointer;" />
          <label for="isBrokenCheck" style="color: #fca5a5; font-weight: 600; cursor: pointer;">
            Thiết bị bị hỏng / lỗi cần chuyển đi sửa chữa ngay
          </label>
        </div>

        <div class="form-group">
          <label class="form-label">Tình trạng thực tế lúc thu hồi</label>
          <input type="text" class="form-control" v-model="returnForm.conditionStatus" placeholder="Vd: Máy nguyên vẹn, đầy đủ sạc..." />
        </div>

        <div class="form-group">
          <label class="form-label">Ghi chú thêm</label>
          <textarea class="form-control" rows="2" v-model="returnForm.note" placeholder="Tình trạng bàn phím, pin, phụ kiện kèm theo..."></textarea>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isReturnOpen = false">Hủy</button>
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Xác Nhận Thu Hồi
          </button>
        </div>
      </form>
    </Modal>

    <!-- SUB-MODAL 4: BÁO HỎNG (REPORT ISSUE) -->
    <Modal 
      :is-open="isReportIssueOpen" 
      title="Báo Hỏng / Gửi Đi Bảo Trì Thiết Bị"
      :subtitle="`Thiết bị [${selectedOpAsset?.assetCode}] ${selectedOpAsset?.assetName} của ${employee?.fullName}`"
      @close="isReportIssueOpen = false"
      max-width="580px"
    >
      <form @submit.prevent="submitReportIssue">
        <div class="form-group">
          <label class="form-label">Mô Tả Sự Cố / Lỗi Chi Tiết *</label>
          <textarea class="form-control" rows="3" v-model="reportIssueForm.issueDescription" required placeholder="Vd: Vỡ màn hình, không lên nguồn, chai pin, lỗi bàn phím..."></textarea>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Tình Trạng Xử Lý</label>
            <select class="form-control" v-model="reportIssueForm.status">
              <option value="Broken">Bị hỏng (Lưu tại kho chờ sửa)</option>
              <option value="Maintenance">Đang gửi đi bảo hành / sửa chữa</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Đơn Vị Bảo Hành / Sửa Chữa</label>
            <input type="text" class="form-control" v-model="reportIssueForm.vendorName" placeholder="Vd: FPT Services, Dell Care..." />
          </div>
        </div>

        <div class="modal-grid-2">
          <div class="form-group">
            <label class="form-label">Chi Phí Dự Kiến (VNĐ)</label>
            <input type="number" class="form-control" v-model="reportIssueForm.estimatedCost" placeholder="0" />
          </div>

          <div class="form-group">
            <label class="form-label">Ngày Dự Kiến Trả</label>
            <input type="date" class="form-control" v-model="reportIssueForm.expectedReturnDate" />
          </div>
        </div>

        <div class="modal-actions-right">
          <button type="button" class="btn btn-secondary" @click="isReportIssueOpen = false">Hủy</button>
          <button type="submit" class="btn btn-danger" :disabled="submitting">
            <span v-if="submitting" class="loading-spinner"></span>
            Xác Nhận Báo Hỏng
          </button>
        </div>
      </form>
    </Modal>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import Modal from '@/components/common/Modal.vue'
import StatusBadge from '@/components/common/StatusBadge.vue'
import AssetSelector from '@/components/common/AssetSelector.vue'
import EmployeeSelector from '@/components/common/EmployeeSelector.vue'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  employee: { type: Object, default: null },
  empAssetsData: { type: Object, default: () => ({ assets: [], totalHeld: 0 }) },
  loading: { type: Boolean, default: false },
  isHR: { type: Boolean, default: false },
  categories: { type: Array, default: () => [] },
  employees: { type: Array, default: () => [] },
  departments: { type: Array, default: () => [] },
  warehouseAssets: { type: Array, default: () => [] },
  uploadingAssetId: { type: Number, default: null },
  submitting: { type: Boolean, default: false }
})

const emit = defineEmits([
  'close',
  'assign',
  'transfer',
  'return',
  'report-issue',
  'print-single',
  'print-bulk-non-laptop',
  'view-pdf',
  'upload-pdf',
  'delete-pdf'
])

const selectedAssetIdsForPrint = ref([])
const pdfInputRefs = ref({})

const setPdfInputRef = (assetId, el) => {
  if (el) pdfInputRefs.value[assetId] = el
}

const triggerPdfFileInput = (assetId) => {
  pdfInputRefs.value[assetId]?.click()
}

const handlePdfSelected = (e, assetId) => {
  const file = e.target.files?.[0]
  if (file) {
    emit('upload-pdf', { assetId, file })
    e.target.value = ''
  }
}

// Sub-modals state
const isAssignOpen = ref(false)
const isTransferOpen = ref(false)
const isReturnOpen = ref(false)
const isReportIssueOpen = ref(false)
const selectedOpAsset = ref(null)

const assignForm = reactive({
  assetID: null,
  mouseAssetID: null,
  monitorAssetID: null,
  keyboardAssetID: null,
  conditionStatus: 'Hoạt động tốt',
  note: ''
})

const transferForm = reactive({
  toEmployeeID: null,
  conditionStatus: 'Hoạt động bình thường',
  note: ''
})

const returnForm = reactive({
  warehouseLocation: 'Kho IT',
  isBroken: false,
  conditionStatus: 'Máy hoạt động tốt, đã vệ sinh',
  note: ''
})

const reportIssueForm = reactive({
  issueDescription: '',
  status: 'Broken',
  vendorName: '',
  estimatedCost: null,
  expectedReturnDate: ''
})

const isLaptopCategory = (categoryId, categoryName, assetCode = '', assetName = '') => {
  const cat = props.categories.find(c => c.categoryID === categoryId)
  const name = (cat?.categoryName || categoryName || '').toLowerCase().trim()
  const code = (cat?.categoryCode || '').toLowerCase().trim()
  const aCode = (assetCode || '').toLowerCase().trim()
  const aName = (assetName || '').toLowerCase().trim()

  if (
    aCode.includes('dt') || aCode.includes('pc') ||
    aName.includes('optiplex') || aName.includes('thinkcentre') || aName.includes('prodesk') || aName.includes('elitedesk') ||
    name.includes('để bàn') || name.includes('desktop')
  ) {
    return false
  }

  return name.includes('laptop') || 
         name.includes('xách tay') || 
         name.includes('xachtay') || 
         name.includes('notebook') || 
         name.includes('macbook') || 
         code.includes('laptop') || 
         code.includes('nb') ||
         aCode.includes('nb') || aCode.includes('lt')
}

const isDesktopCategory = (categoryId, categoryName, assetCode = '', assetName = '') => {
  const cat = props.categories.find(c => c.categoryID === categoryId)
  const name = (cat?.categoryName || categoryName || '').toLowerCase().trim()
  const code = (cat?.categoryCode || '').toLowerCase().trim()
  const aCode = (assetCode || '').toLowerCase().trim()
  const aName = (assetName || '').toLowerCase().trim()

  return name.includes('pc') || 
         name.includes('máy tính để bàn') || 
         name.includes('desktop') || 
         code.includes('dt') || 
         code.includes('pc') ||
         aCode.includes('dt') || 
         aCode.includes('pc') ||
         aName.includes('optiplex') || 
         aName.includes('thinkcentre') || 
         aName.includes('prodesk') || 
         aName.includes('elitedesk')
}

const nonLaptopAssets = computed(() => {
  if (!props.empAssetsData?.assets) return []
  return props.empAssetsData.assets.filter(a => !isLaptopCategory(a.categoryID, a.categoryName, a.assetCode, a.assetName))
})

const isAllNonLaptopSelected = computed(() => {
  return nonLaptopAssets.value.length > 0 && nonLaptopAssets.value.every(a => selectedAssetIdsForPrint.value.includes(a.assetID))
})

const toggleSelectAllNonLaptop = () => {
  if (isAllNonLaptopSelected.value) {
    selectedAssetIdsForPrint.value = []
  } else {
    selectedAssetIdsForPrint.value = nonLaptopAssets.value.map(a => a.assetID)
  }
}

const availableWarehouseAssets = computed(() => {
  return props.warehouseAssets.filter(a => a.status === 'Available')
})

const availableWarehouseMice = computed(() => {
  return availableWarehouseAssets.value.filter(a => {
    const name = (a.assetName || '').toLowerCase()
    const cat = (a.categoryName || '').toLowerCase()
    const code = (a.assetCode || '').toLowerCase()
    return name.includes('chuột') || name.includes('mouse') || cat.includes('chuột') || cat.includes('mouse') || code.includes('mou')
  })
})

const availableWarehouseMonitors = computed(() => {
  return availableWarehouseAssets.value.filter(a => {
    const name = (a.assetName || '').toLowerCase()
    const cat = (a.categoryName || '').toLowerCase()
    const code = (a.assetCode || '').toLowerCase()
    return name.includes('màn hình') || name.includes('monitor') || cat.includes('màn hình') || cat.includes('monitor') || code.includes('mn') || code.includes('mon')
  })
})

const availableWarehouseKeyboards = computed(() => {
  return availableWarehouseAssets.value.filter(a => {
    const name = (a.assetName || '').toLowerCase()
    const cat = (a.categoryName || '').toLowerCase()
    const code = (a.assetCode || '').toLowerCase()
    return name.includes('bàn phím') || name.includes('keyboard') || cat.includes('bàn phím') || cat.includes('keyboard') || code.includes('kb')
  })
})

const isAssignLaptop = computed(() => {
  if (!assignForm.assetID) return false
  const asset = availableWarehouseAssets.value.find(a => a.assetID === assignForm.assetID)
  return asset ? isLaptopCategory(asset.categoryID, asset.categoryName, asset.assetCode, asset.assetName) : false
})

const isAssignDesktop = computed(() => {
  if (!assignForm.assetID) return false
  const asset = availableWarehouseAssets.value.find(a => a.assetID === assignForm.assetID)
  return asset ? isDesktopCategory(asset.categoryID, asset.categoryName, asset.assetCode, asset.assetName) : false
})

const openAssignModal = () => {
  assignForm.assetID = null
  assignForm.mouseAssetID = null
  assignForm.monitorAssetID = null
  assignForm.keyboardAssetID = null
  assignForm.conditionStatus = 'Hoạt động tốt'
  assignForm.note = ''
  isAssignOpen.value = true
}

const submitAssign = () => {
  emit('assign', { ...assignForm })
  isAssignOpen.value = false
}

const openTransferModal = (asset) => {
  selectedOpAsset.value = asset
  transferForm.toEmployeeID = null
  transferForm.conditionStatus = 'Hoạt động bình thường'
  transferForm.note = ''
  isTransferOpen.value = true
}

const submitTransfer = () => {
  emit('transfer', { asset: selectedOpAsset.value, ...transferForm })
  isTransferOpen.value = false
}

const openReturnModal = (asset) => {
  selectedOpAsset.value = asset
  returnForm.warehouseLocation = 'Kho IT'
  returnForm.isBroken = false
  returnForm.conditionStatus = 'Máy hoạt động tốt, đã vệ sinh'
  returnForm.note = ''
  isReturnOpen.value = true
}

const submitReturn = () => {
  emit('return', { asset: selectedOpAsset.value, ...returnForm })
  isReturnOpen.value = false
}

const openReportIssueModal = (asset) => {
  selectedOpAsset.value = asset
  reportIssueForm.issueDescription = ''
  reportIssueForm.status = 'Broken'
  reportIssueForm.vendorName = ''
  reportIssueForm.estimatedCost = null
  reportIssueForm.expectedReturnDate = ''
  isReportIssueOpen.value = true
}

const submitReportIssue = () => {
  emit('report-issue', { asset: selectedOpAsset.value, ...reportIssueForm })
  isReportIssueOpen.value = false
}

const getInitials = (name) => {
  if (!name) return 'NV'
  const parts = name.trim().split(' ')
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  }
  return name.substring(0, 2).toUpperCase()
}

const formatDate = (dateStr) => {
  if (!dateStr) return '---'
  const d = new Date(dateStr)
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const getPdfUrl = (filePath) => {
  if (!filePath) return '#'
  if (filePath.startsWith('http://') || filePath.startsWith('https://')) return filePath
  return filePath.startsWith('/') ? filePath : `/${filePath}`
}
</script>
