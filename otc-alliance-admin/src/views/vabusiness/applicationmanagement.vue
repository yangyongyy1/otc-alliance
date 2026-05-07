<template>
  <div class="application-container">
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
      :sortable-fields="['status', 'creationTime', 'requestType']"
    >
      <!-- 自定义请求类型列渲染 -->
      <template #table-requestType="{ row }">

        <el-tag v-if="row.failStep!==0">{{ getRequestTypeLabel(row.failStep) }}</el-tag>
      </template>
      <!-- 自定义用户信息列渲染 -->
      <template #table-userInfo="{ row }">
        <div v-if="row.user" class="custom-cell-content">
          <p>
            <strong class="label-text">{{ t("user.email") }}:</strong> {{ row.user.email }}
          </p>
          <p>
            <strong class="label-text">{{ t("user.name") }}:</strong> {{ row.user.firstName }} {{ row.user.middleName }} {{ row.user.lastName }}
          </p>
          <p>
            <strong class="label-text">{{ t("user.allianceName") }}:</strong> {{ row.user?.alliance?.name || "--" }}
          </p>
          <p>
            <strong class="label-text">{{ t("user.merchantName") }}:</strong> {{ row.user?.merchant?.name || "--" }}
          </p>
        </div>
      </template>

      <!-- 自定义状态列渲染 -->
      <template #table-status="{ row }">
        <el-tag :type="getStatusTagType(row.status)">
          {{ getStatusLabel(row.status) }}
        </el-tag>
      </template>

      <!-- 自定义账户信息列渲染 -->
      <template #table-accountInfo="{ row }">
        <div v-if="row.account || row.accountName || row.accountId" class="custom-cell-content">
          <p v-if="row.account?.accountName || row.accountName">
            <strong class="label-text">{{ t("account.accountName") }}:</strong> {{ row.account?.accountName || row.accountName || "--" }}
          </p>
          <p v-if="row.account?.currency || row.currency">
            <strong class="label-text">{{ t("account.currency") }}:</strong> {{ row.account?.currency || row.currency || "--" }}
          </p>
          <p v-if="row.accountId">
            <strong class="label-text">{{ t("account.accountId") }}:</strong> {{ row.accountId }}
          </p>
        </div>
        <span v-else>--</span>
      </template>

      <!-- 自定义渠道信息列渲染 -->
      <template #table-channelInfo="{ row }">
        <div v-if="row.channelProvider || row.channelAccountId" class="custom-cell-content">
          <p v-if="row.channelProvider">
            {{ row.channelProvider }}
          </p>
          <p v-if="row.channelAccountId">
            <strong class="label-text">{{ t("account.channelAccountId") }}:</strong> {{ row.channelAccountId }}
          </p>
        </div>
        <span v-else>--</span>
      </template>

      <!-- 自定义时间列渲染 -->
      <template #table-creationTime="{ row }">
        <div class="custom-cell-content">
          <p>{{ t('common.creationTime') }}：{{ row.creationTime || "--" }}</p>
          <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime || "--" }}</p>
        </div>
      </template>
    </common-table>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getPayChannelServiceRequestList } from "@/api/VAAccount";
import { getEnum } from "@/api/enum";
import { getAllianceOptions } from "@/api/allianceManagement";

const router = useRouter();
const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  requestType: "",
  status: "",
  accountId: "",
  channelProvider: "",
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
});

const requestTypeOptions = ref([]);
const statusOptions = ref([]);
const allianceOptions = ref([]);

