<template>
  <div class="common-table-container">
    <!-- 搜索区域 -->
    <div class="search-area" v-if="filterList && filterList.length">
      <el-form
        :model="actualSearchForm"
        ref="searchFormRef"
        :inline="true"
        class="search-form"
      >
        <el-form-item
          v-for="(item, index) in filterList"
          :key="index"
          :label="item.label"
          :prop="item.name"
          class="search-form-item"
        >
          <template v-if="item.type === 'slot'">
            <slot
              :name="`search-${item.name}`"
              :form="actualSearchForm"
              :item="item"
              :index="index"
            />
          </template>
          <template v-else-if="item.type === 'multDatetime'">
            <div class="date-range-wrapper">
              <el-date-picker
                v-model="actualSearchForm[item.name1]"
                type="datetime"
                :placeholder="item.startPlaceholder || t('components.startDate')"
                :locale="locale"
                clearable
                style="width: 120px"
              />
              <span class="date-range-separator">{{ t('components.to') }}</span>
              <el-date-picker
                v-model="actualSearchForm[item.name2]"
                type="datetime"
                :placeholder="item.endPlaceholder || t('components.endDate')"
                :locale="locale"
                clearable
                style="width: 120px"
              />
            </div>
          </template>

          <template v-else-if="item.type === 'enum'">
            <EnumSelect
              v-model="actualSearchForm[item.name]"
              :enum-name="item.enumName"
              :placeholder="item.placeholder || t('components.pleaseSelect')"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :multiple="item.multiple || false"
              :width="item.width || '180px'"
              :size="item.size || 'default'"
              @change="item.change ? handleCustomChange(item.change, $event) : null"
            />
          </template>

          <template v-else-if="item.type === 'business'">
            <BusinessSelect
              v-model="actualSearchForm[item.name]"
              :business-type="item.businessType"
              :value-key="item.valueKey || 'id'"
              :placeholder="item.placeholder || t('components.pleaseSelectMerchant')"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :multiple="item.multiple || false"
              :width="item.width || '180px'"
              :size="item.size || 'default'"
            />
          </template>

          <template v-else-if="item.type === 'agent'">
            <AgentSelect
              v-model="actualSearchForm[item.name]"
              :value-key="item.valueKey || 'id'"
              :placeholder="item.placeholder || t('components.pleaseSelectAgent')"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :multiple="item.multiple || false"
              :width="item.width || '180px'"
              :size="item.size || 'default'"
            />
          </template>

          <template v-else-if="item.type === 'coin'">
            <CoinSelect
              v-model="actualSearchForm[item.name]"
              :placeholder="item.placeholder || t('components.pleaseSelectCoin')"
              :clearable="item.clearable !== false"
              :filterable="item.filterable !== false"
              :multiple="item.multiple || false"
              :width="item.width || '180px'"
              :size="item.size || 'default'"
              :show-chain-select="item.showChainSelect || false"
              :chain-model-value="actualSearchForm[item.chainName]"
              :chain-placeholder="item.chainPlaceholder || t('components.pleaseSelectChainType')"
              :chain-clearable="item.chainClearable !== false"
              :chain-filterable="item.chainFilterable !== false"
              :chain-multiple="item.chainMultiple || false"
              :chain-width="item.chainWidth || '180px'"
              :chain-size="item.chainSize || 'default'"
              @update:chain-model-value="actualSearchForm[item.chainName] = $event"
              @chain-change="item.chainChange && item.chainChange($event)"
            />
          </template>
          
          <template v-else-if="item.type === 'options'">
            <el-select
              v-model="actualSearchForm[item.name]"
              :placeholder="item.placeholder || t('components.pleaseSelect')"
              clearable
              filterable
            >
              <el-option
                v-for="option in getEnumOptions(item.options)"
                :key="option.value"
                :label="option.displayName"
                :value="option.value"
              />
            </el-select>
          </template>
          
          <template v-else-if="item.type === 'datetime'">
            <el-date-picker
              v-model="actualSearchForm[item.name]"
              type="datetime"
              :placeholder="item.placeholder || t('components.startDateTime')"
              clearable
              :locale="locale"
            />
          </template>
          <template v-else>
            <el-input
              v-if="item.type === 'text'"
              v-model="actualSearchForm[item.name]"
              :placeholder="item.placeholder || t('components.pleaseEnter')"
              clearable
              @keyup.enter="handleSearch"
            />
            <el-select
              v-else-if="item.type === 'select'"
              v-model="actualSearchForm[item.name]"
              :placeholder="item.placeholder || t('components.pleaseSelect')"
              clearable
              filterable
            >
              <el-option
                v-for="option in item.options"
                :key="option.value"
                :label="option.label"
                :value="option.value"
              />
            </el-select>
            <el-date-picker
              v-else-if="item.type === 'date'"
              v-model="actualSearchForm[item.name]"
              :type="item.dateType || 'date'"
              :placeholder="item.placeholder || t('components.startDate')"
              clearable
              :locale="locale"
            />
          </template>
        </el-form-item>

        <el-form-item class="search-form-item search-btns">
          <el-button type="primary" @click="handleSearch" :loading="loading">
            <el-icon><Search /></el-icon>{{ t('components.search') }}
          </el-button>
          <slot name="search-buttons"></slot>
          <el-button 
            v-if="availableFilterFields.length > 0"
            @click="handleAdvancedSearch"
          >
            <el-icon><Filter /></el-icon>{{ t('components.advancedSearch') }}
            <el-badge
              v-if="customFiltersCount > 0"
              :value="customFiltersCount"
              class="advanced-search-badge"
            />
          </el-button>
          <el-button
            v-if="
              showAddButton &&
              (!addButtonPermission || hasPermission(addButtonPermission))
            "
            type="primary"
            @click="handleAdd"
          >
            <el-icon><Plus /></el-icon>{{ t('components.add') }}
          </el-button>
          <el-button @click="handleReset">
            <el-icon><Refresh /></el-icon>{{ t('components.reset') }}
          </el-button>
        </el-form-item>
      </el-form>
    </div>
    <div v-else class="search-area">
      <div class="search-form">
        <el-form-item class="search-form-item search-btns">
          <el-button 
            v-if="availableFilterFields.length > 0"
            @click="handleAdvancedSearch"
          >
            <el-icon><Filter /></el-icon>{{ t('components.advancedSearch') }}
            <el-badge
              v-if="customFiltersCount > 0"
              :value="customFiltersCount"
              class="advanced-search-badge"
            />
          </el-button>
          <el-button
            v-if="
              showAddButton &&
              (!addButtonPermission || hasPermission(addButtonPermission))
            "
            type="primary"
            @click="handleAdd"
          >
            <el-icon><Plus /></el-icon>{{ t('components.add') }}
          </el-button>
        </el-form-item>
      </div>
    </div>

    <!-- 高级搜索弹窗 -->
    <el-dialog
      v-model="advancedSearchVisible"
      :title="t('components.advancedSearch')"
      width="800px"
      :close-on-click-modal="false"
    >
      <div class="advanced-search-content">
        <div class="filter-list">
          <div
            v-for="(filter, index) in customFilters"
            :key="index"
            class="filter-item"
          >
            <el-form-item
              :label="`${t('components.condition')} ${index + 1}`"
              class="filter-field-name"
            >
              <el-select
                v-model="filter.FieldName"
                :placeholder="t('components.pleaseSelectField')"
                filterable
                clearable
                style="width: 100%"
                @change="handleFieldChange(index)"
              >
                <el-option
                  v-for="field in availableFilterFields"
                  :key="field.value"
                  :label="field.label || field.displayName"
                  :value="field.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="t('components.operator')" class="filter-operator">
              <el-select
                v-model="filter.Operator"
                :placeholder="t('components.pleaseSelectOperator')"
                style="width: 150px"
              >
                <el-option
                  v-for="op in getAvailableOperators(filter.FieldName)"
                  :key="op.value"
                  :label="op.label"
                  :value="op.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item :label="t('components.value')" class="filter-value">
              <!-- 根据字段数据类型动态渲染不同的输入组件 -->
              <template v-if="getFieldDataType(filter.FieldName) === 'text'">
                <el-input
                  v-model="filter.Value"
                  :placeholder="t('components.pleaseEnterValue')"
                  clearable
                />
              </template>
              <template v-else-if="getFieldDataType(filter.FieldName) === 'number'">
                <el-input-number
                  v-model="filter.Value"
                  :placeholder="t('components.pleaseEnterNumber')"
                  :precision="0"
                  :controls="true"
                  :min="undefined"
                  :max="undefined"
                  style="width: 100%"
                />
              </template>
              <template v-else-if="getFieldDataType(filter.FieldName) === 'date'">
                <el-date-picker
                  v-model="filter.Value"
                  type="date"
                  :placeholder="t('components.startDate')"
                  clearable
                  style="width: 100%"
                  :locale="locale"
                />
              </template>
              <template v-else-if="getFieldDataType(filter.FieldName) === 'datetime'">
                <el-date-picker
                  v-model="filter.Value"
                  type="datetime"
                  :placeholder="t('components.startDateTime')"
                  clearable
                  style="width: 100%"
                  :locale="locale"
                />
              </template>
              <template v-else-if="getFieldDataType(filter.FieldName) === 'select'">
                <el-select
                  v-model="filter.Value"
                  :placeholder="t('components.pleaseSelect')"
                  clearable
                  filterable
                  style="width: 100%"
                >
                  <el-option
                    v-for="option in getFieldOptions(filter.FieldName)"
                    :key="option.value"
                    :label="option.label"
                    :value="option.value"
                  />
                </el-select>
              </template>
              <template v-else-if="getFieldDataType(filter.FieldName) === 'boolean'">
                <el-select
                  v-model="filter.Value"
                  :placeholder="t('components.pleaseSelect')"
                  clearable
                  style="width: 100%"
                >
                  <el-option :label="t('components.yes')" :value="true" />
                  <el-option :label="t('components.no')" :value="false" />
                </el-select>
              </template>
              <template v-else>
                <el-input
                  v-model="filter.Value"
                  :placeholder="t('components.pleaseEnterValue')"
                  clearable
                />
              </template>
            </el-form-item>
            <el-form-item class="filter-action">
              <el-button
                type="danger"
                :icon="Delete"
                circle
                @click="removeFilter(index)"
                :disabled="customFilters.length <= 1"
              />
            </el-form-item>
          </div>
        </div>
        <div class="filter-actions">
          <el-button type="primary" :icon="Plus" @click="addFilter">
            {{ t('components.addCondition') }}
          </el-button>
          <el-button @click="clearAllFilters">{{ t('components.clearAllConditions') }}</el-button>
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="handleAdvancedSearchCancel">{{ t('components.cancel') }}</el-button>
          <el-button type="primary" @click="handleAdvancedSearchConfirm">
            {{ t('common.confirm') }}
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 表格和列设置按钮区域 -->
    <div class="table-area">
      <div class="table-toolbar">
        <div class="toolbar-right">
          <el-popover
            placement="bottom"
            width="260"
            trigger="click"
            popper-class="column-setting-popover"
          >
            <template #reference>
              <el-button 
                circle 
                class="column-setting-btn large"
                title="列显示设置"
              >
                <el-icon :size="15"><Setting /></el-icon>
              </el-button>
            </template>
            <div class="column-setting-title">列显示设置</div>
            <el-checkbox-group
              v-model="checkedColumnKeys"
              class="column-checkbox-group"
            >
              <el-checkbox
                v-for="col in actualTableColumns"
                :key="col.prop || col.name"
                :value="col.prop || col.name"
                class="column-checkbox"
                >{{ col.label }}</el-checkbox
              >
            </el-checkbox-group>
            <div class="column-setting-actions">
              <el-button size="small" @click="handleSelectAll"
                >全选</el-button
              >
            </div>
          </el-popover>
        </div>
      </div>
      <div class="table-wrapper">
        <el-table
          v-loading="loading"
          :data="tableData"
          border
          style="width: 100%"
          @selection-change="handleSelectionChange"
          @sort-change="handleSortChange"
        >
          <!-- 选择列 -->
          <el-table-column
            v-if="showSelection"
            type="selection"
            width="55"
            align="center"
          />

          <!-- 序号列 -->
          <el-table-column
            v-if="showIndex"
            type="index"
            :label="t('components.index')"
            width="60"
            align="center"
          />

          <!-- 数据列 -->
          <template v-for="(column, index) in visibleColumns" :key="index">
            <el-table-column
              v-bind="column"
              v-if="column.width"
              :width="column.width"
              :align="column.align || 'center'"
              :sortable="isSortable(column.prop || column.name) ? 'custom' : false"
            >
              <template #default="scope" v-if="column.type === 'slot'">
                <slot
                  :name="`table-${column.prop || column.name}`"
                  :row="scope.row"
                  :index="scope.$index"
                />
              </template>
              <template #default="scope" v-else-if="column.type === 'no'">
                <div class="no-column">
                  <span class="no-text">{{ scope.row[column.prop || column.name] }}</span>
                  <el-button
                    link
                    size="small"
                    class="copy-btn"
                    @click="handleCopy(scope.row[column.prop || column.name])"
                    :title="t('common.copy') + ' ' + scope.row[column.prop || column.name]"
                  >
                    <el-icon><CopyDocument /></el-icon>
                  </el-button>
                </div>
              </template>
              <template #default="scope" v-else-if="column.type === 'float'">
                <span>{{ formatFloat(scope.row[column.prop || column.name]) }}</span>
              </template>
              <template #default="scope" v-else-if="column.type === 'enum'">
                <el-tag :type="getEnumTagType(scope.row[column.prop || column.name], column.enumName)">
                  {{ getEnumDisplayName(scope.row[column.prop || column.name], column.enumName) }}
                </el-tag>
              </template>
              <template #default="scope" v-else-if="column.slot">
                <slot
                  :name="`table-${column.slot || column.prop || column.name}`"
                  :row="scope.row"
                  :index="scope.$index"
                />
              </template>
            </el-table-column>
            <el-table-column
              v-else
              v-bind="column"
              :align="column.align || 'center'"
              :sortable="isSortable(column.prop || column.name) ? 'custom' : false"
            >
              <template #default="scope" v-if="column.type === 'slot'">
                <slot
                  :name="`table-${column.prop || column.name}`"
                  :row="scope.row"
                  :index="scope.$index"
                />
              </template>
              <template #default="scope" v-else-if="column.type === 'no'">
                <div class="no-column">
                  <span class="no-text">{{ scope.row[column.prop || column.name] }}</span>
                  <el-button
                    link
                    size="small"
                    class="copy-btn"
                    @click="handleCopy(scope.row[column.prop || column.name])"
                    :title="t('common.copy') + ' ' + scope.row[column.prop || column.name]"
                  >
                    <el-icon><CopyDocument /></el-icon>
                  </el-button>
                </div>
              </template>
              <template #default="scope" v-else-if="column.type === 'float'">
                <span>{{ formatFloat(scope.row[column.prop || column.name]) }}</span>
              </template>
              <template #default="scope" v-else-if="column.type === 'enum'">
                <el-tag :type="getEnumTagType(scope.row[column.prop || column.name], column.enumName)">
                  {{ getEnumDisplayName(scope.row[column.prop || column.name], column.enumName) }}
                </el-tag>
              </template>
              <template #default="scope" v-else-if="column.slot">
                <slot
                  :name="`table-${column.slot}`"
                  :row="scope.row"
                  :index="scope.$index"
                />
              </template>
            </el-table-column>
          </template>

          <!-- 操作列 -->
          <el-table-column
            v-if="showOperation || hasVisibleOperationButtons"
            :label="t('common.operation')"
            :width="operationWidth"
            fixed="right"
            align="center"
          >
            <template #default="scope">
              <div class="operation-btns">
                <slot name="operation" :row="scope.row" :index="scope.$index">
                  <el-button
                    v-for="(btn, index) in operationButtons"
                    :key="index"
                    :type="btn.type || 'primary'"
                    :size="btn.size || 'small'"
                    :icon="btn.icon"
                    v-show="(!btn.show || btn.show(scope.row, scope.$index)) && hasPermission(btn.permission)"
                    @click="btn.onClick(scope.row, scope.$index)"
                  >
                    {{ btn.label || btn.computedLable(scope.row) }}
                  </el-button>
                </slot>
              </div>
            </template>
          </el-table-column>
        </el-table>
        <el-empty
          v-if="!tableData || tableData.length === 0"
          :image="emptyImage"
          :description="t('common.noData')"
          class="table-empty"
        />
      </div>
    </div>

    <!-- 分页区域 - 固定在底部 -->
    <div class="pagination-area" v-if="showPagination">
      <el-pagination
        v-model:current-page="actualSearchForm.pageNo"
        v-model:page-size="actualSearchForm.maxResultCount"
        :page-sizes="[10, 20, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, getCurrentInstance } from "vue";
