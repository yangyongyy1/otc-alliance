<template>
    <el-dialog
      v-model="visible"
      title="新增"
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
        <el-form-item label="联盟名称" prop="name" required>
          <el-input
            v-model="form.name"
            placeholder="请输入联盟名称"
            size="large"
            :disabled="isEdit"
          />
        </el-form-item>
        
      </el-form>
      <template #footer>
        <div class="footer-btns">
          <el-button @click="handleClose" size="large">取消</el-button>
          <el-button type="primary" @click="handleSubmit" size="large" :loading="loading"
            >确定</el-button
          >
        </div>
      </template>
    </el-dialog>
  </template>
  
  <script setup>
  import { ref, reactive } from "vue";
  import request from "@/utils/request";
  import { createAlliance } from "@/api/allianceManagement";
  import { ElMessage } from "element-plus";
  
  const emit = defineEmits(["success"]);
  const visible = ref(false);
  const formRef = ref();
  const form = reactive({
    name: "",
  });
  
  const roleList = ref([]);
  
  const loading = ref(false);
  
  const isEdit = ref(false);
  
  const rules = {
    name: [{ required: true, message: "请输入联盟名称", trigger: "blur" }],
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
        createAlliance.data = form;
        const { success } = await request(createAlliance);
        if (success) {
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
  
    fetchRoleList();
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
  