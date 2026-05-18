using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Planner.Core.Data.Entities;

public record RecipeComponent
{
    [JsonPropertyName("item")]
    public required string ItemClassName { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}
