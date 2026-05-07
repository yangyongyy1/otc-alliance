<template>
  <el-menu
    :default-active="activeMenu"
    class="side-menu"
    :collapse="isCollapse"
    background-color="#304156"
    text-color="#bfcbd9"
    active-text-color="#409EFF"
    :unique-opened="false"
    :router="true"
    :default-openeds="openMenus"
  >
    <template v-for="item in menuItems" :key="item.key">
      <menu-item :menu-info="item" />
    </template>
  </el-menu>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import { transformMenuToTree } from "@/utils/menuUtils";
import MenuItem from "@/components/MenuItem.vue";

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
const menuItems = computed(() => transformMenuToTree(props.menuData));
const activeMenu = computed(() => route.path);
</script>

<style scoped>
.side-menu {
  height: 100%;
  border-right: none;
}

.side-menu:not(.el-menu--collapse) {
  width: 250px;
}

.el-menu-item,
:deep(.el-sub-menu__title) {
  &:hover {
    background-color: #263445 !important;
  }
}

.el-menu-item.is-active {
  background-color: #263445 !important;
}

.el-icon {
  margin-right: 16px;
  width: 1em;
  height: 1em;
}
</style>
