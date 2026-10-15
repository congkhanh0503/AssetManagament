import { apiClient, assetsApi, employeesApi, dashboardApi, departmentsApi, categoriesApi } from '@/api/client'

const MAX_HISTORY_ROWS = 200

const fmtDate = (d) => {
  if (!d) return ''
  const dt = new Date(d)
  return isNaN(dt) ? '' : dt.toLocaleString('vi-VN', { hour12: false })
}

// Đầu tuần (thứ Hai 00:00) & đầu tháng theo giờ máy người dùng
const startOfWeek = () => {
  const d = new Date()
  d.setHours(0, 0, 0, 0)
  d.setDate(d.getDate() - ((d.getDay() + 6) % 7))
  return d
}
const startOfMonth = () => {
  const d = new Date()
  d.setHours(0, 0, 0, 0)
  d.setDate(1)
  return d
}

const countBy = (rows, keyFn) => {
  const out = {}
  for (const r of rows) {
    const k = keyFn(r) || 'Không rõ'
    out[k] = (out[k] || 0) + 1
  }
  return out
}

/**
 * Lấy Lịch Sử Cấp Phát, Thu Hồi & Điều Chuyển thiết bị, rút gọn cho vừa token
 */
export async function getHandoverHistoryContext() {
  try {
    const raw = await assetsApi.getHandoverHistory({ period: 'all' })
    const rows = (Array.isArray(raw) ? raw : (raw?.data || raw?.items || []))
      .slice()
      .sort((a, b) => new Date(b.actionDate) - new Date(a.actionDate))

    const weekStart = startOfWeek()
    const monthStart = startOfMonth()
    const inRange = (from) => rows.filter(h => new Date(h.actionDate) >= from)
    const summarize = (list) => countBy(list, h => h.actionTypeLabel || h.actionType)

    return {
      total: rows.length,
      shown: Math.min(rows.length, MAX_HISTORY_ROWS),
      countAllTime: summarize(rows),
      countThisWeek: summarize(inRange(weekStart)),
      countThisMonth: summarize(inRange(monthStart)),
      assignedByDeptThisMonth: countBy(
        inRange(monthStart).filter(h => h.actionType === 'Assign'),
        h => h.toDepartmentName
      ),
      latest: rows.slice(0, MAX_HISTORY_ROWS).map(h => ({
        date: fmtDate(h.actionDate),
        action: h.actionTypeLabel || h.actionType,
        asset: h.assetCode,
        name: h.assetName,
        serial: h.serialNumber,
        from: h.fromEmployeeName ? `${h.fromEmployeeName}${h.fromDepartmentName ? ' (' + h.fromDepartmentName + ')' : ''}` : (h.fromLocation || ''),
        to: h.toEmployeeName ? `${h.toEmployeeName}${h.toDepartmentName ? ' (' + h.toDepartmentName + ')' : ''}` : (h.toLocation || ''),
        condition: h.conditionStatus || '',
        note: h.note || ''
      }))
    }
  } catch (error) {
    console.warn('[Gemini Context] Không thể tải lịch sử bàn giao:', error)
    return null
  }
}

/**
 * Thu thập dữ liệu ngữ cảnh thực tế từ hệ thống QLKho để làm kiến thức cho Gemini
 */
