using Fly.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fly.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodeDefinitionsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(NodeCatalogService.GetAll());
}
