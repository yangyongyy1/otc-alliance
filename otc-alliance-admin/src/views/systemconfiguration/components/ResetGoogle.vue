<template>
  <div>
    <el-dialog v-model="dialogVisible" :title="t('system.resetGoogleAuth')" width="30%">
      <el-form :model="form" label-width="120px" ref="formRef" :rules="rules">
        <SecurityVerifyFields :modelValue="form" />
      </el-form>
      <template #footer>
        <el-button type="primary" @click="handleResetPwd">{{ t('common.confirm') }}</el-button>
        <el-button @click="handleCancel">{{ t('common.cancel') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, defineExpose, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { UserResetGA } from "@/api/systemUser";

import SecurityVerifyFields from "@/components/SecurityVerifyFields.vue";

const { t } = useI18n();
const dialogVisible = ref(false);
const form = reactive({
  userPwd: "",
  smsCode: "",
  gaCode: "",
  userId: "",
});

const rules = computed(() => ({
  userPwd: [{ required: true, message: t("common.pleaseEnterPassword") }],
}));

const formRef = ref(null);

const emit = defineEmits(["refresh"]);

const handleResetPwd = () => {
  formRef.value.validate(async (valid) => {
    if (valid) {
      UserResetGA.data = form;
      const { result } = await request(UserResetGA);
      if (result) {
        ElMessage.success(t("system.googleAuthResetSuccess"));
        dialogVisible.value = false;
        formRef.value.resetFields();
        emit("refresh");
      }
    }
  });
};
const handleCancel = () => {
  dialogVisible.value = false;
  formRef.value.resetFields();
};

const open = (id) => {
  dialogVisible.value = true;
  form.userId = id;
};

defineExpose({
  open,
});
</script>

<style scoped>
.user-add-dialog >>> .el-dialog__body {
  padding: 32px 40px 10px 40px;
}
.user-add-form {
  margin-top: 10px;
}
.user-add-form :deep(.el-form-item) {
  margin-bottom: 18px;
}
.user-add-form :deep(.el-form-item__label) {
  position: relative;
  font-size: 16px;
  line-height: 1.2;
  padding-left: 0;
  font-weight: normal;
  color: #606266;
  display: flex;
  align-items: center;
}
.user-add-form :deep(.el-form-item__label)::before {
  content: "*";
  display: inline-block;
  width: 14px;
  color: transparent;
  margin-right: 4px;
}
.user-add-form :deep(.el-form-item__label) .el-form-item__required {
  color: #f56c6c;
  position: absolute;
  left: 0;
  width: 14px;
  text-align: left;
  font-size: 16px;
  font-weight: normal;
  background: transparent;
}
.user-add-form :deep(.el-form-item__label):not(.el-form-item__label--right) {
  justify-content: flex-start;
}
.user-add-form :deep(.el-input),
.user-add-form :deep(.el-select),
.user-add-form :deep(.el-textarea) {
  font-size: 14px;
}
.footer-btns {
  display: flex;
  justify-content: flex-end;
  gap: 18px;
  padding: 8px 0 0 0;
}
.user-add-dialog >>> .el-dialog__header {
  text-align: left;
  padding-left: 40px;
}
</style>
