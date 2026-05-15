using System;
using System.Collections.Generic;
using System.Text;

namespace Planner.Core.Interfaces;

public interface IFactoryNode
{
    string DisplayName { get; }
    decimal TargetItemsPerMinute { get; }
}