export async function getLiveWarehouseContext() {
  try {
    const [kpisRes, assetsRes, employeesRes, warrantyRes, departmentsRes, categoriesRes, historyRes] = await Promise.allSettled([
      dashboardApi.getKpis(),
      assetsApi.getAll(),
      employeesApi.getAll(),
      assetsApi.getWarrantyAlerts(),
      departmentsApi.getAll(),
      categoriesApi.getAll(),
      getHandoverHistoryContext(),
    ])

    const handoverHistory = historyRes.status === 'fulfilled' ? historyRes.value : null
    const kpis = kpisRes.status === 'fulfilled' ? kpisRes.value : null
    const rawAssets = assetsRes.status === 'fulfilled' ? assetsRes.value : []
    const assets = Array.isArray(rawAssets) ? rawAssets : (rawAssets?.data || rawAssets?.items || [])

    const rawEmployees = employeesRes.status === 'fulfilled' ? employeesRes.value : []
    const employees = Array.isArray(rawEmployees) ? rawEmployees : (rawEmployees?.data || rawEmployees?.items || [])

    const warrantyAlerts = warrantyRes.status === 'fulfilled' ? warrantyRes.value : []
    const rawDepts = departmentsRes.status === 'fulfilled' ? departmentsRes.value : []
    const departments = Array.isArray(rawDepts) ? rawDepts : (rawDepts?.data || [])

    const rawCats = categoriesRes.status === 'fulfilled' ? categoriesRes.value : []
    const categories = Array.isArray(rawCats) ? rawCats : (rawCats?.data || [])

    // Tối ưu hóa danh sách tài sản (rút gọn các trường quan trọng nhất để vừa token)
    const assetSummary = assets.slice(0, 400).map(a => ({
      code: a.assetCode || a.code,
      name: a.assetName || a.name,
      category: a.categoryName || a.category,
      brand: a.brandName || a.brand,
      serial: a.serialNumber || a.serviceTag || a.serial,
      status: a.status,
      holder: a.holderName || a.employeeName || (a.status === 'Available' ? 'Kho (Chưa cấp)' : 'N/A'),
      department: a.departmentName || a.departmentCode || '',
      warranty: a.warrantyExpireDate ? new Date(a.warrantyExpireDate).toLocaleDateString('vi-VN') : 'Không rõ'
    }))

    const employeeSummary = employees.slice(0, 200).map(e => ({
      code: e.employeeCode || e.code,
      name: e.fullName || e.employeeName || e.name,
      dept: e.departmentName || e.departmentCode,
      status: e.status,
      assetsHolding: e.holdingAssetCount || e.assetsCount || (e.assets ? e.assets.length : 0)
    }))

    return {
      kpiSummary: kpis ? {
        totalAssets: kpis.totalAssets || assets.length,
        inUse: kpis.inUseAssets,
        available: kpis.availableAssets,
        broken: kpis.brokenAssets,
        totalEmployees: kpis.totalEmployees || employees.length
      } : { totalAssets: assets.length, totalEmployees: employees.length },
      departments: departments.map(d => d.departmentName || d.name),
      categories: categories.map(c => c.categoryName || c.name),
      warrantyAlertsCount: warrantyAlerts.length,
      warrantyAlertsPreview: warrantyAlerts.slice(0, 10).map(w => ({
        code: w.assetCode,
        name: w.assetName,
        holder: w.holderName,
        daysRemaining: w.daysRemaining,
        isExpired: w.isExpired
      })),
      assetSampleList: assetSummary,
      employeeSampleList: employeeSummary,
      handoverHistory
    }
  } catch (error) {
    console.warn('[Gemini Context] Không thể tải đầy đủ ngữ cảnh kho:', error)
    return null
  }
}

/**
 * Xây dựng System Instruction cho Gemini
 */
