<template>
  <div class="login-container" v-loading="loading">
    <!-- 背景装饰元素 -->
    <div class="bg-decoration">
      <div class="floating-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
        <div class="shape shape-4"></div>
        <div class="shape shape-5"></div>
      </div>
    </div>

    <!-- 登录表单区域 -->
    <div class="login-section">
      <div class="login-form">
        <div class="form-header">
          <h2 class="welcome-title">{{ $t("login.welcome") }}</h2>
          <p class="welcome-subtitle">{{ $t("login.systemName") }}</p>
        </div>

        <div class="language-switch">
          <el-dropdown @command="handleLanguageChange" trigger="click">
            <div class="language-btn">
              <el-icon><Setting /></el-icon>
              <span>{{ currentLanguage === "zh-CN" ? $t("common.chinese") : $t("common.english") }}</span>
              <el-icon class="arrow"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="zh-CN">
                  <el-icon><CircleCheck /></el-icon>
                  {{ $t("common.chinese") }}
                </el-dropdown-item>
                <el-dropdown-item command="en-US">
                  <el-icon><CircleCheck /></el-icon>
                  {{ $t("common.english") }}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>

        <el-form ref="loginFormRef" :model="loginForm" :rules="rules" class="login-form-content">
          <div class="form-group">
            <label class="form-label">
              <el-icon><User /></el-icon>
              {{ $t('login.usernamePlaceholder') }}
            </label>
            <el-form-item prop="username">
              <el-input
                v-model="loginForm.username"
                :placeholder="$t('login.usernamePlaceholder')"
                clearable
                class="modern-input"
                size="large"
              />
            </el-form-item>
          </div>

          <div class="form-group">
            <label class="form-label">
              <el-icon><Lock /></el-icon>
              {{ $t('login.passwordPlaceholder') }}
            </label>
            <el-form-item prop="password">
              <el-input
                v-model="loginForm.password"
                :type="passwordType"
                :placeholder="$t('login.passwordPlaceholder')"
                clearable
                class="modern-input"
                size="large"
                @keyup.enter="handleLogin"
              >
                <template #suffix>
                  <el-icon class="show-password" @click="togglePasswordType">
                    <component
                      :is="passwordType === 'password' ? 'Hide' : 'View'"
                    />
                  </el-icon>
                </template>
              </el-input>
            </el-form-item>
          </div>

          <div class="form-group">
            <label class="form-label">
              <el-icon><Check /></el-icon>
              {{ $t("common.securityVerification") }}
            </label>
            <el-form-item prop="captcha">
              <div id="captcha" class="captcha-container">
                <div id="yidun_captcha"></div>
              </div>
            </el-form-item>
          </div>

          <el-button
            type="primary"
            class="login-btn"
            :loading="loading"
            :disabled="!isCaptchaVerified"
            @click="handleLogin"
            size="large"
          >
            <el-icon v-if="!loading"><Right /></el-icon>
            {{ loading ? ($t("login.login") + '...') : $t("login.login") }}
          </el-button>
        </el-form>

        <div class="form-footer">
          <div class="remember-me">
            <el-checkbox>{{ $t("login.rememberMe") }}</el-checkbox>
          </div>
          <div class="forgot-password">
            <el-button link class="forgot-btn">{{ $t("login.forgetPassword") }}</el-button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import { User, Lock, View, Hide, Setting, Monitor, Check, Star, ArrowDown, CircleCheck, Right } from "@element-plus/icons-vue";
import { setLanguage } from "@/i18n/index";
import service from "@/utils/request";
import { usePermissionStore } from "@/store/modules/permission";
import { useUserStore } from "@/store/modules/user";
import { getUserInfo } from "@/api/user";

const router = useRouter();
const route = useRoute();
const { t, locale } = useI18n();
const loading = ref(false);
const passwordType = ref("password");
const loginFormRef = ref(null);
const isCaptchaVerified = ref(false);
const loginForm = ref({
  username: "",
  password: "",
  captcha: "",
});

// 登录函数
const login = async (formData) => {
  const payload = {
    usernameOrEmailAddress: formData.username,
    password: formData.password,
    userType: 4,
    imageCode: "",
    smsCode: "",
    NECaptchaValidate: formData.captcha,
  };
  return service.post("/api/TokenAuth/Authenticate", payload);
};

