<template>
  <div class="common-form-container">
    <el-form
      ref="formRef"
      :model="formData"
      :rules="rules"
      :label-width="labelWidth"
      :label-position="labelPosition"
      :inline="inline"
      :size="size"
      :disabled="disabled"
      :class="formClass"
    >
      <el-row :gutter="gutter">
        <el-col
          v-for="(item, index) in formItems"
          :key="item.prop || index"
          :span="item.span || colSpan"
          :xs="item.xs || 24"
          :sm="item.sm"
          :md="item.md"
          :lg="item.lg"
          :xl="item.xl"
        >
          <el-form-item
            v-if="!item.hidden"
            :label="item.label"
            :prop="item.prop"
            :required="item.required"
            :rules="item.rules"
            :label-width="item.labelWidth"
            :class="item.class"
          >
            <!-- 插槽类型 -->
            <template v-if="item.type === 'slot'">
              <slot
                :name="item.slotName || item.prop"
                :form="formData"
                :item="item"
                :index="index"
              />
            </template>

            <!-- 输入框 -->
            <el-input
              v-else-if="item.type === 'input' || item.type === 'text'"
              v-model="formData[item.prop]"
              :placeholder="item.placeholder || t('components.pleaseEnterLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :maxlength="item.maxlength"
              :show-word-limit="item.showWordLimit"
              :prefix-icon="item.prefixIcon"
              :suffix-icon="item.suffixIcon"
              @change="handleChange(item, $event)"
              @blur="handleBlur(item, $event)"
              @focus="handleFocus(item, $event)"
            />

            <!-- 数字输入框 -->
            <el-input-number
              v-else-if="item.type === 'number'"
              v-model="formData[item.prop]"
              :placeholder="item.placeholder || t('components.pleaseEnterLabel', { label: item.label })"
              :min="item.min"
              :max="item.max"
              :step="item.step || 1"
              :precision="item.precision"
              :disabled="item.disabled"
              :controls="item.controls !== false"
              :controls-position="item.controlsPosition"
              @change="handleChange(item, $event)"
            />

            <!-- 文本域 -->
            <el-input
              v-else-if="item.type === 'textarea'"
              v-model="formData[item.prop]"
              type="textarea"
              :placeholder="item.placeholder || t('components.pleaseEnterLabel', { label: item.label })"
              :rows="item.rows || 3"
              :autosize="item.autosize"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :maxlength="item.maxlength"
              :show-word-limit="item.showWordLimit"
              @change="handleChange(item, $event)"
            />

            <!-- 下拉选择 -->
            <el-select
              v-else-if="item.type === 'select'"
              v-model="formData[item.prop]"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :disabled="item.disabled"
              :multiple="item.multiple"
              :collapse-tags="item.collapseTags"
              :collapse-tags-tooltip="item.collapseTagsTooltip"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
              @clear="handleClear(item)"
            >
              <el-option
                v-for="option in item.options"
                :key="option.value"
                :label="option.label"
                :value="option.value"
                :disabled="option.disabled"
              />
            </el-select>

            <!-- 枚举选择器 -->
            <EnumSelect
              v-else-if="item.type === 'enum'"
              :key="item.key || item.prop"
              v-model="formData[item.prop]"
              :enum-name="item.enumName"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :disabled="item.disabled"
              :multiple="item.multiple"
              :collapse-tags="item.collapseTags"
              :collapse-tags-tooltip="item.collapseTagsTooltip"
              :width="item.width || '100%'"
              :size="item.size || size"
              @change="handleChange(item, $event)"
              @clear="handleClear(item)"
              @loaded="item.loaded && item.loaded($event)"
            />

            <!-- 商户选择器 -->
            <BusinessSelect
              v-else-if="item.type === 'business'"
              v-model="formData[item.prop]"
              :business-type="item.businessType"
              :value-key="item.valueKey || 'id'"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :disabled="item.disabled"
              :multiple="item.multiple"
              :collapse-tags="item.collapseTags"
              :collapse-tags-tooltip="item.collapseTagsTooltip"
              :width="item.width || '100%'"
              :size="item.size || size"
              @change="handleChange(item, $event)"
              @clear="handleClear(item)"
            />

            <!-- 代理选择器 -->
            <AgentSelect
              v-else-if="item.type === 'agent'"
              v-model="formData[item.prop]"
              :value-key="item.valueKey || 'id'"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :disabled="item.disabled"
              :multiple="item.multiple"
              :collapse-tags="item.collapseTags"
              :collapse-tags-tooltip="item.collapseTagsTooltip"
              :width="item.width || '100%'"
              :size="item.size || size"
              @change="handleChange(item, $event)"
              @clear="handleClear(item)"
            />

            <!-- 虚拟币选择器 -->
            <CoinSelect
              v-else-if="item.type === 'coin'"
              v-model="formData[item.prop]"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :disabled="item.disabled"
              :multiple="item.multiple"
              :collapse-tags="item.collapseTags"
              :collapse-tags-tooltip="item.collapseTagsTooltip"
              :width="item.width || '100%'"
              :size="item.size || size"
              :show-chain-select="item.showChainSelect"
              :chain-model-value="formData[item.chainProp]"
              :chain-placeholder="item.chainPlaceholder"
              :chain-clearable="item.chainClearable !== false"
              :chain-filterable="item.chainFilterable !== false"
              :chain-multiple="item.chainMultiple"
              :chain-width="item.chainWidth"
              :chain-size="item.chainSize || size"
              :chain-required="item.chainRequired"
              @update:chain-model-value="formData[item.chainProp] = $event"
              @change="handleChange(item, $event)"
              @chain-change="handleChainChange(item, $event)"
              @clear="handleClear(item)"
            />

            <!-- 日期选择器 -->
            <el-date-picker
              v-else-if="item.type === 'date'"
              v-model="formData[item.prop]"
              :type="item.dateType || 'date'"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 时间选择器 -->
            <el-time-picker
              v-else-if="item.type === 'time'"
              v-model="formData[item.prop]"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 日期时间选择器 -->
            <el-date-picker
              v-else-if="item.type === 'datetime'"
              v-model="formData[item.prop]"
              type="datetime"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 日期范围选择器 -->
            <el-date-picker
              v-else-if="item.type === 'daterange'"
              v-model="formData[item.prop]"
              type="daterange"
              :range-separator="item.rangeSeparator || t('components.to')"
              :start-placeholder="item.startPlaceholder || t('components.startDate')"
              :end-placeholder="item.endPlaceholder || t('components.endDate')"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 时间范围选择器 -->
            <el-time-picker
              v-else-if="item.type === 'timerange'"
              v-model="formData[item.prop]"
              is-range
              :range-separator="item.rangeSeparator || t('components.to')"
              :start-placeholder="item.startPlaceholder || t('components.startTime')"
              :end-placeholder="item.endPlaceholder || t('components.endTime')"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 日期时间范围选择器 -->
            <el-date-picker
              v-else-if="item.type === 'datetimerange'"
              v-model="formData[item.prop]"
              type="datetimerange"
              :range-separator="item.rangeSeparator || t('components.to')"
              :start-placeholder="item.startPlaceholder || t('components.startTime')"
              :end-placeholder="item.endPlaceholder || t('components.endTime')"
              :clearable="item.clearable !== false"
              :disabled="item.disabled"
              :format="item.format"
              :value-format="item.valueFormat"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 开关 -->
            <el-switch
              v-else-if="item.type === 'switch'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              :active-text="item.activeText"
              :inactive-text="item.inactiveText"
              :active-value="item.activeValue !== undefined ? item.activeValue : true"
              :inactive-value="item.inactiveValue !== undefined ? item.inactiveValue : false"
              @change="handleChange(item, $event)"
            />

            <!-- 单选框 -->
            <el-radio-group
              v-else-if="item.type === 'radio'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              @change="handleChange(item, $event)"
            >
              <el-radio
                v-for="option in item.options"
                :key="option.value"
                :label="option.value"
                :disabled="option.disabled"
              >
                {{ option.label }}
              </el-radio>
            </el-radio-group>

            <!-- 复选框组 -->
            <el-checkbox-group
              v-else-if="item.type === 'checkbox'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              @change="handleChange(item, $event)"
            >
              <el-checkbox
                v-for="option in item.options"
                :key="option.value"
                :label="option.value"
                :disabled="option.disabled"
              >
                {{ option.label }}
              </el-checkbox>
            </el-checkbox-group>

            <!-- 上传 -->
            <el-upload
              v-else-if="item.type === 'upload'"
              :action="item.action"
              :headers="item.headers"
              :data="item.data"
              :name="item.name || 'file'"
              :file-list="formData[item.prop] || []"
              :multiple="item.multiple"
              :limit="item.limit"
              :accept="item.accept"
              :disabled="item.disabled"
              :list-type="item.listType || 'text'"
              :auto-upload="item.autoUpload !== false"
              @success="(response, file, fileList) => handleUploadSuccess(item, response, file, fileList)"
              @error="(error, file, fileList) => handleUploadError(item, error, file, fileList)"
              @remove="(file, fileList) => handleUploadRemove(item, file, fileList)"
              @change="(file, fileList) => handleUploadChange(item, file, fileList)"
            >
              <template v-if="item.listType === 'picture-card'">
                <el-icon><Plus /></el-icon>
              </template>
              <template v-else>
                <el-button type="primary">{{ item.uploadText || t('components.upload') }}</el-button>
              </template>
              <template #tip v-if="item.tip">
                <div class="el-upload__tip">{{ item.tip }}</div>
              </template>
            </el-upload>

            <!-- 级联选择器 -->
            <el-cascader
              v-else-if="item.type === 'cascader'"
              v-model="formData[item.prop]"
              :options="item.options"
              :props="item.cascaderProps"
              :placeholder="item.placeholder || t('components.pleaseSelectLabel', { label: item.label })"
              :clearable="item.clearable !== false"
              :filterable="item.filterable"
              :disabled="item.disabled"
              :style="{ width: item.width || '100%' }"
              @change="handleChange(item, $event)"
            />

            <!-- 滑块 -->
            <el-slider
              v-else-if="item.type === 'slider'"
              v-model="formData[item.prop]"
              :min="item.min || 0"
              :max="item.max || 100"
              :step="item.step || 1"
              :show-input="item.showInput"
              :disabled="item.disabled"
              :range="item.range"
              @change="handleChange(item, $event)"
            />

            <!-- 评分 -->
            <el-rate
              v-else-if="item.type === 'rate'"
              v-model="formData[item.prop]"
              :max="item.max || 5"
              :allow-half="item.allowHalf"
              :disabled="item.disabled"
              :show-text="item.showText"
              :show-score="item.showScore"
              @change="handleChange(item, $event)"
            />

            <!-- 颜色选择器 -->
            <el-color-picker
              v-else-if="item.type === 'color'"
              v-model="formData[item.prop]"
              :disabled="item.disabled"
              :show-alpha="item.showAlpha"
              @change="handleChange(item, $event)"
            />

            <!-- 提示信息 -->
            <template v-if="item.tip" #extra>
              <div class="form-item-tip">{{ item.tip }}</div>
            </template>
          </el-form-item>
        </el-col>
      </el-row>

      <!-- 操作按钮 -->
      <el-form-item v-if="showButtons" :class="buttonClass">
        <slot name="buttons" :form="formData" :validate="validate" :resetFields="resetFields">
          <el-button
            v-if="showSubmitButton"
            type="primary"
            :loading="submitLoading"
            @click="handleSubmit"
          >
            {{ submitButtonText || t('components.submit') }}
          </el-button>
          <el-button
            v-if="showResetButton"
            @click="handleReset"
          >
            {{ resetButtonText || t('components.reset') }}
          </el-button>
          <el-button
            v-if="showCancelButton"
            @click="handleCancel"
          >
            {{ cancelButtonText || t('components.cancel') }}
          </el-button>
        </slot>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from "vue";
