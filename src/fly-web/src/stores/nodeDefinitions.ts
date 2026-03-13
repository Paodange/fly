import { defineStore } from 'pinia'
import { ref } from 'vue'
import { nodeDefinitionApi } from '@/api'
import type { NodeDefinition } from '@/types'

export const useNodeDefinitionStore = defineStore('nodeDefinitions', () => {
  const list = ref<NodeDefinition[]>([])
  const byType = ref<Record<string, NodeDefinition>>({})

  async function fetchList() {
    if (list.value.length > 0) return
    list.value = await nodeDefinitionApi.list()
    byType.value = Object.fromEntries(list.value.map((d) => [d.type, d]))
  }

  function getByType(type: string): NodeDefinition | undefined {
    return byType.value[type]
  }

  const categories = () =>
    [...new Set(list.value.map((d) => d.category))]

  return { list, byType, fetchList, getByType, categories }
})
