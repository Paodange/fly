<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, markRaw } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { VueFlow, useVueFlow, type Node, type Edge } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import { MiniMap } from '@vue-flow/minimap'
import { ElMessage } from 'element-plus'
import { useWorkflowStore } from '@/stores/workflows'
import { useNodeDefinitionStore } from '@/stores/nodeDefinitions'
import { useExecutionStore } from '@/stores/executions'
import LabFlowNode from '@/components/LabFlowNode.vue'
import LabContainerNode from '@/components/LabContainerNode.vue'
import type { NodeDefinition } from '@/types'

const route = useRoute()
const router = useRouter()
const workflowStore = useWorkflowStore()
const ndStore = useNodeDefinitionStore()
const executionStore = useExecutionStore()

const { nodes, edges, addNodes, addEdges, onConnect, setNodes, setEdges, onNodeDrag, onNodeDragStop, getIntersectingNodes, removeNodes, removeEdges } = useVueFlow()

const saving = ref(false)
const running = ref(false)
const selectedNode = ref<Node | null>(null)
const showParamPanel = ref(false)
const draggedNode = ref<Node | null>(null)

const workflowId = computed(() => route.params.id as string)
const workflow = computed(() => workflowStore.current)

// Convert backend model ↔ Vue Flow format
function toFlowNodes(backendNodes: typeof workflowStore.current extends null ? never : NonNullable<typeof workflowStore.current>['nodes']): Node[] {
  return (backendNodes ?? []).map((n) => {
    // Container nodes (loop, parallel, etc.) use special node type
    const isContainer = ['loop', 'parallel', 'async', 'decision'].includes(n.type)

    return {
      id: n.id,
      type: isContainer ? 'labContainerNode' : 'labNode',
      position: { x: n.position.x, y: n.position.y },
      data: {
        label: n.label,
        type: n.type,
        parameters: { ...n.parameters },
        parentNode: n.parentNode
      },
      parentNode: n.parentNode, // Vue Flow parent-child relationship
      extent: n.parentNode ? 'parent' : undefined, // Constrain child nodes to parent
    }
  })
}

function toFlowEdges(backendEdges: NonNullable<typeof workflowStore.current>['edges']): Edge[] {
  return (backendEdges ?? []).map((e) => ({
    id: e.id,
    source: e.source,
    target: e.target,
    label: e.label,
    animated: false,
  }))
}

function fromFlowToBackend() {
  return {
    ...workflow.value!,
    nodes: nodes.value.map((n) => ({
      id: n.id,
      type: (n.data as { type: string }).type,
      label: (n.data as { label: string }).label,
      position: { x: n.position.x, y: n.position.y },
      parameters: (n.data as { parameters: Record<string, unknown> }).parameters,
      parentNode: n.parentNode, // Save parent-child relationship
    })),
    edges: edges.value.map((e) => ({
      id: e.id,
      source: e.source,
      target: e.target,
      label: typeof e.label === 'string' ? e.label : undefined,
      condition: undefined,
    })),
  }
}

onMounted(async () => {
  await Promise.all([workflowStore.fetchOne(workflowId.value), ndStore.fetchList()])
  if (!workflow.value) {
    ElMessage.error('未找到该流程')
    router.push('/')
    return
  }
  setNodes(toFlowNodes(workflow.value.nodes))
  setEdges(toFlowEdges(workflow.value.edges))

  // Add keyboard event listener for delete
  window.addEventListener('keydown', onKeyDown)
})

onUnmounted(() => {
  // Remove keyboard event listener
  window.removeEventListener('keydown', onKeyDown)
})

onConnect((params) => {
  addEdges([{ ...params, id: `e-${Date.now()}` }])
})

// Drop node from palette
function onDragOver(e: DragEvent) {
  e.preventDefault()
  if (e.dataTransfer) e.dataTransfer.dropEffect = 'copy'
}

