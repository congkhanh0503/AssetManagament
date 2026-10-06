/**
 * Tiện ích Parse Excel (.xlsx, .xls, .csv) & Tải File Mẫu Import Cho Quản Lý Kho IT
 */
import ExcelJS from 'exceljs'

/**
 * Chuẩn hóa chuỗi tìm kiếm key (Fuzzy key matching)
 * Loại bỏ dấu tiếng Việt, ký tự đặc biệt, khoảng trắng, chuyển về chữ thường
 */
export function cleanKey(str) {
  if (!str) return ''
  return str
    .toString()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '') // Bỏ dấu tiếng Việt
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9]/g, '') // Chỉ giữ lại ký tự chữ và số
}

/**
 * Trích xuất giá trị từ dòng dữ liệu dựa trên danh sách alias tiêu đề cột linh hoạt
 */
export function getRowValue(row, possibleAliases) {
  if (!row || typeof row !== 'object') return ''

  // 1. Khớp chính xác key nguyên bản
  for (const alias of possibleAliases) {
    if (row[alias] !== undefined && row[alias] !== null && String(row[alias]).trim() !== '') {
      return String(row[alias]).trim()
    }
  }

  // Chuẩn bị danh sách key của row kèm cleanKey
  const rowKeys = Object.keys(row)
  const cleanedRowEntries = rowKeys.map(k => ({
    originalKey: k,
    cleanKey: cleanKey(k),
    value: row[k]
  })).filter(e => e.value !== undefined && e.value !== null && String(e.value).trim() !== '')

  // 2. Khớp Exact cleanKey match (ví dụ: "Host Name" vs "hostname", "Mã Máy" vs "mamay")
  for (const alias of possibleAliases) {
    const cAlias = cleanKey(alias)
    if (!cAlias) continue
    const matched = cleanedRowEntries.find(e => e.cleanKey === cAlias)
    if (matched) {
      return String(matched.value).trim()
    }
  }

  // 3. Khớp Substring match (ví dụ: header là "Mã Thiết Bị (Asset Code)*" -> cleanKey: "mathietbiassetcode"
  //    chứa alias "mathietbi" hoặc chứa alias "assetcode")
  for (const alias of possibleAliases) {
    const cAlias = cleanKey(alias)
    if (!cAlias || cAlias.length < 3) continue
    const matched = cleanedRowEntries.find(e => e.cleanKey.includes(cAlias) || cAlias.includes(e.cleanKey))
    if (matched) {
      return String(matched.value).trim()
    }
  }

  return ''
}

/**
 * Chuẩn hóa giá trị ngày tháng từ Excel/CSV về dạng YYYY-MM-DD
 */
export function normalizeDateValue(val) {
  if (!val) return ''
  if (val instanceof Date && !isNaN(val.getTime())) {
    return val.toISOString().split('T')[0]
  }
  const str = String(val).trim()
  if (!str) return ''
  if (str.includes('T')) return str.split('T')[0]

  // Định dạng YYYY-MM-DD hoặc YYYY/MM/DD
  const ymdMatch = str.match(/^(\d{4})[-/.](\d{1,2})[-/.](\d{1,2})$/)
  if (ymdMatch) {
    const y = ymdMatch[1]
    const m = String(ymdMatch[2]).padStart(2, '0')
    const d = String(ymdMatch[3]).padStart(2, '0')
    return `${y}-${m}-${d}`
  }

  // Định dạng DD/MM/YYYY hoặc DD-MM-YYYY hoặc MM/DD/YYYY (4 số năm)
  const dmyMatch = str.match(/^(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})$/)
  if (dmyMatch) {
    let p1 = Number(dmyMatch[1])
    let p2 = Number(dmyMatch[2])
    const y = dmyMatch[3]
    let m = p2
    let d = p1
    if (p1 <= 12 && p2 > 12) { // M/D/YYYY
      m = p1
      d = p2
    }
    return `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`
  }

  // Định dạng M/D/YY hoặc D/M/YY (2 số năm, ví dụ: 8/4/26)
  const shortMatch = str.match(/^(\d{1,2})[-/.](\d{1,2})[-/.](\d{2})$/)
  if (shortMatch) {
    let p1 = Number(shortMatch[1])
    let p2 = Number(shortMatch[2])
    const y = '20' + shortMatch[3]
    let m = p1
    let d = p2
    if (p1 > 12 && p2 <= 12) { // D/M/YY
      d = p1
      m = p2
    }
    return `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`
  }

  // Nếu là số serial của Excel (VD: 45529)
  const num = Number(str)
  if (!isNaN(num) && num > 20000 && num < 70000) {
    const excelEpoch = new Date(1899, 11, 30)
    const dateObj = new Date(excelEpoch.getTime() + num * 86400000)
    if (!isNaN(dateObj.getTime())) {
      return dateObj.toISOString().split('T')[0]
    }
  }

  // Thử parse qua Date object của JS
  const parsed = new Date(str)
  if (!isNaN(parsed.getTime()) && parsed.getFullYear() > 1990 && parsed.getFullYear() < 2100) {
    return parsed.toISOString().split('T')[0]
  }

  return ''
}

