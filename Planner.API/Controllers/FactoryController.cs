using Microsoft.AspNetCore.Mvc;
using Planner.Api.Models;
using Planner.Api.Services;
using Planner.Core.Interfaces; // Use the interface!
using Planner.Core.Models;

namespace Planner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FactoryController : ControllerBase
{
    private readonly IProductionEngine _engine;
    private readonly IDataRepository _dataRepository;

    // Injecting the INTERFACE, not the concrete class
    public FactoryController(IProductionEngine engine, IDataRepository dataRepository)
    {
        _engine = engine;
        _dataRepository = dataRepository;
    }

    // Changed to HttpPost so we can accept a complex JSON body
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] CalculateFactoryRequest request)
    {
        // 1. Map the request weights to our Domain Profile
        var profile = new HeuristicProfile
        {
            ByproductPenaltyWeight = request.ByproductPenaltyWeight,
            ScavengeRewardWeight = request.ScavengeRewardWeight
        };

        // 2. Call your pristine engine with the new signature
        var result = _engine.CalculateProductionTree(
            request.TargetItemId,
            request.TargetAmountPerMinute,
            request.UnlockedAlternates,
            profile);

        // 3. Handle the Sad Path gracefully (No try/catch needed!)
        if (!result.IsSuccess)
        {
            // Returns a clean 400 Bad Request if the item isn't found
            return BadRequest(new { error = result.ErrorMessage });
        }

        // 4. Flatten the tree for React Flow (Using the unwrapped Value!)
        var safeGraphDto = GraphConverter.FlattenAndConsolidate(result.Value!);

        // 5. Return a 200 OK status code, with the graph data inside
        return Ok(safeGraphDto);
    }
    [HttpGet("items")]
    public IActionResult GetItems()
    {
        // Assuming GetAvailableItems() returns a list of items with Id and Name
        var items = _dataRepository.GetAllItems()
            .Select(i => new { id = i.Id, name = i.Name })
            .OrderBy(i => i.name);

        return Ok(items);
    }

    [HttpGet("alternates")]
    public IActionResult GetAlternateRecipes()
    {
        // Fetch only recipes flagged as alternates
        var alternates = _dataRepository.GetAllRecipes()
            .Where(r => r.IsAlternate)
            .Select(r => new { id = r.Id, name = r.Name })
            .OrderBy(r => r.name);

        return Ok(alternates);
    }
}

