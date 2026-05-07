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
      <template #table-userInfo="{ row }">
        <div>{{ row.userName || "--" }}</div>
        <div style="color: #909399; font-size: 12px">{{ row.userEmail || "" }}</div>
      </template>

      <template #table-orderStatus="{ row }">
        <el-tag :type="getStatusTagType(row.orderStatus)">
          {{ getStatusLabel(row.orderStatus) }}
        </el-tag>
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
import { ElMessage } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getDepositOrderList } from "@/api/orderManagement";
import { getAllianceOptions, getMerchantItmes } from "@/api/allianceManagement";
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
  remark: "",
  reference: "",
  allianceId: null,
  merchantId: null,
  currency: "",
  paymentMethod: "",
  orderStatus: null,
  creationTimeStart: "",
  creationTimeEnd: "",
  modificationTimeStart: "",
  modificationTimeEnd: "",
});

const allianceOptions = ref([]);
const merchantOptions = ref([]);
const currencyOptions = ref([]);
const paymentMethodOptions = ref([]);
const orderStatusOptions = ref([]);

const initOptions = async () => {
  try {
    const [allianceRes, merchantRes, currencyRes, payMethodRes, statusRes] = await Promise.all([
      request(getAllianceOptions).catch(() => ({ result: [] })),
      request(getMerchantItmes).catch(() => ({ result: [] })),
      request(getCurrencies).catch(() => ({ result: [] })),
      request(getPaymentMethods).catch(() => ({ result: [] })),
      getEnum("CollectionOrderStatus"),
    ]);

    allianceOptions.value = (allianceRes.result || []).map((item) => ({
      label: item.name,
      value: item.id,
    }));

    merchantOptions.value = (merchantRes.result || []).map((item) => ({
      label: item.name,
      value: item.id,
    }));

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
  { label: t("order.remark"), name: "remark", type: "text" },
  { label: t("order.reference"), name: "reference", type: "text" },
  { label: t("order.allianceName"), name: "allianceId", type: "select", options: allianceOptions },
  { label: t("order.merchantName"), name: "merchantId", type: "select", options: merchantOptions },
  { label: t("order.currency"), name: "currency", type: "select", options: currencyOptions },
  { label: t("order.paymentMethod"), name: "paymentMethod", type: "select", options: paymentMethodOptions },
  { label: t("order.orderStatus"), name: "orderStatus", type: "select", options: orderStatusOptions },
  { label: t("common.creationTime"), name1: "creationTimeStart", name2: "creationTimeEnd", type: "multDatetime" },
  { label: t("common.lastModificationTime"), name1: "modificationTimeStart", name2: "modificationTimeEnd", type: "multDatetime" },
]);

const tableColumns = ref([
  { prop: "platformOrderNo", label: t("order.platformOrderNo") },
  { prop: "channelOrderNo", label: t("order.channelOrderNo") },
  { prop: "allianceName", label: t("order.allianceName") },
  { prop: "merchantName", label: t("order.merchantName") },
  { prop: "userInfo", label: t("order.userName"), type: "slot" },
  { prop: "currency", label: t("order.currency") },
  { prop: "paymentMethod", label: t("order.paymentMethod") },
  { prop: "amount", label: t("order.amount") },
  { prop: "orderStatus", label: t("order.orderStatus"), type: "slot" },
  { prop: "creationTime", label: t("order.transactionTime"), type: "slot" },
  { prop: "reference", label: t("order.reference") },
  { prop: "remark", label: t("order.remark") },
]);

const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);

const operationButtons = ref([
  {
    label: t("common.detail"),
    type: "text",
    permission: "Pages.OrderManagement.DepositOrders.BtnViewDetails",
    onClick: (row) => {
      router.push({ path: `/ordermanagement/depositorders_detail/${row.id}` });
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

const handleSearch = () => {
  fetchData();
};

const fetchData = async () => {
  loading.value = true;
  searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
  getDepositOrderList.params = { ...searchForm, orderType: 1 };
  try {
    const { result } = await request(getDepositOrderList);
    tableData.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (e) {
    ElMessage.error(t("common.operationFailed"));
  } finally {
    loading.value = false;
  }
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

</style>
