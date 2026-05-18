using Planner.Core.Domain;
using Planner.Core.Models;

namespace Planner.Core.Interfaces;

public interface IRecipeScorer
{
    int ScoreRecipe(Recipe recipe, FactoryState state, HeuristicProfile profile, string targetItemId);
}