import {
  Search,
  Refresh,
  Plus,
  Setting,
  CopyDocument,
  Filter,
  Delete,
} from "@element-plus/icons-vue";
import { useI18n } from "vue-i18n";
import { ElEmpty, ElMessage } from "element-plus";
import emptyImage from "@/assets/data_empty.png";
import { useUserStore } from "@/store/modules/user";

const userStore = useUserStore();
const btnPermissions = userStore.btnPermissions;
const { t } = useI18n();

// Add permission check function
const hasPermission = (permission) => {
  
  return btnPermissions.includes(permission);
};

// 兼容性处理：优先使用 queryForm，其次使用 searchForm
const actualSearchForm = computed(() => {
  return props.queryForm || props.searchForm;
});

// 兼容性处理：优先使用 tableList，其次使用 tableColumns
const actualTableColumns = computed(() => {
  return props.tableList || props.tableColumns;
});

const props = defineProps({
  // 搜索表单配置
  filterList: {
    type: Array,
    default: () => [],
  },
  // 搜索表单数据由父组件传入
  searchForm: {
    type: Object,
    required: true,
  },
  // 兼容 queryForm 属性名
  queryForm: {
    type: Object,
    default: null,
  },
  // 表格列配置
  tableColumns: {
    type: Array,
    required: true,
  },
  // 兼容 tableList 属性名
  tableList: {
    type: Array,
    default: null,
  },
  // 表格数据
  tableData: {
    type: Array,
    default: () => [],
  },
  // 是否显示选择列
  showSelection: {
    type: Boolean,
    default: false,
  },
  // 是否显示序号列
  showIndex: {
    type: Boolean,
    default: true,
  },
  // 是否显示操作列
  showOperation: {
    type: Boolean,
    default: true,
  },
  // 操作列宽度
  operationWidth: {
    type: [String, Number],
    default: 150,
  },
  // 操作按钮配置
  operationButtons: {
    type: Array,
    default: () => [],
  },
  // 是否显示新增按钮
  showAddButton: {
    type: Boolean,
    default: true,
  },
  // 是否显示分页
  showPagination: {
    type: Boolean,
    default: true,
  },
  // 总数据量
  total: {
    type: Number,
    default: 0,
  },
  // 加载状态
  loading: {
    type: Boolean,
    default: false,
  },
  // Add permission prop for add button
  addButtonPermission: {
    type: String,
    default: "",
  },
  // 高级搜索字段配置
  customFilterFields: {
    type: Array,
    default: () => [],
    // 格式: [
    //   { 
    //     value: "UserName",           // 字段名（必填）
    //     label: "用户名",             // 显示名称（必填）
    //     displayName: "用户名",       // 显示名称（可选，与label相同）
    //     dataType: "text",            // 数据类型：text/number/date/datetime/select/boolean（可选，默认text）
    //     options: [                   // 当dataType为select时必填
    //       { value: "1", label: "选项1" }
    //     ]
    //   }
    // ]
  },
  // 可排序字段数组
  sortableFields: {
    type: Array,
    default: () => [],
    // 格式: ['字段1', '字段2', '字段3']
  },
});

