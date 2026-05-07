<template>
  <el-dialog
    v-model="dialogVisible"
    title="选择收款国家/地区"
    width="1000px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <div class="area-selector-container">
      <div class="selector-layout">
        <!-- 左侧：可选国家/地区 -->
        <div class="candidate-panel">
          <div class="panel-header">
            <div class="header-title">
              <span>可选国家/地区</span>
            </div>
            <div class="header-actions">
              <el-radio-group v-model="codeType" size="small" @change="handleCodeTypeChange">
                <el-radio-button :value="0">CCA2</el-radio-button>
                <el-radio-button :value="1">CCA3</el-radio-button>
                <el-radio-button :value="2">CCN3</el-radio-button>
              </el-radio-group>
            </div>
          </div>
          
          <div class="panel-body">
            <!-- 搜索框 -->
            <div class="search-box">
              <el-input
                v-model="filterText"
                placeholder="输入关键字进行过滤"
                clearable
                @input="handleFilter"
              >
                <template #prefix>
                  <el-icon><Search /></el-icon>
                </template>
              </el-input>
            </div>

            <!-- 树形选择 -->
            <div class="tree-wrapper">
              <el-tree
                ref="treeRef"
                :data="treeData"
                :props="treeProps"
                show-checkbox
                node-key="key"
                :default-expand-all="false"
                :default-expanded-keys="defaultExpandedKeys"
                :expand-on-click-node="false"
                :filter-node-method="filterNode"
                :check-strictly="checkStrictly"
                :check-on-click-node="true"
                @check="handleTreeCheck"
                class="country-tree"
              >
                <template #default="{ node, data }">
                  <span class="tree-node-label">
                    {{ data.label }}
                  </span>
                </template>
              </el-tree>
            </div>
          </div>
        </div>

        <!-- 右侧：已选 -->
        <div class="selected-panel">
          <div class="panel-header">
            <div class="header-title">
              <span>已选（{{ selectedLeafNodes.length }}）</span>
            </div>
            <div class="header-actions">
              <el-button text type="danger" @click="clearAll" :disabled="selectedLeafNodes.length === 0">
                清空
              </el-button>
            </div>
          </div>
          
          <div class="panel-body">
            <div class="selected-list">
              <el-empty 
                v-if="selectedLeafNodes.length === 0" 
                description="暂未选择任何国家"
                :image-size="80"
              />
              <div
                v-for="item in selectedLeafNodes"
                :key="item.key"
                class="selected-item"
              >
                <div class="item-info">
                  <span class="item-name">{{ item.cnName }}</span>
                  <span class="item-code">{{ getCodeSuffix(item) }}</span>
                </div>
                <el-button
                  text
                  type="danger"
                  @click="removeItem(item)"
                  class="remove-btn"
                >
                  <el-icon><Close /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleConfirm">
          确定
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue';
import { ElMessage } from 'element-plus';
import { Search, Close } from '@element-plus/icons-vue';
import request from '@/utils/request';

const emit = defineEmits(['confirm', 'areaSave']);

const dialogVisible = ref(false);
const filterText = ref('');
const codeType = ref(0); // 0: CCA2, 1: CCA3, 2: CCN3
const rawList = ref([]);
const treeData = ref([]);
const treeRef = ref(null);
const checkedKeys = ref([]);
const selectedLeafNodes = ref([]);
const allLeafKeys = ref([]);
const checkStrictly = ref(false);
const selectAllMode = ref(false);
const excludedKeys = ref(new Set());
const keyToNode = ref(new Map());
const isProgrammaticChange = ref(false);
const selectAllOriginalKeys = ref([]);
const defaultExpandedKeys = ref([]);

const treeProps = {
  label: 'label',
  children: 'children',
};

// 获取国家数据的 API
const CountryInfoGetAllForSelectItem = {
  url: '/api/services/app/CountryInfo/GetAllForSelectItem',
  method: 'get',
  params: {},
};

