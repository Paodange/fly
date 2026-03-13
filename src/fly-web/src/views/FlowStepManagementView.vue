<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { flowStepApi } from '@/api'
import type { FlowStep, FlowStepParameter, ParameterConstraint, ConstraintRuleDefinition } from '@/types'

// ─── Steps List ──────────────────────────────────────────────────────────────
const steps = ref<FlowStep[]>([])
const loading = ref(false)

// ─── Constraint Rules (built-in, loaded from API) ─────────────────────────────
const constraintRules = ref<ConstraintRuleDefinition[]>([])

// ─── Step Create/Edit Dialog ──────────────────────────────────────────────────
const stepDialogVisible = ref(false)
const stepEditMode = ref(false)
const stepLoading = ref(false)
const currentStep = ref<Partial<FlowStep>>({})
const stepParams = ref<FlowStepParameter[]>([])

// ─── Parameter Create/Edit Sub-Dialog ─────────────────────────────────────────
const paramDialogVisible = ref(false)
const paramEditMode = ref(false)
const paramEditIndex = ref(-1)
const currentParam = ref<Partial<FlowStepParameter>>({})
const newOption = ref('')

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(() => {
  loadSteps()
  loadConstraintRules()
})

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

async function loadConstraintRules() {
  try {
    constraintRules.value = await flowStepApi.constraintRules()
  } catch {
    constraintRules.value = []
  }
}

// ─── Step CRUD ────────────────────────────────────────────────────────────────
function handleCreate() {
  stepEditMode.value = false
  currentStep.value = {
    type: '',
    category: '',
    label: '',
    description: '',
    icon: 'Document',
    color: '#409EFF',
    position: { x: 100, y: 100 }
  }
  stepParams.value = []
  stepDialogVisible.value = true
}

function handleEdit(step: FlowStep) {
  stepEditMode.value = true
  currentStep.value = { ...step, position: { ...step.position } }
  stepParams.value = step.parameters.map(p => ({
    ...p,
    options: [...p.options],
    constraints: p.constraints.map(c => ({ ...c })),
  }))
  stepDialogVisible.value = true
}

async function handleSaveStep() {
  if (!currentStep.value.type?.trim()) {
    ElMessage.warning('请输入步骤类型')
    return
  }
  if (!currentStep.value.category?.trim()) {
    ElMessage.warning('请输入步骤分类')
    return
  }
  if (!currentStep.value.label?.trim()) {
    ElMessage.warning('请输入步骤标签')
    return
  }
  if (!currentStep.value.icon?.trim()) {
    ElMessage.warning('请输入步骤图标')
    return
  }
  if (!currentStep.value.color?.trim()) {
    ElMessage.warning('请输入步骤颜色')
    return
  }
  stepLoading.value = true
  try {
    const payload: Partial<FlowStep> = { ...currentStep.value, parameters: stepParams.value }
    if (stepEditMode.value && currentStep.value.id) {
      await flowStepApi.update(currentStep.value.id, payload as FlowStep)
      ElMessage.success('步骤已更新')
    } else {
      await flowStepApi.create(payload)
      ElMessage.success('步骤已创建')
    }
    stepDialogVisible.value = false
    await loadSteps()
  } catch (e: unknown) {
    ElMessage.error(`保存失败: ${String(e)}`)
  } finally {
    stepLoading.value = false
  }
}

