using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Domain;

// A simple pair used by the Domain Recipe
public class RecipeQuantity
{
    public required string ItemId { get; init; }
    public decimal Amount { get; init; }
}
