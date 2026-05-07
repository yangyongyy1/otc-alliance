<template>
  <div class="tabs-view-container" v-if="visitedViews.length > 0">
    <el-scrollbar class="tabs-scrollbar">
      <div class="tabs-content">
        <div
          v-for="tag in visitedViews"
          :key="tag.path"
          :class="['tabs-item', isActive(tag) ? 'active' : '']"
          @click="handleClickTab(tag)"
          @contextmenu.prevent="openContextMenu(tag, $event)"
        >
          <span class="tabs-item-title">{{ tag.meta?.titleKey ? t(tag.meta.titleKey) : tag.title }}</span>
          <el-icon
            v-if="!isAffix(tag)"
            class="tabs-item-close"
            @click.stop="handleCloseTab(tag)"
          >
            <Close />
          </el-icon>
        </div>
      </div>
    </el-scrollbar>
  </div>

  <!-- 右键菜单 - 使用 Teleport 挂载到 body -->
  <Teleport to="body">
    <ul
      v-show="contextMenuVisible"
      :style="{ left: contextMenuLeft + 'px', top: contextMenuTop + 'px' }"
      class="context-menu"
    >
      <!-- <li @click="refreshSelectedTag(selectedTag)">
        <el-icon><Refresh /></el-icon>
        <span>刷新页面</span>
      </li> -->
      <li v-if="!isAffix(selectedTag)" @click="handleCloseTab(selectedTag)">
        <el-icon><Close /></el-icon>
        <span>{{ t('tabs.closeCurrent') }}</span>
      </li>
      <li @click="closeOthersTabs">
        <el-icon><CircleClose /></el-icon>
        <span>{{ t('tabs.closeOthers') }}</span>
      </li>
      <li @click="closeLeftTabs">
        <el-icon><Back /></el-icon>
        <span>{{ t('tabs.closeLeft') }}</span>
      </li>
      <li @click="closeRightTabs">
        <el-icon><Right /></el-icon>
        <span>{{ t('tabs.closeRight') }}</span>
      </li>
      <li @click="closeAllTabs(selectedTag)">
        <el-icon><CircleClose /></el-icon>
        <span>{{ t('tabs.closeAll') }}</span>
      </li>
    </ul>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch, nextTick } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { useTabsStore } from "@/store/modules/tabs";
import {
  Close,
  Refresh,
  CircleClose,
  Back,
  Right,
} from "@element-plus/icons-vue";

const route = useRoute();
const router = useRouter();
const tabsStore = useTabsStore();
const { t } = useI18n();

const contextMenuVisible = ref(false);
const contextMenuLeft = ref(0);
const contextMenuTop = ref(0);
const selectedTag = ref({});

// 获取所有访问过的页面
const visitedViews = computed(() => tabsStore.visitedViews);

// 判断是否是当前激活的标签
const isActive = (tag) => {
  return tag.path === route.path;
};

// 判断是否是固定标签（不能关闭的）
const isAffix = (tag) => {
  return tag.meta?.affix;
};

// 点击标签切换页面
const handleClickTab = (tag) => {
  if (route.path !== tag.path) {
    router.push({ path: tag.path });
  }
};

// 关闭标签
const handleCloseTab = async (tag) => {
  const { visitedViews } = await tabsStore.delView(tag);
  
  // 如果关闭的是当前页面，需要跳转到最后一个标签
  if (isActive(tag)) {
    toLastView(visitedViews, tag);
  }
};

// 跳转到最后一个标签
const toLastView = (visitedViews, view) => {
  const latestView = visitedViews.slice(-1)[0];
  if (latestView) {
    router.push(latestView.path);
  } else {
    // 如果没有标签了，跳转到首页
    if (view.name === "Dashboard") {
      // 如果关闭的是仪表盘，重新加载页面
      router.replace({ path: "/redirect" + view.path });
    } else {
      router.push("/");
    }
  }
};

// 打开右键菜单
const openContextMenu = (tag, e) => {
  const menuMinWidth = 140; // 菜单最小宽度
  const menuHeight = 240; // 菜单大致高度
  
  // 获取鼠标位置
  let left = e.clientX;
  let top = e.clientY;
  
  // 防止菜单超出右边界
  const offsetWidth = window.innerWidth;
  const maxLeft = offsetWidth - menuMinWidth - 5;
  if (left > maxLeft) {
    left = maxLeft;
  }
  
  // 防止菜单超出底部
  const offsetHeight = window.innerHeight;
  const maxTop = offsetHeight - menuHeight - 5;
  if (top > maxTop) {
    top = maxTop;
  }
  
  // 防止菜单超出左边界
  if (left < 5) {
    left = 5;
  }
  
  // 防止菜单超出顶部
  if (top < 5) {
    top = 5;
  }

  contextMenuTop.value = top;
  contextMenuLeft.value = left;
  contextMenuVisible.value = true;
  selectedTag.value = tag;
};

// 关闭右键菜单
const closeContextMenu = () => {
  contextMenuVisible.value = false;
};

