# CommonForm 通用表单组件使用指南

## 简介

`CommonForm` 是一个高度可配置的通用表单组件，支持多种表单项类型，包括自定义选择器组件（EnumSelect、BusinessSelect、AgentSelect、CoinSelect），可以大幅减少表单开发的重复代码。

## 特性

- ✅ 支持 20+ 种表单项类型
- ✅ 集成自定义选择器组件
- ✅ 支持插槽自定义
- ✅ 支持表单验证
- ✅ 支持响应式布局（24栅格系统）
- ✅ 支持行内表单
- ✅ 支持禁用状态
- ✅ 支持 v-model 双向绑定
- ✅ 完善的事件系统

## 安装使用

### 基本使用

```vue
<template>
  <CommonForm
    v-model="formData"
    :form-items="formItems"
    :rules="rules"
    @submit="handleSubmit"
  />
</template>

<script setup>
import { ref } from 'vue';
import CommonForm from '@/components/CommonForm/index.vue';

const formData = ref({
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
    { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' },
  ],
};

const handleSubmit = (data) => {
  console.log('提交数据：', data);
};
</script>
```

## Props 属性

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| modelValue | Object | required | 表单数据对象（v-model） |
| formItems | Array | required | 表单项配置数组 |
| rules | Object | {} | 表单验证规则 |
| labelWidth | String | '100px' | 标签宽度 |
| labelPosition | String | 'right' | 标签位置（left/right/top） |
| inline | Boolean | false | 是否为行内表单 |
| size | String | 'default' | 表单尺寸（large/default/small） |
| disabled | Boolean | false | 是否禁用表单 |
| gutter | Number | 20 | 列间距 |
| colSpan | Number | 12 | 默认列跨度（24栅格） |
| showButtons | Boolean | true | 是否显示操作按钮 |
| showSubmitButton | Boolean | true | 是否显示提交按钮 |
| showResetButton | Boolean | true | 是否显示重置按钮 |
| showCancelButton | Boolean | false | 是否显示取消按钮 |
| submitButtonText | String | '提交' | 提交按钮文本 |
| resetButtonText | String | '重置' | 重置按钮文本 |
| cancelButtonText | String | '取消' | 取消按钮文本 |
| submitLoading | Boolean | false | 提交按钮加载状态 |

## FormItem 配置

### 通用配置

每个表单项都支持以下通用配置：

| 属性名 | 类型 | 说明 |
|--------|------|------|
| label | String | 标签文本 |
| prop | String | 字段名称（对应 formData 的 key） |
| type | String | 表单项类型（见下方类型列表） |
| required | Boolean | 是否必填 |
| rules | Array | 表单项验证规则 |
| hidden | Boolean | 是否隐藏 |
| disabled | Boolean | 是否禁用 |
| placeholder | String | 占位符 |
| span | Number | 列跨度（0-24） |
| xs/sm/md/lg/xl | Number | 响应式栅格 |
| class | String | 自定义类名 |
| labelWidth | String | 标签宽度（覆盖全局） |

### 表单项类型

#### 1. input（文本输入框）

```javascript
{
  label: '姓名',
  prop: 'name',
  type: 'input',
  placeholder: '请输入姓名',
  clearable: true,
  maxlength: 20,
  showWordLimit: true,
  prefixIcon: 'User',
  suffixIcon: 'Close',
}
```

#### 2. number（数字输入框）

```javascript
{
  label: '年龄',
  prop: 'age',
  type: 'number',
  min: 0,
  max: 150,
  step: 1,
  precision: 0,
  controls: true,
  controlsPosition: 'right',
}
```

#### 3. textarea（文本域）

```javascript
{
  label: '描述',
  prop: 'description',
  type: 'textarea',
  rows: 4,
  autosize: { minRows: 2, maxRows: 6 },
  maxlength: 200,
  showWordLimit: true,
}
```

#### 4. select（下拉选择）

```javascript
{
  label: '分类',
  prop: 'category',
  type: 'select',
  options: [
    { label: '选项1', value: 1 },
    { label: '选项2', value: 2 },
  ],
  filterable: true,
  multiple: false,
  collapseTags: false,
}
```

#### 5. enum（枚举选择器）

```javascript
{
  label: '状态',
  prop: 'status',
  type: 'enum',
  enumName: 'OpenClosedStatus', // 枚举名称
  filterable: true,
  multiple: false,
}
```

#### 6. business（商户选择器）

```javascript
{
  label: '商户',
  prop: 'businessId',
  type: 'business',
  businessType: 1, // 商户类型
  valueKey: 'id', // 返回值字段
  filterable: true,
  multiple: false,
}
```