/**
 * Tự động nhận diện mã hóa (Encoding) và đọc text tiếng Việt không bị lỗi font
 */
export async function readFileAsTextSmart(file) {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)

  // 1. Kiểm tra UTF-8 BOM
  if (bytes.length >= 3 && bytes[0] === 0xEF && bytes[1] === 0xBB && bytes[2] === 0xBF) {
    return new TextDecoder('utf-8').decode(bytes.subarray(3))
  }

  // 2. Thử decode bằng UTF-8 nghiêm ngặt (fatal: true)
  try {
    const text = new TextDecoder('utf-8', { fatal: true }).decode(bytes)
    // Nếu có ký tự thay thế unicode, thử tiếp windows-1258
    if (!text.includes('\uFFFD')) {
      return text
    }
  } catch (e) {
    // Không phải UTF-8 hợp lệ
  }

  // 3. Thử decode bằng Windows-1258 (Mã tiếng Việt mặc định của Microsoft Excel trên Windows)
  try {
    const textWin1258 = new TextDecoder('windows-1258').decode(bytes)
    if (!textWin1258.includes('\uFFFD')) {
      return textWin1258
    }
  } catch (e) {}

  // 4. Thử decode bằng Windows-1252
  try {
    const textWin1252 = new TextDecoder('windows-1252').decode(bytes)
    return textWin1252
  } catch (e) {}

  // 5. Fallback cuối cùng
  return new TextDecoder('utf-8').decode(bytes)
}

/**
 * Đọc và parse mọi loại file bảng tính: .xlsx, .xls, .csv, .txt
 * Tự động đọc dữ liệu chuẩn xác dù người dùng lưu bằng Excel hay CSV
 */
