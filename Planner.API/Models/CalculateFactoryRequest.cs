using System.Collections.Generic;

namespace Planner.Api.Models;

public class CalculateFactoryRequest
{
    public required string TargetItemId { get; set; }
    public decimal TargetAmountPerMinute { get; set; }

    // The list of alternate recipes the user has checked in the UI
    public List<string> UnlockedAlternates { get; set; } = [];

    // The sliders from the UI! (0.0 to 100.0, etc.)
    public int ByproductPenaltyWeight { get; set; } = 80;
    public int ScavengeRewardWeight { get; set; } = 20;
}