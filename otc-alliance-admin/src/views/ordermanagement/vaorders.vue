<template>
  <div class="order-container">
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
      :sortable-fields="['amount', 'creationTime', 'lastModificationTime']"
    >
      <!-- 工具栏：导出按钮 -->
      <template #toolbar>
        <div class="table-toolbar-wrapper">
          <span class="table-title">{{ t("order.dataList") }}</span>
          <el-button :icon="Download" @click="handleExport">{{ t("order.export") }}</el-button>
        </div>
      </template>

      <template #table-userInfo="{ row }">
        <div>{{ row.userName || "--" }}</div>
        <div style="color: #909399; font-size: 12px">{{ row.userEmail || "" }}</div>
      </template>

      <template #table-orderType="{ row }">
        <el-tag :type="row.orderType === 1 ? '' : 'success'">
          {{ getOrderTypeLabel(row.orderType) }}
        </el-tag>
      </template>

      <template #table-orderStatus="{ row }">
        <el-tag :type="getStatusTagType(row.orderStatus)">
          {{ getStatusLabel(row.orderStatus) }}
        </el-tag>
      </template>

      <template #table-amount="{ row }">
        {{ row.amount }} {{ row.currency || "" }}
      </template>

      <template #table-creationTime="{ row }">
        <div>{{ t("common.creationTime") }}：{{ row.creationTime || "--" }}</div>
        <div>{{ t("common.lastModificationTime") }}：{{ row.lastModificationTime || "--" }}</div>
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
import { getVAOrderList } from "@/api/orderManagement";
import { getCurrencies, getPaymentMethods } from "@/api/basic";
import { getEnum } from "@/api/enum";

const router = useRouter();

const { t } = useI18n();

const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 50,
  skipCount: 0,
  Sorting: "",
  platformOrderNo: "",
  channelOrderNo: "",
  userInfo: "",
  accountId: "",
  payerName: "",
  recipientName: "",
  reference: "",
  remark: "",
  currency: "",
  orderType: null,
  paymentMethod: "",
  orderStatus: null,
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
});

const currencyOptions = ref([]);
const orderTypeOptions = ref([]);
const paymentMethodOptions = ref([]);
const orderStatusOptions = ref([]);

