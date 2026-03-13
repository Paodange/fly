namespace Fly.Api.Models;

public class Workflow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowNode> Nodes { get; set; } = [];
    public List<WorkflowEdge> Edges { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkflowNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public NodePosition Position { get; set; } = new();
    public Dictionary<string, object?> Parameters { get; set; } = [];
    public string? ParentNode { get; set; } // ID of parent node for nested relationships
}

public class WorkflowEdge
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Condition { get; set; }
}

public class NodePosition
{
    public double X { get; set; }
    public double Y { get; set; }
}
