<template>
  <div class="coin-chain-select">
    <el-select
      v-model="selectedValue"
      :placeholder="placeholder"
      :clearable="clearable"
      :filterable="filterable"
      :disabled="disabled"
      :loading="loading"
      :multiple="multiple"
      :collapse-tags="collapseTags"
      :collapse-tags-tooltip="collapseTagsTooltip"
      :size="size"
      :style="{ width: width }"
      @change="handleChange"
      @clear="handleClear"
      @focus="handleFocus"
      @blur="handleBlur"
    >
      <el-option
        v-for="coin in coinOptions"
        :key="coin.coinCode"
        :label="coin.coinName"
        :value="coin.coinName"
      >
        <div class="coin-option">
          <span class="coin-symbol">{{ coin.coinName }}</span>
        </div>
      </el-option>
    </el-select>
    
    <!-- 链类型下拉框 -->
    <div v-if="showChainSelect" class="chain-select-wrapper">
      <span v-if="chainRequired" class="chain-required-mark">*</span>
      <el-select
        v-model="selectedChainValue"
        :placeholder="chainPlaceholder"
        :clearable="chainClearable"
        :filterable="chainFilterable"
        :disabled="chainDisabled"
        :loading="chainLoading"
        :multiple="chainMultiple"
        :collapse-tags="chainCollapseTags"
        :collapse-tags-tooltip="chainCollapseTagsTooltip"
        :size="chainSize"
        :class="{ 'is-required': chainRequired }"
        :style="{ width: chainWidth }"
        @change="handleChainChange"
        @clear="handleChainClear"
        @focus="handleChainFocus"
        @blur="handleChainBlur"
      >
        <el-option
          v-for="chain in chainOptions"
          :key="chain.chainType"
          :label="chain.chainType"
          :value="chain.chainType"
        >
          <div class="chain-option">
            <span class="chain-symbol">{{ chain.chainType }}</span>
          </div>
        </el-option>
      </el-select>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from "vue";
import { ElMessage } from "element-plus";
import request from "@/utils/request";

