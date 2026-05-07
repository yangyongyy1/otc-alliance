<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? t('common.edit') : t('common.add')"
    width="500px"
    @close="handleClose"
    :close-on-click-modal="false"
    v-loading="loading"
  >
    <el-form
      :model="form"
      :rules="rules"
      ref="formRef"
      label-width="100px"
      label-position="left"
    >
      <el-form-item :label="t('basicData.name')" prop="name" required>
        <el-input v-model="form.name" :placeholder="t('basicData.pleaseEnterName')" />
      </el-form-item>
      <el-form-item label="CCA2" prop="ccA2" required>
        <el-input v-model="form.ccA2" :placeholder="t('basicData.pleaseEnterCode')" maxlength="2" />
      </el-form-item>
      <el-form-item label="CCA3" prop="ccA3" required>
        <el-input v-model="form.ccA3" :placeholder="t('basicData.pleaseEnterCode')" maxlength="3" />
      </el-form-item>
      <el-form-item label="CCN3" prop="ccN3" required>
        <el-input v-model="form.ccN3" :placeholder="t('basicData.pleaseEnterCode')" maxlength="3" />
      </el-form-item>
      <el-form-item :label="t('basicData.chineseName')" prop="cnName" required>
        <el-input v-model="form.cnName" :placeholder="t('basicData.pleaseEnterChineseName')" />
      </el-form-item>
      <el-form-item :label="t('basicData.region')" prop="region" required>
        <el-select
          v-model="form.region"
          :placeholder="t('basicData.pleaseSelectRegion')"
          @change="handleRegionChange"
          style="width: 100%"
        >
          <el-option
            v-for="item in regionList"
            :key="item.value"
            :label="item.enName"
            :value="item.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="t('basicData.subRegion')" prop="subRegion" required>
        <el-select
          v-model="form.subRegion"
          :placeholder="t('basicData.pleaseSelectSubRegion')"
          @change="handleSubRegionChange"
          style="width: 100%"
        >
          <el-option
            v-for="item in subRegionListOptions"
            :key="item.value"
            :label="item.enName"
            :value="item.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="t('basicData.referenceCurrency')" prop="currency">
        <el-input v-model="form.currency" :placeholder="t('basicData.pleaseEnterCurrency')" />
      </el-form-item>
      <el-form-item :label="t('basicData.areaCode')" prop="areaPhoneCode">
        <el-input v-model="form.areaPhoneCode" :placeholder="t('basicData.pleaseEnterAreaCode')" />
      </el-form-item>
      <el-form-item :label="t('basicData.regionCn')" prop="cnRegion">
        <el-input v-model="form.cnRegion" :placeholder="t('basicData.pleaseEnterRegionCn')" />
      </el-form-item>
      <el-form-item :label="t('basicData.subRegionCn')" prop="cnSubRegion">
        <el-input
          v-model="form.cnSubRegion"
          :placeholder="t('basicData.pleaseEnterSubRegionCn')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="footer-btns">
        <el-button @click="handleClose">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
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
import { getCountryInfo, createCountryInfo, updateCountryInfo } from "@/api/baseData";
import { ElMessage } from "element-plus";

const { t } = useI18n();

const emit = defineEmits(["fetchData"]);

const visible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);

const form = reactive({
  name: "",
  ccA2: "",
  ccA3: "",
  ccN3: "",
  cnName: "",
  region: "",
  subRegion: "",
  currency: "",
  areaPhoneCode: "",
  cnRegion: "",
  cnSubRegion: "",
});

const regionList = [
  { enName: "Africa", cnName: "非洲", value: "Africa" },
  { enName: "Europe", cnName: "欧洲", value: "Europe" },
  { enName: "Asia", cnName: "亚洲", value: "Asia" },
  { enName: "Oceania", cnName: "大洋洲", value: "Oceania" },
  { enName: "Americas", cnName: "美洲", value: "Americas" },
  { enName: "Antarctic", cnName: "南美洲", value: "Antarctic" },
];

