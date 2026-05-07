<template>
  <div id="app">
    <!-- 加载状态 -->
    <div v-if="isLoading" class="loading-container">
      <div class="loading-content">
        <div class="loading-spinner">
          <div class="spinner"></div>
        </div>
        <div class="loading-text">V-Account Alliance</div>
        <div class="loading-subtitle">{{ t('common.loading') }}</div>
      </div>
    </div>
    
    <!-- 主要内容 -->
    <router-view v-else />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const isLoading = ref(true)

// 立即设置body背景为灰色，防止蓝底闪烁
onMounted(() => {
  // 立即设置body背景
  document.body.style.background = '#f5f7fa'
  
  // 确保有足够的加载时间，让用户看到加载状态
  setTimeout(() => {
    isLoading.value = false
    document.body.classList.add('loaded')
    // 移除内联样式，让CSS类生效
    document.body.style.background = ''
  }, 3000) // 增加加载时间到3秒
})
</script>

<style lang="scss">
#app {
  height: 100%;
}

.loading-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: #f5f7fa !important;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 99999;
  transition: opacity 0.5s ease, visibility 0.5s ease;
  
  // 确保覆盖所有可能的背景
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: #f5f7fa;
    z-index: -1;
  }
}

.loading-content {
  text-align: center;
  animation: fadeInUp 0.6s ease-out;
}

.loading-spinner {
  margin-bottom: 24px;
}

.spinner {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(102, 126, 234, 0.1);
  border-left: 4px solid #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

.loading-text {
  font-size: 28px;
  font-weight: 700;
  color: #667eea;
  margin-bottom: 8px;
  letter-spacing: 1px;
}

.loading-subtitle {
  font-size: 16px;
  color: #6c757d;
  font-weight: 400;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
