﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeQualityAnalysis
{
    public class Type
    {
        public ISet<Method> Methods { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public Module Module { get; set; }

        public Type()
        {
            Methods = new HashSet<Method>();
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