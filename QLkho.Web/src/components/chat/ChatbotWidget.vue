<template>
  <div class="chatbot-widget-container">
    <!-- Nút Floating Mở Chatbot (FAB) -->
    <transition name="fab-fade">
      <button
        v-if="!isOpen"
        id="btn-open-chatbot"
        class="chatbot-fab"
        title="Trợ lý Ảo QLKho AI - Hỗ trợ tra cứu"
        @click="toggleChat"
      >
        <div class="fab-glow"></div>
        <div class="fab-icon-wrapper">
          <svg class="fab-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <!-- AI Robot / Sparkle Icon -->
            <path d="M12 2a2 2 0 0 1 2 2v2a2 2 0 0 1-2 2 2 2 0 0 1-2-2V4a2 2 0 0 1 2-2z" />
            <rect x="4" y="8" width="16" height="12" rx="4" />
            <circle cx="9" cy="13" r="1.5" fill="currentColor" />
            <circle cx="15" cy="13" r="1.5" fill="currentColor" />
            <path d="M9 17h6" />
          </svg>
        </div>
        <span class="fab-badge">AI</span>
        <div class="fab-tooltip">Trợ lý tra cứu AI</div>
      </button>
    </transition>

    <!-- Cửa sổ Chatbot Floating Window -->
    <transition name="chat-window-pop">
      <div v-if="isOpen" class="chat-window" id="chatbot-window">
        <!-- Header -->
        <div class="chat-header">
          <div class="header-info">
            <div class="bot-avatar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="4" y="8" width="16" height="12" rx="4" />
                <circle cx="9" cy="13" r="1.5" fill="currentColor" />
                <circle cx="15" cy="13" r="1.5" fill="currentColor" />
                <path d="M9 17h6" />
                <path d="M12 4v4" />
              </svg>
              <span class="status-indicator"></span>
            </div>
            <div class="header-text">
              <div class="header-title">
                <span>QLKho AI Assistant</span>
                <span class="model-badge">Gemini 3.5</span>
              </div>
              <div class="header-status">
                <span class="status-dot"></span>
                <span>{{ isSyncing ? 'Đang nạp dữ liệu kho...' : 'Sẵn sàng tra cứu' }}</span>
              </div>
            </div>
          </div>
          <div class="header-actions">
            <button
              class="icon-btn"
              title="Làm mới cuộc trò chuyện"
              @click="clearChat"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M3 12a9 9 0 0 1 15-6.7L21 8" />
                <path d="M21 3v5h-5" />
                <path d="M21 12a9 9 0 0 1-15 6.7L3 16" />
                <path d="M3 21v-5h5" />
              </svg>
            </button>
            <button
              class="icon-btn"
              title="Thu nhỏ"
              @click="toggleChat"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>
        </div>

        <!-- Gợi ý nhanh (Quick Suggestions) -->
        <div v-if="messages.length <= 1" class="quick-chips-container">
          <div class="chips-title">Tra cứu nhanh:</div>
          <div class="chips-scroll">
            <button
              v-for="(chip, idx) in quickPrompts"
              :key="idx"
              class="chip-btn"
              @click="sendQuickPrompt(chip.query)"
            >
              <span class="chip-icon">{{ chip.icon }}</span>
              <span>{{ chip.label }}</span>
            </button>
          </div>
        </div>

        <!-- Khung danh sách tin nhắn -->
        <div ref="messagesContainer" class="chat-messages">
          <div
            v-for="(msg, index) in messages"
            :key="index"
            :class="['message-row', msg.role === 'user' ? 'user-row' : 'bot-row']"
          >
            <div v-if="msg.role === 'bot'" class="msg-avatar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="4" y="8" width="16" height="12" rx="4" />
                <circle cx="9" cy="13" r="1.5" fill="currentColor" />
                <circle cx="15" cy="13" r="1.5" fill="currentColor" />
                <path d="M9 17h6" />
              </svg>
            </div>

            <div class="message-bubble">
              <div v-if="msg.role === 'bot'" class="markdown-content" v-html="renderMarkdown(msg.text)"></div>
              <div v-else class="user-text">{{ msg.text }}</div>
              <div class="msg-time">{{ msg.time }}</div>
            </div>
          </div>

          <!-- Typing Indicator -->
          <div v-if="isLoading" class="message-row bot-row">
            <div class="msg-avatar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="4" y="8" width="16" height="12" rx="4" />
                <circle cx="9" cy="13" r="1.5" fill="currentColor" />
                <circle cx="15" cy="13" r="1.5" fill="currentColor" />
              </svg>
            </div>
            <div class="message-bubble typing-bubble">
              <div class="typing-dots">
                <span></span>
                <span></span>
                <span></span>
              </div>
              <span class="typing-text">AI đang tra cứu...</span>
            </div>
          </div>
        </div>

        <!-- Thanh nhập liệu (Input Bar) -->
        <div class="chat-input-area">
          <form @submit.prevent="handleSendMessage" class="input-form">
            <input
              ref="inputRef"
              v-model="inputMessage"
              type="text"
              id="chatbot-input"
              class="chat-input"
              placeholder="Hỏi mã máy, nhân viên giữ, bảo hành..."
              :disabled="isLoading"
              autocomplete="off"
            />
            <button
              type="submit"
              class="send-btn"
              :disabled="!inputMessage.trim() || isLoading"
              title="Gửi tin nhắn"
            >
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
              </svg>
            </button>
          </form>
          <div class="input-hint">
            <span>Powered by Google Gemini 3.5 Flash Lite</span>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, reactive, nextTick, onMounted } from 'vue'
