# 通用组件系统总览

本项目封装了一套完整的通用组件系统，旨在提高开发效率，减少重复代码，统一业务逻辑。

## 📦 组件列表

### 1. CommonForm - 通用表单组件

高度可配置的表单组件，支持 20+ 种表单类型。

**核心特性：**
- ✅ 配置式开发，无需重复编写表单代码
- ✅ 支持所有常用表单组件
- ✅ 集成自定义业务选择器
- ✅ 响应式布局（24栅格）
- ✅ 完整的表单验证支持

**快速使用：**
```vue
<CommonForm
  v-model="form"
  :form-items="items"
  @submit="handleSubmit"
/>
```

**文档：**
- [快速上手](./common-form-quick-start.md)
- [完整指南](./common-form-guide.md)
- [示例代码](../src/components/CommonForm/example.vue)

---

### 2. CommonTable - 通用表格组件

功能强大的表格组件，包含搜索、分页、列设置等功能。

**核心特性：**
- ✅ 搜索区域配置
- ✅ 表格列配置
- ✅ 自动分页
- ✅ 列显示/隐藏设置
- ✅ 操作按钮配置
- ✅ 权限控制

**快速使用：**
```vue
<CommonTable
  :search-form="searchForm"
  :filter-list="filterList"
  :table-columns="columns"
  :table-data="data"
  @search="handleSearch"
/>
```

**文档：**
- [示例代码](../src/components/CommonTable/example.vue)

---

### 3. 自定义选择器组件

#### 3.1 EnumSelect - 枚举选择器

从后端动态加载枚举数据的选择器。

```vue
<EnumSelect
  v-model="value"
  enum-name="OpenClosedStatus"
/>
```

#### 3.2 BusinessSelect - 商户选择器

根据商户类型加载商户列表。

```vue
<BusinessSelect
  v-model="businessId"
  :business-type="1"
/>
```

#### 3.3 AgentSelect - 代理选择器

加载代理列表的选择器。

```vue
<AgentSelect
  v-model="agentId"
/>
```

#### 3.4 CoinSelect - 虚拟币选择器

支持链类型选择的虚拟币选择器。

```vue
<CoinSelect
  v-model="coinName"
  :show-chain-select="true"
  :chain-model-value="chainType"
  @update:chain-model-value="chainType = $event"
/>
```

---

## 🎯 使用场景

### 场景1：标准 CRUD 页面

使用 CommonForm + CommonTable 快速实现 CRUD 功能。

```vue
<template>
  <div>
    <!-- 搜索区域 -->
    <CommonForm
      v-model="searchForm"
      :form-items="searchItems"
      :inline="true"
      submit-button-text="搜索"
      @submit="loadData"
    />
    
    <!-- 表格区域 -->
    <CommonTable
      :search-form="searchForm"
      :table-columns="columns"
      :table-data="tableData"
      :total="total"
      :loading="loading"
      @add="handleAdd"
      @search="loadData"
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
  </div>
</template>
```

### 场景2：复杂表单页面

使用 CommonForm 快速构建复杂表单。

```vue
<template>
  <CommonForm
    v-model="form"
    :form-items="formItems"
    :rules="rules"
    :col-span="24"
    @submit="handleSubmit"
  />
</template>

<script setup>
const formItems = [
  { label: '基本信息', prop: 'name', type: 'input', span: 24 },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'Status' },
  { label: '商户', prop: 'business', type: 'business', businessType: 1 },
  { label: '虚拟币', prop: 'coin', type: 'coin', showChainSelect: true, chainProp: 'chain' },
  { label: '日期范围', prop: 'dateRange', type: 'daterange', span: 24 },
  { label: '描述', prop: 'desc', type: 'textarea', rows: 4, span: 24 },
];
</script>
```

### 场景3：搜索筛选

使用 CommonForm 的 inline 模式实现搜索表单。

```vue
<CommonForm
  v-model="searchForm"
  :form-items="searchItems"
  :inline="true"
  :show-reset-button="true"
  submit-button-text="搜索"
  @submit="handleSearch"
/>
```

---

## 🔧 组件架构

```
通用组件系统
│
├── CommonForm (表单组件)
│   ├── 基础表单项 (input, textarea, number...)
│   ├── 日期时间组件 (date, datetime, daterange...)
│   ├── 选择组件 (select, radio, checkbox...)
│   ├── 自定义业务组件
│   │   ├── EnumSelect (枚举选择器)
│   │   ├── BusinessSelect (商户选择器)
│   │   ├── AgentSelect (代理选择器)
│   │   └── CoinSelect (虚拟币选择器)
│   └── 其他组件 (upload, slider, rate...)
│
├── CommonTable (表格组件)
│   ├── 搜索区域
│   │   └── 支持所有 CommonForm 的表单项
│   ├── 表格区域
│   │   ├── 列配置
│   │   ├── 列显示设置
│   │   └── 操作列
│   └── 分页区域
│
└── 自定义选择器 (可独立使用)
    ├── EnumSelect
    ├── BusinessSelect
    ├── AgentSelect
    └── CoinSelect
```

