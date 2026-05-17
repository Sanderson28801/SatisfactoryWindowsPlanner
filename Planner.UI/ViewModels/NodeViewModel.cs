using Planner.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel; // The new magic namespace
using Planner.Core.Domain;
using System.Windows;

namespace Planner.UI.ViewModels;

public partial class NodeViewModel : ObservableObject
{
    // 2. We only write the private field. 
    // The generator automatically creates a public "Location" property for us!
    [ObservableProperty]
    private Point _location;

    public IFactoryNode CoreNode { get; }

    public string Title => CoreNode.DisplayName;
    public string RateText => $"{CoreNode.TargetItemsPerMinute:0.##} / min";

    public NodeViewModel(IFactoryNode coreNode)
    {
        CoreNode = coreNode;
    }
}
