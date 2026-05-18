using Planner.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Planner.Core.Domain;
using Planner.Core.Models;

namespace Planner.Core.Services;

public class ProductionEngine : IProductionEngine
{
    private readonly IDataRepository _dataRepository;

    public ProductionEngine(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public IngredientNode CalculateProductionTree(
        string targetItemClassName,
        decimal targetAmountPerMinute,
        FactoryState? state = null,
        HashSet<string>? currentPath = null)
    {
        // Initialize our tracking objects if this is the very top of the tree
        state ??= new FactoryState();
        currentPath ??= new HashSet<string>();

        Item? currItem = _dataRepository.GetItem(targetItemClassName);
        if (currItem is null)
        {
            throw new InvalidOperationException($"No item found for class name: {targetItemClassName}");
        }

        IngredientNode currentNode = new IngredientNode
        {
            Item = currItem,
            RecipeUsed = null
        };

        // Cycle Detection: (Correctly using current path to prevent infinite loops 
        // without improperly memoizing globally explored nodes, ensuring total counts remain accurate)
        if (currentPath.Contains(targetItemClassName))
        {
            currentNode.TargetItemsPerMinute = targetAmountPerMinute;
            return currentNode;
        }

        // 2. CHECK THE POOL FIRST! (The Heuristic Scavenger)
        decimal amountScavenged = state.ConsumeByproduct(targetItemClassName, targetAmountPerMinute);
        decimal remainingAmountNeeded = targetAmountPerMinute - amountScavenged;

        currentNode.TargetItemsPerMinute = remainingAmountNeeded;

        if (remainingAmountNeeded <= 0)
        {
            return currentNode;
        }

        // 3. SMART SELECTOR
        Recipe? currRecipe = SelectBestRecipe(targetItemClassName, state);
        if (currRecipe is null)
        {
            // No recipe means this is a raw resource
            return currentNode;
        }

        currentPath.Add(targetItemClassName);

        // 4. Calculate Machines and Power
        RecipeQuantity? mainProduct = currRecipe.Products.Find(p => p.ItemId == targetItemClassName);
        if (mainProduct == null)
        {
            return currentNode;
        }

        decimal amountProducedPerMinute = (60m / currRecipe.CraftingTimeSeconds) * mainProduct.Amount;
        decimal numberOfOperations = remainingAmountNeeded / amountProducedPerMinute;
        decimal minuteToTimeRatio = 60m / currRecipe.CraftingTimeSeconds;

        ProductionNode productionNode = new ProductionNode
        {
            Recipe = currRecipe,
            TargetItemsPerMinute = remainingAmountNeeded,
            Dependencies = new List<IngredientNode>(),
            MachinesRequired = numberOfOperations,
            // THE REFACTOR WIN: Instant power calculation, no repository lookup!
            PowerRequired = currRecipe.BasePowerDraw * numberOfOperations
        };

        // 5. DUMP WASTE INTO THE POOL
        foreach (RecipeQuantity product in currRecipe.Products)
        {
            if (product.ItemId != targetItemClassName)
            {
                decimal wasteProducedPerMinute = product.Amount * numberOfOperations * minuteToTimeRatio;
                state.AddByproduct(product.ItemId, wasteProducedPerMinute);
            }
        }

        // 6. Recurse into ingredients
        foreach (RecipeQuantity item in currRecipe.Ingredients)
        {
            decimal amountNeededPerMinute = item.Amount * numberOfOperations * minuteToTimeRatio;

            IngredientNode childIngredientNode = CalculateProductionTree(
                item.ItemId,
                amountNeededPerMinute,
                state,
                currentPath);

            productionNode.Dependencies.Add(childIngredientNode);
        }

        currentNode.RecipeUsed = productionNode;
        currentPath.Remove(targetItemClassName);

        return currentNode;
    }

    private Recipe? SelectBestRecipe(string targetItemClassName, FactoryState state)
    {
        // Now safely expecting an empty enumerable for raw materials instead of null
        var validRecipes = _dataRepository.GetRecipesProducing(targetItemClassName)
            .Where(r => r.IsAlternate == false)
            .ToList();

        if (!validRecipes.Any()) return null; // It's a raw material

        Recipe? bestRecipe = null;
        int lowestScore = int.MaxValue;

        foreach (var recipe in validRecipes)
        {
            int score = 0;

            foreach (var product in recipe.Products)
            {
                if (product.ItemId != targetItemClassName)
                {
                    score += 80;
                }
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (state.AvailableByproducts.ContainsKey(ingredient.ItemId)
                    && state.AvailableByproducts[ingredient.ItemId] > 0)
                {
                    score -= 20;
                }
            }

            if (score < lowestScore)
            {
                lowestScore = score;
                bestRecipe = recipe;
            }
        }

        return bestRecipe;
    }
}