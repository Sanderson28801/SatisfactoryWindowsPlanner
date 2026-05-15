using Planner.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Interfaces;

public interface IDataRepository
{
    // Retrieves a single item by its class name (e.g., "Desc_Cement_C")
    ItemData? GetItem(string itemClassName);

    // Retrieves a specific recipe by its class name
    RecipeData? GetRecipe(string recipeClassName);

    // Retrieves all recipes that produce a specific item
    IEnumerable<RecipeData> GetRecipesProducing(string itemClassName);

    // Useful for populating UI dropdown menus later
    IEnumerable<ItemData> GetAllItems();
}
