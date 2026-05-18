using Planner.Core.Domain;
using Planner.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Planner.Core.Tests
{
    // A fake repository solely for testing the Engine's math and traversal
    public class FakeDataRepository : IDataRepository
    {
        private readonly List<Item> _items = [];
        private readonly List<Recipe> _recipes = [];

        public FakeDataRepository()
        {
            // 1. Raw Resource (No recipe produces this)
            _items.Add(new Item { Id = "Desc_Ore", Name = "Iron Ore", IsLiquid = false });

            // 2. Simple Ratio Item 
            _items.Add(new Item { Id = "Desc_Ingot", Name = "Iron Ingot", IsLiquid = false });
            _recipes.Add(new Recipe
            {
                Id = "Recipe_Ingot",
                Name = "Smelt Ingot",
                CraftingTimeSeconds = 60m, // 1 cycle per min
                BasePowerDraw = 4m,        // Power attached directly! No building needed.
                Products = [new RecipeQuantity { ItemId = "Desc_Ingot", Amount = 1m }],
                Ingredients = [new RecipeQuantity { ItemId = "Desc_Ore", Amount = 1m }]
            });

            // 3. Complex Ratio Item (Produces multiple, requires multiple)
            _items.Add(new Item { Id = "Desc_Wire", Name = "Wire", IsLiquid = false });
            _recipes.Add(new Recipe
            {
                Id = "Recipe_Wire",
                Name = "Craft Wire",
                CraftingTimeSeconds = 30m, // 2 cycles per minute
                BasePowerDraw = 4m,
                Products = [new RecipeQuantity { ItemId = "Desc_Wire", Amount = 3m }],    // 6 per min base
                Ingredients = [new RecipeQuantity { ItemId = "Desc_Ingot", Amount = 1m }] // 2 per min base
            });

            // 4. Multi-Ingredient Item
            _items.Add(new Item { Id = "Desc_Stator", Name = "Stator", IsLiquid = false });
            _recipes.Add(new Recipe
            {
                Id = "Recipe_Stator",
                Name = "Assemble Stator",
                CraftingTimeSeconds = 12m, // 5 cycles per minute
                BasePowerDraw = 5m,
                Products = [new RecipeQuantity { ItemId = "Desc_Stator", Amount = 1m }],  // 5 per min base
                Ingredients = [
                    new RecipeQuantity { ItemId = "Desc_Wire", Amount = 8m },  // 40 per min base
                    new RecipeQuantity { ItemId = "Desc_Ingot", Amount = 3m }  // 15 per min base
                ]
            });
        }

        public Item? GetItem(string id) => _items.FirstOrDefault(i => i.Id == id);
        public Recipe? GetRecipe(string id) => _recipes.FirstOrDefault(r => r.Id == id);
        public IEnumerable<Recipe> GetRecipesProducing(string id) => _recipes.Where(r => r.Products.Any(p => p.ItemId == id));
        public IEnumerable<Item> GetAllItems() => _items;
    }
}