import { useI18n } from "vue-i18n";
import { Plus } from "@element-plus/icons-vue";
import EnumSelect from "@/components/EnumSelect.vue";
import BusinessSelect from "@/components/BusinessSelect.vue";
import AgentSelect from "@/components/AgentSelect.vue";
import CoinSelect from "@/components/CoinSelect.vue";

const { t } = useI18n();

const props = defineProps({
  // 表单数据
  modelValue: {
    type: Object,
    required: true,
  },
  // 表单项配置
  formItems: {
    type: Array,
    required: true,
  },
  // 表单验证规则
  rules: {
    type: Object,
    default: () => ({}),
  },
  // 标签宽度
  labelWidth: {
    type: String,
    default: "100px",
  },
  // 标签位置
  labelPosition: {
    type: String,
    default: "right",
    validator: (value) => ["left", "right", "top"].includes(value),
  },
  // 是否为行内表单
  inline: {
    type: Boolean,
    default: false,
  },
  // 表单尺寸
  size: {
    type: String,
    default: "default",
    validator: (value) => ["large", "default", "small"].includes(value),
  },
  // 是否禁用表单
  disabled: {
    type: Boolean,
    default: false,
  },
  // 列间距
  gutter: {
    type: Number,
    default: 20,
  },
  // 默认列跨度（24栅格系统）
  colSpan: {
    type: Number,
    default: 12,
  },
  // 是否显示操作按钮
  showButtons: {
    type: Boolean,
    default: true,
  },
  // 是否显示提交按钮
  showSubmitButton: {
    type: Boolean,
    default: true,
  },
  // 是否显示重置按钮
  showResetButton: {
    type: Boolean,
    default: true,
  },
  // 是否显示取消按钮
  showCancelButton: {
    type: Boolean,
    default: false,
  },
  // 提交按钮文本
  submitButtonText: {
    type: String,
    default: "",
  },
  // 重置按钮文本
  resetButtonText: {
    type: String,
    default: "",
  },
  // 取消按钮文本
  cancelButtonText: {
    type: String,
    default: "",
  },
  // 提交加载状态
  submitLoading: {
    type: Boolean,
    default: false,
  },
  // 表单类名
  formClass: {
    type: String,
    default: "",
  },
  // 按钮类名
  buttonClass: {
    type: String,
    default: "form-buttons",
  },
});

