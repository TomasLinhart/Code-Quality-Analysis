using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeQualityAnalysis
{
    public class Namespace : IDependency
    {
        public ISet<Type> Types { get; set; }
        public string Name { get; set; }
        public Module Module { get; set; }

        public Namespace()
        {
            Types = new HashSet<Type>();
        }
    }
}
