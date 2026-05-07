<template>
  <div class="page-container">
    <div class="menu-management-wrapper">
      <div class="action-bar">
        <el-button
          type="primary"
          @click="handleAdd"
          v-permission="
            'Pages.SystemConfiguration.MenuManagement.MerchantBackstageMenu.BtnAdd'
          "
          >增加</el-button
        >
        <el-button type="primary" @click="searchQuery">搜索</el-button>
      </div>
      <el-card class="menu-table-card" v-loading="loading">
        <el-table
          :data="menuList"
          row-key="id"
          border
          :tree-props="{ children: 'childs', hasChildren: 'hasChilds' }"
          style="width: 100%"
          default-expand-all
        >
          <el-table-column align="center" type="selection" width="55" />
          <el-table-column
            align="left"
            label="菜单名称"
            prop="menuName"
            show-overflow-tooltip
          />
          <el-table-column
            align="center"
            label="类型"
            prop="isMenu"
            show-overflow-tooltip
            width="80"
          >
            <template #default="{ row }">
              {{ row.isMenu ? "菜单" : "页面" }}
            </template>
          </el-table-column>
          <el-table-column
            align="center"
            label="路由地址"
            prop="routePath"
            show-overflow-tooltip
          />
          <el-table-column
            align="center"
            label="组件名称"
            prop="componentsPath"
            show-overflow-tooltip
          />
          <el-table-column
            align="center"
            label="排序"
            prop="index"
            show-overflow-tooltip
          />
          <el-table-column
            align="center"
            label="图标"
            prop="iCon"
            show-overflow-tooltip
            width="100"
          >
            <template #default="{ row }">
              <vab-icons :icon="row.iCon" />
            </template>
          </el-table-column>
          <el-table-column
            align="center"
            label="操作"
            show-overflow-tooltip
            width="140"
          >
            <template #default="{ row }">
              <div>
                <el-button
                  link
                  @click="handleEdit(row)"
                  v-permission="
                    'Pages.SystemConfiguration.MenuManagement.MerchantBackstageMenu.BtnEdit'
                  "
                >
                  编辑
                </el-button>
                <el-button
                  link
                  @click="handleDelete(row)"
                  v-permission="
                    'Pages.SystemConfiguration.MenuManagement.MerchantBackstageMenu.BtnDelete'
                  "
                >
                  删除
                </el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>
    <BusinessMenuAddEdit
      ref="BusinessMenuAddEditRef"
      :visible="visible"
      :isEdit="isEdit"
      :menuData="menuData"
      @update:visible="visible = $event"
      @success="searchQuery"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from "vue";
import { SysMeunGetAllWithTree, SysMeunDelete } from "@/api/businessPermission";
import request from "@/utils/request";
import BusinessMenuAddEdit from "@/views/systemconfiguration/components/BusinessMenuAddEdit.vue";
import { ElMessage, ElMessageBox } from "element-plus";

const BusinessMenuAddEditRef = ref(null);
const menuList = ref([]);
const loading = ref(false);
const visible = ref(false);
const isEdit = ref(false);
const menuData = ref({});

const searchQuery = () => {
  getMenuList();
};

onMounted(async () => {
  await getMenuList();
});

onBeforeUnmount(() => {
  loading.value = false;
  visible.value = false;
  menuData.value = {};
});

async function getMenuList() {
  try {
    loading.value = true;
    const res = await request(SysMeunGetAllWithTree);
    menuList.value = res.result;
  } catch (error) {
    ElMessage.error("获取菜单列表失败");
  } finally {
    loading.value = false;
  }
}

function handleAdd() {
  BusinessMenuAddEditRef.value.open();
}

function handleEdit(row) {
  BusinessMenuAddEditRef.value.open(row);
}

function handleDelete(row) {
  //询问
  ElMessageBox.confirm("确定删除该菜单吗？", "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    try {
      SysMeunDelete.params = { id: row.id };
      await request(SysMeunDelete);
      ElMessage.success("删除成功");
      searchQuery();
    } catch (error) {
      ElMessage.error("删除菜单失败");
    }
  });
}
</script>

<style scoped>
.page-container {
  position: relative;
  width: 100%;
  height: 100%;
}

.menu-management-wrapper {
  padding: 24px;
  background: #f5f6fa;
  min-height: 100vh;
  position: relative;
  z-index: 1;
}

.action-bar {
  display: flex;
  justify-content: center;
  margin-bottom: 24px;
}

.menu-table-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}
</style>
