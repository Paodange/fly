import type { Workflow, WorkflowExecution, NodeDefinition, WorkflowNode } from '@/types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(`[${res.status}] ${text}`)
  }
  if (res.status === 204) return undefined as T
  return res.json()
}

// ─── Workflows ───────────────────────────────────────────────────────────────

export const workflowApi = {
  list: () => request<Workflow[]>('/api/workflows'),
  get: (id: string) => request<Workflow>(`/api/workflows/${id}`),
  create: (w: Partial<Workflow>) =>
    request<Workflow>('/api/workflows', { method: 'POST', body: JSON.stringify(w) }),
  update: (id: string, w: Workflow) =>
    request<Workflow>(`/api/workflows/${id}`, { method: 'PUT', body: JSON.stringify(w) }),
  delete: (id: string) => request<void>(`/api/workflows/${id}`, { method: 'DELETE' }),
}

// ─── Executions ──────────────────────────────────────────────────────────────

export const executionApi = {
  list: () => request<WorkflowExecution[]>('/api/executions'),
  get: (id: string) => request<WorkflowExecution>(`/api/executions/${id}`),
  listByWorkflow: (workflowId: string) =>
    request<WorkflowExecution[]>(`/api/executions/workflow/${workflowId}`),
  start: (workflowId: string) =>
    request<WorkflowExecution>(`/api/executions/start/${workflowId}`, { method: 'POST' }),
  cancel: (executionId: string) =>
    request<void>(`/api/executions/${executionId}/cancel`, { method: 'POST' }),
}

// ─── Node definitions ─────────────────────────────────────────────────────────

export const nodeDefinitionApi = {
  list: () => request<NodeDefinition[]>('/api/nodedefinitions'),
}

// ─── Flow Steps ───────────────────────────────────────────────────────────────

export const flowStepApi = {
  list: (workflowId: string) => request<WorkflowNode[]>(`/api/workflows/${workflowId}/steps`),
  get: (workflowId: string, stepId: string) =>
    request<WorkflowNode>(`/api/workflows/${workflowId}/steps/${stepId}`),
  create: (workflowId: string, node: Partial<WorkflowNode>) =>
    request<WorkflowNode>(`/api/workflows/${workflowId}/steps`, {
      method: 'POST',
      body: JSON.stringify(node)
    }),
  update: (workflowId: string, stepId: string, node: WorkflowNode) =>
    request<WorkflowNode>(`/api/workflows/${workflowId}/steps/${stepId}`, {
      method: 'PUT',
      body: JSON.stringify(node)
    }),
  delete: (workflowId: string, stepId: string) =>
    request<void>(`/api/workflows/${workflowId}/steps/${stepId}`, { method: 'DELETE' }),
}
