<script setup lang="ts">
import { onMounted, computed, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useExecutionStore } from '@/stores/executions'
import type { ExecutionStatus } from '@/types'

const route = useRoute()
const router = useRouter()
const store = useExecutionStore()

const id = computed(() => route.params.id as string)
const exec = computed(() => store.current)

onMounted(async () => {
  await store.fetchOne(id.value)
  if (exec.value?.status === 'Running' || exec.value?.status === 'Paused') {
    store.startPolling(id.value)
  }
})

onUnmounted(() => store.stopPolling())

const statusMap: Record<ExecutionStatus, { label: string; type: string; step: string }> = {
  Pending:   { label: '等待中', type: 'info',    step: 'wait' },
  Running:   { label: '运行中', type: 'warning', step: 'process' },
  Paused:    { label: '已暂停', type: 'info',    step: 'wait' },
  Completed: { label: '已完成', type: 'success', step: 'finish' },
  Failed:    { label: '失败',   type: 'danger',  step: 'error' },
  Cancelled: { label: '已取消', type: 'info',    step: 'wait' },
}

const logStatusMap: Record<ExecutionStatus, { type: string; icon: string }> = {
  Pending:   { type: 'info',    icon: 'Clock' },
  Running:   { type: 'warning', icon: 'Loading' },
  Paused:    { type: 'info',    icon: 'Pause' },
  Completed: { type: 'success', icon: 'CircleCheck' },
  Failed:    { type: 'danger',  icon: 'CircleClose' },
  Cancelled: { type: 'info',    icon: 'Remove' },
}

const formatDate = (d: string) => new Date(d).toLocaleString('zh-CN')
</script>

<template>
  <div class="page">
    <div class="page-header">
      <el-button text :icon="'ArrowLeft'" @click="router.push('/executions')">返回列表</el-button>
      <h2 class="page-title">执行详情</h2>
      <el-button
        v-if="exec?.status === 'Running'"
        type="danger"
        size="small"
        @click="store.cancel(id)"
      >取消执行</el-button>
    </div>

    <template v-if="exec">
      <!-- Summary card -->
      <el-card class="summary-card">
        <el-descriptions :column="3" border>
          <el-descriptions-item label="流程名称">{{ exec.workflowName }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="statusMap[exec.status as ExecutionStatus]?.type ?? 'info'">
              {{ statusMap[exec.status as ExecutionStatus]?.label ?? exec.status }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="开始时间">{{ formatDate(exec.startedAt) }}</el-descriptions-item>
          <el-descriptions-item label="结束时间">
            {{ exec.finishedAt ? formatDate(exec.finishedAt) : '—' }}
          </el-descriptions-item>
          <el-descriptions-item v-if="exec.errorMessage" label="错误信息" :span="2">
            <el-text type="danger">{{ exec.errorMessage }}</el-text>
          </el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- Execution log timeline -->
      <el-card class="log-card">
        <template #header>执行日志</template>
        <el-timeline v-if="exec.logs.length">
          <el-timeline-item
            v-for="log in exec.logs"
            :key="log.nodeId + log.timestamp"
            :type="logStatusMap[log.status as ExecutionStatus]?.type ?? 'info'"
            :timestamp="new Date(log.timestamp).toLocaleTimeString('zh-CN')"
          >
            <div class="log-item">
              <el-tag size="small" :type="logStatusMap[log.status as ExecutionStatus]?.type ?? 'info'" class="log-tag">
                {{ log.nodeLabel }}
              </el-tag>
              <span class="log-msg">{{ log.message }}</span>
              <span v-if="log.durationMs" class="log-dur">{{ log.durationMs.toFixed(0) }}ms</span>
            </div>
          </el-timeline-item>
        </el-timeline>
        <el-empty v-else description="暂无日志" />

        <div v-if="exec.status === 'Running'" class="running-hint">
          <el-icon class="is-loading"><Loading /></el-icon>
          <span>正在执行节点：{{ exec.currentNodeId }}</span>
        </div>
      </el-card>
    </template>

    <el-skeleton v-else :rows="6" animated />
  </div>
</template>

<style scoped>
.page { padding: 0; }
.page-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}
.page-title { margin: 0; font-size: 18px; flex: 1; }
.summary-card { margin-bottom: 16px; }
.log-card { }
.log-item { display: flex; align-items: center; gap: 8px; }
.log-tag { flex-shrink: 0; }
.log-msg { flex: 1; font-size: 13px; }
.log-dur { color: #909399; font-size: 12px; }
.running-hint {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 16px;
  color: #E6A23C;
  font-size: 13px;
}
</style>
