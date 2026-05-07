<template>
  <div class="identity-detail-container" v-loading="loading">
    <div class="header-title">
      <h1>认证详情</h1>
      <div class="header-button">
        <el-button :icon="Refresh" @click="handleRefresh" :loading="loading">刷新</el-button>
        <el-button type="primary" @click="router.back()">返回</el-button>
      </div>
    </div>

    <div class="detail-content">
      <el-descriptions :column="2" border v-if="detailData">
        <el-descriptions-item label="邮箱" :span="2">
          {{ detailData.email || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="名">
          {{ detailData.firstName || "--" }}
          <span v-if="detailData.firstNameEn && detailData.firstNameEn !== detailData.firstName" style="color: #909399; margin-left: 8px;">
            ({{ detailData.firstNameEn }})
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="姓">
          {{ detailData.lastName || "--" }}
          <span v-if="detailData.lastNameEn && detailData.lastNameEn !== detailData.lastName" style="color: #909399; margin-left: 8px;">
            ({{ detailData.lastNameEn }})
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="修正后姓名" :span="2" v-if="detailData.fixedFirstName || detailData.fixedLastName">
          {{ (detailData.fixedFirstName || '') + ' ' + (detailData.fixedLastName || '') }}
          <span v-if="detailData.fixedFirstNameEn || detailData.fixedLastNameEn" style="color: #909399; margin-left: 8px;">
            ({{ (detailData.fixedFirstNameEn || '') + ' ' + (detailData.fixedLastNameEn || '') }})
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="KYC提供商">
          {{ detailData.kycProvider || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="KYC级别">
          {{ getKycLevelLabel(detailData.kycLevelName) }}
        </el-descriptions-item>
        <el-descriptions-item label="认证状态">
          <el-tag :type="getStatusTagType(detailData.kycBizStatus)">
            {{ getStatusLabel(detailData.kycBizStatus) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="申请人ID" :span="2">
          {{ detailData.applicantId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="KYC验证链接" :span="2" v-if="detailData.kycVerificationLink">
          <a :href="detailData.kycVerificationLink" target="_blank" style="color: #409eff; text-decoration: none;">
            {{ detailData.kycVerificationLink }}
          </a>
        </el-descriptions-item>
        <el-descriptions-item label="证件类型" :span="2">
          {{ getDocumentTypeLabel(detailData.idDocType) }}
        </el-descriptions-item>
        <el-descriptions-item label="证件号码" :span="2">
          {{ detailData.idDocNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="证件有效期" :span="2">
          {{ detailData.idDocValidUntil || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="证件正面" :span="2" v-if="detailData.documentPhotoFrontUrl" class="document-item">
          <div class="document-image-wrapper">
            <el-image
              :src="detailData.documentPhotoFrontUrl"
              :preview-src-list="[detailData.documentPhotoFrontUrl]"
              :preview-teleported="true"
              :z-index="99999"
              fit="contain"
              class="document-image"
            />
          </div>
        </el-descriptions-item>
        <el-descriptions-item label="证件背面" :span="2" v-if="detailData.documentPhotoBackUrl" class="document-item">
          <div class="document-image-wrapper">
            <el-image
              :src="detailData.documentPhotoBackUrl"
              :preview-src-list="[detailData.documentPhotoBackUrl]"
              :preview-teleported="true"
              :z-index="99999"
              fit="contain"
              class="document-image"
            />
          </div>
        </el-descriptions-item>
        <el-descriptions-item label="出生日期" :span="2">
          {{ detailData.dateOfBirth || "--" }}
          <span v-if="detailData.fixedDob && detailData.fixedDob !== detailData.dateOfBirth" style="color: #909399; margin-left: 8px;">
            (修正后: {{ detailData.fixedDob }})
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="性别">
          {{ detailData.gender || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="国籍">
          {{ detailData.nationality || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="税务居住国" :span="2">
          {{ detailData.taxResidenceCountry || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="地址" :span="2">
          {{ detailData.formattedAddress || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="街道">
          {{ detailData.street || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="城市">
          {{ detailData.town || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="州/省">
          {{ detailData.state || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="邮编">
          {{ detailData.postCode || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="申请人类型">
          {{ detailData.applicantType || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="申请人平台">
          {{ detailData.applicantPlatform || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="IP国家" :span="2">
          {{ detailData.ipCountry || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="审核状态" :span="2">
          {{ detailData.reviewStatus || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="审核结果">
          {{ detailData.reviewAnswer || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="审核级别">
          {{ detailData.reviewLevel || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="审核日期" :span="2">
          {{ detailData.reviewDate || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="有自拍" v-if="detailData.hasSelfie !== null">
          {{ detailData.hasSelfie ? '是' : '否' }}
        </el-descriptions-item>
        <el-descriptions-item label="有视频" v-if="detailData.hasVideo !== null">
          {{ detailData.hasVideo ? '是' : '否' }}
        </el-descriptions-item>
        <el-descriptions-item label="有地址证明" v-if="detailData.hasProofOfAddress !== null">
          {{ detailData.hasProofOfAddress ? '是' : '否' }}
        </el-descriptions-item>
        <el-descriptions-item label="驳回原因" :span="2" v-if="detailData.moderationComment || detailData.reviewRejectType">
          <span style="color: #f56c6c;">{{ detailData.moderationComment || detailData.reviewRejectType || "--" }}</span>
        </el-descriptions-item>
        <el-descriptions-item label="平台用户ID" :span="2">
          {{ detailData.userId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="KYC渠道产品类型">
          {{ getKycChannelProductTypesLabel(detailData.kycChannelProductTypes) }}
        </el-descriptions-item>
        <el-descriptions-item label="KYC类型">
          {{ getBusinessUserTypeLabel(detailData.kycType) }}
        </el-descriptions-item>
        <el-descriptions-item label="KYC渠道状态" :span="2" v-if="detailData.kycChannelStatus !== null">
          {{ detailData.kycChannelStatus || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="检查ID" :span="2">
          {{ detailData.inspectionId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="外部用户ID" :span="2">
          {{ detailData.externalUserId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="客户端ID">
          {{ detailData.clientId || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="API密钥">
          {{ detailData.apiKey || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="国家" :span="2">
          {{ detailData.country || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="建筑物编号" :span="2">
          {{ detailData.buildingNumber || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="证件国家" :span="2">
          {{ detailData.idDocCountry || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="审核尝试次数" :span="2">
          {{ detailData.reviewAttemptCount ?? "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="优先级" :span="2" v-if="detailData.priority !== null">
          {{ detailData.priority ?? "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="客户端评论" :span="2" v-if="detailData.clientComment">
          {{ detailData.clientComment }}
        </el-descriptions-item>
        <el-descriptions-item label="拒绝标签" :span="2" v-if="detailData.rejectLabels">
          {{ detailData.rejectLabels }}
        </el-descriptions-item>
        <el-descriptions-item label="协议接受时间" :span="2" v-if="detailData.agreementAcceptedAt">
          {{ detailData.agreementAcceptedAt }}
        </el-descriptions-item>
        <el-descriptions-item label="协议来源" :span="2" v-if="detailData.agreementSource">
          {{ detailData.agreementSource }}
        </el-descriptions-item>
        <el-descriptions-item label="就业状态" :span="2" v-if="detailData.employmentStatus">
          {{ detailData.employmentStatus }}
        </el-descriptions-item>
        <el-descriptions-item label="年收入" :span="2" v-if="detailData.annualIncome">
          {{ detailData.annualIncome }}
        </el-descriptions-item>
        <el-descriptions-item label="公司名称" :span="2" v-if="detailData.companyName">
          {{ detailData.companyName }}
        </el-descriptions-item>
        <el-descriptions-item label="公司注册号" :span="2" v-if="detailData.companyRegistrationNumber">
          {{ detailData.companyRegistrationNumber }}
        </el-descriptions-item>
        <el-descriptions-item label="公司国家" :span="2" v-if="detailData.companyCountry">
          {{ detailData.companyCountry }}
        </el-descriptions-item>
        <el-descriptions-item label="公司法定地址" :span="2" v-if="detailData.companyLegalAddress">
          {{ detailData.companyLegalAddress }}
        </el-descriptions-item>
        <el-descriptions-item label="公司邮箱" :span="2" v-if="detailData.companyEmail">
          {{ detailData.companyEmail }}
        </el-descriptions-item>
        <el-descriptions-item label="所有权结构深度" :span="2" v-if="detailData.ownershipStructureDepth !== null">
          {{ detailData.ownershipStructureDepth }}
        </el-descriptions-item>
        <el-descriptions-item label="跳过的受益人类型" :span="2" v-if="detailData.skippedBeneficiaryTypes">
          {{ detailData.skippedBeneficiaryTypes }}
        </el-descriptions-item>
        <el-descriptions-item label="创建时间">
          {{ detailData.creationTime || "--" }}
        </el-descriptions-item>
        <el-descriptions-item label="更新时间">
          {{ detailData.lastModificationTime || "--" }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- 动态表单 - 普通字段 -->
      <el-descriptions :column="3" border v-if="dynamicFormFields.length > 0" class="dynamic-form-section">
        <el-descriptions-item :label="field.label"  v-for="field in dynamicFormFields" :key="field.key">
          {{ field.value }}
        </el-descriptions-item>
      </el-descriptions>

      <!-- 动态表单 - 文件字段 -->
      <el-descriptions :column="2" border v-if="dynamicFormFiles.length > 0" class="dynamic-form-section">
        <el-descriptions-item :label="file.label" :span="2" v-for="file in dynamicFormFiles" :key="file.key" class="file-item">
          <div class="file-content">
            <el-image
              v-if="isImageFile(file.value)"
              :src="file.value"
              :preview-src-list="[file.value]"
              :preview-teleported="true"
              :z-index="99999"
              fit="contain"
              class="file-preview-image"
            />
            <div v-else-if="isPdfFile(file.value)" class="pdf-file">
              <el-icon :size="40"><Document /></el-icon>
              <a :href="file.value" target="_blank" class="pdf-link">查看PDF</a>
            </div>
            <a v-else :href="file.value" target="_blank" class="file-link">{{ file.value }}</a>
          </div>
        </el-descriptions-item>
      </el-descriptions>

      <!-- KYC申请人证件文件 -->
      <div v-if="kycApplicantDocuments.length > 0" class="kyc-documents-section">
        <h3 class="section-title">KYC证件文件</h3>
        <div class="documents-grid">
          <div v-for="doc in kycApplicantDocuments" :key="doc.id" class="document-card">
            <div class="document-header">
              <span class="document-type">{{ getDocumentTypeLabel(doc.idDocType) }}</span>
              <el-tag :type="getReviewAnswerTagType(doc.reviewAnswer)" size="small">
                {{ doc.reviewAnswer || "--" }}
              </el-tag>
            </div>
            <div class="document-image-container">
              <el-image
                :src="getDocumentImageUrl(doc.url)"
                :preview-src-list="getPreviewImageList()"
                :initial-index="kycApplicantDocuments.findIndex(d => d.id === doc.id)"
                :preview-teleported="true"
                :z-index="99999"
                fit="contain"
                class="document-card-image"
                :lazy="true"
                :hide-on-click-modal="true"
              >
                <template #error>
                  <div class="image-error">
                    <el-icon><Document /></el-icon>
                    <span>图片加载失败</span>
                  </div>
                </template>
                <template #placeholder>
                  <div class="image-placeholder">
                    <el-icon class="is-loading"><Loading /></el-icon>
                    <span>加载中...</span>
                  </div>
                </template>
              </el-image>
              <div class="preview-hint">
                <el-icon><ZoomIn /></el-icon>
              </div>
            </div>
            <div class="document-info">
              <div class="info-item">
                <span class="info-label">位置：</span>
                <span>{{ getImageSlotLabel(doc.imageSlot) }}</span>
              </div>
              <div class="info-item" v-if="doc.country">
                <span class="info-label">国家：</span>
                <span>{{ doc.country }}</span>
              </div>
              <div class="info-item" v-if="doc.moderationComment">
                <span class="info-label">审核评论：</span>
                <span style="color: #f56c6c;">{{ doc.moderationComment }}</span>
              </div>
              <div class="info-item" v-if="doc.clientComment">
                <span class="info-label">客户端评论：</span>
                <span>{{ doc.clientComment }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">创建时间：</span>
                <span>{{ doc.creationTime || "--" }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import { Refresh, Document, ZoomIn, Loading } from "@element-plus/icons-vue";
import { getEnum } from "@/api/enum";
import request from "@/utils/request";
import { getUserIdentitiesByUserId } from "@/api/userManagement";

const router = useRouter();
const route = useRoute();

const loading = ref(false);
const detailData = ref(null);
const statusOptions = ref([]);
const kycLevelOptions = ref([]);
const kycChannelProductTypesOptions = ref([]);
const businessUserTypeOptions = ref([]);

// 初始化选项数据
const initOptions = async () => {
  try {
    const [statusResult, kycLevelResult, kycChannelProductTypesResult, businessUserTypeResult] = await Promise.all([
      getEnum("KycBizStatus"),
      getEnum("VAUserKYCLevel"),
      getEnum("KycChannelProductTypes"),
      getEnum("BusinessUserType"),
    ]);

    statusOptions.value = (statusResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    kycLevelOptions.value = (kycLevelResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    kycChannelProductTypesOptions.value = (kycChannelProductTypesResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));

    businessUserTypeOptions.value = (businessUserTypeResult.result || []).map(item => ({
      label: item.displayName,
      value: item.value,
    }));
  } catch (error) {
  }
};

// 获取状态标签类型
const getStatusTagType = (status) => {
  // 处理 null/undefined 值
  if (status === null || status === undefined) {
    return "info";
  }
  // 根据原型图：审核中=warning，驳回=danger，通过=success
  const statusMap = {
    [-1]: "info",      // 未开始
    0: "info",         // 待提交
    1: "warning",      // 审核中
    2: "success",      // 已通过
    3: "danger",       // 已拒绝
    4: "warning",      // 需要重新提交
  };
  return statusMap[status] || "info";
};

// 获取状态标签文本
const getStatusLabel = (status) => {
  const option = statusOptions.value.find(item => item.value === status);
  return option ? option.label : "--";
};

// 获取KYC级别标签
const getKycLevelLabel = (kycLevelName) => {
  if (kycLevelName === null || kycLevelName === undefined) return "--";
  const option = kycLevelOptions.value.find(item => item.value === kycLevelName);
  return option ? option.label : kycLevelName;
};

// 获取KYC渠道产品类型标签
const getKycChannelProductTypesLabel = (value) => {
  if (value === null || value === undefined) return "--";
  const option = kycChannelProductTypesOptions.value.find(item => item.value === value);
  return option ? option.label : value;
};

// 获取业务用户类型标签
const getBusinessUserTypeLabel = (value) => {
  if (value === null || value === undefined) return "--";
  const option = businessUserTypeOptions.value.find(item => item.value === value);
  return option ? option.label : value;
};


const documentTypeOptions = ref([]);
const initDocumentTypeOptions = async () => {
  try {
    const { result } = await getEnum("DocumentType");
    documentTypeOptions.value = result || [];
  } catch (error) {
  }
};

// 获取证件类型标签
const getDocumentTypeLabel = (documentType) => {
  if (!documentType) return "--";
  // ID_CARD, PASSPORT 等直接显示，或从枚举中查找
  const option = documentTypeOptions.value.find(item => item.value === documentType);
  return option ? option.displayName : documentType;
};

// 判断是否是图片文件
const isImageFile = (url) => {
  if (!url || typeof url !== 'string') return false;
  const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg'];
  const lowerUrl = url.toLowerCase();
  return imageExtensions.some(ext => lowerUrl.includes(ext)) || lowerUrl.match(/\.(jpg|jpeg|png|gif|bmp|webp|svg)(\?|$)/i);
};

// 判断是否是PDF文件
const isPdfFile = (url) => {
  if (!url || typeof url !== 'string') return false;
  const lowerUrl = url.toLowerCase();
  return lowerUrl.includes('.pdf') || lowerUrl.match(/\.pdf(\?|$)/i);
};

// 判断是否是文件链接
const isFileLink = (value) => {
  if (!value || typeof value !== 'string') return false;
  // 检查是否是URL
  try {
    new URL(value);
    // 检查是否包含文件扩展名
    return isImageFile(value) || isPdfFile(value) || value.match(/\.(jpg|jpeg|png|gif|bmp|webp|svg|pdf|doc|docx|xls|xlsx|zip|rar)(\?|$)/i);
  } catch {
    return false;
  }
};

// 解析动态表单数据
const dynamicFormFields = computed(() => {
  // 优先从 dynamicForm.dyFormJson 解析，如果没有则从 questionnaireJson 解析
  let formData = null;
  
  if (detailData.value?.dynamicForm?.dyFormJson) {
    try {
      formData = typeof detailData.value.dynamicForm.dyFormJson === 'string' 
        ? JSON.parse(detailData.value.dynamicForm.dyFormJson)
        : detailData.value.dynamicForm.dyFormJson;
    } catch (error) {
    }
  } else if (detailData.value?.questionnaireJson) {
    try {
      formData = typeof detailData.value.questionnaireJson === 'string' 
        ? JSON.parse(detailData.value.questionnaireJson)
        : detailData.value.questionnaireJson;
    } catch (error) {
    }
  }
  
  if (!formData) return [];
  
  try {
    const fields = [];
    // 递归解析嵌套对象
    const parseFields = (obj, prefix = '') => {
      for (const [key, value] of Object.entries(obj)) {
        const fullKey = prefix ? `${prefix}.${key}` : key;
        if (value && typeof value === 'object' && !Array.isArray(value)) {
          parseFields(value, fullKey);
        } else if (value && !isFileLink(value)) {
          fields.push({
            key: fullKey,
            label: fullKey,
            value: Array.isArray(value) ? JSON.stringify(value) : String(value),
            span: 1
          });
        }
      }
    };
    parseFields(formData);
    return fields;
  } catch (error) {
    return [];
  }
});

// KYC申请人证件文件
const kycApplicantDocuments = computed(() => {
  return detailData.value?.kycApplicantDocuments || [];
});

// 获取预览图片列表
const getPreviewImageList = () => {
  return kycApplicantDocuments.value.map(doc => getDocumentImageUrl(doc.url)).filter(url => url);
};

// 获取证件图片完整URL
const getDocumentImageUrl = (url) => {
  if (!url) return "";
  // 如果已经是完整URL，直接返回
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }
  // 否则拼接API基础URL
  const baseUrl = import.meta.env.VITE_API_BASE_URL || '';
  if (!baseUrl) return url;
  
  // 确保URL格式正确
  const cleanBaseUrl = baseUrl.replace(/\/$/, ''); // 移除末尾的斜杠
  const cleanUrl = url.startsWith('/') ? url : `/${url}`; // 确保URL以斜杠开头
  
  return `${cleanBaseUrl}${cleanUrl}`;
};

// 获取图片位置标签
const getImageSlotLabel = (slot) => {
  const slotMap = {
    0: "正面",
    1: "背面",
    2: "其他",
  };
  return slotMap[slot] || `位置${slot}`;
};

// 获取审核结果标签类型
const getReviewAnswerTagType = (answer) => {
  if (!answer) return "info";
  const answerUpper = answer.toUpperCase();
  const typeMap = {
    "GREEN": "success",
    "RED": "danger",
    "YELLOW": "warning",
    "PENDING": "info",
  };
  return typeMap[answerUpper] || "info";
};

// 解析动态表单文件
const dynamicFormFiles = computed(() => {
  // 优先从 dynamicForm.dyFormJson 解析，如果没有则从 fileInfos 或 questionnaireJson 解析
  let formData = null;
  
  if (detailData.value?.dynamicForm?.dyFormJson) {
    try {
      formData = typeof detailData.value.dynamicForm.dyFormJson === 'string' 
        ? JSON.parse(detailData.value.dynamicForm.dyFormJson)
        : detailData.value.dynamicForm.dyFormJson;
    } catch (error) {
    }
  } else if (detailData.value?.fileInfos) {
    formData = detailData.value.fileInfos;
  } else if (detailData.value?.questionnaireJson) {
    try {
      formData = typeof detailData.value.questionnaireJson === 'string' 
        ? JSON.parse(detailData.value.questionnaireJson)
        : detailData.value.questionnaireJson;
    } catch (error) {
    }
  }
  
  if (!formData) return [];
  
  try {
    const files = [];
    // 递归解析嵌套对象
    const parseFiles = (obj, prefix = '') => {
      for (const [key, value] of Object.entries(obj)) {
        const fullKey = prefix ? `${prefix}.${key}` : key;
        if (value && typeof value === 'object' && !Array.isArray(value)) {
          parseFiles(value, fullKey);
        } else if (value && isFileLink(value)) {
          files.push({
            key: fullKey,
            label: fullKey,
            value: value
          });
        }
      }
    };
    parseFiles(formData);
    return files;
  } catch (error) {
    return [];
  }
});


// 获取详情数据
const fetchDetail = async () => {
  loading.value = true;
  try {
    // 通过列表接口获取，然后根据id筛选
    getUserIdentitiesByUserId.params = {
      id: route.params.id,
    };
    const { result } = await request(getUserIdentitiesByUserId);
    detailData.value = result;
  } catch (error) {
  } finally {
    loading.value = false;
  }
};

// 刷新数据
const handleRefresh = () => {
  fetchDetail();
  
};

onMounted(() => {
  initOptions();
  fetchDetail();
  initDocumentTypeOptions();
});
</script>

<style lang="scss" scoped>
.identity-detail-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: 100vh;
  overflow: visible;
}

.header-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  h1 {
    margin: 0;
    font-size: 20px;
    font-weight: 600;
    color: #303133;
  }

  .header-button {
    display: flex;
    gap: 12px;
  }
}

.detail-content {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

:deep(.el-descriptions) {
  .el-descriptions__label {
    font-weight: 500;
  }

  // 证件图片行的样式
  .document-item {
    .el-descriptions__cell {
      padding: 30px 0 !important;
      vertical-align: middle;
    }
    
    .el-descriptions__content {
      padding: 20px 0 !important;
    }
  }
}

.document-image-wrapper {
  padding: 10px 0;
  min-height: 160px;
  display: flex;
  align-items: center;
}

.dynamic-form-section {
  margin-top: 20px;
}

.file-item {
  .file-content {
    padding: 10px 0;
  }
}

.file-preview-image {
  max-width: 300px;
  max-height: 200px;
  cursor: pointer;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  padding: 4px;

  &:hover {
    border-color: #409eff;
  }

  :deep(.el-image__inner) {
    transition: none;
  }
}

.pdf-file {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 10px;

  .pdf-link {
    color: #409eff;
    text-decoration: none;
    font-size: 14px;

    &:hover {
      text-decoration: underline;
    }
  }
}

.file-link {
  color: #409eff;
  text-decoration: none;
  word-break: break-all;

  &:hover {
    text-decoration: underline;
  }
}

.document-image {
  max-width: 200px;
  max-height: 150px;
  cursor: pointer;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  padding: 4px;

  &:hover {
    border-color: #409eff;
  }

  :deep(.el-image__inner) {
    transition: none;
  }
}

.kyc-documents-section {
  margin-top: 20px;
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);

  .section-title {
    margin: 0 0 20px 0;
    font-size: 18px;
    font-weight: 600;
    color: #303133;
    padding-bottom: 12px;
    border-bottom: 2px solid #409eff;
  }

  .documents-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 20px;

      .document-card {
      border: 1px solid #e4e7ed;
      border-radius: 8px;
      overflow: hidden;
      transition: all 0.3s;
      background: #fafafa;

      &:hover {
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        transform: translateY(-2px);

        .preview-hint {
          opacity: 1;
        }
      }

      .document-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 12px 16px;
        background: #fff;
        border-bottom: 1px solid #e4e7ed;

        .document-type {
          font-weight: 600;
          color: #303133;
          font-size: 14px;
        }
      }

      .document-image-container {
        position: relative;
        padding: 16px;
        background: #fff;
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 200px;

        .document-card-image {
          max-width: 100%;
          max-height: 300px;
          cursor: pointer;
          border: 1px solid #dcdfe6;
          border-radius: 4px;
          padding: 4px;
          transition: all 0.3s;

          &:hover {
            border-color: #409eff;
            box-shadow: 0 2px 8px rgba(64, 158, 255, 0.2);
          }

          :deep(.el-image__inner) {
            transition: none;
          }
        }

        .image-error {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          color: #909399;
          padding: 20px;

          .el-icon {
            font-size: 48px;
            margin-bottom: 8px;
          }

          span {
            font-size: 14px;
          }
        }

        .image-placeholder {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          color: #909399;
          padding: 20px;

          .el-icon {
            font-size: 32px;
            margin-bottom: 8px;
          }

          span {
            font-size: 14px;
          }
        }

        .preview-hint {
          position: absolute;
          bottom: 20px;
          right: 20px;
          display: flex;
          align-items: center;
          gap: 4px;
          background: rgba(0, 0, 0, 0.6);
          color: #fff;
          padding: 6px 12px;
          border-radius: 4px;
          font-size: 12px;
          pointer-events: none;
          opacity: 0;
          transition: opacity 0.3s;
          z-index: 10;

          .el-icon {
            font-size: 14px;
          }
        }
      }

      &:hover .preview-hint {
        opacity: 1;
      }

      .document-info {
        padding: 12px 16px;
        background: #fff;
        border-top: 1px solid #e4e7ed;

        .info-item {
          display: flex;
          margin-bottom: 8px;
          font-size: 13px;

          &:last-child {
            margin-bottom: 0;
          }

          .info-label {
            color: #909399;
            min-width: 80px;
            font-weight: 500;
          }

          span:not(.info-label) {
            color: #606266;
            flex: 1;
            word-break: break-all;
          }
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .kyc-documents-section {
    .documents-grid {
      grid-template-columns: 1fr;
    }
  }
}

</style>

