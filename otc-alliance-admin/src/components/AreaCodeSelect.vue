<template>
  <el-select
    v-model="selectedValue"
    filterable
    clearable
    :placeholder="placeholder"
    :loading="loading"
    @change="onChange"
    style="width: 100%"
  >
    <el-option
      v-for="item in areaList"
      :key="item.phone_code"
      :label="`${item.chinese_name} (${item.phone_code})`"
      :value="item.phone_code"
    />
  </el-select>
</template>

<script setup>
import { ref, watch } from "vue";

const props = defineProps({
  modelValue: {
    type: String,
    default: "",
  },
  placeholder: {
    type: String,
    default: "请选择区号",
  },
});

const emit = defineEmits(["update:modelValue", "change"]);

const areaList = ref([]);
const loading = ref(false);
const selectedValue = ref(props.modelValue);

watch(
  () => props.modelValue,
  (val) => {
    selectedValue.value = val;
  }
);

watch(selectedValue, (val) => {
  emit("update:modelValue", val);
});

const onChange = (val) => {
  emit("change", val);
};
</script>
