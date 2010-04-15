using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

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

        public BidirectionalGraph<object, IEdge<object>> BuildDependencyGraph()
        {
            var g = new BidirectionalGraph<object, IEdge<object>>();

            foreach (var method in Methods)
            {
                g.AddVertex(method.Name);
            }

            foreach (var field in Fields)
            {
                g.AddVertex(field.Name);
            }

            foreach (var method in Methods)
            {
                foreach (var methodUse in method.MethodUses)
                {
                    g.AddEdge(new Edge<object>(method.Name, methodUse.Name));
                }

                foreach (var fieldUse in method.FieldUses)
                {
                    g.AddEdge(new Edge<object>(method.Name, fieldUse.Name));
                }
            }

            return g;
        }
    }
}
