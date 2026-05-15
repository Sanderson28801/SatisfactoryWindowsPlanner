using FluentAssertions; // If you installed it
using Planner.Core.Models;
using Planner.Core.Repositories;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Planner.Core.Tests;

public class JsonDataRepositoryTests
{
    [Fact] // This attribute tells xUnit this is a test to run
    public void GetItem_WhenItemExists_ShouldReturnCorrectItemData()
    {
        // --- ARRANGE ---
        // Set up the environment and the inputs
        string testFilePath = "test-data.json";

        // --- ACT ---
        // Execute the specific logic we are testing
        var repository = new JsonDataRepository(testFilePath);
        var result = repository.GetItem("Desc_Cement_C");

        // --- ASSERT ---
        // Prove the result is exactly what we expect

        // Using standard xUnit:
        //Assert.NotNull(result);
        //Assert.Equal("cement", result.Slug);
        //Assert.Equal("Cement", result.Name);
        //Assert.False(result.Liquid);

        //OR, if you installed FluentAssertions(much more readable!):
        result.Should().NotBeNull();
        result!.Slug.Should().Be("cement");
        result.Name.Should().Be("Cement");
        result.Liquid.Should().BeFalse();
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
        Assert.Single(result.Ingredients); // Proves there is exactly 1 ingredient
        Assert.Equal("Desc_Limestone_C", result.Ingredients[0].ItemClassName);
        Assert.Equal(3.0m, result.Ingredients[0].Amount); // The 'm' specifies it's a decimal
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
        // E.g., Iron Ore isn't "produced" by any recipe, it's mined.
        var repository = new JsonDataRepository("test-data.json");
        var result = repository.GetRecipesProducing("Desc_RawIronOre_C");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    [Fact]
    public void Constructor_WhenItemIsMissingRequiredSlug_ShouldThrowJsonException()
    {
        // Arrange - Create bad JSON using raw string literals
        string badJson = """
    {
      "items": {
        "Desc_BadItem_C": {
          "name": "Nameless Item",
          "liquid": false
        }
      }
    }
    """;

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, badJson);

        // Act & Assert
        // We use a lambda expression () => so FluentAssertions can catch the crash
        Action act = () => new JsonDataRepository(tempFile);

        act.Should().Throw<System.Text.Json.JsonException>()
           .WithMessage("*slug*"); // Ensures the error mentions the missing slug property

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

        // These are empty because they were explicitly '[]' in the JSON
        result!.Ingredients.Should().BeEmpty();
        result.Products.Should().BeEmpty();

        // This is explicitly checking YOUR default initialization
        // It proves that missing JSON data falls back to your safe `= [];`
        result.ProducedIn.Should().NotBeNull();
        result.ProducedIn.Should().BeEmpty();

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
            sb.AppendLine($"      \"className\": \"Test Item {i}\",");
            sb.AppendLine($"      \"liquid\": false");

            // Add a comma after every item EXCEPT the very last one
            if (i < totalItems - 1)
            {
                sb.AppendLine("    },");
            }
            else
            {
                sb.AppendLine("    }");
            }
        }
        sb.AppendLine("  }");
        sb.AppendLine("}");

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, sb.ToString());

        // Act - Measure the time it takes to parse
        var watch = Stopwatch.StartNew();
        var repository = new JsonDataRepository(tempFile);
        watch.Stop();

        var allItems = repository.GetAllItems().ToList();

        // Assert
        allItems.Should().HaveCount(totalItems);

        // Proves the source generator is fast (should be well under 500ms)
        watch.ElapsedMilliseconds.Should().BeLessThan(500);

        // Prove O(1) dictionary lookup works at the very end of the massive list
        var lastItem = repository.GetItem($"Desc_TestItem_{totalItems - 1}_C");
        lastItem.Should().NotBeNull();
        lastItem!.Name.Should().Be($"Test Item {totalItems - 1}");

        File.Delete(tempFile);
    }
}