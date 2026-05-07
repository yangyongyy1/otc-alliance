<template>
  <el-dialog
    v-model="dialogVisible"
    :title="isBound ? '解绑谷歌验证器' : '绑定谷歌验证器'"
    width="500px"
    @close="handleClose"
  >
    <template v-if="!isBound">
      <div class="google-auth-steps">
        <div class="step">
          <h4>1. 下载谷歌验证器</h4>
          <p>在手机应用商店搜索并下载"Google Authenticator"</p>
        </div>
        <div class="step">
          <h4>2. 扫描二维码</h4>
          <div class="qrcode-container">
            <img :src="googleAuthQRCode" alt="Google Auth QR Code" />
          </div>
        </div>
        <div class="step">
          <h4>3. 输入验证码</h4>
          <el-form
            ref="googleFormRef"
            :model="googleForm"
            :rules="googleRules"
            label-width="80px"
          >
            <el-form-item label="验证码" prop="code">
              <el-input
                v-model="googleForm.code"
                placeholder="请输入谷歌验证器中的6位验证码"
              />
            </el-form-item>
          </el-form>
        </div>
      </div>
    </template>
    <template v-else>
      <div class="unbind-confirm">
        <el-alert
          title="解绑谷歌验证器后，您的账号将失去双重认证保护"
          type="warning"
          :closable="false"
          show-icon
        />
        <el-form
          ref="unbindFormRef"
          :model="unbindForm"
          :rules="unbindRules"
          label-width="80px"
          class="mt-4"
        >
          <el-form-item label="验证码" prop="code">
            <el-input
              v-model="unbindForm.code"
              placeholder="请输入谷歌验证器中的6位验证码"
            />
          </el-form-item>
        </el-form>
      </div>
    </template>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit">
          {{ isBound ? "确认解绑" : "确认绑定" }}
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { ElMessage } from "element-plus";

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

const isBound = computed(() => props.userSecuritySetting.googleAuthEnabled);

const googleFormRef = ref(null);
const unbindFormRef = ref(null);
const googleAuthQRCode = ref("");

const googleForm = reactive({
  code: "",
});

const unbindForm = reactive({
  code: "",
});

const googleRules = {
  code: [
    { required: true, message: "请输入验证码", trigger: "blur" },
    { pattern: /^\d{6}$/, message: "验证码为6位数字", trigger: "blur" },
  ],
};

const unbindRules = {
  code: [
    { required: true, message: "请输入验证码", trigger: "blur" },
    { pattern: /^\d{6}$/, message: "验证码为6位数字", trigger: "blur" },
  ],
};

const handleClose = () => {
  dialogVisible.value = false;
  googleForm.code = "";
  unbindForm.code = "";
};

const handleSubmit = async () => {
  const formRef = isBound.value ? unbindFormRef : googleFormRef;
  if (!formRef.value) return;

  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        // TODO: 调用绑定/解绑API
        ElMessage.success(
          isBound.value ? "谷歌验证器解绑成功" : "谷歌验证器绑定成功"
        );
        emit("success");
        handleClose();
      } catch (error) {
        ElMessage.error(isBound.value ? "解绑失败" : "绑定失败");
      }
    }
  });
};

// 获取二维码
const getQRCode = async () => {
  if (!isBound.value) {
    try {
      // TODO: 调用获取二维码API
      googleAuthQRCode.value = "https://example.com/qrcode.png";
    } catch (error) {
      ElMessage.error("获取二维码失败");
    }
  }
};

// 监听对话框显示状态
watch(
  () => dialogVisible.value,
  (val) => {
    if (val && !isBound.value) {
      getQRCode();
    }
  }
);
</script>

<style lang="scss" scoped>
.google-auth-steps {
  .step {
    margin-bottom: 24px;

    h4 {
      margin: 0 0 8px;
      font-size: 16px;
      color: #303133;
    }

    p {
      margin: 0;
      color: #606266;
    }
  }

  .qrcode-container {
    margin: 16px 0;
    text-align: center;

    img {
      width: 200px;
      height: 200px;
    }
  }
}

.unbind-confirm {
  .mt-4 {
    margin-top: 16px;
  }
}

:deep(.el-dialog__body) {
  padding-top: 20px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
