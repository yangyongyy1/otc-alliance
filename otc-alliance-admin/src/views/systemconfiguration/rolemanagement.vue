<template>
  <div class="acquiring-container">
    <common-table
      ref="tableRef"
      :filter-list="filterList"
      :table-columns="tableColumns"
      :table-data="tableData"
      :total="total"
      :loading="loading"
      @search="handleSearch"
      @add="handleAdd"
      addButtonPermission="Pages.SystemConfiguration.RoleManagement.BtnAdd"
      :searchForm="searchForm"
      :operationButtons="operationButtons"
      :showPagination="false"
    >
      <!-- 自定义状态列渲染 -->
      <template #auditStatusName="{ row }">
        <el-tag :type="row.auditStatus === 0 ? 'success' : 'danger'">
          {{ row.auditStatusName }}
        </el-tag>
      </template>
    </common-table>

    <!-- 新增/编辑对话框 -->
    <RoleAdd ref="roleAddRef" @refresh="fetchData" />
  </div>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getRoleList, deleteRole } from "@/api/permission";
import { useRouter } from "vue-router";
import RoleAdd from "@/views/systemconfiguration/components/RoleAdd.vue";

const { t } = useI18n();

const router = useRouter();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  businessType: 0,
});

// 搜索表单配置 - 使用 computed 确保语言切换时自动更新
const filterList = computed(() => [
  {
    label: t("common.creationTime"),
    name1: "creationDateStart",
    name2: "creationDateEnd",
    type: "multDatetime",
  },
]);

// 表格列配置 - 使用 computed 确保语言切换时自动更新
const tableColumns = computed(() => [
  { prop: "name", label: t("system.roleName") },
  { prop: "displayName", label: t("system.role") },
  { prop: "description", label: t("common.remark") },
  { prop: "creationTime", label: t("common.creationTime") },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const roleAddRef = ref(null);

// 对话框相关
const dialogVisible = ref(false);
const dialogType = ref("add");
const submitLoading = ref(false);
const formRef = ref(null);
const form = reactive({
  merchantName: "",
  merchantNo: "",
  contactPerson: "",
  contactPhone: "",
  status: 1,
});

// 操作按钮配置 - 使用 computed 确保语言切换时自动更新
const operationButtons = computed(() => [
  {
    label: t("common.view"),
    type: "text",
    permission:
      "Pages.SystemConfiguration.RoleManagement.BtnEdit",
    onClick: (row) => {
      router.push({
        path: `/systemConfiguration/roleManagement_detail/${row.id}`,
      });
    },
  },
  {
    label: t("common.delete"),
    type: "text",
    permission:
      "Pages.SystemConfiguration.RoleManagement.BtnDelete",
    onClick: (row) => {
      handleDelete(row);
    },
  },
]);

// 搜索处理
const handleSearch = (form) => {
  // TODO: 实现搜索逻辑
  fetchData(form);
};

// 重置处理
const handleReset = () => {
  fetchData();
};

// 新增处理
const handleAdd = () => {
  roleAddRef.value.open();
};

const handleDelete = async (row) => {
  ElMessageBox.confirm(t("system.confirmDeleteRole"), t("common.tip"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  })
    .then(async () => {
      loading.value = true;
      deleteRole.params = { id: row.id };
      const { success } = await request(deleteRole);
      loading.value = false;
      if (success) {
        ElMessage.success(t("common.deleteSuccess"));
        fetchData();
      }
      else {
        ElMessage.error(result?.error?.message || t("common.deleteFailed"));
      }
    })
    .catch(() => {});
};

// 编辑处理
const handleEdit = (row) => {
  dialogType.value = "edit";
  dialogVisible.value = true;
  // 填充表单数据
  Object.keys(form).forEach((key) => {
    form[key] = row[key];
  });
};

// 切换状态
const handleToggleStatus = (row) => {
  const action = row.status === 1 ? t("common.disable") : t("common.enable");
  ElMessageBox.confirm(t("system.confirmToggleRoleStatus", { action }), t("common.tip"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  })
    .then(() => {
      // TODO: 实现状态切换逻辑
      ElMessage.success(t("common.operationSuccess"));
      fetchData();
    })
    .catch(() => {});
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  getRoleList.params = searchForm;
  const { result } = await request(getRoleList);
  tableData.value = result.items;
  total.value = result.totalCount;
  loading.value = false;
};

// 初始化
fetchData();
</script>

<style lang="scss" scoped>
.acquiring-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}
</style>
