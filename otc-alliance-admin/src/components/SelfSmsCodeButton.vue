<template>
  <el-button
    :disabled="countdown > 0 || loading"
    type="primary"
    size="large"
    @click="sendCode"
    style="margin-left: 12px"
  >
    <span v-if="countdown === 0">发送验证码</span>
    <span v-else>{{ countdown }}秒后重试</span>
  </el-button>
</template>

<script setup>
import { ref } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { BasicSendSmssCode } from "@/api/systemCode";

const countdown = ref(0);
const timer = ref(null);
const loading = ref(false);

function startCountdown() {
  countdown.value = 60;
  timer.value = setInterval(() => {
    countdown.value--;
    if (countdown.value <= 0) {
      clearInterval(timer.value);
      timer.value = null;
    }
  }, 1000);
}

async function sendCode() {
  if (countdown.value > 0 || loading.value) return;
  loading.value = true;
  try {
    await request({ ...BasicSendSmssCode });
    ElMessage.success("验证码已发送");
    startCountdown();
  } catch (e) {
    ElMessage.error("验证码发送失败");
  } finally {
    loading.value = false;
  }
}
</script>
