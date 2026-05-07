<template>
  <el-dialog
    v-model="dialogVisible"
    :title="title"
    width="400px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    :show-close="false"
    destroy-on-close
    class="verify-dialog"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="0"
      class="verify-form"
    >
      <!-- 手机验证码 -->
      <template v-if="type === 'phone'">
        <el-form-item prop="smsCode">
          <div class="input-group">
            <el-input
              v-model="form.smsCode"
              :placeholder="$t('login.smsCode')"
              clearable
              @keyup.enter="handleSubmit"
            >
              <template #append>
                <el-button
                  class="send-code-btn"
                  :class="{ 'is-disabled': countdown > 0 }"
                  :disabled="countdown > 0"
                  @click="handleSendCode"
                >
                  {{ countdown > 0 ? `${countdown}s` : $t("login.sendCode") }}
                </el-button>
              </template>
            </el-input>
          </div>
        </el-form-item>
      </template>

      <!-- Google验证码 -->
      <template v-if="type === 'google'">
        <el-form-item prop="googleCode">
          <div class="input-group">
            <el-input
              v-model="form.googleCode"
              :placeholder="$t('login.googleCode')"
              clearable
              @keyup.enter="handleSubmit"
            />
          </div>
        </el-form-item>
      </template>

      <!-- 资金密码 -->
      <template v-if="type === 'fund'">
        <el-form-item prop="fundPassword">
          <div class="input-group">
            <el-input
              v-model="form.fundPassword"
              type="password"
              :placeholder="$t('login.fundPassword')"
              clearable
              show-password
              @keyup.enter="handleSubmit"
            />
          </div>
        </el-form-item>
      </template>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button class="cancel-btn" @click="handleCancel">{{
          $t("common.cancel")
        }}</el-button>
        <el-button
          class="confirm-btn"
          type="primary"
          :loading="loading"
          @click="handleSubmit"
        >
          {{ $t("common.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import request from "@/utils/request";

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
  type: {
    type: String,
    required: true,
    validator: (value) => {
      return ["phone", "google", "fund"].includes(value);
    },
  },
  clientKey: {
    type: String,
    required: true,
  },
});

const emit = defineEmits(["update:modelValue", "success", "cancel"]);

const { t } = useI18n();
const loading = ref(false);
const countdown = ref(0);
const formRef = ref(null);
const form = ref({
  smsCode: "",
  googleCode: "",
  fundPassword: "",
});

// 标题
const title = computed(() => {
  const titles = {
    phone: t("login.phoneVerify"),
    google: t("login.googleVerify"),
    fund: t("login.fundVerify"),
  };
  return titles[props.type] || t("login.verify");
});

// 验证规则
const rules = computed(() => ({
  smsCode: [
    { required: true, message: t("login.smsCodeRequired"), trigger: "blur" },
    { len: 6, message: t("login.smsCodeLength"), trigger: "blur" },
  ],
  googleCode: [
    { required: true, message: t("login.googleCodeRequired"), trigger: "blur" },
    { len: 6, message: t("login.googleCodeLength"), trigger: "blur" },
  ],
  fundPassword: [
    {
      required: true,
      message: t("login.fundPasswordRequired"),
      trigger: "blur",
    },
    { min: 6, message: t("login.fundPasswordLength"), trigger: "blur" },
  ],
}));

// 对话框显示状态
const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => {
    emit("update:modelValue", value);
    if (!value) {
      // 重置表单
      form.value = {
        smsCode: "",
        googleCode: "",
        fundPassword: "",
      };
      countdown.value = 0;
    }
  },
});

// 发送验证码
const handleSendCode = async () => {
  if (countdown.value > 0) return;

  try {
    loading.value = true;
    const response = await request.post("/api/TokenAuth/SendSmsCode", {
      clientKey: props.clientKey,
    });

    if (response.success) {
      ElMessage.success(t("login.codeSent"));
      countdown.value = 60;
      startCountdown();
    }
  } catch (error) {
    ElMessage.error(t("login.sendCodeFailed"));
  } finally {
    loading.value = false;
  }
};

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    loading.value = true;

    // 根据验证类型准备数据
    const verifyData = {};
    if (props.type === "phone") {
      verifyData.smsCode = form.value.smsCode;
    } else if (props.type === "google") {
      verifyData.googleCode = form.value.googleCode;
    } else if (props.type === "fund") {
      verifyData.fundPassword = form.value.fundPassword;
    }

    // 直接发送验证数据给父组件
    emit("success", verifyData);
    dialogVisible.value = false;
  } catch (error) {
    ElMessage.error(t("login.verifyFailed"));
  } finally {
    loading.value = false;
  }
};

// 取消验证
const handleCancel = () => {
  dialogVisible.value = false;
  emit("cancel");
};
</script>

<style lang="scss" scoped>
.verify-dialog {
  :deep(.el-dialog) {
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 12px 32px rgba(0, 0, 0, 0.1);

    .el-dialog__header {
      margin: 0;
      padding: 16px 20px;
      background: #fff;
      border-bottom: 1px solid #f0f0f0;

      .el-dialog__title {
        color: #333;
        font-size: 16px;
        font-weight: 600;
      }
    }

    .el-dialog__body {
      padding: 24px 20px;
    }

    .el-dialog__footer {
      padding: 16px 20px;
      border-top: 1px solid #f0f0f0;
      background: #fafafa;
    }
  }

  .verify-form {
    .el-form-item {
      margin-bottom: 20px;

      &:last-child {
        margin-bottom: 0;
      }
    }

    .input-group {
      :deep(.el-input) {
        .el-input__wrapper {
          box-shadow: none !important;
          border: 1px solid #dcdfe6;
          border-radius: 4px;
          height: 40px;
          padding: 0 12px;
          transition: all 0.2s;

          &:hover {
            border-color: #c0c4cc;
          }

          &.is-focus {
            border-color: #409eff;
          }

          .el-input__inner {
            height: 38px;
            line-height: 38px;
            color: #333;
            font-size: 14px;

            &::placeholder {
              color: #909399;
            }
          }
        }

        .el-input-group__append {
          padding: 0;
          border: none;
          background: none;
        }
      }
    }

    .send-code-btn {
      margin: 0;
      padding: 0 15px;
      height: 38px;
      line-height: 38px;
      font-size: 14px;
      color: #409eff;
      border: 1px solid #409eff;
      border-radius: 4px;
      background: #fff;
      transition: all 0.2s;

      &:hover:not(.is-disabled) {
        color: #fff;
        background: #409eff;
      }

      &.is-disabled {
        color: #c0c4cc;
        border-color: #e4e7ed;
        background: #f5f7fa;
        cursor: not-allowed;
      }
    }
  }

  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 8px;

    .el-button {
      min-width: 88px;
      height: 36px;
      font-size: 14px;
      font-weight: 500;
      border-radius: 4px;
      padding: 0 16px;

      &.cancel-btn {
        color: #606266;
        border: 1px solid #dcdfe6;
        background: #fff;

        &:hover {
          color: #409eff;
          border-color: #c6e2ff;
          background: #ecf5ff;
        }
      }

      &.confirm-btn {
        color: #fff;
        border: none;
        background: #409eff;

        &:hover {
          background: #66b1ff;
        }

        &:active {
          background: #3a8ee6;
        }

        &.is-loading {
          background: #409eff;
          opacity: 0.8;
        }
      }
    }
  }
}
</style>
