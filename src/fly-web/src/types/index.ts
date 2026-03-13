// Types mirroring backend models

export interface NodePosition {
  x: number
  y: number
}

export interface WorkflowNode {
  id: string
  type: string
  label: string
  position: NodePosition
  parameters: Record<string, unknown>
  parentNode?: string // ID of parent node for nested relationships
}

export interface WorkflowEdge {
  id: string
  source: string
  target: string
  label?: string
  condition?: string
}

export interface Workflow {
  id: string
  name: string
  description: string
  nodes: WorkflowNode[]
  edges: WorkflowEdge[]
  createdAt: string
  updatedAt: string
}

export type ExecutionStatus =
  | 'Pending'
  | 'Running'
  | 'Paused'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'

export interface NodeExecutionLog {
  nodeId: string
  nodeLabel: string
  nodeType: string
  status: ExecutionStatus
  message?: string
  timestamp: string
  durationMs?: number
}

export interface WorkflowExecution {
  id: string
  workflowId: string
  workflowName: string
  status: ExecutionStatus
  currentNodeId?: string
  logs: NodeExecutionLog[]
  startedAt: string
  finishedAt?: string
  errorMessage?: string
}

export interface NodeParameterDef {
  key: string
  label: string
  type: string   // 'string' | 'number' | 'boolean' | 'select'
  defaultValue?: unknown
  required?: boolean
  unit?: string
  options?: string[]
  description?: string
}

export interface NodeDefinition {
  type: string
  category: string
  label: string
  description: string
  icon: string
  color: string
  parameters: NodeParameterDef[]
}

// ─── Flow Step Parameter / Constraint types ───────────────────────────────────

export interface ParameterConstraint {
  ruleType: string
  value: string
  message?: string
}

export interface FlowStepParameter {
  key: string
  label: string
  type: 'string' | 'number' | 'boolean' | 'select'
  defaultValue?: string
  required: boolean
  unit?: string
  description?: string
  options: string[]
  constraints: ParameterConstraint[]
}

export interface ConstraintRuleDefinition {
  type: string
  label: string
  description: string
  applicableTypes: string[]
  valueType: 'number' | 'string'
}

export interface FlowStep {
  id: string
  type: string
  category: string
  label: string
  description: string
  icon: string
  color: string
  position: NodePosition
  parameters: FlowStepParameter[]
  createdAt: string
  updatedAt: string
}
