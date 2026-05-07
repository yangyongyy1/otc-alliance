// Convert path to route path format
const formatPath = (name) => {
  return name
    .split(".")
    .slice(1)
    .map((segment) => segment.charAt(0).toLowerCase() + segment.slice(1))
    .join("/");
};

// 将菜单数据转换为树形结构
export const transformMenuToTree = (permissions) => {
  if (!Array.isArray(permissions)) {
    return [];
  }

  // 递归查找子节点
  const findChildren = (parentName, allItems) => {
    const children = allItems
      .filter(
        (item) => !item.name.includes(".Btn") && item.parentName === parentName
      )
      .map((item) => ({
        key: `/${formatPath(item.name)}`,
        path: formatPath(item.name),
        meta: {
          title: item.displayName || item.name,
          icon: "Menu",
        },
        children: findChildren(item.name, allItems),
      }))
      .filter((item) => item !== null);

    return children;
  };

  // 找出顶层节点（parentName 为 null 的节点）
  const rootNodes = permissions
    .filter((item) => !item.name.includes(".Btn") && !item.parentName)
    .map((item) => ({
      key: `/${formatPath(item.name)}`,
      path: formatPath(item.name),
      meta: {
        title: item.displayName || item.name,
        icon: "Menu",
      },
      children: findChildren(item.name, permissions),
    }));

  // 清理空的 children 数组
  const cleanupTree = (nodes) => {
    return nodes.map((node) => {
      const cleanNode = { ...node };
      if (cleanNode.children && cleanNode.children.length > 0) {
        cleanNode.children = cleanupTree(cleanNode.children);
      } else {
        delete cleanNode.children;
      }
      return cleanNode;
    });
  };

  const result = cleanupTree(rootNodes);
  
  // 添加仪表盘菜单项
  // 动态获取仪表盘标题，需要在使用时传入 i18n 或使用 key
  // 这里使用一个特殊的 key，在组件中处理国际化
  result.unshift({
    key: '/dashboard',
    path: 'dashboard',
    meta: {
      title: 'dashboard', // 使用 key，在组件中通过 i18n 转换
      titleKey: 'common.dashboard', // 添加 i18n key
      icon: 'Menu',
    },
  });
  
  return result;
};

// Generate routes from menu tree
export const generateRoutes = (menuTree) => {
  // 生成嵌套路由
  const generateNestedRoutes = (tree, parentPath = "") => {
    return tree.map((node) => {
      // 构建完整的路径
      const fullPath = parentPath ? `${parentPath}/${node.path}` : node.path;

      const route = {
        path: fullPath,
        name: node.key,
        meta: node.meta,
        component: () =>
          import(`@/views/${fullPath}/index.vue`).catch(() =>
            import("@/layout/BlankLayout.vue")
          ),
      };

      if (node.children && node.children.length > 0) {
        route.children = generateNestedRoutes(node.children, fullPath);
      }

      return route;
    });
  };

  // 创建根路由，包含 Layout
  const routes = [
    {
      path: "/",
      component: () => import("@/layout/index.vue"),
      redirect: "/dashboard",
      children: [
        {
          path: "dashboard",
          name: "Dashboard",
          component: () => import("@/views/dashboard/index.vue"),
          meta: { title: "dashboard", titleKey: "common.dashboard", icon: "Menu" },
        },
        ...generateNestedRoutes(menuTree),
      ],
    },
    {
      path: "/login",
      name: "Login",
      component: () => import("@/views/login/index.vue"),
      meta: { title: "登录" },
      hidden: true,
    },
    {
      path: "/:pathMatch(.*)*",
      name: "NotFound",
      component: () => import("@/views/error/404.vue"),
      hidden: true,
    },
  ];

  return routes;
};
