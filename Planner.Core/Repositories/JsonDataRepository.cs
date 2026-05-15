using Planner.Core.Interfaces;
using Planner.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Planner.Core.Repositories
{
    public class JsonDataRepository : IDataRepository
    {
        // We store the data in memory using Dictionaries for O(1) lookups
        private readonly Dictionary<string, ItemData> _items = [];
        private readonly Dictionary<string, RecipeData> _recipes = [];

        // The constructor requires the path to data.json
        public JsonDataRepository(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Could not find the data file at: {jsonFilePath}");
            }

            string jsonContent = File.ReadAllText(jsonFilePath);


            SatisfactoryData? parsedData = JsonSerializer.Deserialize(
                jsonContent,
                PlannerJsonContext.Default.SatisfactoryData
            );

            if (parsedData is null)
            {
                throw new InvalidOperationException("Failed to parse the Satisfactory JSON data.");
            }

            _items = parsedData.Items;


            if (parsedData.Recipes is not null)
            {
                _recipes = parsedData.Recipes;
            }
        }

        public ItemData? GetItem(string itemClassName)
        {
            return _items.TryGetValue(itemClassName, out var item) ? item : null;
        }

        public RecipeData? GetRecipe(string recipeClassName)
        {
            return _recipes.TryGetValue(recipeClassName, out var recipe) ? recipe : null;
        }

        public IEnumerable<RecipeData> GetRecipesProducing(string itemClassName)
        {
            // LINQ query to find all recipes where the products list contains our target item
            return _recipes.Values
                .Where(recipe => recipe.Products.Any(p => p.ItemClassName == itemClassName));
        }

        public IEnumerable<ItemData> GetAllItems()
        {
            return _items.Values;
        }
    }
}