// 获取数据
const fetchData = async () => {
  try {
    const { result } = await request(CountryInfoGetAllForSelectItem);
    rawList.value = Array.isArray(result) ? result : [];
  } catch (error) {
    ElMessage.error('获取国家数据失败');
    rawList.value = [];
  }
};

// 根据类型获取编码
const getCodeByType = (item) => {
  if (codeType.value === 1) return item.ccA3 || null;
  if (codeType.value === 2) return item.ccN3 || null;
  return item.ccA2 || null;
};

// 获取编码后缀
const getCodeSuffix = (node) => {
  const codeTypeMap = { 0: 'CCA2', 1: 'CCA3', 2: 'CCN3' };
  const codeTypeStr = codeTypeMap[codeType.value];
  const code = node[codeTypeStr];
  return code ? `(${code})` : '';
};

// 组合国家标签
const composeCountryLabel = (item) => {
  const name = item.cnName || item.name;
  const code = getCodeByType(item);
  return code ? `${name} (${code})` : name;
};

// 构建树形数据
const buildTree = () => {
  const regionMap = new Map();
  const subRegionMap = new Map();
  let countryCounter = 0;

  for (const item of rawList.value) {
    const region = item.cnRegion || item.region || '其他';
    const subRegion = item.cnSubRegion || item.subRegion || '其他';

    // 创建区域节点
    if (!regionMap.has(region)) {
      const regionNode = {
        key: `region:${region}`,
        type: 'region',
        region,
        label: region,
        children: [],
      };
      regionMap.set(region, regionNode);
    }

    const regionNode = regionMap.get(region);
    const subKey = `${region}|${subRegion}`;

    // 创建子区域节点
    if (!subRegionMap.has(subKey)) {
      const subRegionNode = {
        key: `sub:${subKey}`,
        type: 'subRegion',
        region,
        subRegion,
        label: subRegion,
        children: [],
      };
      subRegionMap.set(subKey, subRegionNode);
      regionNode.children.push(subRegionNode);
    }

    const subNode = subRegionMap.get(subKey);
    const code = getCodeByType(item);
    const uniqueKey = code ? `country:${code}` : `country:${countryCounter++}`;

    // 创建国家节点
    const countryNode = {
      key: uniqueKey,
      type: 'country',
      cnName: item.cnName || item.name,
      CCA2: item.ccA2,
      CCA3: item.ccA3,
      CCN3: item.ccN3,
      label: composeCountryLabel(item),
    };

    subNode.children.push(countryNode);
  }

  let children = Array.from(regionMap.values());

  // 欧洲放在最前面
  children.sort((a, b) => {
    const isA = (a.region === '欧洲' || a.label === '欧洲') ? 1 : 0;
    const isB = (b.region === '欧洲' || b.label === '欧洲') ? 1 : 0;
    if (isA !== isB) return isB - isA;
    return 0;
  });

  // 缓存所有叶子节点的key
  const leafKeys = [];
  keyToNode.value = new Map();

  const walk = (nodes) => {
    for (const n of nodes) {
      keyToNode.value.set(n.key, n);
      if (n.type === 'country') leafKeys.push(n.key);
      if (n.children && n.children.length) walk(n.children);
    }
  };

  walk(children);
  allLeafKeys.value = leafKeys;

  // 默认展开欧洲区域
  const expanded = new Set(['all']);
  for (const regionNode of children) {
    const isEurope = regionNode.region === '欧洲' || regionNode.label === '欧洲';
    if (isEurope) {
      expanded.add(regionNode.key);
      if (Array.isArray(regionNode.children)) {
        for (const sub of regionNode.children) {
          expanded.add(sub.key);
        }
      }
    }
  }
  defaultExpandedKeys.value = Array.from(expanded);

  treeData.value = [
    {
      key: 'all',
      type: 'all',
      label: '全选',
      children,
    }
  ];
};

