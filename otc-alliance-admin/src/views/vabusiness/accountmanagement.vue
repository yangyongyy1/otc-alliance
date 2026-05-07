<template>
  <div class="acquiring-container">
    <!-- 表格工具栏 -->
    

    <common-table
      ref="tableRef"
      :filter-list="filterList"
      :table-columns="tableColumns"
      :table-data="tableData"
      :total="total"
      :loading="loading"
      @search="handleSearch"
      :searchForm="searchForm"
      :show-add-button="false"
      :operationButtons="operationButtons"
      :sortable-fields="['status', 'creationTime','accountName']"
    >
      <!-- 自定义账户币种列渲染 -->
      <template #table-currency="{ row }">
        {{ row.currency || "--" }}
      </template>

      <!-- 自定义所属联盟列渲染 -->
      <template #table-allianceName="{ row }">
        {{ row.user.alliance.name }}
      </template>

      <!-- 自定义类型列渲染 -->
      <template #table-accountUserType="{ row }">
        <el-tag>{{ getAccountUserTypeLabel(row.user.userType) }}</el-tag>
      </template>
      

      <!-- 自定义名称列渲染 -->
      <template #table-accountName="{ row }">
        {{ row.accountName || "--" }}
      </template>

      <!-- 自定义邮箱列渲染 -->
      <template #table-email="{ row }">
        {{ row.user.email || "--" }}
      </template>

      <!-- 自定义状态列渲染 -->
      <template #table-status="{ row }">
        <el-tag :type="getAccountStatusTagType(row.status)">
          {{ getAccountStatusLabel(row.status) }}
        </el-tag>
      </template>

      <!-- 自定义时间列渲染 -->
      <template #table-creationTime="{ row }">
        <p>{{ t('common.creationTime') }}：{{ row.creationTime || "--" }}</p>
        <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime || "--" }}</p>
      </template>
    </common-table>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { Download } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getAllVAAccountList, getAccountCurrencies } from "@/api/VAAccount";
import { getEnum } from "@/api/enum";
import { getAllianceOptions } from "@/api/allianceManagement";

const router = useRouter();
const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 50,
  skipCount: 0,
  name: "",
  email: "",
  allianceId: "",
  currency: "",
  accountUserType: "",
  accountStatus: "",
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
});

const allianceOptions = ref([]);
const currencyOptions = ref([]);
const accountUserTypeOptions = ref([]);
const accountStatusOptions = ref([]);

