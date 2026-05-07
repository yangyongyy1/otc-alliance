<template>
  <div class="rich-text-editor">
    <el-button-group>
      <el-button
        @click="openEditDialog"
      >
        {{ t('common.edit') }}
      </el-button>
      <el-button
        @click="openPreviewDialog"
      >
        {{ t('common.preview') }}
      </el-button>
    </el-button-group>

    <!-- 编辑弹窗 -->
    <el-dialog
      v-model="editDialogVisible"
      :title="t('components.editContent')"
      width="900px"
      :close-on-click-modal="false"
      @close="handleEditDialogClose"
    >
      <div v-if="editDialogVisible" class="editor-wrapper">
        <Toolbar
          :editor="editorRef"
          :defaultConfig="toolbarConfig"
          style="border-bottom: 1px solid #ccc"
        />
        <Editor
          v-model="editorValue"
          :defaultConfig="editorConfig"
          style="height: 500px; overflow-y: hidden; width: 100%;"
          @onCreated="handleCreated"
        />
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="handleCancel">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" @click="handleEditSave">{{ t('common.confirm') }}</el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 预览弹窗 -->
    <el-dialog
      v-model="previewDialogVisible"
      :title="t('components.contentPreview')"
      width="800px"
      :close-on-click-modal="false"
    >
      <div class="preview-wrapper">
        <div 
          class="preview-content" 
          v-html="previewContent"
        ></div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button type="primary" @click="previewDialogVisible = false">{{ t('common.close') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, shallowRef, onBeforeUnmount, watch, nextTick, computed } from "vue";
import { useI18n } from "vue-i18n";
import '@wangeditor/editor/dist/css/style.css';
import { Editor, Toolbar } from '@wangeditor/editor-for-vue';
import { i18nChangeLanguage } from '@wangeditor/editor';

const { t, locale } = useI18n();

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: '请输入内容...'
  },
  height: {
    type: String,
    default: '300px'
  }
});

const emit = defineEmits(['update:modelValue', 'change']);

const editDialogVisible = ref(false);
const previewDialogVisible = ref(false);
const editorRef = shallowRef();

// 编辑器值（用于弹窗内编辑）
const editorValue = ref(props.modelValue);

// 预览内容
const previewContent = computed(() => {
  return props.modelValue || `<p style="color: #999;">${t('common.noContent')}</p>`;
});

// 监听外部值变化
watch(() => props.modelValue, (newVal) => {
  editorValue.value = newVal;
});

// 打开编辑弹窗
const openEditDialog = () => {
  editorValue.value = props.modelValue;
  // 先重置编辑器引用
  if (editorRef.value) {
    editorRef.value.destroy();
    editorRef.value = null;
  }
  editDialogVisible.value = true;
  // 等待弹窗完全打开后再初始化编辑器
  nextTick(() => {
    // 设置编辑器语言
    const currentLang = locale.value === 'zh-CN' ? 'zh-CN' : 'en';
    i18nChangeLanguage(currentLang);
  });
};

// 打开预览弹窗
const openPreviewDialog = () => {
  previewDialogVisible.value = true;
};

// 取消编辑
const handleCancel = () => {
  editDialogVisible.value = false;
};

// 编辑弹窗关闭时的处理
const handleEditDialogClose = () => {
  // 重置为原始值（取消编辑）
  editorValue.value = props.modelValue;
  // 延迟销毁编辑器实例，确保组件已卸载
  setTimeout(() => {
    if (editorRef.value) {
      editorRef.value.destroy();
      editorRef.value = null;
    }
  }, 100);
};

// 保存编辑内容
const handleEditSave = () => {
  emit('update:modelValue', editorValue.value);
  emit('change', editorValue.value);
  editDialogVisible.value = false;
  // 延迟销毁编辑器实例，确保组件已卸载
  setTimeout(() => {
    if (editorRef.value) {
      editorRef.value.destroy();
      editorRef.value = null;
    }
  }, 100);
};

// 工具栏配置
const toolbarConfig = {
  excludeKeys: []
};

// 获取当前语言
const getCurrentLang = () => {
  return locale.value === 'zh-CN' ? 'zh-CN' : 'en';
};

// 编辑器配置
const editorConfig = computed(() => {
  const currentLang = getCurrentLang();
  
  return {
    placeholder: props.placeholder,
    MENU_CONF: {},
    lang: currentLang
  };
});

// 编辑器创建回调
const handleCreated = (editor) => {
  editorRef.value = editor;
  // 设置编辑器语言
  const currentLang = locale.value === 'zh-CN' ? 'zh-CN' : 'en';
  i18nChangeLanguage(currentLang);
};