export async function parseSpreadsheetFile(file) {
  if (!file) throw new Error('Chưa chọn file bảng tính')

  const fileName = file.name.toLowerCase()

  // Trường hợp 1: File Excel nhị phân (.xlsx, .xls)
  if (fileName.endsWith('.xlsx') || fileName.endsWith('.xls')) {
    const buffer = await file.arrayBuffer()
    const workbook = new ExcelJS.Workbook()
    await workbook.xlsx.load(buffer)

    const worksheet = workbook.worksheets[0]
    if (!worksheet || worksheet.rowCount < 2) {
      return { headers: [], rows: [] }
    }

    // Từ khóa nhận diện các cột tiêu đề quen thuộc
    const HEADER_KEYWORDS = [
      'ma', 'code', 'hoten', 'hovaten', 'fullname', 'name', 'ten',
      'email', 'mail', 'bophan', 'phongban', 'department', 'dept',
      'chucdanh', 'chucvu', 'title', 'position', 'status', 'trangthai', 'stt',
      'thietbi', 'taisan', 'asset', 'serial', 'seri', 'servicetag', 'model', 'account'
    ]

    // Tự động tìm dòng tiêu đề (Header Row) thông minh từ dòng 1 đến dòng 10
    let detectedHeaderRow = 1
    let maxMatchCount = 0

    const maxCheckRows = Math.min(10, worksheet.rowCount)
    for (let r = 1; r <= maxCheckRows; r++) {
      const row = worksheet.getRow(r)
      let matchCount = 0
      const seenText = new Set()

      row.eachCell({ includeEmpty: false }, (cell) => {
        const text = cell.text ? cell.text.trim() : ''
        if (!text) return
        const cKey = cleanKey(text)
        if (seenText.has(cKey)) return
        seenText.add(cKey)

        if (HEADER_KEYWORDS.some(kw => cKey.includes(kw))) {
          matchCount++
        }
      })

      if (matchCount > maxMatchCount && matchCount >= 2) {
        maxMatchCount = matchCount
        detectedHeaderRow = r
      }
    }

    const headers = []
    const headerRow = worksheet.getRow(detectedHeaderRow)
    const headerOccurrences = new Map()

    headerRow.eachCell({ includeEmpty: false }, (cell, colNumber) => {
      let title = cell.text ? cell.text.trim() : `Col_${colNumber}`
      const cTitle = cleanKey(title)

      const occ = headerOccurrences.get(cTitle) || 0
      headerOccurrences.set(cTitle, occ + 1)

      // Nếu có cột Họ và tên xuất hiện lần thứ 2, tự động gán thành Tên tiếng Anh
      if (occ > 0) {
        if (cTitle.includes('hovaten') || cTitle.includes('hoten') || cTitle === 'ten' || cTitle === 'name') {
          title = 'Tên tiếng Anh (English Name)'
        } else {
          title = `${title}_${occ + 1}`
        }
      }

      headers[colNumber] = title
    })

    const rows = []
    worksheet.eachRow((row, rowNumber) => {
      if (rowNumber <= detectedHeaderRow) return // Bỏ qua tất cả các dòng trước và tại header

      const rowObj = {}
      let hasData = false
      row.eachCell({ includeEmpty: true }, (cell, colNumber) => {
        const header = headers[colNumber] || `Col_${colNumber}`
        let val = cell.text !== undefined && cell.text !== null ? cell.text : cell.value
        if (typeof val === 'object' && val !== null) {
          val = val.result || val.text || JSON.stringify(val)
        }
        val = val ? String(val).trim() : ''
        if (val) hasData = true
        rowObj[header] = val
      })

      if (hasData) {
        rows.push(rowObj)
      }
    })

    return { headers: headers.filter(Boolean), rows }
  }

  // Trường hợp 2: File văn bản (.csv, .txt)
  try {
    let text = await readFileAsTextSmart(file)
    // Bỏ ký tự BOM nếu có
    if (text.charCodeAt(0) === 0xFEFF) {
      text = text.slice(1)
    }

    const lines = text.split(/\r\n|\n/).filter(line => line.trim().length > 0)
    if (lines.length === 0) {
      return { headers: [], rows: [] }
    }

    // Tự động nhận diện phân cách bằng cách đếm tần suất xuất hiện trên dòng header
    const firstLine = lines[0]
    const countComma = (firstLine.match(/,/g) || []).length
    const countSemicolon = (firstLine.match(/;/g) || []).length
    const countTab = (firstLine.match(/\t/g) || []).length

    let separator = ','
    if (countTab > countComma && countTab > countSemicolon) {
      separator = '\t'
    } else if (countSemicolon > countComma) {
      separator = ';'
    }

    const parseLine = (line) => {
      const result = []
      let cur = ''
      let inQuotes = false
      for (let i = 0; i < line.length; i++) {
        const char = line[i]
        if (char === '"') {
          if (inQuotes && line[i + 1] === '"') {
            cur += '"'
            i++
          } else {
            inQuotes = !inQuotes
          }
        } else if (char === separator && !inQuotes) {
          result.push(cur.trim())
          cur = ''
        } else {
          cur += char
        }
      }
      result.push(cur.trim())
      return result
    }

    const headers = parseLine(lines[0])
    const rows = []

    for (let i = 1; i < lines.length; i++) {
      const values = parseLine(lines[i])
      if (values.length === 0 || values.every(v => v === '')) continue

      const rowObj = {}
      let hasData = false
      headers.forEach((h, index) => {
        const val = values[index] !== undefined ? values[index].trim() : ''
        if (val) hasData = true
        rowObj[h] = val
      })
      if (hasData) {
        rows.push(rowObj)
      }
    }

    return { headers, rows }
  } catch (err) {
    throw err
  }
}

// Alias tương thích
export const parseCsvFile = parseSpreadsheetFile

/**
 * 1. Tải Mẫu Excel Import Riêng Cho Laptop & PC (Máy Tính) - File .xlsx & .csv
 */
