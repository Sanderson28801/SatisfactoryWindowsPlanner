using Planner.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Interfaces;

public interface IProductionEngine
{
    // Returns the Root Node. By holding the root, you hold the whole tree.
    IngredientNode CalculateProductionTree(string targetItemClassName, decimal targetAmountPerMinute);
}
