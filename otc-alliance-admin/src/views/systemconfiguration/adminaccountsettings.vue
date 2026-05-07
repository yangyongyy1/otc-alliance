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
      addButtonPermission="Pages.SystemConfiguration.AdminAccountSettings.BtnAdd"
      :searchForm="searchForm"
      :operationButtons="operationButtons"
      :operationWidth="300"
    >
      <template #search-isActive="{ form, item, index }">
        <!-- 搜索区内容 -->
        <el-select v-model="form.isActive" :placeholder="t('common.pleaseSelectStatus')">
          <el-option :label="t('common.enable')" :value="true"></el-option>
          <el-option :label="t('common.disable')" :value="false"></el-option>
        </el-select>
      </template>
      <!-- 自定义状态列渲染 -->
      <template #table-isActive="{ row }">
        <el-tag type="success" v-if="row.isActive === true">{{ t('common.enable') }}</el-tag>
        <el-tag type="danger" v-else>{{ t('common.disable') }}</el-tag>
      </template>

      <template #table-roleName="{ row }">
        {{ row.roleNames?row.roleNames[0]:'' }}
      </template>
    </common-table>

    <!-- 新增/编辑对话框 -->
    <UserAdd ref="userAddRef" @refresh="fetchData" />
    <ResetUserPwd ref="resetUserPwdRef" @refresh="fetchData" />
    <ResetGoogle ref="resetGoogleRef" @refresh="fetchData" />
  </div>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getUserList, UserDeActivate, UserActivate, UserDelete } from "@/api/systemUser";
import { useRouter } from "vue-router";
import UserAdd from "@/views/systemconfiguration/components/UserAdd.vue";
import ResetUserPwd from "@/views/systemconfiguration/components/ResetUserPwd.vue";
import ResetGoogle from "@/views/systemconfiguration/components/ResetGoogle.vue";

const { t } = useI18n();

const router = useRouter();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  userAccount: "",
  isActive: "",
  creationDateStart: "",
  creationDateEnd: "",
});

// 搜索表单配置 - 使用 computed 确保语言切换时自动更新
const filterList = computed(() => [
  {
    label: t("system.userName"),
    name: "userAccount",
    type: "text",
  },
  {
    label: t("common.status"),
    name: "isActive",
    type: "slot",
  },
  {
    label: t("common.creationTime"),
    name1: "creationDateStart",
    name2: "creationDateEnd",
    type: "multDatetime",
  },
]);

// 表格列配置 - 使用 computed 确保语言切换时自动更新
const tableColumns = computed(() => [
  { prop: "userName", label: t("system.userName") },
  { prop: "roleName", label: t("system.role"), type: "slot" },
  { prop: "emailAddress", label: t("common.email"), width: "250px" },
  { prop: "isActive", label: t("common.status"), type: "slot" },
  { prop: "remark", label: t("common.remark") },
  { prop: "creationTime", label: t("common.creationTime"), width: "180px" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const userAddRef = ref(null);
const resetUserPwdRef = ref(null);
const resetGoogleRef = ref(null);

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
    label: t("common.edit"),
    type: "text",
    permission:
      "Pages.SystemConfiguration.AdminAccountSettings.BtnEdit",
    onClick: (row) => {
      userAddRef.value.open(row);
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
  userAddRef.value.open();
};

const handleDelete = async (row) => {
  ElMessageBox.confirm(t("system.confirmDeleteUser"), t("common.tip"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  })
    .then(async () => {
      UserDelete.params = { id: row.id };
      const { result } = await request(UserDelete);
      if (result) {
        ElMessage.success(t("common.deleteSuccess"));
        fetchData();
      }
    })
    .catch(() => {});
};

const handleToggleStatus = async (row) => {
  let api = row.isActive ? UserDeActivate : UserActivate;
  const action = row.isActive ? t("common.disable") : t("common.enable");
  ElMessageBox.confirm(t("system.confirmToggleUserStatus", { action }), t("common.tip"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  })
    .then(async () => {
      api.data = { id: row.id };
      const { result } = await request(api);
      fetchData();
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

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  getUserList.params = searchForm;
  const { result } = await request(getUserList);
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