const subRegionList = [
  { parentRegion: "Asia", enName: "Eastern Asia", value: "Eastern Asia", cnName: "东亚" },
  { parentRegion: "Asia", enName: "Central Asia", value: "Central Asia", cnName: "中亚" },
  { parentRegion: "Asia", enName: "Southern Asia", value: "Southern Asia", cnName: "南亚" },
  { parentRegion: "Asia", enName: "Western Asia", value: "Western Asia", cnName: "西亚" },
  { parentRegion: "Asia", enName: "Southeast Asia", value: "Southeast Asia", cnName: "东南亚" },
  { parentRegion: "Europe", enName: "Northern Europe", value: "Northern Europe", cnName: "北欧" },
  { parentRegion: "Europe", enName: "Southern Europe", value: "Southern Europe", cnName: "南欧" },
  { parentRegion: "Europe", enName: "Western Europe", value: "Western Europe", cnName: "西欧" },
  { parentRegion: "Europe", enName: "Central Europe", value: "Central Europe", cnName: "中欧" },
  { parentRegion: "Europe", enName: "Eastern Europe", value: "Eastern Europe", cnName: "东欧" },
  { parentRegion: "Oceania", enName: "Melanesia", value: "Melanesia", cnName: "美拉尼西亚" },
  { parentRegion: "Oceania", enName: "Micronesia", value: "Micronesia", cnName: "密克罗尼西亚" },
  { parentRegion: "Oceania", enName: "Polynesia", value: "Polynesia", cnName: "波利尼西亚" },
  { parentRegion: "Oceania", enName: "Australia/New Zealand", value: "Australia/New Zealand", cnName: "澳大利亚和新西兰" },
  { parentRegion: "Americas", enName: "Caribbean", value: "Caribbean", cnName: "加勒比" },
  { parentRegion: "Americas", enName: "Central America", value: "Central America", cnName: "中美洲" },
  { parentRegion: "Americas", enName: "South America", value: "South America", cnName: "南美洲" },
  { parentRegion: "Americas", enName: "Northern America", value: "Northern America", cnName: "北美洲" },
  { parentRegion: "Antarctic", enName: "Antarctic", value: "Antarctic", cnName: "南极洲" },
  { parentRegion: "Africa", enName: "Northern Africa", value: "Northern Africa", cnName: "北非" },
  { parentRegion: "Africa", enName: "Southern Africa", value: "Southern Africa", cnName: "南非" },
  { parentRegion: "Africa", enName: "Western Africa", value: "Western Africa", cnName: "西非" },
  { parentRegion: "Africa", enName: "Eastern Africa", value: "Eastern Africa", cnName: "东非" },
  { parentRegion: "Africa", enName: "Middle Africa", value: "Middle Africa", cnName: "中非" },
];

const subRegionListOptions = ref([]);

const rules = computed(() => ({
  name: [{ required: true, message: t("basicData.pleaseEnterName"), trigger: "blur" }],
  ccA2: [
    { required: true, message: t("basicData.pleaseEnterCode"), trigger: "blur" },
    { max: 2, message: t("basicData.codeLengthExceeded", { max: 2 }), trigger: "blur" },
  ],
  ccA3: [
    { required: true, message: t("basicData.pleaseEnterCode"), trigger: "blur" },
    { max: 3, message: t("basicData.codeLengthExceeded", { max: 3 }), trigger: "blur" },
  ],
  ccN3: [
    { required: true, message: t("basicData.pleaseEnterCode"), trigger: "blur" },
    { max: 3, message: t("basicData.codeLengthExceeded", { max: 3 }), trigger: "blur" },
  ],
  cnName: [{ required: true, message: t("basicData.pleaseEnterChineseName"), trigger: "blur" }],
  region: [{ required: true, message: t("basicData.pleaseSelectRegion"), trigger: "change" }],
  subRegion: [{ required: true, message: t("basicData.pleaseSelectSubRegion"), trigger: "change" }],
}));

const handleRegionChange = (val) => {
  const regionItem = regionList.find((item) => item.value === val);
  if (regionItem) {
    form.cnRegion = regionItem.cnName;
  }
  subRegionListOptions.value = subRegionList.filter(
    (item) => item.parentRegion === val
  );
  // 清空子区域选择
  form.subRegion = "";
  form.cnSubRegion = "";
};

const handleSubRegionChange = () => {
  const subRegionItem = subRegionList.find(
    (item) => item.value === form.subRegion
  );
  if (subRegionItem) {
    form.cnSubRegion = subRegionItem.cnName;
  }
};

const handleClose = () => {
  formRef.value?.resetFields();
  visible.value = false;
  isEdit.value = false;
  // 重置表单数据
  Object.assign(form, {
    name: "",
    ccA2: "",
    ccA3: "",
    ccN3: "",
    cnName: "",
    region: "",
    subRegion: "",
    currency: "",
    areaPhoneCode: "",
    cnRegion: "",
    cnSubRegion: "",
  });
  subRegionListOptions.value = [];
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  try {
    await formRef.value.validate();
    loading.value = true;
    const api = isEdit.value ? updateCountryInfo : createCountryInfo;
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
  subRegionListOptions.value = [];

  if (row) {
    isEdit.value = true;
    // 获取详情数据
    try {
      loading.value = true;
      getCountryInfo.params = { id: row.id };
      const { result } = await request(getCountryInfo);
      Object.assign(form, result);
      // 设置子区域选项
      if (form.region) {
        subRegionListOptions.value = subRegionList.filter(
          (item) => item.parentRegion === form.region
        );
      }
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

<style lang="scss" scoped>
.footer-btns {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>

