<template>
  <!-- Nếu đang ở trang đăng nhập, chỉ render nội dung login -->
  <div v-if="isLoginPage" class="login-layout">
    <RouterView />
  </div>

  <!-- Layout quản trị chính sau khi đăng nhập -->
  <div v-else class="app-container">
    <!-- Sidebar Navigation -->
    <Sidebar />

    <!-- Main Workspace -->
    <div class="main-content">
      <!-- Top Header -->
      <Header />

      <!-- Active Page Body -->
      <main class="page-body">
        <RouterView />
      </main>
    </div>

    <!-- Chatbot Widget AI Tra Cứu -->
    <ChatbotWidget />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import Sidebar from '@/components/layout/Sidebar.vue'
import Header from '@/components/layout/Header.vue'
import ChatbotWidget from '@/components/chat/ChatbotWidget.vue'

const route = useRoute()
const isLoginPage = computed(() => route.name === 'login' || route.path === '/login')
</script>

<style>
/* Base imports in main.css */
.login-layout {
  min-height: 100vh;
  width: 100%;
}
</style>