const emit = defineEmits([
  "search",
  "reset",
  "add",
  "selection-change",
  "size-change",
  "current-change",
]);

const instance = getCurrentInstance();
function isEventOverridden(eventName) {
  const props = instance?.vnode?.props || {};
  // 支持 onSearch、on-search
  return (
    props[`on${eventName.charAt(0).toUpperCase()}${eventName.slice(1)}`] ||
    props[`on${eventName}`]
  );
}

// 分页数据
const currentPage = ref(1);
const pageSize = ref(10);

// 选中的数据
const selectedRows = ref([]);

// 列显示控制
const checkedColumnKeys = ref(actualTableColumns.value.map((col) => col.prop || col.name));

watch(
  () => actualTableColumns.value,
  (newCols) => {
    checkedColumnKeys.value = newCols.map((col) => col.prop || col.name);
  }
);

const visibleColumns = computed(() => {
  return actualTableColumns.value.filter((col) =>
    checkedColumnKeys.value.includes(col.prop || col.name)
  );
});

// Add computed property to check if any operation buttons are visible
const hasVisibleOperationButtons = computed(() => {
  return props.operationButtons.length > 0;
});

// 搜索表单数据
const searchFormRef = ref(null);

// 高级搜索相关
const advancedSearchVisible = ref(false);
const customFilters = ref([
  { FieldName: "", Operator: "contains", Value: null },
]);

