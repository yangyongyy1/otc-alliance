<template>
  <div class="application-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>{{ t('account.accountDetail') }}</h1>
      <div class="header-button">
        <el-button :icon="Refresh" @click="handleRefresh" :loading="loading">{{ t('common.refresh') }}</el-button>
        <el-button type="primary" @click="router.back()">{{ t('common.return') }}</el-button>
      </div>
    </div>

    <div class="detail-content">
      <!-- 基本信息 -->
      <el-descriptions :title="t('account.basicInfo')" :column="2" border v-if="detailData">
        <el-descriptions-item :label="t('common.failStep')">
          <el-tag>{{ getFailStepLabel(detailData.failStep) }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.status')">
          <el-tag :type="getStatusTagType(detailData.status)">
            {{ getStatusLabel(detailData.status) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.channelProvider')" :span="2">
          {{ detailData.channelProvider || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.channelAccountId')" :span="2">
          {{ detailData.channelAccountId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountId')" :span="2">
          {{ detailData.accountId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.creationTime')">
          {{ detailData.creationTime || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.lastModificationTime')">
          {{ detailData.lastModificationTime || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- 账户信息 -->
      <el-descriptions :title="t('account.accountInfo')" :column="2" border v-if="detailData?.account" class="detail-section">
        <el-descriptions-item :label="t('account.accountName')">
          {{ detailData.account.accountName || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.currency')">
          {{ detailData.account.currency || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('account.accountNumber')" :span="2">
          {{ detailData.account.accountNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.status')">
          <el-tag :type="getAccountStatusTagType(detailData.account.status)">
            {{ getAccountStatusLabel(detailData.account.status) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item :label="t('common.creationTime')">
          {{ detailData.account.creationTime || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- JSON 字段显示 -->
      <template v-for="(value, key) in jsonFields" :key="key">
        <el-descriptions 
          :title="formatFieldLabel(key)" 
          :column="2" 
          border 
          class="detail-section"
          v-if="value && Object.keys(value).length > 0"
        >
          <el-descriptions-item 
            v-for="(jsonValue, jsonKey) in value" 
            :key="jsonKey"
            :label="formatFieldLabel(jsonKey)"
            :span="isLongValue(jsonValue) ? 2 : 1"
          >
            <div v-if="isObject(jsonValue)" class="json-object">
              <el-descriptions :column="1" border size="small">
                <el-descriptions-item 
                  v-for="(subValue, subKey) in jsonValue" 
                  :key="subKey"
                  :label="formatFieldLabel(subKey)"
                >
                  <div v-if="isJsonString(subValue) || (typeof subValue === 'object' && subValue !== null)" class="json-display">
                    <pre>{{ formatJsonValue(subValue) }}</pre>
                  </div>
                  <span v-else>{{ formatJsonValue(subValue) }}</span>
                </el-descriptions-item>
              </el-descriptions>
            </div>
            <div v-else-if="Array.isArray(jsonValue)" class="json-display">
              <pre>{{ formatJsonValue(jsonValue) }}</pre>
            </div>
            <div v-else class="json-value">
              {{ formatJsonValue(jsonValue) }}
            </div>
          </el-descriptions-item>
        </el-descriptions>
      </template>
    </div>

    <!-- 底部按钮区域 -->
    <div class="footer-buttons">
      <el-button 
        v-if="hasCustomerResponse" 
        type="primary" 
        @click="showCustomerResponseDialog = true"
      >
        {{ t('account.viewResponse') }} - {{ t('account.customerResponse') }}
      </el-button>
      <el-button 
        v-if="hasAccountResponse" 
        type="primary" 
        @click="showAccountResponseDialog = true"
      >
        {{ t('account.viewResponse') }} - {{ t('account.accountResponse') }}
      </el-button>
    </div>

    <!-- 客户响应弹窗 -->
    <el-dialog
      v-model="showCustomerResponseDialog"
      :title="t('account.customerResponse')"
      width="800px"
      :close-on-click-modal="false"
    >
      <div class="json-viewer-container">
        <vue-json-pretty
          :data="parseJson(getResponseData('customerResponse'))"
          :deep="4"
          :showLine="false"
          showIcon
        />
      </div>
      <template #footer>
        <el-button type="primary" @click="showCustomerResponseDialog = false">{{ t('common.close') }}</el-button>
      </template>
    </el-dialog>

    <!-- 账户响应弹窗 -->
    <el-dialog
      v-model="showAccountResponseDialog"
      :title="t('account.accountResponse')"
      width="800px"
      :close-on-click-modal="false"
    >
      <div class="json-viewer-container">
        <vue-json-pretty
          :data="parseJson(getResponseData('accountResponse'))"
          :deep="4"
          :showLine="false"
          showIcon
        />
      </div>
      <template #footer>
        <el-button type="primary" @click="showAccountResponseDialog = false">{{ t('common.close') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter, useRoute } from "vue-router";
import { Refresh } from "@element-plus/icons-vue";
import { getEnum } from "@/api/enum";
import request from "@/utils/request";
import { getPayChannelServiceRequestById } from "@/api/VAAccount";
import VueJsonPretty from "vue-json-pretty";
import "vue-json-pretty/lib/styles.css";

const router = useRouter();
const route = useRoute();
const { t } = useI18n();

const loading = ref(false);
const detailData = ref(null);
const failStepOptions = ref([]);
const statusOptions = ref([]);
const accountStatusOptions = ref([]);
const showCustomerResponseDialog = ref(false);
const showAccountResponseDialog = ref(false);

// 初始化选项数据
const initOptions = async () => {
  try {
    const [failStepResult, statusResult, accountStatusResult] = await Promise.all([
      getEnum("PayChannelServiceRequestFailStep").catch(() => ({ result: [] })),
      getEnum("PayChannelServiceRequestStatus").catch(() => ({ result: [] })),
      getEnum("VAStatus").catch(() => ({ result: [] })),
    ]);

    failStepOptions.value = (failStepResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    statusOptions.value = (statusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    accountStatusOptions.value = (accountStatusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (error) {
    console.error("初始化选项失败:", error);
  }
};

// 获取进件类型标签
const getFailStepLabel = (failStep) => {
  const option = failStepOptions.value.find(item => item.value === failStep);
  return option ? option.label : failStep || "--";
};

// 获取状态标签类型
const getStatusTagType = (status) => {
  if (status === null || status === undefined) {
    return "info";
  }
  const statusMap = {
    0: "warning",  // PendingCustomer
    1: "info",     // CustomerProcessing
    2: "warning",  // PendingAccount
    3: "info",     // AccountProcessing
    4: "success",  // Completed
    5: "danger",   // Failed
  };
  return statusMap[status] || "info";
};

// 获取状态标签文本
const getStatusLabel = (status) => {
  const option = statusOptions.value.find(item => item.value === status);
  return option ? option.label : status || "--";
};

// 获取账户状态标签类型
const getAccountStatusTagType = (accountStatus) => {
  if (accountStatus === null || accountStatus === undefined) {
    return "info";
  }
  const statusMap = {
    0: "info",
    1: "warning",
    2: "success",
    3: "danger",
    4: "warning",
    5: "info",
  };
  return statusMap[accountStatus] || "info";
};

// 获取账户状态标签文本
const getAccountStatusLabel = (accountStatus) => {
  const option = accountStatusOptions.value.find(item => item.value === accountStatus);
  return option ? option.label : "--";
};

// 判断是否为 JSON 字符串
const isJsonString = (value) => {
  if (typeof value !== 'string') return false;
  try {
    const parsed = JSON.parse(value);
    return typeof parsed === 'object' && parsed !== null;
  } catch {
    return false;
  }
};

// 格式化 JSON 字符串
const formatJsonString = (value) => {
  try {
    const parsed = JSON.parse(value);
    return JSON.stringify(parsed, null, 2);
  } catch {
    return value;
  }
};

// 判断是否为对象
const isObject = (value) => {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
};

// 判断是否为长值
const isLongValue = (value) => {
  if (typeof value === 'string') {
    return value.length > 50;
  }
  if (isObject(value)) {
    return Object.keys(value).length > 3;
  }
  return false;
};

// 格式化 JSON 值
const formatJsonValue = (value) => {
  if (value === null || value === undefined) {
    return "--";
  }
  if (Array.isArray(value)) {
    return JSON.stringify(value, null, 2);
  }
  if (typeof value === 'object') {
    return JSON.stringify(value, null, 2);
  }
  return String(value);
};

// 格式化字段标签
const formatFieldLabel = (key) => {
  // 特殊字段映射（不区分大小写）
  const normalizedKey = key.toLowerCase().replace(/[_\s]/g, '');
  
  // 检查是否是 Request Payload 相关的字段
  if (normalizedKey === 'requestpayload' || 
      normalizedKey.includes('request') && normalizedKey.includes('payload')) {
    return t('account.requestPayload');
  }
  
  // 将驼峰命名转换为可读标签
  const formatted = key
    .replace(/([A-Z])/g, ' $1')
    .replace(/[_-]/g, ' ')
    .replace(/^./, str => str.toUpperCase())
    .trim();
  
  // 再次检查格式化后的字符串是否匹配 Request Payload
  const formattedLower = formatted.toLowerCase();
  if (formattedLower.includes('request') && formattedLower.includes('payload')) {
    return t('account.requestPayload');
  }
  
  return formatted;
};

// 检查是否是需要隐藏的字段
const isHiddenField = (key) => {
  const normalizedKey = key.toLowerCase().replace(/[_\s]/g, '');
  // 隐藏 Customer Response 和 Account Response 相关字段
  return normalizedKey === 'customerresponse' || 
         normalizedKey === 'accountresponse' ||
         (normalizedKey.includes('customer') && normalizedKey.includes('response')) ||
         (normalizedKey.includes('account') && normalizedKey.includes('response'));
};

// 解析 JSON 字段
const parseJsonFields = (data) => {
  const jsonFields = {};
  const otherFields = [];
  
  if (!data) return { jsonFields, otherFields };

  // 需要检查的字段列表（根据实际 API 响应调整）
  const fieldsToCheck = Object.keys(data);
  
  fieldsToCheck.forEach(key => {
    const value = data[key];
    
    // 跳过已处理的字段
    if (['id', 'failStep', 'status', 'channelProvider', 'channelAccountId', 
         'accountId', 'creationTime', 'lastModificationTime', 'account'].includes(key)) {
      return;
    }

    // 跳过需要隐藏的字段（Customer Response 和 Account Response）
    if (isHiddenField(key)) {
      return;
    }

    // 如果是字符串，尝试解析为 JSON
    if (typeof value === 'string' && isJsonString(value)) {
      try {
        const parsed = JSON.parse(value);
        if (isObject(parsed)) {
          jsonFields[key] = parsed;
        } else {
          otherFields.push({ key, value: formatJsonString(value) });
        }
      } catch {
        otherFields.push({ key, value });
      }
    }
    // 如果已经是对象
    else if (isObject(value)) {
      jsonFields[key] = value;
    }
    // 其他类型
    else {
      otherFields.push({ key, value });
    }
  });

  return { jsonFields, otherFields };
};

// 计算 JSON 字段和其他字段
const jsonFields = computed(() => {
  if (!detailData.value) return {};
  const { jsonFields } = parseJsonFields(detailData.value);
  return jsonFields;
});

const otherFields = computed(() => {
  if (!detailData.value) return [];
  const { otherFields } = parseJsonFields(detailData.value);
  return otherFields;
});

// 获取响应数据（不区分大小写）
const getResponseData = (fieldName) => {
  if (!detailData.value) return null;
  const normalizedFieldName = fieldName.toLowerCase();
  for (const key in detailData.value) {
    if (key.toLowerCase() === normalizedFieldName) {
      return detailData.value[key];
    }
  }
  return null;
};

// 检查是否有客户响应
const hasCustomerResponse = computed(() => {
  const response = getResponseData('customerResponse');
  return response !== null && response !== undefined && response !== '';
});

// 检查是否有账户响应
const hasAccountResponse = computed(() => {
  const response = getResponseData('accountResponse');
  return response !== null && response !== undefined && response !== '';
});

// 解析响应为 JSON（优先返回对象，解析失败时返回原始值）
const parseJson = (value) => {
  if (!value) return null;
  try {
    if (typeof value === "string") {
      return JSON.parse(value);
    }
    return value;
  } catch {
    return value;
  }
};

// 获取详情数据
const fetchDetail = async () => {
  loading.value = true;
  try {
    getPayChannelServiceRequestById.params = {
      id: route.params.id,
    };
    const { result } = await request(getPayChannelServiceRequestById);
    detailData.value = result;
  } catch (error) {
    console.error("获取详情失败:", error);
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
.application-detail-container {
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

.json-display {
  pre {
    margin: 0;
    padding: 12px;
    background-color: #f5f7fa;
    border-radius: 4px;
    font-family: 'Courier New', monospace;
    font-size: 12px;
    line-height: 1.5;
    overflow-x: auto;
    max-height: 400px;
    overflow-y: auto;
  }
}

.json-object {
  :deep(.el-descriptions) {
    margin-top: 8px;
  }
}

.json-value {
  word-break: break-word;
}

.footer-buttons {
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #ebeef5;
  display: flex;
  justify-content: center;
  gap: 16px;
}

.json-viewer-container {
  max-height: 600px;
  overflow: auto;
  background-color: #f5f7fa;
  border-radius: 4px;
  padding: 16px;
  
  .json-display-pre {
    margin: 0;
    padding: 0;
    font-family: 'Courier New', Courier, monospace;
    font-size: 13px;
    line-height: 1.6;
    color: #303133;
    white-space: pre-wrap;
    word-wrap: break-word;
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
