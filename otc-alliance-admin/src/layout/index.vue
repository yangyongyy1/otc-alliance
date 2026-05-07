<template>
  <div class="app-wrapper">
    <!-- 侧边栏 -->
    <div class="sidebar-container" :class="{ 'is-collapse': isCollapse }">
      <div class="logo-container">
        <el-icon class="logo"><Monitor /></el-icon>
        <h1 class="title" v-if="!isCollapse">Admin</h1>
      </div>
      <side-menu
        :menu-data="permissionStore.permissions"
        :is-collapse="isCollapse"
      />
    </div>

    <!-- 主要内容区 -->
    <div class="main-container">
      <!-- 顶部导航栏 -->
      <div class="navbar">
        <div class="left-area">
          <el-icon class="fold-btn" @click="toggleSidebar">
            <component :is="isCollapse ? 'Expand' : 'Fold'" />
          </el-icon>
          <breadcrumb />
        </div>
        <div class="right-area">
          <el-dropdown
            trigger="click"
            @command="handleLanguageChange"
            class="language-dropdown"
          >
            <span class="language-switch-text">
              <el-icon><Setting /></el-icon>
              {{ currentLang }}
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="zh-CN">{{ t("common.chinese") }}</el-dropdown-item>
                <el-dropdown-item command="en-US">{{ t("common.english") }}</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown
            trigger="click"
            @command="handleTimezoneChange"
            class="timezone-dropdown"
          >
            <span class="timezone-switch-text">
              <el-icon><Timer /></el-icon>
              {{ currentTimezone }}
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item
                  v-for="tz in timezones"
                  :key="tz.value"
                  :command="tz.value"
                >
                  {{ tz.label }}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>

          <el-dropdown trigger="click" @command="handleCommand">
            <div class="user-dropdown">
              <el-avatar
                :size="32"
                :src="
                  userStore.avatar ||
                  'https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png'
                "
              />
              <span class="username">{{ userStore.userInfo.userName }}</span>
              <el-icon><CaretBottom /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="profile">
                  <el-icon><User /></el-icon>{{ t("common.profile") }}
                </el-dropdown-item>
                <el-dropdown-item divided command="logout">
                  <el-icon><SwitchButton /></el-icon>{{ t("common.logout") }}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>

      <!-- 标签页 -->
      <tabs-view />

      <!-- 内容区 -->
      <div class="app-main">
        <router-view v-slot="{ Component }">
          <transition name="fade-transform" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>

      <!-- 页脚 -->
      <div class="app-footer">
        <div class="footer-content">
          <span class="copyright">
            © {{ new Date().getFullYear() }} Klickl. {{ t("common.allRightsReserved") || "All rights reserved" }}
          </span>
          <span class="version">v1.0.0</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import { usePermissionStore } from "@/store/modules/permission";
import { useUserStore } from "@/store/modules/user";
import { setUserTimezone } from "@/api/user";
import request from "@/utils/request";
import {
  Monitor,
  CaretBottom,
  User,
  SwitchButton,
  Setting,
  Timer,
  Expand,
  Fold,
} from "@element-plus/icons-vue";
import SideMenu from "@/components/SideMenu.vue";
import Breadcrumb from "./components/Breadcrumb.vue";
import TabsView from "./components/TabsView.vue";
import { setLanguage } from "@/i18n/index";

const router = useRouter();
const { t, locale } = useI18n();
const isCollapse = ref(false);
const permissionStore = usePermissionStore();
const userStore = useUserStore();

const currentLang = computed(() => {
  return locale.value === "zh-CN" ? t("common.chinese") : t("common.english");
});

// 切换侧边栏
const toggleSidebar = () => {
  isCollapse.value = !isCollapse.value;
};

// 处理个人信息
const handleProfile = () => {
  router.push("/profile");
};

// 处理退出登录
const handleLogout = async () => {
  await userStore.logout();
  router.push("/login");
};

// 处理语言切换
const handleLanguageChange = (lang) => {
  setLanguage(lang);
  locale.value = lang;
  window.location.reload();
};

// 时区列表
const timezones = [
  {
    label: "(UTC+08:00) 深圳，北京，香港，台北,吉隆坡，新加坡",
    value: -8,
  },
  {
    label: "(UTC+04:00) 阿布扎比",
    value: -4,
  },
  {
    label: "(UTC+09:00) 东京，首尔",
    value: -9,
  },
  {
    label: "(UTC-05:00) 纽约",
    value: 5,
  },
  {
    label: "(UTC+00:00) 伦敦",
    value: 0,
  },
  {
    label: "(UTC+01:00) 柏林",
    value: -1,
  },
  {
    label: "(UTC-08:00) 洛杉矶，旧金山，温哥华，西雅图，拉斯维加斯",
    value: 8,
  },
];

const getTimezoneLabel = (value) => {
  const tz = timezones.find((t) => t.value === value);
  return tz ? tz.label.split(" ")[0] : "UTC+08:00";
};

// 当前时区
const userStoreTimezone = userStore.userInfo.timeZoneValue || 8;
const currentTimezone = ref(getTimezoneLabel(userStoreTimezone));

// 获取时区显示文本

// 处理时区切换
const handleTimezoneChange = async (timezone) => {
  currentTimezone.value = getTimezoneLabel(Number(timezone));
  localStorage.setItem("userTimezone", timezone);
  await setUserTimezone(timezone);
};