// 处理字段变化
const handleFieldChange = (index) => {
  const filter = customFilters.value[index];
  if (filter && filter.FieldName) {
    const availableOps = getAvailableOperators(filter.FieldName);
    const hasValidOp = availableOps.some((op) => op.value === filter.Operator);
    
    // 如果当前操作符不适用于该字段类型，自动调整为第一个可用操作符
    if (!hasValidOp && availableOps.length > 0) {
      filter.Operator = availableOps[0].value;
    }
    
    // 根据字段类型，可能需要清空或调整值
    const dataType = getFieldDataType(filter.FieldName);
    if (dataType === "number") {
      // 如果是数字类型，确保值是数字或null
      if (filter.Value !== null && filter.Value !== undefined && isNaN(Number(filter.Value))) {
        filter.Value = null;
      }
    } else if (dataType === "date" || dataType === "datetime") {
      // 日期类型，如果值不是日期格式，清空
      // 这里不做强制清空，让用户自己选择
    } else if (dataType === "select" || dataType === "boolean") {
      // 选择类型和布尔类型，如果当前值不在选项中，清空
      if (dataType === "select") {
        const options = getFieldOptions(filter.FieldName);
        const hasValue = options.some((opt) => opt.value === filter.Value);
        if (!hasValue && filter.Value !== null && filter.Value !== undefined && filter.Value !== "") {
          filter.Value = null;
        }
      }
    }
  }
};

