using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

internal class ItemData
{

    [JsonPropertyName("slug")]
    public string slug { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string description { get; init; } = string.Empty;

    [JsonPropertyName("sinkPoints")]
    public int sinkPoints { get; init; }

    [JsonPropertyName("className")]
    public string className { get; init; } = string.Empty;

    [JsonPropertyName("stackSize")]
    public int stackSize { get; init; }

    [JsonPropertyName("energyValue")]
    public double energyValue { get; init; }

    [JsonPropertyName("radioactiveDecay")]
    public double radioactiveDecay { get; init; }

    [JsonPropertyName("liquid")]
    public bool liquid { get; init; }
}
