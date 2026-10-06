import { assetsApi, employeesApi, dashboardApi, departmentsApi, categoriesApi } from '@/api/client'

const GEMINI_API_KEY = import.meta.env.VITE_GEMINI_API_KEY || ''
const PRIMARY_MODEL = 'gemini-3.5-flash-lite'
const FALLBACK_MODEL = 'gemini-flash-latest'

/**
 * Thu thập dữ liệu ngữ cảnh thực tế từ hệ thống QLKho để làm kiến thức cho Gemini
 */
export async function getLiveWarehouseContext() {
  try {
    const [kpisRes, assetsRes, employeesRes, warrantyRes, departmentsRes, categoriesRes] = await Promise.allSettled([
      dashboardApi.getKpis(),
      assetsApi.getAll(),
      employeesApi.getAll(),
      assetsApi.getWarrantyAlerts(),
      departmentsApi.getAll(),
      categoriesApi.getAll(),
    ])

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
    const assetSummary = assets.slice(0, 150).map(a => ({
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

    const employeeSummary = employees.slice(0, 80).map(e => ({
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
      employeeSampleList: employeeSummary
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
5. Giữ câu trả lời ngắn gọn, súc tích, không lan man.

${contextText}`
}

/**
 * Gửi yêu cầu sinh nội dung tới Google Gemini API
 */
export async function sendChatMessage({ message, history = [], context = null }) {
  if (!GEMINI_API_KEY) {
    throw new Error('Chưa cấu hình API Key Gemini. Vui lòng cấu hình VITE_GEMINI_API_KEY trong tệp .env')
  }

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
      maxOutputTokens: 1024,
      topP: 0.8,
    }
  }

  // Thử model chính, nếu gặp sự cố thì fallback sang model phụ
  const models = [PRIMARY_MODEL, FALLBACK_MODEL]

  let lastError = null
  for (const model of models) {
    try {
      const url = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${GEMINI_API_KEY}`
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload)
      })

      const data = await response.json()

      if (!response.ok) {
        throw new Error(data.error?.message || `Lỗi API Gemini (${response.status})`)
      }

      const replyText = data.candidates?.[0]?.content?.parts?.[0]?.text
      if (replyText) {
        return {
          text: replyText,
          model: model
        }
      } else {
        throw new Error('Không nhận được phản hồi hợp lệ từ Gemini AI')
      }
    } catch (err) {
      console.warn(`[Gemini API] Thử model ${model} thất bại:`, err.message)
      lastError = err
    }
  }

  throw lastError || new Error('Không thể kết nối đến Gemini AI')
}
