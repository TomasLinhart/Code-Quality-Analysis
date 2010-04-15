using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CodeQualityAnalysis
{
    public class Field : IDependency
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public BidirectionalGraph<object, IEdge<object>> BuildDependencyGraph()
        {
            return null;
        }
    }
}
