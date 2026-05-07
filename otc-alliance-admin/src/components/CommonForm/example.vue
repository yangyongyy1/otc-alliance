<template>
  <div class="example-container">
    <h2>CommonForm 使用示例</h2>

    <!-- 示例1：基础表单 -->
    <div class="example-section">
      <h3>1. 基础表单</h3>
      <CommonForm
        v-model="basicForm"
        :form-items="basicFormItems"
        :rules="basicRules"
        @submit="handleBasicSubmit"
        @reset="handleBasicReset"
      />
    </div>

    <!-- 示例2：自定义选择器表单 -->
    <div class="example-section">
      <h3>2. 自定义选择器表单（枚举、商户、代理、虚拟币）</h3>
      <CommonForm
        v-model="customForm"
        :form-items="customFormItems"
        :rules="customRules"
        @submit="handleCustomSubmit"
        @reset="handleCustomReset"
      />
    </div>

    <!-- 示例3：复杂表单（包含日期范围、上传等） -->
    <div class="example-section">
      <h3>3. 复杂表单</h3>
      <CommonForm
        v-model="complexForm"
        :form-items="complexFormItems"
        :col-span="24"
        :show-cancel-button="true"
        @submit="handleComplexSubmit"
        @cancel="handleComplexCancel"
      />
    </div>

    <!-- 示例4：插槽自定义 -->
    <div class="example-section">
      <h3>4. 插槽自定义</h3>
      <CommonForm
        v-model="slotForm"
        :form-items="slotFormItems"
        @submit="handleSlotSubmit"
      >
        <!-- 自定义插槽 -->
        <template #customField="{ form }">
          <div class="custom-field">
            <el-tag>自定义内容：{{ form.name }}</el-tag>
            <el-input v-model="form.customValue" placeholder="自定义输入" style="margin-top: 10px;" />
          </div>
        </template>
      </CommonForm>
    </div>

    <!-- 示例5：行内表单 -->
    <div class="example-section">
      <h3>5. 行内表单（搜索）</h3>
      <CommonForm
        v-model="searchForm"
        :form-items="searchFormItems"
        :inline="true"
        :show-reset-button="true"
        submit-button-text="搜索"
        @submit="handleSearch"
      />
    </div>

    <!-- 示例6：对话框表单 -->
    <div class="example-section">
      <h3>6. 对话框表单</h3>
      <el-button type="primary" @click="dialogVisible = true">打开对话框表单</el-button>
      
      <el-dialog
        v-model="dialogVisible"
        title="用户信息"
        width="600px"
      >
        <CommonForm
          v-model="dialogForm"
          :form-items="dialogFormItems"
          :rules="dialogRules"
          :submit-loading="dialogLoading"
          :show-cancel-button="true"
          cancel-button-text="关闭"
          @submit="handleDialogSubmit"
          @cancel="dialogVisible = false"
        />
      </el-dialog>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { ElMessage } from "element-plus";
import CommonForm from "./index.vue";

// ===== 示例1：基础表单 =====
const basicForm = ref({
  name: "",
  age: null,
  email: "",
  description: "",
});

const basicFormItems = [
  {
    label: "姓名",
    prop: "name",
    type: "input",
    placeholder: "请输入姓名",
    required: true,
  },
  {
    label: "年龄",
    prop: "age",
    type: "number",
    min: 0,
    max: 150,
    required: true,
  },
  {
    label: "邮箱",
    prop: "email",
    type: "input",
    placeholder: "请输入邮箱",
    required: true,
  },
  {
    label: "描述",
    prop: "description",
    type: "textarea",
    rows: 4,
    span: 24,
    maxlength: 200,
    showWordLimit: true,
  },
];

const basicRules = {
  name: [
    { required: true, message: "请输入姓名", trigger: "blur" },
    { min: 2, max: 20, message: "长度在 2 到 20 个字符", trigger: "blur" },
  ],
  age: [
    { required: true, message: "请输入年龄", trigger: "blur" },
    { type: "number", message: "年龄必须为数字", trigger: "blur" },
  ],
  email: [
    { required: true, message: "请输入邮箱", trigger: "blur" },
    { type: "email", message: "请输入正确的邮箱格式", trigger: "blur" },
  ],
};

const handleBasicSubmit = (formData) => {
  console.log("基础表单提交：", formData);
  ElMessage.success("基础表单提交成功");
};

const handleBasicReset = () => {
  ElMessage.info("基础表单已重置");
};

// ===== 示例2：自定义选择器表单 =====
const customForm = ref({
  status: "",
  businessId: "",
  agentId: "",
  coinName: "",
  chainType: "",
});

const customFormItems = [
  {
    label: "状态",
    prop: "status",
    type: "enum",
    enumName: "OpenClosedStatus",
    placeholder: "请选择状态",
    required: true,
  },
  {
    label: "商户",
    prop: "businessId",
    type: "business",
    businessType: 1, // 根据实际业务类型设置
    placeholder: "请选择商户",
  },
  {
    label: "代理",
    prop: "agentId",
    type: "agent",
    placeholder: "请选择代理",
  },
  {
    label: "虚拟币",
    prop: "coinName",
    type: "coin",
    placeholder: "请选择虚拟币",
    showChainSelect: true,
    chainProp: "chainType",
    chainPlaceholder: "请选择链类型",
    span: 24,
  },
];

