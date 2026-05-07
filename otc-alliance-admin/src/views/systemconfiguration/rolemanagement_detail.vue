<template>
  <div class="role-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>{{ t('system.roleDetail') }}</h1>
      <div class="header-button">
        <el-button
          type="primary"
          v-permission="
            'Pages.SystemConfiguration.RoleManagement.BtnGrantPermission'
          "
          @click="saveRole"
          :loading="saveLoading"
          >{{ t('common.save') }}</el-button
        >
        <el-button type="primary" @click="router.back()">{{ t('common.return') }}</el-button>
      </div>
    </div>

    <!-- 角色基本信息表单 -->
    <el-form :model="roleDetail" label-width="100px" class="role-form">
      <el-row :gutter="20">
        <el-col :span="8">
          <el-form-item :label="t('system.roleIdentifier')">
            <el-input v-model="roleDetail.name" :placeholder="t('system.pleaseEnterRoleIdentifier')" />
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item :label="t('system.roleName')">
            <el-input
              v-model="roleDetail.displayName"
              :placeholder="t('system.pleaseEnterRoleName')"
            />
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item :label="t('system.roleDescription')">
            <el-input
              v-model="roleDetail.description"
              :placeholder="t('system.pleaseEnterRoleDescription')"
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>

    <div class="main-content">
      <!-- 左侧权限树 -->
      <div class="permission-tree">
        <h2>{{ t('system.permissionList') }}</h2>
        <el-input
          v-model="searchQuery"
          :placeholder="t('system.searchPermission')"
          prefix-icon="el-icon-search"
          clearable
          class="search-input"
        />
        <el-tree
          ref="permissionTree"
          :data="permissionTreeData"
          :props="defaultProps"
          show-checkbox
          node-key="name"
          :default-checked-keys="checkedPermissions"
          @check="handleCheck"
        >
          <template #default="{ node, data }">
            <div class="custom-tree-node">
              <span>{{ data.displayName }}</span>
            </div>
          </template>
        </el-tree>
      </div>

      <!-- 右侧已授权权限 -->
      <div class="granted-permissions">
        <h2>{{ t('system.grantedPermissions') }}</h2>
        <div class="stats"></div>
        <div class="permission-list">
          <el-tree
            :data="grantedPermissionsTreeData"
            :props="defaultProps"
            :default-expand-all="true"
            node-key="name"
          >
            <template #default="{ node, data }">
              <div class="custom-tree-node">
                <span>{{ data.displayName }}</span>
              </div>
            </template>
          </el-tree>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from "vue";
import { useI18n } from "vue-i18n";
import {
  getRoleDetail,
  RoleGetAllPermissionsWithLevel,
  RoleUpdate,
} from "@/api/permission";
import request from "@/utils/request";
import { useRoute, useRouter } from "vue-router";
import { ElMessage } from "element-plus";

const { t } = useI18n();

const route = useRoute();
const router = useRouter();

const loading = ref(false);
const saveLoading = ref(false);

const roleDetail = ref({
  name: "",
  displayName: "",
  description: "",
  grantedPermissions: [],
});
const permissions = ref([]);
const searchQuery = ref("");
const permissionTree = ref(null);

const defaultProps = {
  children: "children",
  label: "displayName",
};

// 将扁平权限数据转换为树形结构
const permissionTreeData = computed(() => {
  const buildTree = (items, parentName = null) => {
    return items
      .filter((item) => item.parentName === parentName)
      .map((item) => ({
        ...item,
        children: buildTree(items, item.name),
      }));
  };
  return buildTree(permissions.value);
});

// 已授权的权限树形数据
const grantedPermissionsTreeData = computed(() => {
  const buildTree = (items, parentName = null) => {
    const nodes = items.filter((item) => item.parentName === parentName);
    return nodes
      .map((item) => {
        const children = buildTree(items, item.name);
        // 如果当前节点被选中，或者有子节点被选中，则保留该节点
        const isSelected = roleDetail.value?.grantedPermissions?.includes(
          item.name
        );
        const hasSelectedChildren = children.length > 0;

        if (isSelected || hasSelectedChildren) {
          return {
            ...item,
            children,
          };
        }
        return null;
      })
      .filter(Boolean); // 过滤掉 null 值
  };
  return buildTree(permissions.value);
});

// 已授权权限数量（只计算叶子节点）

// 当前选中的权限
const checkedPermissions = computed(() => {
  return roleDetail.value?.grantedPermissions || [];
});

