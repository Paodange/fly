<script setup lang="ts">
import { computed } from 'vue'
import { Handle, Position } from '@vue-flow/core'
import { useNodeDefinitionStore } from '@/stores/nodeDefinitions'

const props = defineProps<{
  id: string
  data: { label: string; type: string; parameters: Record<string, unknown> }
  selected: boolean
}>()

const ndStore = useNodeDefinitionStore()
const def = computed(() => ndStore.getByType(props.data.type))
const color = computed(() => def.value?.color ?? '#409EFF')
const icon = computed(() => def.value?.icon ?? 'RefreshRight')

// Container nodes that can have children
const isContainer = computed(() =>
  ['loop', 'parallel', 'async', 'decision'].includes(props.data.type)
)
</script>

<template>
  <div
    class="lab-container-node"
    :class="{ selected, 'is-container': isContainer }"
    :style="{ '--node-color': color }"
  >
    <Handle type="target" :position="Position.Top" />

    <div class="container-header">
      <div class="node-icon">
        <el-icon :size="18" :color="color"><component :is="icon" /></el-icon>
      </div>
      <div class="node-label">{{ data.label }}</div>
    </div>

    <!-- Container area for child nodes -->
    <div class="container-body">
      <div class="container-hint">
        将节点拖放到此处以添加到{{ data.label }}中
      </div>
    </div>

    <Handle type="source" :position="Position.Bottom" />
  </div>
</template>

<style scoped>
.lab-container-node {
  background: #fff;
  border: 2px solid var(--node-color);
  border-radius: 12px;
  min-width: 300px;
  min-height: 150px;
  cursor: grab;
  box-shadow: 0 2px 8px rgba(0,0,0,0.12);
  transition: box-shadow 0.15s;
}

.lab-container-node.selected {
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--node-color) 30%, transparent);
}

.container-header {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 8px 16px;
  background: color-mix(in srgb, var(--node-color) 10%, transparent);
  border-radius: 10px 10px 0 0;
  border-bottom: 2px dashed var(--node-color);
}

.node-icon {
  display: flex;
  align-items: center;
}

.node-label {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
  white-space: nowrap;
}

.container-body {
  padding: 20px;
  min-height: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, var(--node-color) 5%, transparent);
  border-radius: 0 0 10px 10px;
}

.container-hint {
  font-size: 12px;
  color: #909399;
  text-align: center;
  font-style: italic;
}
</style>