const emit = defineEmits([
  "update:modelValue",
  "submit",
  "reset",
  "cancel",
  "change",
  "blur",
  "focus",
  "clear",
  "chainChange",
  "uploadSuccess",
  "uploadError",
  "uploadRemove",
  "uploadChange",
]);

const formRef = ref(null);
const formData = ref(props.modelValue);

// 监听 modelValue 变化
watch(
  () => props.modelValue,
  (val) => {
    formData.value = val;
  },
  { deep: true }
);

// 监听 formData 变化
watch(
  formData,
  (val) => {
    emit("update:modelValue", val);
  },
  { deep: true }
);

// 表单验证
const validate = async () => {
  if (!formRef.value) return false;
  try {
    await formRef.value.validate();
    return true;
  } catch (error) {
    return false;
  }
};

// 验证指定字段
const validateField = (prop) => {
  if (!formRef.value) return;
  formRef.value.validateField(prop);
};

// 重置表单
const resetFields = () => {
  if (!formRef.value) return;
  formRef.value.resetFields();
};

// 清除验证
const clearValidate = (props) => {
  if (!formRef.value) return;
  formRef.value.clearValidate(props);
};

// 处理提交
const handleSubmit = async () => {
  const valid = await validate();
  if (valid) {
    emit("submit", formData.value);
  }
};

