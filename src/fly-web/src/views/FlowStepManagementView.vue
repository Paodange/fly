<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { flowStepApi } from '@/api'
import type { FlowStep } from '@/types'

const steps = ref<FlowStep[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const editMode = ref(false)
const currentStep = ref<Partial<FlowStep>>({
  type: '',
  label: '',
  position: { x: 0, y: 0 },
  parameters: {},
})

onMounted(() => loadSteps())

async function loadSteps() {
  loading.value = true
  try {
    steps.value = await flowStepApi.list()
  } catch (e: unknown) {
    ElMessage.error(`加载步骤失败: ${String(e)}`)
  } finally {
    loading.value = false
  }
}

function handleCreate() {
  editMode.value = false
  currentStep.value = {
    type: '',
    label: '',
    position: { x: 100, y: 100 },
    parameters: {},
  }
  dialogVisible.value = true
}

function handleEdit(step: FlowStep) {
  editMode.value = true
  currentStep.value = { ...step, position: { ...step.position }, parameters: { ...step.parameters } }
  dialogVisible.value = true
}

async function handleSave() {
  if (!currentStep.value.type?.trim()) {
    ElMessage.warning('请输入步骤类型')
    return
  }
  if (!currentStep.value.label?.trim()) {
    ElMessage.warning('请输入步骤标签')
    return
  }

  loading.value = true
  try {
    if (editMode.value && currentStep.value.id) {
      await flowStepApi.update(currentStep.value.id, currentStep.value as FlowStep)
      ElMessage.success('步骤已更新')
    } else {
      await flowStepApi.create(currentStep.value)
      ElMessage.success('步骤已创建')
    }
    dialogVisible.value = false
    await loadSteps()
  } catch (e: unknown) {
    ElMessage.error(`保存失败: ${String(e)}`)
  } finally {
    loading.value = false
  }
}

async function handleDelete(step: FlowStep) {
  await ElMessageBox.confirm(`确定删除步骤「${step.label}」？`, '警告', { type: 'warning' })
  loading.value = true
  try {
    await flowStepApi.delete(step.id)
    ElMessage.success('已删除')
    await loadSteps()
  } catch (e: unknown) {
    ElMessage.error(`删除失败: ${String(e)}`)
  } finally {
    loading.value = false
  }
}

function formatPosition(pos: { x: number; y: number }) {
  return `(${pos.x}, ${pos.y})`
}
</script>

<template>
  <div class="page">
    <div class="page-header">
      <h2 class="page-title">流程步骤管理</h2>
      <div class="header-actions">
        <el-button
          type="primary"
          :icon="'Plus'"
          @click="handleCreate"
        >新建步骤</el-button>
      </div>
    </div>

    <el-table
      v-loading="loading"
      :data="steps"
      stripe
      style="width: 100%"
    >
      <el-table-column prop="label" label="步骤标签" min-width="160" />
      <el-table-column prop="type" label="步骤类型" min-width="140" />
      <el-table-column label="位置" width="120">
        <template #default="{ row }">{{ formatPosition(row.position) }}</template>
      </el-table-column>
      <el-table-column label="参数" min-width="200">
        <template #default="{ row }">
          <el-tag v-if="Object.keys(row.parameters).length === 0" type="info" size="small">无</el-tag>
          <el-tooltip v-else placement="top">
            <template #content>
              <div style="max-width: 300px; word-break: break-word">
                {{ JSON.stringify(row.parameters, null, 2) }}
              </div>
            </template>
            <el-tag size="small">{{ Object.keys(row.parameters).length }} 个参数</el-tag>
          </el-tooltip>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="160" align="center" fixed="right">
        <template #default="{ row }">
          <el-button
            size="small"
            :icon="'Edit'"
            @click="handleEdit(row)"
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

    <el-empty
      v-if="!loading && steps.length === 0"
      description="暂无步骤，点击「新建步骤」开始"
    />

    <!-- Create/Edit Step Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="editMode ? '编辑步骤' : '新建步骤'"
      width="520px"
      draggable
    >
      <el-form label-width="90px" @submit.prevent="handleSave">
        <el-form-item label="步骤类型" required>
          <el-input v-model="currentStep.type" placeholder="例如: start, process, decision, end" />
        </el-form-item>
        <el-form-item label="步骤标签" required>
          <el-input v-model="currentStep.label" placeholder="步骤的显示名称" />
        </el-form-item>
        <el-form-item label="X 坐标">
          <el-input-number v-model="currentStep.position!.x" :step="10" />
        </el-form-item>
        <el-form-item label="Y 坐标">
          <el-input-number v-model="currentStep.position!.y" :step="10" />
        </el-form-item>
        <el-form-item label="参数 (JSON)">
          <el-input
            v-model="currentStep.parameters"
            type="textarea"
            rows="4"
            placeholder='{"key": "value"}'
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="handleSave">
          {{ editMode ? '更新' : '创建' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page {
  padding: 0;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.page-title {
  margin: 0;
  font-size: 18px;
}

.header-actions {
  display: flex;
  align-items: center;
}
</style>