// 计算可用的字段列表（仅使用 customFilterFields，不自动提取）
const availableFilterFields = computed(() => {
  if (props.customFilterFields && props.customFilterFields.length > 0) {
    return props.customFilterFields.map((field) => ({
      value: field.value || field.FieldName,
      label: field.displayName || field.label || field.value || field.FieldName,
      displayName: field.displayName || field.label || field.value || field.FieldName,
      dataType: field.dataType || field.type || "text",
      options: field.options || [],
    }));
  }
  // 如果没有提供字段配置，返回空数组（不自动提取）
  return [];
});

// 根据字段名获取字段配置
const getFieldConfig = (fieldName) => {
  return availableFilterFields.value.find((field) => field.value === fieldName);
};

// 获取字段的数据类型
const getFieldDataType = (fieldName) => {
  const field = getFieldConfig(fieldName);
  return field?.dataType || "text";
};

// 获取字段的选项（用于select类型）
const getFieldOptions = (fieldName) => {
  const field = getFieldConfig(fieldName);
  return field?.options || [];
};

// 根据数据类型获取可用的操作符
const getAvailableOperators = (fieldName) => {
  const dataType = getFieldDataType(fieldName);
  
  // 所有操作符
  const allOperators = [
    { label: t('common.equals'), value: "eq" },
    { label: t('common.notEquals'), value: "ne" },
    { label: t('common.contains'), value: "contains" },
    { label: t('common.notContains'), value: "notcontains" },
    { label: t('common.greaterThan'), value: "gt" },
    { label: t('common.greaterThanOrEqual'), value: "ge" },
    { label: t('common.lessThan'), value: "lt" },
    { label: t('common.lessThanOrEqual'), value: "le" },
    { label: t('components.startsWith'), value: "startswith" },
    { label: t('components.endsWith'), value: "endswith" },
  ];
  
  // 文本类型支持所有操作符
  if (dataType === "text") {
    return allOperators;
  }
  
  // 数字、日期、日期时间类型只支持比较操作符
  if (dataType === "number" || dataType === "date" || dataType === "datetime") {
    return [
      { label: t('common.equals'), value: "eq" },
      { label: t('common.notEquals'), value: "ne" },
      { label: t('common.greaterThan'), value: "gt" },
      { label: t('common.greaterThanOrEqual'), value: "ge" },
      { label: t('common.lessThan'), value: "lt" },
      { label: t('common.lessThanOrEqual'), value: "le" },
    ];
  }
  
  // 布尔类型和选择类型只支持等于和不等于
  if (dataType === "boolean" || dataType === "select") {
    return [
      { label: t('common.equals'), value: "eq" },
      { label: t('common.notEquals'), value: "ne" },
    ];
  }
  
  // 默认返回所有操作符
  return allOperators;
};

