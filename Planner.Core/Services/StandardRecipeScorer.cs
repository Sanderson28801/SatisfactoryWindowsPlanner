using Planner.Core.Domain;
using Planner.Core.Interfaces;
using Planner.Core.Models;

namespace Planner.Core.Services;

public class StandardRecipeScorer : IRecipeScorer
{
    public int ScoreRecipe(Recipe recipe, FactoryState state, HeuristicProfile profile, string targetItemId)
    {
        int score = 0;

        // PENALTY: Does it produce messy byproducts?
        foreach (var product in recipe.Products)
        {
            if (product.ItemId != targetItemId)
            {
                // Uses the UI profile weight instead of a hardcoded 80
                score += profile.ByproductPenaltyWeight;
            }
        }

        // REWARD: Does it consume our waste?
        foreach (var ingredient in recipe.Ingredients)
        {
            if (state.AvailableByproducts.TryGetValue(ingredient.ItemId, out decimal amount) && amount > 0)
            {
                // Uses the UI profile weight instead of a hardcoded 20
                score -= profile.ScavengeRewardWeight;
            }
        }

        return score;
    }
}