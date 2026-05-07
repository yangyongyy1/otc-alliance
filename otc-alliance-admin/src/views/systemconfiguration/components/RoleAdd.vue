<template>
  <el-dialog
    v-model="dialogVisible"
    :title="t('system.addRole')"
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
      <el-form-item :label="t('system.roleIdentifier')" prop="name">
        <el-input v-model="formData.name" :placeholder="t('system.pleaseEnterRoleIdentifier')" />
      </el-form-item>
      <el-form-item :label="t('system.roleName')" prop="displayName">
        <el-input v-model="formData.displayName" :placeholder="t('system.pleaseEnterRoleName')" />
      </el-form-item>
      <el-form-item :label="t('system.roleDescription')" prop="description">
        <el-input
          v-model="formData.description"
          type="textarea"
          :rows="3"
          :placeholder="t('system.pleaseEnterRoleDescription')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          {{ t('common.confirm') }}
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { RoleAdd } from "@/api/permission";

const { t } = useI18n();

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

const formData = ref({
  name: "",
  description: "",
  displayName: "",
});

const rules = computed(() => ({
  name: [
    {
      required: true,
      message: t("system.pleaseEnterRoleIdentifier"),
      trigger: "blur",
    },
  ],
  displayName: [
    {
      required: true,
      message: t("system.pleaseEnterRoleName"),
      trigger: "blur",
    },
  ],
}));

const handleClose = () => {
  formRef.value?.resetFields();
  dialogVisible.value = false;
  //调用父组件刷新方法
  emit("refresh");
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    loading.value = true;
    RoleAdd.data = formData.value;
    const { result } = await request(RoleAdd);
    if (result) {
      ElMessage.success(t("common.addSuccess"));
      emit("success");
      handleClose();
    } else {
      ElMessage.error(result?.error?.message || t("common.addFailed"));
    }
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 打开弹窗的方法
const open = () => {
  dialogVisible.value = true;
  formData.value = {
    name: "",
    displayName: "",
    description: "",
    parentName: "",
  };
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
