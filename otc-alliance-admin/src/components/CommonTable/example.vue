<template>
  <div class="example-container">
    <h2>CommonTable 使用示例</h2>
    
    <!-- 基本使用示例 -->
    <div class="example-section">
      <h3>1. 基本使用</h3>
      <CommonTable
        :columns="basicColumns"
        :filter-list="basicFilterList"
        :data="tableData"
        :loading="loading"
        @search="handleSearch"
        @add="handleAdd"
        @edit="handleEdit"
        @delete="handleDelete"
      />
    </div>

    <!-- 枚举下拉组件使用示例 -->
    <div class="example-section">
      <h3>2. 枚举下拉组件使用示例</h3>
      <div class="enum-examples">
        <div class="example-item">
          <label>状态选择（单选）：</label>
          <EnumSelect
            v-model="selectedStatus"
            enum-name="OpenClosedStatus"
            placeholder="请选择状态"
            @change="handleStatusChange"
          />
          <span>选中值：{{ selectedStatus }}</span>
        </div>
        
        <div class="example-item">
          <label>币种选择（多选）：</label>
          <EnumSelect
            v-model="selectedCoins"
            enum-name="Coins"
            placeholder="请选择币种"
            :multiple="true"
            :collapse-tags="true"
            @change="handleCoinsChange"
          />
          <span>选中值：{{ selectedCoins }}</span>
        </div>
        
        <div class="example-item">
          <label>自定义宽度：</label>
          <EnumSelect
            v-model="selectedCustom"
            enum-name="OpenClosedStatus"
            placeholder="自定义宽度"
            width="300px"
            @change="handleCustomChange"
          />
          <span>选中值：{{ selectedCustom }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import CommonTable from './index.vue';
import EnumSelect from '../EnumSelect.vue';

// 表格列配置
const basicColumns = [
  {
    prop: 'id',
    label: 'ID',
    width: 80,
    type: 'no'
  },
  {
    prop: 'name',
    label: '名称',
    minWidth: 120
  },
  {
    prop: 'status',
    label: '状态',
    width: 100,
    type: 'enum',
    enumName: 'OpenClosedStatus'
  },
  {
    prop: 'coin',
    label: '币种',
    width: 120,
    type: 'enum',
    enumName: 'Coins'
  },
  {
    prop: 'amount',
    label: '金额',
    width: 120,
    type: 'float'
  },
  {
    prop: 'createTime',
    label: '创建时间',
    width: 180
  },
  {
    label: '操作',
    width: 200,
    type: 'slot',
    slot: 'action'
  }
];

// 搜索表单配置
const basicFilterList = [
  {
    name: 'name',
    label: '名称',
    type: 'text',
    placeholder: '请输入名称'
  },
  {
    name: 'status',
    label: '状态',
    type: 'enum',
    enumName: 'OpenClosedStatus',
    placeholder: '请选择状态'
  },
  {
    name: 'coin',
    label: '币种',
    type: 'enum',
    enumName: 'Coins',
    placeholder: '请选择币种',
    multiple: true
  },
  {
    name: 'createTime',
    label: '创建时间',
    type: 'multDatetime',
    name1: 'startTime',
    name2: 'endTime',
    startPlaceholder: '开始时间',
    endPlaceholder: '结束时间'
  }
];

// 表格数据
const tableData = ref([
  {
    id: 1,
    name: '测试数据1',
    status: 1,
    coin: 'BTC',
    amount: 1234.56,
    createTime: '2024-01-01 10:00:00'
  },
  {
    id: 2,
    name: '测试数据2',
    status: 0,
    coin: 'ETH',
    amount: 5678.90,
    createTime: '2024-01-02 11:00:00'
  },
  {
    id: 3,
    name: '测试数据3',
    status: 1,
    coin: 'USDT',
    amount: 9999.99,
    createTime: '2024-01-03 12:00:00'
  }
]);

// 枚举选择器的值
const selectedStatus = ref('');
const selectedCoins = ref([]);
const selectedCustom = ref('');

const loading = ref(false);

// 事件处理
const handleSearch = (form) => {
  console.log('搜索参数：', form);
  loading.value = true;
  // 模拟搜索
  setTimeout(() => {
    loading.value = false;
  }, 1000);
};

const handleAdd = () => {
  console.log('新增');
};

const handleEdit = (row) => {
  console.log('编辑：', row);
};

const handleDelete = (row) => {
  console.log('删除：', row);
};

const handleStatusChange = (value) => {
  console.log('状态变化：', value);
};

const handleCoinsChange = (value) => {
  console.log('币种变化：', value);
};

const handleCustomChange = (value) => {
  console.log('自定义变化：', value);
};
</script>

<style lang="scss" scoped>
.example-container {
  padding: 20px;
  
  .example-section {
    margin-bottom: 40px;
    
    h3 {
      margin-bottom: 20px;
      color: #333;
    }
  }
  
  .enum-examples {
    .example-item {
      display: flex;
      align-items: center;
      margin-bottom: 15px;
      gap: 10px;
      
      label {
        min-width: 150px;
        font-weight: 500;
      }
      
      span {
        color: #666;
        font-size: 14px;
      }
    }
  }
}
</style>