<template>
    <el-dialog
      v-model="visible"
      :title="t('merchant.authSettings')"
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
      <el-form-item label="认证方式" prop="authType" required>
            <el-select
                v-model="form.authType"
                :placeholder="t('merchant.pleaseSelectAuthType')"
                size="large"
            >
                <el-option v-for="item in authTypeOptions" :key="item.value" :label="item.displayName" :value="item.value" />
            </el-select>
        </el-form-item>
        <el-form-item :label="t('merchant.authStandard')" prop="authStandard" required>
            <el-select
                v-model="form.authStandard"
                :placeholder="t('merchant.pleaseSelectAuthStandard')"
                size="large"
            >
                <el-option v-for="item in authStandardOptions" :key="item.value" :label="item.displayName" :value="item.value" />
            </el-select>
        </el-form-item>
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
  import { ref, reactive } from "vue";
  import { useI18n } from "vue-i18n";
  import request from "@/utils/request";

  import { settingMerchant } from "@/api/allianceManagement";
  import { getEnum } from "@/api/enum";
  import { ElMessage } from "element-plus";

  const { t } = useI18n();
  
  const emit = defineEmits(["success"]);
  const visible = ref(false);
  const formRef = ref();
  const form = reactive({
    authType: "",
    authStandard: undefined,
  });

  const authTypeOptions = ref([]);
  const authStandardOptions = ref([]);
  const fetchAuthTypeOptions = async () => {
    const { result } = await getEnum("AuthType");
    authTypeOptions.value = result;
  };
  const fetchAuthStandardOptions = async () => {
    const { result } = await getEnum("AuthStandardLevel");
    authStandardOptions.value = result;
  };
  
  const loading = ref(false);
  const uploadUrl = ref(import.meta.env.VITE_API_BASE_URL + "/api/services/app/Basic/UploadFile");
  const headers = ref({
    Authorization: "Bearer " + localStorage.getItem("token"),
  });


  const isEdit = ref(false);
  
  const rules = {
    authStandard: [{ required: true, message: () => t('merchant.pleaseSelectAuthStandard'), trigger: "change" }],
    userAgreementLink: [{ required: true, message: "请输入用户协议", trigger: "blur" },{ validator: (rule, value, callback) => {
        if (!value.startsWith('http') && !value.startsWith('https')) {
            callback(new Error("请输入正确的用户协议"));
        } else {
            callback();
        }
    }, trigger: "blur" }],
    logoUrl: [{ required: true, message: "请上传Logo", trigger: "blur" }],
  };
  
  const handleClose = () => {
    formRef.value?.resetFields();
    visible.value = false;
   
  };

  const handleSuccess = (response, file, fileList) => {
    form.logoUrl = response.result;
  };

  const handleError = (error, file, fileList) => {
    ElMessage.error(error.message);
  };

  const beforeUpload = (file) => {
    const isJPG = file.type === 'image/jpeg' || file.type === 'image/png';
    const isLt2M = file.size / 1024 / 1024 < 2;

    if (!isJPG) {
      ElMessage.error('上传头像图片只能是 JPG/PNG 格式!');
    }
    if (!isLt2M) {
      ElMessage.error('上传头像图片大小不能超过 2MB!');
    }
    return isJPG && isLt2M;
  };



  const handleSubmit = async () => {
    if (!formRef.value) return;
    await formRef.value.validate();
    loading.value = true;
    try {
        settingMerchant.data = form;
        const { success } = await request(settingMerchant);
        if (success) {
            ElMessage.success(t('common.operationSuccess'));
            handleClose();
            emit("fetchData");
            
        } 
      
    } finally {
      loading.value = false;
    }
  };
  
  
  
  // 打开弹窗时先重置表单再填入当前行，避免打开第二个商户时仍显示上一个的值
  const open = (row) => {
    form.authType = "";
    form.authStandard = undefined;
    form.id = undefined;
    visible.value = true;
    form.authType = row.authType;
    form.authStandard = row.authStandard ?? undefined;
    form.id = row.id;
    fetchAuthTypeOptions();
    fetchAuthStandardOptions();
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
  .avatar-uploader {
    border: 1px dashed var(--el-border-color);
    border-radius: 6px;
    cursor: pointer;
    position: relative;
    overflow: hidden;
    transition: var(--el-transition-duration-fast);
    width: 178px;
    height: 178px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }
  .avatar-uploader:hover {
    border-color: var(--el-color-primary);
  }
  .avatar-uploader img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    display: block;
  }
  .el-icon.avatar-uploader-icon {
    font-size: 28px;
    color: #8c939d;
    width: 178px;
    height: 178px;
    text-align: center;
    line-height: 178px;
  }



  </style>
  
  