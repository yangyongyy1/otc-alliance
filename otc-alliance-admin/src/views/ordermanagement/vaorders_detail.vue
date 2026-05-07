<template>
  <div class="order-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>{{ t("order.orderDetail") }}</h1>
      <div class="header-button">
        <el-button :icon="Refresh" @click="fetchDetail" :loading="loading">{{ t("common.refresh") }}</el-button>
        <el-button type="primary" @click="router.back()">{{ t("common.return") }}</el-button>
      </div>
    </div>

    <div class="detail-content" v-if="detailData">
      <!-- 订单基本信息 -->
      <el-descriptions :title="t('order.orderInfo')" :column="2" border>
        <el-descriptions-item :label="t('order.detailPlatformOrder')">{{ detailData.order?.platformOrderNo || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailChannelOrder')">{{ detailData.order?.channelOrderNo || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailAllianceName')">{{ detailData.order?.allianceName || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailMerchantName')">{{ detailData.order?.merchantName || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailUserName')">{{ detailData.order?.userName || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailUserEmail')">{{ detailData.order?.userEmail || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.accountNumber')">{{ detailData.order?.accountId || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailCurrency')">{{ detailData.order?.currency || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailAmount')">{{ detailData.order?.amount }} {{ detailData.order?.currency || "" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.orderType')">
          <el-tag :type="detailData.order?.orderType === 1 ? '' : 'success'">
            {{ getOrderTypeLabel(detailData.order?.orderType) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('order.detailPaymentMethod')">{{ detailData.order?.paymentMethod || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.detailOrderStatus')">
          <el-tag :type="getStatusTagType(detailData.order?.orderStatus)">
            {{ getStatusLabel(detailData.order?.orderStatus) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('order.payerName')">{{ detailData.order?.payerName || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.recipientName')">{{ detailData.order?.recipientName || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.reference')">{{ detailData.order?.reference || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('order.remark')">{{ detailData.order?.remark || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('common.creationTime')">{{ detailData.order?.creationTime || "--" }}</el-descriptions-item>
        <el-descriptions-item :label="t('common.lastModificationTime')">{{ detailData.order?.lastModificationTime || "--" }}</el-descriptions-item>
      </el-descriptions>

      <!-- 关联信息 -->
      <div class="detail-section">
        <div class="section-title">{{ t("order.relatedInfo") }}</div>
        <el-table :data="relatedInfoList" border stripe>
          <el-table-column type="index" :label="t('common.index')" width="60" />
          <el-table-column :label="t('common.type')" min-width="100">
            <template #default="{ row }">{{ t("order." + row.typeKey) }}</template>
          </el-table-column>
          <el-table-column :label="t('order.orderNo')" prop="orderNo" min-width="180" />
          <el-table-column :label="t('common.operation')" width="200" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" @click="handleSyncStatus(row)">
                <el-icon><RefreshRight /></el-icon>
                {{ t("order.syncStatus") }}
              </el-button>
              <el-button v-if="detailData.detail" link type="primary" @click="reqLogVisible = true">
                {{ t("order.requestLog") }}
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <!-- 交易明细 -->
      <template v-if="detailData.detail">
        <!-- 付款方信息 -->
        <el-descriptions :title="t('order.payerInfo')" :column="2" border class="detail-section">
          <el-descriptions-item :label="t('order.payerName')">{{ detailData.detail.payerName || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.payerCurrency')">{{ detailData.detail.payerCurrency || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.payerIban')">{{ detailData.detail.payerIban || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.payerSwiftBic')">{{ detailData.detail.payerSwiftBic || "--" }}</el-descriptions-item>
        </el-descriptions>

        <!-- 收款方信息 -->
        <el-descriptions :title="t('order.recipientInfo')" :column="2" border class="detail-section">
          <el-descriptions-item :label="t('order.recipientName')">{{ detailData.detail.recipientName || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientCurrency')">{{ detailData.detail.recipientCurrency || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientAccountHolderName')">{{ detailData.detail.recipientAccountHolderName || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientAccountNumber')">{{ detailData.detail.recipientAccountNumber || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientIban')">{{ detailData.detail.recipientIban || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientSwiftBic')">{{ detailData.detail.recipientSwiftBic || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientBankName')">{{ detailData.detail.recipientBankName || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.sortCode')">{{ detailData.detail.sortCode || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientBankAddress')" :span="2">{{ detailData.detail.recipientBankAddress || "--" }}</el-descriptions-item>
          <el-descriptions-item :label="t('order.recipientBankCountry')">{{ detailData.detail.recipientBankCountry || "--" }}</el-descriptions-item>
        </el-descriptions>

      </template>
    </div>

    <!-- 请求记录弹窗 -->
    <el-dialog v-model="reqLogVisible" :title="t('order.requestLog')" width="860px" :close-on-click-modal="false">
      <template v-if="detailData?.detail">
        <div class="json-section">
          <h4 class="dialog-section-title">{{ t("order.requestInfo") }}</h4>
          <div class="json-viewer-wrap">
            <vue-json-pretty :data="parseJson(detailData.detail.requestInfo)" :deep="3" :showLine="false" showIcon />
          </div>
        </div>
        <div class="json-section" style="margin-top: 20px">
          <h4 class="dialog-section-title">{{ t("order.responseInfo") }}</h4>
          <div class="json-viewer-wrap">
            <vue-json-pretty :data="parseJson(detailData.detail.responseInfo)" :deep="3" :showLine="false" showIcon />
          </div>
        </div>
        <div class="json-section" style="margin-top: 20px">
          <h4 class="dialog-section-title">{{ t("order.callback") }}</h4>
          <div class="json-viewer-wrap">
            <vue-json-pretty v-if="parseJson(detailData.detail.callback) !== detailData.detail.callback" :data="parseJson(detailData.detail.callback)" :deep="3" :showLine="false" showIcon />
            <pre v-else>{{ detailData.detail.callback || "--" }}</pre>
          </div>
        </div>
      </template>
      <template #footer>
        <el-button @click="reqLogVisible = false">{{ t("common.close") }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter, useRoute } from "vue-router";
import { Refresh, RefreshRight } from "@element-plus/icons-vue";
import request from "@/utils/request";
import { getOrderDetail } from "@/api/orderManagement";
import { getEnum } from "@/api/enum";
import VueJsonPretty from "vue-json-pretty";
import "vue-json-pretty/lib/styles.css";

const router = useRouter();
const route = useRoute();
const { t } = useI18n();

const loading = ref(false);
const detailData = ref(null);
const orderStatusOptions = ref([]);
const orderTypeOptions = ref([]);
const reqLogVisible = ref(false);

const initOptions = async () => {
  try {
    const [statusRes, typeRes] = await Promise.all([
      getEnum("CollectionOrderStatus"),
      getEnum("CollectionOrderType"),
    ]);
    orderStatusOptions.value = (statusRes.result || []).map((item) => ({
      label: item.displayName,
      value: item.value,
    }));
    orderTypeOptions.value = (typeRes.result || []).map((item) => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (e) {
    // silent
  }
};

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

const relatedInfoList = computed(() => {
  if (!detailData.value?.order?.channelOrderNo) return [];
  return [
    {
      typeKey: "detailChannelOrder",
      orderNo: detailData.value.order.channelOrderNo || "--",
    },
  ];
});

const handleSyncStatus = () => {
  // TODO: 调用同步状态接口
};

const fetchDetail = async () => {
  loading.value = true;
  try {
    getOrderDetail.params = { id: route.params.id };
    const { result } = await request(getOrderDetail);
    detailData.value = result;
  } catch (e) {
    // silent
  } finally {
    loading.value = false;
  }
};

const parseJson = (str) => {
  if (!str) return null;
  try {
    return JSON.parse(str);
  } catch {
    return str;
  }
};

onMounted(() => {
  initOptions();
  fetchDetail();
});
</script>

<style lang="scss" scoped>
.order-detail-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: 100vh;
  overflow: visible;
}

.header-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  h1 {
    margin: 0;
    font-size: 20px;
    font-weight: 600;
    color: #303133;
  }

  .header-button {
    display: flex;
    gap: 12px;
  }
}

.detail-content {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.detail-section {
  margin-top: 24px;

  .section-title {
    font-size: 16px;
    font-weight: 600;
    color: #409eff;
    margin-bottom: 12px;
  }
}

.json-section {
  .dialog-section-title {
    margin: 0 0 10px;
    font-size: 14px;
    font-weight: 600;
    color: #303133;
  }
}

.json-viewer-wrap {
  max-height: 360px;
  overflow-y: auto;
  background: #f8f9fa;
  border: 1px solid #ebeef5;
  border-radius: 6px;
  padding: 14px;
  font-size: 13px;
  line-height: 1.6;

  pre {
    margin: 0;
    white-space: pre-wrap;
    word-break: break-all;
  }
}

:deep(.el-descriptions) {
  .el-descriptions__title {
    font-size: 16px;
    font-weight: 600;
    color: #303133;
    margin-bottom: 16px;
  }

  .el-descriptions__label {
    font-weight: 500;
  }
}
</style>
