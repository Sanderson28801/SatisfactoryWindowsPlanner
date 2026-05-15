using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

public class SatisfactoryData
{
    [JsonPropertyName("items")]
    public Dictionary<string, ItemData> Items { get; init; } = [];

    // You likely have a recipes dictionary further down in the JSON
    [JsonPropertyName("recipes")]
    public Dictionary<string, RecipeData> Recipes { get; init; } = [];
    [JsonPropertyName("buildings")]
    public Dictionary<string, BuildingData> Buildings { get; init; } = [];
}
