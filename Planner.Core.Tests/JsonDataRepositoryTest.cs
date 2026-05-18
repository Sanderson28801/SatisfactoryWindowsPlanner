using FluentAssertions;
using Planner.Core.Repositories;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Planner.Core.Tests;

public class JsonDataRepositoryTests
{
    [Fact]
    public void GetItem_WhenItemExists_ShouldReturnCorrectItemData()
    {
        // Arrange
        string testFilePath = "test-data.json";

        // Act
        var repository = new JsonDataRepository(testFilePath);
        var result = repository.GetItem("Desc_Cement_C");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cement");
        result.IsLiquid.Should().BeFalse(); // Changed from Liquid to IsLiquid
    }

    [Fact]
    public void GetRecipe_WhenRecipeExists_ShouldReturnCorrectIngredients()
    {
        // Arrange
        var repository = new JsonDataRepository("test-data.json");

        // Act
        var result = repository.GetRecipe("Recipe_Cement_C");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Ingredients);
        Assert.Equal("Desc_Limestone_C", result.Ingredients[0].ItemId); // ItemClassName -> ItemId
        Assert.Equal(3.0m, result.Ingredients[0].Amount);
    }

    [Fact]
    public void GetItem_WhenItemDoesNotExist_ShouldReturnNull()
    {
        var repository = new JsonDataRepository("test-data.json");
        var result = repository.GetItem("Desc_GhostItem_C");
        result.Should().BeNull();
    }

    [Fact]
    public void GetRecipe_WhenRecipeDoesNotExist_ShouldReturnNull()
    {
        var repository = new JsonDataRepository("test-data.json");
        var result = repository.GetRecipe("Recipe_NonExistent_C");
        result.Should().BeNull();
    }

    [Fact]
    public void GetRecipesProducing_WhenItemIsRawResource_ShouldReturnEmptyList()
    {
        var repository = new JsonDataRepository("test-data.json");
        var result = repository.GetRecipesProducing("Desc_RawIronOre_C");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenItemIsMissingRequiredSlug_ShouldThrowJsonException()
    {
        // Arrange - The JSON entity still requires a slug to parse!
        string badJson = """
        {
          "items": {
            "Desc_BadItem_C": {
              "name": "Nameless Item",
              "className": "Desc_BadItem_C",
              "liquid": false
            }
          },
          "recipes": {},
          "buildings": {}
        }
        """;

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, badJson);

        // Act & Assert
        Action act = () => new JsonDataRepository(tempFile);

        act.Should().Throw<System.Text.Json.JsonException>()
           .WithMessage("*slug*");

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Constructor_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        Action act = () => new JsonDataRepository("completely-fake-path.json");
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetRecipe_WhenMissingProperties_ShouldRetainEmptyListDefaults()
    {
        // Arrange
        string weirdRecipeJson = """
        {
          "items": {},
          "buildings": {},
          "recipes": {
            "Recipe_Weird_C": {
              "slug": "weird",
              "name": "Weird Recipe",
              "className": "Recipe_Weird_C",
              "ingredients": [],
              "products": []
            }
          }
        }
        """;
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, weirdRecipeJson);
        var repository = new JsonDataRepository(tempFile);

        // Act
        var result = repository.GetRecipe("Recipe_Weird_C");

        // Assert
        result.Should().NotBeNull();
        result!.Ingredients.Should().BeEmpty();
        result.Products.Should().BeEmpty();

        // Because "producedIn" was omitted in the JSON, our mapper leaves MachineClassName as null
        result.MachineClassName.Should().BeNull();

        File.Delete(tempFile);
    }

    [Fact]
    public void GetAllItems_WithMassiveDataset_ShouldLoadSuccessfullyAndPerformantly()
    {
        // Arrange: Build a 10,000 item JSON string safely
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"items\": {");

        int totalItems = 10000;
        for (int i = 0; i < totalItems; i++)
        {
            sb.AppendLine($"    \"Desc_TestItem_{i}_C\": {{");
            sb.AppendLine($"      \"slug\": \"test-item-{i}\",");
            sb.AppendLine($"      \"name\": \"Test Item {i}\",");
            sb.AppendLine($"      \"className\": \"Desc_TestItem_{i}_C\",");
            sb.AppendLine($"      \"liquid\": false");

            if (i < totalItems - 1) sb.AppendLine("    },");
            else sb.AppendLine("    }");
        }
        sb.AppendLine("  },");
        sb.AppendLine("  \"recipes\": {},");
        sb.AppendLine("  \"buildings\": {}");
        sb.AppendLine("}");

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, sb.ToString());

        // Act - Measure the time it takes to parse and map everything to Domain models
        var watch = Stopwatch.StartNew();
        var repository = new JsonDataRepository(tempFile);
        watch.Stop();

        var allItems = repository.GetAllItems().ToList();

        // Assert
        allItems.Should().HaveCount(totalItems);
        watch.ElapsedMilliseconds.Should().BeLessThan(500);

        var lastItem = repository.GetItem($"Desc_TestItem_{totalItems - 1}_C");
        lastItem.Should().NotBeNull();
        lastItem!.Name.Should().Be($"Test Item {totalItems - 1}");

        File.Delete(tempFile);
    }
}