// 计算高级搜索条件数量
const customFiltersCount = computed(() => {
  return customFilters.value.filter(
    (f) => f.FieldName && f.Operator && f.Value
  ).length;
});

// 添加过滤条件
const addFilter = () => {
  customFilters.value.push({
    FieldName: "",
    Operator: "contains",
    Value: null,
  });
};

// 删除过滤条件
const removeFilter = (index) => {
  if (customFilters.value.length > 1) {
    customFilters.value.splice(index, 1);
  }
};

// 清空所有过滤条件
const clearAllFilters = () => {
  customFilters.value = [
    { FieldName: "", Operator: "contains", Value: null },
  ];
};

// 打开高级搜索弹窗
const handleAdvancedSearch = () => {
  // 如果有已保存的条件，先加载
  if (actualSearchForm.value.CustomFilters && actualSearchForm.value.CustomFilters.length > 0) {
    customFilters.value = JSON.parse(
      JSON.stringify(actualSearchForm.value.CustomFilters)
    );
  } else {
    // 如果没有已保存的条件，初始化一个空条件
    customFilters.value = [
      { FieldName: "", Operator: "contains", Value: null },
    ];
  }
  advancedSearchVisible.value = true;
};

// 取消高级搜索
const handleAdvancedSearchCancel = () => {
  advancedSearchVisible.value = false;
};

// 确认高级搜索
const handleAdvancedSearchConfirm = () => {
  // 过滤掉空条件，对于不同类型有不同的验证逻辑
  const validFilters = customFilters.value.filter((f) => {
    if (!f.FieldName || !f.Operator) {
      return false;
    }
    
    // 值验证：不同类型的值有不同的验证方式
    const dataType = getFieldDataType(f.FieldName);
    if (dataType === "boolean") {
      // 布尔类型允许 true/false/null
      return f.Value === true || f.Value === false || f.Value === 0 || f.Value === 1;
    } else if (dataType === "number") {
      // 数字类型必须是非空的数字
      return f.Value !== null && f.Value !== undefined && f.Value !== "" && !isNaN(Number(f.Value));
    } else {
      // 其他类型（text、date、datetime、select）只要非空即可
      return f.Value !== null && f.Value !== undefined && f.Value !== "";
    }
  });
  
  // 将有效的过滤条件保存到搜索表单
  if (validFilters.length > 0) {
    actualSearchForm.value.CustomFilters = validFilters;
  } else {
    // 如果没有有效条件，清除CustomFilters
    if (actualSearchForm.value.CustomFilters) {
      delete actualSearchForm.value.CustomFilters;
    }
  }
  
  advancedSearchVisible.value = false;
  
  // 自动执行搜索
  handleSearch();
};

// 搜索
const handleSearch = () => {
  emit("search", actualSearchForm.value);
  if (!isEventOverridden("search")) {
    // 默认逻辑：可在此发起请求或其他操作
    // console.log('默认搜索逻辑', searchForm.value);
  }
};

// 处理自定义 change 事件
const handleCustomChange = (changeHandler, value) => {
  // 如果父组件传递了自定义的 change 处理函数
  if (typeof changeHandler === 'function') {
    changeHandler(value);
  } else if (typeof changeHandler === 'string') {
    // 如果是字符串，尝试从父组件的作用域中调用
    const parentComponent = getCurrentInstance()?.parent;
    if (parentComponent && parentComponent.exposed && parentComponent.exposed[changeHandler]) {
      parentComponent.exposed[changeHandler](value);
    } else {
    }
  }
};

