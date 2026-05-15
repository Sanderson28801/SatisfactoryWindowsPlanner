using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

public class RecipeData
{
    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("className")]
    public required string ClassName { get; init; }

    [JsonPropertyName("alternate")]
    public bool Alternate { get; init; }
    [JsonPropertyName("time")]
    public decimal Time { get; init; }

    [JsonPropertyName("inHand")]
    public bool InHand { get; init; }

    [JsonPropertyName("forBuilding")]
    public bool ForBuilding { get; init; }

    [JsonPropertyName("inWorkshop")]
    public bool InWorkshop { get; init; }

    [JsonPropertyName("inMachine")]
    public bool InMachine { get; init; }

    [JsonPropertyName("manualTimeMultiplier")]
    public decimal ManualTimeMultiplier { get; init; }

    [JsonPropertyName("isVariablePower")]
    public bool IsVariablePower { get; init; }

    [JsonPropertyName("minPower")]
    public int MinPower { get; init; }

    [JsonPropertyName("maxPower")]
    public int MaxPower { get; init; }

    private readonly List<string>? _producedIn = [];

    [JsonPropertyName("producedIn")]
    public List<string> ProducedIn
    {
        get => _producedIn ?? []; // If _producedIn is null, return an empty list instead
        init => _producedIn = value;
    }

    private readonly List<RecipeComponent>? _ingredients = [];
    [JsonPropertyName("ingredients")]
    public List<RecipeComponent> Ingredients
    {
        get => _ingredients ?? [];
        init => _ingredients = value;
    }

    private readonly List<RecipeComponent>? _products = [];
    [JsonPropertyName("products")]
    public List<RecipeComponent> Products
    {
        get => _products ?? [];
        init => _products = value;
    }
}




