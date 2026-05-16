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

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ProductionEngine _engine;
    public MainWindow()
    {
        InitializeComponent();

        var repo = new JsonDataRepository("Items&Recipes.json");
        _engine = new ProductionEngine(repo);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        
        string name = ItemInput.Text;
        string amountText = AmountInput.Text;
        if (!decimal.TryParse(amountText, out decimal result))
        {
            ErrorBlock.Text = "Error: Please enter a valid number for the amount.";
            return;
        }
        try { 
            IngredientNode rootNode = _engine.CalculateProductionTree(name, result);

            FactoryTree.Items.Clear();
            TreeViewItem visualRoot = BuildVisualTree(rootNode);
            FactoryTree.Items.Add(visualRoot);
        }
        catch (Exception ex)
        {
            ErrorBlock.Text = $"Error: {ex.Message}";
        }
    }


    private TreeViewItem BuildVisualTree(IFactoryNode node)
    {
        // 1. Create the visual container for this specific node
        TreeViewItem visualItem = new TreeViewItem();

        // 2. Format the text based on what kind of node it is
        if (node is IngredientNode ingredient)
        {
            visualItem.Header = $"[{ingredient.TargetItemsPerMinute:0.##}/min] {ingredient.DisplayName}";
            visualItem.IsExpanded = true; // Automatically open the tree so you don't have to click every arrow

            // If this ingredient requires a recipe, recurse down into the recipe
            if (ingredient.RecipeUsed != null)
            {
                visualItem.Items.Add(BuildVisualTree(ingredient.RecipeUsed));
            }
        }
        else if (node is ProductionNode production)
        {
            visualItem.Header = $"⚙️ Recipe: {production.DisplayName} " +
                                $"({production.MachinesRequired:0.##}x Machines, {production.PowerRequired:0.##} MW)";
            visualItem.IsExpanded = true;

            // Recurse down into all the raw materials needed for this recipe
            foreach (var dependency in production.Dependencies)
            {
                visualItem.Items.Add(BuildVisualTree(dependency));
            }
        }

        // 3. Return the fully built visual branch
        return visualItem;
    }
}