// 重置
const handleReset = () => {
  if (searchFormRef.value) {
    searchFormRef.value.resetFields();
  }

  // 手动重置 multDatetime 类型的字段
  if (props.filterList && props.filterList.length) {
    props.filterList.forEach(item => {
      if (item.type === 'multDatetime') {
        if (item.name1) {
          actualSearchForm.value[item.name1] = null;
        }
        if (item.name2) {
          actualSearchForm.value[item.name2] = null;
        }
      }
    });
  }

  // 清空高级搜索条件
  if (actualSearchForm.value.CustomFilters) {
    delete actualSearchForm.value.CustomFilters;
  }
  customFilters.value = [
    { FieldName: "", Operator: "contains", Value: "" },
  ];

  // 清空排序条件
  if (actualSearchForm.value.Sorting) {
    delete actualSearchForm.value.Sorting;
  }

  // 直接重置表单，不调用父级方法
};

// 新增
const handleAdd = () => {
  emit("add");
  if (!isEventOverridden("add")) {
    // 默认逻辑：可在此弹窗或跳转
    // console.log('默认新增逻辑');
  }
};

// 选择变化
const handleSelectionChange = (selection) => {
  selectedRows.value = selection;
  emit("selection-change", selection);
  // 通常不需要默认逻辑
};

// 每页条数变化
const handleSizeChange = (val) => {
  actualSearchForm.value.maxResultCount = val;
  actualSearchForm.value.skipCount = (actualSearchForm.value.pageNo - 1) * val;
  emit("size-change", val);
  handleSearch();
  if (!isEventOverridden("size-change")) {
    // 默认逻辑：可在此自动刷新数据
  }
};

// 当前页变化
const handleCurrentChange = (val) => {
  actualSearchForm.value.pageNo = val;
  actualSearchForm.value.skipCount = (val - 1) * actualSearchForm.value.maxResultCount;
  emit("current-change", val);
  // 调用查询方法
  handleSearch();
  if (!isEventOverridden("current-change")) {
    // 默认逻辑：可在此自动刷新数据
  }
};

const handleSelectAll = () => {
  checkedColumnKeys.value = actualTableColumns.value.map((col) => col.prop || col.name);
};

const handleResetColumns = () => {
  checkedColumnKeys.value = actualTableColumns.value.map((col) => col.prop || col.name);
};

// 复制功能
const handleCopy = async (text) => {
  try {
    await navigator.clipboard.writeText(text);
    ElMessage.success('复制成功');
  } catch (err) {
    // 如果现代API不可用，使用传统方法
    const textArea = document.createElement('textarea');
    textArea.value = text;
    document.body.appendChild(textArea);
    textArea.select();
    try {
      document.execCommand('copy');
      ElMessage.success('复制成功');
    } catch (fallbackErr) {
      ElMessage.error('复制失败');
    }
    document.body.removeChild(textArea);
  }
};

// 格式化浮点数
const formatFloat = (value) => {
  if (value === null || value === undefined || value === '') {
    return '-';
  }
  const num = parseFloat(value);
  if (isNaN(num)) {
    return value;
  }
  return num.toLocaleString('zh-CN', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
  });
};

// 获取枚举选项
const getEnumOptions = (enumName) => {
  // 这里需要根据实际的枚举数据源来获取选项
  // 可以从 store 或者 API 获取
  const enumStore = {
    'Coins': [
      { value: 'BTC', displayName: '比特币' },
      { value: 'ETH', displayName: '以太坊' },
      { value: 'USDT', displayName: '泰达币' }
    ],
    'OpenClosedStatus': [
      { value: 0, displayName: '关闭' },
      { value: 1, displayName: '开启' }
    ]
  };
  return enumStore[enumName] || [];
};

// 获取枚举显示名称
const getEnumDisplayName = (value, enumName) => {
  const options = getEnumOptions(enumName);
  const option = options.find(opt => opt.value === value);
  return option ? option.displayName : value;
};

// 获取枚举标签类型
const getEnumTagType = (value, enumName) => {
  // 根据枚举值返回不同的标签类型
  if (enumName === 'OpenClosedStatus') {
    return value === 1 ? 'success' : 'danger';
  }
  // 确保始终返回有效的 type 值
  return 'info';
};

// daterange change handler，支持只选一个
const handleDaterangeChange = (prop, val) => {
  // val 可能为 [], [start], [start, end]
  actualSearchForm.value[prop] = val;
};

// 判断字段是否可排序
const isSortable = (fieldName) => {
  if (!fieldName || !props.sortableFields || props.sortableFields.length === 0) {
    return false;
  }
  return props.sortableFields.includes(fieldName);
};

// 处理排序变化
const handleSortChange = ({ column, prop, order }) => {
  // order: 'ascending' | 'descending' | null
  // 直接修改 props 中的 searchForm 或 queryForm
  const searchForm = props.queryForm || props.searchForm;
  
  if (!prop || !order) {
    // 取消排序
    if (searchForm.Sorting) {
      delete searchForm.Sorting;
    }
  } else {
    // 设置排序
    const sortOrder = order === 'ascending' ? 'asc' : 'desc';
    // 直接修改原始对象
    searchForm.Sorting = `${prop} ${sortOrder}`;
  }
  
  // 重置到第一页
  searchForm.pageNo = 1;
  searchForm.skipCount = 0;
  
  // 触发搜索，确保传递最新的 searchForm
  handleSearch();
};