// 初始化选项数据
const initOptions = async () => {
  try {
    // 获取联盟选项
    const { result: allianceResult } = await request(getAllianceOptions);
    allianceOptions.value = allianceResult.map(item => ({
      label: item.name,
      value: item.id,
    }));

    // 获取币种列表
    const { result: currencyResult } = await request(getAccountCurrencies);
    currencyOptions.value = (currencyResult || []).map(item => ({
      label: item,
      value: item,
    }));

    // 获取枚举选项
    const [accountUserTypeResult, accountStatusResult] = await Promise.all([
      getEnum("AccountUserType"),
      getEnum("VAStatus"),
    ]);

    accountUserTypeOptions.value = (accountUserTypeResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    accountStatusOptions.value = (accountStatusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (error) {
  }
};

// 搜索表单配置
const filterList = ref([
  {
    label: t("account.accountName"),
    name: "accountName",
    type: "text",
  },
  {
    label: t("user.email"),
    name: "email",
    type: "text",
  },
  {
    label: t("user.allianceName"),
    name: "allianceId",
    type: "select",
    options: allianceOptions,
  },
  {
    label: t("account.currency"),
    name: "currency",
    type: "select",
    options: currencyOptions,
  },
  {
    label: t("user.userType"),
    name: "accountUserType",
    type: "select",
    options: accountUserTypeOptions,
  },
  {
    label: t("common.status"),
    name: "status",
    type: "select",
    options: accountStatusOptions,
  },
  {
    label: t("common.creationTime"),
    name1: "creationTimeStart",
    name2: "creationTimeEnd",
    type: "multDatetime",
  },
  {
    label: t("common.lastModificationTime"),
    name1: "modificationTimeStart",
    name2: "modificationTimeEnd",
    type: "multDatetime",
  },
]);

// 表格列配置
const tableColumns = ref([
  { prop: "currency", label: t("account.currency"), type: "slot" },
  { prop: "allianceName", label: t("user.allianceName"), type: "slot" },
  { prop: "accountUserType", label: t("common.type"), type: "slot" },
  { prop: "accountName", label: t("account.accountName"), type: "slot" },
  { prop: "email", label: t("user.email"), type: "slot" },
  { prop: "status", label: t("common.status"), type: "slot" },
  { prop: "creationTime", label: t("common.time"), type: "slot" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);

// 操作按钮配置
const operationButtons = ref([
  {
    label: t("account.detail"),
    type: "text",
    permission: "Pages.VABusiness.AccountManagement.BtnViewDetails",
    onClick: (row) => {
      router.push({
        path: `/vabusiness/accountmanagement_detail/${row.id}`,
      });
    },
  },
]);

// 获取联盟名称
const getAllianceName = (allianceId) => {
  const alliance = allianceOptions.value.find(item => item.value === allianceId);
  return alliance ? alliance.label : "--";
};

// 获取账户用户类型标签
const getAccountUserTypeLabel = (accountUserType) => {
  const option = accountUserTypeOptions.value.find(item => item.value === accountUserType);
  return option ? option.label : "--";
};

// 获取账户状态标签类型
const getAccountStatusTagType = (accountStatus) => {
  // 处理 null/undefined 值
  if (accountStatus === null || accountStatus === undefined) {
    return "info";
  }
  // 根据枚举值映射标签类型
  const statusMap = {
    "NOT_SUBMITTED": "info",      // 未提交（默认状态）
    "PENDDING": "warning",         // 待定
    "ACTIVE": "success",          // 活动
    "FAILED": "danger",           // 失败
    "SUSPENDED": "warning",       // 暂停
    "CLOSED": "info",             // 关闭
    // 兼容数字值（如果后端返回数字）
    0: "info",      // 未提交
    1: "warning",   // 待定
    2: "success",   // 活动
    3: "danger",    // 失败
    4: "warning",   // 暂停
    5: "info",      // 关闭
  };
  return statusMap[accountStatus] || "info";
};

// 获取账户状态标签文本
const getAccountStatusLabel = (accountStatus) => {
  const option = accountStatusOptions.value.find(item => item.value === accountStatus);
  return option ? option.label : "--";
};

// 搜索处理
const handleSearch = (form) => {
  fetchData();
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getAllVAAccountList.params = searchForm;
  try {
    const { result } = await request(getAllVAAccountList);
    tableData.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (error) {
    ElMessage.error(t("common.operationFailed"));
  } finally {
    loading.value = false;
  }
};

// 导出功能
const handleExport = () => {
  if (!tableData.value.length) {
    ElMessage.warning(t("common.noData"));
    return;
  }
  const headers = [
    "#",
    t("account.currency"),
    t("user.allianceName"),
    t("common.type"),
    t("account.accountName"),
    t("user.email"),
    t("common.status"),
    t("common.creationTime"),
    t("common.lastModificationTime"),
  ];
  const indexMethod = (index) => {
    return (searchForm.pageNo - 1) * searchForm.maxResultCount + index + 1;
  };
  const rows = tableData.value.map((row, idx) => [
    indexMethod(idx),
    row.currency || "",
    getAllianceName(row.allianceId),
    getAccountUserTypeLabel(row.accountUserType),
    row.accountName || "",
    row.email || "",
    getAccountStatusLabel(row.accountStatus),
    row.creationTime || "",
    row.lastModificationTime || "",
  ]);
  const csvContent = [headers, ...rows]
    .map((cols) =>
      cols
        .map((col) => {
          const value = col ?? "";
          if (typeof value === "string" && value.includes(",")) {
            return `"${value}"`;
          }
          return value;
        })
        .join(",")
    )
    .join("\r\n");
  const blob = new Blob([`\uFEFF${csvContent}`], {
    type: "text/csv;charset=utf-8;",
  });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `va-account-${Date.now()}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
};

// 初始化
onMounted(() => {
  initOptions();
  fetchData();
});
</script>

<style lang="scss" scoped>
.acquiring-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}

.table-toolbar-wrapper {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  padding: 16px 18px;
  margin-bottom: 18px;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .table-title {
    font-size: 16px;
    font-weight: 600;
    color: #303133;
  }
}
</style>

