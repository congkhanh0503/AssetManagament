import axios from 'axios'

// Cấu hình URL API Backend
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://10.0.160.67:5000/api'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
})

// Request interceptor: Gắn Token xác thực nếu có & Tự động sinh boundary cho FormData
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('qlkho_auth_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  // Tự động xóa Content-Type cố định nếu payload là FormData để trình duyệt/axios tự gắn boundary multipart
  if (config.data instanceof FormData) {
    delete config.headers['Content-Type']
  }

  return config
})

// Response interceptor: Xử lý lỗi tập trung
apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    let message = error.response?.data?.message

    if (!message && error.response?.data?.errors) {
      const errObj = error.response.data.errors
      const errorList = []
      for (const key of Object.keys(errObj)) {
        if (Array.isArray(errObj[key])) {
          errorList.push(...errObj[key])
        } else if (typeof errObj[key] === 'string') {
          errorList.push(errObj[key])
        }
      }
      if (errorList.length > 0) {
        message = errorList.join(' | ')
      }
    }

    if (!message) {
      message = error.response?.data?.title || error.message || 'Đã xảy ra lỗi khi kết nối tới máy chủ'
    }

    console.error('[API Error]:', message, error)
    return Promise.reject(new Error(message))
  }
)

// ==========================================
// 1. DASHBOARD APIs (Served by IDashboardService)
// ==========================================
export const dashboardApi = {
  getKpis: () => apiClient.get('/dashboard/kpis'),
  getHighlights: (period = 'month') => apiClient.get(`/dashboard/highlights?period=${period}`),
  getBrokenHighlights: () => apiClient.get('/dashboard/broken-highlights'),
  getCategoryDistribution: () => apiClient.get('/dashboard/categories'),
  getDepartmentDistribution: () => apiClient.get('/dashboard/departments'),
  getBrandDefectStats: (month = 'all') => apiClient.get(`/dashboard/brand-defect-stats?month=${month}`),
  getTopHoldingEmployees: (limit = 6) => apiClient.get(`/dashboard/top-holding-employees?limit=${limit}`),
}

// ==========================================
// 2. ASSETS APIs (Served by IAssetService & IExportService)
// ==========================================
export const assetsApi = {
  getAll: (params = {}) => apiClient.get('/assets', { params }),
  getById: (id) => apiClient.get(`/assets/${id}`),
  create: (data) => apiClient.post('/assets', data),
  update: (id, data) => apiClient.put(`/assets/${id}`, data),
  delete: (id) => apiClient.delete(`/assets/${id}`),

  // Cấp phát trọn gói ACID (Main asset + Mouse + Keyboard + Monitor)
  bundleAssign: (data) => apiClient.post('/assets/bundle-assign', data),

  assign: (arg1, arg2) => {
    const payload = typeof arg1 === 'object' && arg1 !== null ? { ...arg1 } : { assetID: arg1, ...(arg2 || {}) }
    if (payload.employeeID && !payload.toEmployeeID) {
      payload.toEmployeeID = payload.employeeID
    }
    return apiClient.post('/assets/assign', payload)
  },
  return: (arg1, arg2) => {
    const payload = typeof arg1 === 'object' && arg1 !== null ? { ...arg1 } : { assetID: arg1, ...(arg2 || {}) }
    return apiClient.post('/assets/return', payload)
  },
  transfer: (arg1, arg2) => {
    const payload = typeof arg1 === 'object' && arg1 !== null ? { ...arg1 } : { assetID: arg1, ...(arg2 || {}) }
    return apiClient.post('/assets/transfer', payload)
  },
  reportIssue: (arg1, arg2) => {
    const payload = typeof arg1 === 'object' && arg1 !== null ? { ...arg1 } : { assetID: arg1, ...(arg2 || {}) }
    if (!payload.expectedReturnDate || payload.expectedReturnDate === '') {
      payload.expectedReturnDate = null
    }
    if (payload.estimatedCost === '' || payload.estimatedCost === null || isNaN(Number(payload.estimatedCost))) {
      payload.estimatedCost = null
    } else {
      payload.estimatedCost = Number(payload.estimatedCost)
    }
    if (!payload.status && payload.newStatus) {
      payload.status = payload.newStatus
    }
    if (!payload.status) {
      payload.status = 'Broken'
    }
    return apiClient.post('/assets/report-issue', payload)
  },

  // Nhập lô phụ kiện tự động sinh mã
  createAccessoryLot: (data) => apiClient.post('/assets/accessory-lot', data),
  getAccessoryLotPreview: (categoryId, brand) => apiClient.get('/assets/accessory-lot/preview', { params: { categoryId, brand } }),

  // Import / Export
  importBulk: (items) => apiClient.post('/assets/import-bulk', items),
  importHandover: (items) => apiClient.post('/assets/import-handover', items),
  exportCsvUrl: (params = {}) => {
    const query = new URLSearchParams(params).toString()
    return `/api/assets/export-csv${query ? '?' + query : ''}`
  },

  // Cảnh báo hạn bảo hành
  getWarrantyAlerts: async () => {
    try {
      const res = await apiClient.get('/assets')
      const list = Array.isArray(res) ? res : (res?.data || [])
      const now = new Date()
      return list.filter(a => a.warrantyExpireDate).map(a => {
        const exp = new Date(a.warrantyExpireDate)
        const diffDays = Math.ceil((exp - now) / (1000 * 60 * 60 * 24))
        return {
          assetID: a.assetID,
          assetCode: a.assetCode,
          assetName: a.assetName,
          supplierName: a.supplierName,
          holderName: a.holderName,
          warrantyExpireDate: a.warrantyExpireDate,
          daysRemaining: diffDays,
          isExpired: diffDays < 0
        }
      }).filter(a => a.daysRemaining <= 30)
    } catch (e) {
      return []
    }
  },

  // Lịch sử bàn giao
  getHandoverHistory: (params = {}) => apiClient.get('/assets/handover-history', { params }),
  exportHandoverHistoryCsvUrl: (params = {}) => {
    const query = new URLSearchParams(params).toString()
    return `/api/assets/handover-history/export-csv${query ? '?' + query : ''}`
  },
}