const initOptions = async () => {
  try {
    const [currencyRes, payMethodRes, typeRes, statusRes] = await Promise.all([
      request(getCurrencies).catch(() => ({ result: [] })),
      request(getPaymentMethods).catch(() => ({ result: [] })),
      getEnum("CollectionOrderType"),
      getEnum("CollectionOrderStatus"),
    ]);

    currencyOptions.value = (currencyRes.result || []).map((v) => ({
      label: v,
      value: v,
    }));

    const rawPm = payMethodRes.result || [];
    paymentMethodOptions.value = rawPm.map((item) =>
      typeof item === "string"
        ? { label: item, value: item }
        : { label: item.displayName ?? item.platformName ?? "", value: item.platformName ?? item.displayName ?? "" }
    );

    orderTypeOptions.value = (typeRes.result || []).map((item) => ({
      label: item.displayName,
      value: item.value,
    }));

    orderStatusOptions.value = (statusRes.result || []).map((item) => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (e) {
    // silent
  }
};

const filterList = ref([
  { label: t("order.platformOrderNo"), name: "platformOrderNo", type: "text" },
  { label: t("order.channelOrderNo"), name: "channelOrderNo", type: "text" },
  { label: t("order.userName"), name: "userInfo", type: "text" },
  { label: t("order.accountNumber"), name: "accountId", type: "text" },
  { label: t("order.payerName"), name: "payerName", type: "text" },
  { label: t("order.recipientName"), name: "recipientName", type: "text" },
  { label: t("order.reference"), name: "reference", type: "text" },
  { label: t("order.remark"), name: "remark", type: "text" },
  { label: t("order.currency"), name: "currency", type: "select", options: currencyOptions },
  { label: t("order.orderType"), name: "orderType", type: "select", options: orderTypeOptions },
  { label: t("order.paymentMethod"), name: "paymentMethod", type: "select", options: paymentMethodOptions },
  { label: t("order.orderStatus"), name: "orderStatus", type: "select", options: orderStatusOptions },
  { label: t("common.creationTime"), name1: "creationTimeStart", name2: "creationTimeEnd", type: "multDatetime" },
  { label: t("common.lastModificationTime"), name1: "modificationTimeStart", name2: "modificationTimeEnd", type: "multDatetime" },
]);

const tableColumns = ref([
  { prop: "platformOrderNo", label: t("order.platformOrderNo") },
  { prop: "channelOrderNo", label: t("order.channelOrderNo") },
  { prop: "userInfo", label: t("order.userName"), type: "slot" },
  { prop: "accountId", label: t("order.accountNumber") },
  { prop: "currency", label: t("order.currency") },
  { prop: "orderType", label: t("order.orderType"), type: "slot" },
  { prop: "paymentMethod", label: t("order.paymentMethod") },
  { prop: "payerName", label: t("order.payerName") },
  { prop: "recipientName", label: t("order.recipientName") },
  { prop: "amount", label: t("order.amount"), type: "slot" },
  { prop: "reference", label: t("order.reference") },
  { prop: "orderStatus", label: t("order.orderStatus"), type: "slot" },
  { prop: "creationTime", label: t("common.time"), type: "slot" },
]);

const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);

const operationButtons = ref([
  {
    label: t("common.detail"),
    type: "text",
    permission: "Pages.OrderManagement.VAOrders.BtnViewDetails",
    onClick: (row) => {
      router.push({ path: `/ordermanagement/vaorders_detail/${row.id}` });
    },
  },
]);

const getStatusTagType = (status) => {
  const map = { 0: "warning", 1: "success", 2: "danger", 3: "info" };
  return map[status] ?? "info";
};

const getStatusLabel = (status) => {
  const opt = orderStatusOptions.value.find((o) => o.value === status);
  return opt ? opt.label : "--";
};

const getOrderTypeLabel = (type) => {
  const opt = orderTypeOptions.value.find((o) => o.value === type);
  return opt ? opt.label : "--";
};

const handleSearch = () => {
  fetchData();
};

const fetchData = async () => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getVAOrderList.params = { ...searchForm, orderType: 2 };
  try {
    const { result } = await request(getVAOrderList);
    tableData.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (e) {
    ElMessage.error(t("common.operationFailed"));
  } finally {
    loading.value = false;
  }
};

const handleExport = () => {
  if (!tableData.value.length) {
    ElMessage.warning(t("common.noData"));
    return;
  }
  const headers = [
    "#",
    t("order.platformOrderNo"),
    t("order.channelOrderNo"),
    t("order.userName"),
    t("order.userEmail"),
    t("order.accountNumber"),
    t("order.currency"),
    t("order.orderType"),
    t("order.paymentMethod"),
    t("order.payerName"),
    t("order.recipientName"),
    t("order.amount"),
    t("order.reference"),
    t("order.orderStatus"),
    t("order.remark"),
    t("common.creationTime"),
    t("common.lastModificationTime"),
  ];
  const idx = (i) => (searchForm.pageNo - 1) * searchForm.maxResultCount + i + 1;
  const rows = tableData.value.map((row, i) => [
    idx(i),
    row.platformOrderNo || "",
    row.channelOrderNo || "",
    row.userName || "",
    row.userEmail || "",
    row.accountId || "",
    row.currency || "",
    getOrderTypeLabel(row.orderType),
    row.paymentMethod || "",
    row.payerName || "",
    row.recipientName || "",
    row.amount,
    row.reference || "",
    getStatusLabel(row.orderStatus),
    row.remark || "",
    row.creationTime || "",
    row.lastModificationTime || "",
  ]);
  const csvContent = [headers, ...rows]
    .map((cols) =>
      cols
        .map((col) => {
          const v = col ?? "";
          return typeof v === "string" && (v.includes(",") || v.includes('"'))
            ? `"${v.replace(/"/g, '""')}"`
            : v;
        })
        .join(",")
    )
    .join("\r\n");
  const blob = new Blob([`\uFEFF${csvContent}`], { type: "text/csv;charset=utf-8;" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `va-orders-${Date.now()}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
};

onMounted(() => {
  initOptions();
  fetchData();
});
</script>

<style lang="scss" scoped>
.order-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}

.table-toolbar-wrapper {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;

  .table-title {
    font-size: 15px;
    font-weight: 600;
    color: #303133;
  }
}

</style>
