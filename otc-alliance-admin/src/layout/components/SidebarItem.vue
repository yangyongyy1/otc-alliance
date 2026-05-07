<template>
  <div v-if="!item.meta?.hidden">
    <template v-if="!hasChildren(item)">
      <el-menu-item :index="resolvePath(basePath)">
        <el-icon v-if="item.meta?.icon">
          <component :is="item.meta.icon" />
        </el-icon>
        <template #title>
          <span>{{ item.meta?.title }}</span>
        </template>
      </el-menu-item>
    </template>

    <el-sub-menu v-else :index="resolvePath(basePath)">
      <template #title>
        <el-icon v-if="item.meta?.icon">
          <component :is="item.meta.icon" />
        </el-icon>
        <span>{{ item.meta?.title }}</span>
      </template>

      <sidebar-item
        v-for="child in item.children"
        :key="child.name"
        :item="child"
        :base-path="resolvePath(basePath)"
      />
    </el-sub-menu>
  </div>
</template>

<script setup>
import { computed } from "vue";
import path from "path-browserify";

const props = defineProps({
  item: {
    type: Object,
    required: true,
  },
  basePath: {
    type: String,
    default: "",
  },
});

// 判断是否有子菜单
const hasChildren = (item) => {
  return item.children && item.children.length > 0;
};

// 解析路径
const resolvePath = (basePath) => {
  if (props.item.path.startsWith("/")) {
    return props.item.path;
  }
  return path.resolve(basePath, props.item.path);
};
</script>
