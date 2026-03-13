using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(WorkflowService workflowService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await workflowService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var workflow = await workflowService.GetByIdAsync(id);
        return workflow is null ? NotFound() : Ok(workflow);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workflow workflow)
    {
        var created = await workflowService.CreateAsync(workflow);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Workflow workflow)
    {
        var updated = await workflowService.UpdateAsync(id, workflow);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await workflowService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // Flow step management endpoints
    [HttpGet("{workflowId}/steps")]
    public async Task<IActionResult> GetSteps(string workflowId)
    {
        var workflow = await workflowService.GetByIdAsync(workflowId);
        if (workflow is null) return NotFound();

        var steps = await workflowService.GetStepsAsync(workflowId);
        return Ok(steps);
    }

    [HttpGet("{workflowId}/steps/{stepId}")]
    public async Task<IActionResult> GetStep(string workflowId, string stepId)
    {
        var step = await workflowService.GetStepAsync(workflowId, stepId);
        return step is null ? NotFound() : Ok(step);
    }

    [HttpPost("{workflowId}/steps")]
    public async Task<IActionResult> CreateStep(string workflowId, [FromBody] WorkflowNode node)
    {
        var created = await workflowService.CreateStepAsync(workflowId, node);
        if (created is null) return NotFound();

        return CreatedAtAction(nameof(GetStep), new { workflowId, stepId = created.Id }, created);
    }

    [HttpPut("{workflowId}/steps/{stepId}")]
    public async Task<IActionResult> UpdateStep(string workflowId, string stepId, [FromBody] WorkflowNode node)
    {
        var updated = await workflowService.UpdateStepAsync(workflowId, stepId, node);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{workflowId}/steps/{stepId}")]
    public async Task<IActionResult> DeleteStep(string workflowId, string stepId)
    {
        var deleted = await workflowService.DeleteStepAsync(workflowId, stepId);
        return deleted ? NoContent() : NotFound();
    }
}
