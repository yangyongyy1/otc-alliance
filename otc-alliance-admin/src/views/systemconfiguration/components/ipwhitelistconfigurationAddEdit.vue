<template>
  <el-dialog
    v-model="dialogVisible"
    :title="title"
    width="500px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    @close="handleClose"
    v-loading="loading"
  >
    <el-form
      ref="formRef"
      :model="formData"
      :rules="rules"
      label-width="100px"
      class="permission-form"
    >
      <el-form-item prop="createType" label="类型">
        <el-select v-model="formData.createType" placeholder="请选择">
          <el-option key="0" label="API" value="0"></el-option>
          <el-option key="1" label="Web" value="1"></el-option>
        </el-select>
      </el-form-item>

      <el-form-item prop="sourceTag" label="标签">
        <el-select v-model="formData.sourceTag" placeholder="请选择">
          <el-option key="0" label="sunpay" value="0"></el-option>
          <el-option key="1" label="klicklpay" value="1"></el-option>
        </el-select>
      </el-form-item>

      <el-form-item prop="ip" label="IP">
        <el-input v-model="formData.ip" placeholder="请输入IP" />
      </el-form-item>

      <el-form-item prop="userType" label="用户类型">
        <el-select
          v-model="formData.userType"
          placeholder="请选择"
          clearable
          @change="getBusinessOptions"
        >
          <el-option
            v-for="item in userTypes"
            :key="item.value"
            :label="item.displayName"
            :value="item.value"
            :disabled="item.value === 4"
          ></el-option>
        </el-select>
      </el-form-item>

      <el-form-item prop="businessID" label="商家">
        <el-select v-model="formData.businessID" placeholder="请选择">
          <el-option
            v-for="item in businessOptions"
            :key="item.id"
            :label="item.name"
            :value="item.id"
          ></el-option>
        </el-select>
      </el-form-item>

      <el-form-item prop="comment" label="备注">
        <el-input v-model="formData.comment" placeholder="请输入备注" />
      </el-form-item>
      <security-verify-fields v-model="formData" />
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          确定
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed, reactive } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import { CreateWhiteListItem } from "@/api/systemConfig";

import { GetAllForSelectItemByType } from "@/api/commonApi";

//安全认证
import SecurityVerifyFields from "@/components/SecurityVerifyFields.vue";

import { getEnum } from "@/api/enum";

const props = defineProps({
  permissions: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(["success"]);

const userTypes = ref([]);
const businessOptions = ref([]);
const dialogVisible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);
const parentMenus = ref([]);
const title = ref("新增IP白名单");

const formData = reactive({
  ip: "",
  createType: "",
  comment: "",
  userPwd: "",
  smssCode: "",
  gaCode: "",
  sourceTag: "",
  businessID: "",
  userType: "",
  userPwd: "",
  smssCode: "",
  gaCode: "",
});

const rules = {
  ip: [
    { required: true, message: "请输入IP", trigger: "blur" },
    {
      pattern: /^(\d{1,3}\.){3}\d{1,3}$/,
      message: "请输入正确的IP地址",
      trigger: "blur",
    },
  ],
  createType: [{ required: true, message: "请选择类型", trigger: "blur" }],
  comment: [{ required: true, message: "请输入备注", trigger: "blur" }],
  userPwd: [{ required: true, message: "请输入密码", trigger: "blur" }],
  smssCode: [{ required: true, message: "请输入短信验证码", trigger: "blur" }],
  gaCode: [{ required: true, message: "请输入谷歌验证码", trigger: "blur" }],
  sourceTag: [{ required: true, message: "请选择标签", trigger: "blur" }],
  businessID: [{ required: true, message: "请选择商家", trigger: "blur" }],
  userType: [{ required: true, message: "请选择用户类型", trigger: "blur" }],
  userPwd: [{ required: true, message: "请输入密码", trigger: "blur" }],
  smssCode: [{ required: true, message: "请输入短信验证码", trigger: "blur" }],
  gaCode: [{ required: true, message: "请输入谷歌验证码", trigger: "blur" }],
};

const getBusinessOptions = async () => {
  GetAllForSelectItemByType.params.userType = formData.userType;
  const { result } = await request(GetAllForSelectItemByType);
  businessOptions.value = result;
};

const handleClose = () => {
  resetFormData();
  dialogVisible.value = false;
  //调用父组件刷新方法
  emit("refresh");
};

const resetFormData = () => {
  Object.keys(formData).forEach((key) => {
    formData[key] = "";
  });
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    loading.value = true;
    let api = isEdit.value ? CreateWhiteListItem : CreateWhiteListItem;
    api.data = formData;
    const { result } = await request(api);
    ElMessage.success("添加成功");
    emit("success");
    handleClose();
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 打开弹窗的方法
const open = async (row) => {
  dialogVisible.value = true;
  const { result: userTypesResult } = await getEnum("UserType");
  userTypes.value = userTypesResult;
  isEdit.value = false;
  if (row) {
    isEdit.value = true;
    Object.assign(formData, row);
    title.value = "编辑IP白名单";
  }
};

// 暴露方法给父组件
defineExpose({
  open,
});
</script>

<style scoped>
.permission-form {
  padding: 20px 0;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

:deep(.el-dialog__body) {
  padding-top: 10px;
}

:deep(.el-cascader) {
  width: 100%;
}
</style>
