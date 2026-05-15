using Planner.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Planner.Core.Domain;
using Planner.Core.Models;
using System.ComponentModel;

namespace Planner.Core.Services;

public class ProductionEngine : IProductionEngine
{
    private readonly IDataRepository _dataRepository;

    public ProductionEngine(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public IngredientNode CalculateProductionTree(string targetItemClassName, decimal targetAmountPerMinute)
    {
        ItemData currItem = _dataRepository.GetItem(targetItemClassName);
        if (currItem is null)
        {
            throw new InvalidOperationException($"No item found for class name: {targetItemClassName}");
        }
        RecipeData currRecipe = _dataRepository.GetRecipesProducing(targetItemClassName).FirstOrDefault();
        if (currRecipe is null)
        {
            // No recipe means this is a raw resource, so we return an IngredientNode with no dependencies
            return new IngredientNode
            {
                Item = currItem,
                TargetItemsPerMinute = targetAmountPerMinute,
                RecipeUsed = null
            };
        }
        RecipeComponent mainProduct = currRecipe.Products.Find(p => p.ItemClassName == targetItemClassName);
        decimal amountProducedPerMinute = (60 / currRecipe.Time) * mainProduct.Amount;
        decimal numberOfOperations = targetAmountPerMinute / amountProducedPerMinute;
        decimal minuteToTimeRatio = 60m / currRecipe.Time;

        ProductionNode productionNode = new()
        {
            Recipe = currRecipe,
            TargetItemsPerMinute = targetAmountPerMinute,
            Dependencies = [],
            MachinesRequired = numberOfOperations
        };
        string buildingClassName = currRecipe.ProducedIn.FirstOrDefault() ?? string.Empty;

        // 2. Fetch the building from your NEW repository method
        BuildingData? building = _dataRepository.GetBuilding(buildingClassName);

        // 3. Calculate power (defaulting to 0 if no building is found)
        decimal basePower = building?.Metadata.PowerConsumption ?? 0m;
        productionNode.PowerRequired = basePower * productionNode.MachinesRequired;

        //Recursive loop to calculate each ingredient
        foreach (RecipeComponent item in currRecipe.Ingredients)
        {
            decimal amountNeededPerMinute = item.Amount * numberOfOperations * minuteToTimeRatio;
            IngredientNode ingredientNode = CalculateProductionTree(item.ItemClassName, amountNeededPerMinute);
            productionNode.Dependencies.Add(ingredientNode);
        }


        IngredientNode resultNode = new()
        {
            Item = currItem,
            TargetItemsPerMinute = targetAmountPerMinute,
            RecipeUsed = productionNode
        };

        return resultNode;


    }
}