import { getLiveWarehouseContext, sendChatMessage } from '@/services/geminiService'

const isOpen = ref(false)
const isLoading = ref(false)
const isSyncing = ref(false)
const inputMessage = ref('')
const inputRef = ref(null)
const messagesContainer = ref(null)
const warehouseContext = ref(null)

const quickPrompts = [
  { icon: '📊', label: 'Tổng quan kho', query: 'Tổng quan tình trạng kho tài sản hiện tại thế nào?' },
  { icon: '⚠️', label: 'Sắp hết bảo hành', query: 'Có thiết bị nào sắp hết hạn bảo hành không?' },
  { icon: '💻', label: 'Tài sản sẵn sàng', query: 'Hiện có những thiết bị nào đang Available để cấp phát?' },
  { icon: '🛠️', label: 'Thiết bị lỗi/hỏng', query: 'Hiện có bao nhiêu thiết bị bị hỏng hoặc cần bảo trì?' }
]

const messages = reactive([
  {
    role: 'bot',
    text: 'Xin chào! Tôi là **Trợ lý Ảo QLKho AI**. Bạn có thể hỏi tôi tra cứu mã tài sản, người đang giữ, hạn bảo hành hoặc thống kê tình trạng thiết bị trong kho nhé! 🚀',
    time: getCurrentTime()
  }
])

function getCurrentTime() {
  const now = new Date()
  return now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
}

async function loadContext() {
  if (warehouseContext.value) return
  isSyncing.value = true
  try {
    warehouseContext.value = await getLiveWarehouseContext()
  } finally {
    isSyncing.value = false
  }
}

async function toggleChat() {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    loadContext()
    await nextTick()
    scrollToBottom()
    inputRef.value?.focus()
  }
}

function clearChat() {
  messages.splice(0, messages.length, {
    role: 'bot',
    text: 'Đã làm mới cuộc hội thoại. Tôi có thể giúp gì cho bạn trong việc tra cứu tài sản?',
    time: getCurrentTime()
  })
}

function sendQuickPrompt(query) {
  inputMessage.value = query
  handleSendMessage()
}

async function handleSendMessage() {
  const text = inputMessage.value.trim()
  if (!text || isLoading.value) return

  // Thêm tin nhắn của user
  messages.push({
    role: 'user',
    text,
    time: getCurrentTime()
  })

  inputMessage.value = ''
  isLoading.value = true
  await nextTick()
  scrollToBottom()

  // Đảm bảo context được nạp
  if (!warehouseContext.value) {
    warehouseContext.value = await getLiveWarehouseContext()
  }

  try {
    const response = await sendChatMessage({
      message: text,
      history: messages.slice(0, -1),
      context: warehouseContext.value
    })

    messages.push({
      role: 'bot',
      text: response.text,
      time: getCurrentTime()
    })
  } catch (error) {
    messages.push({
      role: 'bot',
      text: `⚠️ **Rất tiếc:** ${error.message || 'Không thể kết nối đến máy chủ AI. Vui lòng kiểm tra lại đường truyền hoặc thử lại sau!'}`,
      time: getCurrentTime()
    })
  } finally {
    isLoading.value = false
    await nextTick()
    scrollToBottom()
    inputRef.value?.focus()
  }
}

