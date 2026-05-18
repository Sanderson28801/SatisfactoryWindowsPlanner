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
        private readonly Dictionary<string, BuildingData> _buildings = [];

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
            if (parsedData.Items is null || parsedData.Recipes is null || parsedData.Buildings is null)
            {
                throw new InvalidOperationException("Parsed data does not contain any items, recipes, or buildings.");
            }

            _items = parsedData.Items;
            _recipes = parsedData.Recipes;
            _buildings = parsedData.Buildings;

        }

        public ItemData? GetItem(string itemClassName)
        {
            return _items.TryGetValue(itemClassName, out var item) ? item : null;
        }

        public RecipeData? GetRecipe(string recipeClassName)
        {
            return _recipes.TryGetValue(recipeClassName, out var recipe) ? recipe : null;
        }

        public BuildingData? GetBuilding(string buildingClassName)
        {
            return _buildings.TryGetValue(buildingClassName, out var building) ? building : null;
        }

        public IEnumerable<RecipeData> GetRecipesProducing(string itemClassName)
        {
            // LINQ query to find all recipes where the products list contains our target item
            return _recipes.Values
                .Where(recipe => recipe.Products.Any(p => p.ItemClassName == itemClassName));
        }
        public RecipeData? GetRecipeProducing(string itemClassName)
        {
            // 1. HARDCODE RAW MATERIALS
            // If the engine asks how to "craft" these, tell it you can't. 
            // They are mined/extracted directly from the world!
            var rawMaterials = new HashSet<string>
    {
        "Desc_Water_C",
        "Desc_OreIron_C",
        "Desc_OreCopper_C",
        "Desc_OreBauxite_C",
        "Desc_Coal_C",
        "Desc_OreGold_C", // Caterium
        "Desc_Stone_C",   // Limestone
        "Desc_LiquidOil_C",
        "Desc_Sulfur_C",
        "Desc_RawQuartz_C",
        "Desc_OreUranium_C",
        "Desc_NitrogenGas_C"
    };

            if (rawMaterials.Contains(itemClassName))
            {
                return null; // Force the engine to treat this as a dead-end Ingredient Node
            }

            // 2. FILTER OUT ALTERNATES
            // Search your loaded JSON data for a recipe that makes this item
            RecipeData? recipe = _recipes.Values
                .Where(r =>
                    // Must produce the item we want
                    r.Products.Any(p => p.ItemClassName == itemClassName) &&

                    // MUST NOT be an alternate recipe (assuming your JSON has a flag like IsAlternate)
                    // If your JSON doesn't have this flag, you can check if the recipe name starts with "Recipe_Alternate"
                    r.Alternate == false
                )
                .FirstOrDefault();

            return recipe;
        }
        public IEnumerable<ItemData> GetAllItems()
        {
            return _items.Values;
        }
    }
}
