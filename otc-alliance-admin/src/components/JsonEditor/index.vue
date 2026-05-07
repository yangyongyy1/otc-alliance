<template>
  <div class="json-editor-container">
    <div class="json-editor-toolbar">
      <el-button size="small" @click="formatJson" :disabled="!isValidJson">
        格式化
      </el-button>
      <el-button size="small" @click="compressJson" :disabled="!isValidJson">
        压缩
      </el-button>
      <el-button size="small" @click="clearJson">清空</el-button>
      <span v-if="!isValidJson && jsonValue" class="error-text">
        JSON 格式错误
      </span>
    </div>
    <el-input
      v-model="jsonValue"
      type="textarea"
      :rows="rows"
      :placeholder="placeholder"
      :class="{ 'json-error': !isValidJson && jsonValue }"
      @input="handleInput"
      @blur="handleBlur"
      size="large"
    />
    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from "vue";

const props = defineProps({
  modelValue: {
    type: String,
    default: "",
  },
  rows: {
    type: Number,
    default: 10,
  },
  placeholder: {
    type: String,
    default: "请输入 JSON 格式的数据",
  },
});

const emit = defineEmits(["update:modelValue", "change"]);

const jsonValue = ref(props.modelValue || "");
const errorMessage = ref("");

// 监听外部值变化
watch(
  () => props.modelValue,
  (val) => {
    if (val !== jsonValue.value) {
      jsonValue.value = val || "";
      errorMessage.value = "";
    }
  }
);

// 验证 JSON 格式
const isValidJson = computed(() => {
  if (!jsonValue.value || jsonValue.value.trim() === "") {
    return true;
  }
  try {
    JSON.parse(jsonValue.value);
    return true;
  } catch {
    return false;
  }
});

// 处理输入
const handleInput = (value) => {
  jsonValue.value = value;
  errorMessage.value = "";
  emit("update:modelValue", value);
  emit("change", value);
};

// 处理失焦
const handleBlur = () => {
  if (jsonValue.value && jsonValue.value.trim() !== "") {
    try {
      JSON.parse(jsonValue.value);
      errorMessage.value = "";
    } catch (e) {
      errorMessage.value = e.message || "JSON 格式错误";
    }
  }
};

// 格式化 JSON
const formatJson = () => {
  try {
    const parsed = JSON.parse(jsonValue.value);
    jsonValue.value = JSON.stringify(parsed, null, 2);
    errorMessage.value = "";
    emit("update:modelValue", jsonValue.value);
    emit("change", jsonValue.value);
  } catch (e) {
    errorMessage.value = e.message || "无法格式化，JSON 格式错误";
  }
};

// 压缩 JSON
const compressJson = () => {
  try {
    const parsed = JSON.parse(jsonValue.value);
    jsonValue.value = JSON.stringify(parsed);
    errorMessage.value = "";
    emit("update:modelValue", jsonValue.value);
    emit("change", jsonValue.value);
  } catch (e) {
    errorMessage.value = e.message || "无法压缩，JSON 格式错误";
  }
};

// 清空
const clearJson = () => {
  jsonValue.value = "";
  errorMessage.value = "";
  emit("update:modelValue", "");
  emit("change", "");
};
</script>

<style scoped lang="scss">
.json-editor-container {
  width: 100%;
}

.json-editor-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  padding: 8px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.error-text {
  color: #f56c6c;
  font-size: 12px;
  margin-left: auto;
}

.json-error :deep(.el-textarea__inner) {
  border-color: #f56c6c;
}

.error-message {
  color: #f56c6c;
  font-size: 12px;
  margin-top: 4px;
  padding-left: 4px;
}

:deep(.el-textarea__inner) {
  font-family: "Courier New", Courier, monospace;
  font-size: 14px;
  line-height: 1.5;
}
</style>

