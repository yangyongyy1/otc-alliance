import { createI18n } from "vue-i18n";
import zhCN from "./lang/zh-CN";
import enUS from "./lang/en-US";

const messages = {
  "zh-CN": zhCN,
  "en-US": enUS,
};

// 获取浏览器语言设置
const getBrowserLanguage = () => {
  const language = navigator.language || navigator.userLanguage;
  const lang = language.toLowerCase();
  if (lang.includes("zh")) {
    return "zh-CN";
  }
  return "en-US";
};

// 获取本地存储的语言设置
const getStoredLanguage = () => {
  return localStorage.getItem("language") || "en-US";
};

const i18n = createI18n({
  legacy: false, // 使用 Composition API
  locale: getStoredLanguage(),
  fallbackLocale: "en-US",
  messages,
});

// 切换语言
export const setLanguage = (lang) => {
  i18n.global.locale.value = lang;
  localStorage.setItem("language", lang);
  document.querySelector("html").setAttribute("lang", lang);
};

export default i18n;