// 处理重置
const handleReset = () => {
  resetFields();
  emit("reset");
};

// 处理取消
const handleCancel = () => {
  emit("cancel");
};

// 处理字段变化
const handleChange = (item, value) => {
  if (item.change && typeof item.change === "function") {
    item.change(value, formData.value);
  }
  emit("change", item.prop, value, formData.value);
};

// 处理失焦
const handleBlur = (item, event) => {
  if (item.blur && typeof item.blur === "function") {
    item.blur(event, formData.value);
  }
  emit("blur", item.prop, event, formData.value);
};

// 处理获焦
const handleFocus = (item, event) => {
  if (item.focus && typeof item.focus === "function") {
    item.focus(event, formData.value);
  }
  emit("focus", item.prop, event, formData.value);
};

// 处理清空
const handleClear = (item) => {
  if (item.clear && typeof item.clear === "function") {
    item.clear(formData.value);
  }
  emit("clear", item.prop, formData.value);
};

// 处理链类型变化
const handleChainChange = (item, value) => {
  if (item.chainChange && typeof item.chainChange === "function") {
    item.chainChange(value, formData.value);
  }
  emit("chainChange", item.prop, value, formData.value);
};

// 上传成功
const handleUploadSuccess = (item, response, file, fileList) => {
  formData.value[item.prop] = fileList;
  if (item.onSuccess && typeof item.onSuccess === "function") {
    item.onSuccess(response, file, fileList);
  }
  emit("uploadSuccess", item.prop, response, file, fileList);
};

// 上传失败
const handleUploadError = (item, error, file, fileList) => {
  if (item.onError && typeof item.onError === "function") {
    item.onError(error, file, fileList);
  }
  emit("uploadError", item.prop, error, file, fileList);
};

// 移除文件
const handleUploadRemove = (item, file, fileList) => {
  formData.value[item.prop] = fileList;
  if (item.onRemove && typeof item.onRemove === "function") {
    item.onRemove(file, fileList);
  }
  emit("uploadRemove", item.prop, file, fileList);
};

// 文件变化
const handleUploadChange = (item, file, fileList) => {
  if (item.onChange && typeof item.onChange === "function") {
    item.onChange(file, fileList);
  }
  emit("uploadChange", item.prop, file, fileList);
};

