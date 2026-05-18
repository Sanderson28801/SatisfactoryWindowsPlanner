using Planner.Core.Domain;
using Planner.Core.Interfaces;
using Planner.Core.Models;
using Planner.Core.Repositories;
using Planner.Core.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace Planner.UI;

using Planner.UI.ViewModels;
using System.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ProductionEngine _engine;

    public GraphViewModel Graph { get; } = new GraphViewModel();

    public MainWindow()
    {
        InitializeComponent();


        DataContext = Graph;
        var repo = new JsonDataRepository("Data/Items&Recipes.json");
        _engine = new ProductionEngine(repo);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(AmountInput.Text, out decimal targetAmount)) return;

        try
        {
            // 1. Math
            var rootNode = _engine.CalculateProductionTree(ItemInput.Text, targetAmount);


            // 2. Clear old UI
            Graph.Nodes.Clear();
            Graph.Connections.Clear();

            // 3. Flatten and Draw!
            int startingRow = 0;

            // Make sure the item they typed actually has a recipe
            if (rootNode.RecipeUsed != null)
            {
                FlattenRecipesOnly(rootNode.RecipeUsed, 0, ref startingRow);
            }
            else
            {
                // Edge Case: They searched for a raw material like "Desc_Ore"
                var rawVM = new NodeViewModel(rootNode) { Location = new Point(0, 0) };
                Graph.Nodes.Add(rawVM);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    // We use 'depth' (X) and 'row' (Y) to push the nodes apart on the canvas
    // Notice we are passing a ProductionNode (Recipe) now, not the base interface!
    private NodeViewModel FlattenRecipesOnly(ProductionNode currentRecipe, int depth, ref int globalRow)
    {
        // 1. Create the ViewModel for THIS recipe
        var recipeVM = new NodeViewModel(currentRecipe)
        {
            // Space them out: 300px horizontally per tier, 150px vertically
            Location = new Point(depth * 30, globalRow * 15)
        };

        Graph.Nodes.Add(recipeVM);

        // Push the global row down so the next item drawn doesn't overlap us
        globalRow++;

        // 2. Loop through the dependencies (Ingredients)
        foreach (IngredientNode ingredient in currentRecipe.Dependencies)
        {
            // Does this ingredient have a recipe? (e.g., Iron Ingot -> Smelt Iron Ingot)
            if (ingredient.RecipeUsed != null)
            {
                // RECURSE DIRECTLY INTO THE RECIPE!
                // We completely ignore drawing the 'ingredient' node.
                var childRecipeVM = FlattenRecipesOnly(ingredient.RecipeUsed, depth + 1, ref globalRow);

                // Draw a wire connecting the Parent Recipe directly to the Child Recipe
                Graph.Connections.Add(new ConnectionViewModel(recipeVM, childRecipeVM));
            }
            else
            {
                // RAW MATERIAL EXCEPTION (e.g., Iron Ore)
                // It has no recipe, so we MUST draw the ingredient node so the user sees it.
                var rawMaterialVM = new NodeViewModel(ingredient)
                {
                    Location = new Point((depth + 1) * 30, globalRow * 15)
                };

                Graph.Nodes.Add(rawMaterialVM);
                Graph.Connections.Add(new ConnectionViewModel(recipeVM, rawMaterialVM));

                globalRow++; // Push the row down for the next item
            }
        }

        return recipeVM;
    }


}