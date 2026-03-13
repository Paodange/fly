namespace Fly.Api.Models;

public class FlowStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public NodePosition Position { get; set; } = new();
    public Dictionary<string, object?> Parameters { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
