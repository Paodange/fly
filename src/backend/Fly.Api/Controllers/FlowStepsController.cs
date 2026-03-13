using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/steps")]
public class FlowStepsController(FlowStepService flowStepService) : ControllerBase
{
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
