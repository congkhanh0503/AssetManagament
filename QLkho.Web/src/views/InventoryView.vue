<template>
  <div class="inv-page">
    <!-- ============ DANH SÁCH CÁC ĐỢT ============ -->
    <template v-if="!currentId">
      <div class="inv-head">
        <h2 class="inv-title">{{ $t('inventory.title') }}</h2>
        <button v-if="!isViewer" type="button" class="btn btn-primary" @click="showCreate = !showCreate">
          {{ $t('inventory.new_session') }}
        </button>
      </div>

      <div v-if="showCreate" class="inv-card inv-create">
        <input v-model="newName" class="form-control" :placeholder="$t('inventory.name_ph')" maxlength="200" />
        <input v-model="newNote" class="form-control" :placeholder="$t('inventory.note_ph')" maxlength="500" />
        <div class="inv-create-actions">
          <button type="button" class="btn btn-secondary" @click="showCreate = false">{{ $t('inventory.cancel') }}</button>
          <button type="button" class="btn btn-primary" :disabled="creating || !newName.trim()" @click="createSession">
            {{ creating ? $t('inventory.creating') : $t('inventory.create') }}
          </button>
        </div>
        <p class="inv-hint">{{ $t('inventory.create_hint') }}</p>
      </div>

      <div v-if="loadingList" class="inv-empty">{{ $t('inventory.loading') }}</div>
      <div v-else-if="sessions.length === 0" class="inv-empty">{{ $t('inventory.no_sessions') }}</div>

      <div v-for="s in sessions" :key="s.sessionID" class="inv-card inv-session" @click="openSession(s.sessionID)">
        <div class="inv-session-top">
          <strong>{{ s.name }}</strong>
          <span class="inv-badge" :class="s.status === 'Open' ? 'open' : 'closed'">
            {{ s.status === 'Open' ? $t('inventory.status_open') : $t('inventory.status_closed') }}
          </span>
        </div>
        <div class="inv-meta">{{ formatDateTime(s.createdAt) }} · {{ s.createdBy }}</div>
        <div class="inv-progress"><div class="inv-progress-bar" :style="{ width: percent(s.scanned, s.total) + '%' }"></div></div>
        <div class="inv-meta">
          {{ $t('inventory.progress', { scanned: s.scanned, total: s.total, missing: s.missing }) }}
          <span v-if="s.extra"> · {{ $t('inventory.extra_count', { count: s.extra }) }}</span>
        </div>
      </div>
    </template>

    <!-- ============ CHI TIẾT MỘT ĐỢT ============ -->
    <template v-else-if="summary">
      <div class="inv-head">
        <button type="button" class="btn btn-secondary" @click="backToList">{{ $t('inventory.back') }}</button>
        <span class="inv-badge" :class="isOpen ? 'open' : 'closed'">{{ isOpen ? $t('inventory.status_open') : $t('inventory.status_closed') }}</span>
      </div>
      <h2 class="inv-title">{{ summary.session.name }}</h2>
      <div v-if="summary.session.note" class="inv-meta">{{ summary.session.note }}</div>

      <div class="inv-stats">
        <div class="inv-stat"><span>{{ $t('inventory.stat_total') }}</span><strong>{{ summary.session.total }}</strong></div>
        <div class="inv-stat ok"><span>{{ $t('inventory.stat_scanned') }}</span><strong>{{ summary.session.scanned }}</strong></div>
        <div class="inv-stat warn"><span>{{ $t('inventory.stat_missing') }}</span><strong>{{ summary.session.missing }}</strong></div>
        <div class="inv-stat extra"><span>{{ $t('inventory.stat_extra') }}</span><strong>{{ summary.session.extra }}</strong></div>
      </div>
      <div class="inv-progress big"><div class="inv-progress-bar" :style="{ width: percent(summary.session.scanned, summary.session.total) + '%' }"></div></div>

      <!-- Khu vực quét -->
      <div v-if="isOpen && !isViewer" class="inv-card">
        <button type="button" class="btn btn-primary inv-scan-btn" @click="toggleScanning">
          {{ scanning ? $t('inventory.scan_stop') : $t('inventory.scan_start') }}
        </button>
        <QrScanner v-if="scanning" class="inv-scanner" @detect="handleScan" />

        <div class="inv-manual">
          <input v-model="manualCode" class="form-control" :placeholder="$t('inventory.manual_ph')" @keyup.enter="submitManual" />
          <button type="button" class="btn btn-secondary" :disabled="!manualCode.trim()" @click="submitManual">{{ $t('inventory.record') }}</button>
        </div>

        <div v-if="lastResult" class="inv-result" :class="resultClass(lastResult.result)">
          <div class="inv-result-msg">{{ resultMessage(lastResult) }}</div>
          <template v-if="lastResult.item">
            <div class="inv-result-asset">{{ lastResult.item.assetCode }} · {{ lastResult.item.assetName }}</div>
            <div class="inv-meta">
              {{ holderText(lastResult.item) }}
              · {{ deptLabel(lastResult.item.departmentName) }}
            </div>
            <div v-if="lastResult.department && !lastResult.item.isExtra" class="inv-result-dept">
              {{ $t('inventory.dept_progress', { dept: deptLabel(lastResult.department.departmentName), scanned: lastResult.department.scanned, total: lastResult.department.total, missing: lastResult.department.missing }) }}
            </div>
          </template>
        </div>
      </div>

      <!-- Tiến độ theo phòng ban -->
      <h3 class="inv-sub">{{ $t('inventory.by_department') }}</h3>
      <div class="inv-dept-list">
        <div class="inv-dept" :class="{ active: !deptFilter }" @click="setDept('')">
          <span>{{ $t('inventory.all') }}</span>
          <span class="inv-dept-count">{{ summary.session.scanned }}/{{ summary.session.total }}</span>
        </div>
        <div
          v-for="d in summary.departments" :key="d.departmentName"
          class="inv-dept" :class="{ active: deptFilter === d.departmentName, done: d.missing === 0 }"
          @click="setDept(d.departmentName)"
        >
          <span class="inv-dept-name">{{ deptLabel(d.departmentName) }}</span>
          <span class="inv-dept-count">{{ d.scanned }}/{{ d.total }}<template v-if="d.missing"> · {{ $t('inventory.missing_short', { count: d.missing }) }}</template></span>
        </div>
      </div>

      <!-- Danh sách tài sản -->
      <div class="inv-tabs">
        <button type="button" :class="{ active: statusFilter === 'missing' }" @click="setStatus('missing')">{{ $t('inventory.tab_missing') }}</button>
        <button type="button" :class="{ active: statusFilter === 'scanned' }" @click="setStatus('scanned')">{{ $t('inventory.tab_scanned') }}</button>
        <button type="button" :class="{ active: statusFilter === 'extra' }" @click="setStatus('extra')">{{ $t('inventory.tab_extra') }}</button>
      </div>
      <input v-model="search" class="form-control inv-search" :placeholder="$t('inventory.search_ph')" @input="debouncedLoadItems" />

      <div v-if="loadingItems" class="inv-empty">{{ $t('inventory.loading') }}</div>
      <div v-else-if="items.length === 0" class="inv-empty">{{ $t('inventory.no_items') }}</div>

      <div v-for="it in visibleItems" :key="it.itemID" class="inv-card inv-item">
        <div class="inv-item-main">
          <strong>{{ it.assetCode }}</strong>
          <span class="inv-item-name">{{ it.assetName }}</span>
          <span class="inv-meta">
            {{ holderText(it) }} · {{ deptLabel(it.departmentName) }}
          </span>
          <span v-if="it.isScanned" class="inv-meta">{{ $t('inventory.scanned_at', { time: formatDateTime(it.scannedAt), by: it.scannedBy }) }}</span>
        </div>
        <button
          v-if="it.isScanned && isOpen && !isViewer" type="button" class="btn btn-secondary inv-undo"
          @click="undo(it)"
        >{{ $t('inventory.undo') }}</button>
      </div>
      <button v-if="items.length > visibleCount" type="button" class="btn btn-secondary inv-more" @click="visibleCount += 100">
        {{ $t('inventory.show_more', { count: items.length - visibleCount }) }}
      </button>

      <!-- Chốt / mở lại -->
      <div v-if="!isViewer" class="inv-footer-actions">
        <button v-if="isOpen" type="button" class="btn btn-primary" @click="closeSession">{{ $t('inventory.close_session') }}</button>
        <button v-else type="button" class="btn btn-secondary" @click="reopenSession">{{ $t('inventory.reopen_session') }}</button>
        <button type="button" class="btn btn-danger" @click="deleteSession">{{ $t('inventory.delete_session') }}</button>
      </div>
    </template>

    <Toast ref="toastRef" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { inventoryApi } from '@/api/client'
