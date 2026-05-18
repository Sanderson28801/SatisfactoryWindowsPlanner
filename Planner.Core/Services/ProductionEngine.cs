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
    private readonly IRecipeScorer _recipeScorer;

    public ProductionEngine(IDataRepository dataRepository, IRecipeScorer recipeScorer)
    {
        _dataRepository = dataRepository;
        _recipeScorer = recipeScorer;
    }

    public Result<IngredientNode> CalculateProductionTree(
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
            return Result<IngredientNode>.Failure($"No item found for class name: {targetItemClassName}");
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
            return Result<IngredientNode>.Success(currentNode);
        }

        // 2. CHECK THE POOL FIRST! (The Heuristic Scavenger)
        decimal amountScavenged = state.ConsumeByproduct(targetItemClassName, targetAmountPerMinute);
        decimal remainingAmountNeeded = targetAmountPerMinute - amountScavenged;

        currentNode.TargetItemsPerMinute = remainingAmountNeeded;

        if (remainingAmountNeeded <= 0)
        {
            return Result<IngredientNode>.Success(currentNode);
        }

        // 3. SMART SELECTOR
        Recipe? currRecipe = SelectBestRecipe(targetItemClassName, state);
        if (currRecipe is null)
        {
            // No recipe means this is a raw resource
            return Result<IngredientNode>.Success(currentNode);
        }

        currentPath.Add(targetItemClassName);

        // 4. Calculate Machines and Power
        RecipeQuantity? mainProduct = currRecipe.Products.Find(p => p.ItemId == targetItemClassName);
        if (mainProduct == null)
        {
            return Result<IngredientNode>.Failure("Main product not found in recipe");
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

            Result<IngredientNode> childIngredientResult = CalculateProductionTree(
                item.ItemId,
                amountNeededPerMinute,
                state,
                currentPath);

            if (!childIngredientResult.IsSuccess)
            {
                // If a child fails (e.g., missing data), bubble the failure up
                return Result<IngredientNode>.Failure($"Failed to calculate ingredient {item.ItemId}: {childIngredientResult.ErrorMessage}");
            }

            productionNode.Dependencies.Add(childIngredientResult.Value!);
        }

        currentNode.RecipeUsed = productionNode;
        currentPath.Remove(targetItemClassName);

        return Result<IngredientNode>.Success(currentNode);
    }

    private Recipe? SelectBestRecipe(string targetItemId, FactoryState state)
    {
        var validRecipes = _dataRepository.GetRecipesProducing(targetItemId)
            .Where(r => r.IsAlternate == false)
            .ToList();

        if (!validRecipes.Any()) return null;

        // For now, we just create a default profile. Later, this will come from the API parameters!
        var profile = new HeuristicProfile();

        Recipe? bestRecipe = null;
        int lowestScore = int.MaxValue;

        foreach (var recipe in validRecipes)
        {
            // THE MAGIC: The engine delegates the math to the Strategy!
            int score = _recipeScorer.ScoreRecipe(recipe, state, profile, targetItemId);

            if (score < lowestScore)
            {
                lowestScore = score;
                bestRecipe = recipe;
            }
        }

        return bestRecipe;
    }
}