async function handleDeleteStep(step: FlowStep) {
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

// ─── Parameter CRUD (within step dialog) ──────────────────────────────────────
function openAddParam() {
  paramEditMode.value = false
  paramEditIndex.value = -1
  currentParam.value = {
    key: '', label: '', type: 'string', defaultValue: '',
    required: false, unit: '', description: '', options: [], constraints: [],
  }
  newOption.value = ''
  paramDialogVisible.value = true
}

function openEditParam(index: number) {
  paramEditMode.value = true
  paramEditIndex.value = index
  const p = stepParams.value[index]
  if (!p) return
  currentParam.value = {
    ...p,
    options: [...p.options],
    constraints: p.constraints.map((c: ParameterConstraint) => ({ ...c })),
  }
  newOption.value = ''
  paramDialogVisible.value = true
}

function handleSaveParam() {
  if (!currentParam.value.key?.trim()) {
    ElMessage.warning('请输入参数键名')
    return
  }
  if (!currentParam.value.label?.trim()) {
    ElMessage.warning('请输入参数显示名称')
    return
  }
  const param = currentParam.value as FlowStepParameter
  if (paramEditMode.value && paramEditIndex.value >= 0) {
    stepParams.value[paramEditIndex.value] = {
      ...param,
      options: [...param.options],
      constraints: param.constraints.map(c => ({ ...c })),
    }
  } else {
    if (stepParams.value.some(p => p.key === param.key)) {
      ElMessage.warning(`参数键名「${param.key}」已存在`)
      return
    }
    stepParams.value.push({
      ...param,
      options: [...param.options],
      constraints: param.constraints.map(c => ({ ...c })),
    })
  }
  paramDialogVisible.value = false
}

function handleDeleteParam(index: number) {
  stepParams.value.splice(index, 1)
}

// ─── Select options editor ─────────────────────────────────────────────────────
function addOption() {
  const val = newOption.value.trim()
  if (val) {
    if (!currentParam.value.options) currentParam.value.options = []
    if (!currentParam.value.options.includes(val)) {
      currentParam.value.options.push(val)
    }
    newOption.value = ''
  }
}

// ─── Constraints editor ────────────────────────────────────────────────────────
function addConstraint() {
  if (!currentParam.value.constraints) currentParam.value.constraints = []
  currentParam.value.constraints.push({ ruleType: '', value: '', message: '' })
}

function removeConstraint(index: number) {
  currentParam.value.constraints?.splice(index, 1)
}

function onParamTypeChange() {
  if (currentParam.value.type !== 'select') {
    currentParam.value.options = []
  }
  const applicable = getApplicableRules(currentParam.value.type ?? 'string').map(r => r.type)
  if (currentParam.value.constraints) {
    currentParam.value.constraints = currentParam.value.constraints.filter(
      c => !c.ruleType || applicable.includes(c.ruleType),
    )
  }
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function getApplicableRules(paramType: string): ConstraintRuleDefinition[] {
  return constraintRules.value.filter(r => r.applicableTypes.includes(paramType))
}

function getRuleLabel(ruleType: string): string {
  return constraintRules.value.find(r => r.type === ruleType)?.label ?? ruleType
}

const paramTypeOptions = [
  { value: 'string',  label: '字符串' },
  { value: 'number',  label: '数值' },
  { value: 'boolean', label: '布尔值' },
  { value: 'select',  label: '下拉选择' },
]

function getTypeLabel(type: string): string {
  return paramTypeOptions.find(t => t.value === type)?.label ?? type
}
</script>

<template>
  <div class="page">
    <!-- ── Page Header ───────────────────────────────────────────────────── -->
    <div class="page-header">
      <h2 class="page-title">流程步骤管理</h2>
      <el-button type="primary" @click="handleCreate">新建步骤</el-button>
    </div>

    <!-- ── Steps Table ───────────────────────────────────────────────────── -->
    <el-table v-loading="loading" :data="steps" stripe style="width: 100%">
      <el-table-column prop="label" label="步骤标签" min-width="140" />
      <el-table-column prop="type" label="步骤类型" min-width="120" />
      <el-table-column prop="category" label="分类" min-width="100" />
      <el-table-column label="图标" width="80" align="center">
        <template #default="{ row }">
          <el-icon :color="row.color || '#409EFF'">
            <component :is="row.icon || 'Document'" />
          </el-icon>
        </template>
      </el-table-column>
      <el-table-column label="参数" min-width="240">
        <template #default="{ row }">
          <el-tag v-if="row.parameters.length === 0" type="info" size="small">无</el-tag>
          <template v-else>
            <el-tag
              v-for="p in row.parameters.slice(0, 3)"
              :key="p.key"
              size="small"
              style="margin-right: 4px; margin-bottom: 2px"
            >{{ p.label }}<span style="opacity:.6"> ({{ getTypeLabel(p.type) }})</span></el-tag>
            <el-tag v-if="row.parameters.length > 3" size="small" type="info">
              +{{ row.parameters.length - 3 }}
            </el-tag>
          </template>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="160" align="center" fixed="right">
        <template #default="{ row }">
          <el-button size="small" @click="handleEdit(row)">编辑</el-button>
          <el-button size="small" type="danger" @click="handleDeleteStep(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!loading && steps.length === 0" description="暂无步骤，点击「新建步骤」开始" />

    <!-- ── Step Create/Edit Dialog ──────────────────────────────────────── -->
    <el-dialog
      v-model="stepDialogVisible"
      :title="stepEditMode ? '编辑步骤' : '新建步骤'"
      width="780px"
      draggable
      :close-on-click-modal="false"
    >
      <el-form label-width="80px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="步骤类型" required>
              <el-input v-model="currentStep.type" placeholder="例如: pipetting, incubation" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="步骤标签" required>
              <el-input v-model="currentStep.label" placeholder="步骤的显示名称" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="分类" required>
              <el-input v-model="currentStep.category" placeholder="例如: 液体处理, 样品处理" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="图标" required>
              <el-input v-model="currentStep.icon" placeholder="Element Plus图标名称" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="颜色" required>
              <el-color-picker v-model="currentStep.color" show-alpha />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="描述">
              <el-input v-model="currentStep.description" placeholder="步骤说明" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="X 坐标">
              <el-input-number v-model="currentStep.position!.x" :step="10" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Y 坐标">
              <el-input-number v-model="currentStep.position!.y" :step="10" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <!-- Parameters section -->
      <div class="section-header">
        <span class="section-title">参数配置</span>
        <el-button size="small" type="primary" @click="openAddParam">+ 添加参数</el-button>
      </div>

      <el-table v-if="stepParams.length > 0" :data="stepParams" size="small" border style="width: 100%">
        <el-table-column prop="key" label="键名" width="110" />
        <el-table-column prop="label" label="显示名称" min-width="110" />
        <el-table-column label="类型" width="90">
          <template #default="{ row }">
            <el-tag size="small" type="info">{{ getTypeLabel(row.type) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="defaultValue" label="默认值" width="90" />
        <el-table-column label="必填" width="55" align="center">
          <template #default="{ row }">
            <el-icon v-if="row.required" color="#67c23a"><CircleCheck /></el-icon>
          </template>
        </el-table-column>
        <el-table-column prop="unit" label="单位" width="70" />
        <el-table-column label="约束规则" min-width="140">
          <template #default="{ row }">
            <el-tag v-if="row.constraints.length === 0" type="info" size="small">无</el-tag>
            <el-tag
              v-for="c in row.constraints"
              :key="c.ruleType"
              size="small"
              type="warning"
              style="margin-right: 4px; margin-bottom: 2px"
            >{{ getRuleLabel(c.ruleType) }}: {{ c.value }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" align="center">
          <template #default="{ $index }">
            <el-button size="small" @click="openEditParam($index)">编辑</el-button>
            <el-button size="small" type="danger" @click="handleDeleteParam($index)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div v-else class="empty-hint">暂无参数，点击「添加参数」</div>

      <template #footer>
        <el-button @click="stepDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="stepLoading" @click="handleSaveStep">
          {{ stepEditMode ? '更新' : '创建' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- ── Parameter Create/Edit Sub-Dialog ─────────────────────────────── -->
    <el-dialog
      v-model="paramDialogVisible"
      :title="paramEditMode ? '编辑参数' : '添加参数'"
      width="600px"
      draggable
      :close-on-click-modal="false"
      append-to-body
    >
      <el-form label-width="90px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="参数键名" required>
              <el-input
                v-model="currentParam.key"
                placeholder="e.g. volume_ul"
                :disabled="paramEditMode"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="显示名称" required>
              <el-input v-model="currentParam.label" placeholder="显示给用户的名称" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="数据类型">
              <el-select v-model="currentParam.type" style="width: 100%" @change="onParamTypeChange">
                <el-option
                  v-for="t in paramTypeOptions"
                  :key="t.value"
                  :label="t.label"
                  :value="t.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="默认值">
              <el-input v-model="currentParam.defaultValue" placeholder="可选" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="单位">
              <el-input v-model="currentParam.unit" placeholder="例如: ml, °C" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="是否必填">
              <el-switch v-model="currentParam.required" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="描述">
          <el-input
            v-model="currentParam.description"
            type="textarea"
            :rows="2"
            placeholder="参数说明（可选）"
          />
        </el-form-item>

        <!-- Options (only for select type) -->
        <el-form-item v-if="currentParam.type === 'select'" label="选项列表">
          <div class="option-editor">
            <el-tag
              v-for="(opt, i) in currentParam.options"
              :key="i"
              closable
              style="margin-right: 6px; margin-bottom: 6px"
              @close="currentParam.options!.splice(i, 1)"
            >{{ opt }}</el-tag>
            <el-input
              v-model="newOption"
              size="small"
              style="width: 130px"
              placeholder="输入后回车添加"
              @keyup.enter="addOption"
            />
            <el-button size="small" style="margin-left: 6px" @click="addOption">添加</el-button>
          </div>
        </el-form-item>

        <!-- Constraints -->
        <div class="section-header" style="margin-top: 12px; margin-bottom: 8px">
          <span class="section-title">约束规则</span>
          <el-button
            size="small"
            :disabled="getApplicableRules(currentParam.type ?? 'string').length === 0"
            @click="addConstraint"
          >+ 添加约束</el-button>
        </div>

        <div
          v-if="getApplicableRules(currentParam.type ?? 'string').length === 0"
          class="empty-hint"
          style="margin-bottom: 0"
        >该类型暂无可用约束规则</div>

        <template v-else>
          <el-table
            v-if="currentParam.constraints && currentParam.constraints.length > 0"
            :data="currentParam.constraints"
            size="small"
            border
            style="width: 100%"
          >
            <el-table-column label="规则类型" width="140">
              <template #default="{ row }">
                <el-select v-model="row.ruleType" size="small" style="width: 100%">
                  <el-option
                    v-for="r in getApplicableRules(currentParam.type ?? 'string')"
                    :key="r.type"
                    :label="r.label"
                    :value="r.type"
                  >
                    <span>{{ r.label }}</span>
                    <span style="color: #999; font-size: 11px; margin-left: 4px">{{ r.description }}</span>
                  </el-option>
                </el-select>
              </template>
            </el-table-column>
            <el-table-column label="约束值" width="110">
              <template #default="{ row }">
                <el-input v-model="row.value" size="small" placeholder="值" />
              </template>
            </el-table-column>
            <el-table-column label="错误提示（可选）" min-width="140">
              <template #default="{ row }">
                <el-input v-model="row.message" size="small" placeholder="自定义错误信息" />
              </template>
            </el-table-column>
            <el-table-column label="" width="46" align="center">
              <template #default="{ $index }">
                <el-button
                  size="small"
                  type="danger"
                  :icon="'Delete'"
                  circle
                  @click="removeConstraint($index)"
                />
              </template>
            </el-table-column>
          </el-table>
          <div v-else class="empty-hint" style="margin-bottom: 0">暂无约束，点击「添加约束」</div>
        </template>
      </el-form>

      <template #footer>
        <el-button @click="paramDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveParam">确认</el-button>
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

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.empty-hint {
  text-align: center;
  color: #909399;
  font-size: 13px;
  padding: 12px 0;
  margin-bottom: 8px;
}

.option-editor {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
}
</style>
