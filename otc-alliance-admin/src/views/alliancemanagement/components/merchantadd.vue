<template>
    <el-dialog
      v-model="visible"
      :title="t('common.add')"
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
        
        <el-form-item :label="t('user.allianceName')" prop="allianceId" required>
            <el-select
                v-model="form.allianceId"
                :placeholder="t('merchant.pleaseSelectAlliance')"
                size="large"
            >
                <el-option v-for="item in allianceOptions" :key="item.value" :label="item.label" :value="item.value" />
            </el-select>
        </el-form-item>
        <el-form-item :label="t('merchant.merchantName')" prop="name" required>
            <el-input
                v-model="form.name"
                :placeholder="t('merchant.pleaseEnterMerchantName')"
                size="large"
            />
        </el-form-item>
        <el-form-item :label="t('merchant.merchantId')" prop="businessID" required>
            <el-input
                v-model="form.businessID"
                :placeholder="t('merchant.pleaseEnterMerchantId')"
                size="large"
            />
        </el-form-item>

        <el-form-item :label="t('merchant.key')" prop="key" required>
            <el-input
                v-model="form.key"
                :placeholder="t('merchant.pleaseEnterMerchantKey')"
                size="large"
            />
        </el-form-item>

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
                clearable
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
  import { createMerchant, getAllianceOptions } from "@/api/allianceManagement";
  import { getEnum } from "@/api/enum";
  import { ElMessage } from "element-plus";

  const { t } = useI18n();
  
  const emit = defineEmits(["success"]);
  const visible = ref(false);
  const formRef = ref();
  const form = reactive({
    name: "",
    allianceId: "",
    businessID: "",
    key: "",
    authType: "",
    authStandard: undefined,
  });
  
  const allianceOptions = ref([]);

  const fetchAllianceOptions = async () => {
    const { result } = await request(getAllianceOptions);
    allianceOptions.value = result.map(item=>{
        return{
            label: item.name,
            value: item.id,
        }
    });
  };

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
  
  const isEdit = ref(false);
  
  const rules = {
    name: [{ required: true, message: t("merchant.pleaseEnterMerchantName"), trigger: "blur" }],
  };
  
  const handleClose = () => {
    formRef.value?.resetFields();
    visible.value = false;
   
  };
  
  const handleSubmit = async () => {
    if (!formRef.value) return;
    await formRef.value.validate();
    loading.value = true;
    try {
        createMerchant.data = form;
        const { success } = await request(createMerchant);
        if (success) {
            ElMessage.success(t('common.operationSuccess'));
            handleClose();
            emit("fetchData");
            
        } 
      
    } finally {
      loading.value = false;
    }
  };
  
  
  
  // 打开弹窗的方法
  const open = (row) => {
    visible.value = true;
    fetchAuthTypeOptions();
    fetchAuthStandardOptions();
    fetchAllianceOptions();
    if (row) {
      Object.assign(form, row);
      isEdit.value = true;
      form.roleName = row.roleName;
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
  