import axios from "axios";
import { ElMessage } from "element-plus";
import router from "@/router";
import { useUserStore } from "@/store/modules/user";
import {
  processRequestData,
  processResponseData,
  isTimeField,
  formatDateTime,
} from "./timezone";

// 语言代码映射
const languageMap = {
  "zh-CN": "zh-Hans",
  "en-US": "en-US",
};

// 获取环境变量中的 baseURL
const baseURL = import.meta.env.VITE_API_BASE_URL;

// 创建 axios 实例
const service = axios.create({
  baseURL,
  timeout: 55000,
  headers: {
    "Content-Type": "application/json",
  },
});

// 请求拦截器
service.interceptors.request.use(
  (config) => {
    const userStore = useUserStore();
    const token = userStore.token;
    const clientId = localStorage.getItem("client_Id");

    // 如果有 token 就带上
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
    if (clientId) {
      config.headers["Client-ID"] = clientId;
    }

    // 从 localStorage 获取当前语言并设置 Accept-Language 头
    const currentLanguage = localStorage.getItem("language") || "zh-CN";
    config.headers["Accept-Language"] =
      languageMap[currentLanguage] || currentLanguage;

    // 获取时区偏移量
    const timeZoneValue = localStorage.getItem("userTimezone");
    const timezoneOffset = timeZoneValue ? timeZoneValue : 0;

    // 处理请求数据中的时间字段
    if (config.data) {
      config.data = processRequestData(config.data, timezoneOffset);
    }

    // 处理 URL 参数中的时间字段
    if (config.params) {
      config.params = processRequestData(config.params, timezoneOffset);
    }

    // 处理 URL 中的时间参数
    if (config.url) {
      const url = new URL(config.url, config.baseURL);
      const searchParams = new URLSearchParams(url.search);

      for (const [key, value] of searchParams.entries()) {
        if (isTimeField(key)) {
          const date = new Date(value);
          if (!isNaN(date.getTime())) {
            const offsetMs = timezoneOffset * 60 * 60 * 1000;
            const localDate = new Date(date.getTime() + offsetMs);
            searchParams.set(key, formatDateTime(localDate));
          }
        }
      }

      url.search = searchParams.toString();
      config.url = url.pathname + url.search;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 响应拦截器
service.interceptors.response.use(
  (response) => {
    const res = response.data;

    // 处理响应数据中的时间字段
    if (res.result) {
      const timeZoneValue2 = localStorage.getItem("userTimezone");
      var timezoneOffset2 = timeZoneValue2 ? timeZoneValue2 : 0;
      if (timezoneOffset2 != 0) {
        timezoneOffset2 = timezoneOffset2 * -1;
      }
      res.result = processResponseData(res.result, timezoneOffset2);
    }

    // 正常响应，直接返回数据
    return res;
  },
  (error) => {
    // 401：不弹错误信息，直接清除登录态并跳转登录页
    if (error.response && error.response.status === 401) {
      const userStore = useUserStore();
      userStore.logout();
      if (router && router.currentRoute.value.path !== "/login") {
        const currentPath = router.currentRoute.value.fullPath;
        router.push(`/login?redirect=${encodeURIComponent(currentPath)}`).catch(() => {
          window.location.href = `/login?redirect=${encodeURIComponent(currentPath)}`;
        });
      }
      return Promise.reject(error);
    }

    // 非 401 才弹错误信息
    ElMessage({
      message: error.response?.data?.error?.message || error.response?.data?.message || "系统错误",
      type: "error",
      duration: 5 * 1000,
    });
    return Promise.reject(error.response?.data || error);
  }
);

export default service;
