<template>
  <div class="acquiring-container">
    <!-- 表格工具栏 -->
    

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
      :addButtonPermission="'Pages.BasicData.CountriesAndRegions.BtnEdit'"
    >
      <!-- 搜索按钮区域的自定义按钮 -->
      <template #search-buttons>
        <el-button type="primary" @click="handleSyncCountries" :loading="syncLoading" v-permission="'Pages.BasicData.CountriesAndRegions.BtnInitData'">
          {{ t('basicData.init') }}
        </el-button>
      </template>

      <!-- 自定义名称列渲染 -->
      <template #table-name="{ row }">
        {{ row.name }}({{ row.cnName }})
      </template>

      <!-- 自定义编码列渲染 -->
      <template #table-cca="{ row }">
        {{ row.ccA2 }}/{{ row.ccA3 }}/{{ row.ccN3 }}
      </template>

      <!-- 自定义区域列渲染 -->
      <template #table-region="{ row }">
        <p>{{ row.region }}/{{ row.subRegion }}</p>
        <p>{{ row.cnRegion }}/{{ row.cnSubRegion }}</p>
      </template>

      <!-- 自定义币种列渲染 -->
      <template #table-currency="{ row }">
        {{ row.currency }}
      </template>

      <!-- 自定义区号列渲染 -->
      <template #table-timeZone="{ row }">
        {{ row.areaPhoneCode }}
      </template>
    </common-table>

    <!-- 编辑对话框 -->
    <CountryRegionEdit ref="editRef" @fetchData="fetchData" />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import {
  getCountryInfoPaged,
  deleteCountryInfo,
  syncCountries,
} from "@/api/baseData";
import CountryRegionEdit from "./components/CountryRegionEdit.vue";

const { t } = useI18n();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  name: "",
  cca2: "",
  region: "",
  Currency: "",
});

// 搜索表单配置 - 使用 computed 确保语言切换时自动更新
const filterList = computed(() => [
  {
    label: t("basicData.name"),
    name: "name",
    type: "text",
  },
  {
    label: t("basicData.code"),
    name: "cca2",
    type: "text",
  },
  {
    label: t("basicData.region"),
    name: "region",
    type: "text",
  },
  {
    label: t("basicData.referenceCurrency"),
    name: "Currency",
    type: "text",
  },
]);

// 表格列配置 - 使用 computed 确保语言切换时自动更新
const tableColumns = computed(() => [
  { prop: "name", label: t("basicData.name"), type: "slot" },
  { prop: "cca", label: t("basicData.code"), type: "slot" },
  { prop: "region", label: t("basicData.region"), type: "slot" },
  { prop: "currency", label: t("basicData.referenceCurrency"), type: "slot" },
  { prop: "timeZone", label: t("basicData.areaCode"), type: "slot" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const syncLoading = ref(false);
const tableRef = ref(null);
const editRef = ref(null);

// 操作按钮配置 - 使用 computed 确保语言切换时自动更新
const operationButtons = computed(() => [
  {
    label: t("common.edit"),
    type: "text",
    permission: "Pages.BasicData.CountriesAndRegions.BtnEdit",
    onClick: (row) => {
      editRef.value.open(row);
    },
  },
  {
    label: t("common.delete"),
    type: "text",
    permission: "Pages.BasicData.CountriesAndRegions.BtnDelete",
    onClick: (row) => {
      handleDelete(row);
    },
  },
]);

// 搜索处理
const handleSearch = () => {
  //searchForm.pageNo = 1;
  fetchData();
};

// 获取表格数据
const fetchData = async () => {
  loading.value = true;
  try {
    searchForm.skipCount = (searchForm.pageNo - 1) * searchForm.maxResultCount;
    getCountryInfoPaged.params = searchForm;
    const { result } = await request(getCountryInfoPaged);
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
    deleteCountryInfo.params = { id: row.id };
    const { success } = await request(deleteCountryInfo);
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

// 同步国家数据
const handleSyncCountries = async () => {
  syncLoading.value = true;
  try {
    const { success } = await request(syncCountries);
    if (success) {
      ElMessage.success(t("basicData.syncSuccess"));
      fetchData();
    }
  } catch (error) {
  } finally {
    syncLoading.value = false;
  }
};

// 初始化
onMounted(() => {
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

.table-toolbar {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  padding: 16px 18px;
  margin-bottom: 18px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}
</style>

