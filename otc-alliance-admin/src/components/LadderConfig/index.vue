<template>
  <div class="ladder-config-container">
    <el-alert
      title="阶梯费率配置"
      type="info"
      :closable="false"
      show-icon
      style="margin-bottom: 20px"
    >
      <template #default>
        配置不同金额区间的费率标准，支持多层级阶梯
      </template>
    </el-alert>
    
    <div v-for="(ladder, index) in ladderList" :key="ladder.index || index" class="ladder-item">
      <div class="ladder-header">
        <div class="ladder-number">
          <span class="number-badge">{{ index + 1 }}</span>
          <span class="ladder-title">阶梯 {{ index + 1}}</span>
        </div>
        <el-button 
          v-if="ladderList.length > 1" 
          type="danger" 
          size="small"
          text
          :icon="Delete" 
          @click="handleDelete(index)"
        >
          删除
        </el-button>
      </div>

      <!-- 阶梯范围 -->
      <div class="range-section">
        <div class="section-label">
          <el-icon><Money /></el-icon>
          <span>金额区间</span>
        </div>
        <el-row :gutter="16">
          <el-col :span="11">
            <el-form-item label="起始金额" label-width="80px">
              <el-input-number 
                v-model="ladder.startAmount" 
                :min="0" 
                :precision="2"
                placeholder="0.00"
                :controls="false"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="2" class="range-separator">
            <div class="separator-icon">→</div>
          </el-col>
          <el-col :span="11">
            <el-form-item label="结束金额" label-width="80px">
              <el-input-number 
                v-model="ladder.endAmount" 
                :min="0" 
                :precision="2"
                placeholder="0.00"
                :controls="false"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
      </div>

      <!-- 计费规则 -->
      <div class="rule-section">
        <div class="section-label">
          <el-icon><Setting /></el-icon>
          <span>计费配置</span>
        </div>
        <el-form-item label="计费规则" label-width="80px">
          <el-select 
            v-model="ladder.cashBillingRulesConfigId" 
            placeholder="请选择计费规则"
            clearable
            style="width: 100%"
          >
            <el-option 
              v-for="item in billingRuleOptions" 
              :key="item.id" 
              :label="item.name" 
              :value="item.id" 
            />
          </el-select>
        </el-form-item>
      </div>

      <!-- 费用配置 -->
      <div class="fee-section">
        <div class="section-label">
          <el-icon><Wallet /></el-icon>
          <span>费用设置</span>
        </div>
        
        <!-- 固定费率 -->
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="固定费率" label-width="80px">
              <el-input-number 
                v-model="ladder.cost" 
                :min="0" 
                :precision="2"
                placeholder="0.00"
                :controls="false"
                style="width: 100%"
              >
                <template #suffix>
                  <span style="color: #909399;">%</span>
                </template>
              </el-input-number>
            </el-form-item>
          </el-col>
        </el-row>

        <!-- 固定金额 -->
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="固定金额" label-width="80px">
              <el-input-number 
                v-model="ladder.fixedCost" 
                :min="0" 
                :precision="2"
                placeholder="0.00"
                :controls="false"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="计价币种" label-width="80px">
              <el-select 
                v-model="ladder.fixedCostCurrency" 
                placeholder="请选择币种"
                filterable 
                clearable
                style="width: 100%"
              >
                <el-option 
                  v-for="item in coinOptions" 
                  :key="item.coinCode" 
                  :label="item.coinName" 
                  :value="item.coinCode" 
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <!-- 税率（可选） -->
        <el-row :gutter="16" v-if="showTaxRate">
          <el-col :span="12">
            <el-form-item label="税率" label-width="80px">
              <el-input-number 
                v-model="ladder.taxRate" 
                :min="0" 
                :precision="2"
                placeholder="0.00"
                :controls="false"
                style="width: 100%"
              >
                <template #suffix>
                  <span style="color: #909399;">%</span>
                </template>
              </el-input-number>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="计价币种" label-width="80px">
              <el-select 
                v-model="ladder.taxCurrency" 
                placeholder="请选择币种"
                filterable 
                clearable
                style="width: 100%"
              >
                <el-option 
                  v-for="item in coinOptions" 
                  :key="item.coinCode" 
                  :label="item.coinName" 
                  :value="item.coinCode" 
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
      </div>
    </div>

    <!-- 添加阶梯按钮 -->
    <div class="add-ladder-btn">
      <el-button 
        type="primary" 
        :icon="Plus"
        @click="handleAdd"
        plain
      >
        添加新阶梯
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Plus, Delete, Money, Setting, Wallet } from '@element-plus/icons-vue'

