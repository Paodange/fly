<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useWorkflowStore } from '@/stores/workflows'
import { useExecutionStore } from '@/stores/executions'
import type { Workflow } from '@/types'

const router = useRouter()
const workflowStore = useWorkflowStore()
const executionStore = useExecutionStore()

const dialogVisible = ref(false)
const newWorkflow = ref({ name: '', description: '' })
const creating = ref(false)
const runningId = ref<string | null>(null)

onMounted(() => workflowStore.fetchList())

const formatDate = (d: string) => new Date(d).toLocaleString('zh-CN')

async function handleCreate() {
  if (!newWorkflow.value.name.trim()) {
    ElMessage.warning('请输入流程名称')
    return
  }
  creating.value = true
  try {
    const wf = await workflowStore.create({
      name: newWorkflow.value.name,
      description: newWorkflow.value.description,
      nodes: [],
      edges: [],
    })
    dialogVisible.value = false
    newWorkflow.value = { name: '', description: '' }
    router.push(`/editor/${wf.id}`)
  } finally {
    creating.value = false
  }
}

async function handleDelete(wf: Workflow) {
  await ElMessageBox.confirm(`确定删除流程「${wf.name}」？`, '警告', { type: 'warning' })
  await workflowStore.remove(wf.id)
  ElMessage.success('已删除')
}

async function handleRun(wf: Workflow) {
  runningId.value = wf.id
  try {
    const exec = await executionStore.start(wf.id)
    ElMessage.success(`流程「${wf.name}」开始运行`)
    router.push(`/executions/${exec.id}`)
  } catch (e: unknown) {
    ElMessage.error(String(e))
  } finally {
    runningId.value = null
  }
}
</script>

<template>
  <div class="page">
    <div class="page-header">
      <h2 class="page-title">工作流列表</h2>
      <el-button type="primary" :icon="'Plus'" @click="dialogVisible = true">新建流程</el-button>
    </div>

    <el-table
      v-loading="workflowStore.loading"
      :data="workflowStore.list"
      stripe
      style="width: 100%"
    >
      <el-table-column prop="name" label="流程名称" min-width="180">
        <template #default="{ row }">
          <el-button text type="primary" @click="router.push(`/editor/${row.id}`)">
            {{ row.name }}
          </el-button>
        </template>
      </el-table-column>
      <el-table-column prop="description" label="描述" min-width="240" show-overflow-tooltip />
      <el-table-column label="节点数" width="90" align="center">
        <template #default="{ row }">{{ row.nodes.length }}</template>
      </el-table-column>
      <el-table-column label="更新时间" width="180">
        <template #default="{ row }">{{ formatDate(row.updatedAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="220" align="center" fixed="right">
        <template #default="{ row }">
          <el-button
            size="small"
            :icon="'VideoPlay'"
            type="success"
            :loading="runningId === row.id"
            @click="handleRun(row)"
          >运行</el-button>
          <el-button
            size="small"
            :icon="'EditPen'"
            @click="router.push(`/editor/${row.id}`)"
          >编辑</el-button>
          <el-button
            size="small"
            :icon="'Delete'"
            type="danger"
            @click="handleDelete(row)"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!workflowStore.loading && workflowStore.list.length === 0" description="暂无流程，点击「新建流程」开始" />

    <!-- New Workflow Dialog -->
    <el-dialog v-model="dialogVisible" title="新建流程" width="420px" draggable>
      <el-form label-width="80px" @submit.prevent="handleCreate">
        <el-form-item label="流程名称" required>
          <el-input v-model="newWorkflow.name" placeholder="请输入流程名称" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="newWorkflow.description" type="textarea" rows="3" placeholder="可选描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="creating" @click="handleCreate">创建并编辑</el-button>
      </template>
    </el-dialog>
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
