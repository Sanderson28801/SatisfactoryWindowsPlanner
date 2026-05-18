namespace Planner.Core.Domain;

// A clean representation of an Item for math/routing
public class Item
{
    public required string Id { get; init; } // Maps to ClassName
    public required string Name { get; init; }
    public bool IsLiquid { get; init; }
}


