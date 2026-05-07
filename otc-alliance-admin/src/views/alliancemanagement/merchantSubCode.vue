<template>
  <div class="merchant-subcode-page">
    <!-- 顶部商户名称展示，风格与其他页面保持一致 -->
    <div class="merchant-header">
      <span>{{ t("merchant.merchantName") }}：{{ merchantName || "-" }}</span>
      <el-button type="primary" @click="goToList" style="margin-left: 80%;">{{ t("common.return") }}</el-button>
    </div>

    <!-- 使用 CommonTable，样式与其他列表保持一致 -->
    <common-table
      ref="tableRef"
      :filter-list="filterList"
      :table-columns="tableColumns"
      :table-data="tableData"
      :total="total"
      :loading="loading"
      :searchForm="searchForm"
      :showSelection="false"
      :showOperation="true"
      :operationButtons="operationButtons"
      :operationWidth="120"
      @search="handleSearch"
      @reset="handleReset"
      @add="openAddDialog"
      addButtonPermission="Pages.AllianceManagement.MerchantList.BtnSubCodeSettings"
    >
      <!-- 时间列 slot，与其他页面时间展示保持一致 -->
      <template #table-creationTime="{ row }">
        <p>{{ t("common.creationTime") }}：{{ row.creationTime }}</p>
        <p>{{ t("common.lastModificationTime") }}：{{ row.lastModificationTime }}</p>
      </template>
    </common-table>

    <!-- 新增/编辑弹窗 -->
    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.add')"
      v-model="dialogVisible"
      width="500px"
    >
      <el-form :model="dialogForm" :rules="dialogRules" ref="dialogFormRef" label-width="90px">
        <el-form-item :label="t('merchant.belongsToMerchant')">
          <el-input v-model="merchantName" disabled />
        </el-form-item>
        <el-form-item label="备注" prop="subDescription">
          <el-input
            type="textarea"
            v-model="dialogForm.subDescription"
            :rows="4"
            :placeholder="t('common.pleaseEnter')"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" @click="handleDialogConfirm">{{ t('common.confirm') }}</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { ElMessage } from "element-plus";
import request from "@/utils/request";
import CommonTable from "@/components/CommonTable/index.vue";
import {
  getMerchantSubCodeList,
  createMerchantSubCode,
  updateMerchantSubCode,
  getMerchant,
} from "@/api/allianceManagement";

const route = useRoute();
const { t } = useI18n();

const router = useRouter();

// 搜索表单（交给 CommonTable 管理分页）
const merchantId = Number(route.params.merchantId);
const merchantName = ref("");

const loading = ref(false);
const tableData = ref([]);
const total = ref(0);

const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  merchantId,
  subCode: "",
  subDescription: "",
  // 创建时间 / 修改时间筛选
  creationTimeStart: "",
  creationTimeEnd: "",
  lastModificationTimeStart: "",
  lastModificationTimeEnd: "",
  sorting: "CreationTime desc",
});
// 搜索表单配置（Sub Code + 创建时间 / 修改时间）
const filterList = computed(() => [
  {
    label: "Sub Code",
    name: "subCode",
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
    name1: "lastModificationTimeStart",
    name2: "lastModificationTimeEnd",
    type: "multDatetime",
  },
]);

// 表格列配置
const tableColumns = computed(() => [
  {
    prop: "merchantName",
    label: t("merchant.merchantName"),
    formatter: (row) => row.merchant?.name ?? "-",
  },
  { prop: "subCode", label: "Sub Code" },
  { prop: "subDescription", label: "备注" },
  {
    prop: "creationTime",
    label: t("common.creationTime"),
    width: "220px",
    type: "slot",
  },
]);

// 操作列按钮（保持与其他页面相同写法）
const operationButtons = computed(() => [
  {
    label: t("common.edit"),
    type: "text",
    permission: "Pages.AllianceManagement.MerchantList.BtnSubCodeSettings",
    onClick: (row) => {
      openEditDialog(row);
    },
  },
]);


// 弹窗表单
const dialogVisible = ref(false);
const isEdit = ref(false);
const dialogFormRef = ref(null);
const dialogForm = reactive({
  id: null,
  subDescription: "",
});

const dialogRules = {
  subDescription: [
    { required: true, message: t("common.pleaseEnter"), trigger: "blur" },
  ],
};

const fetchMerchantInfo = async () => {
  try {
    getMerchant.params = { id: merchantId };
    const { result } = await request(getMerchant);
    merchantName.value = result?.name || "";
  } catch {
    // 忽略错误，名称留空
  }
};

const fetchData = async () => {
  loading.value = true;
  try {
    getMerchantSubCodeList.params = {
      pageNo: searchForm.pageNo,
      merchantId: searchForm.merchantId,
      subCode: searchForm.subCode,
      subDescription: searchForm.subDescription,
      creationTimeStart: searchForm.creationTimeStart,
      creationTimeEnd: searchForm.creationTimeEnd,
      lastModificationTimeStart: searchForm.lastModificationTimeStart,
      lastModificationTimeEnd: searchForm.lastModificationTimeEnd,
      skipCount: searchForm.skipCount,
      maxResultCount: searchForm.maxResultCount,
      sorting: searchForm.sorting,
    };
    const { result } = await request(getMerchantSubCodeList);
    tableData.value = result?.items || [];
    total.value = result?.totalCount || 0;
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  searchForm.pageNo = 1;
  searchForm.skipCount = 0;
  fetchData();
};

const handleReset = () => {
  searchForm.subCode = "";
  searchForm.subDescription = "";
  searchForm.pageNo = 1;
  searchForm.skipCount = 0;
  fetchData();
};

const openAddDialog = () => {
  isEdit.value = false;
  dialogForm.id = null;
  dialogForm.subDescription = "";
  dialogVisible.value = true;
};

const openEditDialog = (row) => {
  isEdit.value = true;
  dialogForm.id = row.id;
  dialogForm.subDescription = row.subDescription;
  dialogVisible.value = true;
};

const goToList = () => {
   //路由后退
   router.back();
};

const handleDialogConfirm = () => {
  dialogFormRef.value.validate(async (valid) => {
    if (!valid) return;
    try {
      if (isEdit.value) {
        updateMerchantSubCode.data = {
          id: dialogForm.id,
          subDescription: dialogForm.subDescription,
        };
        await request(updateMerchantSubCode);
      } else {
        createMerchantSubCode.data = {
          merchantId,
          subDescription: dialogForm.subDescription,
        };
        await request(createMerchantSubCode);
      }
      ElMessage.success(t("common.operationSuccess") || "操作成功");
      dialogVisible.value = false;
      fetchData();
    } catch {
      // 错误由拦截器统一处理
    }
  });
};

onMounted(() => {
  fetchMerchantInfo();
  fetchData();
});
</script>

<style scoped>
.merchant-subcode-page {
  padding: 16px;
}

.merchant-header {
  margin-bottom: 12px;
  font-size: 14px;
}
</style>

