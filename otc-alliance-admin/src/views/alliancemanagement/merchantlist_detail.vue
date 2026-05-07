<template>
  <div class="merchant-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>{{ t("merchant.paySettings") }} - {{ merchantName }}</h1>
      <div class="header-button">
        <el-button :icon="Refresh" @click="fetchData" :loading="loading">{{ t("common.refresh") }}</el-button>
        <el-button @click="router.back()">{{ t("common.return") }}</el-button>
      </div>
    </div>

    <!-- Direct Pay / VA Pay 选显卡 -->
    <div class="detail-content tab-content">
      <el-tabs v-model="activeTab" type="card" class="pay-setting-tabs">
        <el-tab-pane label="Direct Pay" name="direct" />
        <el-tab-pane label="VA Pay" name="va" />
      </el-tabs>

      <!-- 数据筛选 -->
      <div class="query-section">
        <el-form :model="queryForm" class="query-form">
          <div class="query-row">
            <el-form-item :label="t('merchant.currency') + ':'">
              <el-select v-model="queryForm.currency" clearable :placeholder="t('common.pleaseSelect')" class="query-input">
                <el-option v-for="item in currencyOptions" :key="item.value" :label="item.label" :value="item.value" />
              </el-select>
            </el-form-item>
            <el-form-item v-if="currentTabType === 1" :label="t('merchant.method') + ':'">
              <el-select v-model="queryForm.paymentMethod" clearable :placeholder="t('common.pleaseSelect')" class="query-input">
                <el-option v-for="item in paymentMethodOptions" :key="item.value" :label="item.label" :value="item.value" />
              </el-select>
            </el-form-item>
            <el-form-item v-if="currentTabType === 2" :label="t('merchant.channel') + ':'">
              <el-select v-model="queryForm.channelCode" clearable :placeholder="t('common.pleaseSelect')" class="query-input" filterable>
                <el-option v-for="item in channelOptionsForVa" :key="item.value" :label="item.label" :value="item.value" />
              </el-select>
            </el-form-item>
            <el-form-item :label="t('merchant.operator') + ':'">
              <el-input v-model="queryForm.operator" clearable :placeholder="t('common.pleaseEnter')" class="query-input" />
            </el-form-item>
            <el-form-item :label="t('common.status') + ':'">
              <el-select v-model="queryForm.status" clearable :placeholder="t('common.pleaseSelect')" class="query-input status-select">
                <el-option v-for="item in statusOptions" :key="item.value" :label="item.displayName" :value="item.value" />
              </el-select>
            </el-form-item>
          </div>
          <div class="query-row">
            <el-form-item :label="t('common.time') + ':'">
              <el-date-picker
                v-model="queryForm.timeRange"
                type="datetimerange"
                range-separator="至"
                start-placeholder="yyyy/MM/dd"
                end-placeholder="yyyy/MM/dd"
                value-format="YYYY-MM-DD HH:mm:ss"
                class="query-date-range"
              />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :icon="Search" @click="handleQuery">{{ t('common.search') }}</el-button>
              <el-button @click="handleResetQuery">{{ t('common.reset') }}</el-button>
            </el-form-item>
          </div>
        </el-form>
      </div>

      <div class="table-toolbar">
        <el-button type="primary" @click="handleAdd">{{ t("common.add") }}</el-button>
      </div>
      <el-table :data="filteredList" border stripe>
        <el-table-column type="index" :label="t('common.index')" />
        <el-table-column prop="currency" :label="t('merchant.currency')" />
        <el-table-column v-if="currentTabType === 1" prop="paymentMethod" :label="t('merchant.method')" />
        <el-table-column v-if="currentTabType === 2" prop="channelCode" :label="t('merchant.channel')" />
        <el-table-column :label="t('common.time')">
          <template #default="{ row }">
            <div>{{ t('common.creationTime') }}：{{ row.creationTime || '--' }}</div>
            <div>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime || '--' }}</div>
          </template>
        </el-table-column>
        <el-table-column prop="operator" :label="t('merchant.operator')" show-overflow-tooltip />
        <el-table-column prop="status" :label="t('common.status')">
          <template #default="{ row }">
            <el-tag :type="row.status === 0 ? 'success' : 'info'">
              {{ row.status === 0 ? t('common.enable') : t('common.disable') }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.operation')" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleStatusChange(row)">
              {{ row.status === 0 ? t('common.disable') : t('common.enable') }}
            </el-button>
            <el-button link type="primary" @click="handleEdit(row)">{{ t('common.edit') }}</el-button>
            <el-button link type="danger" @click="handleDelete(row)">{{ t('common.delete') }}</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 新增/编辑弹窗：Direct 配置币种+支付方式，VA 配置币种+渠道 -->
    <el-dialog
      v-model="dialogVisible"
      :title="(editingId ? t('common.edit') : t('common.add')) + ' - ' + (currentTabType === 1 ? 'Direct Pay' : 'VA Pay')"
      width="500px"
      @close="resetForm"
      :close-on-click-modal="false"
    >
      <el-form ref="formRef" :model="form" label-width="120px">
        <el-form-item :label="t('merchant.currency')" prop="currency" required>
          <el-select v-model="form.currency" :placeholder="t('common.pleaseSelect')" style="width: 100%" filterable allow-create>
            <el-option v-for="item in currencyOptions" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </el-form-item>
        <!-- Direct：币种 + 支付方式 -->
        <template v-if="currentTabType === 1">
          <el-form-item :label="t('merchant.method')" prop="paymentMethod" required>
            <el-select v-model="form.paymentMethod" :placeholder="t('common.pleaseSelect')" style="width: 100%">
              <el-option
                v-for="item in paymentMethodOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </template>
        <!-- VA：币种 + 渠道 -->
        <template v-if="currentTabType === 2">
          <el-form-item :label="t('merchant.channel')" prop="channelCode" required>
            <el-select v-model="form.channelCode" :placeholder="t('common.pleaseSelect')" style="width: 100%" filterable>
              <el-option
                v-for="item in channelOptionsForVa"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </template>
        <el-form-item :label="t('common.status')" prop="status" required>
          <el-select v-model="form.status" :placeholder="t('common.pleaseSelect')" style="width: 100%">
            <el-option v-for="item in statusOptions" :key="item.value" :label="item.displayName" :value="item.value" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSave" :loading="saveLoading">{{ t('common.confirm') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter, useRoute } from "vue-router";
import { Refresh, Search } from "@element-plus/icons-vue";
import request from "@/utils/request";
import { getEnum } from "@/api/enum";
import {
  getMerchant,
  getMerchantPaySettings,
  createOrUpdateMerchantPaySetting,
  deleteMerchantPaySetting,
  updateMerchantPaySettingStatus,
} from "@/api/allianceManagement";
import { getCurrencies, getChannels, getPaymentMethods } from "@/api/basic";
import { ElMessage, ElMessageBox } from "element-plus";

const router = useRouter();
const route = useRoute();
const { t } = useI18n();

// MerchantPaySettingType: Direct = 1, VA = 2. Tab name 用字符串避免 el-tabs 内部 vnode 为 null
const id = computed(() => route.params.id);
const loading = ref(false);
const saveLoading = ref(false);
const merchantName = ref("");
const paySettingsList = ref([]);
const activeTab = ref("direct");

const currentTabType = computed(() => (activeTab.value === "va" ? 2 : 1));
const typeOptions = ref([]);
const statusOptions = ref([]);
const paymentMethodOptions = ref([]);
const currencyOptions = ref([]);
const channelOptionsForVa = ref([]);
const dialogVisible = ref(false);
const editingId = ref(null);
const formRef = ref(null);

const queryForm = reactive({
  currency: "",
  paymentMethod: "",
  channelCode: "",
  operator: "",
  status: null,
  timeRange: null,
});

const appliedQuery = ref({});

const filteredList = computed(() => {
  let list = paySettingsList.value.filter((item) => item.type === currentTabType.value);
  const q = appliedQuery.value;
  if (!q || Object.keys(q).every((k) => q[k] === undefined || q[k] === null || q[k] === "" || (Array.isArray(q[k]) && q[k].length === 0))) {
    return list;
  }
  if (q.currency) {
    list = list.filter((item) => (item.currency || "").toLowerCase().includes(String(q.currency).toLowerCase()));
  }
  if (q.paymentMethod) {
    list = list.filter((item) => item.paymentMethod === q.paymentMethod);
  }
  if (q.channelCode) {
    list = list.filter((item) => (item.channelCode || "") === q.channelCode);
  }
  if (q.operator) {
    list = list.filter((item) => (item.operator || "").toLowerCase().includes(String(q.operator).toLowerCase()));
  }
  if (q.status !== undefined && q.status !== null) {
    list = list.filter((item) => item.status === q.status);
  }
  if (q.timeRange && Array.isArray(q.timeRange) && q.timeRange.length === 2) {
    const [start, end] = q.timeRange;
    if (start && end) {
      list = list.filter((item) => {
        const ct = item.creationTime || "";
        const mt = item.lastModificationTime || "";
        return (ct >= start && ct <= end) || (mt >= start && mt <= end);
      });
    }
  }
  return list;
});

const handleQuery = () => {
  appliedQuery.value = {
    currency: queryForm.currency || undefined,
    paymentMethod: queryForm.paymentMethod || undefined,
    channelCode: queryForm.channelCode || undefined,
    operator: queryForm.operator || undefined,
    status: queryForm.status,
    timeRange: queryForm.timeRange && queryForm.timeRange.length === 2 ? queryForm.timeRange : undefined,
  };
};

const handleResetQuery = () => {
  queryForm.currency = "";
  queryForm.paymentMethod = "";
  queryForm.channelCode = "";
  queryForm.operator = "";
  queryForm.status = null;
  queryForm.timeRange = null;
  appliedQuery.value = {};
};

const form = reactive({
  type: null,
  currency: "",
  paymentMethod: "",
  channelCode: "",
  status: 0,
});

const loadOptions = async () => {
  const [typeRes, statusRes, currenciesRes, channelsRes, paymentMethodsRes] = await Promise.all([
    getEnum("MerchantPaySettingType"),
    getEnum("OpenClose"),
    request(getCurrencies).catch(() => ({ result: [] })),
    request(getChannels).catch(() => ({ result: [] })),
    request(getPaymentMethods).catch(() => ({ result: [] })),
  ]);
  typeOptions.value = typeRes.result || [];
  statusOptions.value = statusRes.result || [];
  const toOptions = (arr) => (Array.isArray(arr) ? arr : []).map((v) => ({ label: v, value: v }));
  currencyOptions.value = toOptions(currenciesRes.result);
  channelOptionsForVa.value = toOptions(channelsRes.result);
  const raw = paymentMethodsRes.result || [];
  paymentMethodOptions.value = Array.isArray(raw)
    ? raw.map((item) =>
        typeof item === "string"
          ? { label: item, value: item }
          : { label: item.displayName ?? item.platformName ?? "", value: item.platformName ?? item.displayName ?? "" }
      )
    : [];
};

const fetchMerchant = async () => {
  getMerchant.params = { id: id.value };
  const { result } = await request(getMerchant);
  if (result) {
    merchantName.value = result.name;
  }
};

const fetchPaySettings = async () => {
  getMerchantPaySettings.params = { merchantId: id.value };
  const { result } = await request(getMerchantPaySettings);
  paySettingsList.value = result || [];
};

const fetchData = async () => {
  loading.value = true;
  try {
    await Promise.all([fetchMerchant(), fetchPaySettings()]);
  } finally {
    loading.value = false;
  }
};

const resetForm = () => {
  editingId.value = null;
  form.type = null;
  form.currency = "";
  form.paymentMethod = "";
  form.channelCode = "";
  form.status = 0;
  formRef.value?.resetFields();
};

const handleAdd = () => {
  resetForm();
  form.type = currentTabType.value;
  if (form.type === 2) form.paymentMethod = "";
  else form.channelCode = "";
  dialogVisible.value = true;
};

const handleEdit = (row) => {
  editingId.value = row.id;
  form.type = row.type;
  form.currency = row.currency || "";
  form.status = row.status;
  if (row.type === 1) {
    form.paymentMethod = row.paymentMethod || "";
    form.channelCode = "";
  } else {
    form.channelCode = row.channelCode || "";
    form.paymentMethod = "";
  }
  dialogVisible.value = true;
};

const handleSave = async () => {
  if (form.type == null) {
    form.type = currentTabType.value;
  }
  if (!form.currency?.trim()) {
    ElMessage.warning(t("merchant.pleaseSelectCurrency"));
    return;
  }
  const isDirect = form.type === 1;
  if (isDirect && !form.paymentMethod?.trim()) {
    ElMessage.warning(t("merchant.pleaseSelect") + t("merchant.method"));
    return;
  }
  if (!isDirect && !form.channelCode?.trim()) {
    ElMessage.warning(t("merchant.pleaseSelect") + t("merchant.channel"));
    return;
  }
  saveLoading.value = true;
  try {
    createOrUpdateMerchantPaySetting.data = {
      id: editingId.value || undefined,
      merchantId: parseInt(id.value, 10),
      type: form.type,
      currency: form.currency?.trim() || undefined,
      paymentMethod: isDirect ? (form.paymentMethod?.trim() || undefined) : undefined,
      channelCode: !isDirect ? (form.channelCode?.trim() || undefined) : undefined,
      status: form.status,
    };
    const { success } = await request(createOrUpdateMerchantPaySetting);
    if (success) {
      ElMessage.success(t("common.operationSuccess"));
      dialogVisible.value = false;
      fetchPaySettings();
    }
  } finally {
    saveLoading.value = false;
  }
};

const handleStatusChange = (row) => {
  const newStatus = row.status === 0 ? 1 : 0;
  const actionText = newStatus === 0 ? t("common.enable") : t("common.disable");
  ElMessageBox.confirm(
    t("common.confirmToggleStatus", { action: actionText }),
    t("common.tip"),
    {
      confirmButtonText: t("common.confirm"),
      cancelButtonText: t("common.cancel"),
      type: "warning",
    }
  )
    .then(async () => {
      try {
        updateMerchantPaySettingStatus.data = {
          id: row.id,
          merchantId: parseInt(id.value, 10),
          status: newStatus,
        };
        const { success } = await request(updateMerchantPaySettingStatus);
        if (success) {
          ElMessage.success(t("common.operationSuccess"));
          row.status = newStatus;
        }
      } catch {
        // 保持原状态
      }
    })
    .catch(() => {});
};

const handleDelete = (row) => {
  ElMessageBox.confirm(t("common.confirmDelete"), t("common.tip"), {
    confirmButtonText: t("common.confirm"),
    cancelButtonText: t("common.cancel"),
    type: "warning",
  }).then(async () => {
    deleteMerchantPaySetting.params = { id: row.id, merchantId: id.value };
    const { success } = await request(deleteMerchantPaySetting);
    if (success) {
      ElMessage.success(t("common.operationSuccess"));
      fetchPaySettings();
    }
  }).catch(() => {});
};

onMounted(() => {
  loadOptions();
  fetchData();
});
</script>

<style lang="scss" scoped>
.merchant-detail-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: 100vh;
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

.tab-content {
  margin-top: 0;
}

.pay-setting-tabs {
  margin-bottom: 16px;
}

.pay-setting-tabs :deep(.el-tabs__header) {
  margin-bottom: 0;
}

.pay-setting-tabs :deep(.el-tabs__item) {
  font-size: 14px;
}

.pay-setting-tabs :deep(.el-tabs__item.is-active) {
  font-weight: 600;
  color: var(--el-color-primary);
}

.query-section {
  margin-bottom: 16px;
  padding: 16px;
  background: #fafafa;
  border-radius: 6px;
}

.query-form :deep(.el-form-item) {
  margin-bottom: 16px;
  margin-right: 8px;
}

.query-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px 24px;
}

.query-row .query-input {
  width: 140px;
}

.query-row .status-select {
  width: 120px;
}

.query-row .query-date-range {
  width: 360px;
}

.query-form .query-row + .query-row {
  margin-top: 8px;
}

.table-toolbar {
  margin-bottom: 12px;
}
</style>
