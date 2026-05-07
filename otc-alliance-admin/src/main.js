import { createApp } from "vue";
import { createPinia } from "pinia";
import ElementPlus from "element-plus";
import "element-plus/dist/index.css";
import * as ElementPlusIconsVue from "@element-plus/icons-vue";
import App from "./App.vue";
import router from "./router";
import i18n from "@/i18n/index";
import "./styles/index.scss";
import { registerComponents } from "@/components";
import zhCn from "element-plus/es/locale/lang/zh-cn";
import en from "element-plus/es/locale/lang/en";
import { permission } from "./directives/permission";

const app = createApp(App);
const pinia = createPinia();

// 注册所有图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component);
}

// 注册全局组件
registerComponents(app);

// 注册权限指令
app.directive("permission", permission);

// 根据当前语言设置 Element Plus 的 locale
const getElementPlusLocale = () => {
  const currentLang = localStorage.getItem("language") || i18n.global.locale.value || "zh-CN";
  return currentLang === "zh-CN" ? zhCn : en;
};

app.use(pinia);
app.use(router);
app.use(i18n);
app.use(ElementPlus, {
  locale: getElementPlusLocale(),
});

app.mount("#app");
