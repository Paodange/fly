<script setup lang="ts">
import { computed } from 'vue'
import { Handle, Position } from '@vue-flow/core'
import { useNodeDefinitionStore } from '@/stores/nodeDefinitions'

const props = defineProps<{
  id: string
  data: { label: string; type: string; parameters: Record<string, unknown>; parentNode?: string }
  selected: boolean
}>()

const ndStore = useNodeDefinitionStore()
const def = computed(() => ndStore.getByType(props.data.type))
const color = computed(() => def.value?.color ?? '#909399')
const icon = computed(() => def.value?.icon ?? 'Operation')
const isStart = computed(() => props.data.type === 'start')
const isEnd = computed(() => props.data.type === 'end')
const isNested = computed(() => !!props.data.parentNode)
</script>

<template>
  <div
    class="lab-node"
    :class="{ selected, 'is-start': isStart, 'is-end': isEnd, 'is-nested': isNested }"
    :style="{ '--node-color': color }"
  >
    <Handle v-if="!isStart && !isNested" type="target" :position="Position.Top" />

    <div class="node-icon">
      <el-icon :size="18" :color="color"><component :is="icon" /></el-icon>
    </div>
    <div class="node-label">{{ data.label }}</div>

    <Handle v-if="!isEnd && !isNested" type="source" :position="Position.Bottom" />
  </div>
</template>

<style scoped>
.lab-node {
  background: #fff;
  border: 2px solid var(--node-color);
  border-radius: 8px;
  padding: 8px 16px;
  min-width: 120px;
  text-align: center;
  cursor: grab;
  box-shadow: 0 2px 8px rgba(0,0,0,0.12);
  transition: box-shadow 0.15s;
}
.lab-node.selected {
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--node-color) 30%, transparent);
}
.lab-node.is-start { border-style: dashed; }
.lab-node.is-end   { border-style: dashed; }
.lab-node.is-nested {
  background: color-mix(in srgb, var(--node-color) 8%, #fff);
  border-width: 1.5px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08);
}
.node-icon { margin-bottom: 2px; }
.node-label { font-size: 13px; font-weight: 500; color: #303133; white-space: nowrap; }
</style>