// 监听语言变化，更新编辑器语言
watch(() => locale.value, (newLang) => {
  if (editorRef.value) {
    const currentLang = newLang === 'zh-CN' ? 'zh-CN' : 'en';
    i18nChangeLanguage(currentLang);
  }
});

// 组件销毁时，也及时销毁编辑器
onBeforeUnmount(() => {
  if (editorRef.value) {
    editorRef.value.destroy();
  }
});
</script>

<style scoped>
.rich-text-editor {
  width: 100%;
}

.editor-wrapper {
  width: 100%;
  min-width: 0;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  overflow: hidden;
}

.editor-wrapper :deep(.w-e-text-container),
.editor-wrapper :deep(.w-e-text-container-wrapper),
.editor-wrapper :deep(.w-e-toolbar),
.editor-wrapper :deep(.w-e-text-container) {
  width: 100% !important;
  min-width: 0 !important;
}

.editor-wrapper :deep(.w-e-text-container-wrapper) {
  width: 100% !important;
}

.editor-wrapper :deep(.w-e-text-container) {
  width: 100% !important;
}

.editor-wrapper :deep(.w-e-text-container__inner) {
  width: 100% !important;
}

.preview-wrapper {
  min-height: 300px;
  padding: 16px;
  background-color: #fff;
}

.preview-content {
  min-height: 300px;
  line-height: 1.6;
  color: #333;
  word-wrap: break-word;
  word-break: break-all;
}

/* 段落 */
.preview-content :deep(p) {
  margin: 8px 0;
  line-height: 1.6;
}

/* 标题 */
.preview-content :deep(h1) {
  margin: 20px 0 12px 0;
  font-weight: bold;
  font-size: 24px;
  line-height: 1.4;
}

.preview-content :deep(h2) {
  margin: 18px 0 10px 0;
  font-weight: bold;
  font-size: 20px;
  line-height: 1.4;
}

.preview-content :deep(h3) {
  margin: 16px 0 8px 0;
  font-weight: bold;
  font-size: 18px;
  line-height: 1.4;
}

.preview-content :deep(h4),
.preview-content :deep(h5),
.preview-content :deep(h6) {
  margin: 14px 0 8px 0;
  font-weight: bold;
  line-height: 1.4;
}

/* 列表 */
.preview-content :deep(ul),
.preview-content :deep(ol) {
  margin: 8px 0;
  padding-left: 24px;
}

.preview-content :deep(li) {
  margin: 4px 0;
  line-height: 1.6;
}

/* 引用 */
.preview-content :deep(blockquote) {
  margin: 8px 0;
  padding: 12px 16px;
  border-left: 4px solid #ddd;
  background-color: #f5f7fa;
  color: #666;
}

/* 图片 */
.preview-content :deep(img) {
  max-width: 100%;
  height: auto;
  display: block;
  margin: 12px 0;
  border-radius: 4px;
}

/* 链接 */
.preview-content :deep(a) {
  color: #409eff;
  text-decoration: none;
  cursor: pointer;
}

.preview-content :deep(a:hover) {
  color: #66b1ff;
  text-decoration: underline;
}

/* 表格 */
.preview-content :deep(table) {
  width: 100%;
  border-collapse: collapse;
  margin: 12px 0;
  border: 1px solid #ddd;
}

.preview-content :deep(table td),
.preview-content :deep(table th) {
  border: 1px solid #ddd;
  padding: 8px 12px;
  text-align: left;
}

.preview-content :deep(table th) {
  background-color: #f5f7fa;
  font-weight: bold;
}

.preview-content :deep(table tr:nth-child(even)) {
  background-color: #fafafa;
}

/* 文本格式 */
.preview-content :deep(strong),
.preview-content :deep(b) {
  font-weight: bold;
}

.preview-content :deep(em),
.preview-content :deep(i) {
  font-style: italic;
}

.preview-content :deep(u) {
  text-decoration: underline;
}

.preview-content :deep(s),
.preview-content :deep(strike) {
  text-decoration: line-through;
}

.preview-content :deep(code) {
  background-color: #f5f7fa;
  padding: 2px 6px;
  border-radius: 3px;
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.9em;
  color: #e83e8c;
}

.preview-content :deep(pre) {
  background-color: #f5f7fa;
  padding: 12px;
  border-radius: 4px;
  overflow-x: auto;
  margin: 12px 0;
}

.preview-content :deep(pre code) {
  background-color: transparent;
  padding: 0;
  color: #333;
}

/* 分割线 */
.preview-content :deep(hr) {
  border: none;
  border-top: 1px solid #ddd;
  margin: 16px 0;
}

/* 确保所有 HTML 元素都能正确显示 */
.preview-content :deep(*) {
  box-sizing: border-box;
}
</style>

