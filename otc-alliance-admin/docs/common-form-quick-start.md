# CommonForm 快速上手

## 5分钟快速开始

### 1. 基础表单（最简单）

```vue
<template>
  <CommonForm
    v-model="form"
    :form-items="items"
    @submit="onSubmit"
  />
</template>

<script setup>
import { ref } from 'vue';
import CommonForm from '@/components/CommonForm/index.vue';

const form = ref({
  name: '',
  age: null,
});

const items = [
  { label: '姓名', prop: 'name', type: 'input' },
  { label: '年龄', prop: 'age', type: 'number' },
];

const onSubmit = (data) => {
  console.log('提交：', data);
};
</script>
```

### 2. 添加验证规则

```javascript
const rules = {
  name: [
    { required: true, message: '请输入姓名', trigger: 'blur' }
  ],
  age: [
    { required: true, message: '请输入年龄', trigger: 'blur' },
    { type: 'number', message: '必须是数字', trigger: 'blur' }
  ],
};
```

```vue
<CommonForm
  v-model="form"
  :form-items="items"
  :rules="rules"
  @submit="onSubmit"
/>
```

### 3. 使用自定义选择器

#### 枚举选择器

```javascript
{
  label: '状态',
  prop: 'status',
  type: 'enum',
  enumName: 'OpenClosedStatus', // 枚举名称
}
```

#### 商户选择器

```javascript
{
  label: '商户',
  prop: 'businessId',
  type: 'business',
  businessType: 1, // 商户类型
}
```

#### 代理选择器

```javascript
{
  label: '代理',
  prop: 'agentId',
  type: 'agent',
}
```

#### 虚拟币选择器（带链类型）

```javascript
// 在 form 中添加两个字段
const form = ref({
  coinName: '',
  chainType: '',
});

// 配置项
{
  label: '虚拟币',
  prop: 'coinName',
  type: 'coin',
  showChainSelect: true,
  chainProp: 'chainType', // 链类型字段
}
```

### 4. 常用表单项示例

```javascript
const formItems = [
  // 文本输入
  { label: '用户名', prop: 'username', type: 'input' },
  
  // 数字输入
  { label: '金额', prop: 'amount', type: 'number', min: 0, step: 0.01 },
  
  // 文本域
  { label: '描述', prop: 'desc', type: 'textarea', rows: 4 },
  
  // 下拉选择
  {
    label: '类型',
    prop: 'type',
    type: 'select',
    options: [
      { label: '类型A', value: 1 },
      { label: '类型B', value: 2 },
    ],
  },
  
  // 日期选择
  { label: '日期', prop: 'date', type: 'date' },
  
  // 日期范围
  { label: '日期范围', prop: 'dateRange', type: 'daterange' },
  
  // 开关
  {
    label: '启用',
    prop: 'enabled',
    type: 'switch',
    activeText: '是',
    inactiveText: '否',
  },
  
  // 单选
  {
    label: '性别',
    prop: 'gender',
    type: 'radio',
    options: [
      { label: '男', value: 1 },
      { label: '女', value: 2 },
    ],
  },
];
```

### 5. 布局控制

```javascript
{
  label: '标题',
  prop: 'title',
  type: 'input',
  span: 24, // 占满一行（24栅格）
}

{
  label: '名称',
  prop: 'name',
  type: 'input',
  span: 12, // 占半行
}
```

### 6. 对话框中使用

```vue
<template>
  <el-button @click="open">打开表单</el-button>
  
  <el-dialog v-model="visible" title="编辑" width="600px">
    <CommonForm
      v-model="form"
      :form-items="items"
      :rules="rules"
      :submit-loading="loading"
      :show-cancel-button="true"
      @submit="handleSubmit"
      @cancel="visible = false"
    />
  </el-dialog>
</template>

<script setup>
import { ref } from 'vue';

const visible = ref(false);
const loading = ref(false);
const form = ref({ name: '' });

const items = [
  { label: '姓名', prop: 'name', type: 'input', span: 24 },
];

const rules = {
  name: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
};

const open = () => {
  visible.value = true;
};

const handleSubmit = async (data) => {
  loading.value = true;
  try {
    // 调用 API
    await saveData(data);
    visible.value = false;
  } finally {
    loading.value = false;
  }
};
</script>
```

### 7. 搜索表单

```vue
<template>
  <CommonForm
    v-model="searchForm"
    :form-items="searchItems"
    :inline="true"
    :show-reset-button="true"
    submit-button-text="搜索"
    @submit="handleSearch"
  />
</template>

<script setup>
const searchForm = ref({
  keyword: '',
  status: '',
  date: '',
});

const searchItems = [
  { label: '关键词', prop: 'keyword', type: 'input' },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'OpenClosedStatus' },
  { label: '日期', prop: 'date', type: 'date' },
];

const handleSearch = (data) => {
  console.log('搜索：', data);
  // 调用搜索 API
};
</script>
```

