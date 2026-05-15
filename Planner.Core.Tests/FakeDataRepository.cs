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

        public FakeDataRepository()
        {
            // 1. Raw Resource (No recipe produces this)
            _items.Add(new ItemData { ClassName = "Desc_Ore", Name = "Iron Ore", Slug = "ore" });

            // 2. Simple 1-to-1 Item
            _items.Add(new ItemData { ClassName = "Desc_Ingot", Name = "Iron Ingot", Slug = "ingot" });
            _recipes.Add(new RecipeData
            {
                ClassName = "Recipe_Ingot",
                Name = "Smelt Ingot",
                Time = 60m, // 1 cycle per minute
                Products = [new RecipeComponent { ItemClassName = "Desc_Ingot", Amount = 1m }],   // 1 per min
                Ingredients = [new RecipeComponent { ItemClassName = "Desc_Ore", Amount = 1m }]   // 1 per min
            });

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
        }

        public ItemData? GetItem(string itemClassName) => _items.FirstOrDefault(i => i.ClassName == itemClassName);
        public RecipeData? GetRecipe(string recipeClassName) => _recipes.FirstOrDefault(r => r.ClassName == recipeClassName);
        public IEnumerable<RecipeData> GetRecipesProducing(string itemClassName) => _recipes.Where(r => r.Products.Any(p => p.ItemClassName == itemClassName));
        public IEnumerable<ItemData> GetAllItems() => _items;
    }
}