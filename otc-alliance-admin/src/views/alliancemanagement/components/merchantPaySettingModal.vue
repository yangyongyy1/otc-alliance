<template>
  <el-dialog
    v-model="visible"
    :title="t('merchant.functionSettings')"
    width="500px"
    @close="handleClose"
    class="user-add-dialog"
    :close-on-click-modal="false"
    v-loading="loading"
  >
    <el-form ref="formRef" label-width="120px" label-position="left" class="user-add-form">
      <el-form-item :label="t('merchant.merchantName')">
        <span>{{ merchantName }}</span>
      </el-form-item>
      <el-form-item :label="t('merchant.paySettings')" prop="paySettings">
        <el-checkbox-group v-model="form.paySettingsArr">
          <div class="checkbox-list">
            <el-checkbox
              v-for="item in paySettingTypeOptions"
              :key="item.value"
              :label="item.value"
            >
              {{ item.displayName }}
            </el-checkbox>
          </div>
        </el-checkbox-group>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="footer-btns">
        <el-button @click="handleClose" size="large">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" size="large" :loading="loading">
          {{ t('common.confirm') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive } from "vue";
import { useI18n } from "vue-i18n";
import request from "@/utils/request";
import { updateMerchantPaySettings } from "@/api/allianceManagement";
import { getEnum } from "@/api/enum";
import { ElMessage } from "element-plus";

const { t } = useI18n();
const emit = defineEmits(["fetchData"]);
const visible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const merchantName = ref("");
const merchantId = ref(null);

const form = reactive({
  paySettingsArr: [],
});

const paySettingTypeOptions = ref([]);

const loadOptions = async () => {
  const { result } = await getEnum("MerchantPaySettingType");
  paySettingTypeOptions.value = result || [];
};

const handleClose = () => {
  visible.value = false;
  form.paySettingsArr = [];
  merchantName.value = "";
  merchantId.value = null;
};

const handleSubmit = async () => {
  loading.value = true;
  try {
    const paySettings = form.paySettingsArr.length
      ? form.paySettingsArr.map((v) => String(v)).join(",")
      : "";
    updateMerchantPaySettings.data = { id: merchantId.value, paySettings };
    const { success } = await request(updateMerchantPaySettings);
    if (success) {
      ElMessage.success(t("common.operationSuccess"));
      handleClose();
      emit("fetchData");
    }
  } finally {
    loading.value = false;
  }
};

const open = (row) => {
  merchantId.value = row.id;
  merchantName.value = row.name;
  if (row.paySettings && typeof row.paySettings === "string") {
    form.paySettingsArr = row.paySettings
      .split(",")
      .map((s) => parseInt(s.trim(), 10))
      .filter((n) => !Number.isNaN(n));
  } else {
    form.paySettingsArr = [];
  }
  visible.value = true;
  loadOptions();
};

defineExpose({ open });
</script>

<style scoped>
.user-add-dialog >>> .el-dialog__body {
  padding: 32px 40px 10px 40px;
}
.user-add-form {
  margin-top: 10px;
}
.footer-btns {
  display: flex;
  justify-content: flex-end;
  gap: 18px;
  padding: 8px 0 0 0;
}
.checkbox-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.checkbox-list .el-checkbox {
  margin-right: 0;
}
</style>
