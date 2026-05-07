<template>
  <div class="detail-page">
    <!-- 顶部标题区域 -->
    <div class="detail-header">
      <div class="title-section">
        <h2 class="title">{{ title }}</h2>
        <div class="subtitle" v-if="subtitle">{{ subtitle }}</div>
      </div>
      <div class="header-actions">
        <slot name="header-actions"></slot>
      </div>
    </div>

    <!-- 详情内容区域 -->
    <div class="detail-content">
      <el-descriptions
        :column="column"
        border
        :label-style="labelStyle"
        :content-style="contentStyle"
      >
        <template v-for="(field, index) in displayFields" :key="index">
          <el-descriptions-item :label="field.label">
            <slot :name="field.prop" :row="detailData">
              {{ formatFieldValue(field, detailData[field.prop]) }}
            </slot>
          </el-descriptions-item>
        </template>
      </el-descriptions>

      <!-- 自定义内容插槽 -->
      <div class="custom-content">
        <slot></slot>
      </div>
    </div>

    <!-- 底部按钮区域 -->
    <div class="detail-footer">
      <el-button @click="handleBack" :icon="Back">返回</el-button>
      <slot name="footer-actions"></slot>
    </div>
  </div>
</template>

<script setup>
import { computed } from "vue";
import { useRouter } from "vue-router";
import { Back } from "@element-plus/icons-vue";

const props = defineProps({
  // 详情数据对象
  detailData: {
    type: Object,
    required: true,
  },
  // 页面标题
  title: {
    type: String,
    default: "详情信息",
  },
  // 副标题
  subtitle: {
    type: String,
    default: "",
  },
  // 字段配置
  fields: {
    type: Array,
    default: () => [],
  },
  // 列数
  column: {
    type: Number,
    default: 2,
  },
  // 标签样式
  labelStyle: {
    type: Object,
    default: () => ({
      width: "120px",
      backgroundColor: "#f5f7fa",
    }),
  },
  // 内容样式
  contentStyle: {
    type: Object,
    default: () => ({
      padding: "12px 20px",
    }),
  },
});

const router = useRouter();

// 计算要显示的字段
const displayFields = computed(() => {
  return props.fields.filter((field) => {
    // 如果字段配置了show属性，则根据show的值决定是否显示
    if (field.hasOwnProperty("show")) {
      return field.show;
    }
    // 默认显示所有字段
    return true;
  });
});

// 格式化字段值
const formatFieldValue = (field, value) => {
  if (value === undefined || value === null) return "-";

  // 如果字段配置了formatter，使用formatter处理
  if (field.formatter) {
    return field.formatter(value, props.detailData);
  }

  // 如果字段配置了type，根据type进行格式化
  switch (field.type) {
    case "date":
      return value ? new Date(value).toLocaleDateString() : "-";
    case "datetime":
      return value ? new Date(value).toLocaleString() : "-";
    case "boolean":
      return value ? "是" : "否";
    case "enum":
      return field.enumMap?.[value] || value;
    default:
      return value;
  }
};

// 返回上一页
const handleBack = () => {
  router.back();
};
</script>

<style lang="scss" scoped>
.detail-page {
  background-color: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  padding: 20px;
  min-height: calc(100vh - 120px);
  display: flex;
  flex-direction: column;

  .detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    padding-bottom: 16px;
    border-bottom: 1px solid #ebeef5;

    .title-section {
      .title {
        font-size: 20px;
        font-weight: 600;
        color: #303133;
        margin: 0;
      }

      .subtitle {
        font-size: 14px;
        color: #909399;
        margin-top: 8px;
      }
    }

    .header-actions {
      display: flex;
      gap: 12px;
    }
  }

  .detail-content {
    flex: 1;
    margin-bottom: 24px;

    .custom-content {
      margin-top: 24px;
    }
  }

  .detail-footer {
    display: flex;
    justify-content: center;
    gap: 16px;
    padding-top: 24px;
    border-top: 1px solid #ebeef5;
  }
}

:deep(.el-descriptions) {
  padding: 20px;
  background-color: #fff;
  border-radius: 4px;

  .el-descriptions__label {
    font-weight: 500;
  }
}
</style>
