/**
 * Tiện ích xuất dữ liệu ra file CSV chuẩn UTF-8 có BOM (tương thích 100% với Microsoft Excel)
 */
export function exportToCsv(arg1, arg2, arg3) {
  let filename = 'Data_Export'
  let rows = []
  let columns = []

  // Hỗ trợ cả 2 chữ ký: (filename, rows, columns) hoặc (rows, filename, columns)
  if (typeof arg1 === 'string') {
    filename = arg1 || 'Data_Export'
    rows = Array.isArray(arg2) ? arg2 : []
    columns = Array.isArray(arg3) ? arg3 : []
  } else if (Array.isArray(arg1)) {
    rows = arg1
    filename = typeof arg2 === 'string' ? arg2 : 'Data_Export'
    columns = Array.isArray(arg3) ? arg3 : []
  }

  if (!rows || !rows.length) {
    alert('Không có dữ liệu để xuất file!')
    return
  }

  // Nếu không chỉ định columns, tự động lấy keys từ row đầu tiên
  if (!columns || !columns.length) {
    const keys = Object.keys(rows[0] || {})
    columns = keys.map(k => ({ label: k, field: k }))
  }

  // Header
  const header = columns.map(c => `"${(c.label || '').replace(/"/g, '""')}"`).join(',')

  // Body
  const csvRows = rows.map(row => {
    return columns.map(c => {
      let val = typeof c.field === 'function' ? c.field(row) : row[c.field]
      if (val === null || val === undefined) val = ''
      val = String(val).replace(/"/g, '""')
      return `"${val}"`
    }).join(',')
  })

  // Thêm BOM \uFEFF để Excel đọc chuẩn tiếng Việt có dấu
  const csvContent = '\uFEFF' + [header, ...csvRows].join('\r\n')
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', `${filename}_${new Date().toISOString().split('T')[0]}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}