#### 7. agent（代理选择器）

```javascript
{
  label: '代理',
  prop: 'agentId',
  type: 'agent',
  valueKey: 'id',
  filterable: true,
  multiple: false,
}
```

#### 8. coin（虚拟币选择器）

```javascript
{
  label: '虚拟币',
  prop: 'coinName',
  type: 'coin',
  showChainSelect: true, // 显示链类型选择
  chainProp: 'chainType', // 链类型字段名
  chainPlaceholder: '请选择链类型',
  filterable: true,
}
```

#### 9. date（日期选择器）

```javascript
{
  label: '日期',
  prop: 'date',
  type: 'date',
  dateType: 'date', // date/week/month/year
  format: 'YYYY-MM-DD',
  valueFormat: 'YYYY-MM-DD',
}
```

#### 10. time（时间选择器）

```javascript
{
  label: '时间',
  prop: 'time',
  type: 'time',
  format: 'HH:mm:ss',
  valueFormat: 'HH:mm:ss',
}
```

#### 11. datetime（日期时间选择器）

```javascript
{
  label: '日期时间',
  prop: 'datetime',
  type: 'datetime',
  format: 'YYYY-MM-DD HH:mm:ss',
  valueFormat: 'YYYY-MM-DD HH:mm:ss',
}
```

#### 12. daterange（日期范围选择器）

```javascript
{
  label: '日期范围',
  prop: 'dateRange',
  type: 'daterange',
  rangeSeparator: '至',
  startPlaceholder: '开始日期',
  endPlaceholder: '结束日期',
}
```

#### 13. timerange（时间范围选择器）

```javascript
{
  label: '时间范围',
  prop: 'timeRange',
  type: 'timerange',
  rangeSeparator: '至',
  startPlaceholder: '开始时间',
  endPlaceholder: '结束时间',
}
```

#### 14. datetimerange（日期时间范围选择器）

```javascript
{
  label: '日期时间范围',
  prop: 'datetimeRange',
  type: 'datetimerange',
  rangeSeparator: '至',
  startPlaceholder: '开始时间',
  endPlaceholder: '结束时间',
}
```

#### 15. switch（开关）

```javascript
{
  label: '启用',
  prop: 'enabled',
  type: 'switch',
  activeText: '启用',
  inactiveText: '禁用',
  activeValue: true,
  inactiveValue: false,
}
```

#### 16. radio（单选框）

```javascript
{
  label: '性别',
  prop: 'gender',
  type: 'radio',
  options: [
    { label: '男', value: 1 },
    { label: '女', value: 2 },
  ],
}
```

#### 17. checkbox（复选框组）

```javascript
{
  label: '爱好',
  prop: 'hobbies',
  type: 'checkbox',
  options: [
    { label: '篮球', value: 'basketball' },
    { label: '足球', value: 'football' },
  ],
}
```

#### 18. upload（上传）

```javascript
{
  label: '附件',
  prop: 'files',
  type: 'upload',
  action: '/api/upload',
  headers: { Authorization: 'Bearer token' },
  data: { userId: 1 },
  listType: 'picture-card', // text/picture/picture-card
  multiple: true,
  limit: 5,
  accept: 'image/*',
  tip: '只能上传jpg/png文件，且不超过2MB',
  onSuccess: (response, file, fileList) => {
    console.log('上传成功', response);
  },
}
```

#### 19. cascader（级联选择器）

```javascript
{
  label: '地区',
  prop: 'region',
  type: 'cascader',
  options: [
    {
      value: 'zhejiang',
      label: '浙江',
      children: [
        { value: 'hangzhou', label: '杭州' },
      ],
    },
  ],
  cascaderProps: {
    expandTrigger: 'hover',
    checkStrictly: false,
  },
}
```

#### 20. slider（滑块）

```javascript
{
  label: '进度',
  prop: 'progress',
  type: 'slider',
  min: 0,
  max: 100,
  step: 1,
  showInput: true,
  range: false,
}
```

#### 21. rate（评分）

```javascript
{
  label: '评分',
  prop: 'rating',
  type: 'rate',
  max: 5,
  allowHalf: true,
  showText: true,
  showScore: true,
}
```

#### 22. color（颜色选择器）

```javascript
{
  label: '颜色',
  prop: 'color',
  type: 'color',
  showAlpha: false,
}
```

#### 23. slot（插槽自定义）

```javascript
{
  label: '自定义',
  prop: 'custom',
  type: 'slot',
  slotName: 'customField',
  span: 24,
}
```