export async function downloadComputerTemplate(asXlsx = true) {
  const todayStr = new Date().toISOString().split('T')[0]
  const warrantyStr = new Date(Date.now() + 3 * 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]

  const headers = [
    'Host Name (Mã Máy)*',
    'Model (Tên Thiết Bị)*',
    'Loại Thiết Bị',
    'Hãng',
    'Service Tag (Số Serial)*',
    'Asset Number (Mã Tài Sản)',
    'CPU',
    'RAM',
    'Disk (Ổ Cứng)',
    'OS (Hệ Điều Hành)',
    'Display (Màn Hình)',
    'Keyboard',
    'Củ Sạc & Phụ Kiện',
    'Ngày Mua (Purchase Date)',
    'Hạn Bảo Hành (Warranty Expire)',
    'Vị Trí Kho',
    'Nhà Cung Cấp'
  ]

  const samples = [
    ['TTH-NB0065', 'HP Elitebook 640 G11', 'Laptop', 'HP', '5CD5183KKV', 'VT-HP-640', 'Core Ultra 5-125U', '16 GB', '512 GB SSD', 'Windows 11', '14-inch', 'Theo máy (Built-in)', 'Kèm củ sạc + Dây nguồn zin', todayStr, warrantyStr, 'Kho IT - Kệ A1', 'FPT Services'],
    ['TTH-DT0012', 'Dell OptiPlex 7010', 'Máy tính để bàn', 'Dell', '7GK9012', 'VT-DELL-7010', 'Core i7-13700', '32 GB', '1 TB SSD', 'Windows 11 Pro', '24-inch', 'Bàn phím có dây Dell', 'Kèm dây nguồn zin', todayStr, warrantyStr, 'Kho IT - Kệ B2', 'Dell Vietnam']
  ]

  if (asXlsx) {
    await exportToExcelFile('Mau_Import_Laptop_PC.xlsx', 'Laptop_PC', headers, samples)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + samples.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, 'Mau_Import_Laptop_PC.csv', 'text/csv;charset=utf-8;')
  }
}

/**
 * 2. Tải Mẫu Excel Import Cho Thiết Bị Khác (Chuột, Màn Hình, Bàn Phím, Máy In, Switch...)
 */
export async function downloadPeripheralTemplate(asXlsx = true) {
  const todayStr2 = new Date().toISOString().split('T')[0]
  const warrantyStr2 = new Date(Date.now() + 2 * 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]

  const headers = [
    'Mã Thiết Bị (Asset Code)*',
    'Tên Thiết Bị (Asset Name)*',
    'Loại Thiết Bị (Category)*',
    'Thương Hiệu (Brand)',
    'Số Serial (S/N)',
    'Mã Tài Sản / Vật Tư',
    'Thông Số Kỹ Thuật (Specifications)',
    'Ngày Mua (Purchase Date)',
    'Hạn Bảo Hành (Warranty Expire)',
    'Vị Trí Kho',
    'Nhà Cung Cấp',
    'Ghi Chú / Phụ Kiện Kèm Theo'
  ]

  const samples = [
    ['AST-MOU-001', 'Chuột không dây Logitech M185', 'Chuột', 'Logitech', 'SN-MOU-8891', 'VT-MOU-185', 'Không dây 2.4GHz, Pin AA, 1000 DPI', todayStr2, warrantyStr2, 'Kho IT - Kệ C1', 'Phong Vũ', 'Kèm đầu USB Receiver + 1 Pin AA'],
    ['AST-MON-001', 'Màn hình Dell UltraSharp U2422H', 'Màn hình', 'Dell', 'CN-0K791T-74261', 'VT-MON-2422', '24-inch FHD IPS 60Hz 100% sRGB', todayStr2, warrantyStr2, 'Kho IT - Kệ A3', 'Dell Vietnam', 'Kèm chân đế xoay, cáp nguồn, cáp DisplayPort'],
    ['AST-KB-001', 'Bàn phím không dây Logitech K380', 'Bàn phím', 'Logitech', 'SN-KB-9921', 'VT-KB-380', 'Bluetooth đa thiết bị (Easy-Switch)', todayStr2, warrantyStr2, 'Kho IT - Kệ C1', 'Phong Vũ', 'Kèm 2 pin AAA'],
    ['AST-PRN-001', 'Máy in Canon LBP 2900', 'Máy in', 'Canon', 'CN-PRN-1002', 'VT-PRN-2900', 'In laser trắng đen A4, 12 trang/phút', todayStr2, warrantyStr2, 'Phòng Hành Chính', 'Lê Bảo Minh', 'Kèm hộp mực cartridge 303 zin + cáp USB']
  ]

  if (asXlsx) {
    await exportToExcelFile('Mau_Import_Thiet_Bi_Ngoai_Vi_Khac.xlsx', 'Thiet_Bi_Khac', headers, samples)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + samples.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, 'Mau_Import_Thiet_Bi_Ngoai_Vi_Khac.csv', 'text/csv;charset=utf-8;')
  }
}