const fetchRoleDetail = async () => {
  try {
    loading.value = true;
    getRoleDetail.params = { id: route.params.id };
    const { result } = await request(getRoleDetail);
    roleDetail.value = result || roleDetail.value;
    const granted = roleDetail.value.grantedPermissions || [];
    await nextTick();
    if (permissionTree.value && granted.length >= 0) {
      permissionTree.value.setCheckedKeys(granted);
    }
  } catch (error) {
    ElMessage.error(t("system.getRoleDetailFailed"));
  } finally {
    loading.value = false;
  }
};

const fetchPermissions = async () => {
  try {
    loading.value = true;
    const { result } = await request(RoleGetAllPermissionsWithLevel);
    permissions.value = result.items;
  } catch (error) {
    ElMessage.error(t("system.getPermissionListFailed"));
  } finally {
    loading.value = false;
  }
};

const handleCheck = (data, checked) => {
  // 只获取完全选中的节点
  const checkedNodes = permissionTree.value.getCheckedNodes();

  // 更新已授权的权限列表
  roleDetail.value.grantedPermissions = checkedNodes.map((node) => node.name);
};

const removePermission = (permission) => {
  const index = roleDetail.value.grantedPermissions.indexOf(permission.name);
  if (index > -1) {
    roleDetail.value.grantedPermissions.splice(index, 1);
    permissionTree.value.setChecked(permission.name, false);
  }
};

const saveRole = async () => {
  if (!permissionTree.value) {
    ElMessage.error(t("common.saveFailed"));
    return;
  }
  try {
    saveLoading.value = true;
    // 保存前从树重新收集当前勾选（含半选），确保与界面一致
    const checkedKeys = permissionTree.value.getCheckedKeys() || [];
    const halfCheckedKeys = permissionTree.value.getHalfCheckedKeys
      ? permissionTree.value.getHalfCheckedKeys()
      : [];
    roleDetail.value.grantedPermissions = [
      ...new Set([...checkedKeys, ...halfCheckedKeys]),
    ];

    const payload = {
      id: roleDetail.value.id,
      name: roleDetail.value.name,
      displayName: roleDetail.value.displayName,
      description: roleDetail.value.description,
      grantedPermissions: roleDetail.value.grantedPermissions || [],
    };
    RoleUpdate.data = payload;
    await request(RoleUpdate);
    ElMessage.success(t("common.saveSuccess"));
    await fetchRoleDetail();
    await fetchPermissions();
  } catch (error) {
    ElMessage.error(error?.error?.message || error?.message || t("common.saveFailed"));
  } finally {
    saveLoading.value = false;
  }
};

fetchRoleDetail();
fetchPermissions();
</script>

<style scoped>
.role-detail-container {
  padding: 20px;
  height: 100%;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.main-content {
  display: flex;
  gap: 20px;
  height: calc(100vh - 120px);
  position: relative;
}

.permission-tree {
  flex: 1;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.granted-permissions {
  flex: 1;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.search-input {
  margin-bottom: 20px;
}

.custom-tree-node {
  display: flex;
  align-items: center;
  width: 100%;
}

.permission-description {
  color: #909399;
  font-size: 12px;
}

.stats {
  margin-bottom: 20px;
}

.stat-card {
  margin-bottom: 20px;
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #409eff;
  text-align: center;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.permission-list {
  height: calc(100% - 120px);
  overflow-y: auto;
  padding-right: 10px;
}

.permission-list::-webkit-scrollbar {
  width: 6px;
}

.permission-list::-webkit-scrollbar-thumb {
  background-color: #dcdfe6;
  border-radius: 3px;
}

.permission-list::-webkit-scrollbar-track {
  background-color: #f5f7fa;
}

h2 {
  margin-bottom: 20px;
  color: #303133;
  font-size: 18px;
}

.role-form {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.node-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

:deep(.el-table) {
  .cell {
    white-space: nowrap;
    padding: 0 8px;
  }
}

:deep(.el-button--text) {
  padding: 0 4px;
  margin: 0 2px;
  height: 24px;
  line-height: 24px;
  min-width: auto;
}

:deep(.el-button + .el-button) {
  margin-left: 4px;
}

:deep(.el-table__row) {
  .el-button {
    padding: 0 4px;
    margin: 0 2px;
  }
}

:deep(.el-table__cell) {
  padding: 8px 0;
}
</style>
