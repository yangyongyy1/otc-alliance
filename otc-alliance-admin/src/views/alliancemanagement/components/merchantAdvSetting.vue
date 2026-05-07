<template>
    <el-dialog
      v-model="visible"
      :title="t('merchant.applicationSettings')"
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
        <!-- 动态币种部分 -->
        <div 
          v-for="currency in currencyList" 
          :key="currency"
          class="currency-section"
        >
          <div class="currency-label">{{ currency }}</div>
          <el-form-item 
            :label="t('common.status') + ':'" 
            :prop="`${currency}.status`"
          >
            <el-select
              v-model="form[currency].status"
              :placeholder="t('common.pleaseSelect') + t('common.status')"
              size="large"
              style="width: 100%"
            >
              <el-option
                v-for="item in statusOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
          <el-form-item 
            :label="t('merchant.channel') + ':'" 
            :prop="`${currency}.channelCode`"
          >
            <el-select
              v-model="form[currency].channelCode"
              :placeholder="t('common.pleaseSelect')"
              size="large"
              style="width: 100%"
            >
              <el-option
                v-for="item in channelOptions[currency] || []"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
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
  import request from "@/utils/request";
  import { ElMessage } from "element-plus";
  import { getMerchantAdvSetting, createMerchantAdvSetting, getDataDic } from "@/api/allianceManagement";

  const { t } = useI18n();
  
  const emit = defineEmits(["success", "fetchData"]);
  const visible = ref(false);
  const formRef = ref();
  const currentMerchantId = ref(null);
  
  // 币种列表
  const currencyList = ref([]);
  
  // 表单数据，使用币种作为key
  const form = reactive({});
  
  // 状态选项
  const statusOptions = computed(() => [
    { label: t('common.enable'), value: 0 },
    { label: t('common.disable'), value: 1 },
  ]);

  // 渠道选项（从接口获取），使用币种作为key
  const channelOptions = reactive({});
  
  const loading = ref(false);
  
  const rules = {};
  
  // 初始化表单数据
  const initForm = (currencies) => {
    currencies.forEach(currency => {
      if (!form[currency]) {
        form[currency] = {
          status: "",
          channelCode: "",
        };
      }
    });
  };
  
  // 获取币种和渠道数据
  const fetchChannelOptions = async () => {
    try {
      getDataDic.params = { key: "CurrencyChannel" };
      const { result } = await request(getDataDic);
      if (result) {
        // 如果返回的是 JSON 字符串，需要解析
        let data = result;
        if (typeof result === 'string') {
          data = JSON.parse(result);
        }
        
        // 提取所有不同的币种
        const currencies = [...new Set(data.map(item => item.currency))];
        currencyList.value = currencies.sort();
        
        // 初始化表单数据
        initForm(currencies);
        
        // 根据 currency 分组渠道选项
        currencies.forEach(currency => {
          channelOptions[currency] = data
            .filter(item => item.currency === currency)
            .map(item => ({
              label: item.channelCode,
              value: item.channelCode,
            }));
        });
      }
    } catch (error) {
      ElMessage.error(t('merchant.getChannelDataFailed'));
    }
  };
  
  const handleClose = () => {
    formRef.value?.resetFields();
    visible.value = false;
    currentMerchantId.value = null;
    // 重置表单数据
    currencyList.value.forEach(currency => {
      if (form[currency]) {
        form[currency].status = "";
        form[currency].channelCode = "";
      }
    });
  };

  const handleSubmit = async () => {
    if (!formRef.value) return;
    loading.value = true;
    try {
      const apiData = [];
      
      // 遍历所有币种，收集有值的配置
      currencyList.value.forEach(currency => {
        const currencyData = form[currency];
        if (currencyData && currencyData.status !== "" && currencyData.channelCode) {
          apiData.push({
            currency: currency,
            openClose: currencyData.status,
            channelCode: currencyData.channelCode,
            merchantId: currentMerchantId.value,
          });
        }
      });
      
      if (apiData.length === 0) {
        ElMessage.warning(t('merchant.pleaseSetAtLeastOneCurrency'));
        return;
      }
      
      createMerchantAdvSetting.data = apiData;
      const { success } = await request(createMerchantAdvSetting);
      if (success) {
        ElMessage.success(t('common.operationSuccess'));
        handleClose();
        emit("fetchData");
      }
    } catch (error) {
    } finally {
      loading.value = false;
    }
  };
  
  // 打开弹窗的方法
  const open = async (row) => {
    visible.value = true;
    currentMerchantId.value = row.id;
    loading.value = true;
    
    try {
      // 先获取渠道选项
      await fetchChannelOptions();
      
      // 重置表单数据
      currencyList.value.forEach(currency => {
        if (form[currency]) {
          form[currency].status = "";
          form[currency].channelCode = "";
        }
      });
      
      // 获取商户设置数据
      getMerchantAdvSetting.params = {
        merchantId: row.id,
      };
      const { result } = await request(getMerchantAdvSetting);
      
      if (result && Array.isArray(result)) {
        // 填充表单数据
        result.forEach(item => {
          if (form[item.currency]) {
            form[item.currency].status = item.openClose ?? "";
            form[item.currency].channelCode = item.channelCode || "";
          }
        });
      }
    } catch (error) {
      ElMessage.error(t('common.operationFailed'));
    } finally {
      loading.value = false;
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

  .currency-section {
    margin-bottom: 24px;
  }

  .currency-section:last-child {
    margin-bottom: 0;
  }

  .currency-label {
    font-size: 16px;
    font-weight: 500;
    color: #303133;
    margin-bottom: 16px;
    padding-left: 0;
  }

  </style>
  
  