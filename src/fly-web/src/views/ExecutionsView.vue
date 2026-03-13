<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useExecutionStore } from '@/stores/executions'
import type { WorkflowExecution, ExecutionStatus } from '@/types'

const router = useRouter()
const store = useExecutionStore()

onMounted(() => store.fetchList())

const statusMap: Record<ExecutionStatus, { label: string; type: string }> = {
  Pending:   { label: '等待中', type: 'info' },
  Running:   { label: '运行中', type: 'warning' },
  Paused:    { label: '已暂停', type: 'info' },
  Completed: { label: '已完成', type: 'success' },
  Failed:    { label: '失败',   type: 'danger' },
  Cancelled: { label: '已取消', type: 'info' },
}

const formatDate = (d: string) => new Date(d).toLocaleString('zh-CN')

function duration(exec: WorkflowExecution): string {
  if (!exec.finishedAt) return '—'
  const ms = new Date(exec.finishedAt).getTime() - new Date(exec.startedAt).getTime()
  const s = Math.round(ms / 1000)
  return s < 60 ? `${s}s` : `${Math.floor(s / 60)}m ${s % 60}s`
}
</script>

<template>
  <div class="page">
    <div class="page-header">
      <h2 class="page-title">执行记录</h2>
      <el-button @click="store.fetchList()">刷新</el-button>
    </div>

    <el-table :data="store.list" stripe style="width: 100%">
      <el-table-column label="流程名称" prop="workflowName" min-width="180" />
      <el-table-column label="状态" width="110" align="center">
        <template #default="{ row }">
          <el-tag :type="statusMap[row.status as ExecutionStatus]?.type ?? 'info'" size="small">
            {{ statusMap[row.status as ExecutionStatus]?.label ?? row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="开始时间" width="180">
        <template #default="{ row }">{{ formatDate(row.startedAt) }}</template>
      </el-table-column>
      <el-table-column label="耗时" width="100">
        <template #default="{ row }">{{ duration(row) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="100" align="center" fixed="right">
        <template #default="{ row }">
          <el-button text type="primary" @click="router.push(`/executions/${row.id}`)">
            详情
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!store.list.length" description="暂无执行记录" />
  </div>
</template>

<style scoped>
.page { padding: 0; }
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-title { margin: 0; font-size: 18px; }
</style>
