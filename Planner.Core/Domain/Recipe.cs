using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Domain;

// A clean representation of a Recipe for math/routing
public class Recipe
{
    public required string Id { get; init; } // Maps to ClassName
    public required string Name { get; init; }
    public bool IsAlternate { get; init; }
    public decimal CraftingTimeSeconds { get; init; }

    // Notice how these are now highly specific to the math engine
    public List<RecipeQuantity> Ingredients { get; init; } = [];
    public List<RecipeQuantity> Products { get; init; } = [];

    public string? MachineClassName { get; init; }
    public decimal BasePowerDraw { get; init; }
}
