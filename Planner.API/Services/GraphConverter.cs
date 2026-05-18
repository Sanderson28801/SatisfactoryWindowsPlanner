using Planner.Core.Domain;
using Planner.Api.Models;

namespace Planner.Api.Services
{
    public static class GraphConverter
    {
        public static GraphDto FlattenAndConsolidate(IngredientNode rootNode)
        {
            var nodes = new Dictionary<string, NodeDto>();
            var edges = new Dictionary<string, EdgeDto>();

            // Kick off the recursive walk starting at the final product
            TraverseIngredient(rootNode, null);

            return new GraphDto(nodes.Values.ToList(), edges.Values.ToList());

            // --- LOCAL RECURSIVE METHODS ---

            void TraverseIngredient(IngredientNode currItem, string? consumerRecipeId)
            {
                string itemId = "item_" + currItem.Item.Id;

                // 1. Consolidate the Item Node
                if (!nodes.ContainsKey(itemId))
                {
                    nodes[itemId] = new NodeDto { Id = itemId, Label = currItem.DisplayName, Type = "Item", Amount = 0 };
                }
                nodes[itemId].Amount += currItem.TargetItemsPerMinute;

                // 2. Create an Edge TO the Recipe that needs this item (if it's not the final product)
                if (consumerRecipeId != null)
                {
                    string edgeId = $"{itemId}->{consumerRecipeId}";
                    if (!edges.ContainsKey(edgeId))
                    {
                        edges[edgeId] = new EdgeDto { Source = itemId, Target = consumerRecipeId, Amount = 0 };
                    }
                    edges[edgeId].Amount += currItem.TargetItemsPerMinute;
                }

                // 3. Recurse into the recipe that makes this item
                if (currItem.RecipeUsed != null)
                {
                    TraverseRecipe(currItem.RecipeUsed, itemId, currItem.TargetItemsPerMinute);
                }
            }

            void TraverseRecipe(ProductionNode currRecipe, string producedItemId, decimal amountProducedForThisPath)
            {
                string recipeId = "recipe_" + currRecipe.Recipe.Id;

                // 1. Consolidate the Recipe Node
                if (!nodes.ContainsKey(recipeId))
                {
                    nodes[recipeId] = new NodeDto { Id = recipeId, Label = currRecipe.DisplayName, Type = "Recipe", Machines = 0, Power = 0 };
                }
                nodes[recipeId].Machines += currRecipe.MachinesRequired;
                nodes[recipeId].Power += currRecipe.PowerRequired;

                // 2. Create an Edge FROM this recipe TO the item it just produced
                string edgeId = $"{recipeId}->{producedItemId}";
                if (!edges.ContainsKey(edgeId))
                {
                    edges[edgeId] = new EdgeDto { Source = recipeId, Target = producedItemId, Amount = 0 };
                }
                edges[edgeId].Amount += amountProducedForThisPath;

                // 3. Recurse into all the raw materials this recipe needs
                foreach (var dependency in currRecipe.Dependencies)
                {
                    TraverseIngredient(dependency, recipeId);
                }
            }
        }
    }
}