function onDrop(e: DragEvent) {
  const type = e.dataTransfer?.getData('nodeType')
  if (!type) return
  const def = ndStore.getByType(type)
  if (!def) return

  // Get the flow container rect
  const container = document.querySelector('.vue-flow') as HTMLElement
  const rect = container?.getBoundingClientRect()
  const x = e.clientX - (rect?.left ?? 0) - 60
  const y = e.clientY - (rect?.top ?? 0) - 30

  // Container nodes (loop, parallel, etc.) use special node type
  const isContainer = ['loop', 'parallel', 'async', 'decision'].includes(type)

  const id = `node-${Date.now()}`
  addNodes([{
    id,
    type: isContainer ? 'labContainerNode' : 'labNode',
    position: { x, y },
    data: {
      label: def.label,
      type,
      parameters: Object.fromEntries(
        def.parameters.map((p) => [p.key, p.defaultValue ?? null])
      ),
    },
    ...(isContainer && {
      style: {
        width: '350px',
        height: '200px',
      }
    })
  }])
}

// Node click → open param panel
function onNodeClick({ node }: { node: Node }) {
  selectedNode.value = node
  showParamPanel.value = true
}

const selectedDef = computed(() =>
  selectedNode.value ? ndStore.getByType((selectedNode.value.data as { type: string }).type) : null
)

function updateParam(key: string, value: unknown) {
  if (!selectedNode.value) return
  ;(selectedNode.value.data as { parameters: Record<string, unknown> }).parameters[key] = value
  // Force reactivity
  setNodes([...nodes.value])
}

async function save() {
  saving.value = true
  try {
    await workflowStore.save(fromFlowToBackend() as NonNullable<typeof workflowStore.current>)
    ElMessage.success('保存成功')
  } catch (e: unknown) {
    ElMessage.error(`保存失败：${e}`)
  } finally {
    saving.value = false
  }
}

async function runWorkflow() {
  await save()
  running.value = true
  try {
    const exec = await executionStore.start(workflowId.value)
    ElMessage.success('流程已开始运行')
    router.push(`/executions/${exec.id}`)
  } catch (e: unknown) {
    ElMessage.error(`运行失败：${e}`)
  } finally {
    running.value = false
  }
}

const categoryGroups = computed(() => {
  const map: Record<string, NodeDefinition[]> = {}
  for (const def of ndStore.list) {
    if (!map[def.category]) map[def.category] = []
    ;(map[def.category] as NodeDefinition[]).push(def)
  }
  return map
})

// Validate node placement constraints
function validateNodePlacement(nodeType: string, containerType: string): string | null {
  // break and continue can only be placed inside loop containers
  if (nodeType === 'break' || nodeType === 'continue') {
    if (containerType !== 'loop') {
      return `${nodeType === 'break' ? '跳出循环' : '继续循环'}节点只能放置在循环容器中`
    }
  }
  return null
}

// Delete selected nodes and their connected edges
function deleteSelectedNodes() {
  if (!selectedNode.value) return

  const nodeId = selectedNode.value.id
  const nodeType = (selectedNode.value.data as { type: string }).type

  // Prevent deletion of start and end nodes
  if (nodeType === 'start' || nodeType === 'end') {
    ElMessage.warning('不能删除开始或结束节点')
    return
  }

  // Find all child nodes (nodes that have this node as parent)
  const childNodes = nodes.value.filter((n) => n.parentNode === nodeId)
  const nodeIdsToDelete = [nodeId, ...childNodes.map((n) => n.id)]

  // Find all edges connected to these nodes
  const edgesToDelete = edges.value.filter((e) =>
    nodeIdsToDelete.includes(e.source) || nodeIdsToDelete.includes(e.target)
  )

  // Remove edges first
  if (edgesToDelete.length > 0) {
    removeEdges(edgesToDelete.map((e) => e.id))
  }

  // Remove nodes
  removeNodes(nodeIdsToDelete)

  // Clear selection
  selectedNode.value = null
  showParamPanel.value = false

  ElMessage.success(`已删除节点${childNodes.length > 0 ? `及其 ${childNodes.length} 个子节点` : ''}`)
}

