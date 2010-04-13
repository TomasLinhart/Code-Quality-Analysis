using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeQualityAnalysis
{
    public class Type : IDependency
    {
        public ISet<Type> InnerTypes { get; set; }
        public ISet<Method> Methods { get; set; }
        public ISet<Field> Fields { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public Namespace Namespace { get; set; }

        public Type()
        {
            Methods = new HashSet<Method>();
            Fields = new HashSet<Field>();
        }

        public ISet<Type> GetUses()
        {
            var set = new HashSet<Type>();

            foreach (var method in Methods)
            {
                set.Union(method.TypeUses);
            }

            return set;
        }
    }
}
