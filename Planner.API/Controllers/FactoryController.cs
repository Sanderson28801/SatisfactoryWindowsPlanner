using Microsoft.AspNetCore.Mvc;
using Planner.Api.Services;
using Planner.Core.Services;

namespace Planner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FactoryController : ControllerBase
{
    private readonly ProductionEngine _engine;

    // ASP.NET magically passes your engine into this constructor
    // because you registered it in Program.cs!
    public FactoryController(ProductionEngine engine)
    {
        _engine = engine;
    }

    // This listens for URLs like: http://localhost:5000/api/factory/calculate?item=Desc_Stator&amount=50
    [HttpGet("calculate")]
    public IActionResult Calculate(string item, decimal amount)
    {
        try
        {
            // 1. Call your pristine, untouched engine
            var rootNode = _engine.CalculateProductionTree(item, amount);

            // 2. (We will write the DTO translation here in the next step!)
            var safeGraphDto = GraphConverter.FlattenAndConsolidate(rootNode);

            // 3. Return a 200 OK status code, with the data inside
            return Ok(safeGraphDto);
        }
        catch (Exception ex)
        {
            // Return a 400 Bad Request status code if the math fails
            return BadRequest(ex.Message);
        }
    }
}