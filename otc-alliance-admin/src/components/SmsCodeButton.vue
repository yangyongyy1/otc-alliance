<template>
  <el-button
    :disabled="disabled || loading"
    type="primary"
    size="large"
    @click="sendCode"
  >
    <span v-if="countdown === 0">{{ buttonText }}</span>
    <span v-else>{{ countdown }}秒后重试</span>
  </el-button>
</template>

<script setup>
import { ref, watch, computed } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { BasicSendSmssCodeByPhoneNumer } from "@/api/systemCode";

const props = defineProps({
  areaCode: { type: String, required: true },
  phoneNumber: { type: String, required: true },
  buttonText: { type: String, default: "获取验证码" },
  duration: { type: Number, default: 60 },
});
const emit = defineEmits(["success"]);

const countdown = ref(0);
const timer = ref(null);
const loading = ref(false);

const disabled = computed(() => {
  return !props.areaCode || !props.phoneNumber || countdown.value > 0;
});

function startCountdown() {
  countdown.value = props.duration;
  timer.value = setInterval(() => {
    countdown.value--;
    if (countdown.value <= 0) {
      clearInterval(timer.value);
      timer.value = null;
    }
  }, 1000);
}

async function sendCode() {
  if (disabled.value) return;
  loading.value = true;
  try {
    const params = {
      areaCode: props.areaCode,
      phoneNumber: props.phoneNumber,
    };
    BasicSendSmssCodeByPhoneNumer.params = params;
    await request(BasicSendSmssCodeByPhoneNumer);
    ElMessage.success("验证码已发送");
    emit("success");
    startCountdown();
  } catch (e) {
    ElMessage.error("验证码发送失败");
  } finally {
    loading.value = false;
  }
}

watch(
  () => [props.areaCode, props.phoneNumber],
  () => {
    // 如果区号或手机号变更，重置倒计时
    if (countdown.value > 0) {
      clearInterval(timer.value);
      timer.value = null;
      countdown.value = 0;
    }
  }
);
</script>
