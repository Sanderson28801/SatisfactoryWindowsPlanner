using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

internal class ItemData
{

    [JsonPropertyName("slug")]
    public required string slug { get; init; }

    [JsonPropertyName("name")]
    public required string name { get; init; }

    [JsonPropertyName("description")]
    public string description { get; init; } = string.Empty;

    [JsonPropertyName("sinkPoints")]
    public int sinkPoints { get; init; }

    [JsonPropertyName("className")]
    public required string className { get; init; }

    [JsonPropertyName("stackSize")]
    public int stackSize { get; init; }

    [JsonPropertyName("energyValue")]
    public decimal energyValue { get; init; }

    [JsonPropertyName("radioactiveDecay")]
    public decimal radioactiveDecay { get; init; }

    [JsonPropertyName("liquid")]
    public bool liquid { get; init; }
}