// 刷新选中的标签
const refreshSelectedTag = (view) => {
  tabsStore.delCachedView(view).then(() => {
    const { fullPath } = view;
    nextTick(() => {
      router.replace({
        path: "/redirect" + fullPath,
      });
    });
  });
  closeContextMenu();
};

// 关闭其他标签
const closeOthersTabs = () => {
  router.push(selectedTag.value.path);
  tabsStore.delOthersViews(selectedTag.value);
  closeContextMenu();
};

// 关闭左侧标签
const closeLeftTabs = () => {
  tabsStore.delLeftViews(selectedTag.value);
  if (!isActive(selectedTag.value)) {
    router.push(selectedTag.value.path);
  }
  closeContextMenu();
};

// 关闭右侧标签
const closeRightTabs = () => {
  tabsStore.delRightViews(selectedTag.value);
  if (!isActive(selectedTag.value)) {
    router.push(selectedTag.value.path);
  }
  closeContextMenu();
};

// 关闭所有标签
const closeAllTabs = (view) => {
  tabsStore.delAllViews().then(({ visitedViews }) => {
    if (isAffix(view)) {
      return;
    }
    toLastView(visitedViews, view);
  });
  closeContextMenu();
};

// 监听路由变化，添加标签
watch(
  route,
  () => {
    if (route.name && route.meta?.title) {
      tabsStore.addView(route);
    }
  },
  { immediate: true }
);

// 监听点击事件，关闭右键菜单
watch(contextMenuVisible, (value) => {
  if (value) {
    document.body.addEventListener("click", closeContextMenu);
  } else {
    document.body.removeEventListener("click", closeContextMenu);
  }
});
</script>

<style lang="scss" scoped>
.tabs-view-container {
  height: 44px;
  width: 100%;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.12), 0 0 3px 0 rgba(0, 0, 0, 0.04);
  position: relative;
  z-index: 9;
}

.tabs-scrollbar {
  height: 44px;
  
  :deep(.el-scrollbar__wrap) {
    height: 54px; // 比容器高10px，隐藏横向滚动条
  }
  
  :deep(.el-scrollbar__view) {
    height: 44px;
  }
}

.tabs-content {
  display: flex;
  align-items: center;
  height: 44px;
  padding: 0 8px;
  min-width: min-content;
}

.tabs-item {
  display: flex;
  align-items: center;
  height: 32px;
  padding: 0 12px;
  margin: 0 4px;
  font-size: 13px;
  color: #495060;
  background: #f5f7fa;
  border: 1px solid #e8eaec;
  border-radius: 4px;
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
  transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);
  position: relative;

  &:hover {
    color: #667eea;
    background: #ecf0fe;
    border-color: #d4ddfe;

    .tabs-item-close {
      opacity: 1;
      transform: scale(1);
    }
  }

  &.active {
    color: #fff;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-color: #667eea;
    box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);

    .tabs-item-title {
      font-weight: 500;
    }

    .tabs-item-close {
      color: #fff;
      opacity: 1;
      transform: scale(1);

      &:hover {
        background: rgba(255, 255, 255, 0.2);
        color: #fff;
      }
    }
  }

  .tabs-item-title {
    margin-right: 4px;
    font-size: 13px;
  }

  .tabs-item-close {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transform: scale(0.8);
    transition: all 0.2s;
    margin-left: 4px;

    &:hover {
      background: #f0f0f0;
      color: #f56c6c;
    }
  }
}

// 右键菜单
.context-menu {
  position: fixed;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15), 0 0 1px rgba(0, 0, 0, 0.1);
  z-index: 3000;
  list-style-type: none;
  padding: 6px;
  margin: 0;
  font-size: 13px;
  color: #333;
  min-width: 140px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(0, 0, 0, 0.06);
  animation: contextMenuSlide 0.2s cubic-bezier(0.4, 0, 0.2, 1);

  li {
    display: flex;
    align-items: center;
    padding: 10px 14px;
    cursor: pointer;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    border-radius: 6px;
    margin: 2px 0;
    position: relative;
    overflow: hidden;

    &::before {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      width: 3px;
      height: 100%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      transform: scaleY(0);
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .el-icon {
      margin-right: 10px;
      font-size: 15px;
      transition: all 0.3s;
      color: #909399;
    }

    span {
      font-weight: 500;
    }

    &:hover {
      background: linear-gradient(90deg, rgba(102, 126, 234, 0.08) 0%, rgba(118, 75, 162, 0.08) 100%);
      color: #667eea;
      padding-left: 18px;

      &::before {
        transform: scaleY(1);
      }

      .el-icon {
        color: #667eea;
        transform: scale(1.1);
      }
    }

    &:active {
      background: linear-gradient(90deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%);
      transform: scale(0.98);
    }

    // 第一个和最后一个菜单项样式
    &:first-child {
      margin-top: 0;
    }

    &:last-child {
      margin-bottom: 0;
    }
  }
}

// 右键菜单出现动画
@keyframes contextMenuSlide {
  from {
    opacity: 0;
    transform: translateY(-8px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>

