<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? t('common.edit') : t('common.add')"
    width="650px"
    @close="handleClose"
    class="user-add-dialog"
    :close-on-click-modal="false"
    v-loading="loading"
  >
    <el-form
      :model="form"
      :rules="rules"
      ref="formRef"
      label-width="110px"
      label-position="left"
      class="user-add-form"
    >
      <el-form-item :label="t('system.userName')" prop="userName" required>
        <el-input
          v-model="form.userName"
          :placeholder="t('system.pleaseEnterUserName')"
          size="large"
          :disabled="isEdit"
        />
      </el-form-item>
      <el-form-item :label="t('common.email')" prop="emailAddress" required>
        <el-input
          v-model="form.emailAddress"
          :placeholder="t('common.pleaseEnterEmail')"
          size="large"
          :disabled="isEdit"
        />
      </el-form-item>
      <!-- <el-form-item label="区号" prop="areaCode" required>
        <AreaCodeSelect v-model="form.areaCode" placeholder="请选择区号" />
      </el-form-item>
      <el-form-item label="绑定手机" prop="phoneNumber" required>
        <el-input
          v-model="form.phoneNumber"
          placeholder="请输入手机号"
          size="large"
        />
      </el-form-item>
      <el-form-item label="验证码" prop="mobileCode">
        <el-input
          v-model="form.mobileCode"
          placeholder="请输入短信验证码"
          style="width: 60%"
          size="large"
        />
        <SmsCodeButton
          :areaCode="form.areaCode"
          :phoneNumber="form.phoneNumber"
          style="margin-left: 12px"
        />
      </el-form-item> -->
      <el-form-item :label="t('system.userRole')" prop="roleName" required>
        <el-select v-model="form.roleName" :placeholder="t('system.pleaseSelectUserRole')">
          <el-option
            v-for="item in roleList"
            :key="item.id"
            :label="item.name"
            :value="item.name"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="t('common.remark')" prop="remark">
        <el-input
          v-model="form.remark"
          type="textarea"
          :placeholder="t('common.pleaseEnterRemark')"
          :autosize="{ minRows: 2, maxRows: 4 }"
        />
      </el-form-item>
      <el-form-item :label="t('common.status')" prop="isActive">
        <el-switch v-model="form.isActive" />
      </el-form-item>

      <!-- 新增安全认证组件 -->
      <!-- <SecurityVerifyFields :modelValue="form" /> -->
    </el-form>
    <template #footer>
      <div class="footer-btns">
        <el-button @click="handleClose" size="large">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" size="large" :loading="loading"
          >{{ t('common.confirm') }}</el-button
        >
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { useI18n } from "vue-i18n";
import AreaCodeSelect from "@/components/AreaCodeSelect.vue";
import SmsCodeButton from "@/components/SmsCodeButton.vue";
import SecurityVerifyFields from "@/components/SecurityVerifyFields.vue";
import request from "@/utils/request";
import { GetRoleSelectItem } from "@/api/permission";
import { UserCreate, UserUpdate } from "@/api/systemUser";
import { ElMessage } from "element-plus";

const { t } = useI18n();

const emit = defineEmits(["success"]);
const visible = ref(false);
const formRef = ref();
const form = reactive({
  userName: "",
  surname: "",
  name: "",
  roleName: "",
  emailAddress: "",
  remark: "",
  password: "",
  repassword: "",
  phoneNumber: "",
  mobile: "",
  areaCode: "",
  mobileCode: "",
  id: "",
  roleNames: [],
  isActive:true,
});

const roleList = ref([]);

const loading = ref(false);

const isEdit = ref(false);

const rules = computed(() => ({
  userName: [{ required: true, message: t("system.pleaseEnterUserName"), trigger: "blur" }],
  emailAddress: [{ required: true, message: t("common.pleaseEnterEmail"), trigger: "blur" }],
  areaCode: [{ required: true, message: t("common.pleaseSelectAreaCode"), trigger: "change" }],
  phoneNumber: [{ required: true, message: t("common.pleaseEnterPhoneNumber"), trigger: "blur" }],
  roleName: [{ required: true, message: t("system.pleaseSelectUserRole"), trigger: "change" }],

  userPwd: [{ required: true, message: t("common.pleaseEnterLoginPassword"), trigger: "blur" }],
  smssCode: [{ required: true, message: t("common.pleaseEnterSmsCode"), trigger: "blur" }],
  gaCode: [{ required: true, message: t("common.pleaseEnterGaCode"), trigger: "blur" }],
}));

const handleClose = () => {
  formRef.value?.resetFields();
  visible.value = false;
  //调用父组件刷新方法
  emit("refresh");
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate();
  loading.value = true;
  try {
    let api = UserCreate;
    if (isEdit.value) {
      api = UserUpdate;
    }

    const payload = { ...form };
    payload.roleNames = [form.roleName];
    payload.surname = form.userName;
    payload.name = form.userName;
    // 新增用户时，密码后端固定为默认值
    if (!isEdit.value) {
      payload.password = "Aa@123456.";
    }

    api.data = payload;
    const { result } = await request(api);
    if (result) {
      ElMessage.success(t("common.addSuccess"));
      handleClose();
    } else {
      ElMessage.error(result.error.message);
    }
  } finally {
    loading.value = false;
  }
};

const fetchRoleList = async () => {
  const { result } = await request(GetRoleSelectItem);
  roleList.value = result.items;
};

// 打开弹窗的方法
const open = (row) => {
  visible.value = true;

  fetchRoleList();
  if (row) {
    Object.assign(form, row);
    isEdit.value = true;
    form.roleName = row.roleNames[0];
  }
};

// 暴露方法给父组件
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