import { getCurrentUser } from '@/api/auth'
import { t, currentLocale } from '@/i18n'
import QrScanner from '@/components/common/QrScanner.vue'
import Toast from '@/components/common/Toast.vue'

const toastRef = ref(null)
const isViewer = getCurrentUser()?.role === 'Viewer'

const sessions = ref([])
const loadingList = ref(false)
const showCreate = ref(false)
const creating = ref(false)
const newName = ref('')
const newNote = ref('')

const currentId = ref(null)
const summary = ref(null)
const items = ref([])
const loadingItems = ref(false)
const deptFilter = ref('')
const statusFilter = ref('missing')
const search = ref('')
const visibleCount = ref(100)

const scanning = ref(false)
const manualCode = ref('')
const lastResult = ref(null)
let scanBusy = false
let lastCode = ''

const isOpen = computed(() => summary.value?.session.status === 'Open')
const visibleItems = computed(() => items.value.slice(0, visibleCount.value))

const percent = (a, b) => (b > 0 ? Math.round((a / b) * 100) : 0)

const formatDateTime = (v) => {
  if (!v) return ''
  return new Date(v).toLocaleString(currentLocale.value === 'en' ? 'en-GB' : 'vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' })
}

// Tên phòng ban "Kho / Chưa cấp phát" do backend đặt, dịch khi hiển thị
const UNASSIGNED_DEPT = 'Kho / Chưa cấp phát'
const deptLabel = (name) => (name === UNASSIGNED_DEPT ? t('inventory.unassigned_dept') : name)

