using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeQualityAnalysis
{
    public class Module
    {
        public ISet<Type> Types { get; set; }
        public string Name { get; set; }

        public Module()
        {
            Types = new HashSet<Type>();
        }
    }
}