### 8. 获取表单实例

```vue
<template>
  <CommonForm ref="formRef" v-model="form" :form-items="items" />
  <el-button @click="submitForm">提交</el-button>
</template>

<script setup>
const formRef = ref(null);

const submitForm = async () => {
  // 手动验证
  const valid = await formRef.value.validate();
  if (valid) {
    console.log('验证通过');
  }
};

// 重置表单
const reset = () => {
  formRef.value.resetFields();
};

// 清除验证
const clear = () => {
  formRef.value.clearValidate();
};
</script>
```

## 与 CommonTable 配合使用

```vue
<template>
  <!-- 搜索表单 -->
  <CommonForm
    v-model="searchForm"
    :form-items="searchItems"
    :inline="true"
    submit-button-text="搜索"
    @submit="handleSearch"
  />
  
  <!-- 数据表格 -->
  <CommonTable
    :search-form="searchForm"
    :filter-list="[]"
    :table-columns="columns"
    :table-data="tableData"
    :total="total"
    :loading="loading"
    @add="handleAdd"
  />
  
  <!-- 新增/编辑对话框 -->
  <el-dialog v-model="dialogVisible" :title="dialogTitle">
    <CommonForm
      v-model="currentRow"
      :form-items="formItems"
      :rules="rules"
      :submit-loading="submitLoading"
      :show-cancel-button="true"
      @submit="handleSubmit"
      @cancel="dialogVisible = false"
    />
  </el-dialog>
</template>

<script setup>
import { ref } from 'vue';
import CommonForm from '@/components/CommonForm/index.vue';
import CommonTable from '@/components/CommonTable/index.vue';

// 搜索表单
const searchForm = ref({ keyword: '', status: '' });
const searchItems = [
  { label: '关键词', prop: 'keyword', type: 'input' },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'OpenClosedStatus' },
];

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);

// 对话框表单
const dialogVisible = ref(false);
const dialogTitle = ref('新增');
const submitLoading = ref(false);
const currentRow = ref({});

const formItems = [
  { label: '名称', prop: 'name', type: 'input', required: true, span: 24 },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'OpenClosedStatus', span: 24 },
];

const rules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
};

// 搜索
const handleSearch = (data) => {
  console.log('搜索：', data);
  loadData();
};

// 新增
const handleAdd = () => {
  dialogTitle.value = '新增';
  currentRow.value = {};
  dialogVisible.value = true;
};

// 提交
const handleSubmit = async (data) => {
  submitLoading.value = true;
  try {
    // 调用 API
    await saveData(data);
    dialogVisible.value = false;
    loadData();
  } finally {
    submitLoading.value = false;
  }
};

// 加载数据
const loadData = async () => {
  loading.value = true;
  try {
    const res = await fetchData(searchForm.value);
    tableData.value = res.items;
    total.value = res.total;
  } finally {
    loading.value = false;
  }
};
</script>
```

## 常见问题

### 1. 如何设置默认值？

直接在 `formData` 中设置即可：

```javascript
const form = ref({
  name: '张三',
  age: 18,
  status: 1,
});
```

### 2. 如何隐藏某个字段？

```javascript
{
  label: '隐藏字段',
  prop: 'hidden',
  type: 'input',
  hidden: true, // 设置为 true
}
```

### 3. 如何禁用某个字段？

```javascript
{
  label: '禁用字段',
  prop: 'disabled',
  type: 'input',
  disabled: true,
}
```

### 4. 如何动态改变表单项？

使用 `computed` 计算属性：

```javascript
const formItems = computed(() => {
  const items = [/* 基础配置 */];
  
  if (condition) {
    items.push({/* 动态添加 */});
  }
  
  return items;
});
```

### 5. 如何监听字段变化？

方法1：使用 `change` 回调

```javascript
{
  label: '类型',
  prop: 'type',
  type: 'select',
  change: (value, formData) => {
    console.log('类型改变：', value);
    // 可以修改其他字段
    formData.relatedField = '';
  },
}
```

方法2：使用 `@change` 事件

```vue
<CommonForm
  v-model="form"
  :form-items="items"
  @change="handleChange"
/>
```

```javascript
const handleChange = (prop, value, formData) => {
  if (prop === 'type') {
    console.log('类型改变：', value);
  }
};
```

## 下一步

- 查看完整文档：`docs/common-form-guide.md`
- 查看更多示例：`src/components/CommonForm/example.vue`
- 了解自定义选择器：查看各个选择器组件的源码

## 技术支持

如有问题，请查看：
1. Element Plus 官方文档
2. 项目组件源码
3. 项目示例代码

