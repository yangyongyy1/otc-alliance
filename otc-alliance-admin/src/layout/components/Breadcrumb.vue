<template>
  <div class="breadcrumb-container">
    <el-breadcrumb separator="/" class="modern-breadcrumb">
      <el-breadcrumb-item v-for="(item, index) in breadcrumbList" :key="index">
        <div class="breadcrumb-item">
          <span 
            v-if="index === breadcrumbList.length - 1" 
            class="breadcrumb-current"
          >
            {{ item.meta.titleKey ? t(item.meta.titleKey) : item.meta.title }}
          </span>
          <a 
            v-else 
            @click="handleClick(item)" 
            class="breadcrumb-link"
          >
            {{ item.meta.titleKey ? t(item.meta.titleKey) : item.meta.title }}
          </a>
        </div>
      </el-breadcrumb-item>
    </el-breadcrumb>
  </div>
</template>

<script setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { usePermissionStore } from "@/store/modules/permission";

const { t } = useI18n();

const route = useRoute();
const router = useRouter();
const permissionStore = usePermissionStore();

const breadcrumbList = computed(() => {
  // 直接使用 route.matched 构建面包屑
  const matched = route.matched.filter((item) => item.meta && (item.meta.title || item.meta.displayName));

  if (matched.length > 1) { // 确保有多个层级
    const breadcrumb = matched.map(item => ({
      path: item.path,
      meta: {
        title: item.meta.displayName || item.meta.title,
        titleKey: item.meta.titleKey
      }
    }));
    return breadcrumb;
  }

  // 如果 route.matched 层级不够，使用动态构建
  return buildDynamicBreadcrumb(route.path);
});

// 动态构建面包屑
const buildDynamicBreadcrumb = (path) => {
  // 处理详情页路径
  if (path.includes('_detail')) {
    // 移除ID参数，只保留基础路径
    const basePath = path.replace(/\/[^/]+$/, '').replace('_detail', '');
    const baseBreadcrumb = buildPathBreadcrumb(basePath);
    if (baseBreadcrumb.length > 0) {
      // 添加基础路径的面包屑
      const breadcrumbItems = [...baseBreadcrumb];
      
      // 添加详情页（不显示ID）
      const lastItem = baseBreadcrumb[baseBreadcrumb.length - 1];
      const detailTitle = lastItem.meta.titleKey 
        ? `${t(lastItem.meta.titleKey)}${t('common.detail')}`
        : `${lastItem.meta.title}${t('common.detail')}`;
      breadcrumbItems.push({
        path: path,
        meta: {
          title: detailTitle
        }
      });
      return breadcrumbItems;
    }
  }
  
  // 构建普通路径的面包屑
  return buildPathBreadcrumb(path);
};

// 根据路径构建面包屑
const buildPathBreadcrumb = (path) => {
  const breadcrumbItems = [];

  // 分割路径，过滤掉ID参数（UUID格式的字符串）
  const pathSegments = path.split('/').filter(segment => {
    if (!segment || segment === 'dashboard') return false;
    // 过滤掉UUID格式的ID参数
    const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    return !uuidRegex.test(segment);
  });

  // 构建路径层级，逐级构建
  let currentPath = '';
  for (let i = 0; i < pathSegments.length; i++) {
    currentPath += `/${pathSegments[i]}`;
    
    // 从路由配置中查找匹配的路由
    const matchedRoute = findRouteByPath(currentPath);
    if (matchedRoute && matchedRoute.meta) {
      // 优先使用 displayName，然后是 title
      const title = matchedRoute.meta.displayName || matchedRoute.meta.title || pathSegments[i];
      breadcrumbItems.push({
        path: currentPath,
        meta: {
          title: title,
          titleKey: matchedRoute.meta.titleKey
        }
      });
    } else {
      // 使用格式化标题
      const title = formatSegmentTitle(pathSegments[i]);
      breadcrumbItems.push({
        path: currentPath,
        meta: {
          title: title
        }
      });
    }
  }

  return breadcrumbItems;
};


