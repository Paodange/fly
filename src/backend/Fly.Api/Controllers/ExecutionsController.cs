using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionsController(WorkflowExecutorService executorService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(executorService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var execution = executorService.GetById(id);
        return execution is null ? NotFound() : Ok(execution);
    }

    [HttpGet("workflow/{workflowId}")]
    public IActionResult GetByWorkflow(string workflowId) =>
        Ok(executorService.GetByWorkflow(workflowId));

    [HttpPost("start/{workflowId}")]
    public IActionResult Start(string workflowId)
    {
        var execution = executorService.Start(workflowId);
        return execution is null
            ? NotFound(new { message = $"Workflow '{workflowId}' not found." })
            : Ok(execution);
    }

    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(string id)
    {
        var cancelled = executorService.Cancel(id);
        return cancelled ? Ok() : NotFound();
    }
}
