using Planner.Core.Interfaces;
using Planner.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Planner.Core.Tests
{
    // A fake repository solely for testing the Engine's math and traversal
    public class FakeDataRepository : IDataRepository
    {
        private readonly List<ItemData> _items = [];
        private readonly List<RecipeData> _recipes = [];
        private readonly List<BuildingData> _buildings = [];

        public FakeDataRepository()
        {
            // 1. Raw Resource (No recipe produces this)


            // 3. Complex Ratio Item (Produces multiple, requires multiple)
            _items.Add(new ItemData { ClassName = "Desc_Wire", Name = "Wire", Slug = "wire" });
            _recipes.Add(new RecipeData
            {
                ClassName = "Recipe_Wire",
                Name = "Craft Wire",
                Time = 30m, // 2 cycles per minute
                Products = [new RecipeComponent { ItemClassName = "Desc_Wire", Amount = 3m }],    // 6 per min base
                Ingredients = [new RecipeComponent { ItemClassName = "Desc_Ingot", Amount = 1m }] // 2 per min base
            });

            // 4. Multi-Ingredient Item
            _items.Add(new ItemData { ClassName = "Desc_Stator", Name = "Stator", Slug = "stator" });
            _recipes.Add(new RecipeData
            {
                ClassName = "Recipe_Stator",
                Name = "Assemble Stator",
                Time = 12m, // 5 cycles per minute
                Products = [new RecipeComponent { ItemClassName = "Desc_Stator", Amount = 1m }],  // 5 per min base
                Ingredients = [
                    new RecipeComponent { ItemClassName = "Desc_Wire", Amount = 8m },  // 40 per min base
                    new RecipeComponent { ItemClassName = "Desc_Ingot", Amount = 3m }  // 15 per min base
                ]
            });
            _buildings.Add(new BuildingData
            {
                ClassName = "Build_Smelter",
                Name = "Smelter",
                Slug = "smelter",
                Metadata = new BuildingMetadata
                {
                    PowerConsumption = 4m,
                    ManufacturingSpeed = 1m,
                    PowerConsumptionExponent = 1.6m
                }
            });
            _items.Add(new ItemData { ClassName = "Desc_Ore", Name = "Iron Ore", Slug = "ore" });

            _items.Add(new ItemData { ClassName = "Desc_Ingot", Name = "Iron Ingot", Slug = "ingot" });
            _recipes.Add(new RecipeData
            {
                ClassName = "Recipe_Ingot",
                Name = "Smelt Ingot",
                Time = 60m, // 1 cycle per min
                Products = [new RecipeComponent { ItemClassName = "Desc_Ingot", Amount = 1m }],
                Ingredients = [new RecipeComponent { ItemClassName = "Desc_Ore", Amount = 1m }],
                ProducedIn = ["Build_Smelter"] // Links to the Smelter building class!
            });
        }

        public ItemData? GetItem(string itemClassName) => _items.FirstOrDefault(i => i.ClassName == itemClassName);
        public RecipeData? GetRecipe(string recipeClassName) => _recipes.FirstOrDefault(r => r.ClassName == recipeClassName);

        public BuildingData? GetBuilding(string buildingClassName) => _buildings.FirstOrDefault(b => b.ClassName == buildingClassName);
        public IEnumerable<RecipeData> GetRecipesProducing(string itemClassName) => _recipes.Where(r => r.Products.Any(p => p.ItemClassName == itemClassName));
        public IEnumerable<ItemData> GetAllItems() => _items;
    }
}