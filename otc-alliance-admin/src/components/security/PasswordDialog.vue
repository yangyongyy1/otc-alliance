<template>
  <el-dialog
    v-model="dialogVisible"
    title="修改密码"
    width="400px"
    @close="handleClose"
  >
    <el-form
      ref="passwordFormRef"
      :model="passwordForm"
      :rules="passwordRules"
      label-width="100px"
    >
      <el-form-item label="当前密码" prop="oldPassword">
        <el-input
          v-model="passwordForm.oldPassword"
          type="password"
          placeholder="请输入当前密码"
          show-password
        />
      </el-form-item>
      <el-form-item label="新密码" prop="newPassword">
        <el-input
          v-model="passwordForm.newPassword"
          type="password"
          placeholder="请输入新密码"
          show-password
        />
      </el-form-item>
      <el-form-item label="确认新密码" prop="confirmPassword">
        <el-input
          v-model="passwordForm.confirmPassword"
          type="password"
          placeholder="请再次输入新密码"
          show-password
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确认</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { ElMessage } from "element-plus";
import { changePassword } from "@/api/user";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
  userSecuritySetting: {
    type: Object,
    required: true,
  },
});

const emit = defineEmits(["update:modelValue", "success"]);

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

const passwordFormRef = ref(null);

const passwordForm = reactive({
  oldPassword: "",
  newPassword: "",
  confirmPassword: "",
});

const passwordRules = {
  oldPassword: [{ required: true, message: "请输入当前密码", trigger: "blur" }],
  newPassword: [
    { required: true, message: "请输入新密码", trigger: "blur" },
    { min: 8, message: "密码长度不能小于8位", trigger: "blur" },
  ],
  confirmPassword: [
    { required: true, message: "请再次输入新密码", trigger: "blur" },
    {
      validator: (rule, value, callback) => {
        if (value !== passwordForm.newPassword) {
          callback(new Error("两次输入的密码不一致"));
        } else {
          callback();
        }
      },
      trigger: "blur",
    },
  ],
};

const handleClose = () => {
  dialogVisible.value = false;
  passwordForm.oldPassword = "";
  passwordForm.newPassword = "";
  passwordForm.confirmPassword = "";
};

const handleSubmit = async () => {
  if (!passwordFormRef.value) return;
  await passwordFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        await changePassword({
          currentPassword: passwordForm.oldPassword,
          newPassword: passwordForm.newPassword,
        });
        ElMessage.success("密码修改成功");
        emit("success");
        handleClose();
      } catch (error) {
        ElMessage.error(error?.message || "密码修改失败");
      }
    }
  });
};
</script>

<style lang="scss" scoped>
:deep(.el-dialog__body) {
  padding-top: 20px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