const holderText = (it) =>
  it.holderName ? `${it.holderName} (${it.holderCode || '---'})` : t('inventory.unassigned')

// Thông điệp kết quả quét dịch theo mã kết quả, không dùng chuỗi tiếng Việt từ server
const resultMessage = (r) => {
  const code = r.item?.assetCode || lastCode
  switch (r.result) {
    case 'Scanned': return t('inventory.res_scanned', { code })
    case 'AlreadyScanned': return t('inventory.res_already', { code, time: formatDateTime(r.item?.scannedAt) })
    case 'Extra': return t('inventory.res_extra', { code })
    case 'Closed': return t('inventory.res_closed')
    default: return t('inventory.res_notfound', { code })
  }
}

const resultClass = (r) => ({ Scanned: 'ok', AlreadyScanned: 'warn', Extra: 'warn' }[r] || 'bad')

const notifyError = (title, err) => toastRef.value?.addToast(title, err?.message || String(err), 'error')

const loadSessions = async () => {
  loadingList.value = true
  try {
    sessions.value = await inventoryApi.getSessions()
  } catch (err) {
    notifyError(t('inventory.err_load_sessions'), err)
  } finally {
    loadingList.value = false
  }
}

const createSession = async () => {
  creating.value = true
  try {
    const s = await inventoryApi.createSession({ name: newName.value, note: newNote.value })
    newName.value = ''
    newNote.value = ''
    showCreate.value = false
    await openSession(s.sessionID)
  } catch (err) {
    notifyError(t('inventory.err_create'), err)
  } finally {
    creating.value = false
  }
}

const loadSummary = async () => {
  summary.value = await inventoryApi.getSummary(currentId.value)
}