// 刷新树标签（编码类型改变时）
const refreshTreeLabels = () => {
  const walk = (nodes) => {
    if (!nodes) return;
    for (const n of nodes) {
      if (n.type === 'country') {
        n.label = `${n.cnName} ${getCodeSuffix(n)}`;
      }
      if (n.children && n.children.length) walk(n.children);
    }
  };
  walk(treeData.value);
};

// 编码类型改变
const handleCodeTypeChange = () => {
  refreshTreeLabels();
};

// 过滤节点
const filterNode = (value, data) => {
  if (!value) return true;
  const label = (data.label || '').toString().toLowerCase();
  return label.includes(value.toLowerCase());
};

// 处理过滤
const handleFilter = (val) => {
  treeRef.value?.filter(val);
  // 过滤改变时退出全选模式
  if (selectAllMode.value && val !== filterText.value) {
    exitSelectAllMode();
  }
};

// 获取当前可见的叶子节点keys
const getVisibleLeafKeys = () => {
  const result = [];
  const walk = (nodes) => {
    for (const n of nodes) {
      if (n.type === 'country') {
        if (!filterText.value || filterNode(filterText.value, n)) {
          result.push(n.key);
        }
      }
      if (n.children && n.children.length) {
        walk(n.children);
      }
    }
  };

  if (treeData.value && treeData.value.length > 0) {
    walk(treeData.value[0].children || []);
  }
  return result;
};

// 收集叶子节点keys
const collectLeafKeys = (node) => {
  const result = [];
  const walk = (n) => {
    if (!n) return;
    if (n.type === 'country') {
      result.push(n.key);
      return;
    }
    if (Array.isArray(n.children)) {
      for (const c of n.children) walk(c);
    }
  };
  walk(node);
  return result;
};

// 树节点勾选事件
const handleTreeCheck = (nodeData, info) => {
  if (isProgrammaticChange.value) return;

  // 全选/取消全选
  if (nodeData && nodeData.key === 'all') {
    const selectingAll = info.checkedKeys && info.checkedKeys.includes('all');
    selectAllMode.value = selectingAll;
    excludedKeys.value = new Set();

    const visibleKeys = getVisibleLeafKeys();

    if (selectingAll) {
      selectAllOriginalKeys.value = [...visibleKeys];
    } else {
      selectAllOriginalKeys.value = [];
    }

    checkStrictly.value = true;
    isProgrammaticChange.value = true;

    if (selectingAll) {
      treeRef.value?.setCheckedKeys(['all', ...visibleKeys]);
    } else {
      treeRef.value?.setCheckedKeys([]);
    }

    nextTick(() => {
      checkedKeys.value = treeRef.value?.getCheckedKeys() || [];
      selectedLeafNodes.value = selectingAll
        ? visibleKeys
            .filter(k => !excludedKeys.value.has(k))
            .map(k => keyToNode.value.get(k))
            .filter(Boolean)
        : [];
      checkStrictly.value = false;
      isProgrammaticChange.value = false;
    });
    return;
  }

  // 全选模式下的勾选处理
  if (selectAllMode.value) {
    const toggleLeafKeys = (keys, exclude) => {
      for (const k of keys) {
        if (exclude) excludedKeys.value.add(k);
        else excludedKeys.value.delete(k);
      }

      checkStrictly.value = true;
      isProgrammaticChange.value = true;

      for (const k of keys) {
        treeRef.value?.setChecked(k, !excludedKeys.value.has(k), false);
      }
      treeRef.value?.setChecked('all', true, false);

      nextTick(() => {
        checkStrictly.value = false;
        isProgrammaticChange.value = false;
      });
    };

    if (nodeData.type === 'country') {
      const currentlyExcluded = excludedKeys.value.has(nodeData.key);
      toggleLeafKeys([nodeData.key], !currentlyExcluded);
    } else if (nodeData.type === 'subRegion' || nodeData.type === 'region') {
      const keys = collectLeafKeys(nodeData);
      const hasIncluded = keys.some(k => !excludedKeys.value.has(k));
      toggleLeafKeys(keys, hasIncluded);
    }

    selectedLeafNodes.value = selectAllOriginalKeys.value
      .filter(k => !excludedKeys.value.has(k))
      .map(k => keyToNode.value.get(k))
      .filter(Boolean);
    return;
  }

  // 默认勾选处理
  checkedKeys.value = info.checkedKeys || [];
  const nodes = treeRef.value?.getCheckedNodes(true) || [];
  selectedLeafNodes.value = nodes.filter(n => n.type === 'country');
};