// Giữ lại alias để tương thích
export function downloadAssetTemplate() {
  downloadComputerTemplate(true)
}

/**
 * 3. Tải Mẫu Import Nhân Sự
 */
export async function downloadEmployeeTemplate(asXlsx = true) {
  const headers = [
    'Mã Nhân Viên', 
    'Họ và Tên*', 
    'Tên Tiếng Anh (English Name)',
    'Phòng Ban', 
    'Chức Danh', 
    'Email (Địa chỉ Email)', 
    'Số Điện Thoại', 
    'Ngày Vào Làm (YYYY-MM-DD)', 
    'Tài Khoản QAD', 
    'Tài Khoản OA', 
    'Trạng Thái Email Cty (Available/Disable)', 
    'Active Directory (AD)'
  ]
  const todayStr = new Date().toISOString().split('T')[0]
  const nextWeekStr = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]

  const samples = [
    ['EMP010', 'Hoàng Văn Cường', 'David Hoang', 'Phòng Công Nghệ Thông Tin', 'Software Engineer', 'cuong.hoang@company.com', '0912345678', todayStr, 'Available', 'Available', 'Available', 'Available'],
    ['EMP011', 'Phạm Minh Trang', 'Trang Pham', 'Phòng Hành Chính Nhân Sự', 'HR Officer', 'trang.pham@company.com', '0987654321', nextWeekStr, 'Available', 'Disable', 'Available', 'Disable']
  ]

  if (asXlsx) {
    await exportToExcelFile('Mau_Import_Nhan_Su.xlsx', 'Nhan_Su', headers, samples)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + samples.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, 'Mau_Import_Nhan_Su.csv', 'text/csv;charset=utf-8;')
  }
}

/**
 * 4. Xuất Danh Sách Nhân Sự Mới Hiện Tại Ra File Excel Chuẩn Để Điền/Cập Nhật Email
 */
export async function exportOnboardingEmailTemplate(employees, asXlsx = true) {
  const headers = [
    'Mã Nhân Viên', 
    'Họ và Tên*', 
    'Tên Tiếng Anh (English Name)',
    'Phòng Ban', 
    'Chức Danh', 
    'Email (Địa chỉ Email mới)*', 
    'Số Điện Thoại', 
    'Ngày Vào Làm (YYYY-MM-DD)'
  ]

  const rows = (employees || []).map(emp => [
    emp.employeeCode || '',
    emp.fullName || '',
    emp.englishName || '',
    emp.departmentName || '',
    emp.title || 'Nhân viên',
    emp.email || '',
    emp.phone || '',
    emp.joinDate ? emp.joinDate.split('T')[0] : ''
  ])

  const todayStr = new Date().toISOString().split('T')[0]
  const filename = `Danh_Sach_Nhan_Su_Moi_Dien_Email_${todayStr}.xlsx`

  if (asXlsx) {
    await exportToExcelFile(filename, 'Nhan_Su_Moi', headers, rows)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + rows.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, `Danh_Sach_Nhan_Su_Moi_Dien_Email_${todayStr}.csv`, 'text/csv;charset=utf-8;')
  }
}

/**
 * Xuất file .xlsx chuyên nghiệp bằng ExcelJS (Có styling, border, auto width)
 */