在模板中使用：

```vue
<CommonForm v-model="formData" :form-items="formItems">
  <template #customField="{ form, item }">
    <!-- 自定义内容 -->
    <div>{{ form.custom }}</div>
  </template>
</CommonForm>
```

## 事件

| 事件名 | 参数 | 说明 |
|--------|------|------|
| submit | formData | 提交表单 |
| reset | - | 重置表单 |
| cancel | - | 取消操作 |
| change | prop, value, formData | 字段值变化 |
| blur | prop, event, formData | 字段失焦 |
| focus | prop, event, formData | 字段获焦 |
| clear | prop, formData | 清空字段 |
| chainChange | prop, value, formData | 链类型变化 |
| uploadSuccess | prop, response, file, fileList | 上传成功 |
| uploadError | prop, error, file, fileList | 上传失败 |
| uploadRemove | prop, file, fileList | 移除文件 |
| uploadChange | prop, file, fileList | 文件状态改变 |

## 方法

通过 `ref` 可以调用以下方法：

```vue
<template>
  <CommonForm ref="formRef" v-model="formData" :form-items="formItems" />
  <el-button @click="validate">验证</el-button>
</template>

<script setup>
import { ref } from 'vue';

const formRef = ref(null);

// 验证整个表单
const validate = async () => {
  const valid = await formRef.value.validate();
  console.log('验证结果：', valid);
};

// 验证指定字段
const validateField = (prop) => {
  formRef.value.validateField(prop);
};

// 重置表单
const resetFields = () => {
  formRef.value.resetFields();
};

// 清除验证
const clearValidate = (props) => {
  formRef.value.clearValidate(props);
};
</script>
```

## 高级用法

### 1. 响应式布局

```javascript
const formItems = [
  {
    label: '姓名',
    prop: 'name',
    type: 'input',
    xs: 24,  // 手机端占满
    sm: 12,  // 平板占一半
    md: 8,   // 桌面占1/3
    lg: 6,   // 大屏占1/4
  },
];
```

### 2. 动态表单项

```javascript
const formItems = computed(() => {
  const items = [
    { label: '类型', prop: 'type', type: 'select', options: typeOptions },
  ];
  
  // 根据类型显示不同的字段
  if (formData.value.type === 1) {
    items.push({
      label: '选项A',
      prop: 'optionA',
      type: 'input',
    });
  } else {
    items.push({
      label: '选项B',
      prop: 'optionB',
      type: 'input',
    });
  }
  
  return items;
});
```

### 3. 表单项联动

```javascript
{
  label: '省份',
  prop: 'province',
  type: 'select',
  options: provinceOptions,
  change: (value, formData) => {
    // 省份变化时，重新加载城市列表
    formData.city = '';
    loadCities(value);
  },
}
```

### 4. 自定义按钮

```vue
<CommonForm v-model="formData" :form-items="formItems">
  <template #buttons="{ form, validate }">
    <el-button type="primary" @click="handleCustomSubmit(validate)">
      自定义提交
    </el-button>
    <el-button @click="handleCustomAction">
      自定义操作
    </el-button>
  </template>
</CommonForm>
```

### 5. 对话框中使用

```vue
<el-dialog v-model="dialogVisible" title="编辑">
  <CommonForm
    v-model="formData"
    :form-items="formItems"
    :rules="rules"
    :submit-loading="loading"
    :show-cancel-button="true"
    @submit="handleSubmit"
    @cancel="dialogVisible = false"
  />
</el-dialog>
```

## 完整示例

查看 `src/components/CommonForm/example.vue` 获取更多完整示例。

## 最佳实践

1. **表单配置分离**：将 `formItems` 和 `rules` 配置抽离到单独的文件或配置对象中
2. **复用性**：对于常用的表单项组合，可以封装成配置函数
3. **验证规则**：充分利用 Element Plus 的验证规则，避免重复编写
4. **性能优化**：对于大型表单，使用 `v-show` 而不是 `v-if` 来切换字段可见性
5. **错误处理**：在提交时做好错误处理和用户提示

## 注意事项

1. `prop` 字段必须与 `formData` 中的 key 对应
2. 使用自定义选择器时，确保对应的 API 接口可用
3. 上传组件需要配置正确的 `action` 地址
4. 日期选择器的 `valueFormat` 应与后端接口要求的格式一致
5. 表单验证规则要与实际业务需求匹配

## 浏览器兼容性

支持所有现代浏览器：
- Chrome/Edge (最新版本)
- Firefox (最新版本)
- Safari (最新版本)

