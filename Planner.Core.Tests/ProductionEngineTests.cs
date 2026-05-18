using Planner.Core.Domain;
using Planner.Core.Services;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Planner.Core.Tests
{
    public class ProductionEngineTests
    {
        private readonly ProductionEngine _engine;

        public ProductionEngineTests()
        {
            // Inject BOTH our fake repository and our new Strategy scorer
            var repo = new FakeDataRepository();
            var scorer = new StandardRecipeScorer();
            _engine = new ProductionEngine(repo, scorer);
        }

        [Fact]
        public void Calculate_WithRawResource_ShouldReturnNodeWithNoRecipe()
        {
            // Act: Ask for 10 Iron Ore / min
            var result = _engine.CalculateProductionTree("Desc_Ore", 10m);

            // Assert: Open the Result wrapper first
            result.IsSuccess.Should().BeTrue();
            var node = result.Value!;

            node.Should().NotBeNull();
            node.Item.Name.Should().Be("Iron Ore");
            node.TargetItemsPerMinute.Should().Be(10m);

            // The tree stops here
            node.RecipeUsed.Should().BeNull();
        }

        [Fact]
        public void Calculate_WithSimpleRecipe_ShouldBuildOneLevelTree()
        {
            // Act: Ask for 25 Ingots / min
            var result = _engine.CalculateProductionTree("Desc_Ingot", 25m);

            // Assert: Result Success
            result.IsSuccess.Should().BeTrue();
            var node = result.Value!;

            // Assert: Root Node (Ingot)
            node.TargetItemsPerMinute.Should().Be(25m);
            node.RecipeUsed.Should().NotBeNull();
            node.RecipeUsed!.Recipe.Name.Should().Be("Smelt Ingot");

            // Assert: Dependencies (Ore)
            node.RecipeUsed.Dependencies.Should().HaveCount(1);
            var oreNode = node.RecipeUsed.Dependencies.First();

            oreNode.Item.Name.Should().Be("Iron Ore");
            oreNode.TargetItemsPerMinute.Should().Be(25m);
            oreNode.RecipeUsed.Should().BeNull();
        }

        [Fact]
        public void Calculate_WithComplexMathRatio_ShouldCalculateCorrectIngredientRates()
        {
            // Act
            var result = _engine.CalculateProductionTree("Desc_Wire", 15m);
            result.IsSuccess.Should().BeTrue();
            var node = result.Value!;

            // Assert Root
            node.TargetItemsPerMinute.Should().Be(15m);

            // Assert Dependency Math
            var ingotNode = node.RecipeUsed!.Dependencies.First();
            ingotNode.Item.Name.Should().Be("Iron Ingot");
            ingotNode.TargetItemsPerMinute.Should().Be(5m);
        }

        [Fact]
        public void Calculate_WithMultipleIngredients_ShouldBranchAndRecurseCorrectly()
        {
            // Act
            var result = _engine.CalculateProductionTree("Desc_Stator", 10m);
            result.IsSuccess.Should().BeTrue();
            var root = result.Value!;

            // Assert Branches
            var deps = root.RecipeUsed!.Dependencies;
            deps.Should().HaveCount(2);

            // NOTE: Changed ClassName to Id to match our new Domain Model
            var wireDep = deps.First(d => d.Item.Id == "Desc_Wire");
            var ingotDep = deps.First(d => d.Item.Id == "Desc_Ingot");

            // Check math on the branches
            wireDep.TargetItemsPerMinute.Should().Be(80m);
            ingotDep.TargetItemsPerMinute.Should().Be(30m);

            // Check that the tree CONTINUED to recurse down the Wire branch
            wireDep.RecipeUsed.Should().NotBeNull();
            wireDep.RecipeUsed!.Recipe.Name.Should().Be("Craft Wire");

            // Calculate final raw Ore needed down the Ingot branch
            ingotDep.RecipeUsed!.Dependencies.First().TargetItemsPerMinute.Should().Be(30m);
        }

        [Fact]
        public void Calculate_ShouldCorrectlyCalculateMachinesRequired()
        {
            // Act
            var result = _engine.CalculateProductionTree("Desc_Ingot", 25m);
            result.IsSuccess.Should().BeTrue();

            // Assert
            var productionNode = result.Value!.RecipeUsed;
            productionNode.Should().NotBeNull();
            productionNode!.MachinesRequired.Should().Be(25m);
        }

        [Fact]
        public void Calculate_ShouldCorrectlyCalculatePowerRequired()
        {
            // Act
            var result = _engine.CalculateProductionTree("Desc_Ingot", 25m);
            result.IsSuccess.Should().BeTrue();

            // Assert
            var productionNode = result.Value!.RecipeUsed;
            productionNode.Should().NotBeNull();

            // 25 machines * 4 MW = 100 MW
            // Note: The math hasn't changed, but it's now using BasePowerDraw under the hood!
            productionNode!.PowerRequired.Should().Be(100m);
        }

        [Fact]
        public void CalculateTree_WhenItemDoesNotExist_ReturnsFailureResult()
        {
            // Arrange
            var mockRepo = new FakeDataRepository();
            var scorer = new StandardRecipeScorer();
            var engine = new ProductionEngine(mockRepo, scorer); // Inject both here too!

            // Act (Try to calculate a fake item)
            var result = engine.CalculateProductionTree("Invalid_Item", 100);

            // Assert (Verify the FedEx box is marked as a failure)
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Contains("No item found", result.ErrorMessage);
        }
    }
}