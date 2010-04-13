using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CodeQualityAnalysis
{
    /// <summary>
    /// Extracts neccesery with Mono.Cecil to calculate code metrics
    /// </summary>
    public class MetricsReader
    {
        public Module MainModule { get; private set; } 

        public MetricsReader(string file)
        {
            this.ReadAssembly(file);
        }

        /// <summary>
        /// Opens file as assembly and starts extracting MainModule
        /// </summary>
        /// <param name="file"></param>
        private void ReadAssembly(string file)
        {
            var assembly = AssemblyFactory.GetAssembly(file);
            ReadModule(assembly.MainModule);
        }

        /// <summary>
        /// Extracts main module from assembly
        /// </summary>
        /// <param name="moduleDefinition"></param>
        private void ReadModule(ModuleDefinition moduleDefinition)
        {
            this.MainModule = new Module()
                                  {
                                      Name = moduleDefinition.Name
                                  };

            ReadTypes(MainModule, moduleDefinition.Types);
        }

        /// <summary>
        /// Extracts types from module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="types"></param>
        private void ReadTypes(Module module, TypeDefinitionCollection types)
        {
            // first add all types, because i will need find depend types

            foreach (TypeDefinition typeDefinition in types)
            {
                if (typeDefinition.Name != "<Module>")
                {
                    var type = new Type()
                    {
                        FullName = FormatTypeName(typeDefinition),
                        Name = FormatTypeName(typeDefinition)
                    };

                    // try find first namespace

                    string nsName = String.Empty;

                    if (!String.IsNullOrEmpty(typeDefinition.Namespace))
                        nsName = typeDefinition.Namespace;
                    else
                        nsName = "-";

                    var ns = (from n in module.Namespaces
                              where n.Name == nsName
                              select n).SingleOrDefault();

                    if (ns == null)
                    {
                        ns = new Namespace()
                               {
                                   Name = nsName,
                                   Module = module
                               };

                        module.Namespaces.Add(ns);
                    }

                    type.Namespace = ns;
                    ns.Types.Add(type);
                }
            }

            foreach (TypeDefinition typeDefinition in types)
            {
                if (typeDefinition.Name != "<Module>")
                {
                    var lol = (from n in module.Namespaces
                               from t in n.Types
                               where t.Name == FormatTypeName(typeDefinition)
                               select t).ToArray();

                    var type =
                        (from n in module.Namespaces
                         from t in n.Types
                         where (t.Name == FormatTypeName(typeDefinition))
                        select t).SingleOrDefault();
                    

                    ReadMethods(type, typeDefinition.Methods);

                    ReadConstructors(type, typeDefinition.Constructors);
                }
            }


            // TODO: fields

        }

        /// <summary>
        /// Extracts methods and add them to method list for type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methods"></param>
        private void ReadMethods(Type type, MethodDefinitionCollection methods)
        {
            foreach (MethodDefinition methodDefinition in methods)
            {
                var method = new Method
                {
                    Name = FormatMethodName(methodDefinition),
                    Type = type
                };

                type.Methods.Add(method);
            }

            foreach (MethodDefinition methodDefinition in methods)
            {
                var method = (from m in type.Methods
                              where m.Name == FormatMethodName(methodDefinition)
                              select m).SingleOrDefault();

                if (methodDefinition.Body != null)
                {
                    ReadInstructions(method, methodDefinition, methodDefinition.Body.Instructions);
                }
            }
        }

        /// <summary>
        /// Extracts constructors and add them to method list for type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructors"></param>
        private void ReadConstructors(Type type, ConstructorCollection constructors)
        {
            foreach (MethodDefinition constructor in constructors)
            {
                var method = new Method
                {
                    Name = FormatMethodName(constructor),
                    Type = type
                };

                type.Methods.Add(method);
            }

            foreach (MethodDefinition constructor in  constructors)
            {
                var method = (from m in type.Methods
                              where m.Name == FormatMethodName(constructor)
                              select m).SingleOrDefault();

                if (constructor.Body != null)
                {
                    ReadInstructions(method, constructor, constructor.Body.Instructions);
                }
            }
        }

        /// <summary>
        /// Extracts method calls by extracting instrunctions
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodDefinition"></param>
        /// <param name="instructions"></param>
        public void ReadInstructions(Method method, MethodDefinition methodDefinition,
            InstructionCollection instructions)
        {
            foreach (Instruction instruction in instructions)
            {
                var meth = ReadInstruction(instruction) as MethodDefinition;
                if (meth != null)
                {
                    var type = (from n in method.Type.Namespace.Module.Namespaces
                                from t in n.Types
                                where t.Name == FormatTypeName(meth.DeclaringType) &&
                                n.Name == t.Namespace.Name
                                select t).SingleOrDefault();

                    method.TypeUses.Add(type);

                    var findTargetMethod = (from m in type.Methods
                                            where m.Name == FormatMethodName(meth)
                                            select m).SingleOrDefault();

                    if (findTargetMethod != null && type == method.Type) 
                        method.MethodUses.Add(findTargetMethod);
                }
            }
        }

        /// <summary>
        /// Extracts instruction operand by recursive calling until non-instruction
        /// operand is found 
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public object ReadInstruction(Instruction instruction)
        {
            if (instruction.Operand == null)
                return null;
            
            var nextInstruction = instruction.Operand as Instruction;

            if (nextInstruction != null)
                return ReadInstruction(nextInstruction);
            else
                return instruction.Operand;
        }

        /// <summary>
        /// Formats method name by adding parameters to it. If there are not any parameters
        /// only empty brackers will be added.
        /// </summary>
        /// <param name="methodDefinition"></param>
        /// <returns></returns>
        public static string FormatMethodName(MethodDefinition methodDefinition)
        {
            if (methodDefinition.HasParameters)
            {
                var builder = new StringBuilder();
                var enumerator = methodDefinition.Parameters.GetEnumerator();
                bool hasNext = enumerator.MoveNext();
                while (hasNext)
                {
                    builder.Append(((ParameterDefinition) enumerator.Current).ParameterType.FullName);
                    hasNext = enumerator.MoveNext();
                    if (hasNext)
                        builder.Append(", ");
                }

                return methodDefinition.Name + "(" + builder.ToString() + ")";
            }
            else
            {
                return methodDefinition.Name + "()";
            }
        }

        public static string FormatTypeName(TypeDefinition typeDefinition)
        {
            if (typeDefinition.HasGenericParameters)
            {
                var builder = new StringBuilder();
                var enumerator = typeDefinition.GenericParameters.GetEnumerator();
                bool hasNext = enumerator.MoveNext();
                while (hasNext)
                {
                    builder.Append(((GenericParameter)enumerator.Current).Name);
                    hasNext = enumerator.MoveNext();
                    if (hasNext)
                        builder.Append(",");
                }

                return typeDefinition.Name + "<" + builder.ToString() + ">";
            }

            return typeDefinition.Name; 
        }
    }
}