// 暴露方法给父组件
defineExpose({
  validate,
  validateField,
  resetFields,
  clearValidate,
  formRef,
});
</script>

<style lang="scss" scoped>
.common-form-container {
  width: 100%;

  // 表单项样式优化
  :deep(.el-form-item) {
    margin-bottom: 22px;
    transition: all 0.3s ease;

    // 标签样式
    .el-form-item__label {
      color: #303133;
      font-weight: 500;
      font-size: 14px;
      line-height: 40px;
      padding-right: 16px;

      &::before {
        margin-right: 4px;
      }
    }

    // 表单项内容区域
    .el-form-item__content {
      line-height: 40px;
    }

    // 错误提示样式
    .el-form-item__error {
      font-size: 12px;
      line-height: 1.5;
      padding-top: 4px;
      color: #f56c6c;
      animation: slideDown 0.3s ease;
    }
  }

  // 输入框样式优化
  :deep(.el-input) {
    .el-input__wrapper {
      border-radius: 6px;
      box-shadow: 0 0 0 1px #dcdfe6 inset;
      transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);
      background-color: #fff;

      &:hover {
        box-shadow: 0 0 0 1px #c0c4cc inset;
      }

      &.is-focus {
        box-shadow: 0 0 0 1px #667eea inset;
      }
    }

    .el-input__inner {
      color: #303133;
      font-size: 14px;

      &::placeholder {
        color: #c0c4cc;
        font-size: 13px;
      }
    }
  }

  // 数字输入框样式
  :deep(.el-input-number) {
    width: 100%;

    .el-input__wrapper {
      border-radius: 6px;
      box-shadow: 0 0 0 1px #dcdfe6 inset;

      &:hover {
        box-shadow: 0 0 0 1px #c0c4cc inset;
      }

      &.is-focus {
        box-shadow: 0 0 0 1px #667eea inset;
      }
    }
  }

  // 下拉选择器样式
  :deep(.el-select) {
    .el-input__wrapper {
      border-radius: 6px;
      box-shadow: 0 0 0 1px #dcdfe6 inset;
      transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);

      &:hover {
        box-shadow: 0 0 0 1px #c0c4cc inset;
      }

      &.is-focus {
        box-shadow: 0 0 0 1px #667eea inset;
      }
    }
  }

  // 文本域样式
  :deep(.el-textarea) {
    .el-textarea__inner {
      border-radius: 6px;
      border: 1px solid #dcdfe6;
      transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);
      font-family: inherit;
      font-size: 14px;
      line-height: 1.6;
      padding: 10px 12px;

      &:hover {
        border-color: #c0c4cc;
      }

      &:focus {
        border-color: #667eea;
        outline: none;
      }

      &::placeholder {
        color: #c0c4cc;
        font-size: 13px;
      }
    }

    .el-input__count {
      background: transparent;
      color: #909399;
      font-size: 12px;
    }
  }

  // 日期选择器样式
  :deep(.el-date-editor) {
    .el-input__wrapper {
      border-radius: 6px;
      box-shadow: 0 0 0 1px #dcdfe6 inset;

      &:hover {
        box-shadow: 0 0 0 1px #c0c4cc inset;
      }

      &.is-focus {
        box-shadow: 0 0 0 1px #667eea inset;
      }
    }
  }

  // 开关样式优化
  :deep(.el-switch) {
    height: 24px;
    line-height: 24px;

    &.is-checked {
      .el-switch__core {
        background-color: #667eea;
        border-color: #667eea;
      }
    }

    .el-switch__core {
      height: 24px;
      border-radius: 12px;
      transition: all 0.3s;

      &:hover {
        box-shadow: 0 0 8px rgba(102, 126, 234, 0.3);
      }
    }
  }

  // 单选框样式优化
  :deep(.el-radio) {
    margin-right: 24px;
    font-size: 14px;

    .el-radio__input {
      &.is-checked {
        .el-radio__inner {
          background-color: #667eea;
          border-color: #667eea;
        }
      }
    }

    .el-radio__inner {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      transition: all 0.3s;

      &:hover {
        border-color: #667eea;
      }
    }

    .el-radio__label {
      color: #606266;
      font-size: 14px;
      padding-left: 8px;
    }
  }

  // 复选框样式优化
  :deep(.el-checkbox) {
    margin-right: 24px;
    font-size: 14px;

    .el-checkbox__input {
      &.is-checked {
        .el-checkbox__inner {
          background-color: #667eea;
          border-color: #667eea;
        }
      }
    }

    .el-checkbox__inner {
      width: 16px;
      height: 16px;
      border-radius: 4px;
      transition: all 0.3s;

      &:hover {
        border-color: #667eea;
      }
    }

    .el-checkbox__label {
      color: #606266;
      font-size: 14px;
      padding-left: 8px;
    }
  }

  // 滑块样式优化
  :deep(.el-slider) {
    .el-slider__runway {
      background-color: #e4e7ed;
      border-radius: 3px;
    }

    .el-slider__bar {
      background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
      border-radius: 3px;
    }

    .el-slider__button {
      border: 2px solid #667eea;
      background-color: #fff;
      transition: all 0.3s;

      &:hover {
        transform: scale(1.2);
        box-shadow: 0 0 8px rgba(102, 126, 234, 0.4);
      }
    }
  }

  // 评分样式优化
  :deep(.el-rate) {
    .el-rate__icon {
      font-size: 20px;
      transition: all 0.3s;

      &:hover {
        transform: scale(1.1);
      }
    }

    .el-rate__icon.is-active {
      color: #667eea;
    }
  }

  // 上传组件样式优化
  :deep(.el-upload) {
    .el-upload-dragger {
      border-radius: 8px;
      border: 2px dashed #dcdfe6;
      transition: all 0.3s;

      &:hover {
        border-color: #667eea;
        background-color: #f5f7fa;
      }
    }
  }

  // 提示信息样式
  .form-item-tip {
    color: #909399;
    font-size: 12px;
    line-height: 1.6;
    margin-top: 6px;
    padding: 4px 0;
    display: flex;
    align-items: flex-start;
    
    &::before {
      content: '💡';
      margin-right: 4px;
      flex-shrink: 0;
    }
  }

  // 按钮区域样式
  .form-buttons {
    margin-top: 32px;
    padding-top: 24px;
    border-top: 1px solid #e8e8e8;
    text-align: center;

    :deep(.el-form-item__content) {
      justify-content: center;
    }

    .el-button {
      min-width: 100px;
      height: 40px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      margin: 0 8px;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      }

      &:active {
        transform: translateY(0);
      }
    }

    .el-button--primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border: none;
      color: #fff;

      &:hover {
        background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
      }

      &:active {
        background: linear-gradient(135deg, #5a3a82 0%, #5568ca 100%);
      }
    }
  }

  // 确保所有表单项的宽度一致
  :deep(.el-form-item__content) {
    width: 100%;

    > * {
      width: 100%;
    }
  }

  // 开关和单选不需要全宽
  :deep(.el-switch),
  :deep(.el-radio-group),
  :deep(.el-checkbox-group),
  :deep(.el-rate),
  :deep(.el-color-picker),
  :deep(.el-slider) {
    width: auto !important;
  }

  // 上传组件特殊处理
  :deep(.el-upload) {
    width: auto !important;
  }

  // 级联选择器
  :deep(.el-cascader) {
    .el-input__wrapper {
      border-radius: 6px;
    }
  }

  // 时间选择器
  :deep(.el-time-picker) {
    .el-input__wrapper {
      border-radius: 6px;
    }
  }

  // 颜色选择器优化
  :deep(.el-color-picker) {
    .el-color-picker__trigger {
      border-radius: 6px;
      border: 1px solid #dcdfe6;
      transition: all 0.3s;

      &:hover {
        border-color: #667eea;
      }
    }
  }

  // 组合字段样式（如范围输入）
  :deep(.el-row) {
    .el-col {
      .el-input-number,
      .el-select,
      .el-input {
        width: 100%;
      }
    }
  }
}

// 错误提示动画
@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// 聚焦动画效果
:deep(.el-form-item.is-error) {
  .el-input__wrapper,
  .el-textarea__inner {
    box-shadow: 0 0 0 1px #f56c6c inset !important;
    animation: shake 0.3s;
  }
}

@keyframes shake {
  0%, 100% {
    transform: translateX(0);
  }
  10%, 30%, 50%, 70%, 90% {
    transform: translateX(-2px);
  }
  20%, 40%, 60%, 80% {
    transform: translateX(2px);
  }
}

// 响应式优化
@media (max-width: 768px) {
  .common-form-container {
    :deep(.el-form-item__label) {
      font-size: 13px;
    }

    .form-buttons {
      .el-button {
        min-width: 80px;
        height: 36px;
        font-size: 13px;
      }
    }
  }
}
</style>

