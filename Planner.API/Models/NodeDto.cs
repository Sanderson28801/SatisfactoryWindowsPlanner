namespace Planner.Api.Models;

public class NodeDto
{
    public required string Id { get; set; }
    public required string Label { get; set; }

    // "Item" or "Recipe" so the frontend knows what color/shape to draw
    public required string Type { get; set; }

    // For Items
    public decimal Amount { get; set; }

    // For Recipes
    public decimal Machines { get; set; }
    public decimal Power { get; set; }
}