// 当前语言
const currentLanguage = computed(() => locale.value);

// 获取重定向地址
const redirect = computed(() => route.query.redirect || "/");

// 验证规则
const rules = computed(() => ({
  username: [
    {
      required: true,
      message: t("login.usernamePlaceholder"),
      trigger: "blur",
    },
  ],
  password: [
    {
      required: true,
      message: t("login.passwordPlaceholder"),
      trigger: "blur",
    },
    { min: 6, message: t("login.passwordLength"), trigger: "blur" },
  ],
}));

// 切换密码显示类型
const togglePasswordType = () => {
  passwordType.value = passwordType.value === "password" ? "text" : "password";
};

// 语言切换
const handleLanguageChange = async (lang) => {
  try {
    await setLanguage(lang);
    locale.value = lang;
    ElMessage.success(t("common.languageChanged"));
  } catch (error) {
    ElMessage.error(t("common.languageChangeFailed"));
  }
};

// 验证码实例
let captchaIns = null;

// 初始化验证码
const initCaptcha = () => {
  if (window.initNECaptcha) {
    window.initNECaptcha(
      {
        element: "#yidun_captcha",
        captchaId: "0265a7cec9684c75b5ed6d8dfd7b0c51",
        width: "100%",
        mode: "float",
        onVerify: (err, data) => {
          if (err) {
            isCaptchaVerified.value = false;
            loginForm.value.captcha = "";
            return;
          }
          loginForm.value.captcha = data.validate;
          isCaptchaVerified.value = true;
        },
        onClose: () => {
          isCaptchaVerified.value = false;
          loginForm.value.captcha = "";
        },
        onError: () => {
          isCaptchaVerified.value = false;
          loginForm.value.captcha = "";
        },
      },
      (instance) => {
        captchaIns = instance;
      },
      (err) => {
        ElMessage.error("验证码初始化失败");
      }
    );
  } else {
    setTimeout(initCaptcha, 1000);
  }
};

// 登录处理
const handleLogin = async () => {
  // 验证验证码是否已完成
  if (!isCaptchaVerified.value || !loginForm.value.captcha) {
    ElMessage.warning("请先完成安全验证");
    return;
  }

  try {
    loading.value = true;
    const res = await login(loginForm.value);

    if (res.success) {
      // 登录成功
      const userStore = useUserStore();
      userStore.token = res.result.accessToken;
      localStorage.setItem("token", res.result.accessToken);

      const userInfo = await getUserInfo(res.result.userId);
      userStore.userInfo = userInfo.result;
      localStorage.setItem("userInfo", JSON.stringify(userInfo.result));

      const permissionStore = usePermissionStore();
      await permissionStore.generateRoutes();
      await router.replace(redirect.value);
    } else {
      //ElMessage.error(res.error?.message || "登录失败");
    }
  } catch (error) {
    ElMessage.error(error.error.details);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  setTimeout(initCaptcha, 500);
});
</script>

