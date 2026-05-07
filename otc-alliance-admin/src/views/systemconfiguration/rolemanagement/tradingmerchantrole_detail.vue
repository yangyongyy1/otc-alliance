<template>
  <div class="role-permission-wrapper">
    <div class="header">
      <div class="header-actions">
        <el-button
          type="primary"
          @click="handleSave"
          v-permission="
            'Pages.SystemConfiguration.RoleManagement.TradingMerchantRole.BtnSetTradingMerchantRolePermission'
          "
          >保存</el-button
        >
        <el-button @click="handleBack">回退</el-button>
      </div>
    </div>
    <div class="role-permission-page" v-loading="loading">
      <div class="permission-tree">
        <h3>权限菜单</h3>
        <el-tree
          :data="menuList"
          node-key="id"
          show-checkbox
          default-expand-all
          :props="treeProps"
          :default-checked-keys="checkedKeys"
          @check="handleCheck"
          ref="treeRef"
        >
          <template #default="{ node, data }">
            <span>
              <i :class="data.iCon" style="margin-right: 6px"></i>
              {{ data.menuName }}
            </span>
          </template>
        </el-tree>
      </div>
      <div class="permission-tree selected-tree">
        <h3>已分配权限</h3>
        <el-tree
          :data="selectedTreeData"
          node-key="id"
          :props="treeProps"
          default-expand-all
          :show-checkbox="false"
          :expand-on-click-node="false"
          :highlight-current="false"
          :draggable="false"
        >
          <template #default="{ node, data }">
            <span>
              <i :class="data.iCon" style="margin-right: 6px"></i>
              {{ data.menuName }}
            </span>
          </template>
        </el-tree>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from "vue";
import { useRouter } from "vue-router";
import request from "@/utils/request";
import { useRoute } from "vue-router";
import { ElMessage } from "element-plus";
import {
  TraderMeunGetAllWithTree,
  SysRoleMenuGetListAll,
  SysRoleMenuSetRoleMenus,
} from "@/api/tradePermission";

const menuList = ref([]);
const checkedKeys = ref([]);
const userPermissionList = ref([]);
const treeRef = ref();
const route = useRoute();
const router = useRouter();

const treeProps = {
  children: "childs",
  label: "menuName",
};

const fetchMenuList = async () => {
  loading.value = true;
  const { result } = await request(TraderMeunGetAllWithTree);
  menuList.value = result;
  loading.value = false;
};

const loading = ref(false);
const fetchUserPermissionList = async () => {
  SysRoleMenuGetListAll.params = {
    roleID: route.params.id,
  };
  const { result } = await request(SysRoleMenuGetListAll);
  userPermissionList.value = result || [];
  checkedKeys.value = userPermissionList.value.map((item) => item.menuId);
  nextTick(() => {
    treeRef.value && treeRef.value.setCheckedKeys(checkedKeys.value);
  });
};

function filterTreeByChecked(data, checkedIds) {
  return data
    .filter(
      (item) =>
        checkedIds.includes(item.id) ||
        (item.childs &&
          item.childs.some((child) => checkedIds.includes(child.id)))
    )
    .map((item) => ({
      ...item,
      childs: item.childs ? filterTreeByChecked(item.childs, checkedIds) : [],
    }))
    .filter((item) => item.childs.length > 0 || checkedIds.includes(item.id));
}

const selectedTreeData = computed(() => {
  return filterTreeByChecked(menuList.value, checkedKeys.value);
});

function handleCheck(checkedNodes, { checkedKeys: keys }) {
  checkedKeys.value = keys;
}

const handleSave = async () => {
  SysRoleMenuSetRoleMenus.data = {
    roleID: route.params.id,
    menuIds: checkedKeys.value,
  };
  const { result } = await request(SysRoleMenuSetRoleMenus);
  if (result) {
    ElMessage.success("保存成功");
    fetchMenuList();
    fetchUserPermissionList();
  }
};

function handleBack() {
  router.back();
}

onMounted(() => {
  fetchMenuList();
  fetchUserPermissionList();
});
</script>

<style scoped>
.role-permission-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-height: 100vh;
  background: #f5f6fa;
  padding: 0 0 40px 0;
}
.header {
  width: 100%;
  max-width: 1100px;
  display: flex;
  flex-direction: column;
  align-items: center;
  margin: 32px 0 24px 0;
}
.header h2 {
  font-size: 28px;
  font-weight: 600;
  color: #222;
  margin-bottom: 18px;
  letter-spacing: 2px;
}
.header-actions {
  display: flex;
  gap: 18px;
  margin-bottom: 8px;
}
.role-permission-page {
  display: flex;
  gap: 40px;
  justify-content: center;
  width: 100%;
  max-width: 1100px;
}
.permission-tree {
  flex: 1;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 12px #0001;
  padding: 32px 24px 24px 24px;
  min-width: 340px;
  max-width: 480px;
  display: flex;
  flex-direction: column;
}
.selected-tree {
  background: #f8f9fa;
}
.permission-tree h3 {
  font-size: 18px;
  font-weight: 500;
  margin-bottom: 18px;
  color: #333;
  text-align: center;
}
:deep(.el-tree) {
  background: transparent;
}
</style>
