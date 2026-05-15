using Planner.Core.Interfaces;
using Planner.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Domain;

public class IngredientNode : IFactoryNode
{
    public string DisplayName => Item.Name;
    public decimal TargetItemsPerMinute { get; set; }

    // We store the actual ItemData so the UI has access to descriptions, liquid status, etc.
    public required ItemData Item { get; set; }

    // THE EDGE: How do we get this item? 
    // If this is null, it means it's a raw resource (like Iron Ore) that you must mine.
    public ProductionNode? RecipeUsed { get; set; } // Only accounting for 1 recipe
}
