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
      addButtonPermission="Pages.SystemConfiguration.GroupNotification.BtnAdd"
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
    <groupnotificationAddEdit
      ref="groupnotificationAddEditRef"
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
  GetNoticeGroupList,
  NoticeGroupDelete,
} from "@/api/groupNoticeSetting";
import { useRouter } from "vue-router";
import GroupnotificationAddEdit from "@/views/systemconfiguration/components/GroupnotificationAddEdit.vue";

const router = useRouter();

const serchApi = GetNoticeGroupList;
const deleteApi = NoticeGroupDelete;

// 搜索表单数据
const searchForm = reactive({
  pageNo: 1,
  maxResultCount: 20,
  skipCount: 0,
  ip: "",
  userName: "",
});

// 搜索表单配置
const filterList = ref([]);

// 表格列配置
const tableColumns = ref([
  { label: "预警项目", prop: "warningTypeName" },
  { label: "群组类型", prop: "noticeTypeName" },
  { label: "群组名称", prop: "noticeGroupName" },
  { label: "创建时间", prop: "creationTime" },
  { label: "创建者", prop: "creatorUserName" },
]);

// 表格数据
const tableData = ref([]);
const total = ref(0);
const loading = ref(false);
const groupnotificationAddEditRef = ref(null);

const operationButtons = ref([
  {
    label: "修改",
    type: "text",
    permission: "Pages.SystemConfiguration.GroupNotification.BtnEdit",
    onClick: (row) => {
      groupnotificationAddEditRef.value.open(row);
    },
  },
  {
    label: "删除",
    type: "text",
    permission: "Pages.SystemConfiguration.GroupNotification.BtnDelete",
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
  groupnotificationAddEditRef.value.open();
};

const handleDelete = async (row) => {
  ElMessageBox.confirm("确认要删除该角色吗？", "提示", {
    confirmButtonText: "确定",
    cancelButtonText: "取消",
    type: "warning",
  }).then(async () => {
    deleteApi.params = { id: row.id };
    const { success } = await request(deleteApi);
    if (success) {
      ElMessage.success("删除成功");
      fetchData();
    }
  });
};

// 获取表格数据
const fetchData = async (params = {}) => {
  loading.value = true;
  serchApi.params = searchForm;
  const { result } = await request(serchApi);
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