// 退出全选模式
const exitSelectAllMode = () => {
  if (selectAllMode.value) {
    const currentlySelected = selectAllOriginalKeys.value
      .filter(k => !excludedKeys.value.has(k));

    selectAllMode.value = false;
    excludedKeys.value = new Set();
    selectAllOriginalKeys.value = [];

    checkStrictly.value = true;
    isProgrammaticChange.value = true;
    treeRef.value?.setCheckedKeys(currentlySelected);

    nextTick(() => {
      checkedKeys.value = treeRef.value?.getCheckedKeys() || [];
      const nodes = treeRef.value?.getCheckedNodes(true) || [];
      selectedLeafNodes.value = nodes.filter(n => n.type === 'country');
      checkStrictly.value = false;
      isProgrammaticChange.value = false;
    });
  }
};

// 清空所有选择
const clearAll = () => {
  selectAllMode.value = false;
  excludedKeys.value = new Set();
  selectAllOriginalKeys.value = [];
  checkedKeys.value = [];
  selectedLeafNodes.value = [];
  treeRef.value?.setCheckedKeys([]);
};

// 移除单个选择
const removeItem = (item) => {
  treeRef.value?.setChecked(item.key, false, false);
  checkedKeys.value = treeRef.value?.getCheckedKeys() || [];
  selectedLeafNodes.value = selectedLeafNodes.value.filter(n => n.key !== item.key);
};

// 应用预选数据
const applyPrechecked = (selectedData = []) => {
  if (!selectedData || selectedData.length === 0) return;

  const keys = [];
  
  // selectedData 格式: [{ locale: 'CN', en: 'China' }, ...]
  for (const item of selectedData) {
    const code = item.locale; // locale 就是国家编码
    // 根据当前编码类型查找对应的节点
    const walk = (nodes) => {
      for (const n of nodes) {
        if (n.type === 'country') {
          const codeTypeMap = { 0: 'CCA2', 1: 'CCA3', 2: 'CCN3' };
          const codeTypeStr = codeTypeMap[codeType.value];
          const codeVal = n[codeTypeStr];
          if (codeVal && String(codeVal) === String(code)) {
            keys.push(n.key);
          }
        }
        if (n.children && n.children.length) walk(n.children);
      }
    };
    walk(treeData.value);
  }

  checkedKeys.value = keys;
  treeRef.value?.setCheckedKeys(keys);

  nextTick(() => {
    const nodes = treeRef.value?.getCheckedNodes(true) || [];
    selectedLeafNodes.value = nodes.filter(n => n.type === 'country');
  });
};

// 打开对话框
const showEdit = async (selectedData = []) => {
  dialogVisible.value = true;

  if (!rawList.value || rawList.value.length === 0) {
    await fetchData();
  }

  buildTree();

  await nextTick();
  applyPrechecked(selectedData);
};

// 确认选择
const handleConfirm = () => {
  const nodes = selectAllMode.value
    ? selectAllOriginalKeys.value
        .filter(k => !excludedKeys.value.has(k))
        .map(k => keyToNode.value.get(k))
        .filter(Boolean)
    : selectedLeafNodes.value;

  const codeTypeMap = { 0: 'CCA2', 1: 'CCA3', 2: 'CCN3' };
  const codeTypeStr = codeTypeMap[codeType.value];

  // 转换为需要的格式：[{ locale: 'CN', en: 'China' }, ...]
  const result = nodes.map(n => ({
    locale: n[codeTypeStr] || n.CCA2,
    en: n.cnName,
  })).filter(item => item.locale);

  // 计算总数（所有国家）
  const allLength = allLeafKeys.value.length;

  emit('areaSave', result, allLength);
  emit('confirm', result, allLength);
  handleClose();
};

