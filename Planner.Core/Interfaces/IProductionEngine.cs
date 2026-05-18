using Planner.Core.Domain;
using Planner.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Interfaces;

public interface IProductionEngine
{
    // Returns the Root Node. By holding the root, you hold the whole tree.
    Result<IngredientNode> CalculateProductionTree(
        string targetItemClassName,
        decimal targetAmountPerMinute,
        List<string> unlockedAlternates,
        HeuristicProfile profile,
        FactoryState? state = null,
        HashSet<string>? currentPath = null);
}
