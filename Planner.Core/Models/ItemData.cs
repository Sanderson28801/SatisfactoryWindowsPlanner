using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Models;

public class ItemData
{

    [JsonPropertyName("slug")]
    public required string Slug { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("sinkPoints")]
    public int SinkPoints { get; init; }

    [JsonPropertyName("className")]
    public required string ClassName { get; init; }

    [JsonPropertyName("stackSize")]
    public int StackSize { get; init; }

    [JsonPropertyName("energyValue")]
    public decimal EnergyValue { get; init; }

    [JsonPropertyName("radioactiveDecay")]
    public decimal RadioactiveDecay { get; init; }

    [JsonPropertyName("liquid")]
    public bool Liquid { get; init; }
}