const props = defineProps({
  // 阶梯数据
  modelValue: {
    type: Array,
    default: () => []
  },
  // 计费规则选项
  billingRuleOptions: {
    type: Array,
    default: () => []
  },
  // 币种选项
  coinOptions: {
    type: Array,
    default: () => []
  },
  // 是否显示税率
  showTaxRate: {
    type: Boolean,
    default: false
  },
  // 最少阶梯数量
  minLadders: {
    type: Number,
    default: 1
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

// 本地阶梯列表
const ladderList = ref([])

// 初始化数据
const initLadderList = () => {
  if (props.modelValue && props.modelValue.length > 0) {
    ladderList.value = props.modelValue.map((item, index) => ({
      ...item,
      index: index
    }))
  } else {
    ladderList.value = [createEmptyLadder(0)]
  }
}

// 创建空阶梯
const createEmptyLadder = (index) => {
  return {
    startAmount: 0,
    endAmount: 0,
    cost: 0,
    fixedCost: 0,
    taxRate: 0,
    index: index,
    cashBillingRulesConfigId: '',
    costCurrency: '',
    fixedCostCurrency: '',
    taxCurrency: ''
  }
}

// 添加阶梯
const handleAdd = () => {
  const newIndex = ladderList.value.length
  ladderList.value.push(createEmptyLadder(newIndex))
  updateModelValue()
}

// 删除阶梯
const handleDelete = (index) => {
  if (ladderList.value.length <= props.minLadders) {
    return
  }
  
  ladderList.value.splice(index, 1)
  
  // 重新设置索引
  ladderList.value.forEach((item, i) => {
    item.index = i
  })
  
  updateModelValue()
}

// 更新父组件数据
const updateModelValue = () => {
  emit('update:modelValue', ladderList.value)
  emit('change', ladderList.value)
}

// 监听数据变化
watch(() => props.modelValue, (newVal) => {
  if (JSON.stringify(newVal) !== JSON.stringify(ladderList.value)) {
    initLadderList()
  }
}, { deep: true })

// 监听表单项变化
watch(ladderList, () => {
  updateModelValue()
}, { deep: true })

// 初始化
initLadderList()
</script>

<style lang="scss" scoped>
.ladder-config-container {
  width: 100%;
  padding: 0;

  .ladder-item {
    position: relative;
    margin-bottom: 24px;
    padding: 20px;
    background: linear-gradient(135deg, #f5f7fa 0%, #ffffff 100%);
    border: 2px solid #e4e7ed;
    border-radius: 12px;
    transition: all 0.3s ease;

    &:hover {
      border-color: #409eff;
      box-shadow: 0 4px 16px rgba(64, 158, 255, 0.1);
      transform: translateY(-2px);
    }

    .ladder-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding-bottom: 12px;
      border-bottom: 2px dashed #e4e7ed;

      .ladder-number {
        display: flex;
        align-items: center;
        gap: 12px;

        .number-badge {
          display: inline-flex;
          align-items: center;
          justify-content: center;
          width: 32px;
          height: 32px;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: #fff;
          font-weight: 600;
          font-size: 16px;
          border-radius: 50%;
          box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
        }

        .ladder-title {
          font-size: 16px;
          font-weight: 600;
          color: #303133;
        }
      }
    }

    .range-section,
    .rule-section,
    .fee-section {
      margin-bottom: 16px;
      padding: 16px;
      background: #fff;
      border-radius: 8px;
      border: 1px solid #f0f0f0;

      .section-label {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 12px;
        font-size: 14px;
        font-weight: 600;
        color: #409eff;

        .el-icon {
          font-size: 16px;
        }
      }
    }

    .range-separator {
      display: flex;
      align-items: center;
      justify-content: center;
      padding-top: 32px;

      .separator-icon {
        font-size: 24px;
        font-weight: bold;
        color: #409eff;
      }
    }

    :deep(.el-form-item) {
      margin-bottom: 16px;

      .el-form-item__label {
        font-weight: 500;
        color: #606266;
        font-size: 13px;
      }
    }
  }

  .add-ladder-btn {
    display: flex;
    justify-content: center;
    margin-top: 20px;

    .el-button {
      min-width: 160px;
      height: 40px;
      font-size: 14px;
      font-weight: 500;
      border-radius: 20px;
      box-shadow: 0 2px 8px rgba(64, 158, 255, 0.2);
      transition: all 0.3s ease;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 16px rgba(64, 158, 255, 0.3);
      }

      &:active {
        transform: translateY(0);
      }
    }
  }

  :deep(.el-input-number) {
    width: 100%;

    .el-input__inner {
      text-align: left;
    }

    &.is-controls-right .el-input__inner {
      padding-left: 15px;
      padding-right: 50px;
    }
  }

  :deep(.el-alert) {
    border-radius: 8px;

    .el-alert__title {
      font-weight: 600;
      font-size: 15px;
    }

    .el-alert__description {
      font-size: 13px;
      margin-top: 4px;
    }
  }
}

// 响应式布局
@media (max-width: 768px) {
  .ladder-config-container {
    .ladder-item {
      padding: 16px;

      .ladder-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;

        .el-button {
          width: 100%;
        }
      }

      .range-separator {
        padding-top: 0;
        padding-bottom: 8px;

        .separator-icon {
          transform: rotate(90deg);
        }
      }
    }

    .add-ladder-btn {
      .el-button {
        width: 100%;
      }
    }
  }
}
</style>

