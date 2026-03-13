namespace Fly.Api.Models;

public class NodeDefinition
{
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<NodeParameterDef> Parameters { get; set; } = [];
}

public class NodeParameterDef
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // string, number, boolean, select
    public object? DefaultValue { get; set; }
    public bool Required { get; set; }
    public string? Unit { get; set; }
    public List<string>? Options { get; set; }
    public string? Description { get; set; }
}
