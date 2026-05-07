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
      addButtonPermission="Pages.SystemConfiguration.IpWhiteListConfiguration.BtnAdd"
      :searchForm="searchForm"
      :operationButtons="operationButtons"
    >
      <!-- 自定义状态列渲染 -->
      <template #table-createType="{ row }">
        <span v-if="row.createType == 0">API</span>
        <span v-if="row.createType == 1">Web</span>
      </template>

      <template #table-sourceTag="{ row }">
        <span v-if="row.sourceTag == '0'">sunpay</span>
        <span v-if="row.sourceTag == '1'">klicklpay</span>
      </template>
    </common-table>

    <!-- 新增/编辑对话框 -->
    <ipwhitelistconfigurationAddEdit
      ref="ipwhitelistconfigurationAddEditRef"
      @refresh="fetchData"
    />
  </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import {
  CloudFlareWhiteGetList,
  CloudFlareWhiteDelete,
} from "@/api/systemConfig";
import { useRouter } from "vue-router";
import ipwhitelistconfigurationAddEdit from "@/views/systemconfiguration/components/ipwhitelistconfigurationAddEdit.vue";

const router = useRouter();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  ip: "",
  userName: "",
});

// 搜索表单配置
const filterList = ref([
  { label: "ip", name: "ip", type: "text" },
  { label: "操作人", name: "userName", type: "text" },
  { label: "商家名称", name: "businessName", type: "text" },
]);

// 表格列配置
const tableColumns = ref([
  { prop: "userName", label: "操作人" }, // 让这一列自适应
  { prop: "creationTime", label: "操作时间" },
  { prop: "ip", label: "IP" },
  { prop: "createType", label: "类型", type: "slot" },
  { prop: "sourceTag", label: "标签", type: "slot" },
  { prop: "businessId", label: "商家ID" },
  { prop: "businessName", label: "商家名称" },
  { prop: "备注", label: "备注" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const ipwhitelistconfigurationAddEditRef = ref(null);

const operationButtons = ref([
  {
    label: "删除",
    type: "text",
    permission: "Pages.SystemConfiguration.IpWhiteListConfiguration.BtnDelete",
    onClick: (row) => {
      handleDelete(row);
    },
  },
]);

// 搜索处理
const handleSearch = (form) => {
  // TODO: 实现搜索逻辑
  fetchData(form);
};

// 重置处理
const handleReset = () => {
  fetchData();
};

// 新增处理
const handleAdd = () => {
  ipwhitelistconfigurationAddEditRef.value.open();
};

const handleDelete = async (row) => {
  ElMessageBox.confirm("确认要删除该角色吗？", "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
    .then(async () => {
      CloudFlareWhiteDelete.params = { id: row.id };
      const { success } = await request(CloudFlareWhiteDelete);
      if (success) {
        ElMessage.success("删除成功");
        fetchData();
      }
    })
    .catch(() => {});
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  CloudFlareWhiteGetList.params = searchForm;
  const { result } = await request(CloudFlareWhiteGetList);
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
