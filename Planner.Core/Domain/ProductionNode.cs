using Planner.Core.Interfaces;
using Planner.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Domain;

public class ProductionNode : IFactoryNode
{
    public string DisplayName => Recipe.Name;
    public decimal TargetItemsPerMinute { get; set; }

    // We store the actual RecipeData so the engine can look at its base time/amounts
    public required RecipeData Recipe { get; set; }

    // Core Math Outputs
    public decimal MachinesRequired { get; set; }
    public decimal PowerRequired { get; set; }

    // THE EDGES: What does this machine need to consume to keep running?
    public List<IngredientNode> Dependencies { get; set; } = [];
}