---

## 📚 开发规范

### 1. 组件使用规范

**推荐做法：**
- ✅ 使用配置式开发，减少重复代码
- ✅ 将表单配置抽离到单独的文件或对象
- ✅ 充分利用组件的默认行为和配置
- ✅ 使用 TypeScript 类型提示（如果有）

**不推荐做法：**
- ❌ 不要在组件内部修改 props
- ❌ 不要过度自定义，优先使用组件提供的功能
- ❌ 不要在循环中创建大量组件实例

### 2. 配置文件组织

推荐的文件结构：

```
views/
└── user/
    ├── index.vue           # 页面主文件
    ├── config.js           # 表单和表格配置
    ├── api.js              # API 接口
    └── components/         # 页面专属组件
        └── UserDialog.vue
```

`config.js` 示例：

```javascript
// 搜索表单配置
export const searchFormItems = [
  { label: '关键词', prop: 'keyword', type: 'input' },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'Status' },
];

// 表格列配置
export const tableColumns = [
  { prop: 'id', label: 'ID', width: 80 },
  { prop: 'name', label: '名称', minWidth: 120 },
  { prop: 'status', label: '状态', width: 100, type: 'enum', enumName: 'Status' },
];

// 表单配置
export const formItems = [
  { label: '名称', prop: 'name', type: 'input', required: true },
  { label: '状态', prop: 'status', type: 'enum', enumName: 'Status' },
];

// 验证规则
export const formRules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
};
```

### 3. 表单验证规范

```javascript
// 基础验证
{ required: true, message: '请输入XXX', trigger: 'blur' }

// 类型验证
{ type: 'email', message: '邮箱格式不正确', trigger: 'blur' }
{ type: 'number', message: '必须是数字', trigger: 'blur' }

// 长度验证
{ min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur' }

// 正则验证
{ pattern: /^1[3-9]\d{9}$/, message: '手机号格式不正确', trigger: 'blur' }

// 自定义验证
{
  validator: (rule, value, callback) => {
    if (value < 18) {
      callback(new Error('年龄必须大于18岁'));
    } else {
      callback();
    }
  },
  trigger: 'blur'
}
```

---

## 🎓 最佳实践

### 1. 表单数据初始化

```javascript
// ✅ 推荐：明确初始化所有字段
const form = ref({
  name: '',
  status: null,
  dateRange: [],
  enabled: true,
});

// ❌ 不推荐：不完整的初始化
const form = ref({});
```

### 2. 异步操作处理

```javascript
// ✅ 推荐：使用 loading 状态
const submitLoading = ref(false);

const handleSubmit = async (data) => {
  submitLoading.value = true;
  try {
    await saveData(data);
    ElMessage.success('保存成功');
    dialogVisible.value = false;
  } catch (error) {
    ElMessage.error('保存失败');
  } finally {
    submitLoading.value = false;
  }
};
```

### 3. 表单重置

```javascript
// ✅ 推荐：使用组件提供的方法
const formRef = ref(null);
formRef.value.resetFields();

// 或者在对话框打开时重新赋值
const handleAdd = () => {
  currentRow.value = {
    name: '',
    status: null,
  };
  dialogVisible.value = true;
};
```

### 4. 表单验证

```javascript
// ✅ 推荐：提交前验证
const handleSubmit = async () => {
  const valid = await formRef.value.validate();
  if (!valid) return;
  
  // 执行提交逻辑
  await saveData(form.value);
};
```

---

## 🚀 性能优化建议

1. **使用 v-show 而不是 v-if**：对于频繁切换的字段
2. **懒加载选项数据**：对于不常用的下拉选项
3. **防抖处理**：对于搜索输入框
4. **虚拟滚动**：对于超长列表
5. **按需加载组件**：使用动态 import

---

## 📝 版本历史

### v1.0.0 (2024-01)
- ✨ 新增 CommonForm 通用表单组件
- ✨ 集成自定义选择器组件
- 🎉 支持 20+ 种表单类型
- 📱 响应式布局支持
- ✅ 完整的表单验证

---

## 🔗 相关资源

- [Element Plus 官方文档](https://element-plus.org/)
- [Vue 3 官方文档](https://cn.vuejs.org/)
- [标签页功能文档](./tabs-feature.md)

---

## 💡 常见问题

### Q: 如何全局注册组件？

组件已在 `src/main.js` 中全局注册，可以直接使用。

### Q: 如何扩展新的表单类型？

在 `CommonForm/index.vue` 中添加新的 `v-else-if` 分支即可。

### Q: 自定义选择器如何添加新的 API？

修改对应组件文件，添加新的 API 调用即可。

### Q: 如何处理表单联动？

使用 `change` 回调或 `@change` 事件实现。

### Q: 如何实现表单的深度嵌套？

使用插槽 `slot` 类型，完全自定义嵌套的表单结构。

---

## 📧 技术支持

如有问题或建议，请联系项目维护人员。

