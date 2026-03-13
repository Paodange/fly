namespace Fly.Api.Models;

/// <summary>
/// Describes a built-in parameter constraint rule.
/// Rules are defined in code only – no database storage required.
/// </summary>
public class ConstraintRuleDefinition
{
    /// <summary>Rule identifier: min | max | minLength | maxLength | pattern</summary>
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Which FlowStepParameter types this rule may be applied to.</summary>
    public string[] ApplicableTypes { get; set; } = [];
    /// <summary>Expected value type for the constraint value: "number" or "string".</summary>
    public string ValueType { get; set; } = "string";
}
