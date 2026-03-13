import { defineStore } from 'pinia'
import { ref } from 'vue'
import { workflowApi } from '@/api'
import type { Workflow } from '@/types'

export const useWorkflowStore = defineStore('workflows', () => {
  const list = ref<Workflow[]>([])
  const current = ref<Workflow | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchList() {
    loading.value = true
    error.value = null
    try {
      list.value = await workflowApi.list()
    } catch (e: unknown) {
      error.value = String(e)
    } finally {
      loading.value = false
    }
  }

  async function fetchOne(id: string) {
    loading.value = true
    error.value = null
    try {
      current.value = await workflowApi.get(id)
    } catch (e: unknown) {
      error.value = String(e)
    } finally {
      loading.value = false
    }
  }

  async function save(workflow: Workflow) {
    const updated = await workflowApi.update(workflow.id, workflow)
    current.value = updated
    const idx = list.value.findIndex((w) => w.id === updated.id)
    if (idx >= 0) list.value[idx] = updated
    else list.value.unshift(updated)
  }

  async function create(workflow: Partial<Workflow>) {
    const created = await workflowApi.create(workflow)
    list.value.unshift(created)
    current.value = created
    return created
  }

  async function remove(id: string) {
    await workflowApi.delete(id)
    list.value = list.value.filter((w) => w.id !== id)
    if (current.value?.id === id) current.value = null
  }

  return { list, current, loading, error, fetchList, fetchOne, save, create, remove }
})
