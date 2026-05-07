import { createRouter, createWebHistory } from "vue-router";
import NProgress from "nprogress";
import "nprogress/nprogress.css";
import { usePermissionStore } from "@/store/modules/permission";
import { useUserStore } from "@/store/modules/user";

// 静态路由
export const constantRoutes = [
  {
    path: "/",
    component: () => import("@/layout/index.vue"),
    redirect: "/dashboard",
    children: [
      {
        path: "dashboard",
        name: "Dashboard",
        component: () => import("@/views/dashboard/index.vue"),
        meta: { title: "dashboard", titleKey: "common.dashboard", icon: "Menu", affix: true },
      },
      {
        path: "redirect/:path(.*)",
        name: "Redirect",
        component: () => import("@/views/redirect/index.vue"),
        meta: { title: "重定向", hidden: true },
      },
      {
        path: "alliancemanagement/merchantSubCode/:merchantId",
        name: "MerchantSubCode",
        component: () => import("@/views/alliancemanagement/merchantSubCode.vue"),
        meta: { title: "MerchantSubCode", hidden: true },
        props: true,
      },
    ],
  },
  {
    path: "/login",
    name: "Login",
    component: () => import("@/views/login/index.vue"),
    meta: { title: "登录" },
    hidden: true,
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes: constantRoutes,
});

NProgress.configure({
  showSpinner: false,
  minimum: 0.2,
  easing: "ease",
  speed: 500,
});

const whiteList = ["/login"];

router.beforeEach(async (to, from, next) => {
  NProgress.start();

  document.title = to.meta.title
    ? `${to.meta.title} - V-Account Alliance`
    : "V-Account Alliance";

  const userStore = useUserStore();
  const permissionStore = usePermissionStore();

  // 如果目标路由是登录页
  if (to.path === "/login") {
    // 如果有有效的token，重定向到首页
    if (userStore.token) {
      try {
        if (!userStore.userInfo) {
          await userStore.getUserInfo();
        }
        // token有效，重定向到首页或redirect参数指定的页面
        const redirect = to.query.redirect || "/";
        next({ path: redirect, replace: true });
        NProgress.done();
        return;
      } catch (error) {
        // token无效，清除状态，允许访问登录页
        userStore.logout();
        next();
        NProgress.done();
        return;
      }
    } else {
      // 没有token，允许访问登录页
      next();
      NProgress.done();
      return;
    }
  }

  // 如果有 token
  if (userStore.token) {
    try {
      // 无论是否有用户信息，都先验证token有效性（通过获取用户信息）
      // 这对于关闭浏览器后重新打开页面的情况很重要
      if (!userStore.userInfo) {
        await userStore.getUserInfo();
      }

      // 如果没有权限数据，生成路由
      if (permissionStore.permissions.length === 0) {
        // 生成路由
        await permissionStore.generateRoutes();
        
        // 路由生成后，使用 replace: true 重新匹配路由
        // 这样可以确保路由系统能够正确匹配到新生成的路由
        next({ ...to, replace: true });
      } else {
        // 已有路由，直接跳转
        next();
      }
    } catch (error) {
      // 检查是否是 401/403 错误（token 过期或无效）
      const isTokenExpired = error.response?.status === 401 || 
                            error.response?.status === 403 ||
                            (error.code === "ERR_NETWORK" && error.response?.status === 401) ||
                            error.code === "NO_USER_ID" ||
                            (error.message && (
                              error.message.includes("401") || 
                              error.message.includes("403") ||
                              error.message.includes("token") ||
                              error.message.includes("Token")
                            ));
      
      // 清除所有状态
      userStore.logout();
      permissionStore.resetPermissions();
      
      // token过期或无效，跳转到登录页
      if (isTokenExpired) {
        next({ path: `/login?redirect=${encodeURIComponent(to.fullPath)}`, replace: true });
      } else {
        // 其他错误（网络错误等），也跳转到登录页
        next({ path: `/login?redirect=${encodeURIComponent(to.fullPath)}`, replace: true });
      }
      NProgress.done();
    }
  } else {
    // 没有token，检查是否在白名单中
    if (whiteList.includes(to.path)) {
      next();
      NProgress.done();
    } else {
      // 不在白名单，跳转到登录页
      next({ path: `/login?redirect=${encodeURIComponent(to.fullPath)}`, replace: true });
      NProgress.done();
    }
  }
});

router.afterEach(() => {
  NProgress.done();
});

export default router;