// 处理下拉菜单命令
const handleCommand = (command) => {
  switch (command) {
    case "profile":
      handleProfile();
      break;
    case "logout":
      handleLogout();
      break;
  }
};
</script>

<style lang="scss" scoped>
.app-wrapper {
  display: flex;
  height: 100vh;
  width: 100vw;
  overflow: hidden; // 防止全局出现滚动条
  position: relative;
}

.sidebar-container {
  background: linear-gradient(180deg, #2c3e50 0%, #34495e 100%);
  backdrop-filter: blur(10px);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  width: 280px;
  height: 100%;
  overflow: hidden;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.1);
  border-right: 1px solid rgba(255, 255, 255, 0.1);

  &.is-collapse {
    width: 70px;
  }

  .logo-container {
    height: 70px;
    display: flex;
    align-items: center;
    padding: 0 20px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    position: relative;
    overflow: hidden;

    &::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(45deg, rgba(255, 255, 255, 0.1) 0%, transparent 100%);
      pointer-events: none;
    }

    .logo {
      font-size: 36px;
      color: #fff;
      z-index: 1;
      filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
    }

    .title {
      color: #fff;
      margin-left: 16px;
      font-size: 20px;
      font-weight: 700;
      white-space: nowrap;
      overflow: hidden;
      z-index: 1;
      text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
      letter-spacing: 0.5px;
    }
  }
}

.main-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0; // 关键，防止主区域内容撑破导致水平滚动
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  height: 100vh;
  overflow: hidden;
  position: absolute;
  top: 0;
  left: 280px;
  right: 0;
  bottom: 0;
  border-radius: 16px 0 0 0;
  transition: left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  
  // 侧边栏折叠时的位置调整
  .sidebar-container.is-collapse ~ & {
    left: 70px;
  }
}

.navbar {
  height: 70px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  position: relative;
  z-index: 10;

  .left-area {
    display: flex;
    align-items: center;

    .fold-btn {
      font-size: 24px;
      cursor: pointer;
      margin-right: 20px;
      padding: 10px;
      border-radius: 8px;
      transition: all 0.3s ease;
      color: #667eea;
      display: flex;
      align-items: center;
      justify-content: center;
      min-width: 40px;
      min-height: 40px;
      background: rgba(102, 126, 234, 0.05);
      border: 1px solid rgba(102, 126, 234, 0.1);

      &:hover {
        color: #764ba2;
        background: rgba(102, 126, 234, 0.15);
        border-color: rgba(102, 126, 234, 0.2);
        transform: scale(1.05);
        box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
      }
    }
  }

  .right-area {
    display: flex;
    align-items: center;
    gap: 20px;

    .language-dropdown {
      .language-switch-text {
        cursor: pointer;
        color: #667eea;
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 14px;
        font-weight: 500;
        padding: 8px 12px;
        border-radius: 8px;
        transition: all 0.3s ease;

        &:hover {
          color: #764ba2;
          background: rgba(102, 126, 234, 0.1);
          transform: translateY(-1px);
        }
      }
    }

    .timezone-dropdown {
      .timezone-switch-text {
        cursor: pointer;
        color: #667eea;
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 14px;
        font-weight: 500;
        padding: 8px 12px;
        border-radius: 8px;
        transition: all 0.3s ease;

        &:hover {
          color: #764ba2;
          background: rgba(102, 126, 234, 0.1);
          transform: translateY(-1px);
        }
      }
    }

    .user-dropdown {
      display: flex;
      align-items: center;
      cursor: pointer;
      padding: 8px 16px;
      height: 48px;
      border-radius: 12px;
      transition: all 0.3s ease;
      background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
      border: 1px solid rgba(102, 126, 234, 0.2);

      &:hover {
        background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%);
        transform: translateY(-2px);
        box-shadow: 0 8px 25px rgba(102, 126, 234, 0.2);
      }

      .username {
        margin: 0 12px;
        font-size: 14px;
        color: #667eea;
        font-weight: 600;
      }

      .el-icon {
        color: #764ba2;
        font-size: 14px;
        transition: transform 0.3s ease;
      }
      
      &:hover .el-icon {
        transform: rotate(180deg);
      }
    }
  }
}

.app-main {
  flex: 1;
  padding: 24px;
  overflow: auto; // 保证只有主内容区域滚动
  box-sizing: border-box;
  background: transparent;
  margin: 0;
  min-height: calc(100vh - 174px); // 减去导航栏高度(70px)、标签页高度(44px)和页脚高度(60px)
  position: relative;
  z-index: 1;
}

.app-footer {
  height: 60px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-top: 1px solid rgba(0, 0, 0, 0.06);
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 24px;
  position: relative;
  z-index: 10;
  flex-shrink: 0;

  .footer-content {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    width: 100%;
    font-size: 13px;
    color: #666;

    .copyright {
      color: #666;
      font-weight: 400;
      letter-spacing: 0.3px;
    }

    .version {
      color: #999;
      font-size: 12px;
      font-weight: 300;
      padding: 4px 12px;
      background: rgba(102, 126, 234, 0.05);
      border-radius: 12px;
      border: 1px solid rgba(102, 126, 234, 0.1);
    }
  }
}

// 路由切换动画
.fade-transform-enter-active,
.fade-transform-leave-active {
  transition: all 0.3s;
}

.fade-transform-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

.dropdown-profile-img {
  justify-content: center;
  background: transparent !important;
  cursor: default !important;
  &:hover {
    background: transparent !important;
  }
  .profile-img {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    display: block;
    margin: 0 auto;
  }
}
</style>