const props = defineProps({
  // 双向绑定的值
  modelValue: {
    type: [String, Number, Array],
    default: "",
  },
  // 占位符
  placeholder: {
    type: String,
    default: "请选择虚拟币",
  },
  // 是否可清空
  clearable: {
    type: Boolean,
    default: true,
  },
  // 是否可搜索
  filterable: {
    type: Boolean,
    default: true,
  },
  // 是否禁用
  disabled: {
    type: Boolean,
    default: false,
  },
  // 是否多选
  multiple: {
    type: Boolean,
    default: false,
  },
  // 多选时是否折叠标签
  collapseTags: {
    type: Boolean,
    default: false,
  },
  // 多选时折叠标签是否显示提示
  collapseTagsTooltip: {
    type: Boolean,
    default: false,
  },
  // 尺寸
  size: {
    type: String,
    default: "default",
    validator: (value) => ["large", "default", "small"].includes(value),
  },
  // 宽度
  width: {
    type: String,
    default: "180px",
  },
  // 是否在组件挂载时自动加载数据
  autoLoad: {
    type: Boolean,
    default: true,
  },
  // 是否显示加载状态
  showLoading: {
    type: Boolean,
    default: true,
  },
  // 是否显示链类型选择框
  showChainSelect: {
    type: Boolean,
    default: false,
  },
  // 链类型绑定值
  chainModelValue: {
    type: [String, Number, Array],
    default: "",
  },
  // 链类型占位符
  chainPlaceholder: {
    type: String,
    default: "请选择链类型",
  },
  // 链类型是否可清空
  chainClearable: {
    type: Boolean,
    default: true,
  },
  // 链类型是否可搜索
  chainFilterable: {
    type: Boolean,
    default: true,
  },
  // 链类型是否禁用
  chainDisabled: {
    type: Boolean,
    default: false,
  },
  // 链类型是否多选
  chainMultiple: {
    type: Boolean,
    default: false,
  },
  // 链类型多选时是否折叠标签
  chainCollapseTags: {
    type: Boolean,
    default: false,
  },
  // 链类型多选时折叠标签是否显示提示
  chainCollapseTagsTooltip: {
    type: Boolean,
    default: false,
  },
  // 链类型尺寸
  chainSize: {
    type: String,
    default: "default",
    validator: (value) => ["large", "default", "small"].includes(value),
  },
  // 链类型宽度
  chainWidth: {
    type: String,
    default: "180px",
  },
  // 链类型是否必填
  chainRequired: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits([
  "update:modelValue",
  "change",
  "clear",
  "focus",
  "blur",
  "loaded",
  "update:chainModelValue",
  "chainChange",
  "chainClear",
  "chainFocus",
  "chainBlur",
  "chainLoaded",
]);

const selectedValue = ref(props.modelValue);
const selectedChainValue = ref(props.chainModelValue);
const coinOptions = ref([]);
const chainOptions = ref([]);
const loading = ref(false);
const chainLoading = ref(false);



// 监听 modelValue 变化
watch(
  () => props.modelValue,
  (val) => {
    selectedValue.value = val;
  },
  { immediate: true }
);

// 监听 chainModelValue 变化
watch(
  () => props.chainModelValue,
  (val) => {
    selectedChainValue.value = val;
  },
  { immediate: true }
);

// 监听 selectedValue 变化
watch(selectedValue, (val) => {
  emit("update:modelValue", val);
  // 当币种选择变化时，重新获取链类型数据
  if (props.showChainSelect) {
    fetchChainData(val);
  }
});

// 监听 selectedChainValue 变化
watch(selectedChainValue, (val) => {
  emit("update:chainModelValue", val);
});

// 获取虚拟币数据
const fetchCoinData = async () => {
  if (props.showLoading) {
    loading.value = true;
  }

  try {
    // 使用固定的API接口获取虚拟币数据
    const apiConfig = {
      url: "/api/services/app/Basic/GetAccountAssetsCoinAll",
      method: "get",
      params: {},
    };
    
    const {result} = await request(apiConfig);
    const apiCoins = result || [];
    
    coinOptions.value = apiCoins;
  } catch (error) {
    // 如果API调用失败，使用空数组
    coinOptions.value = [];
    ElMessage.warning("获取虚拟币数据失败");
  } finally {
    loading.value = false;
  }
};

// 获取链类型数据
const fetchChainData = async (coinName = '') => {
  if (!props.showChainSelect) return;
  
  chainLoading.value = true;
  
  try {
    const apiConfig = {
      url: "/api/services/app/Basic/GetCoinChias",
      method: "get",
      params: {
        coinName: coinName || ''
      },
    };
    
    const { result } = await request(apiConfig);
    const apiChains = result || [];
    
    chainOptions.value = apiChains;
    emit("chainLoaded", apiChains);
    
    // 清空链类型选择
    selectedChainValue.value = '';
    emit("update:chainModelValue", '');
  } catch (error) {
    chainOptions.value = [];
    ElMessage.warning("获取链类型数据失败");
  } finally {
    chainLoading.value = false;
  }
};

// 处理选择变化
const handleChange = (value) => {
  emit("change", value);
};

// 处理清空
const handleClear = () => {
  emit("clear");
};

// 处理获得焦点
const handleFocus = (event) => {
  emit("focus", event);
};

// 处理失去焦点
const handleBlur = (event) => {
  emit("blur", event);
};

// 处理链类型选择变化
const handleChainChange = (value) => {
  emit("chainChange", value);
};

// 处理链类型清空
const handleChainClear = () => {
  emit("chainClear");
};

// 处理链类型获得焦点
const handleChainFocus = (event) => {
  emit("chainFocus", event);
};

// 处理链类型失去焦点
const handleChainBlur = (event) => {
  emit("chainBlur", event);
};

// 手动刷新数据
const refresh = () => {
  fetchCoinData();
};

// 手动刷新链类型数据
const refreshChain = () => {
  fetchChainData(selectedValue.value);
};

// 暴露方法给父组件
defineExpose({
  refresh,
  refreshChain,
  coinOptions: computed(() => coinOptions.value),
  chainOptions: computed(() => chainOptions.value),
});

// 组件挂载时自动加载数据
onMounted(() => {
  if (props.autoLoad) {
    fetchCoinData();
  }
  // 如果显示链类型选择框，初始化时获取链类型数据
  if (props.showChainSelect) {
    fetchChainData('');
  }
});
</script>

<style lang="scss" scoped>
.coin-chain-select {
  display: flex;
  align-items: center;
  gap: 10px;
}

.chain-select-wrapper {
  position: relative;
  display: flex;
  align-items: center;

  .chain-required-mark {
    color: #f56c6c;
    font-size: 14px;
    margin-right: 4px;
    line-height: 1;
  }

  .el-select.is-required {
    :deep(.el-input__wrapper) {
      &.is-focus {
        box-shadow: 0 0 0 1px #667eea inset;
      }
    }
  }
}

.coin-option {
  display: flex;
  align-items: center;
  gap: 8px;
  
  .coin-symbol {
    font-weight: bold;
    color: #409eff;
    min-width: 40px;
  }
  
  .coin-name {
    color: #606266;
  }
}

.chain-option {
  display: flex;
  align-items: center;
  gap: 8px;
  
  .chain-symbol {
    font-weight: bold;
    color: #67c23a;
    min-width: 40px;
  }
}

// 自定义下拉选项样式
:deep(.el-select-dropdown__item) {
  padding: 8px 20px;
  
  &:hover {
    background-color: #f5f7fa;
  }
  
  &.is-selected {
    background-color: #ecf5ff;
    color: #409eff;
  }
}
</style>