// 关闭对话框
const handleClose = () => {
  filterText.value = '';
  selectAllMode.value = false;
  excludedKeys.value = new Set();
  selectAllOriginalKeys.value = [];
  checkedKeys.value = [];
  selectedLeafNodes.value = [];
  dialogVisible.value = false;
};

// 暴露方法
defineExpose({
  showEdit,
  show: showEdit,
});
</script>

<style lang="scss" scoped>
.area-selector-container {
  .selector-layout {
    display: flex;
    gap: 20px;
    min-height: 500px;
  }

  .candidate-panel,
  .selected-panel {
    flex: 1;
    display: flex;
    flex-direction: column;
    border: 1px solid #e8e8e8;
    border-radius: 8px;
    overflow: hidden;
    background: #fff;
  }

  .panel-header {
    background: linear-gradient(135deg, #f5f7fa 0%, #e8eaf0 100%);
    padding: 14px 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid #e8e8e8;

    .header-title {
      font-weight: 600;
      font-size: 15px;
      color: #303133;
    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 10px;
    }
  }

  .panel-body {
    flex: 1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }

  // 左侧面板
  .candidate-panel {
    .search-box {
      padding: 16px;
      border-bottom: 1px solid #f0f0f0;

      :deep(.el-input__wrapper) {
        box-shadow: 0 0 0 1px #dcdfe6 inset;
        transition: all 0.3s;

        &:hover {
          box-shadow: 0 0 0 1px #c0c4cc inset;
        }
      }

      :deep(.el-input.is-focus .el-input__wrapper) {
        box-shadow: 0 0 0 1px #409eff inset;
      }
    }

    .tree-wrapper {
      flex: 1;
      overflow-y: auto;
      padding: 10px 16px;

      &::-webkit-scrollbar {
        width: 8px;
      }

      &::-webkit-scrollbar-track {
        background: #f1f1f1;
        border-radius: 4px;
      }

      &::-webkit-scrollbar-thumb {
        background: #c0c4cc;
        border-radius: 4px;

        &:hover {
          background: #909399;
        }
      }
    }

    .country-tree {
      :deep(.el-tree-node__content) {
        height: 36px;
        padding: 2px 0;
        
        &:hover {
          background-color: #f5f7fa;
        }
      }

      .tree-node-label {
        font-size: 14px;
        color: #606266;
      }
    }
  }

  // 右侧面板
  .selected-panel {
    .selected-list {
      flex: 1;
      overflow-y: auto;
      padding: 8px;

      &::-webkit-scrollbar {
        width: 8px;
      }

      &::-webkit-scrollbar-track {
        background: #f1f1f1;
        border-radius: 4px;
      }

      &::-webkit-scrollbar-thumb {
        background: #c0c4cc;
        border-radius: 4px;

        &:hover {
          background: #909399;
        }
      }

      .selected-item {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 10px 12px;
        margin-bottom: 6px;
        background: #f5f7fa;
        border-radius: 6px;
        transition: all 0.3s;

        &:hover {
          background: #e8eaf0;
          box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
        }

        .item-info {
          flex: 1;
          display: flex;
          align-items: center;
          gap: 8px;

          .item-name {
            font-size: 14px;
            color: #303133;
            font-weight: 500;
          }

          .item-code {
            font-size: 12px;
            color: #909399;
          }
        }

        .remove-btn {
          padding: 4px;
          opacity: 0.6;
          transition: all 0.3s;

          &:hover {
            opacity: 1;
            transform: scale(1.2);
          }
        }
      }
    }
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>

