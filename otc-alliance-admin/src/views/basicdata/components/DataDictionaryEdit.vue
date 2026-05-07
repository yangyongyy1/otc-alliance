<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? t('common.edit') : t('common.add')"
    :width="isJsonType ? '700px' : '500px'"
    @close="handleClose"
    :close-on-click-modal="false"
    v-loading="loading"
    class="user-add-dialog"
  >
    <el-form
      :model="form"
      :rules="rules"
      ref="formRef"
      label-width="110px"
      label-position="left"
      class="user-add-form"
    >
      <el-form-item :label="t('basic.dicType')" prop="dicType" required>
        <el-select
          v-model="form.dicType"
          :placeholder="t('basic.pleaseSelectDicType')"
          size="large"
          style="width: 100%"
          @change="handleDicTypeChange"
        >
          <el-option
            v-for="item in dicTypeOptions"
            :key="item.value"
            :label="item.displayName"
            :value="item.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="t('basic.dicKey')" prop="dicKey" required>
        <el-input v-model="form.dicKey" :placeholder="t('basic.pleaseEnterDicKey')" size="large" />
      </el-form-item>
      <el-form-item :label="t('basic.dicValue')" prop="dicValue" required>
        <JsonEditor
          v-if="isJsonType"
          v-model="form.dicValue"
          :rows="10"
          :placeholder="t('basic.pleaseEnterJsonData')"
        />
        <el-input
          v-else
          v-model="form.dicValue"
          :placeholder="t('basic.pleaseEnterDicValue')"
          size="large"
        />
      </el-form-item>
      <el-form-item :label="t('common.description')" prop="description">
        <el-input
          v-model="form.description"
          type="textarea"
          :rows="3"
          :placeholder="t('common.pleaseEnterDescription')"
          size="large"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="footer-btns">
        <el-button @click="handleClose" size="large">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading" size="large">
          {{ t('common.confirm') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed } from "vue";
import { useI18n } from "vue-i18n";
import request from "@/utils/request";
import {
  getDataDictionary,
  createDataDictionary,
  updateDataDictionary,
} from "@/api/baseData";
import { ElMessage } from "element-plus";
import { getEnum } from "@/api/enum";
import JsonEditor from "@/components/JsonEditor/index.vue";

const { t } = useI18n();

const emit = defineEmits(["fetchData"]);

const visible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);
const dicTypeOptions = ref([]);

const form = reactive({
  id: 0,
  dicKey: "",
  dicValue: "",
  description: "",
  dicType: 0,
});

// 判断当前选择的字典类型是否为 JSON
const isJsonType = computed(() => {
  if (!form.dicType) return false;
  const selectedType = dicTypeOptions.value.find(
    (item) => item.value === form.dicType
  );
  if (!selectedType) return false;
  // 通过 displayName 判断是否包含 JSON（不区分大小写）
  return (
    selectedType.displayName &&
    selectedType.displayName.toUpperCase().includes("JSON")
  );
});

// 获取字典类型枚举
const fetchDicTypeOptions = async () => {
  try {
    const { result } = await getEnum("DataDicType");
    dicTypeOptions.value = result || [];
  } catch (error) {
    dicTypeOptions.value = [];
  }
};

// 自定义验证规则：如果是 JSON 类型，验证 JSON 格式
const validateDicValue = (rule, value, callback) => {
  if (!value) {
    callback(new Error(t("basic.pleaseEnterDicValue")));
    return;
  }
  if (isJsonType.value) {
    try {
      JSON.parse(value);
      callback();
    } catch (e) {
      callback(new Error(t("basic.pleaseEnterValidJson")));
    }
  } else {
    callback();
  }
};

const rules = computed(() => ({
  dicKey: [{ required: true, message: t("basic.pleaseEnterDicKey"), trigger: "blur" }],
  dicValue: [
    { required: true, message: t("basic.pleaseEnterDicValue"), trigger: "blur" },
    { validator: validateDicValue, trigger: "blur" },
  ],
  dicType: [{ required: true, message: t("basic.pleaseSelectDicType"), trigger: "change" }],
}));

// 处理字典类型变化
const handleDicTypeChange = () => {
  // 当类型改变时，重新验证字典值字段
  if (formRef.value) {
    formRef.value.validateField("dicValue");
  }
};

const handleClose = () => {
  formRef.value?.resetFields();
  visible.value = false;
  isEdit.value = false;
  // 重置表单数据
  Object.assign(form, {
    id: 0,
    dicKey: "",
    dicValue: "",
    description: "",
    dicType: 0,
  });
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  try {
    await formRef.value.validate();
    loading.value = true;
    const api = isEdit.value ? updateDataDictionary : createDataDictionary;
    api.data = { ...form };
    const { success } = await request(api);
    if (success) {
      ElMessage.success(t("common.operationSuccess"));
      handleClose();
      emit("fetchData");
    }
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 打开弹窗的方法
const open = async (row) => {
  visible.value = true;
  isEdit.value = false;
  // 加载枚举选项
  await fetchDicTypeOptions();

  if (row) {
    isEdit.value = true;
    // 获取详情数据
    try {
      loading.value = true;
      getDataDictionary.params = { id: row.id };
      const { result } = await request(getDataDictionary);
      Object.assign(form, result);
    } catch (error) {
    } finally {
      loading.value = false;
    }
  }
};

defineExpose({
  open,
});
</script>

<style scoped>
.user-add-dialog >>> .el-dialog__body {
  padding: 32px 40px 10px 40px;
}
.user-add-form {
  margin-top: 10px;
}
.user-add-form :deep(.el-form-item) {
  margin-bottom: 18px;
}
.user-add-form :deep(.el-form-item__label) {
  position: relative;
  font-size: 16px;
  line-height: 1.2;
  padding-left: 0;
  font-weight: normal;
  color: #606266;
  display: flex;
  align-items: center;
}
.user-add-form :deep(.el-form-item__label)::before {
  content: "*";
  display: inline-block;
  width: 14px;
  color: transparent;
  margin-right: 4px;
}
.user-add-form :deep(.el-form-item__label) .el-form-item__required {
  color: #f56c6c;
  position: absolute;
  left: 0;
  width: 14px;
  text-align: left;
  font-size: 16px;
  font-weight: normal;
  background: transparent;
}
.user-add-form :deep(.el-form-item__label):not(.el-form-item__label--right) {
  justify-content: flex-start;
}
.user-add-form :deep(.el-input),
.user-add-form :deep(.el-select),
.user-add-form :deep(.el-textarea),
.user-add-form :deep(.el-input-number) {
  font-size: 14px;
}
.footer-btns {
  display: flex;
  justify-content: flex-end;
  gap: 18px;
  padding: 8px 0 0 0;
}
.user-add-dialog >>> .el-dialog__header {
  text-align: left;
  padding-left: 40px;
}
</style>

