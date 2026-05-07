import { defineStore } from "pinia";
import { getPermissions } from "@/api/user";
import { transformMenuToTree } from "@/utils/menuUtils";
import router from "@/router";
import { loadView } from "@/utils/loadViewUtils";
import { useUserStore } from "@/store/modules/user";
import i18n from "@/i18n/index";

// 根据路径获取对应的图标
const getIconForPath = (path) => {
  const iconMap = {
    'dashboard': 'Odometer',
    'authentication': 'User',
    'business': 'Shop',
    'businessuser': 'UserFilled',
    'systemconfiguration': 'Setting',
    'rolemanagement': 'UserFilled',
    'menumanagement': 'Menu',
    'accountsetting': 'Avatar',
    'groupnotification': 'Bell',
    'ipwhitelistconfiguration': 'Lock',
    'security': 'Lock',
    'user': 'User',
    'vabusiness': 'Shop',
    'customer': 'UserFilled',
    'ordermanagement': 'Document',
  };
  
  const pathParts = path.split('/');
  const mainPath = pathParts[0];
  return iconMap[mainPath] || 'Menu';
};

// 布局路由
const layoutRoute = {
  path: "/",
  name: "Layout",
  component: () => import("@/layout/index.vue"),
  redirect: "/dashboard",
  children: [
    {
      path: "dashboard",
      name: "Dashboard",
      component: () => import("@/views/dashboard/index.vue"),
      meta: {
        title: i18n.global.t('common.dashboard'),
        icon: "dashboard",
        affix: true, // 设置为固定标签，不能关闭
      },
    },
  ],
};

// 将权限数据转换为路由配置
const generateRoutesFromPermissions = (permissions) => {
  // 过滤掉按钮权限
  const menuPermissions = permissions.filter(
    (item) => !item.name.includes(".Btn")
  );
  //是按钮权限
  const btnPermissions = permissions.filter((item) =>
    item.name.includes(".Btn")
  );

  //只存放name
  const btnPermissionsName = btnPermissions.map((item) => item.name);

  //放入user.js的btnPermissions
  useUserStore().btnPermissions = btnPermissionsName;

  // 递归生成路由配置
  const generateRoute = (item) => {
    // 去掉 Pages 前缀，并将点号替换为斜杠
    const path = item.name
      .replace(/^Pages\./, "")
      .replace(/\./g, "/")
      .toLowerCase();

    // 创建主路由
    const route = {
      path,
      name: path.replace(/\//g, "_"), // 使用下划线分隔的路径作为唯一名称
      component: loadView(path),
      meta: {
        title: item.displayName,
        displayName: item.displayName, // 确保 displayName 被设置
        icon: getIconForPath(path),
        breadcrumb: true, // 标记为可显示在面包屑中
      },
    };

    // 创建详情页路由 - 保持原有目录结构，添加id参数
    const detailPath = path + "_detail";
    const detailTitle = `${item.displayName}${i18n.global.t('common.detail')}`;
    const detailRoute = {
      path: `${path}_detail/:id`,
      name: `${path.replace(/\//g, "_")}_detail`,
      component: loadView(detailPath),
      meta: {
        title: detailTitle,
        displayName: detailTitle, // 确保 displayName 被设置
        icon: getIconForPath(path),
        breadcrumb: true,
      },
      props: true, // 将路由参数作为props传递给组件
    };


    // 如果有子路由，递归生成
    if (item.children && item.children.length > 0) {
      route.children = item.children.map((child) => generateRoute(child));
    }

    // 返回主路由和详情路由
    return [route, detailRoute];
  };

  // 生成所有路由并展平数组
  const routes = menuPermissions.map((item) => generateRoute(item)).flat();
  return routes;
};

export const usePermissionStore = defineStore("permission", {
  state: () => ({
    permissions: [],
    menuTree: [],
  }),

  actions: {
    // 重置权限状态
    resetPermissions() {
      this.permissions = [];
      this.menuTree = [];
    },

    // 获取权限并生成路由
    async generateRoutes() {
      try {
        // 获取权限数据
        const res = await getPermissions();
        if (!res || !res.result || !res.result.items) {
          throw new Error("Invalid API response structure");
        }

        // 使用 result.items 作为权限数据
        const permissionData = res.result.items;

        // 添加安全中心到权限数据
        permissionData.push({
          name: "Pages.Security",
          displayName: i18n.global.t('common.securityCenter'),
          icon: "security",
          parentName: null,
        });

        this.permissions = permissionData;

        // 生成菜单树
        this.menuTree = transformMenuToTree(permissionData);

        // 直接从权限数据生成路由
        const accessRoutes = generateRoutesFromPermissions(permissionData);

        // 先移除之前添加的路由
        router.removeRoute("Layout");

        // 将生成的路由添加到布局路由的children中
        const newLayoutRoute = {
          ...layoutRoute,
          children: [
            ...layoutRoute.children.filter(
              (child) => child.path === "dashboard"
            ),
            {
              path: "redirect/:path(.*)",
              name: "Redirect",
              component: () => import("@/views/redirect/index.vue"),
              meta: { title: "重定向", hidden: true },
            },
            ...accessRoutes,
            {
              path: "security",
              name: "Security",
              component: () => import("@/views/security.vue"),
              meta: {
                title: i18n.global.t('common.securityCenter'),
                icon: "security",
              },
            },
          ],
        };

        // 添加布局路由
        router.addRoute(newLayoutRoute);

        // 添加404路由
        router.addRoute({
          path: "/:pathMatch(.*)*",
          name: "NotFound",
          component: () => import("@/views/error/404.vue"),
          meta: { hidden: true },
        });

        return true;
      } catch (error) {
        // 确保在错误时清理状态
        this.resetPermissions();
        throw error; // 重新抛出错误，让路由守卫处理
      }
    },
  },
});
