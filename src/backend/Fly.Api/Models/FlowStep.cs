namespace Fly.Api.Models;

public class FlowStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public NodePosition Position { get; set; } = new();
    public List<FlowStepParameter> Parameters { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A structured parameter definition belonging to a FlowStep.
/// </summary>
public class FlowStepParameter
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    /// <summary>string | number | boolean | select</summary>
    public string Type { get; set; } = "string";
    public string? DefaultValue { get; set; }
    public bool Required { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    /// <summary>Allowed values when Type is "select".</summary>
    public List<string> Options { get; set; } = [];
    public List<ParameterConstraint> Constraints { get; set; } = [];
}

/// <summary>
/// A single validation constraint applied to a FlowStepParameter.
/// </summary>
public class ParameterConstraint
{
    /// <summary>min | max | minLength | maxLength | pattern</summary>
    public string RuleType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Message { get; set; }
}
