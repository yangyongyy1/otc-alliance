<template>
  <div class="dashboard-container">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">
          <el-icon class="title-icon"><DataAnalysis /></el-icon>
          {{ t('dashboard.dataOverview') }}
        </h1>
        <p class="page-description">{{ t('dashboard.realTimeMonitoring') }}</p>
      </div>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-grid">
      <div class="stat-card" data-type="users">
        <div class="card-icon">
          <el-icon><User /></el-icon>
        </div>
        <div class="card-content">
          <div class="card-title">{{ t('dashboard.totalUsers') }}</div>
          <div class="card-value">--</div>
          <div class="card-trend">
            <span class="trend-text">{{ t('dashboard.statisticsInDevelopment') }}</span>
          </div>
        </div>
      </div>

      <div class="stat-card" data-type="active">
        <div class="card-icon">
          <el-icon><TrendCharts /></el-icon>
        </div>
        <div class="card-content">
          <div class="card-title">{{ t('dashboard.todayActive') }}</div>
          <div class="card-value">--</div>
          <div class="card-trend">
            <span class="trend-text">{{ t('dashboard.statisticsInDevelopment') }}</span>
          </div>
        </div>
      </div>

      <div class="stat-card" data-type="orders">
        <div class="card-icon">
          <el-icon><ShoppingCart /></el-icon>
        </div>
        <div class="card-content">
          <div class="card-title">{{ t('dashboard.totalOrders') }}</div>
          <div class="card-value">--</div>
          <div class="card-trend">
            <span class="trend-text">{{ t('dashboard.statisticsInDevelopment') }}</span>
          </div>
        </div>
      </div>

      <div class="stat-card" data-type="revenue">
        <div class="card-icon">
          <el-icon><Money /></el-icon>
        </div>
        <div class="card-content">
          <div class="card-title">{{ t('dashboard.totalRevenue') }}</div>
          <div class="card-value">--</div>
          <div class="card-trend">
            <span class="trend-text">{{ t('dashboard.statisticsInDevelopment') }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 欢迎区域 -->
    <div class="welcome-section">
      <div class="welcome-card">
        <div class="welcome-content">
          <div class="welcome-icon">
            <el-icon><DataAnalysis /></el-icon>
          </div>
          <div class="welcome-text">
            <h2 class="welcome-title">{{ t('dashboard.welcomeToSystem') }}</h2>
            <p class="welcome-description">
              {{ t('dashboard.welcomeDescription') }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- 功能提示区域 -->
    <div class="info-section">
      <div class="info-card">
        <div class="info-header">
          <el-icon class="info-icon"><InfoFilled /></el-icon>
          <span class="info-title">{{ t('dashboard.systemTip') }}</span>
        </div>
        <div class="info-content">
          <div class="info-item">
            <el-icon><CircleCheckFilled /></el-icon>
            <span>{{ t('dashboard.tip1') }}</span>
          </div>
          <div class="info-item">
            <el-icon><CircleCheckFilled /></el-icon>
            <span>{{ t('dashboard.tip2') }}</span>
          </div>
          <div class="info-item">
            <el-icon><CircleCheckFilled /></el-icon>
            <span>{{ t('dashboard.tip3') }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useI18n } from "vue-i18n";
import { DataAnalysis, User, TrendCharts, ShoppingCart, Money, InfoFilled, CircleCheckFilled } from "@element-plus/icons-vue";

const { t } = useI18n();

// 这里可以添加仪表盘的数据获取和处理逻辑
</script>

<style lang="scss" scoped>
.dashboard-container {
  .page-header {
    margin-bottom: 32px;
    
    .header-content {
      .page-title {
        display: flex;
        align-items: center;
        font-size: 32px;
        font-weight: 700;
        color: #1a1a1a;
        margin: 0 0 8px 0;
        
        .title-icon {
          margin-right: 16px;
          color: #667eea;
          font-size: 36px;
        }
      }
      
      .page-description {
        color: #6c757d;
        font-size: 16px;
        margin: 0;
      }
    }
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 24px;
    margin-bottom: 32px;

    .stat-card {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(10px);
      border-radius: 16px;
      padding: 24px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      position: relative;
      overflow: hidden;

      &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: 4px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      }

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
      }

      &[data-type="users"] {
        .card-icon {
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
      }

      &[data-type="active"] {
        .card-icon {
          background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
        }
      }

      &[data-type="orders"] {
        .card-icon {
          background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
        }
      }

      &[data-type="revenue"] {
        .card-icon {
          background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
        }
      }

      .card-icon {
        width: 60px;
        height: 60px;
        border-radius: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
        margin-bottom: 16px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);

        .el-icon {
          font-size: 28px;
          color: white;
        }
      }

      .card-content {
        .card-title {
          font-size: 14px;
          color: #6c757d;
          margin-bottom: 8px;
          font-weight: 500;
        }

      .card-value {
        font-size: 32px;
        font-weight: 700;
        color: #909399;
        margin-bottom: 12px;
        line-height: 1.2;
      }

      .card-trend {
        display: flex;
        align-items: center;
        gap: 6px;

        .trend-text {
          font-size: 14px;
          color: #909399;
        }
      }
      }

    }
  }

  .welcome-section {
    margin-bottom: 24px;

    .welcome-card {
      background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
      border-radius: 16px;
      padding: 40px;
      border: 1px solid rgba(102, 126, 234, 0.2);
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);

      .welcome-content {
        display: flex;
        align-items: center;
        gap: 24px;

        .welcome-icon {
          width: 80px;
          height: 80px;
          border-radius: 20px;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          display: flex;
          align-items: center;
          justify-content: center;
          box-shadow: 0 8px 24px rgba(102, 126, 234, 0.3);
          flex-shrink: 0;

          .el-icon {
            font-size: 40px;
            color: white;
          }
        }

        .welcome-text {
          flex: 1;

          .welcome-title {
            font-size: 28px;
            font-weight: 700;
            color: #1a1a1a;
            margin: 0 0 12px 0;
          }

          .welcome-description {
            font-size: 16px;
            color: #6c757d;
            line-height: 1.6;
            margin: 0;
          }
        }
      }
    }
  }

  .info-section {
    .info-card {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(10px);
      border-radius: 16px;
      padding: 24px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);

      .info-header {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 20px;
        padding-bottom: 16px;
        border-bottom: 1px solid #e4e7ed;

        .info-icon {
          font-size: 20px;
          color: #667eea;
        }

        .info-title {
          font-size: 18px;
          font-weight: 600;
          color: #1a1a1a;
        }
      }

      .info-content {
        display: flex;
        flex-direction: column;
        gap: 16px;

        .info-item {
          display: flex;
          align-items: flex-start;
          gap: 12px;
          padding: 12px;
          background: #f5f7fa;
          border-radius: 8px;
          transition: all 0.3s ease;

          &:hover {
            background: #eef2f7;
            transform: translateX(4px);
          }

          .el-icon {
            font-size: 18px;
            color: #67c23a;
            margin-top: 2px;
            flex-shrink: 0;
          }

          span {
            font-size: 14px;
            color: #606266;
            line-height: 1.6;
          }
        }
      }
    }
  }
}

// 响应式设计
@media screen and (max-width: 768px) {
  .dashboard-container {
    .stats-grid {
      grid-template-columns: 1fr;
    }

    .welcome-section {
      .welcome-card {
        padding: 24px;

        .welcome-content {
          flex-direction: column;
          text-align: center;

          .welcome-icon {
            width: 60px;
            height: 60px;

            .el-icon {
              font-size: 30px;
            }
          }
        }
      }
    }
  }
}
</style>
