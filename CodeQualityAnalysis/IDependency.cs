﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CodeQualityAnalysis
{
    public interface IDependency
    {
        string Name { set; get; }
        BidirectionalGraph<object, IEdge<object>> BuildDependencyGraph();
    }
}
