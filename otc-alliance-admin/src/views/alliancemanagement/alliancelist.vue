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
        @add="handleAdd"
        addButtonPermission="Pages.AllianceManagement.AllianceList.BtnAdd"
        :searchForm="searchForm"
        :operationButtons="operationButtons"
        :operationWidth="300"
        :sortable-fields="['name', 'creationTime']"
      >
       
        <!-- 自定义状态列渲染 -->
        <template #table-creationTime="{ row }">
          <p>{{ t('common.creationTime') }}：{{ row.creationTime }}</p>
          <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime }}</p>
        </template>

        <template #table-logoUrl="{ row }">
            <img :src="row.logoUrl" alt="logo" v-if="row.logoUrl" style="width: 100px; height: 100px;">
        </template>

        <template #table-userAgreementLink="{ row }">
            <span class="user-agreement-text">{{ row.userAgreementLink }}</span>
        </template>
        
      </common-table>
  
      <!-- 新增/编辑对话框 -->
      <allianceAdd ref="allianceAddRef" @fetchData="fetchData" />
      <allianceSetting ref="allianceSettingRef" @fetchData="fetchData" />
    </div>
  </template>
  
<script setup>
import { ref, reactive } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { getAllianceList, deleteAllianceApi } from "@/api/allianceManagement";
import { useRouter } from "vue-router";
import allianceAdd from "@/views/alliancemanagement/components/allianceAdd.vue";
import allianceSetting from "@/views/alliancemanagement/components/allianceSetting.vue";

const { t } = useI18n();
  
  const router = useRouter();
  
  // 搜索表单数据
  const searchForm = reactive({
    pageNo: 1,
    maxResultCount: 20,
    skipCount: 0,
    userAccount: "",
    isActive: "",
    creationDateStart: "",
    creationDateEnd: "",
  });
  
  // 搜索表单配置
  const filterList = ref([
    {
      label: t("alliance.allianceName"),
      name: "name",
      type: "text",
    },
    {
      label: t("alliance.allianceId"),
      name: "serialNumber",
      type: "text",
    },
    {
      label: t("common.creationTime"),
      name1: "creationTimeStart",
      name2: "creationTimeEnd",
      type: "multDatetime",
    },
    {
        label: t("common.lastModificationTime"),
        name1: "modificationTimeStart",
        name2: "modificationTimeEnd",
        type: "multDatetime",
    }
  ]);
  
  // 表格列配置
  const tableColumns = ref([
    { prop: "name", label: t("alliance.allianceName") },
    { prop: "serialNumber", label: t("alliance.allianceId")},
    { prop: "logoUrl", label: t("alliance.logo"), type: "slot" },
    { prop: "userAgreementLink", label: t("alliance.userAgreement"), type: "slot" },
    { prop: "creationTime", label: t("common.creationTime"), type:'slot'  },
  ]);
  
  // 表格数据
  const tableData = ref([]);
  const total = ref(0);
  const loading = ref(false);
  const allianceAddRef = ref(null);
  const allianceSettingRef = ref(null);
  
  
  // 对话框相关
  const dialogVisible = ref(false);
  const dialogType = ref("add");
  const submitLoading = ref(false);
  const formRef = ref(null);
  const form = reactive({
    merchantName: "",
    merchantNo: "",
    contactPerson: "",
    contactPhone: "",
    status: 1,
  });
  
  const operationButtons = ref([
    {
      label: t("alliance.settings"),
      type: "text",
      permission:
        "Pages.AllianceManagement.AllianceList.BtnSettings",
      onClick: (row) => {
        allianceSettingRef.value.open(row);
      },
    },
    {
      label: t("common.delete"),
      type: "text",
      permission:
        "Pages.AllianceManagement.AllianceList.BtnDelete",
      onClick: (row) => {
        deleteAlliance(row);
      },
    },
    
  ]);
  
  // 搜索处理
  const handleSearch = (form) => {
    // CommonTable 组件已经统一处理了 Sorting 等参数，直接使用 searchForm
    fetchData();
  };
  
  // 重置处理
  const handleReset = () => {
    fetchData();
  };
  
  // 新增处理
  const handleAdd = () => {
    allianceAddRef.value.open();
  };
  
  const deleteAlliance = async (row) => {
    try {
      await ElMessageBox.confirm(
        t("alliance.deleteConfirm", { name: row.name || row.serialNumber || row.id }),
        t("common.delete"),
        {
          confirmButtonText: t("common.confirm"),
          cancelButtonText: t("common.cancel"),
          type: "warning",
        }
      );
    } catch {
      return;
    }
    try {
      deleteAllianceApi.params = { id: row.id };
      await request(deleteAllianceApi);
      ElMessage.success(t("alliance.deleteSuccess"));
      fetchData();
    } catch {
      // 错误信息已由 request 拦截器统一展示
    }
  };
  
  // 切换状态
  
  // 获取表格数据
  const fetchData = async (params = {}) => {
    loading.value = true;
    // 使用 searchForm，CommonTable 组件已经统一处理了 Sorting 等参数
    getAllianceList.params = searchForm;
    const { result } = await request(getAllianceList);
    tableData.value = result.items;
    total.value = result.totalCount;
    loading.value = false;
  };
  
  // 初始化
  fetchData();
  </script>
  
<style lang="scss" scoped>
.acquiring-container {
  width: 100%;
  padding: 20px 0;
  background-color: #f5f7fa;
  min-height: 100%;
}

.user-agreement-text {
  display: inline-block;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
  