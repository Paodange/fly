# fly — 实验室自动化流程系统

**FLY** 是一套面向实验室自动化领域的 **可视化流程编辑与执行平台**，采用前后端分离的 B/S 架构。

## 技术栈

| 层次 | 技术 |
|------|------|
| 后端 | .NET 10 · ASP.NET Core Web API |
| 前端 | Vue 3 · TypeScript · Element Plus · Vue Flow |

## 功能

- **流程列表** — 管理所有自动化流程（新建 / 编辑 / 运行 / 删除）
- **可视化编辑器** — 拖拽式节点图编辑器，支持连线与参数配置
- **节点类型** — 涵盖常见实验室操作：移液、孵育、离心、混合、洗涤、检测等
- **流程运行** — 一键启动流程，实时跟踪各节点执行状态与日志

## 本地启动

### 后端

```bash
cd src/backend/Fly.Api
dotnet run
# API 默认监听 http://localhost:5000
```

### 前端

```bash
cd src/fly-web
cp .env.example .env   # 根据需要修改 API 地址
npm install
npm run dev
# 默认访问 http://localhost:5173
```

## 项目结构

```
fly/
├── src/
│   ├── backend/
│   │   └── Fly.Api/              .NET 10 Web API
│   │       ├── Controllers/      WorkflowsController / ExecutionsController / NodeDefinitionsController
│   │       ├── Models/           Workflow / WorkflowExecution / NodeDefinition
│   │       └── Services/         WorkflowService / WorkflowExecutorService / NodeCatalogService
│   └── fly-web/                  Vue 3 前端
│       └── src/
│           ├── api/              封装 fetch 调用后端 REST API
│           ├── stores/           Pinia 状态管理
│           ├── types/            TypeScript 类型定义
│           ├── components/       LabFlowNode（Vue Flow 自定义节点）
│           └── views/            WorkflowListView / WorkflowEditorView / ExecutionsView / ExecutionDetailView
└── README.md
```

