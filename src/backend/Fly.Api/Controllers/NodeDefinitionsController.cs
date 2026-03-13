using Fly.Api.Models;
using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodeDefinitionsController(FlowStepService flowStepService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Get built-in flow control steps
        var builtInSteps = NodeCatalogService.GetAll();

        // Get dynamic steps from database and convert to NodeDefinition format
        var flowSteps = await flowStepService.GetAllAsync();
        var dynamicSteps = flowSteps.Select(fs => new NodeDefinition
        {
            Type = fs.Type,
            Category = fs.Category,
            Label = fs.Label,
            Description = fs.Description,
            Icon = fs.Icon,
            Color = fs.Color,
            Parameters = fs.Parameters.Select(p => new NodeParameterDef
            {
                Key = p.Key,
                Label = p.Label,
                Type = p.Type,
                DefaultValue = p.DefaultValue,
                Required = p.Required,
                Unit = p.Unit,
                Options = p.Options,
                Description = p.Description
            }).ToList()
        });

        // Merge and return all steps
        var allSteps = builtInSteps.Concat(dynamicSteps).ToList();
        return Ok(allSteps);
    }
}
