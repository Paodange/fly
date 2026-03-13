using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/steps")]
public class FlowStepsController(FlowStepService flowStepService) : ControllerBase
{
    // Built-in constraint rule definitions (code only, no database)
    private static readonly ConstraintRuleDefinition[] BuiltInConstraintRules =
    [
        new() { Type = "min",       Label = "最小值",     Description = "数值的最小允许值",         ApplicableTypes = ["number"], ValueType = "number" },
        new() { Type = "max",       Label = "最大值",     Description = "数值的最大允许值",         ApplicableTypes = ["number"], ValueType = "number" },
        new() { Type = "minLength", Label = "最小长度",   Description = "字符串的最小字符长度",     ApplicableTypes = ["string"], ValueType = "number" },
        new() { Type = "maxLength", Label = "最大长度",   Description = "字符串的最大字符长度",     ApplicableTypes = ["string"], ValueType = "number" },
        new() { Type = "pattern",   Label = "正则表达式", Description = "字符串必须匹配的正则表达式", ApplicableTypes = ["string"], ValueType = "string" },
    ];

    [HttpGet("constraint-rules")]
    public IActionResult GetConstraintRules() => Ok(BuiltInConstraintRules);

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await flowStepService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var step = await flowStepService.GetByIdAsync(id);
        return step is null ? NotFound() : Ok(step);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FlowStep step)
    {
        var created = await flowStepService.CreateAsync(step);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] FlowStep step)
    {
        var updated = await flowStepService.UpdateAsync(id, step);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await flowStepService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
