import CommonTable from "./CommonTable/index.vue";
import CommonForm from "./CommonForm/index.vue";
import EnumSelect from "./EnumSelect.vue";
import BusinessSelect from "./BusinessSelect.vue";
import AgentSelect from "./AgentSelect.vue";
import CoinSelect from "./CoinSelect.vue";

// 组件列表
const components = [
  {
    name: "CommonTable",
    component: CommonTable,
  },
  {
    name: "CommonForm",
    component: CommonForm,
  },
  {
    name: "EnumSelect",
    component: EnumSelect,
  },
  {
    name: "BusinessSelect",
    component: BusinessSelect,
  },
  {
    name: "AgentSelect",
    component: AgentSelect,
  },
  {
    name: "CoinSelect",
    component: CoinSelect,
  },
];

// 注册全局组件
export function registerComponents(app) {
  components.forEach((item) => {
    app.component(item.name, item.component);
  });
}