// Handle keyboard events for delete
function onKeyDown(e: KeyboardEvent) {
  if (e.key === 'Delete' || e.key === 'Backspace') {
    // Only delete if parameter panel is closed (to avoid interfering with input fields)
    if (!showParamPanel.value && selectedNode.value) {
      e.preventDefault()
      deleteSelectedNodes()
    }
  }
}

// Handle node drag to detect if dropping onto a container
onNodeDrag(({ node }) => {
  draggedNode.value = node
})

onNodeDragStop(({ node }) => {
  // Check if the node was dropped onto a container node
  const intersectingNodes = getIntersectingNodes(node)

  if (intersectingNodes && intersectingNodes.length > 0) {
    // Find the first container node in the intersections
    const containerNode = intersectingNodes.find((n: Node) =>
      ['loop', 'parallel', 'async', 'decision'].includes((n.data as { type: string })?.type)
    )

    if (containerNode && containerNode.id !== node.id && !node.parentNode) {
      // Check if the node being dropped is also a container node
      const nodeType = (node.data as { type: string }).type
      const isNodeContainer = ['loop', 'parallel', 'async', 'decision'].includes(nodeType)

      if (isNodeContainer) {
        ElMessage.warning('容器节点不能嵌套到其他容器节点中')
        draggedNode.value = null
        return
      }

      // Validate placement constraints
      const constraintError = validateNodePlacement(nodeType, (containerNode.data as { type: string }).type)
      if (constraintError) {
        ElMessage.error(constraintError)
        draggedNode.value = null
        return
      }

      // Set the parent relationship
      const targetNode = nodes.value.find((n) => n.id === node.id)
      if (targetNode) {
        targetNode.parentNode = containerNode.id
        ;(targetNode.data as { parentNode?: string }).parentNode = containerNode.id
        targetNode.extent = 'parent'
        // Adjust position to be relative to parent
        targetNode.position = {
          x: node.position.x - containerNode.position.x,
          y: node.position.y - containerNode.position.y
        }
        setNodes([...nodes.value])
        ElMessage.success(`节点已添加到 ${(containerNode.data as { label: string }).label} 中`)
      }
    }
  }
  draggedNode.value = null
})
</script>

