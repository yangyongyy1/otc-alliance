<template>
  <el-dialog
    v-model="dialogVisible"
    title="新增角色"
    width="500px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    @close="handleClose"
    :loading="loading"
  >
    <el-form
      ref="formRef"
      :model="formData"
      :rules="rules"
      label-width="100px"
      class="permission-form"
    >
      <el-form-item label="菜单名称" prop="menuName">
        <el-input v-model="formData.menuName" placeholder="请输入菜单名称" />
      </el-form-item>

      <el-form-item label="父级菜单" prop="parentID">
        <el-cascader
          v-model="formData.parentID"
          :options="parentMenus"
          :props="{ ...treeProps, checkStrictly: true, emitPath: false }"
          placeholder="请选择父级菜单"
          clearable
        />
      </el-form-item>

      <el-form-item label="菜单图标" prop="iCon">
        <el-input v-model="formData.iCon" placeholder="请输入菜单图标" />
      </el-form-item>

      <el-form-item label="路由路径" prop="routePath">
        <el-input v-model="formData.routePath" placeholder="请输入路由路径" />
      </el-form-item>

      <el-form-item label="组件名称" prop="componentsPath">
        <el-input
          v-model="formData.componentsPath"
          placeholder="请输入组件名称"
        />
      </el-form-item>

      <el-form-item label="菜单排序" prop="sort">
        <el-input-number v-model="formData.sort" placeholder="请输入菜单排序" />
      </el-form-item>
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
  SysMeunGetAllWithTree,
  SysMeunCreate,
  SysMeunUpdate,
} from "@/api/businessPermission";

const props = defineProps({
  permissions: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(["success"]);

const treeProps = {
  value: "id",
  label: "menuName",
  children: "childs",
};

const dialogVisible = ref(false);
const formRef = ref(null);
const loading = ref(false);
const isEdit = ref(false);
const parentMenus = ref([]);

const formData = reactive({
  menuName: "",
  parentID: null,
  componentsPath: "",
  routePath: "",
  iCon: "",
  isMenu: true,
  sort: 0,
});

const rules = {
  menuName: [{ required: true, message: "请输入菜单名称", trigger: "blur" }],
  parentID: [{ required: true, message: "请选择父级菜单", trigger: "blur" }],
  iCon: [{ required: true, message: "请输入菜单图标", trigger: "blur" }],
  routePath: [{ required: true, message: "请输入路由路径", trigger: "blur" }],
  componentsPath: [
    { required: true, message: "请输入组件名称", trigger: "blur" },
  ],
  sort: [{ required: true, message: "请输入菜单排序", trigger: "blur" }],
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
    let api = isEdit.value ? SysMeunUpdate : SysMeunCreate;
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
  await getParentMenus();
  isEdit.value = false;
  if (row) {
    isEdit.value = true;
    Object.assign(formData, row);
    formData.parentID = row.pId || null;
  }
};

const getParentMenus = async () => {
  const { result } = await request(SysMeunGetAllWithTree);
  parentMenus.value = result;
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
