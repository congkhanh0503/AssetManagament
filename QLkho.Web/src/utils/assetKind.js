// Thiết bị không quản lý theo S/N (chuột, bàn phím): nhập/cấp phát theo số lượng
const QUANTITY_KEYWORDS = ['chuột', 'chuot', 'mouse', 'bàn phím', 'ban phim', 'keyboard', 'thiết bị khác', 'thiet bi khac']

export const isQuantityAsset = (...names) => {
  const text = names.filter(Boolean).join(' ').toLowerCase()
  return QUANTITY_KEYWORDS.some(k => text.includes(k))
}