// 暴露方法给父组件
defineExpose({
  selectedRows,
  resetForm: handleReset,
});

const { locale } = useI18n();
</script>

<style lang="scss" scoped>
.common-table-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  max-width: 100%;

  .search-area {
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px 0 rgba(0, 0, 0, 0.04);
    padding: 18px 12px 8px 12px;
    margin-bottom: 18px;
    flex-shrink: 0;
    .search-form {
      display: flex;
      flex-wrap: wrap;
      align-items: flex-end;
      gap: 0 24px;
      .search-form-item {
        margin-bottom: 10px;
      }
      .search-btns {
        margin-left: 8px;
        margin-bottom: 10px;
      }
    }
    :deep(.el-select),
    :deep(.el-date-editor),
    :deep(.el-input) {
      min-width: 180px;
      width: 180px; // 固定宽度，防止清空按钮导致的宽度变化
    }
    
    // 修复输入框清空按钮导致的抖动问题
    :deep(.el-input) {
      .el-input__wrapper {
        transition: none !important;
      }
      
      .el-input__suffix {
        transition: none !important;
      }
      
      .el-input__clear {
        transition: none !important;
      }
    }
    
    :deep(.el-select) {
      .el-select__wrapper {
        transition: none !important;
      }
      
      .el-select__suffix {
        transition: none !important;
      }
      
      .el-select__clear {
        transition: none !important;
      }
    }
    
    :deep(.el-date-editor) {
      .el-input__wrapper {
        transition: none !important;
      }
      
      .el-input__suffix {
        transition: none !important;
      }
      
      .el-input__clear {
        transition: none !important;
      }
    }
  }

  .table-area {
    position: relative;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px 0 rgba(0, 0, 0, 0.04);
    padding: 0 0 8px 0;
    margin-bottom: 18px;
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    
    .table-toolbar {
      display: flex;
      justify-content: flex-end;
      align-items: center;
      padding: 10px 12px 0 12px;
      min-height: 44px;
      background: transparent;
      flex-shrink: 0;
    }
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .table-wrapper {
      position: relative;
      flex: 1;
      overflow: auto;
    }
  }

  .column-setting-float {
    display: none;
  }

  .el-table {
    min-width: 100%;
    width: max-content;
  }
  
  .pagination-area {
    display: flex;
    justify-content: flex-end;
    flex-shrink: 0;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px 0 rgba(0, 0, 0, 0.04);
    padding: 12px 12px;
    margin-top: auto;
  }
}

::v-deep(.column-setting-popover) {
  padding: 12px 16px 8px 16px;
  .column-setting-title {
    font-weight: bold;
    font-size: 15px;
    margin-bottom: 10px;
    color: #333;
  }
  .column-checkbox-group {
    display: grid;
    grid-template-columns: repeat(2, 1fr); // 两列
    gap: 8px; // 项之间的间距
    padding: 8px 0;
  }

  .column-checkbox {
    display: flex;
    align-items: center;
  }
  .column-setting-actions {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
  }
}

.column-setting-btn.large {
  width: 30px;
  height: 30px;
  font-size: 28px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 5px;
}

.date-range-wrapper {
  display: flex;
  align-items: center;
  .date-range-separator {
    margin: 0 8px;
    color: #999;
  }
  :deep(.el-date-editor) {
    min-width: 120px;
  }
}

// 高级搜索相关样式
.advanced-search-badge {
  margin-left: 4px;
}

.advanced-search-content {
  .filter-list {
    max-height: 400px;
    overflow-y: auto;
    padding: 10px 0;
    
    .filter-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px;
      margin-bottom: 12px;
      background: #f5f7fa;
      border-radius: 6px;
      border: 1px solid #e4e7ed;
      
      .filter-field-name {
        flex: 1;
        margin-bottom: 0;
      }
      
      .filter-operator {
        width: 180px;
        margin-bottom: 0;
      }
      
      .filter-value {
        flex: 1;
        margin-bottom: 0;
      }
      
      .filter-action {
        margin-bottom: 0;
        padding-top: 4px;
      }
    }
  }
  
  .filter-actions {
    display: flex;
    gap: 12px;
    padding: 16px 0;
    border-top: 1px solid #e4e7ed;
    margin-top: 12px;
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.table-empty {
  margin: 32px 0;
}

.operation-btns {
  display: flex;
  align-items: center;
  justify-content: center;
  white-space: nowrap;
  text-align: center;
}
.operation-btns .el-button {
  margin: 0 !important;
  margin-bottom: 0px !important;
}

.no-column {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  
  .no-text {
    font-family: 'Courier New', monospace;
    font-weight: 500;
    color: #333;
    user-select: text;
  }
  
  .copy-btn {
    padding: 4px;
    min-height: auto;
    opacity: 0;
    transition: opacity 0.2s ease;
    
    &:hover {
      background-color: #f5f7fa;
    }
  }
  
  &:hover .copy-btn {
    opacity: 1;
  }
}
</style>
