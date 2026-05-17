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
            FlattenTreeToGraph(rootNode);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    // We use 'depth' (X) and 'row' (Y) to push the nodes apart on the canvas
    private NodeViewModel FlattenTreeToGraph(IFactoryNode currentCoreNode, int depth = 0, int row = 0)
    {
        // 1. Create the ViewModel for this node
        var nodeVM = new NodeViewModel(currentCoreNode)
        {
            // Space them out: 300px horizontal per tier, 150px vertical per row
            Location = new Point(depth * 300, row * 150)
        };

        // 2. Add it to the screen
        Graph.Nodes.Add(nodeVM);

        // 3. Recurse! Pattern match to find the children
        int childRow = row; // Keep track of rows so siblings don't stack

        if (currentCoreNode is IngredientNode ingredient && ingredient.RecipeUsed != null)
        {
            // Ingredients go left-to-right into Recipes (Depth + 1)
            var childVM = FlattenTreeToGraph(ingredient.RecipeUsed, depth + 1, childRow);
            Graph.Connections.Add(new ConnectionViewModel(nodeVM, childVM));
        }
        else if (currentCoreNode is ProductionNode production)
        {
            // Recipes split into multiple Ingredients (Depth + 1, different rows)
            foreach (var dependency in production.Dependencies)
            {
                var childVM = FlattenTreeToGraph(dependency, depth + 1, childRow);
                Graph.Connections.Add(new ConnectionViewModel(nodeVM, childVM));
                childRow++; // Push the next dependency down a row
            }
        }

        return nodeVM;
    }


}