<template>
  <div class="side-menu-wrapper">
    <el-menu
      :default-active="activeMenu"
      class="side-menu"
      :collapse="isCollapse"
      background-color="transparent"
      text-color="#bfcbd9"
      active-text-color="#ffffff"
      :unique-opened="false"
      :router="true"
      :default-openeds="openMenus"
    >
      <template v-for="item in menuItems" :key="item.key">
        <menu-item :menu-info="item" />
      </template>
    </el-menu>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import { transformMenuToTree } from "@/utils/menuUtils";
import MenuItem from "./MenuItem.vue";

const props = defineProps({
  menuData: {
    type: Array,
    required: true,
  },
  isCollapse: {
    type: Boolean,
    default: false,
  },
});

const route = useRoute();

const activeMenu = computed(() => {
  // 获取当前路由路径
  const currentPath = route.path;
  
  // 如果路径是根路径，返回 dashboard
  if (currentPath === '/' || currentPath === '/dashboard') {
    return '/dashboard';
  }
  
  return currentPath;
});

const openMenus = computed(() => {
  const path = route.path;
  const segments = path.split("/");
  const open = [];

  for (let i = 2; i <= segments.length; i++) {
    const subPath = segments.slice(0, i).join("/");
    if (subPath) open.push(subPath);
  }

  return open;
});

const menuItems = computed(() => {
  const items = transformMenuToTree(props.menuData);
  return items;
});
</script>

<style scoped>
/* 包裹 el-menu 的容器，撑满侧边栏内容区域并启用垂直滚动
   这里减去上方 logo 区高度 70px，避免底部菜单被遮挡，看不到“安全中心”等最后一项 */
.side-menu-wrapper {
  height: calc(100vh - 70px);
  overflow-y: auto;
  overflow-x: hidden;
  padding: 16px 0;
}

/* 菜单样式 */
.side-menu {
  height: 100%;
  border-right: none;
  background: transparent !important;
}

.side-menu:not(.el-menu--collapse) {
  width: 280px;
}

/* 鼠标悬停和激活状态样式 */
.el-menu-item,
:deep(.el-sub-menu__title) {
  margin: 4px 12px;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
    opacity: 0;
    transition: opacity 0.3s ease;
  }
  
  &:hover {
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%) !important;
    transform: translateX(4px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
    
    &::before {
      opacity: 1;
    }
  }
}

/* 激活状态样式 - 使用更高优先级的选择器 */
.side-menu .el-menu-item.is-active,
:deep(.side-menu .el-menu-item.is-active),
:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4) !important;
  transform: translateX(4px) !important;
  
  &::before {
    opacity: 0 !important;
  }
}

/* 子菜单激活状态 */
:deep(.side-menu .el-sub-menu .el-menu-item.is-active),
:deep(.el-sub-menu .el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4) !important;
  transform: translateX(4px) !important;
}

/* 强制覆盖 Element Plus 默认样式 */
:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4) !important;
  transform: translateX(4px) !important;
}

/* 图标样式 */
.el-icon {
  margin-right: 16px;
  width: 1.2em;
  height: 1.2em;
  transition: all 0.3s ease;
}

.el-menu-item:hover .el-icon,
:deep(.el-sub-menu__title:hover .el-icon) {
  transform: scale(1.1);
  color: #667eea;
}

.side-menu .el-menu-item.is-active .el-icon,
:deep(.side-menu .el-menu-item.is-active .el-icon),
:deep(.el-menu-item.is-active .el-icon) {
  color: white !important;
  transform: scale(1.1) !important;
}

/* 子菜单激活状态的图标 */
:deep(.side-menu .el-sub-menu .el-menu-item.is-active .el-icon),
:deep(.el-sub-menu .el-menu-item.is-active .el-icon) {
  color: white !important;
  transform: scale(1.1) !important;
}

/* 强制覆盖图标样式 */
:deep(.el-menu-item.is-active .el-icon) {
  color: white !important;
  transform: scale(1.1) !important;
}

/* 全局激活状态覆盖 */
:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4) !important;
  transform: translateX(4px) !important;
}

:deep(.el-menu-item.is-active .el-icon) {
  color: white !important;
  transform: scale(1.1) !important;
}

/* 滚动条样式 */
.side-menu-wrapper {
  scrollbar-width: thin; /* Firefox */
  scrollbar-color: rgba(102, 126, 234, 0.3) transparent;
}

/* Chrome, Edge, Safari */
.side-menu-wrapper::-webkit-scrollbar {
  width: 6px;
}

.side-menu-wrapper::-webkit-scrollbar-track {
  background: transparent;
}

.side-menu-wrapper::-webkit-scrollbar-thumb {
  background: linear-gradient(45deg, #667eea, #764ba2);
  border-radius: 3px;
  transition: all 0.3s ease;
}

.side-menu-wrapper::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(45deg, #5a6fd8, #6a4190);
}
</style>
