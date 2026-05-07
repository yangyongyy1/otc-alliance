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
      :searchForm="searchForm"
      :show-add-button="false"
      :showOperation="false"
      :custom-filter-fields="customFilterFields"
      :sortable-fields="['userAuthStatus', 'creationTime','name']"
    >
      <!-- 自定义名称列渲染 -->
      <template #table-name="{ row }">
        {{ formatUserName(row) }}
      </template>

      <template #table-userAuthStatus="{ row }">
        <el-tag :type="getStatusTagType(row.userAuthStatus)">
          {{ getStatusLabel(row.userAuthStatus) }}
        </el-tag>
      </template>

      <template #table-userType="{ row }">
        <el-tag :type="row.userType===1?'success':'info'">
          {{ userTypeOptions.find(item => item.value === row.userType)?.label }}
        </el-tag>
      </template>

      <template #table-userStatus="{ row }">
        <el-tag v-if="row.userStatus===0" type="success">
          {{ userStatusOptions.find(item => item.value === row.userStatus)?.label }}
        </el-tag>

        <el-tag v-if="row.userStatus===1" type="danger">
          {{ userStatusOptions.find(item => item.value === row.userStatus)?.label }}
        </el-tag>

        <el-tag v-if="row.userStatus===2" type="warning">
          {{ userStatusOptions.find(item => item.value === row.userStatus)?.label }}
        </el-tag>
      </template>
     
      <!-- 自定义用户类型列渲染 -->
      

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
import { Download } from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getClientUserList, getKycLevelName } from "@/api/userManagement";
import { getMerchantItmes } from "@/api/allianceManagement";
import { getEnum } from "@/api/enum";

const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 50,
  skipCount: 0,
  email: "",
  relationCode: "",
  userAuthStatus: "",
  userStatus: "",
  userType: "",
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
  kycLevelName: "",
});

const merchantOptions = ref([]);
const userAuthStatusOptions = ref([]);
const userStatusOptions = ref([]);
const userTypeOptions = ref([]);
const kycLevelNameOptions = ref([]);
const kycLevelNameForAuthLevel0 = ref("");

// 初始化选项数据
const initOptions = async () => {
  try {
    // 获取商户选项
    const { result: merchantResult } = await request(getMerchantItmes);
    merchantOptions.value = merchantResult.map(item => ({
      label: item.name,
      value: item.id,
    }));

    // 获取枚举选项
    const [userAuthStatusResult, userStatusResult, userTypeResult, kycLevelResult] = await Promise.all([
      getEnum("KycBizStatus"),
      getEnum("UserStatus"),
      getEnum("AccountUserType"),
      getEnum("VAUserKYCLevel"),
    ]);

    // 转换枚举格式为 select 组件需要的格式 {label, value}
    userAuthStatusOptions.value = (userAuthStatusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
    userStatusOptions.value = (userStatusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
    userTypeOptions.value = (userTypeResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    // 获取认证等级名称：用接口返回值替代写死
    getKycLevelName.params = { authLevel: 0 };
    const { result: kycLevelNameResult } = await request(getKycLevelName);
    kycLevelNameForAuthLevel0.value = kycLevelNameResult || "";
    kycLevelNameOptions.value = [
      {
        label: kycLevelNameForAuthLevel0.value || "--",
        value: "0",
      },
    ];
  } catch (error) {
  }
};

// 搜索表单配置
const filterList = ref([
  {
    label: t("user.email"),
    name: "email",
    type: "text",
  },
  {
    label: t("user.relationTarget"),
    name: "relationCode",
    type: "select",
    options: merchantOptions,
  },
  {
    label: t("user.authStatus"),
    name: "userAuthStatus",
    type: "select",
    options: userAuthStatusOptions,
  },
  {
    label: t("user.userStatus"),
    name: "userStatus",
    type: "select",
    options: userStatusOptions,
  },
  {
    label: t("user.userType"),
    name: "userType",
    type: "select",
    options: userTypeOptions,
  },
  {
    label: t("user.certificationType"),
    name: "kycLevelName",
    type: "select",
    options: kycLevelNameOptions,
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
  { prop: "email", label: t("user.email") },
  { prop: "name", label: t("user.name"), type: "slot"},
  { prop: "userAuthStatus", label: t("user.authStatus"), type: "slot" },
  { prop: "userType", label: t("user.userType"), type: "slot"},
  { prop: "kycLevelName", label: t("user.certificationType"), formatter: (row) => {
    if(row.kycLevelCompleted=='0'){
      return kycLevelNameForAuthLevel0.value || "--";
    }
    return ''
  }},
  { prop: "allianceName", label: t("user.allianceName"), formatter: (row) => {
    return row.merchant?.alliance?.name || "--";
  }},

  { prop: "relationTarget", label: t("user.relationTarget"), formatter: (row) => {
    return row.merchant?.name || "--";
  } },
  { prop: "userStatus", label: t("user.userStatus"), type: "slot"},
  { prop: "creationTime", label: t("common.time"), type: "slot" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);

const customFilterFields = ref([
  {
    value: "email",
    label: t("user.email"),
    displayName: t("user.email"),
    dataType: "text",
  },
]);




const formatUserName = (row) => {
  const parts = [row.firstName, row.middleName, row.lastName].filter(Boolean);
  return parts.length ? parts.join(" ") : " ";
};

// 获取状态标签类型
const getStatusTagType = (status) => {
  // 处理 null/undefined 值
  if (status === null || status === undefined) {
    return "info";
  }
  // 根据认证状态枚举值设置标签类型
  const statusMap = {
    [-1]: "info",      // 未开始 (NOTSTARTED)
    0: "info",         // 待提交 (PENDINGSUBMISSION)
    1: "warning",      // 审核中 (UNDERREVIEW)
    2: "success",      // 已通过 (APPROVED)
    3: "danger",       // 已拒绝 (REJECTED)
    4: "warning",      // 需要重新提交 (RESUBMISSIONREQUIRED)
  };
  return statusMap[status] || "info";
};

// 获取状态标签文本
const getStatusLabel = (status) => {
  const option = userAuthStatusOptions.value.find(item => item.value === status);
  return option ? option.label : "--";
};

const getRelationTarget = (row) => {
  return (
    row.merchant?.relationCode ||
    row.merchant?.name ||
    row.alliance?.name ||
    "--"
  );
};

// 搜索处理
const handleSearch = (form) => {
  fetchData();
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getClientUserList.data = searchForm;
  const { result } = await request(getClientUserList);
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
</style>