function buildSystemInstruction(context) {
  let contextText = ''
  if (context) {
    contextText = `
--- DỮ LIỆU NGỮ CẢNH KHO TÀI SẢN HIỆN TẠI (THỰC TẾ) ---
1. Thống kê KPI:
- Tổng số tài sản: ${context.kpiSummary?.totalAssets || 'N/A'}
- Đang cấp phát sử dụng: ${context.kpiSummary?.inUse || 'N/A'}
- Sẵn sàng trong kho: ${context.kpiSummary?.available || 'N/A'}
- Thiết bị hỏng/lỗi: ${context.kpiSummary?.broken || 'N/A'}
- Tổng số nhân sự: ${context.kpiSummary?.totalEmployees || 'N/A'}

2. Danh mục tài sản: ${context.categories?.join(', ') || 'Chưa cập nhật'}
3. Danh sách phòng ban: ${context.departments?.join(', ') || 'Chưa cập nhật'}

4. Cảnh báo bảo hành (Top 10):
${JSON.stringify(context.warrantyAlertsPreview, null, 1)}

5. Danh sách tài sản trong kho (Mẫu tra cứu chi tiết):
${JSON.stringify(context.assetSampleList, null, 1)}

6. Danh sách nhân sự:
${JSON.stringify(context.employeeSampleList, null, 1)}

${context.handoverHistory ? `7. LỊCH SỬ CẤP PHÁT, THU HỒI & ĐIỀU CHUYỂN THIẾT BỊ:
- Tổng số bản ghi: ${context.handoverHistory.total} (bên dưới liệt kê ${context.handoverHistory.shown} bản ghi mới nhất, sắp xếp từ mới đến cũ)
- Thống kê toàn thời gian: ${JSON.stringify(context.handoverHistory.countAllTime)}
- Thống kê tuần này (từ thứ Hai): ${JSON.stringify(context.handoverHistory.countThisWeek)}
- Thống kê tháng này (từ ngày 1): ${JSON.stringify(context.handoverHistory.countThisMonth)}
- Số lượt cấp phát theo phòng ban trong tháng này: ${JSON.stringify(context.handoverHistory.assignedByDeptThisMonth)}
- Chi tiết (date, action, asset=mã TS, from=người giao/nơi đi, to=người nhận/nơi đến):
${JSON.stringify(context.handoverHistory.latest)}
` : '7. Lịch sử cấp phát/thu hồi/điều chuyển: chưa tải được.'}
-------------------------------------------------------
`
  }

  return `Bạn là "Trợ lý Ảo QLKho AI" - Chuyên viên hỗ trợ thông minh của Hệ thống Quản trị Tài sản Doanh nghiệp (IT Asset Management).
Mục tiêu của bạn là giúp người dùng tra cứu thông tin tài sản, kiểm tra thiết bị, người nắm giữ, tình trạng bảo hành, thống kê kho và giải đáp thắc mắc.

Quy tắc ứng xử và phản hồi:
1. Luôn trả lời bằng TIẾNG VIỆT, thái độ thân thiện, lịch sự, chuyên nghiệp.
2. SỬ DỤNG DỮ LIỆU THỰC TẾ được cung cấp ở trên để trả lời chính xác:
   - Khi tra cứu tài sản (theo mã, tên thiết bị, serial hoặc nhân viên), hãy tìm trong danh sách mẫu và trả về thông tin chi tiết: Mã tài sản, Tên thiết bị, Người đang giữ, Phòng ban, Trạng thái, Bảo hành.
   - Trình bày thông tin dạng Bảng Markdown hoặc Gạch đầu dòng rõ ràng, gọn gàng, dễ nhìn.
   - Nếu không tìm thấy trong dữ liệu hiện có, hãy nói rõ là chưa tìm thấy trong mẫu dữ liệu hiện tại và gợi ý người dùng kiểm tra lại mã hoặc từ khóa trên thanh tìm kiếm của trang Tài sản.
3. Giải thích trạng thái thiết bị:
   - InUse: Đang được nhân viên sử dụng
   - Available: Sẵn sàng trong kho để cấp phát
   - Broken: Thiết bị hỏng đang chờ sửa chữa/thanh lý
   - UnderRepair: Đang sửa chữa
4. Hỗ trợ hướng dẫn nghiệp vụ nếu người dùng hỏi:
   - Cấp phát tài sản: Vào menu Tài sản -> chọn bàn giao hoặc cấp phát trọn gói.
   - Báo hỏng: Nhấn nút báo hỏng trên thẻ tài sản.
   - Thu hồi: Vào trang tài sản -> Thu hồi về kho.
5. Tra cứu lịch sử cấp phát / thu hồi / điều chuyển (mục 7):
   - Trả lời các câu hỏi như: thiết bị X đã qua tay ai, ai được cấp gì tuần/tháng này, phòng ban nào nhận nhiều nhất, thiết bị nào vừa thu hồi/điều chuyển, nhân viên Y từng nhận thiết bị nào.
   - Với số lượng, dùng các dòng thống kê đã tính sẵn; với chi tiết thì lọc từ danh sách và nêu rõ thời gian, người giao, người nhận.
   - Nếu danh sách chi tiết chỉ có các bản ghi mới nhất mà câu hỏi cần dữ liệu cũ hơn, hãy nói rõ giới hạn đó và hướng dẫn mở mục Lịch sử bàn giao ở trang Tài sản.
   - Chỉ dùng dữ liệu được cung cấp, tuyệt đối không bịa thông tin.
6. Giữ câu trả lời ngắn gọn, súc tích, không lan man.

${contextText}`
}

/**
 * Gửi yêu cầu sinh nội dung tới Google Gemini API
 */
export async function sendChatMessage({ message, history = [], context = null }) {
  const contents = []

  // Đưa lịch sử hội thoại trước đó vào (giới hạn 8 tin gần nhất để giữ tốc độ)
  const recentHistory = history.slice(-8)
  for (const item of recentHistory) {
    contents.push({
      role: item.role === 'user' ? 'user' : 'model',
      parts: [{ text: item.text }]
    })
  }

  // Thêm tin nhắn hiện tại của người dùng
  contents.push({
    role: 'user',
    parts: [{ text: message }]
  })

  const systemInstruction = buildSystemInstruction(context)

  const payload = {
    contents,
    systemInstruction: {
      parts: [{ text: systemInstruction }]
    },
    generationConfig: {
      temperature: 0.4,
      maxOutputTokens: 2048,
      topP: 0.8,
    }
  }

  const data = await apiClient.post('/gemini/generate', payload)
  const replyText = data.candidates?.[0]?.content?.parts?.[0]?.text
  if (!replyText) {
    throw new Error('Không nhận được phản hồi hợp lệ từ Gemini AI')
  }

  return {
    text: replyText,
    model: data.modelVersion || 'Gemini'
  }
}