const customRules = {
  status: [{ required: true, message: "请选择状态", trigger: "change" }],
};

const handleCustomSubmit = (formData) => {
  console.log("自定义选择器表单提交：", formData);
  ElMessage.success("自定义选择器表单提交成功");
};

const handleCustomReset = () => {
  ElMessage.info("自定义选择器表单已重置");
};

// ===== 示例3：复杂表单 =====
const complexForm = ref({
  title: "",
  category: "",
  tags: [],
  dateRange: [],
  status: true,
  priority: 1,
  files: [],
  color: "#409EFF",
});

const complexFormItems = [
  {
    label: "标题",
    prop: "title",
    type: "input",
    span: 24,
    required: true,
  },
  {
    label: "分类",
    prop: "category",
    type: "select",
    options: [
      { label: "技术", value: "tech" },
      { label: "生活", value: "life" },
      { label: "工作", value: "work" },
    ],
  },
  {
    label: "标签",
    prop: "tags",
    type: "checkbox",
    options: [
      { label: "Vue", value: "vue" },
      { label: "React", value: "react" },
      { label: "Angular", value: "angular" },
    ],
  },
  {
    label: "日期范围",
    prop: "dateRange",
    type: "daterange",
    span: 24,
  },
  {
    label: "启用状态",
    prop: "status",
    type: "switch",
    activeText: "启用",
    inactiveText: "禁用",
  },
  {
    label: "优先级",
    prop: "priority",
    type: "slider",
    min: 1,
    max: 5,
    showInput: true,
  },
  {
    label: "主题色",
    prop: "color",
    type: "color",
  },
  {
    label: "附件",
    prop: "files",
    type: "upload",
    action: "/api/upload",
    listType: "picture-card",
    span: 24,
    tip: "只能上传jpg/png文件，且不超过2MB",
  },
];

const handleComplexSubmit = (formData) => {
  console.log("复杂表单提交：", formData);
  ElMessage.success("复杂表单提交成功");
};

const handleComplexCancel = () => {
  ElMessage.info("取消操作");
};

// ===== 示例4：插槽自定义 =====
const slotForm = ref({
  name: "",
  customValue: "",
});

const slotFormItems = [
  {
    label: "名称",
    prop: "name",
    type: "input",
  },
  {
    label: "自定义字段",
    prop: "customField",
    type: "slot",
    slotName: "customField",
    span: 24,
  },
];

const handleSlotSubmit = (formData) => {
  console.log("插槽表单提交：", formData);
  ElMessage.success("插槽表单提交成功");
};

// ===== 示例5：行内表单（搜索） =====
const searchForm = ref({
  keyword: "",
  status: "",
  date: "",
});

const searchFormItems = [
  {
    label: "关键词",
    prop: "keyword",
    type: "input",
    placeholder: "请输入关键词",
  },
  {
    label: "状态",
    prop: "status",
    type: "enum",
    enumName: "OpenClosedStatus",
    placeholder: "请选择状态",
  },
  {
    label: "日期",
    prop: "date",
    type: "date",
  },
];

const handleSearch = (formData) => {
  console.log("搜索参数：", formData);
  ElMessage.success("搜索成功");
};

// ===== 示例6：对话框表单 =====
const dialogVisible = ref(false);
const dialogLoading = ref(false);
const dialogForm = ref({
  username: "",
  password: "",
  role: "",
  phone: "",
});

const dialogFormItems = [
  {
    label: "用户名",
    prop: "username",
    type: "input",
    span: 24,
    required: true,
  },
  {
    label: "密码",
    prop: "password",
    type: "input",
    span: 24,
    required: true,
  },
  {
    label: "角色",
    prop: "role",
    type: "select",
    options: [
      { label: "管理员", value: "admin" },
      { label: "用户", value: "user" },
    ],
    span: 24,
  },
  {
    label: "手机号",
    prop: "phone",
    type: "input",
    span: 24,
  },
];

const dialogRules = {
  username: [
    { required: true, message: "请输入用户名", trigger: "blur" },
  ],
  password: [
    { required: true, message: "请输入密码", trigger: "blur" },
    { min: 6, message: "密码长度不能少于6位", trigger: "blur" },
  ],
};

const handleDialogSubmit = async (formData) => {
  dialogLoading.value = true;
  // 模拟异步提交
  setTimeout(() => {
    console.log("对话框表单提交：", formData);
    ElMessage.success("对话框表单提交成功");
    dialogLoading.value = false;
    dialogVisible.value = false;
  }, 1000);
};
</script>

<style lang="scss" scoped>
.example-container {
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;

  h2 {
    color: #333;
    margin-bottom: 30px;
    padding-bottom: 10px;
    border-bottom: 2px solid #409eff;
  }

  .example-section {
    background: #fff;
    border-radius: 8px;
    padding: 24px;
    margin-bottom: 30px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);

    h3 {
      color: #409eff;
      margin-bottom: 20px;
      font-size: 16px;
      font-weight: 600;
    }
  }

  .custom-field {
    width: 100%;
  }
}
</style>

