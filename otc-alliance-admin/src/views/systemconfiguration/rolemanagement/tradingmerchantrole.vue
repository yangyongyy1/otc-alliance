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
      addButtonPermission="Pages.SystemConfiguration.RoleManagement.TradingMerchantRole.BtnAdd"
      :searchForm="searchForm"
      :operationButtons="operationButtons"
    >
      <!-- 自定义状态列渲染 -->
      <template #auditStatusName="{ row }">
        <el-tag :type="row.auditStatus === 0 ? 'success' : 'danger'">
          {{ row.auditStatusName }}
        </el-tag>
      </template>
    </common-table>

    <!-- 新增/编辑对话框 -->
    <TradeRoleAddEdit ref="roleAddRef" @refresh="fetchData" />
  </div>
</template>

<script setup>
import { ref, reactive } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import CommonTable from "@/components/CommonTable/index.vue";
import request from "@/utils/request";
import { SysRoleGetAll, SysRoleDelete } from "@/api/tradePermission";
import { useRouter } from "vue-router";
import TradeRoleAddEdit from "@/views/systemconfiguration/components/TradeRoleAddEdit.vue";

const router = useRouter();

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  skipCount: 0,
  maxResultCount: 20,
});

// 搜索表单配置
const filterList = ref([{}]);

// 表格列配置
const tableColumns = ref([
  { label: "角色ID", prop: "id" },
  { label: "角色名", prop: "roleName" },
  { label: "备注", prop: "remark" },
  { label: "创建时间", prop: "creationTime" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const roleAddRef = ref(null);

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
    label: "编辑",
    type: "text",
    permission:
      "Pages.SystemConfiguration.RoleManagement.TradingMerchantRole.BtnEdit",
    onClick: (row) => {
      handleEdit(row);
    },
  },
  {
    label: "删除",
    type: "text",
    permission:
      "Pages.SystemConfiguration.RoleManagement.TradingMerchantRole.BtnDelete",
    onClick: (row) => {
      handleDelete(row);
    },
  },
  {
    label: "绑定菜单",
    type: "text",
    permission:
      "Pages.SystemConfiguration.RoleManagement.TradingMerchantRole.BtnSetTradingMerchantRolePermission",
    onClick: (row) => {
      router.push({
        path: `/systemConfiguration/roleManagement/tradingmerchantrole_detail/${row.id}`,
      });
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
  roleAddRef.value.open();
};

const handleDelete = async (row) => {
  ElMessageBox.confirm("确认要删除该角色吗？", "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  })
    .then(async () => {
      SysRoleDelete.params = { id: row.id };
      const { result } = await request(SysRoleDelete);
      if (result) {
        ElMessage.success("删除成功");
        fetchData();
      }
    })
    .catch(() => {});
};

// 编辑处理
const handleEdit = (row) => {
  roleAddRef.value.open(row);
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  SysRoleGetAll.params = searchForm;
  const { result } = await request(SysRoleGetAll);
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
