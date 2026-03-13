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
}
