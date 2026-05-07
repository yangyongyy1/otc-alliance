<template>
  <el-dialog
    v-model="dialogVisible"
    title="新增角色"
    width="500px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    @close="handleClose"
    :loading="loading"
  >
    <el-form
      ref="formRef"
      :model="formData"
      :rules="rules"
      label-width="100px"
      class="permission-form"
    >
      <el-form-item label="角色" prop="roleName">
        <el-input v-model="formData.roleName" placeholder="请输入角色名称" />
      </el-form-item>

      <el-form-item label="角色描述" prop="remark">
        <el-input
          v-model="formData.remark"
          type="textarea"
          :rows="3"
          placeholder="请输入角色描述"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          确定
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed, reactive } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import {
  BusinessRoleCreate,
  BusinessRoleUpdate,
} from "@/api/businessPermission";

const props = defineProps({
  permissions: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(["success"]);

const dialogVisible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);

const formData = reactive({
  roleName: "",
  remark: "",
});

const rules = {
  roleName: [
    {
      required: true,
      message: "请输入角色",
      trigger: "blur",
    },
  ],
  remark: [
    {
      required: true,
      message: "请输入角色描述",
      trigger: "blur",
    },
  ],
};

const handleClose = () => {
  resetFormData();
  dialogVisible.value = false;
  //调用父组件刷新方法
  emit("refresh");
};

const resetFormData = () => {
  Object.keys(formData).forEach((key) => {
    formData[key] = "";
  });
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    loading.value = true;
    let api = isEdit.value ? BusinessRoleUpdate : BusinessRoleCreate;
    api.data = formData;
    const { result } = await request(api);
    ElMessage.success("添加成功");
    emit("success");
    handleClose();
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 打开弹窗的方法
const open = (row) => {
  dialogVisible.value = true;
  isEdit.value = false;
  if (row) {
    isEdit.value = true;
    Object.assign(formData, row);
  }
};

// 暴露方法给父组件
defineExpose({
  open,
});
</script>

<style scoped>
.permission-form {
  padding: 20px 0;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

:deep(.el-dialog__body) {
  padding-top: 10px;
}

:deep(.el-cascader) {
  width: 100%;
}
</style>
