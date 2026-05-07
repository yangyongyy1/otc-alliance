<template>
    <el-dialog
      v-model="visible"
      :title="t('alliance.settings')"
      width="800px"
      @close="handleClose"
      class="user-add-dialog"
      :close-on-click-modal="false"
      v-loading="loading"
    >
      <el-form
        :model="form"
        :rules="rules"
        ref="formRef"
        label-width="150px"
        label-position="left"
        class="user-add-form"
      >
        <el-form-item :label="t('alliance.userAgreement')" prop="userAgreementLink" >
          <RichTextEditor
            v-model="form.userAgreementLink"
            :placeholder="t('alliance.pleaseEnterUserAgreement')"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.websiteUrl')" prop="webSiteUrl" >
          <el-input
            v-model="form.webSiteUrl"
            :placeholder="t('alliance.pleaseEnterWebsiteUrl')"
            size="large"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.copyrightInfo')" prop="copyrightInfo">
          <el-input
            v-model="form.copyrightInfo"
            type="textarea"
            :rows="6"
            :placeholder="t('alliance.pleaseEnterCopyrightInfo')"
            size="large"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.disclaimer')" prop="disclaimer" >
          <RichTextEditor
            v-model="form.disclaimer"
            :placeholder="t('alliance.pleaseEnterDisclaimer')"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.themeColor')" prop="themeColor" >
          <el-color-picker
            v-model="form.themeColor"
            size="large"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.serviceEmail')" prop="serviceEmail">
          <el-input
            v-model="form.serviceEmail"
            :placeholder="t('alliance.pleaseEnterServiceEmail')"
            size="large"
          />
        </el-form-item>

        <el-form-item :label="t('alliance.websiteIcon')" prop="webSiteIcon" >
            <el-upload
                class="icon-uploader"
                :action="uploadUrl"
                :headers="headers"
                :on-success="handleIconSuccess"
                :show-file-list="false"
            >
            <img :src="form.webSiteIcon" alt="icon" v-if="form.webSiteIcon">
            <el-icon v-else class="icon-uploader-icon"><Plus /></el-icon>
            </el-upload>
        </el-form-item>

        <el-form-item :label="t('alliance.logo')" prop="logoUrl">
            <el-upload
                class="avatar-uploader"
                :action="uploadUrl"
                :headers="headers"
                :on-success="handleSuccess"
                :before-upload="beforeUpload"
                :show-file-list="false"
            >
            <img :src="form.logoUrl" alt="logo" v-if="form.logoUrl">
            <el-icon v-else class="avatar-uploader-icon"><Plus /></el-icon>
            </el-upload>
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
import { ref, reactive, watch } from "vue";
import { useI18n } from "vue-i18n";
import request from "@/utils/request";
import { setAllianceSetting, getAllianceSetting } from "@/api/allianceManagement";
import { ElMessage } from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import RichTextEditor from "@/components/RichTextEditor/index.vue";

