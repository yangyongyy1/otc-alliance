<template>
  <el-sub-menu v-if="hasChildren" :index="menuInfo.key">
    <template #title>
      <el-icon v-if="menuInfo.meta?.icon">
        <component :is="menuInfo.meta.icon" />
      </el-icon>
      <span>{{ menuInfo.meta?.titleKey ? t(menuInfo.meta.titleKey) : menuInfo.meta?.title }}</span>
    </template>

    <MenuItem
      v-for="child in menuInfo.children"
      :key="child.key"
      :menu-info="child"
    />
  </el-sub-menu>

  <el-menu-item v-else :index="menuInfo.key" @click="handleClick">
    <el-icon v-if="menuInfo.meta?.icon">
      <component :is="menuInfo.meta.icon" />
    </el-icon>
    <span>{{ menuInfo.meta?.title }}</span>
  </el-menu-item>
</template>

<script setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";

const router = useRouter();
const { t } = useI18n();

const props = defineProps({
  menuInfo: {
    type: Object,
    required: true,
  },
});

const hasChildren = computed(() => {
  return props.menuInfo?.children && props.menuInfo.children.length > 0;
});

const handleClick = () => {
  if (props.menuInfo.name) {
    // 使用命名路由跳转（如果有）
    router.replace({ name: props.menuInfo.name });
  } else if (props.menuInfo.path) {
    // 确保 path 是绝对路径，防止拼接错误
    const path = props.menuInfo.path.startsWith("/")
      ? props.menuInfo.path
      : "/" + props.menuInfo.path;

    router.push(path);
  }
};
</script>

<style scoped>
/* 移除默认的激活状态样式，让父组件的样式生效 */
.el-menu-item,
:deep(.el-sub-menu__title) {
  &:hover {
    background-color: transparent !important;
  }
}

.el-menu-item.is-active {
  background-color: transparent !important;
}

.el-icon {
  margin-right: 16px;
  width: 1em;
  height: 1em;
}

:deep(.el-menu) {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
}

:deep(.el-menu::-webkit-scrollbar) {
  width: 6px;
}

:deep(.el-menu::-webkit-scrollbar-thumb) {
  background-color: #4a5064;
  border-radius: 3px;
}

:deep(.el-menu::-webkit-scrollbar-track) {
  background-color: #263445;
}
</style>
