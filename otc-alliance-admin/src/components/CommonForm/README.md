# CommonForm 通用表单组件

一个功能强大、高度可配置的 Vue 3 表单组件，支持 20+ 种表单类型，集成自定义业务组件。

## ✨ 特性

- 🚀 **开箱即用**：配置式开发，无需重复编写表单代码
- 🎨 **20+ 表单类型**：支持所有常用表单组件
- 🔌 **自定义组件集成**：内置 EnumSelect、BusinessSelect、AgentSelect、CoinSelect
- 📱 **响应式布局**：基于 24 栅格系统，支持响应式设计
- ✅ **表单验证**：完整支持 Element Plus 验证规则
- 🎯 **插槽支持**：灵活的插槽系统，支持高度自定义
- ⚡ **TypeScript**：完整的类型提示（计划中）
- 📦 **零依赖**：仅依赖 Element Plus 和 Vue 3

## 📦 安装

组件已内置在项目中，直接导入使用：

```javascript
import CommonForm from '@/components/CommonForm/index.vue';
```

## 🚀 快速开始

```vue
<template>
  <CommonForm
    v-model="form"
    :form-items="formItems"
    :rules="rules"
    @submit="handleSubmit"
  />
</template>

<script setup>
import { ref } from 'vue';
import CommonForm from '@/components/CommonForm/index.vue';

const form = ref({
  name: '',
  email: '',
});

const formItems = [
  {
    label: '姓名',
    prop: 'name',
    type: 'input',
    required: true,
  },
  {
    label: '邮箱',
    prop: 'email',
    type: 'input',
    required: true,
  },
];

const rules = {
  name: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '邮箱格式不正确', trigger: 'blur' },
  ],
};

const handleSubmit = (data) => {
  console.log('提交数据：', data);
};
</script>
```

## 📖 文档

- [快速上手](../../../docs/common-form-quick-start.md) - 5分钟快速入门
- [完整指南](../../../docs/common-form-guide.md) - 详细的 API 文档和使用说明
- [示例代码](./example.vue) - 丰富的使用示例

## 🎯 支持的表单类型

### 基础类型
- `input` - 文本输入框
- `number` - 数字输入框
- `textarea` - 文本域
- `select` - 下拉选择

### 自定义选择器
- `enum` - 枚举选择器
- `business` - 商户选择器
- `agent` - 代理选择器
- `coin` - 虚拟币选择器（支持链类型）

### 日期时间
- `date` - 日期选择器
- `time` - 时间选择器
- `datetime` - 日期时间选择器
- `daterange` - 日期范围选择器
- `timerange` - 时间范围选择器
- `datetimerange` - 日期时间范围选择器

### 其他类型
- `switch` - 开关
- `radio` - 单选框
- `checkbox` - 复选框组
- `upload` - 文件上传
- `cascader` - 级联选择器
- `slider` - 滑块
- `rate` - 评分
- `color` - 颜色选择器
- `slot` - 自定义插槽

## 🌟 亮点功能

### 1. 自定义业务组件集成

无缝集成项目中的自定义选择器组件：

```javascript
// 枚举选择器
{ label: '状态', prop: 'status', type: 'enum', enumName: 'OpenClosedStatus' }

// 商户选择器
{ label: '商户', prop: 'businessId', type: 'business', businessType: 1 }

// 代理选择器
{ label: '代理', prop: 'agentId', type: 'agent' }

// 虚拟币选择器（带链类型）
{
  label: '虚拟币',
  prop: 'coinName',
  type: 'coin',
  showChainSelect: true,
  chainProp: 'chainType',
}
```

### 2. 响应式布局

基于 24 栅格系统，支持不同屏幕尺寸：

```javascript
{
  label: '字段名',
  prop: 'field',
  type: 'input',
  xs: 24,  // 手机端占满
  sm: 12,  // 平板占一半
  md: 8,   // 桌面占1/3
  lg: 6,   // 大屏占1/4
}
```

### 3. 表单验证

完整支持 Element Plus 验证规则：

```javascript
const rules = {
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '邮箱格式不正确', trigger: 'blur' },
  ],
  age: [
    { required: true, message: '请输入年龄', trigger: 'blur' },
    { type: 'number', min: 0, max: 150, message: '年龄范围0-150', trigger: 'blur' },
  ],
};
```

### 4. 插槽自定义

灵活的插槽系统，支持高度自定义：

```vue
<CommonForm v-model="form" :form-items="items">
  <template #customField="{ form, item }">
    <!-- 完全自定义的内容 -->
    <el-input v-model="form.custom" placeholder="自定义输入" />
  </template>
  
  <template #buttons="{ validate }">
    <!-- 自定义按钮区域 -->
    <el-button type="primary" @click="customSubmit(validate)">
      自定义提交
    </el-button>
  </template>
</CommonForm>
```

### 5. 表单联动

支持字段联动和动态表单：

```javascript
{
  label: '省份',
  prop: 'province',
  type: 'select',
  options: provinces,
  change: (value, formData) => {
    // 省份变化时清空城市
    formData.city = '';
    // 重新加载城市列表
    loadCities(value);
  },
}
```

## 🔧 API

### Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| modelValue | Object | required | 表单数据对象 |
| formItems | Array | required | 表单项配置 |
| rules | Object | {} | 验证规则 |
| labelWidth | String | '100px' | 标签宽度 |
| size | String | 'default' | 表单尺寸 |
| inline | Boolean | false | 行内表单 |
| disabled | Boolean | false | 禁用表单 |

更多属性请查看[完整文档](../../../docs/common-form-guide.md)

### Events

| 事件 | 参数 | 说明 |
|------|------|------|
| submit | formData | 提交表单 |
| reset | - | 重置表单 |
| cancel | - | 取消操作 |
| change | prop, value, formData | 字段变化 |

更多事件请查看[完整文档](../../../docs/common-form-guide.md)

### Methods

| 方法 | 参数 | 说明 |
|------|------|------|
| validate | - | 验证表单 |
| validateField | prop | 验证指定字段 |
| resetFields | - | 重置表单 |
| clearValidate | props | 清除验证 |

## 🎬 示例

### 对话框表单

```vue
<el-dialog v-model="visible" title="编辑">
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
```

### 搜索表单

```vue
<CommonForm
  v-model="searchForm"
  :form-items="searchItems"
  :inline="true"
  submit-button-text="搜索"
  @submit="handleSearch"
/>
```

### 与 CommonTable 配合

完美配合 CommonTable 组件使用，实现完整的 CRUD 功能。

查看[完整示例](./example.vue)了解更多用法。

## 🤝 兼容性

- Vue 3.x
- Element Plus 2.x
- 现代浏览器（Chrome、Firefox、Safari、Edge）

## 📝 更新日志

### v1.0.0 (2024-01)
- ✨ 首次发布
- 🎉 支持 20+ 种表单类型
- 🔌 集成自定义业务组件
- 📱 响应式布局支持
- ✅ 完整的表单验证
- 🎯 插槽自定义支持

## 📄 许可证

MIT

## 🙏 致谢

本组件基于 [Element Plus](https://element-plus.org/) 构建，感谢 Element Plus 团队的优秀工作。

