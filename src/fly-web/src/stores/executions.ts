import { defineStore } from 'pinia'
import { ref } from 'vue'
import { executionApi } from '@/api'
import type { WorkflowExecution } from '@/types'

export const useExecutionStore = defineStore('executions', () => {
  const list = ref<WorkflowExecution[]>([])
  const current = ref<WorkflowExecution | null>(null)
  const loading = ref(false)
  const pollingTimer = ref<ReturnType<typeof setInterval> | null>(null)

  async function fetchList() {
    list.value = await executionApi.list()
  }

  async function fetchOne(id: string) {
    current.value = await executionApi.get(id)
  }

  async function start(workflowId: string) {
    loading.value = true
    try {
      const exec = await executionApi.start(workflowId)
      list.value.unshift(exec)
      current.value = exec
      startPolling(exec.id)
      return exec
    } finally {
      loading.value = false
    }
  }

  async function cancel(id: string) {
    await executionApi.cancel(id)
    stopPolling()
    await fetchOne(id)
  }

  function startPolling(id: string) {
    stopPolling()
    pollingTimer.value = setInterval(async () => {
      const exec = await executionApi.get(id)
      current.value = exec
      const idx = list.value.findIndex((e) => e.id === id)
      if (idx >= 0) list.value[idx] = exec
      if (exec.status !== 'Running' && exec.status !== 'Paused') stopPolling()
    }, 800)
  }

  function stopPolling() {
    if (pollingTimer.value) {
      clearInterval(pollingTimer.value)
      pollingTimer.value = null
    }
  }

  return { list, current, loading, fetchList, fetchOne, start, cancel, startPolling, stopPolling }
})
