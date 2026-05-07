<template>
  <el-dialog
    v-model="dialogVisible"
    :title="isBound ? '修改手机号' : '绑定手机号'"
    width="500px"
    @close="handleClose"
  >
    <el-form
      ref="phoneFormRef"
      :model="phoneForm"
      :rules="phoneRules"
      label-width="100px"
    >
      <!-- 当前手机号验证 -->

      <!-- 新手机号信息 -->
      <el-form-item label="国家/地区" prop="areaCode">
        <el-select
          v-model="phoneForm.areaCode"
          placeholder="请选择国家/地区"
          class="country-select"
        >
          <el-option
            v-for="item in countryCodes"
            :key="item.country_code"
            :label="item.chinese_name"
            :value="item.phone_code"
          >
            <span class="country-option">
              <span class="flag">{{ item.english_name }}</span>
              <span class="name">{{ item.chinese_name }}</span>
              <span class="code">+{{ item.phone_code }}</span>
            </span>
          </el-option>
        </el-select>
      </el-form-item>

      <el-form-item label="手机号" prop="newPhoneNumber">
        <el-input
          v-model="phoneForm.newPhoneNumber"
          placeholder="请输入新手机号"
        />
      </el-form-item>

      <el-form-item label="验证码" prop="smsCode">
        <div class="verify-code">
          <el-input
            v-model="phoneForm.smsCode"
            placeholder="请输入新手机验证码"
          />
          <el-button
            type="primary"
            :disabled="!!newCountdown"
            @click="handleSendNewCode"
          >
            {{ newCountdown ? `${newCountdown}s后重试` : "获取验证码" }}
          </el-button>
        </div>
      </el-form-item>

      <el-form-item
        label="谷歌验证码"
        prop="gACode"
        v-if="userSecuritySetting.isGoogleEnabled"
      >
        <el-input v-model="phoneForm.gACode" placeholder="请输入谷歌验证码" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确认</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import {
  BasicSendSmssCode,
  BasicSendSmssCodeByPhoneNumer,
} from "@/api/systemCode";

import { SecurityCenterAdminUpdateUserPhone } from "@/api/SecurityCenter";

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
  userSecuritySetting: {
    type: Object,
    required: true,
  },
});

const emit = defineEmits(["update:modelValue", "success"]);

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});

const isBound = computed(() => !!props.userSecuritySetting.phoneNumber);

const phoneFormRef = ref(null);
const currentCountdown = ref(0);
const newCountdown = ref(0);

// 国家/地区代码列表
const countryCodes = ref([]);

const phoneForm = reactive({
  areaCode: "86", // 默认中国
  newPhoneNumber: "",
  smsCode: "",
  gACode: "",
});

const phoneRules = {
  areaCode: [{ required: true, message: "请选择国家/地区" }],
  newPhoneNumber: [{ required: true, message: "请输入新手机号" }],
  smsCode: [{ required: true, message: "请输入新手机验证码" }],
  gACode: [{ required: true, message: "请输入谷歌验证码" }],
};

const handleClose = () => {
  dialogVisible.value = false;
  currentCountdown.value = 0;
  newCountdown.value = 0;
  // 重置表单
  phoneFormRef.value.resetFields();
};

// 发送当前手机验证码
const handleSendCurrentCode = async () => {
  try {
    // TODO: 调用发送当前手机验证码API
    const { result } = await request(BasicSendSmssCode);
    currentCountdown.value = 60;
    const timer = setInterval(() => {
      currentCountdown.value--;
      if (currentCountdown.value <= 0) {
        clearInterval(timer);
      }
    }, 1000);
    ElMessage.success("验证码已发送到当前手机");
  } catch (error) {
    ElMessage.error("发送验证码失败");
  }
};

// 发送新手机验证码
const handleSendNewCode = async () => {
  try {
    BasicSendSmssCodeByPhoneNumer.params = {
      areaCode: phoneForm.areaCode,
      phoneNumber: phoneForm.newPhoneNumber,
    };
    const { result } = await request(BasicSendSmssCodeByPhoneNumer);
    // TODO: 调用发送新手机验证码API
    newCountdown.value = 60;
    const timer = setInterval(() => {
      newCountdown.value--;
      if (newCountdown.value <= 0) {
        clearInterval(timer);
      }
    }, 1000);
    ElMessage.success("验证码已发送到新手机");
  } catch (error) {
    ElMessage.error("发送验证码失败");
  }
};

const handleSubmit = async () => {
  if (!phoneFormRef.value) return;
  await phoneFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        SecurityCenterAdminUpdateUserPhone.data = {
          areaCode: phoneForm.areaCode,
          phoneNumber: phoneForm.newPhoneNumber,
          smsCode: phoneForm.smsCode,
          gACode: phoneForm.gACode,
        };
        const { result } = await request(SecurityCenterAdminUpdateUserPhone);
        if (result.success) {
          ElMessage.success("手机号修改成功");
          emit("success");
          handleClose();
        } else {
          ElMessage.error(result.message);
        }
      } catch (error) {
        ElMessage.error("操作失败");
      }
    }
  });
};

// 工具函数
const maskPhone = (phone) => {
  return phone.replace(/(\d{3})\d{4}(\d{4})/, "$1****$2");
};

// 初始化表单数据
watch(
  () => dialogVisible.value,
  (val) => {
    if (val && isBound.value) {
      phoneForm.phone = props.userSecuritySetting.phoneNumber;
    }
  }
);
</script>

<style lang="scss" scoped>
.verify-code {
  display: flex;
  gap: 12px;

  .el-input {
    flex: 1;
  }

  .el-button {
    width: 120px;
  }
}

.country-select {
  width: 100%;
}

.country-option {
  display: flex;
  align-items: center;
  gap: 8px;

  .flag {
    font-size: 16px;
  }

  .name {
    flex: 1;
  }

  .code {
    color: #909399;
  }
}

.current-phone {
  color: #606266;
  font-size: 14px;
}

:deep(.el-dialog__body) {
  padding-top: 20px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.el-divider {
  margin: 24px 0;
}
</style>