function scrollToBottom() {
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

/**
 * Trình parser Markdown gọn nhẹ không cần thư viện bên thứ 3
 */
function renderMarkdown(rawText) {
  if (!rawText) return ''
  let html = rawText
    // Escape HTML cơ bản để an toàn
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')

  // Định dạng Code Block ``` ... ```
  html = html.replace(/```([\s\S]*?)```/g, '<pre class="chat-code-block"><code>$1</code></pre>')

  // Định dạng Inline Code `...`
  html = html.replace(/`([^`]+)`/g, '<code class="chat-inline-code">$1</code>')

  // Bảng Markdown đơn giản | header | ... |
  const lines = html.split('\n')
  let inTable = false
  let tableHtml = ''
  const processedLines = []

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim()
    if (line.startsWith('|') && line.endsWith('|')) {
      if (!inTable) {
        inTable = true
        tableHtml = '<div class="table-responsive"><table class="chat-table">'
      }
      // Dòng phân cách |---|---|
      if (/^\|[-:\s|]+\|$/.test(line)) {
        continue
      }
      const cells = line.slice(1, -1).split('|').map(c => c.trim())
      const isHeader = !tableHtml.includes('<tbody>') && !tableHtml.includes('<tr>')
      if (isHeader) {
        tableHtml += '<thead><tr>' + cells.map(c => `<th>${c}</th>`).join('') + '</tr></thead><tbody>'
      } else {
        tableHtml += '<tr>' + cells.map(c => `<td>${c}</td>`).join('') + '</tr>'
      }
    } else {
      if (inTable) {
        inTable = false
        tableHtml += '</tbody></table></div>'
        processedLines.push(tableHtml)
        tableHtml = ''
      }
      processedLines.push(line)
    }
  }
  if (inTable) {
    tableHtml += '</tbody></table></div>'
    processedLines.push(tableHtml)
  }
  html = processedLines.join('\n')

  // In đậm **text**
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')

  // In nghiêng *text*
  html = html.replace(/\*([^*]+)\*/g, '<em>$1</em>')

  // Gạch đầu dòng
  html = html.replace(/^[*-]\s+(.+)$/gm, '<li class="chat-list-item">$1</li>')

  // Xuống dòng
  html = html.replace(/\n\n+/g, '<br><br>')
  html = html.replace(/\n/g, '<br>')

  return html
}

onMounted(() => {
  // Tiền tải ngữ cảnh ngầm sau 2 giây khi web khởi động
  setTimeout(() => {
    loadContext()
  }, 2000)
})
</script>

<style scoped>
.chatbot-widget-container {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9999;
  font-family: var(--font-family, 'Plus Jakarta Sans', sans-serif);
}

/* =========================================
   1. FLOATING ACTION BUTTON (FAB)
   ========================================= */
.chatbot-fab {
  position: relative;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 24px -4px rgba(79, 70, 229, 0.45), 0 4px 12px rgba(0, 0, 0, 0.1);
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.chatbot-fab:hover {
  transform: scale(1.08) translateY(-2px);
  box-shadow: 0 12px 28px -2px rgba(79, 70, 229, 0.6), 0 6px 16px rgba(0, 0, 0, 0.15);
}

.chatbot-fab:active {
  transform: scale(0.96);
}

.fab-glow {
  position: absolute;
  top: -3px;
  left: -3px;
  right: -3px;
  bottom: -3px;
  border-radius: 50%;
  background: linear-gradient(135deg, #6366f1, #a855f7);
  z-index: -1;
  opacity: 0.6;
  filter: blur(6px);
  animation: pulse-glow 2.5s infinite ease-in-out;
}

@keyframes pulse-glow {
  0%, 100% { opacity: 0.4; transform: scale(1); }
  50% { opacity: 0.85; transform: scale(1.06); }
}

.fab-icon-wrapper {
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.fab-icon {
  width: 28px;
  height: 28px;
}

.fab-badge {
  position: absolute;
  top: -2px;
  right: -2px;
  background: #10b981;
  color: white;
  font-size: 10px;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 10px;
  border: 2px solid #ffffff;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}

.fab-tooltip {
  position: absolute;
  right: 70px;
  background: #1e293b;
  color: #ffffff;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  white-space: nowrap;
  pointer-events: none;
  opacity: 0;
  transform: translateX(10px);
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.chatbot-fab:hover .fab-tooltip {
  opacity: 1;
  transform: translateX(0);
}

/* =========================================
   2. FLOATING CHAT WINDOW
   ========================================= */
.chat-window {
  width: 400px;
  height: 600px;
  max-width: calc(100vw - 32px);
  max-height: calc(100vh - 100px);
  background: #ffffff;
  border-radius: 20px;
  box-shadow: 0 20px 40px -10px rgba(15, 23, 42, 0.2), 0 8px 16px -4px rgba(15, 23, 42, 0.1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid rgba(226, 232, 240, 0.8);
  backdrop-filter: blur(16px);
}

/* Header */
.chat-header {
  background: linear-gradient(135deg, #312e81 0%, #4338ca 100%);
  color: #ffffff;
  padding: 16px 18px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.bot-avatar {
  position: relative;
  width: 38px;
  height: 38px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(255, 255, 255, 0.25);
}

.bot-avatar svg {
  width: 22px;
  height: 22px;
  color: #ffffff;
}

.status-indicator {
  position: absolute;
  bottom: -1px;
  right: -1px;
  width: 10px;
  height: 10px;
  background-color: #10b981;
  border: 2px solid #312e81;
  border-radius: 50%;
}

.header-text {
  display: flex;
  flex-direction: column;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 700;
  letter-spacing: -0.01em;
}

.model-badge {
  font-size: 10px;
  background: rgba(255, 255, 255, 0.2);
  padding: 1px 6px;
  border-radius: 6px;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.header-status {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.8);
}

.status-dot {
  width: 6px;
  height: 6px;
  background: #34d399;
  border-radius: 50%;
  display: inline-block;
}

.header-actions {
  display: flex;
  gap: 6px;
}

.icon-btn {
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: #ffffff;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.icon-btn:hover {
  background: rgba(255, 255, 255, 0.25);
}

.icon-btn svg {
  width: 16px;
  height: 16px;
}

/* Quick Chips */
.quick-chips-container {
  padding: 10px 16px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.chips-title {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  margin-bottom: 6px;
}

.chips-scroll {
  display: flex;
  gap: 6px;
  overflow-x: auto;
  padding-bottom: 4px;
}

.chips-scroll::-webkit-scrollbar {
  height: 4px;
}

.chips-scroll::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}

.chip-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 5px 10px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  font-size: 12px;
  font-weight: 600;
  color: #334155;
  white-space: nowrap;
  cursor: pointer;
  transition: all 0.2s;
}

.chip-btn:hover {
  background: #eef2ff;
  border-color: #c7d2fe;
  color: #4f46e5;
  transform: translateY(-1px);
}

/* Messages List */
.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  background: #fdfdfd;
}

.chat-messages::-webkit-scrollbar {
  width: 6px;
}

.chat-messages::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 6px;
}

.message-row {
  display: flex;
  gap: 8px;
  max-width: 90%;
}

.user-row {
  align-self: flex-end;
  flex-direction: row-reverse;
}

.bot-row {
  align-self: flex-start;
}

.msg-avatar {
  width: 28px;
  height: 28px;
  border-radius: 8px;
  background: #e0e7ff;
  color: #4f46e5;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.msg-avatar svg {
  width: 16px;
  height: 16px;
}

.message-bubble {
  padding: 10px 14px;
  border-radius: 14px;
  font-size: 13.5px;
  line-height: 1.5;
  position: relative;
  word-break: break-word;
}

.user-row .message-bubble {
  background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
  color: #ffffff;
  border-bottom-right-radius: 4px;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.25);
}

.bot-row .message-bubble {
  background: #f1f5f9;
  color: #1e293b;
  border-bottom-left-radius: 4px;
  border: 1px solid #e2e8f0;
}

.msg-time {
  font-size: 10px;
  opacity: 0.7;
  margin-top: 4px;
  text-align: right;
}

/* Typing Indicator */
.typing-bubble {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
}

.typing-dots {
  display: flex;
  gap: 4px;
}

.typing-dots span {
  width: 6px;
  height: 6px;
  background: #6366f1;
  border-radius: 50%;
  animation: wave 1.3s infinite ease-in-out;
}

.typing-dots span:nth-child(2) { animation-delay: 0.15s; }
.typing-dots span:nth-child(3) { animation-delay: 0.3s; }

@keyframes wave {
  0%, 60%, 100% { transform: translateY(0); }
  30% { transform: translateY(-5px); }
}

.typing-text {
  font-size: 12px;
  color: #64748b;
  font-style: italic;
}

/* Input Area */
.chat-input-area {
  padding: 12px 16px;
  background: #ffffff;
  border-top: 1px solid #e2e8f0;
}

.input-form {
  display: flex;
  align-items: center;
  gap: 8px;
}

.chat-input {
  flex: 1;
  padding: 10px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  font-size: 13.5px;
  font-family: inherit;
  outline: none;
  transition: all 0.2s;
  background: #f8fafc;
}

.chat-input:focus {
  border-color: #6366f1;
  background: #ffffff;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

.send-btn {
  width: 38px;
  height: 38px;
  background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
  border: none;
  border-radius: 12px;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  box-shadow: 0 2px 6px rgba(79, 70, 229, 0.3);
}

.send-btn:hover:not(:disabled) {
  transform: scale(1.05);
}

.send-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.send-btn svg {
  width: 18px;
  height: 18px;
}

.input-hint {
  text-align: center;
  margin-top: 6px;
  font-size: 11px;
  color: #94a3b8;
}

/* =========================================
   3. MARKDOWN STYLES
   ========================================= */
:deep(.markdown-content) {
  font-size: 13.5px;
  line-height: 1.55;
}

:deep(.markdown-content strong) {
  color: #1e1b4b;
  font-weight: 700;
}

:deep(.chat-list-item) {
  margin-left: 18px;
  list-style-type: disc;
  margin-bottom: 3px;
}

:deep(.chat-inline-code) {
  background: #e2e8f0;
  color: #db2777;
  padding: 2px 5px;
  border-radius: 4px;
  font-size: 12px;
  font-family: monospace;
}

:deep(.chat-code-block) {
  background: #1e293b;
  color: #f8fafc;
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 12px;
  margin: 6px 0;
  overflow-x: auto;
}

:deep(.table-responsive) {
  overflow-x: auto;
  margin: 8px 0;
  border-radius: 6px;
  border: 1px solid #cbd5e1;
}

:deep(.chat-table) {
  width: 100%;
  border-collapse: collapse;
  font-size: 12px;
  text-align: left;
}

:deep(.chat-table th) {
  background: #e2e8f0;
  padding: 6px 8px;
  font-weight: 700;
  color: #334155;
  border-bottom: 1px solid #cbd5e1;
}

:deep(.chat-table td) {
  padding: 6px 8px;
  border-bottom: 1px solid #f1f5f9;
  background: #ffffff;
}

:deep(.chat-table tr:last-child td) {
  border-bottom: none;
}

/* =========================================
   4. ANIMATIONS
   ========================================= */
.fab-fade-enter-active, .fab-fade-leave-active {
  transition: all 0.3s ease;
}
.fab-fade-enter-from, .fab-fade-leave-to {
  opacity: 0;
  transform: scale(0.6);
}

.chat-window-pop-enter-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.chat-window-pop-leave-active {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.chat-window-pop-enter-from, .chat-window-pop-leave-to {
  opacity: 0;
  transform: scale(0.85) translateY(20px);
}
</style>
