using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel; // The new magic namespace
using Planner.Core.Domain;

namespace Planner.UI.ViewModels;

public partial class ConnectionViewModel : ObservableObject
{
    public NodeViewModel Source { get; }
    public NodeViewModel Target { get; }

    public ConnectionViewModel(NodeViewModel source, NodeViewModel target)
    {
        Source = target;
        Target = source;
    }
}