<template>
  <div class="editor-root">
    <!-- Toolbar -->
    <div class="toolbar">
      <el-button :icon="'ArrowLeft'" text @click="router.push('/')">返回列表</el-button>
      <span class="wf-name">{{ workflow?.name ?? '…' }}</span>
      <el-button :icon="'Finished'" :loading="saving" @click="save">保存</el-button>
      <el-button type="primary" :icon="'VideoPlay'" :loading="running" @click="runWorkflow">运行</el-button>
    </div>

    <div class="editor-body">
      <!-- Node Palette -->
      <aside class="palette">
        <div class="palette-title">节点类型</div>
        <template v-for="(defs, cat) in categoryGroups" :key="cat">
          <div class="palette-category">{{ cat }}</div>
          <div
            v-for="def in defs"
            :key="def.type"
            class="palette-item"
            :style="{ borderLeftColor: def.color }"
            draggable="true"
            @dragstart="(e) => e.dataTransfer?.setData('nodeType', def.type)"
          >
            <el-icon :color="def.color"><component :is="def.icon" /></el-icon>
            <span>{{ def.label }}</span>
          </div>
        </template>
      </aside>

      <!-- Vue Flow Canvas -->
      <div class="canvas" @dragover="onDragOver" @drop="onDrop">
        <VueFlow
          :node-types="{
            labNode: markRaw(LabFlowNode) as any,
            labContainerNode: markRaw(LabContainerNode) as any
          }"
          fit-view-on-init
          @node-click="onNodeClick"
        >
          <Background />
          <Controls />
          <MiniMap />
        </VueFlow>
      </div>

      <!-- Parameter Panel -->
      <transition name="slide">
        <aside v-if="showParamPanel && selectedNode" class="param-panel">
          <div class="param-header">
            <span>节点参数</span>
            <div class="param-header-actions">
              <el-button
                text
                :icon="'Delete'"
                type="danger"
                @click="deleteSelectedNodes"
                title="删除节点 (Delete)"
              />
              <el-button text :icon="'Close'" @click="showParamPanel = false" />
            </div>
          </div>
          <el-divider style="margin: 8px 0" />
          <el-form label-position="top" size="small">
            <el-form-item label="节点标签">
              <el-input
                :model-value="(selectedNode.data as { label: string }).label"
                @update:model-value="(v: string) => { (selectedNode!.data as { label: string }).label = v; setNodes([...nodes]) }"
              />
            </el-form-item>
            <template v-if="selectedDef">
              <el-form-item
                v-for="param in selectedDef.parameters"
                :key="param.key"
                :label="param.label + (param.unit ? ` (${param.unit})` : '')"
              >
                <el-select
                  v-if="param.type === 'select'"
                  :model-value="(selectedNode.data as { parameters: Record<string, unknown> }).parameters[param.key] as string"
                  @update:model-value="(v: string) => updateParam(param.key, v)"
                >
                  <el-option v-for="opt in param.options" :key="opt" :value="opt" :label="opt" />
                </el-select>
                <el-switch
                  v-else-if="param.type === 'boolean'"
                  :model-value="!!(selectedNode.data as { parameters: Record<string, unknown> }).parameters[param.key]"
                  @update:model-value="(v: boolean) => updateParam(param.key, v)"
                />
                <el-input-number
                  v-else-if="param.type === 'number'"
                  :model-value="Number((selectedNode.data as { parameters: Record<string, unknown> }).parameters[param.key]) || 0"
                  :controls-position="'right'"
                  style="width: 100%"
                  @update:model-value="(v: number) => updateParam(param.key, v)"
                />
                <el-input
                  v-else
                  :model-value="String((selectedNode.data as { parameters: Record<string, unknown> }).parameters[param.key] ?? '')"
                  @update:model-value="(v: string) => updateParam(param.key, v)"
                />
                <div v-if="param.description" class="param-desc">{{ param.description }}</div>
              </el-form-item>
            </template>
          </el-form>
        </aside>
      </transition>
    </div>
  </div>
</template>

<style scoped>
.editor-root {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 80px);  /* minus header */
  margin: -16px;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #fff;
  border-bottom: 1px solid #e4e7ed;
}

.wf-name {
  flex: 1;
  font-weight: 600;
  font-size: 15px;
  color: #303133;
}

.editor-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.palette {
  width: 180px;
  flex-shrink: 0;
  background: #fff;
  border-right: 1px solid #e4e7ed;
  overflow-y: auto;
  padding: 8px 0;
}

.palette-title {
  font-size: 12px;
  font-weight: 600;
  color: #909399;
  padding: 4px 12px 8px;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.palette-category {
  font-size: 11px;
  color: #909399;
  padding: 4px 12px 2px;
  margin-top: 8px;
}

.palette-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  cursor: grab;
  border-left: 3px solid transparent;
  font-size: 13px;
  transition: background 0.15s;
}

.palette-item:hover {
  background: #f5f7fa;
}

.canvas {
  flex: 1;
  overflow: hidden;
}

.param-panel {
  width: 280px;
  flex-shrink: 0;
  background: #fff;
  border-left: 1px solid #e4e7ed;
  overflow-y: auto;
  padding: 12px;
}

.param-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 600;
  font-size: 14px;
}

.param-header-actions {
  display: flex;
  gap: 4px;
}

.param-desc {
  font-size: 11px;
  color: #909399;
  margin-top: 2px;
}

.slide-enter-active,
.slide-leave-active {
  transition: transform 0.2s, opacity 0.2s;
}
.slide-enter-from,
.slide-leave-to {
  transform: translateX(20px);
  opacity: 0;
}
</style>