const { t } = useI18n();
  
  const emit = defineEmits(["success"]);
  const visible = ref(false);
  const formRef = ref();
  const form = reactive({
    userAgreementLink: "",
    webSiteUrl: "",
    copyrightInfo: "",
    disclaimer: "",
    themeColor: "#409EFF",
    serviceEmail: "",
    webSiteIcon: "",
    logoUrl: "",
    id: "",
  });
  
  
  const loading = ref(false);
  const uploadUrl = ref(import.meta.env.VITE_API_BASE_URL + "/api/services/app/Basic/UploadFile");
  const headers = ref({
    Authorization: "Bearer " + localStorage.getItem("token"),
  });


  const isEdit = ref(false);
  
  const rules = {
   
  };
  
  const handleClose = () => {
    formRef.value?.resetFields();
    visible.value = false;
   
  };

  const handleSuccess = (response, file, fileList) => {
    form.logoUrl = response.result;
    ElMessage.success(t('common.uploadSuccess'));
  };

  const handleIconSuccess = (response, file, fileList) => {
    form.webSiteIcon = response.result;
    ElMessage.success(t('common.uploadSuccess'));
  };

  const handleError = (error, file, fileList) => {
    ElMessage.error(error.message || t('common.uploadFailed'));
  };

  const beforeUpload = (file) => {
    const isJPG = file.type === 'image/jpeg' || file.type === 'image/png';
    const isLt2M = file.size / 1024 / 1024 < 2;

    if (!isJPG) {
      ElMessage.error(t('alliance.uploadImageFormatError'));
    }
    if (!isLt2M) {
      ElMessage.error(t('alliance.uploadImageSizeError'));
    }
    return isJPG && isLt2M;
  };




  const handleSubmit = async () => {
    if (!formRef.value) return;
    await formRef.value.validate();
    loading.value = true;
    try {
        setAllianceSetting.data = form;
        const { success } = await request(setAllianceSetting);
        if (success) {
            ElMessage.success(t('common.operationSuccess'));
            handleClose();
            emit("fetchData");
            
        } 
      
    } finally {
      loading.value = false;
    }
  };
  
  
  
  // 存储当前联盟的 id
  const currentAllianceId = ref(null);

  // 加载联盟设置数据
  const loadAllianceSetting = async (id) => {
    loading.value = true;
    try {
      getAllianceSetting.params = id ? { id } : {};
      const { success, result } = await request(getAllianceSetting);
      if (success && result) {
        form.userAgreementLink = result.userAgreementLink || "";
        form.webSiteUrl = result.webSiteUrl || "";
        form.copyrightInfo = result.copyrightInfo || "";
        form.disclaimer = result.disclaimer || "";
        form.themeColor = result.themeColor || "#409EFF";
        form.serviceEmail = result.serviceEmail || "";
        form.webSiteIcon = result.webSiteIcon || "";
        form.logoUrl = result.logoUrl || "";
        form.id = result.id || "";
      }
    } catch (error) {
      console.error("Failed to load alliance settings:", error);
    } finally {
      loading.value = false;
    }
  };

  // 监听弹窗打开，自动加载数据
  watch(visible, (newVal) => {
    if (newVal) {
      // 弹窗打开时调用接口加载数据，传递 id 参数
      loadAllianceSetting(currentAllianceId.value);
    }
  });

  // 打开弹窗的方法
  const open = (row) => {
    // 如果传入了 row，获取 id
    if (row) {
      currentAllianceId.value = row.id || null;
      // 如果传入了 row，使用 row 的数据覆盖接口数据
      form.userAgreementLink = row.userAgreementLink || "";
      form.webSiteUrl = row.webSiteUrl || "";
      form.copyrightInfo = row.copyrightInfo || "";
      form.disclaimer = row.disclaimer || "";
      form.themeColor = row.themeColor || "#409EFF";
      form.serviceEmail = row.serviceEmail || "";
      form.webSiteIcon = row.webSiteIcon || "";
      form.logoUrl = row.logoUrl || "";
      form.id = row.id || "";
    } else {
      currentAllianceId.value = null;
    }
    visible.value = true;
  };
  
  // 暴露方法给父组件
  defineExpose({
    open,
    loadAllianceSetting,
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
  .icon-uploader {
    border: 1px dashed var(--el-border-color);
    border-radius: 6px;
    cursor: pointer;
    position: relative;
    overflow: hidden;
    transition: var(--el-transition-duration-fast);
    width: 200px;
    height: 200px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }
  .icon-uploader:hover {
    border-color: var(--el-color-primary);
  }
  .icon-uploader img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    display: block;
  }
  .el-icon.icon-uploader-icon {
    font-size: 28px;
    color: #8c939d;
    width: 200px;
    height: 200px;
    text-align: center;
    line-height: 200px;
  }
  .upload-tip {
    font-size: 12px;
    color: #909399;
    margin-top: 8px;
  }



  </style>
  
  