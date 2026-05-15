using Planner.Core.Domain;
using Planner.Core.Services; // Assuming your engine is here
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
            // Inject our perfectly controlled fake data into your real engine
            var repo = new FakeDataRepository();
            _engine = new ProductionEngine(repo);
        }

        [Fact]
        public void Calculate_WithRawResource_ShouldReturnNodeWithNoRecipe()
        {
            // Act: Ask for 10 Iron Ore / min
            IngredientNode result = _engine.CalculateProductionTree("Desc_Ore", 10m);

            // Assert: It should build the node, but stop recursing because Ore has no recipe
            result.Should().NotBeNull();
            result.Item.Name.Should().Be("Iron Ore");
            result.TargetItemsPerMinute.Should().Be(10m);

            // The tree stops here
            result.RecipeUsed.Should().BeNull();
        }

        [Fact]
        public void Calculate_WithSimpleRecipe_ShouldBuildOneLevelTree()
        {
            // Act: Ask for 25 Ingots / min
            IngredientNode result = _engine.CalculateProductionTree("Desc_Ingot", 25m);

            // Assert: Root Node (Ingot)
            result.TargetItemsPerMinute.Should().Be(25m);
            result.RecipeUsed.Should().NotBeNull();
            result.RecipeUsed!.Recipe.Name.Should().Be("Smelt Ingot");

            // Assert: Dependencies (Ore)
            result.RecipeUsed.Dependencies.Should().HaveCount(1);
            var oreNode = result.RecipeUsed.Dependencies.First();

            oreNode.Item.Name.Should().Be("Iron Ore");
            // Since the recipe is 1 Ore to 1 Ingot, we should need exactly 25 Ore/min
            oreNode.TargetItemsPerMinute.Should().Be(25m);
            oreNode.RecipeUsed.Should().BeNull();
        }

        [Fact]
        public void Calculate_WithComplexMathRatio_ShouldCalculateCorrectIngredientRates()
        {
            // The Wire recipe naturally produces 6/min and costs 2 Ingot/min.
            // If we ask for 15 Wire/min, the engine must scale by a factor of 2.5.
            // Therefore, we should need exactly 5 Ingot/min (2 * 2.5).

            // Act
            IngredientNode result = _engine.CalculateProductionTree("Desc_Wire", 15m);

            // Assert Root
            result.TargetItemsPerMinute.Should().Be(15m);

            // Assert Dependency Math
            var ingotNode = result.RecipeUsed!.Dependencies.First();
            ingotNode.Item.Name.Should().Be("Iron Ingot");

            // If this fails, your (60 / time * amount) multiplier math is slightly off!
            ingotNode.TargetItemsPerMinute.Should().Be(5m);
        }

        [Fact]
        public void Calculate_WithMultipleIngredients_ShouldBranchAndRecurseCorrectly()
        {
            // The Stator recipe requires BOTH Wire and Ingots.
            // Base Stator rate: 5/min. 
            // We ask for 10/min (Multiplier = 2).
            // Base Wire required: 40/min. Target should be 80.
            // Base Ingot required: 15/min. Target should be 30.

            // Act
            IngredientNode root = _engine.CalculateProductionTree("Desc_Stator", 10m);

            // Assert Branches
            var deps = root.RecipeUsed!.Dependencies;
            deps.Should().HaveCount(2);

            var wireDep = deps.First(d => d.Item.ClassName == "Desc_Wire");
            var ingotDep = deps.First(d => d.Item.ClassName == "Desc_Ingot");

            // Check math on the branches
            wireDep.TargetItemsPerMinute.Should().Be(80m);
            ingotDep.TargetItemsPerMinute.Should().Be(30m);

            // Check that the tree CONTINUED to recurse down the Wire branch
            wireDep.RecipeUsed.Should().NotBeNull();
            wireDep.RecipeUsed!.Recipe.Name.Should().Be("Craft Wire");

            // Calculate final raw Ore needed down the Ingot branch
            // 30 Ingots/min should require 30 Ore/min
            ingotDep.RecipeUsed!.Dependencies.First().TargetItemsPerMinute.Should().Be(30m);
        }
    }
}