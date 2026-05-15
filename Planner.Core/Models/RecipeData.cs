using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

internal class RecipeData
{
    [JsonPropertyName("slug")]
    public required string slug { get; init; }

    [JsonPropertyName("name")]
    public required string name { get; init; }

    [JsonPropertyName("className")]
    public required string className { get; init; }

    [JsonPropertyName("alternate")]
    public bool alternate { get; init; }
    [JsonPropertyName("time")]
    public int time { get; init; }

    [JsonPropertyName("inHand")]
    public bool inHand { get; init; }

    [JsonPropertyName("forBuilding")]
    public bool forBuilding { get; init; }

    [JsonPropertyName("inWorkshop")]
    public bool inWorkshop { get; init; }

    [JsonPropertyName("inMachine")]
    public bool inMachine { get; init; }

    [JsonPropertyName("manualTimeMultiplier")]
    public decimal manualTimeMultiplier { get; init; }
    [JsonPropertyName("ingredients")]
    public List<RecipeComponent> Ingredients { get; init; } = [];

    [JsonPropertyName("products")]
    public List<RecipeComponent> Products { get; init; } = [];

    [JsonPropertyName("producedIn")]
    public List<string> ProducedIn { get; init; } = [];

    [JsonPropertyName("isVariablePower")]
    public bool IsVariablePower { get; init; }

    [JsonPropertyName("minPower")]
    public int MinPower { get; init; }

    [JsonPropertyName("maxPower")]
    public int MaxPower { get; init; }
}




