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
      <el-form-item prop="warningType" label="预警项目">
        <el-select v-model="formData.warningType" placeholder="请选择">
          <el-option
            v-for="item in MessageTypes"
            :key="item.key"
            :label="item.displayName"
            :value="item.value"
          ></el-option>
        </el-select>
      </el-form-item>

      <el-form-item label="群组类型" prop="noticeType">
        <el-select
          v-model="formData.noticeType"
          placeholder="请选择"
          @change="groupChange"
        >
          <el-option label="钉钉" :value="0"></el-option>
          <el-option label="TG" :value="1"></el-option>
        </el-select>
      </el-form-item>

      <el-form-item label="群组名称" prop="noticeGroupKey">
        <el-select v-model="formData.noticeGroupKey" placeholder="请选择">
          <el-option
            :key="item.key"
            :label="item.name"
            :value="item.key"
            v-for="item in noticeGroup"
          ></el-option>
        </el-select>
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
import {
  GetNoticeGroupAll,
  NoticeGroupCreate,
  NoticeGroupUpdate,
} from "@/api/groupNoticeSetting";

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

const MessageTypes = ref([]);

const noticeGroup = ref([]);

const dialogVisible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);
const title = ref("新增群组通知");

const formData = reactive({
  warningType: "",
  noticeType: "",
  noticeGroupKey: "",
});

const rules = {
  warningType: [{ required: true, message: "请选择预警项目" }],
  noticeType: [{ required: true, message: "请选择群组类型" }],
  noticeGroupKey: [{ required: true, message: "请选择群组名称" }],
};

const groupChange = async (value) => {
  GetNoticeGroupAll.params.noticeType = value;
  const { result } = await request(GetNoticeGroupAll);
  noticeGroup.value = result;
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
    let api = isEdit.value ? NoticeGroupUpdate : NoticeGroupCreate;
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
  isEdit.value = false;

  const { result: warningTypeResult } = await getEnum("WarningType");
  MessageTypes.value = warningTypeResult;

  if (row) {
    isEdit.value = true;
    Object.assign(formData, row);
    title.value = "编辑群组通知";
  }
};

const getNoticeGroupAll = async (item) => {
  GetNoticeGroupAll.params.noticeType = item.value;
  const { result } = await request(GetNoticeGroupAll);
  noticeNameOptions.value = result;
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
