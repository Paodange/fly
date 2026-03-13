using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController(WorkflowService workflowService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(workflowService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var workflow = workflowService.GetById(id);
        return workflow is null ? NotFound() : Ok(workflow);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Workflow workflow)
    {
        var created = workflowService.Create(workflow);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Workflow workflow)
    {
        var updated = workflowService.Update(id, workflow);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var deleted = workflowService.Delete(id);
        return deleted ? NoContent() : NotFound();
    }
}
