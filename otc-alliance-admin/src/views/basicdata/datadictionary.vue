<template>
  <div class="acquiring-container">
    <common-table
      ref="tableRef"
      :filter-list="filterList"
      :table-columns="tableColumns"
      :table-data="tableData"
      :total="total"
      :loading="loading"
      @search="handleSearch"
      :searchForm="searchForm"
      :operationButtons="operationButtons"
      @add="handleAdd"
      addButtonPermission="Pages.BasicData.DataDictionary.BtnAdd"
    >
      <!-- 自定义时间列渲染 -->
      <template #table-creationTime="{ row }">
        <p>{{ t('common.creationTime') }}：{{ row.creationTime }}</p>
        <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime }}</p>
      </template>
    </common-table>

    <!-- 新增/编辑对话框 -->
    <DataDictionaryEdit ref="editRef" @fetchData="fetchData" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import {
  getDataDictionaryList,
  deleteDataDictionary,
} from "@/api/baseData";
import { getEnum } from "@/api/enum";
import DataDictionaryEdit from "./components/DataDictionaryEdit.vue";

const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  dicKey: "",
  dicValue: "",
  dicType: "",
});

// 字典类型选项
const dicTypeOptions = ref([]);

// 获取字典类型枚举
const fetchDicTypeOptions = async () => {
  try {
    const { result } = await getEnum("DataDicType");
    dicTypeOptions.value = result || [];
  } catch (error) {
    dicTypeOptions.value = [];
  }
};

// 搜索表单配置 - 使用 computed 确保语言切换时自动更新
const filterList = computed(() => [
  {
    label: t("basic.dicKey"),
    name: "dicKey",
    type: "text",
  },
  {
    label: t("basic.dicValue"),
    name: "dicValue",
    type: "text",
  },
  {
    label: t("basic.dicType"),
    name: "dicType",
    type: "enum",
    enumName: "DataDicType",
    filterable: true,
    multiple: false,
    width: "180px",
    size: "default",
    placeholder: t("basic.pleaseSelectDicType"),
    clearable: true,
  },
]);

// 表格列配置 - 使用 computed 确保语言切换时自动更新
const tableColumns = computed(() => [
  { prop: "dicKey", label: t("basic.dicKey") },
  { prop: "dicValue", label: t("basic.dicValue") },
  { prop: "description", label: t("common.description") },
  {
    prop: "dicType",
    label: t("basic.dicType"),
    formatter: (row) => {
      const option = dicTypeOptions.value.find(
        (item) => item.value === row.dicType
      );
      return option ? option.displayName : row.dicType;
    },
  },
  { prop: "creationTime", label: t("common.timeInfo"), type: "slot" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const tableRef = ref(null);
const editRef = ref(null);

// 操作按钮配置 - 使用 computed 确保语言切换时自动更新
const operationButtons = computed(() => [
  {
    label: t("common.edit"),
    type: "text",
    permission: "Pages.BasicData.DataDictionary.BtnEdit",
    onClick: (row) => {
      editRef.value.open(row);
    },
  },
  {
    label: t("common.delete"),
    type: "text",
    permission: "Pages.BasicData.DataDictionary.BtnDelete",
    onClick: (row) => {
      handleDelete(row);
    },
  },
]);

// 搜索处理
const handleSearch = () => {
  searchForm.pageNo = 1;
  searchForm.skipCount = 0;
  fetchData();
};

// 获取表格数据
const fetchData = async () => {
  loading.value = true;
  try {
    searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
    getDataDictionaryList.params = searchForm;
    const { result } = await request(getDataDictionaryList);
    tableData.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 删除处理
const handleDelete = async (row) => {
  try {
    await ElMessageBox.confirm(t("common.confirmDelete"), t("common.tip"), {
      confirmButtonText: t("common.confirm"),
      cancelButtonText: t("common.cancel"),
      type: "warning",
    });
    deleteDataDictionary.params = { id: row.id };
    const { success } = await request(deleteDataDictionary);
    if (success) {
      ElMessage.success(t("common.deleteSuccess"));
      fetchData();
    }
  } catch (error) {
    if (error !== "cancel") {
    }
  }
};

// 新增处理
const handleAdd = () => {
  editRef.value.open();
};

// 初始化
onMounted(async () => {
  await fetchDicTypeOptions();
  fetchData();
});
</script>

<style lang="scss" scoped>
.acquiring-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}
</style>

