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
        addButtonPermission="Pages.AllianceManagement.MerchantList.BtnAdd"
        :searchForm="searchForm"
        :operationButtons="operationButtons"
        :operationWidth="380"
        :sortable-fields="['name', 'creationTime','authType']"
      >
        <!-- 关联 code 自定义列：显示 code + Add 链接 -->
        <template #table-relationCode="{ row }">
          <span>{{ row.relationCode || '-' }}</span>
          <el-button type="text" @click="goToMerchantRelationCode(row)" style="margin-left: 8px;">
            Add
          </el-button>
        </template>

        <!-- 时间列 -->
        <template #table-creationTime="{ row }">
          <p>{{ t('common.creationTime') }}：{{ row.creationTime }}</p>
          <p>{{ t('common.lastModificationTime') }}：{{ row.lastModificationTime }}</p>
        </template>

        <template #table-logoUrl="{ row }">
          <img
            :src="row.logoUrl"
            alt="logo"
            v-if="row.logoUrl"
            style="width: 100px; height: 100px;"
          />
        </template>

        <template #table-userAgreementLink="{ row }">
          <a :href="row.userAgreementLink" target="_blank">{{ row.userAgreementLink }}</a>
        </template>
      </common-table>
  
      <!-- 新增/编辑对话框 -->
      <merchantAdd ref="merchantAddRef" @fetchData="fetchData" />
      <merchantSetting ref="merchantSettingRef" @fetchData="fetchData" />
      <merchantPaySettingModal ref="merchantPaySettingModalRef" @fetchData="fetchData" />
      <merchantAdvSetting ref="merchantAdvSettingRef" @fetchData="fetchData" />
    </div>
  </template>
  
  <script setup>
  import { ref, reactive, computed } from "vue";
  import { useI18n } from "vue-i18n";
  import { ElMessage, ElMessageBox } from "element-plus";
  import CommonTable from "@/components/CommonTable/index.vue";
  import request from "@/utils/request";
  import {getEnum} from "@/api/enum";
  import { getMerchantList, getAllianceOptions, deleteMerchantApi } from "@/api/allianceManagement";
  import { useRouter } from "vue-router";
  import merchantAdd from "./components/merchantadd.vue";
  import merchantSetting from "@/views/alliancemanagement/components/merchantSetting.vue";
  import merchantPaySettingModal from "@/views/alliancemanagement/components/merchantPaySettingModal.vue";
  import merchantAdvSetting from "@/views/alliancemanagement/components/merchantAdvSetting.vue";
  
  const router = useRouter();
  const { t } = useI18n();
  
  // 搜索表单数据
  const searchForm = reactive({
    pageNo: 1,
    maxResultCount: 20,
    skipCount: 0,
    allianceId: "",
    name: "",
    creationTimeStart: "",
    creationTimeEnd: "",
    modificationTimeStart: "",
    modificationTimeEnd: "",
  });
  
  const allianceOptions = ref([]);
  const fetchAllianceOptions = async () => {
    const { result } = await request(getAllianceOptions);
    allianceOptions.value = result.map(item=>{
        return{
            label: item.name,
            value: item.id,
        }
    });

  };
  const authTypeOptions = ref([]);
  const authStandardOptions = ref([]);
  const fetchAuthTypeOptions = async () => {
      const { result } = await getEnum("AuthType");
      authTypeOptions.value = result;
  };
  const fetchAuthStandardOptions = async () => {
      const { result } = await getEnum("AuthStandardLevel");
      authStandardOptions.value = result;
  };
  fetchAuthTypeOptions();
  fetchAuthStandardOptions();
  
  fetchAllianceOptions();
  // 搜索表单配置 - 使用 computed 确保语言切换时自动更新
  const filterList = computed(() => [
    {
      label: t("merchant.merchantName"),
      name: "name",
      type: "text",
    },
    {
      label: t("user.allianceName"),
      name: "allianceId",
      type: "select",
      options: allianceOptions.value,
      filterable: true,
      multiple: false,
      width: "180px",
      size: "default",
      placeholder: t("merchant.pleaseSelectAlliance"),
      clearable: true,
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
  
  // 表格列配置 - 使用 computed 确保语言切换时自动更新
  const tableColumns = computed(() => [
    { prop: "name", label: t("merchant.merchantName") },
    { prop: "businessID", label: t("merchant.merchantId") },
    {
      prop: "allianceName",
      label: t("user.allianceName"),
      formatter: (row) => {
        return row.alliance?.name ?? "-";
      },
    },
    {
      prop: "relationCode",
      label: t("merchant.relationCode"),
      type: "slot",
    },
    {
      prop: "creationTime",
      label: t("common.creationTime"),
      width: "180px",
      type: "slot",
    },
  ]);
  
  // 表格数据
  const tableData = ref([]);
  const total = ref(0);
  const loading = ref(false);
  const merchantAddRef = ref(null);
  const merchantSettingRef = ref(null);
  const merchantPaySettingModalRef = ref(null);
  const merchantAdvSettingRef = ref(null);
  
  
  
  // 操作按钮配置 - 使用 computed 确保语言切换时自动更新
  const operationButtons = computed(() => [
    {
      label: t("merchant.authSettings"),
      type: "text",
      permission:
        "Pages.AllianceManagement.MerchantList.BtnSettings",
      onClick: (row) => {
        merchantSettingRef.value.open(row);
      },
    },
    {
      label: t("merchant.functionSettings"),
      type: "text",
      permission:
        "Pages.AllianceManagement.MerchantList.BtnFunctionSettings",
      onClick: (row) => {
        merchantPaySettingModalRef.value.open(row);
      },
    },
    {
      label: t("merchant.paySettings"),
      type: "text",
      permission:
        "Pages.AllianceManagement.MerchantList.BtnPaySettings",
      onClick: (row) => {
        router.push(`/alliancemanagement/merchantlist_detail/${row.id}`);
      },
    },
    {
      label: t("common.delete"),
      type: "text",
      permission:
        "Pages.AllianceManagement.MerchantList.BtnDelete",
      onClick: (row) => {
        deleteMerchant(row);
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
    merchantAddRef.value.open();
  };

  // 跳转到商户关联 code 配置静态路由页
  const goToMerchantRelationCode = (row) => {
    router.push(`/alliancemanagement/merchantSubCode/${row.id}`);
  };
  
  const deleteMerchant = async (row) => {
    try {
      await ElMessageBox.confirm(
        t("merchant.deleteConfirm", { name: row.name || row.businessID || row.id }),
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
      deleteMerchantApi.params = { id: row.id };
      await request(deleteMerchantApi);
      ElMessage.success(t("merchant.deleteSuccess"));
      fetchData();
    } catch {
      // 错误信息已由 request 拦截器统一展示
    }
  };
  // 切换状态
  
  // 获取表格数据
  const fetchData = async (params = {}) => {
    loading.value = true;
    getMerchantList.params = searchForm;
    const { result } = await request(getMerchantList);
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
  </style>
  