<template>
  <div class="security-center">
    <el-card class="security-card">
      <template #header>
        <div class="card-header">
          <h2>{{ t('common.securityCenter') }}</h2>
          <p class="subtitle">{{ t('security.manageAccountSecurity') }}</p>
        </div>
      </template>

      <!-- 手机号绑定 -->
      <div class="security-item">
        <div class="item-header">
          <div class="left">
            <el-icon class="icon"><Phone /></el-icon>
            <div class="info">
              <h3>{{ t('security.phoneBinding') }}</h3>
              <p class="desc">{{ t('security.phoneBindingDesc') }}</p>
            </div>
          </div>
          <div class="right">
            <template v-if="isBindPhone">
              <span class="phone-number">{{
                maskPhone(userSecuritySetting.phoneNumber)
              }}</span>
              <el-button
                type="primary"
                link
                @click="showPhoneDialog = true"
              >
                {{ t('security.modify') }}
              </el-button>
              <el-button
                type="primary"
                v-if="isPhoneEnabled"
                link
                @click="showPhoneDialog(1)"
              >
                {{ t('security.disable') }}
              </el-button>
              <el-button type="primary" v-else link @click="showPhoneDialog(2)">
                {{ t('security.enable') }}
              </el-button>
            </template>
            <template v-else>
              <el-button type="primary" @click="showPhoneDialog(3)" :disabled="true">
                {{ t('security.bindNow') }}
              </el-button>
            </template>
          </div>
        </div>
      </div>

      <!-- 谷歌验证器 -->
      <div class="security-item">
        <div class="item-header">
          <div class="left">
            <el-icon class="icon"><Lock /></el-icon>
            <div class="info">
              <h3>{{ t('security.googleAuthenticator') }}</h3>
              <p class="desc">{{ t('security.googleAuthenticatorDesc') }}</p>
            </div>
          </div>
          <div class="right">
            <template v-if="isBindGoogle">
              <el-button
                type="primary"
                v-if="isGoogleEnabled"
                link
                @click="showPhoneDialogFunc(1)"
              >
                {{ t('security.disable') }}
              </el-button>
              <el-button
                type="primary"
                v-else
                link
                @click="showPhoneDialogFunc(2)"
              >
                {{ t('security.enable') }}
              </el-button>
            </template>
            <el-button v-else type="primary" @click="showPhoneDialogFunc(3)" :disabled="true">
              {{ t('security.bindNow') }}
            </el-button>
          </div>
        </div>
      </div>

      <!-- 登录密码 -->
      <div class="security-item">
        <div class="item-header">
          <div class="left">
            <el-icon class="icon"><Key /></el-icon>
            <div class="info">
              <h3>{{ t('security.loginPassword') }}</h3>
              <p class="desc">{{ t('security.loginPasswordDesc') }}</p>
            </div>
          </div>
          <div class="right">
            <el-button type="primary" link @click="showPasswordDialog = true">
              {{ t('security.modifyPassword') }}
            </el-button>
          </div>
        </div>
      </div>
    </el-card>

    <!-- 弹窗组件 -->
    <PhoneBindDialog
      v-model="showPhoneDialog"
      :user-security-setting="userSecuritySetting"
      @success="handleSecurityUpdate"
    />
    <GoogleAuthDialog
      v-model="showGoogleDialog"
      :user-security-setting="userSecuritySetting"
      @success="handleSecurityUpdate"
    />
    <PasswordDialog
      v-model="showPasswordDialog"
      :user-security-setting="userSecuritySetting"
      @success="handleSecurityUpdate"
    />

    <PhoneDialog
      v-model="showPhoneBindDialog"
      :user-security-setting="userSecuritySetting"
      @success="handleSecurityUpdate"
    />
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { Phone, Lock, Key } from "@element-plus/icons-vue";
import PhoneBindDialog from "@/components/security/PhoneBindDialog.vue";
import GoogleAuthDialog from "@/components/security/GoogleAuthDialog.vue";
import PasswordDialog from "@/components/security/PasswordDialog.vue";
import PhoneDialog from "@/components/security/PhoneDialog.vue";
import request from "@/utils/request";
import { ElMessage } from "element-plus";

const { t } = useI18n();

// 弹窗显示状态
const showPhoneDialog = ref(false);
const showGoogleDialog = ref(false);
const showPasswordDialog = ref(false);
const showPhoneBindDialog = ref(false);

// 绑定状态
const isBindPhone = ref(false);
const isPhoneEnabled = ref(false);
const isBindGoogle = ref(false);
const isGoogleEnabled = ref(false);

// 用户安全设置
const userSecuritySetting = ref({});

// 处理安全设置更新
const handleSecurityUpdate = () => {
  // 安全设置更新后的处理逻辑
};

const showPhoneDialogFunc = (type) => {
  showPhoneDialog.value = type;
};

// 切换手机号启用状态
const handleTogglePhoneStatus = async (enable) => {
  try {
    // TODO: 调用切换手机号状态API
    await request({
      url: "/api/security/phone/toggle",
      method: "post",
      data: { enable },
    });
    ElMessage.success(enable ? t('security.phoneEnabled') : t('security.phoneDisabled'));
  } catch (error) {
    ElMessage.error(t('security.operationFailed'));
  }
};

// 切换谷歌验证器启用状态
const handleToggleGoogleStatus = async (enable) => {
  try {
    // TODO: 调用切换谷歌验证器状态API
    await request({
      url: "/api/security/google/toggle",
      method: "post",
      data: { enable },
    });
    ElMessage.success(enable ? t('security.googleEnabled') : t('security.googleDisabled'));
  } catch (error) {
    ElMessage.error(t('security.operationFailed'));
  }
};

// 工具函数
const maskPhone = (phone) => {
  return phone.replace(/(\d{3})\d{4}(\d{4})/, "$1****$2");
};
</script>

<style lang="scss" scoped>
.security-center {
  padding: 20px;
  min-height: calc(100vh);
  background-color: #f5f7fa;

  .security-card {
    max-width: 1200px;
    margin: 0 auto;
    border-radius: 8px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);

    .card-header {
      h2 {
        margin: 0;
        font-size: 20px;
        color: #303133;
      }

      .subtitle {
        margin: 8px 0 0;
        font-size: 14px;
        color: #909399;
      }
    }
  }

  .security-item {
    padding: 20px 0;
    border-bottom: 1px solid #ebeef5;

    &:last-child {
      border-bottom: none;
    }

    .item-header {
      display: flex;
      justify-content: space-between;
      align-items: center;

      .left {
        display: flex;
        align-items: center;
        gap: 16px;

        .icon {
          font-size: 24px;
          color: #409eff;
        }

        .info {
          h3 {
            margin: 0;
            font-size: 16px;
            color: #303133;
          }

          .desc {
            margin: 4px 0 0;
            font-size: 14px;
            color: #909399;
          }
        }
      }

      .right {
        display: flex;
        align-items: center;
        gap: 16px;

        .phone-number {
          color: #606266;
        }
      }
    }
  }
}
</style>
