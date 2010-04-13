using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