const loadItems = async () => {
  loadingItems.value = true
  try {
    items.value = await inventoryApi.getItems(currentId.value, {
      department: deptFilter.value || undefined,
      status: statusFilter.value,
      search: search.value || undefined
    })
    visibleCount.value = 100
  } catch (err) {
    notifyError(t('inventory.err_load_items'), err)
  } finally {
    loadingItems.value = false
  }
}

let searchTimer = null
const debouncedLoadItems = () => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(loadItems, 300)
}

const openSession = async (id) => {
  currentId.value = id
  summary.value = null
  deptFilter.value = ''
  statusFilter.value = 'missing'
  search.value = ''
  lastResult.value = null
  scanning.value = false
  try {
    await Promise.all([loadSummary(), loadItems()])
  } catch (err) {
    notifyError(t('inventory.err_open'), err)
    currentId.value = null
  }
}

const backToList = async () => {
  scanning.value = false
  currentId.value = null
  summary.value = null
  await loadSessions()
}

const setDept = (name) => { deptFilter.value = name; loadItems() }
const setStatus = (st) => { statusFilter.value = st; loadItems() }

const sendScan = async (code) => {
  if (scanBusy || !code) return
  scanBusy = true
  lastCode = code.length > 60 ? `${code.slice(0, 60)}...` : code
  try {
    lastResult.value = await inventoryApi.scan(currentId.value, code)
    playResultSound(lastResult.value.result)
    await Promise.all([loadSummary(), loadItems()])
  } catch (err) {
    playResultSound('Error')
    notifyError(t('inventory.err_scan'), err)
  } finally {
    scanBusy = false
  }
}

const handleScan = (text) => sendScan(text)

// ===== Âm báo kết quả quét (Web Audio) =====
// AudioContext phải được tạo/bật sau một thao tác của người dùng, nên khởi tạo khi bấm nút quét.
let audioCtx = null
const ensureAudio = () => {
  try {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)()
    if (audioCtx.state === 'suspended') audioCtx.resume()
  } catch (e) { /* trình duyệt không hỗ trợ âm thanh: bỏ qua */ }
}

const tone = (freq, start, duration, type = 'sine', volume = 0.35) => {
  const t0 = audioCtx.currentTime + start
  const osc = audioCtx.createOscillator()
  const gain = audioCtx.createGain()
  osc.type = type
  osc.frequency.setValueAtTime(freq, t0)
  gain.gain.setValueAtTime(0.0001, t0)
  gain.gain.exponentialRampToValueAtTime(volume, t0 + 0.01)
  gain.gain.exponentialRampToValueAtTime(0.0001, t0 + duration)
  osc.connect(gain).connect(audioCtx.destination)
  osc.start(t0)
  osc.stop(t0 + duration + 0.02)
}

const playResultSound = (result) => {
  ensureAudio()
  if (!audioCtx) return
  if (result === 'Scanned') {
    // "ting": nốt cao, ngân ngắn
    tone(1568, 0, 0.45)
    tone(3136, 0, 0.25, 'sine', 0.12)
  } else if (result === 'AlreadyScanned' || result === 'Extra') {
    // hai tiếng bíp ngắn: cần chú ý
    tone(660, 0, 0.12, 'square', 0.15)
    tone(660, 0.18, 0.12, 'square', 0.15)
  } else {
    // một tiếng trầm: lỗi
    tone(196, 0, 0.4, 'sawtooth', 0.2)
  }
}

const toggleScanning = () => {
  ensureAudio()
  scanning.value = !scanning.value
}

const submitManual = async () => {
  const code = manualCode.value.trim()
  if (!code) return
  ensureAudio()
  await sendScan(code)
  manualCode.value = ''
}

const undo = async (it) => {
  try {
    await inventoryApi.unscan(currentId.value, it.itemID)
    await Promise.all([loadSummary(), loadItems()])
  } catch (err) {
    notifyError(t('inventory.err_undo'), err)
  }
}

