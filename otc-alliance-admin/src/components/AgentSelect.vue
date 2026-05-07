<template>
  <el-select
    v-model="selectedValue"
    :placeholder="placeholder"
    :clearable="clearable"
    :filterable="filterable"
    :disabled="disabled"
    :loading="loading"
    :multiple="multiple"
    :collapse-tags="collapseTags"
    :collapse-tags-tooltip="collapseTagsTooltip"
    :size="size"
    :style="{ width: width }"
    @change="handleChange"
    @clear="handleClear"
    @focus="handleFocus"
    @blur="handleBlur"
  >
    <el-option
      v-for="item in agentOptions"
      :key="item.id"
      :label="item.agentName"
      :value="item[valueKey]"
      :disabled="item.disabled"
    />
  </el-select>
</template>

<script setup>
import { ref, watch, computed } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { getAgentSelectItems } from "@/api/commonApi";

const props = defineProps({
  // 双向绑定的值
  modelValue: {
    type: [String, Number, Array],
    default: "",
  },
  // 返回值的字段名（id、code等）
  valueKey: {
    type: String,
    default: "id",
  },
  // 占位符
  placeholder: {
    type: String,
    default: "请选择代理",
  },
  // 是否可清空
  clearable: {
    type: Boolean,
    default: true,
  },
  // 是否可搜索
  filterable: {
    type: Boolean,
    default: true,
  },
  // 是否禁用
  disabled: {
    type: Boolean,
    default: false,
  },
  // 是否多选
  multiple: {
    type: Boolean,
    default: false,
  },
  // 多选时是否折叠标签
  collapseTags: {
    type: Boolean,
    default: false,
  },
  // 多选时折叠标签是否显示提示
  collapseTagsTooltip: {
    type: Boolean,
    default: false,
  },
  // 尺寸
  size: {
    type: String,
    default: "default",
    validator: (value) => ["large", "default", "small"].includes(value),
  },
  // 宽度
  width: {
    type: String,
    default: "180px",
  },
  // 是否在组件挂载时自动加载数据
  autoLoad: {
    type: Boolean,
    default: true,
  },
  // 是否显示加载状态
  showLoading: {
    type: Boolean,
    default: true,
  },
});

const emit = defineEmits([
  "update:modelValue",
  "change",
  "clear",
  "focus",
  "blur",
  "loaded",
]);

const selectedValue = ref(props.modelValue);
const agentOptions = ref([]);
const loading = ref(false);

// 获取代理列表数据
const fetchAgentOptions = async () => {
  if (props.showLoading) {
    loading.value = true;
  }

  try {
    const apiConfig = getAgentSelectItems();
    const response = await request(apiConfig);
    
    // 处理响应数据
    let options = response.result || response.data || response || [];
    
    // 确保是数组
    if (!Array.isArray(options)) {
      options = [];
    }
    
    agentOptions.value = options;
    emit("loaded", options);
  } catch (error) {
    ElMessage.error("获取代理列表失败");
    agentOptions.value = [];
  } finally {
    loading.value = false;
  }
};

// 监听 modelValue 变化
watch(
  () => props.modelValue,
  (val) => {
    selectedValue.value = val;
  },
  { immediate: true }
);

// 监听 selectedValue 变化
watch(selectedValue, (val) => {
  emit("update:modelValue", val);
});

// 处理选择变化
const handleChange = (value) => {
  // 获取选中的代理对象
  const selectedAgent = agentOptions.value.find(
    (item) => item[props.valueKey] === value
  );
  emit("change", value, selectedAgent);
};

// 处理清空
const handleClear = () => {
  emit("clear");
};

// 处理获得焦点
const handleFocus = (event) => {
  emit("focus", event);
};

// 处理失去焦点
const handleBlur = (event) => {
  emit("blur", event);
};

// 手动刷新数据
const refresh = () => {
  fetchAgentOptions();
};

// 暴露方法和数据给父组件
defineExpose({
  refresh,
  agentOptions: computed(() => agentOptions.value),
});

// 组件挂载时自动加载数据
if (props.autoLoad) {
  fetchAgentOptions();
}
</script>

<style lang="scss" scoped>
// 可以添加自定义样式
</style>

