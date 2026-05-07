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
      v-for="option in enumOptions"
      :key="option.value"
      :label="option.displayName"
      :value="option.value"
      :disabled="option.disabled"
    />
  </el-select>
</template>

<script setup>
import { ref, watch, onMounted, computed } from "vue";
import { ElMessage } from "element-plus";
import { getEnum } from "@/api/enum";

const props = defineProps({
  // 枚举名称，必传
  enumName: {
    type: String,
    required: true,
  },
  // 双向绑定的值
  modelValue: {
    type: [String, Number, Array],
    default: "",
  },
  // 占位符
  placeholder: {
    type: String,
    default: "请选择",
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
  // 自定义选项映射函数
  optionMapper: {
    type: Function,
    default: null,
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
const enumOptions = ref([]);
const loading = ref(false);

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

// 获取枚举数据
const fetchEnumData = async () => {
  if (!props.enumName) {
    return;
  }

  if (props.showLoading) {
    loading.value = true;
  }

  try {
    const response = await getEnum(props.enumName);
    
    // 处理响应数据，假设后端返回的数据结构为 { result: [...] }
    let options = response.result || response.data || response;
    
    // 如果数据是数组，直接使用；如果是对象，转换为数组
    if (!Array.isArray(options)) {
      options = Object.keys(options).map(key => ({
        value: key,
        displayName: options[key]
      }));
    }
    
    // 如果有自定义映射函数，使用它来转换数据
    if (props.optionMapper && typeof props.optionMapper === 'function') {
      options = options.map(props.optionMapper);
    } else {
      // 默认映射：确保每个选项都有 value 和 displayName
      options = options.map(option => {
        if (typeof option === 'string') {
          return { value: option, displayName: option };
        }
        if (typeof option === 'object') {
          return {
            // 使用 !== undefined 而不是 || 来避免 0、false 等 falsy 值被跳过
            value: option.value !== undefined ? option.value : (option.key !== undefined ? option.key : option.id),
            displayName: option.displayName || option.label || option.name || option.text,
            disabled: option.disabled || false,
          };
        }
        return option;
      });
    }
    
    enumOptions.value = options;
    emit("loaded", options);
  } catch (error) {
    ElMessage.error(`获取${props.enumName}枚举数据失败`);
    enumOptions.value = [];
  } finally {
    loading.value = false;
  }
};

// 处理选择变化
const handleChange = (value) => {
  emit("change", value);
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
  fetchEnumData();
};

// 暴露方法给父组件
defineExpose({
  refresh,
  enumOptions: computed(() => enumOptions.value),
});

// 组件挂载时自动加载数据
onMounted(() => {
  if (props.autoLoad) {
    fetchEnumData();
  }
});
</script>

<style lang="scss" scoped>
// 可以添加自定义样式
</style>
