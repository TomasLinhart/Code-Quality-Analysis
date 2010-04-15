using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace CodeQualityAnalysis
{
    public class Module : IDependency
    {
        public ISet<Namespace> Namespaces { get; set; }
        public string Name { get; set; }

        public Module()
        {
            Namespaces = new HashSet<Namespace>();
        }

        public BidirectionalGraph<object, IEdge<object>> BuildDependencyGraph()
        {
            return null;
        }
    }
}
