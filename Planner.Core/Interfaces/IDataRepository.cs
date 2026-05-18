using Planner.Core.Domain; // Use the domain namespace
using System.Collections.Generic;

namespace Planner.Core.Interfaces;

public interface IDataRepository
{
    Item? GetItem(string itemClassName);
    Recipe? GetRecipe(string recipeClassName);
    IEnumerable<Recipe> GetRecipesProducing(string itemClassName);
    IEnumerable<Item> GetAllItems();
}