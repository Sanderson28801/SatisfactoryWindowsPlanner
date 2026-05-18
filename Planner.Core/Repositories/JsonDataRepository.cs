using Planner.Core.Data.Entities;
using Planner.Core.Domain; // <-- Make sure to use your new Domain namespace
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
        // Notice we are now storing the DOMAIN models, not the Data Entities!
        private readonly Dictionary<string, Item> _items = [];
        private readonly Dictionary<string, Recipe> _recipes = [];

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

            if (parsedData?.Items is null || parsedData.Recipes is null || parsedData.Buildings is null)
            {
                throw new InvalidOperationException("Failed to parse the Satisfactory JSON data or missing required arrays.");
            }

            // 1. Map Items
            foreach (var kvp in parsedData.Items)
            {
                _items[kvp.Key] = new Item
                {
                    Id = kvp.Value.ClassName,
                    Name = kvp.Value.Name,
                    IsLiquid = kvp.Value.Liquid
                };
            }

            // 2. Map Recipes (And attach the Building Power right now!)
            foreach (var kvp in parsedData.Recipes)
            {
                var recipeData = kvp.Value;

                // Find building power
                string? machineId = recipeData.ProducedIn.FirstOrDefault();
                decimal power = 0m;
                if (machineId != null && parsedData.Buildings.TryGetValue(machineId, out var building))
                {
                    power = building.Metadata.PowerConsumption;
                }

                _recipes[kvp.Key] = new Recipe
                {
                    Id = recipeData.ClassName,
                    Name = recipeData.Name,
                    IsAlternate = recipeData.Alternate,
                    CraftingTimeSeconds = recipeData.Time,
                    MachineClassName = machineId,
                    BasePowerDraw = power, // Power is now permanently attached!

                    // Map the components
                    Ingredients = recipeData.Ingredients.Select(i => new RecipeQuantity
                    {
                        ItemId = i.ItemClassName,
                        Amount = i.Amount
                    }).ToList(),

                    Products = recipeData.Products.Select(p => new RecipeQuantity
                    {
                        ItemId = p.ItemClassName,
                        Amount = p.Amount
                    }).ToList()
                };
            }
        }

        public Item? GetItem(string itemClassName)
        {
            return _items.TryGetValue(itemClassName, out var item) ? item : null;
        }

        public Recipe? GetRecipe(string recipeClassName)
        {
            return _recipes.TryGetValue(recipeClassName, out var recipe) ? recipe : null;
        }

        public IEnumerable<Recipe> GetRecipesProducing(string itemClassName)
        {
            var rawMaterials = new HashSet<string>
            {
                "Desc_Water_C", "Desc_OreIron_C", "Desc_OreCopper_C",
                "Desc_OreBauxite_C", "Desc_Coal_C", "Desc_OreGold_C",
                "Desc_Stone_C", "Desc_LiquidOil_C", "Desc_RawQuartz_C",
                "Desc_OreUranium_C", "Desc_NitrogenGas_C", "Desc_SAM_C", "Desc_Sulfur_C"
            };

            if (rawMaterials.Contains(itemClassName))
            {
                return []; // Force the engine to treat this as a dead-end
            }

            // Return the cleanly mapped Domain recipes
            return _recipes.Values.Where(r => r.Products.Any(p => p.ItemId == itemClassName));
        }

        public IEnumerable<Item> GetAllItems()
        {
            return _items.Values;
        }
    }
}