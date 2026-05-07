<template>
  <div class="account-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>{{ t('account.accountDetail') }}</h1>
      <div class="header-button">
        <el-button :icon="Refresh" @click="handleRefresh" :loading="loading">{{ t('common.refresh') }}</el-button>
        <el-button type="primary" @click="router.back()">{{ t('common.return') }}</el-button>
      </div>
    </div>

    <div class="detail-content">
      <!-- 账户基本信息 -->
      <el-descriptions :title="t('account.basicInfo')" :column="2" border v-if="detailData">
        <el-descriptions-item :label="t('account.accountId')" :span="2">
          {{ detailData.accountId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.paymentType')">
          {{ detailData.paymentType || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountHolderName')">
          {{ detailData.accountHolderName || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountNumber')" :span="2">
          {{ detailData.accountNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountRoutingNumber')">
          {{ detailData.accountRoutingNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.remark')">
          {{ detailData.memo || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.swiftBic')">
          {{ detailData.swiftBic || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.intermediarySwiftBic')">
          {{ detailData.intermediarySwiftBic || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.creationTime')">
          {{ detailData.creationTime || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.lastModificationTime')">
          {{ detailData.lastModificationTime || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- Account对象信息 -->
      <el-descriptions :title="t('account.accountInfo')" :column="2" border v-if="detailData?.account" class="detail-section">
        <el-descriptions-item :label="t('account.channelProvider')">
          {{ detailData.account.channelProvider || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.channelAccountId')">
          {{ detailData.account.channelAccountId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.referenceId')" :span="2">
          {{ detailData.account.referenceId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.currency')">
          {{ detailData.account.currency || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountName')">
          {{ detailData.account.accountName || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountNumber')" :span="2">
          {{ detailData.account.accountNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.bankName')" :span="2">
          {{ detailData.account.bankName || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.status')">
          <el-tag :type="getAccountStatusTagType(detailData.account.status)">
            {{ getAccountStatusLabel(detailData.account.status) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.creationTime')">
          {{ detailData.account.creationTime || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.rejectionReason')" :span="2" v-if="detailData.account.rejectionReason">
          <span style="color: #f56c6c;">{{ detailData.account.rejectionReason }}</span>
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.lastModificationTime')">
          {{ detailData.account.lastModificationTime || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- 机构信息 -->
      <el-descriptions :title="t('account.institutionInfo')" :column="2" border v-if="detailData" class="detail-section">
        <el-descriptions-item :label="t('account.institutionName')" :span="2">
          {{ detailData.institutionName || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.institutionAddressLine1')" :span="2">
          {{ detailData.institutionAddressLine1 || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.institutionCity')">
          {{ detailData.institutionCity || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.institutionState')">
          {{ detailData.institutionState || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.institutionPostalCode')">
          {{ detailData.institutionPostalCode || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.institutionCountryCode')">
          {{ detailData.institutionCountryCode || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- 持有者地址信息 -->
      <el-descriptions :title="t('account.holderAddressInfo')" :column="2" border v-if="detailData" class="detail-section">
        <el-descriptions-item :label="t('account.holderAddressLine1')" :span="2">
          {{ detailData.holderAddressLine1 || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.holderCity')">
          {{ detailData.holderCity || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.holderState')">
          {{ detailData.holderState || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.holderPostalCode')">
          {{ detailData.holderPostalCode || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.holderCountryCode')">
          {{ detailData.holderCountryCode || "--" }}
        </el-descriptions-item>
      </el-descriptions>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter, useRoute } from "vue-router";
import { Refresh } from "@element-plus/icons-vue";
import { getEnum } from "@/api/enum";
import request from "@/utils/request";
import { getVAAccountById } from "@/api/VAAccount";

const router = useRouter();
const route = useRoute();
const { t } = useI18n();

const loading = ref(false);
const detailData = ref(null);
const accountStatusOptions = ref([]);

// 初始化选项数据
const initOptions = async () => {
  try {
    const accountStatusResult = await getEnum("VAStatus");
    accountStatusOptions.value = (accountStatusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (error) {
  }
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

// 获取详情数据
const fetchDetail = async () => {
  loading.value = true;
  try {
    getVAAccountById.params = {
      id: route.params.id,
    };
    const { result } = await request(getVAAccountById);
    detailData.value = result;
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 刷新数据
const handleRefresh = () => {
  fetchDetail();
};

onMounted(() => {
  initOptions();
  fetchDetail();
});
</script>

<style lang="scss" scoped>
.account-detail-container {
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
  margin-top: 20px;
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

