<script setup lang="ts">
import { RouterView, useRouter } from 'vue-router'
import { ref, computed } from 'vue'

const router = useRouter()
const collapsed = ref(false)

const menuItems = [
  { index: '/', icon: 'Grid', label: '工作流列表' },
  { index: '/flow-steps', icon: 'Setting', label: '流程步骤管理' },
  { index: '/executions', icon: 'VideoPlay', label: '执行记录' },
]

const activeIndex = computed(() => router.currentRoute.value.path)
</script>

<template>
  <el-container class="app-root">
    <el-aside :width="collapsed ? '64px' : '200px'" class="sidebar">
      <div class="logo-wrap" @click="router.push('/')">
        <el-icon size="28" color="#409EFF"><Grid /></el-icon>
        <span v-if="!collapsed" class="logo-text">FLY</span>
      </div>
      <el-menu
        :default-active="activeIndex"
        :collapse="collapsed"
        router
        background-color="#1a1a2e"
        text-color="#ccc"
        active-text-color="#409EFF"
      >
        <el-menu-item v-for="item in menuItems" :key="item.index" :index="item.index">
          <el-icon><component :is="item.icon" /></el-icon>
          <template #title>{{ item.label }}</template>
        </el-menu-item>
      </el-menu>
      <div class="collapse-btn" @click="collapsed = !collapsed">
        <el-icon><component :is="collapsed ? 'Expand' : 'Fold'" /></el-icon>
      </div>
    </el-aside>

    <el-container>
      <el-header class="app-header">
        <span class="header-title">实验室自动化流程系统</span>
      </el-header>
      <el-main class="app-main">
        <RouterView />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app-root {
  height: 100vh;
  width: 100vw;
  overflow: hidden;
}

.sidebar {
  background-color: #1a1a2e;
  transition: width 0.25s;
  display: flex;
  flex-direction: column;
}

.logo-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 20px;
  cursor: pointer;
}

.logo-text {
  color: #409EFF;
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 2px;
}

.el-menu {
  border-right: none;
  flex: 1;
}

.collapse-btn {
  padding: 12px 20px;
  color: #ccc;
  cursor: pointer;
  display: flex;
  align-items: center;
}

.collapse-btn:hover {
  color: #409EFF;
}

.app-header {
  background: #fff;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  align-items: center;
  box-shadow: 0 1px 4px rgba(0,0,0,0.08);
}

.header-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.app-main {
  background: #f0f2f5;
  overflow-y: auto;
  padding: 16px;
}
</style>