<style lang="scss" scoped>
.login-container {
  min-height: 100vh;
  background: #f5f7fa;
  display: flex;
  position: relative;
  overflow: hidden;
  transition: background 0.3s ease;
  
  // 页面加载完成后的背景
  body.loaded & {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  }

  // 背景装饰
  .bg-decoration {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    pointer-events: none;
    z-index: 1;

    .floating-shapes {
      position: relative;
      width: 100%;
      height: 100%;

      .shape {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.1);
        animation: float 6s ease-in-out infinite;

        &.shape-1 {
          width: 80px;
          height: 80px;
          top: 20%;
          left: 10%;
          animation-delay: 0s;
        }

        &.shape-2 {
          width: 120px;
          height: 120px;
          top: 60%;
          left: 5%;
          animation-delay: 2s;
        }

        &.shape-3 {
          width: 60px;
          height: 60px;
          top: 30%;
          right: 15%;
          animation-delay: 4s;
        }

        &.shape-4 {
          width: 100px;
          height: 100px;
          top: 70%;
          right: 10%;
          animation-delay: 1s;
        }

        &.shape-5 {
          width: 40px;
          height: 40px;
          top: 10%;
          right: 30%;
          animation-delay: 3s;
        }
      }
    }
  }

  // 登录区域
  .login-section {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 40px;
    position: relative;
    z-index: 2;

    .login-form {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(20px);
      border-radius: 24px;
      padding: 48px 40px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      width: 100%;
      max-width: 520px;
      position: relative;
      transition: all 0.3s ease;

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 24px 80px rgba(0, 0, 0, 0.15);
      }

      .form-header {
        text-align: center;
        margin-bottom: 40px;

        .welcome-title {
          font-size: 32px;
          font-weight: 700;
          color: #1a1a1a;
          margin: 0 0 8px 0;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
        }

        .welcome-subtitle {
          font-size: 16px;
          color: #6c757d;
          margin: 0;
        }
      }

      .language-switch {
        position: absolute;
        top: 24px;
        right: 24px;

        .language-btn {
          display: flex;
          align-items: center;
          gap: 8px;
          padding: 8px 16px;
          background: rgba(102, 126, 234, 0.1);
          border-radius: 8px;
          cursor: pointer;
          transition: all 0.3s ease;
          color: #667eea;
          font-weight: 500;

          &:hover {
            background: rgba(102, 126, 234, 0.2);
            transform: translateY(-1px);
          }

          .arrow {
            font-size: 12px;
            transition: transform 0.3s ease;
          }
        }
      }

      .login-form-content {
        .form-group {
          margin-bottom: 24px;

          .form-label {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
            font-weight: 600;
            color: #495057;
            margin-bottom: 8px;

            .el-icon {
              color: #667eea;
            }
          }

          :deep(.el-form-item) {
            margin-bottom: 0;
          }

          .modern-input {
            :deep(.el-input__wrapper) {
              background: #f8f9fa;
              border: 2px solid transparent;
              border-radius: 12px;
              padding: 12px 16px;
              height: 52px;
              transition: all 0.3s ease;
              box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

              &:hover {
                background: #fff;
                border-color: rgba(102, 126, 234, 0.3);
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
              }

              &.is-focus {
                background: #fff;
                border-color: #667eea;
                box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
              }
            }

            :deep(.el-input__inner) {
              font-size: 16px;
              color: #1a1a1a;
            }
          }

          .show-password {
            cursor: pointer;
            color: #6c757d;
            transition: color 0.3s ease;

            &:hover {
              color: #667eea;
            }
          }

          .captcha-container {
            background: #f8f9fa;
            border-radius: 12px;
            padding: 16px;
            border: 2px solid transparent;
            transition: all 0.3s ease;
            width: 100%;
            min-height: 52px;
            display: flex;
            align-items: center;
            justify-content: center;

            &:hover {
              background: #fff;
              border-color: rgba(102, 126, 234, 0.3);
            }

            #yidun_captcha {
              width: 100%;
              min-height: 52px;
            }
          }
        }

        .login-btn {
          width: 100%;
          height: 52px;
          font-size: 16px;
          font-weight: 600;
          margin-top: 32px;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          border: none;
          border-radius: 12px;
          transition: all 0.3s ease;
          display: flex;
          align-items: center;
          justify-content: center;
          gap: 8px;

          &:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
          }

          &:active {
            transform: translateY(0);
          }
        }
      }

      .form-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 24px;
        padding-top: 24px;
        border-top: 1px solid #e9ecef;

        .remember-me {
          :deep(.el-checkbox__label) {
            color: #6c757d;
            font-size: 14px;
          }
        }

        .forgot-password {
          .forgot-btn {
            color: #667eea;
            font-weight: 500;
            padding: 0;

            &:hover {
              color: #764ba2;
            }
          }
        }
      }
    }
  }
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
  }
}

// 响应式设计
@media screen and (max-width: 768px) {
  .login-container {
    .login-section {
      padding: 20px;

      .login-form {
        padding: 32px 24px;
        border-radius: 16px;
        max-width: 480px;
      }
    }
  }
}

@media screen and (max-width: 480px) {
  .login-container {
    .login-section {
      padding: 16px;

      .login-form {
        padding: 24px 20px;
        border-radius: 12px;
        max-width: 100%;
      }
    }
  }
}
</style>