async function exportToExcelFile(filename, sheetName, headers, rows) {
  const workbook = new ExcelJS.Workbook()
  workbook.creator = 'QLKho System'
  workbook.created = new Date()

  const worksheet = workbook.addWorksheet(sheetName)

  // Thêm dòng header
  const headerRow = worksheet.addRow(headers)
  headerRow.font = { bold: true, color: { argb: 'FFFFFFFF' }, size: 11 }
  headerRow.fill = {
    type: 'pattern',
    pattern: 'solid',
    fgColor: { argb: 'FF4F46E5' } // Màu tím chàm Indigo hiện đại
  }
  headerRow.alignment = { vertical: 'middle', horizontal: 'center', wrapText: true }
  headerRow.height = 28

  // Thêm các dòng dữ liệu mẫu
  rows.forEach(r => {
    const row = worksheet.addRow(r)
    row.height = 22
    row.alignment = { vertical: 'middle', horizontal: 'left' }
  })

  // Căn chỉnh độ rộng cột tự động
  worksheet.columns.forEach((column, i) => {
    let maxLen = headers[i] ? headers[i].length : 12
    rows.forEach(r => {
      const val = r[i] ? String(r[i]) : ''
      if (val.length > maxLen) maxLen = val.length
    })
    column.width = Math.min(Math.max(maxLen + 4, 15), 45)
  })

  // Viền bảng (Borders)
  worksheet.eachRow((row) => {
    row.eachCell((cell) => {
      cell.border = {
        top: { style: 'thin', color: { argb: 'FFE2E8F0' } },
        left: { style: 'thin', color: { argb: 'FFE2E8F0' } },
        bottom: { style: 'thin', color: { argb: 'FFE2E8F0' } },
        right: { style: 'thin', color: { argb: 'FFE2E8F0' } }
      }
    })
  })

  const buffer = await workbook.xlsx.writeBuffer()
  const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', filename)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

function downloadFileBlob(content, filename, mimeType = 'text/csv;charset=utf-8;') {
  const blob = new Blob([content], { type: mimeType })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', filename)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * 5. Parse File Excel Bảng Cấp Phát / Bàn Giao Thiết Bị Cho Nhân Viên
 */
export async function parseHandoverImportExcel(file) {
  const { headers, rows } = await parseSpreadsheetFile(file)
  if (!rows || rows.length === 0) {
    return { headers, rows: [], summary: { total: 0, assigned: 0, spare: 0, invalid: 0 } }
  }

  const parsedItems = []
  let assignedCount = 0
  let spareCount = 0
  let invalidCount = 0

  for (let i = 0; i < rows.length; i++) {
    const r = rows[i]
    
    const assetCode = getRowValue(r, [
      'COMPUTER NAME', 'Computer Name', 'Host Name', 'Mã máy', 'Tên máy', 
      'Asset Code', 'Mã thiết bị', 'Computer'
    ])
    const serialNumber = getRowValue(r, [
      'S/N', 'SN', 'Serial Number', 'Serial', 'Service Tag', 'Số serial'
    ])
    const materialCode = getRowValue(r, [
      'Mã tài sản', 'Asset Number', 'Mã TS', 'Số thẻ tài sản', 'Tag', 'Mã vật tư'
    ])
    
    // Ưu tiên cột "Cấp cho Mã NV", nếu không có thì lấy cột "Mã NV"
    let employeeCode = getRowValue(r, ['Cấp cho Mã NV', 'Cap cho Ma NV'])
    if (!employeeCode) {
      employeeCode = getRowValue(r, ['Mã NV', 'Ma NV', 'Mã nhân viên', 'Employee Code', 'Staff ID', 'Emp Code'])
    }

    const employeeName = getRowValue(r, [
      'Họ và tên\n姓名', 'Họ và tên', 'Họ tên', 'Tên nhân viên', 'Full Name', 'Name', '姓名'
    ])
    const departmentName = getRowValue(r, [
      'Bộ phận\n部门', 'Bộ phận', 'Phòng ban', 'Department', 'Dept', '部门'
    ])
    const statusRaw = getRowValue(r, [
      'Tình trạng', 'Status', 'Trạng thái', 'Tình trạng bàn giao'
    ])
    const specifications = getRowValue(r, [
      'Cấu hình\n配置', 'Cấu hình', 'Specs', 'Specifications', '配置'
    ])
    const accountAd = getRowValue(r, [
      'Account', 'Tài khoản AD', 'AD Account', 'AD', 'Domain Account'
    ])
    const type = getRowValue(r, ['Type', 'Loại', 'Loại máy', 'Phân loại'])
    const email = getRowValue(r, ['Email', 'Mail', 'Địa chỉ email'])

    // Dòng hợp lệ phải có ít nhất Computer Name hoặc S/N hoặc Mã tài sản
    const hasIdentifier = Boolean(assetCode || serialNumber || materialCode)
    if (!hasIdentifier) {
      invalidCount++
      continue
    }

    const statusUpper = (statusRaw || '').toUpperCase()
    const isSpare = statusUpper.includes('SPARE') || statusUpper.includes('DỰ PHÒNG') || statusUpper.includes('AVAILABLE')

    if (isSpare) {
      spareCount++
    } else {
      assignedCount++
    }

    parsedItems.push({
      rowIndex: i + 1,
      assetCode,
      serialNumber,
      materialCode,
      employeeCode,
      employeeName,
      departmentName,
      email,
      status: isSpare ? 'SPARE' : 'ĐÃ CẤP',
      specifications,
      accountAd,
      type: type || 'Laptop',
      isSpare,
      hasEmployee: Boolean(employeeCode || employeeName)
    })
  }

  return {
    headers,
    items: parsedItems,
    summary: {
      total: parsedItems.length,
      assigned: assignedCount,
      spare: spareCount,
      invalid: invalidCount
    }
  }
}

/**
 * 6. Tải Mẫu File Excel Cấp Phát / Bàn Giao Thiết Bị
 */
export async function downloadHandoverTemplate(asXlsx = true) {
  const headers = [
    'STT',
    'Type',
    'Mã NV*',
    'Họ và tên*',
    'Email',
    'COMPUTER NAME (Mã Máy)*',
    'Bộ phận',
    'INTERNET',
    'DOMAIN',
    'Cấu hình',
    'S/N (Serial No.)*',
    'Account',
    'Mã tài sản (VNIT#)',
    'Cấp cho Mã NV',
    'Tình trạng (ĐÃ CẤP / SPARE)*'
  ]

  const samples = [
    [1, 'Laptop', 'CBS0015', 'Nguyễn Công Sỏi', 'Jack.soi@amphenol-cbs.com', 'TTH-NB0001', 'IT', 'YES', 'YES', 'HP elitebook 640 G11, Core Ultra 5-125U, 16GB, 512GB, 14inch', '5CD534DZP5', 'AP\\', 'VNIT#0001', 'CBS0015', 'ĐÃ CẤP'],
    [2, 'Laptop', '', '', '', 'TTH-NB0002', '', 'YES', 'YES', 'HP elitebook 640 G11, Core Ultra 5-125U, 16GB, 512GB, 14inch', '5CD5183JTG', '', 'VNIT#0002', '', 'SPARE'],
    [3, 'Desktop', 'CBS0028', 'Hà Thị Hằng', 'hana.hang@amphenol-cbs.com', 'TTH-PC0004', 'ME', 'YES', 'YES', 'Dell OptiPlex 7010, Core i7-13700, 16GB, 512GB', '7GK9012', 'AP\\HangH', 'VNIT#0004', 'CBS0028', 'ĐÃ CẤP']
  ]

  if (asXlsx) {
    await exportToExcelFile('Mau_Danh_Sach_Cap_Phat_Thiet_Bi.xlsx', 'Cap_Phat', headers, samples)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + samples.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, 'Mau_Danh_Sach_Cap_Phat_Thiet_Bi.csv', 'text/csv;charset=utf-8;')
  }
}

/**
 * 7. Parse File Excel Danh Sách Tài Khoản Nhân Viên Được Kích Hoạt / Audit
 */
export async function parseAccountImportExcel(file) {
  const { headers, rows } = await parseSpreadsheetFile(file)
  if (!rows || rows.length === 0) {
    return { 
      headers, 
      items: [], 
      summary: { total: 0, active: 0, left: 0, qadActive: 0, qadDisabled: 0, hasTermDate: 0 } 
    }
  }

  const parsedItems = []
  let activeCount = 0
  let leftCount = 0
  let qadActiveCount = 0
  let qadDisabledCount = 0
  let termDateCount = 0

  for (let i = 0; i < rows.length; i++) {
    const r = rows[i]

    const rawName = getRowValue(r, [
      'Name', 'Tên nhân viên', 'Tên tài khoản', 'Họ và tên', 'FullName', 'Employee Name'
    ])
    
    // Bỏ qua dòng trống không có tên
    if (!rawName) continue

    const statusRaw = getRowValue(r, ['Status', 'Trạng thái', 'Tình trạng', 'Tình trạng làm việc']) || 'Active'
    const termDateRaw = getRowValue(r, [
      'Termination date', 'Termination Date', 'Terminationdate', 'Ngày nghỉ việc', 'Leave Date', 'LeaveDate'
    ])
    const adDisabledRaw = getRowValue(r, [
      'AD account disabled?', 'AD account disabled', 'AD Disabled?', 'AD Disabled', 'AD'
    ]) || 'Y'
    const qadDisabledRaw = getRowValue(r, [
      'QAD account disabled?', 'QAD account disabled', 'QAD Disabled?', 'QAD Disabled', 'QAD'
    ]) || 'N'
    const note = getRowValue(r, ['Note', 'Ghi chú', 'Notes', 'Lý do'])
    const nextSteps = getRowValue(r, ['Next steps', 'Next Steps', 'Bước tiếp theo'])

    // Chuẩn hóa ngày nghỉ việc
    let terminationDate = null
    if (termDateRaw && termDateRaw !== 'N/A' && termDateRaw !== 'null' && termDateRaw !== '-') {
      const norm = normalizeDateValue(termDateRaw)
      terminationDate = norm ? norm : null
    }

    const isLeft = statusRaw.toLowerCase().includes('left') || statusRaw.toLowerCase().includes('nghỉ') || Boolean(terminationDate)
    if (isLeft) leftCount++
    else activeCount++

    const qadUpper = (qadDisabledRaw || '').toUpperCase().trim()
    const adUpper = (adDisabledRaw || 'Y').toUpperCase().trim()

    // Quy ước chuẩn: Y = Kích hoạt / Có tài khoản, N = Chưa kích hoạt / Không có
    const isQadActive = qadUpper === 'Y'
    const isAdActive = adUpper === 'Y'

    if (isQadActive) qadActiveCount++
    else qadDisabledCount++

    if (terminationDate) termDateCount++

    // Tách họ và tên tiếng Anh hiển thị
    let displayName = rawName
    let englishName = ''
    const cleaned = rawName.replace(/^TTHDis-/i, '').replace(/\(.*?\)/g, '').replace(/-ADM$/i, '').trim()
    const parts = cleaned.split(',').map(p => p.trim())
    if (parts.length === 2) {
      displayName = `${parts[0]} ${parts[1]}`
      englishName = parts[1]
    }

    parsedItems.push({
      rowIndex: i + 1,
      rawName,
      displayName,
      englishName,
      status: isLeft ? 'Left' : 'Active',
      terminationDate,
      adDisabled: adUpper,
      qadDisabled: qadUpper,
      note,
      nextSteps,
      isLeft,
      isQadActive,
      isAdActive
    })
  }

  return {
    headers,
    items: parsedItems,
    summary: {
      total: parsedItems.length,
      active: activeCount,
      left: leftCount,
      qadActive: qadActiveCount,
      qadDisabled: qadDisabledCount,
      hasTermDate: termDateCount
    }
  }
}

/**
 * 8. Tải Mẫu File Excel Danh Sách Tài Khoản
 */
export async function downloadAccountTemplate(asXlsx = true) {
  const headers = [
    'STT',
    'Name',
    'Status',
    'Termination date',
    'AD account disabled?',
    'QAD account disabled?',
    'Note',
    'Next steps'
  ]

  const samples = [
    [1, 'An, Anna', 'Active', 'N/A', 'Y', 'Y', '', ''],
    [2, 'Anh, Annie', 'Active', 'N/A', 'Y', 'Y', '', ''],
    [3, 'Anh, Annie(IE)', 'Active', '2026-08-04', 'Y', 'N', '', ''],
    [4, 'Ban, Panda', 'Active', '', 'Y', 'N', '', ''],
    [5, 'TTHDis-Duong, Demi', 'Left', '', 'Y', 'N', 'It needs to be retained for the department.', '']
  ]

  if (asXlsx) {
    await exportToExcelFile('Mau_Danh_Sach_Tai_Khoan.xlsx', 'Danh_Sach_Tai_Khoan', headers, samples)
  } else {
    const content = '\uFEFF' + headers.join(',') + '\r\n' + samples.map(r => r.map(v => `"${v}"`).join(',')).join('\r\n')
    downloadFileBlob(content, 'Mau_Danh_Sach_Tai_Khoan.csv', 'text/csv;charset=utf-8;')
  }
}