// 根据路径查找路由
const findRouteByPath = (path) => {
  // 从当前路由的matched中查找
  const matchedRoute = route.matched.find(item => item.path === path);
  if (matchedRoute) {
    return matchedRoute;
  }
  
  // 从路由实例中查找
  const routes = router.getRoutes();
  
  // 尝试原始路径
  let foundRoute = routes.find(route => route.path === path);
  if (foundRoute) {
    return foundRoute;
  }
  
  // 尝试小写路径（因为动态路由生成时转换为小写）
  const lowerPath = path.toLowerCase();
  foundRoute = routes.find(route => route.path === lowerPath);
  if (foundRoute) {
    return foundRoute;
  }
  
  // 尝试查找子路由（去掉开头的斜杠）
  const pathWithoutSlash = path.startsWith('/') ? path.slice(1) : path;
  const childRoute = routes.find(route => route.path === pathWithoutSlash);
  if (childRoute) {
    return childRoute;
  }
  
  return null;
};


// 格式化路径段标题
const formatSegmentTitle = (segment) => {
  // 移除下划线和特殊字符，转换为中文
  const titleMap = {
    'authentication': t('breadcrumb.authentication'),
    'business': t('breadcrumb.business'),
    'acquiring': t('breadcrumb.acquiring'),
    'standard': t('breadcrumb.standard'),
    'businessuser': t('breadcrumb.businessuser'),
    'personal': t('breadcrumb.personal'),
    'company': t('breadcrumb.company'),
    'systemconfiguration': t('breadcrumb.systemconfiguration'),
    'rolemanagement': t('breadcrumb.rolemanagement'),
    'backstagerole': t('breadcrumb.backstagerole'),
    'merchantbackstagerole': t('breadcrumb.merchantbackstagerole'),
    'tradingmerchantrole': t('breadcrumb.tradingmerchantrole'),
    'menumanagement': t('breadcrumb.menumanagement'),
    'merchantbackstagemenu': t('breadcrumb.merchantbackstagemenu'),
    'tradingmerchantmenu': t('breadcrumb.tradingmerchantmenu'),
    'accountsetting': t('breadcrumb.accountsetting'),
    'administratoraccount': t('breadcrumb.administratoraccount'),
    'groupnotification': t('breadcrumb.groupnotification'),
    'ipwhitelistconfiguration': t('breadcrumb.ipwhitelistconfiguration'),
    'security': t('common.securityCenter'),
    'user': t('breadcrumb.user'),
    'vabusiness': t('breadcrumb.vabusiness'),
    'customer': t('breadcrumb.customer'),
  };
  
  return titleMap[segment] || segment.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
};

const handleClick = (item) => {
  if (item.path && item.path !== route.path) {
    router.push(item.path);
  }
};
</script>

<style lang="scss" scoped>
.breadcrumb-container {
  display: flex;
  align-items: center;
  height: 100%;
}

.modern-breadcrumb {
  display: flex;
  align-items: center;
  font-size: 14px;
  font-weight: 500;
  
  :deep(.el-breadcrumb__item) {
    display: flex;
    align-items: center;
    
    .el-breadcrumb__separator {
      margin: 0 8px;
      color: #c0c4cc;
      font-weight: 400;
    }
  }
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  padding: 4px 8px;
  border-radius: 6px;
  transition: all 0.3s ease;
  
  &:hover {
    background: rgba(102, 126, 234, 0.05);
  }
}

.breadcrumb-link {
  color: #6c757d;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 2px 4px;
  border-radius: 4px;
  
  &:hover {
    color: #667eea;
    background: rgba(102, 126, 234, 0.1);
    transform: translateY(-1px);
  }
}

.breadcrumb-current {
  color: #667eea;
  font-weight: 600;
  background: rgba(102, 126, 234, 0.1);
  padding: 4px 8px;
  border-radius: 6px;
  border: 1px solid rgba(102, 126, 234, 0.2);
}
</style>
