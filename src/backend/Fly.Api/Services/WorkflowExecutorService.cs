using Fly.Api.Models;

namespace Fly.Api.Services;

public class WorkflowExecutorService
{
    private readonly Dictionary<string, WorkflowExecution> _executions = [];
    private readonly WorkflowService _workflowService;

    // Simulated execution times per node type (ms)
    private static readonly Dictionary<string, int> NodeDurations = new()
    {
        ["start"]          = 100,
        ["end"]            = 100,
        ["pipetting"]      = 2000,
        ["incubation"]     = 3000,
        ["centrifugation"] = 2500,
        ["mixing"]         = 1500,
        ["washing"]        = 2000,
        ["detection"]      = 2500,
        ["decision"]       = 500,
        ["wait"]           = 1000,
    };

    public WorkflowExecutorService(WorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public IEnumerable<WorkflowExecution> GetAll() =>
        _executions.Values.OrderByDescending(e => e.StartedAt);

    public WorkflowExecution? GetById(string id) =>
        _executions.TryGetValue(id, out var e) ? e : null;

    public IEnumerable<WorkflowExecution> GetByWorkflow(string workflowId) =>
        _executions.Values.Where(e => e.WorkflowId == workflowId).OrderByDescending(e => e.StartedAt);

    public WorkflowExecution? Start(string workflowId)
    {
        var workflow = _workflowService.GetById(workflowId);
        if (workflow is null) return null;

        var execution = new WorkflowExecution
        {
            WorkflowId  = workflowId,
            WorkflowName = workflow.Name,
            Status      = ExecutionStatus.Running,
            StartedAt   = DateTime.UtcNow
        };
        _executions[execution.Id] = execution;

        // Run simulation in background
        _ = SimulateExecutionAsync(execution, workflow);
        return execution;
    }

    public bool Cancel(string executionId)
    {
        if (!_executions.TryGetValue(executionId, out var execution)) return false;
        if (execution.Status is not (ExecutionStatus.Running or ExecutionStatus.Paused)) return false;
        execution.Status = ExecutionStatus.Cancelled;
        execution.FinishedAt = DateTime.UtcNow;
        return true;
    }

    private async Task SimulateExecutionAsync(WorkflowExecution execution, Workflow workflow)
    {
        var nodeMap = workflow.Nodes.ToDictionary(n => n.Id);
        var edgeMap = workflow.Edges.GroupBy(e => e.Source)
                                    .ToDictionary(g => g.Key, g => g.ToList());

        // Topological walk: start from the node with no incoming edges
        var incomingCount = workflow.Nodes.ToDictionary(n => n.Id, _ => 0);
        foreach (var edge in workflow.Edges)
            if (incomingCount.ContainsKey(edge.Target)) incomingCount[edge.Target]++;

        var queue = new Queue<string>(workflow.Nodes
            .Where(n => incomingCount[n.Id] == 0)
            .Select(n => n.Id));

        while (queue.Count > 0)
        {
            if (execution.Status == ExecutionStatus.Cancelled) return;

            var nodeId = queue.Dequeue();
            if (!nodeMap.TryGetValue(nodeId, out var node)) continue;

            execution.CurrentNodeId = nodeId;
            var startMs = DateTime.UtcNow;
            var log = new NodeExecutionLog
            {
                NodeId    = nodeId,
                NodeLabel = node.Label,
                NodeType  = node.Type,
                Status    = ExecutionStatus.Running,
                Message   = BuildStartMessage(node),
                Timestamp = DateTime.UtcNow
            };
            execution.Logs.Add(log);

            var durationMs = NodeDurations.TryGetValue(node.Type, out var d) ? d : 1000;
            await Task.Delay(durationMs);

            if (execution.Status == ExecutionStatus.Cancelled) return;

            log.Status     = ExecutionStatus.Completed;
            log.Message    = BuildCompleteMessage(node);
            log.DurationMs = (DateTime.UtcNow - startMs).TotalMilliseconds;

            // Enqueue next nodes
            if (edgeMap.TryGetValue(nodeId, out var outEdges))
                foreach (var edge in outEdges)
                    queue.Enqueue(edge.Target);
        }

        if (execution.Status != ExecutionStatus.Cancelled)
        {
            execution.Status      = ExecutionStatus.Completed;
            execution.FinishedAt  = DateTime.UtcNow;
            execution.CurrentNodeId = null;
        }
    }

    private static string BuildStartMessage(WorkflowNode node) => node.Type switch
    {
        "pipetting"      => $"开始移液：{node.Parameters.GetValueOrDefault("volume_ul", "?")} µL，{node.Parameters.GetValueOrDefault("source", "?")} → {node.Parameters.GetValueOrDefault("target", "?")}",
        "incubation"     => $"开始孵育：{node.Parameters.GetValueOrDefault("temperature_c", "?")} °C，{node.Parameters.GetValueOrDefault("duration_min", "?")} 分钟",
        "centrifugation" => $"开始离心：{node.Parameters.GetValueOrDefault("speed_rpm", "?")} rpm，{node.Parameters.GetValueOrDefault("duration_min", "?")} 分钟",
        "mixing"         => $"开始混合：{node.Parameters.GetValueOrDefault("mode", "?")}，{node.Parameters.GetValueOrDefault("duration_sec", "?")} 秒",
        "washing"        => $"开始洗涤：{node.Parameters.GetValueOrDefault("cycles", "?")} 次，每次 {node.Parameters.GetValueOrDefault("volume_ul", "?")} µL",
        "detection"      => $"开始检测：{node.Parameters.GetValueOrDefault("mode", "?")}，{node.Parameters.GetValueOrDefault("wavelength_nm", "?")} nm",
        "wait"           => $"等待：{node.Parameters.GetValueOrDefault("duration_sec", "?")} 秒",
        _                => $"执行节点：{node.Label}"
    };

    private static string BuildCompleteMessage(WorkflowNode node) => node.Type switch
    {
        "start"      => "流程开始",
        "end"        => "流程结束",
        "detection"  => $"检测完成，结果已记录",
        _            => $"{node.Label} 完成"
    };
}
