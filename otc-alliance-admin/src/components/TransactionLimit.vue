<template>
  <el-dialog
    v-model="visible"
    title="交易限制"
    width="900px"
    :close-on-click-modal="false"
    append-to-body
    @close="handleClose"
  >
    <div v-loading="loading" class="transaction-limit-body">
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="140px"
        class="transaction-limit-form"
      >
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="计价标准" prop="currency">
              <el-select
                v-model="form.currency"
                placeholder="请选择计价标准"
                filterable
              >
                <el-option
                  v-for="item in normalizedCurrencyOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="单笔最低" prop="singleMinLimit">
              <el-input-number
                v-model="form.singleMinLimit"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入单笔最低金额"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="单笔最高" prop="singleMaxLimit">
              <el-input-number
                v-model="form.singleMaxLimit"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入单笔最高金额"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="每日限制笔数" prop="dailyLimitCount">
              <el-input-number
                v-model="form.dailyLimitCount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每日限制笔数"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="每日累计金额" prop="dailyTotalAmount">
              <el-input-number
                v-model="form.dailyTotalAmount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每日累计金额"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="每周限制笔数" prop="weeklyLimitCount">
              <el-input-number
                v-model="form.weeklyLimitCount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每周限制笔数"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="每周累计金额" prop="weeklyTotalAmount">
              <el-input-number
                v-model="form.weeklyTotalAmount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每周累计金额"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="每月限制笔数" prop="monthlyLimitCount">
              <el-input-number
                v-model="form.monthlyLimitCount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每月限制笔数"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="每月累计金额" prop="monthlyTotalAmount">
              <el-input-number
                v-model="form.monthlyTotalAmount"
                :min="0"
                :max="limitMax"
                :step="1"
                placeholder="请输入每月累计金额"
              />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </div>

    <template #footer>
      <div class="limit-dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit">保存</el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'

const props = defineProps({
  currencyOptions: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['confirm'])

const visible = ref(false)
const loading = ref(false)
const formRef = ref(null)

const limitMax = 1_000_000_000

const defaultForm = () => ({
  currency: '',
  singleMinLimit: null,
  singleMaxLimit: null,
  dailyLimitCount: null,
  dailyTotalAmount: null,
  weeklyLimitCount: null,
  weeklyTotalAmount: null,
  monthlyLimitCount: null,
  monthlyTotalAmount: null
})

const form = reactive(defaultForm())

const normalizedCurrencyOptions = computed(() =>
  (props.currencyOptions || []).map(item => ({
    label: item.label || item.coinName || item.currencyName || item.value || item.coinCode || '',
    value: item.value || item.coinCode || item.currencyCode || item.label || ''
  }))
)

const rules = {
  currency: [{ required: true, message: '请选择计价标准', trigger: 'change' }],
  singleMinLimit: [{ required: true, message: '请输入单笔最低金额', trigger: 'change' }],
  singleMaxLimit: [{ required: true, message: '请输入单笔最高金额', trigger: 'change' }],
  dailyLimitCount: [{ required: true, message: '请输入每日限制笔数', trigger: 'change' }],
  dailyTotalAmount: [{ required: true, message: '请输入每日累计金额', trigger: 'change' }],
  weeklyLimitCount: [{ required: true, message: '请输入每周限制笔数', trigger: 'change' }],
  weeklyTotalAmount: [{ required: true, message: '请输入每周累计金额', trigger: 'change' }],
  monthlyLimitCount: [{ required: true, message: '请输入每月限制笔数', trigger: 'change' }],
  monthlyTotalAmount: [{ required: true, message: '请输入每月累计金额', trigger: 'change' }]
}

const open = (payload = {}) => {
  Object.assign(form, defaultForm(), payload || {})
  visible.value = true
}

const handleSubmit = () => {
  formRef.value?.validate(valid => {
    if (!valid) return
    if (form.singleMinLimit > form.singleMaxLimit) {
      ElMessage.error('单笔最低金额不能大于单笔最高金额')
      return
    }
    emit('confirm', { ...form })
    visible.value = false
  })
}

const handleClose = () => {
  visible.value = false
}

defineExpose({ open })
</script>

<style scoped lang="scss">
.transaction-limit-body {
  min-height: 200px;
}

.transaction-limit-form {
  :deep(.el-input-number) {
    width: 100%;
  }
}

.limit-dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>

