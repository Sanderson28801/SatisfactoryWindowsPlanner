namespace Planner.Core.Models;

public class HeuristicProfile
{
    
    public int ByproductPenaltyWeight { get; init; } = 80;
    public int ScavengeRewardWeight { get; init; } = 20;

    
}