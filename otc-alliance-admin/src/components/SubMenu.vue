<template>
  <el-menu
    :default-active="route.path"
    :default-openeds="openMenus"
    class="el-menu-vertical-demo"
    background-color="#001529"
    text-color="#bfcbd9"
    active-text-color="#409EFF"
    :router="true"
  >
    <MenuItem v-for="item in menus" :key="item.path" :item="item" />
  </el-menu>
</template>

<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import MenuItem from "./MenuItem.vue";
import { Setting, User, Lock } from "@element-plus/icons-vue";

const route = useRoute();

const menus = [
  {
    label: "系统管理",
    path: "/system",
    icon: Setting,
    children: [
      {
        label: "用户管理",
        path: "/system/user",
        icon: User,
        children: [
          { label: "用户列表", path: "/system/user/list" },
          { label: "用户角色", path: "/system/user/role" },
        ],
      },
      {
        label: "权限管理",
        path: "/system/permission",
        icon: Lock,
        children: [
          { label: "角色分配", path: "/system/permission/role" },
          { label: "菜单设置", path: "/system/permission/menu" },
        ],
      },
    ],
  },
];

const openMenus = computed(() => {
  const segments = route.path.split("/");
  const open = [];
  for (let i = 2; i <= segments.length; i++) {
    const subPath = segments.slice(0, i).join("/");
    open.push(subPath);
  }
  return open;
});
</script>

<style scoped>
.el-menu-vertical-demo {
  width: 240px;
  min-height: 100vh;
}
</style>