// 初始化选项数据
const initOptions = async () => {
  try {
    // 获取联盟选项
    const { result: allianceResult } = await request(getAllianceOptions);
    allianceOptions.value = allianceResult.map(item => ({
      label: item.name,
      value: item.id,
    }));

    // 获取枚举选项 - 根据实际 API 调整枚举名称
    const [requestTypeResult, statusResult] = await Promise.all([
      getEnum("PayChannelServiceRequestFailStep").catch(() => ({ result: [] })),
      getEnum("PayChannelServiceRequestStatus").catch(() => ({ result: [] })),
    ]);

    requestTypeOptions.value = (requestTypeResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    statusOptions.value = (statusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (error) {
    console.error("初始化选项失败:", error);
  }
};

// 搜索表单配置
const filterList = computed(() => [
  {
    label: t("common.failStep"),
    name: "failStep",
    type: "enum",
    enumName: "PayChannelServiceRequestFailStep",
  },
  {
    label: t("common.status"),
    name: "status",
    type: "enum",
    enumName: "PayChannelServiceRequestStatus",
  },
  {
    label: t("account.channelProvider"),
    name: "channelProvider",
    type: "text",
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
const tableColumns = computed(() => [
  { prop: "requestType", label: t("common.failStep"), type: "slot",width: "100px" },
  //用户信息
  { prop: "userInfo", label: t("user.userInfo"), type: "slot",width: "300px" },  
  { prop: "status", label: t("common.status"), type: "slot",width: "150px" },
  { prop: "accountInfo", label: t("account.accountInfo"), type: "slot",width: "350px" },
  { prop: "channelInfo", label: t("account.channelProvider"), type: "slot",width: "150px" },
  { prop: "creationTime", label: t("common.time"), type: "slot",width: "300px" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);

// 操作按钮配置
const operationButtons = ref([
  {
    label: t("common.detail"),
    type: "text",
    permission: "Pages.VABusiness.ApplicationManagement.BtnViewDetails",
    onClick: (row) => {
      router.push({
        path: `/vabusiness/applicationmanagement_detail/${row.id}`,
      });
    },
  },
]);

// 获取请求类型标签
const getRequestTypeLabel = (requestType) => {
  const option = requestTypeOptions.value.find(item => item.value === requestType);
  return option ? option.label : requestType || "--";
};

// 获取状态标签类型
const getStatusTagType = (status) => {
  if (status === null || status === undefined) {
    return "info";
  }
  // 根据枚举值映射标签类型
  // PendingCustomer = 0, CustomerProcessing = 1, PendingAccount = 2, 
  // AccountProcessing = 3, Completed = 4, Failed = 5
  const statusMap = {
    0: "warning",  // PendingCustomer - 待处理客户
    1: "info",      // CustomerProcessing - 客户处理中
    2: "warning",   // PendingAccount - 待处理账户
    3: "info",      // AccountProcessing - 账户处理中
    4: "success",   // Completed - 已完成
    5: "danger",    // Failed - 失败
    // 兼容字符串值
    "PendingCustomer": "warning",
    "CustomerProcessing": "info",
    "PendingAccount": "warning",
    "AccountProcessing": "info",
    "Completed": "success",
    "Failed": "danger",
  };
  return statusMap[status] || "info";
};

// 获取状态标签文本
const getStatusLabel = (status) => {
  const option = statusOptions.value.find(item => item.value === status);
  return option ? option.label : status || "--";
};

// 搜索处理
const handleSearch = (form) => {
  fetchData();
};

// 获取表格数据
const fetchData = async () => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getPayChannelServiceRequestList.params = searchForm;
  try {
    const { result } = await request(getPayChannelServiceRequestList);
    tableData.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (error) {
    ElMessage.error(t("common.operationFailed"));
    console.error("获取数据失败:", error);
  } finally {
    loading.value = false;
  }
};

// 初始化
onMounted(() => {
  initOptions();
  fetchData();
});
</script>

<style lang="scss" scoped>
.application-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}

// 自定义单元格内容样式
:deep(.custom-cell-content) {
  text-align: left;
  
  p {
    margin: 4px 0;
    text-align: left;
    
    .label-text {
      display: inline-block;
      text-align: left;
      font-weight: 600;
      color: #606266;
      margin-right: 4px;
    }
  }
}
</style>