// ==========================================
// 3. EMPLOYEES APIs (Served by IEmployeeService & IExportService)
// ==========================================
export const employeesApi = {
  getAll: (params = {}) => apiClient.get('/employees', { params }),
  getById: (id) => apiClient.get(`/employees/${id}`),
  getAssets: (id) => apiClient.get(`/employees/${id}/assets`),
  create: (data) => apiClient.post('/employees', data),
  update: (id, data) => apiClient.put(`/employees/${id}`, data),
  delete: (id) => apiClient.delete(`/employees/${id}`),

  // Cảnh báo & Trạng thái tài khoản
  getLeaveAlerts: () => apiClient.get('/employees/alerts/leave'),
  getMissingHandoverAlerts: () => apiClient.get('/employees/alerts/missing-handover'),
  updateAccountStatus: (id, data) => apiClient.put(`/employees/${id}/account-status`, data),
  updateAccounts: (id, data) => apiClient.put(`/employees/${id}/account-status`, data),

  // Quản lý file PDF Biên bản bàn giao
  uploadHandoverPdf: (employeeId, assetId, formData) => {
    let fd = formData
    let aid = assetId
    if (assetId instanceof FormData) {
      fd = assetId
      aid = null
    }
    const query = aid ? `?assetId=${aid}` : ''
    return apiClient.post(`/employees/${employeeId}/handover-pdf${query}`, fd)
  },
  getHandoverPdfUrl: (employeeId, assetId = null) => {
    const query = assetId ? `?assetId=${assetId}` : ''
    return `/api/employees/${employeeId}/handover-pdf${query}`
  },
  deleteHandoverPdf: (employeeId, assetId = null) => {
    const query = assetId ? `?assetId=${assetId}` : ''
    return apiClient.delete(`/employees/${employeeId}/handover-pdf${query}`)
  },

  // Import / Export
  importBulk: (rows, updateExisting = false) => apiClient.post(`/employees/import-bulk?updateExisting=${updateExisting}`, rows),
  importAccounts: (data) => apiClient.post('/employees/import-accounts', data),
  exportCsvUrl: (params = {}) => {
    const query = new URLSearchParams(params).toString()
    return `/api/employees/export-csv${query ? '?' + query : ''}`
  },

  // Lịch sử biến động nhân sự
  getHistories: (params = {}) => apiClient.get('/employees/histories', { params }),
  createHistory: (data) => apiClient.post('/employees/histories', data),
  exportHistoriesCsvUrl: (params = {}) => {
    const query = new URLSearchParams(params).toString()
    return `/api/employees/histories/export-csv${query ? '?' + query : ''}`
  },
}

// ==========================================
// 4. DEPARTMENTS, CATEGORIES, SUPPLIERS & BRANDS APIs
// ==========================================
export const departmentsApi = {
  getAll: () => apiClient.get('/departments'),
  create: (data) => apiClient.post('/departments', data),
  update: (id, data) => apiClient.put(`/departments/${id}`, data),
  delete: (id) => apiClient.delete(`/departments/${id}`),
}

export const categoriesApi = {
  getAll: () => apiClient.get('/categories'),
  create: (data) => apiClient.post('/categories', data),
  update: (id, data) => apiClient.put(`/categories/${id}`, data),
  delete: (id) => apiClient.delete(`/categories/${id}`),
}

export const suppliersApi = {
  getAll: () => apiClient.get('/suppliers'),
  create: (data) => apiClient.post('/suppliers', data),
  update: (id, data) => apiClient.put(`/suppliers/${id}`, data),
  delete: (id) => apiClient.delete(`/suppliers/${id}`),
}

export const brandsApi = {
  getAll: () => apiClient.get('/brands'),
  getById: (id) => apiClient.get(`/brands/${id}`),
  create: (data) => apiClient.post('/brands', data),
  update: (id, data) => apiClient.put(`/brands/${id}`, data),
  delete: (id) => apiClient.delete(`/brands/${id}`),
}

// ==========================================
// 5. DOCUMENTS APIs
// ==========================================
export const documentsApi = {
  getAll: (params = {}) => apiClient.get('/documents', { params }),
  upload: (formData) => apiClient.post('/documents/upload', formData),
  delete: (id) => apiClient.delete(`/documents/${id}`),
  getDownloadUrl: (id) => `/api/documents/${id}/download`,
  getFileUrl: (filePath) => (filePath?.startsWith('http') ? filePath : (filePath?.startsWith('/') ? filePath : `/${filePath || ''}`))
}
