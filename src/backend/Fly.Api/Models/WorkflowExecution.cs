namespace Fly.Api.Models;

public enum ExecutionStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public class WorkflowExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public string? CurrentNodeId { get; set; }
    public List<NodeExecutionLog> Logs { get; set; } = [];
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NodeExecutionLog
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeLabel { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? DurationMs { get; set; }
}
