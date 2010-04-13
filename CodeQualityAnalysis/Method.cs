using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeQualityAnalysis
{
    public class Method : IDependency
    {
        public ISet<Type> TypeUses { get; set; }
        public ISet<Method> MethodUses { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }

        public Method()
        {
            TypeUses = new HashSet<Type>();
            MethodUses = new HashSet<Method>();
        }
    }
}