const closeSession = async () => {
  if (!confirm(t('inventory.confirm_close', { missing: summary.value.session.missing }))) return
  try {
    await inventoryApi.close(currentId.value)
    scanning.value = false
    await loadSummary()
  } catch (err) {
    notifyError(t('inventory.err_close'), err)
  }
}

const reopenSession = async () => {
  try {
    await inventoryApi.reopen(currentId.value)
    await loadSummary()
  } catch (err) {
    notifyError(t('inventory.err_reopen'), err)
  }
}

const deleteSession = async () => {
  if (!confirm(t('inventory.confirm_delete'))) return
  try {
    await inventoryApi.remove(currentId.value)
    await backToList()
  } catch (err) {
    notifyError(t('inventory.err_delete'), err)
  }
}

onMounted(loadSessions)
onBeforeUnmount(() => clearTimeout(searchTimer))
</script>

<style scoped>
.inv-page {
  max-width: 720px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.inv-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.inv-title {
  margin: 0;
  font-size: 1.3rem;
}

.inv-sub {
  margin: 6px 0 0;
  font-size: 1rem;
}

.inv-card {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.inv-session {
  cursor: pointer;
}

.inv-session-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
}

.inv-meta {
  font-size: 0.82rem;
  color: #64748b;
}

.inv-hint {
  margin: 0;
  font-size: 0.8rem;
  color: #64748b;
}

.inv-empty {
  text-align: center;
  padding: 24px;
  color: #64748b;
}

.inv-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 3px 10px;
  border-radius: 999px;
  white-space: nowrap;
}

.inv-badge.open { background: #dcfce7; color: #166534; }
.inv-badge.closed { background: #e2e8f0; color: #475569; }

.inv-create-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.inv-progress {
  height: 8px;
  background: #e2e8f0;
  border-radius: 999px;
  overflow: hidden;
}

.inv-progress.big { height: 12px; }

.inv-progress-bar {
  height: 100%;
  background: #22c55e;
  transition: width 0.3s ease;
}

.inv-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
}

.inv-stat {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: 10px;
  padding: 10px 6px;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.inv-stat span { font-size: 0.72rem; color: #64748b; }
.inv-stat strong { font-size: 1.35rem; }
.inv-stat.ok strong { color: #16a34a; }
.inv-stat.warn strong { color: #dc2626; }
.inv-stat.extra strong { color: #d97706; }

.inv-scan-btn {
  width: 100%;
  padding: 14px;
  font-size: 1.05rem;
}

.inv-scanner { margin-top: 4px; }

.inv-manual {
  display: flex;
  gap: 8px;
}

.inv-manual .form-control { flex: 1; min-width: 0; }

.inv-result {
  border-radius: 10px;
  padding: 10px 12px;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.inv-result.ok { background: #dcfce7; border: 1px solid #86efac; }
.inv-result.warn { background: #fef3c7; border: 1px solid #fcd34d; }
.inv-result.bad { background: #fee2e2; border: 1px solid #fca5a5; }

.inv-result-msg { font-weight: 600; }
.inv-result-asset { font-size: 0.95rem; }
.inv-result-dept { font-size: 0.85rem; }

.inv-dept-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.inv-dept {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: 10px;
  cursor: pointer;
  font-size: 0.9rem;
}

.inv-dept.active { border-color: #6366f1; background: #eef2ff; }
.inv-dept.done .inv-dept-count { color: #16a34a; }
.inv-dept-name { overflow-wrap: anywhere; }
.inv-dept-count { font-weight: 600; white-space: nowrap; }

.inv-tabs {
  display: flex;
  gap: 6px;
}

.inv-tabs button {
  flex: 1;
  padding: 10px 6px;
  border: 1px solid var(--border-color);
  background: #ffffff;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.inv-tabs button.active {
  background: #6366f1;
  border-color: #6366f1;
  color: #ffffff;
}

.inv-item {
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
}

.inv-item-main {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.inv-item-name { font-size: 0.88rem; overflow-wrap: anywhere; }
.inv-undo { flex: none; }
.inv-more { width: 100%; }

.inv-footer-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 8px;
  flex-wrap: wrap;
}
</style>
