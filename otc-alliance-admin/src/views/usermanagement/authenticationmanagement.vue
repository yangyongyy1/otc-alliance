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
      :sortable-fields="['kycBizStatus', 'creationTime','name']"
    >
      <!-- 自定义所属联盟列渲染 -->
      <template #table-allianceName="{ row }">
        {{ row.user?.alliance?.name || "--" }}
      </template>

      <!-- 自定义邮箱列渲染 -->
      <template #table-email="{ row }">
        {{ row.user.email || "--" }}
      </template>

      <!-- 自定义名称列渲染 -->
      <template #table-name="{ row }">
        {{ formatUserName(row) }}
      </template>

      <!-- 自定义认证方式列渲染 -->
      <template #table-authType="{ row }">
        {{ getAuthTypeLabel(row.user?.merchant?.authType) }}
      </template>

      <!-- 自定义认证状态列渲染 -->
      <template #table-kycBizStatus="{ row }">
        <el-tag :type="getStatusTagType(row.kycBizStatus)">
          {{ getStatusLabel(row.kycBizStatus) }}
        </el-tag>
      </template>

      <!-- 自定义时间列渲染 -->
      <template #table-creationTime="{ row }">
        <p>{{ t('common.creationTime') }}：{{ row.creationTime || "--" }}</p>
        <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime || "--" }}</p>
      </template>
      <template #operation="{ row }"></template>
    </common-table>

    <!-- 详情对话框 -->
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getKycLevelName, getUserIdentityList } from "@/api/userManagement";
import { getEnum } from "@/api/enum";
import { getAllianceOptions } from "@/api/allianceManagement";
import { userResetKyc } from "@/api/allianceManagement";
import { ElMessageBox } from "element-plus";

const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  name: "",
  email: "",
  allianceId: "",
  authType: "",
  status: "",
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
});

const statusOptions = ref([]);
const authTypeOptions = ref([]);
const allianceOptions = ref([]);
const authStandardLevelOptions = ref([]);
const authStandardLevelName = ref("");
// 初始化选项数据
const initOptions = async () => {
  try {
    // 获取联盟选项
    const { result: allianceResult } = await request(getAllianceOptions);
    allianceOptions.value = allianceResult.map(item => ({
      label: item.name,
      value: item.id,
    }));

    // 获取枚举选项
    const [statusResult, authTypeResult] = await Promise.all([
      getEnum("KycBizStatus"),
      getEnum("AuthType"),
    ]);

    statusOptions.value = (statusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    authTypeOptions.value = (authTypeResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    // 获取认证等级名称：用接口返回值替代写死
    getKycLevelName.params = { authLevel: 0 };
    const { result: kycLevelNameResult } = await request(getKycLevelName);
    authStandardLevelName.value = kycLevelNameResult || "";
    authStandardLevelOptions.value = [
      {
        label: authStandardLevelName.value,
        value: 0,
      },
    ];
  } catch (error) {
  }
};

// 搜索表单配置
const filterList = ref([
  {
    label: t("user.name"),
    name: "name",
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
    label: t("user.authType"),
    name: "authType",
    type: "select",
    options: authTypeOptions,
  },
  

  {
    label: t("user.authStatus"),
    name: "kycBizStatus",
    type: "select",
    options: statusOptions,
  },
  //认证类型
  {
    label: t("user.certificationType"),
    name: "authStandardLevel",
    type: "select",
    options: authStandardLevelOptions,
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
  { prop: "allianceName", label: t("user.allianceName"), type: "slot" },
  { prop: "email", label: t("user.email"), type: "slot" },
  { prop: "name", label: t("user.name"), type: "slot" },
  { prop: "authType", label: t("user.authType"), type: "slot" },
  { prop: "authStandardLevel", label: t("user.certificationType"), formatter: (row) => {
    if(row.authStandardLevel===0){
      return authStandardLevelName.value || "--";
    }
    return ''
  }},
  { prop: "kycBizStatus", label: t("user.authStatus"), type: "slot" },
  { prop: "creationTime", label: t("common.time"), type: "slot" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);
const router = useRouter();


const operationButtons = ref([
  {
    label: t("user.detail"),
    type: "text",
    permission: "Pages.UserManagement.AuthenticationManagement.BtnViewDetails",
    onClick: (row) => {
      router.push({
        path: `/usermanagement/authenticationmanagement_detail/${row.id}`,
      });
    },
  },
  // 重置kyc：kycBizStatus 不等于 0 和 6 时显示重置按钮
  {
    label: t("user.reset"),
    type: "text",
    permission: "Pages.UserManagement.AuthenticationManagement.BtnReset",
    show: (row) => row.kycBizStatus !== 2 && !row.isClose,
    onClick: (row) => {
      resetKyc(row.id);
    },
  },
]);

// 重置kyc
const resetKyc = (id) => {
  //询问是否重置
  ElMessageBox.confirm(t("user.resetKyc"), t("common.confirm"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  }).then( async() => {
    userResetKyc.params.id=id
    const { success } = await request(userResetKyc);
    if(success){
      ElMessage.success(t("user.resetKycSuccess"));
      fetchData();
    }
  });
};

// 格式化用户名
const formatUserName = (row) => {
  const parts = [row.firstName, row.middleName, row.lastName].filter(Boolean);
  return parts.length ? parts.join(" ") : "--";
};

// 获取联盟名称
const getAllianceName = (allianceId) => {
  const alliance = allianceOptions.value.find(item => item.value === allianceId);
  return alliance ? alliance.label : "--";
};

// 获取认证方式标签
const getAuthTypeLabel = (authType) => {
  const option = authTypeOptions.value.find(item => item.value === authType);
  return option ? option.label : "--";
};

// 获取状态标签类型
const getStatusTagType = (status) => {
  // 处理 null/undefined 值
  if (status === null || status === undefined) {
    return "info";
  }
  // 根据原型图：审核中=warning，驳回=danger，通过=success
  const statusMap = {
    [-1]: "info",      // 未开始
    0: "info",         // 待提交
    1: "warning",      // 审核中
    2: "success",      // 已通过
    3: "danger",       // 已拒绝
    4: "warning",      // 需要重新提交
  };
  return statusMap[status] || "info";
};

// 获取状态标签文本
const getStatusLabel = (status) => {
  const option = statusOptions.value.find(item => item.value === status);
  return option ? option.label : "--";
};

// 导出功能
const handleExport = () => {
  if (!tableData.value.length) {
    ElMessage.warning(t("common.noData"));
    return;
  }
  const headers = [
    "#",
    t("user.allianceName"),
    t("user.email"),
    t("user.name"),
    t("user.authType"),
    t("user.authStatus"),
    t("common.creationTime"),
    t("common.lastModificationTime"),
  ];
  const indexMethod = (index) => {
    return (searchForm.pageNo - 1) * searchForm.maxResultCount + index + 1;
  };
  const rows = tableData.value.map((row, idx) => [
    indexMethod(idx),
    getAllianceName(row.allianceId),
    row.email || "",
    formatUserName(row),
    getAuthTypeLabel(row.authType),
    getStatusLabel(row.status),
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
  link.download = `user-identity-${Date.now()}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
};

// 搜索处理
const handleSearch = (form) => {
  fetchData();
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getUserIdentityList.params = searchForm;
  const { result } = await request(getUserIdentityList);
  tableData.value = result.items || [];
  total.value = result.totalCount || 0;
  loading.value = false;
};

// 初始化
onMounted(async () => {
  await initOptions();
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

.table-toolbar {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  padding: 16px 18px;
  margin-bottom: 18px;
  display: flex;
  align-items: center;
}
</style>

