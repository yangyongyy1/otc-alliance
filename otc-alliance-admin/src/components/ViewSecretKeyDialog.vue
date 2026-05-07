<template>
  <el-dialog
    v-model="dialogVisible"
    :title="currentStep === 1 ? '安全认证' : '商户密钥'"
    width="600px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <!-- 步骤1: 安全认证 -->
    <div v-if="currentStep === 1">
      <el-form ref="verifyFormRef" :model="verifyForm" label-width="120px">
        <SecurityVerifyFields v-model="verifyForm" />
      </el-form>
    </div>

    <!-- 步骤2: 显示密钥 -->
    <div v-else-if="currentStep === 2" class="secret-key-content">
      <el-result
        icon="success"
        title="密钥获取成功"
      >
        <template #sub-title>
          <div class="key-info">
            <el-descriptions :column="1" border>
              <el-descriptions-item label="商户ID">
                <div class="key-value">
                  <span>{{ secretKeyData.id }}</span>
                  <el-button
                    type="primary"
                    link
                    @click="handleCopy(secretKeyData.id)"
                  >
                    <el-icon><CopyDocument /></el-icon>
                    复制
                  </el-button>
                </div>
              </el-descriptions-item>
              <el-descriptions-item label="密钥 (Secret Key)">
                <div class="key-value">
                  <span class="secret-key">{{ secretKeyData.secretKey }}</span>
                  <el-button
                    type="primary"
                    link
                    @click="handleCopy(secretKeyData.secretKey)"
                  >
                    <el-icon><CopyDocument /></el-icon>
                    复制
                  </el-button>
                </div>
              </el-descriptions-item>
            </el-descriptions>
            <el-alert
              type="warning"
              :closable="false"
              show-icon
              class="warning-tip"
            >
              <template #title>
                <div>请妥善保管密钥信息，不要泄露给他人</div>
              </template>
            </el-alert>
          </div>
        </template>
      </el-result>
    </div>

    <template #footer>
      <div v-if="currentStep === 1">
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="handleNext">
          下一步
        </el-button>
      </div>
      <div v-else>
        <el-button type="primary" @click="dialogVisible = false">
          关闭
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { CopyDocument } from '@element-plus/icons-vue'
import SecurityVerifyFields from './SecurityVerifyFields.vue'
import request from '@/utils/request'

const dialogVisible = ref(false)
const currentStep = ref(1) // 1: 安全认证, 2: 显示密钥
const loading = ref(false)
const verifyFormRef = ref(null)
const businessId = ref('')

// 安全认证表单
const verifyForm = reactive({
  userPwd: '',
  smssCode: '',
  gaCode: ''
})

// 密钥数据
const secretKeyData = reactive({
  secretKey: '',
  id: ''
})

// 打开对话框
const show = (id) => {
  businessId.value = id
  currentStep.value = 1
  dialogVisible.value = true
  // 重置表单
  Object.keys(verifyForm).forEach(key => {
    verifyForm[key] = ''
  })
  secretKeyData.secretKey = ''
  secretKeyData.id = ''
}

// 处理下一步
const handleNext = async () => {
  // 验证表单
  if (!verifyForm.userPwd) {
    ElMessage.error('请输入登录密码')
    return
  }

  try {
    loading.value = true
    
    // 调用接口获取密钥
    const response = await request({
      url: '/api/services/app/AcquiringBusiness/ViewBusinessSecretKey',
      method: 'post',
      data: {
        businessID: businessId.value,
        ...verifyForm
      }
    })

    if (response.success && response.result) {
      // 更新密钥数据
      secretKeyData.secretKey = response.result.secretKey
      secretKeyData.id = response.result.id
      
      // 进入下一步
      currentStep.value = 2
      ElMessage.success('验证成功')
    } else {
      ElMessage.error(response.error?.message || '验证失败')
    }
  } catch (error) {
    ElMessage.error(error.message || '验证失败，请重试')
  } finally {
    loading.value = false
  }
}

// 处理复制
const handleCopy = async (text) => {
  try {
    // 优先使用现代 Clipboard API
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(text)
      ElMessage.success('复制成功')
    } else {
      // 降级方案：使用传统的复制方法
      const textarea = document.createElement('textarea')
      textarea.value = text
      textarea.style.position = 'fixed'
      textarea.style.opacity = '0'
      textarea.style.top = '0'
      textarea.style.left = '0'
      document.body.appendChild(textarea)
      textarea.focus()
      textarea.select()
      
      try {
        const successful = document.execCommand('copy')
        if (successful) {
          ElMessage.success('复制成功')
        } else {
          ElMessage.error('复制失败，请手动复制')
        }
      } catch (err) {
        ElMessage.error('复制失败，请手动复制')
      }
      
      document.body.removeChild(textarea)
    }
  } catch (error) {
    ElMessage.error('复制失败，请手动复制')
  }
}

// 关闭对话框
const handleClose = () => {
  currentStep.value = 1
  dialogVisible.value = false
}

defineExpose({
  show
})
</script>

<style lang="scss" scoped>
.secret-key-content {
  padding: 20px 0;

  .key-info {
    margin-top: 20px;
    text-align: left;

    :deep(.el-descriptions) {
      margin-bottom: 20px;

      .el-descriptions__label {
        width: 180px;
        font-weight: 600;
      }

      .el-descriptions__cell {
        padding: 16px;
      }
    }

    .key-value {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      width: 100%;

      span {
        flex: 1;
        word-break: break-all;
        font-family: 'Courier New', monospace;
        font-size: 14px;
        color: #303133;
      }

      .secret-key {
        background: #f5f7fa;
        padding: 8px 12px;
        border-radius: 4px;
        border: 1px solid #e4e7ed;
      }

      .el-button {
        flex-shrink: 0;
      }
    }

    .warning-tip {
      margin-top: 20px;
    }
  }
}

:deep(.el-dialog__body) {
  padding: 20px 30px;
}

:deep(.el-result__title) {
  margin-top: 12px;
}
</style>

