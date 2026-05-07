<template>
  <div>
    <el-form-item
      v-for="item in fieldsList"
      :key="item.field"
      :label="item.label"
      :prop="item.field"
      required
    >
      <el-input
        v-if="item.type === 'Password'"
        v-model="modelValue[item.field]"
        type="password"
        :placeholder="`请输入${item.label}`"
        size="large"
      />
      <template v-else-if="item.type === 'Sms'">
        <el-input
          v-model="modelValue[item.field]"
          :placeholder="`请输入${item.label}`"
          size="large"
          style="width: 60%"
        />
        <SelfSmsCodeButton />
      </template>
      <el-input
        v-else-if="item.type === 'Google'"
        v-model="modelValue[item.field]"
        :placeholder="`请输入${item.label}`"
        size="large"
      />
      <!-- 可扩展更多类型 -->
    </el-form-item>
  </div>
</template>

<script setup>
import { ref } from "vue";
import SelfSmsCodeButton from "@/components/SelfSmsCodeButton.vue";

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({}),
  },
});

// 默认显示密码字段
const fieldsList = ref([
  {
    type: "Password",
    label: "登录密码",
    field: "userPwd",
  },
]);

// 初始化父表单对象的字段
fieldsList.value.forEach((item) => {
  if (!(item.field in props.modelValue)) props.modelValue[item.field] = "";
});
</script>
