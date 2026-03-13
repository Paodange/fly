import { createRouter, createWebHistory } from 'vue-router'
import WorkflowListView from '../views/WorkflowListView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'workflow-list',
      component: WorkflowListView,
    },
    {
      path: '/editor/:id',
      name: 'workflow-editor',
      component: () => import('../views/WorkflowEditorView.vue'),
    },
    {
      path: '/executions',
      name: 'executions',
      component: () => import('../views/ExecutionsView.vue'),
    },
    {
      path: '/executions/:id',
      name: 'execution-detail',
      component: () => import('../views/ExecutionDetailView.vue'),
    },
    {
      path: '/flow-steps',
      name: 'flow-steps',
      component: () => import('../views/FlowStepManagementView.vue'),
    },
  ],
})

export default router
