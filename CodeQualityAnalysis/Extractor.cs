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
    public class Extractor
    {
        public Module MainModule { get; private set; } 

        public Extractor(string file)
        {
            this.ExtractAssembly(file);
        }

        /// <summary>
        /// Opens file as assembly and starts extracting MainModule
        /// </summary>
        /// <param name="file"></param>
        private void ExtractAssembly(string file)
        {
            var assembly = AssemblyFactory.GetAssembly(file);
            ExtractModule(assembly.MainModule);
        }

        /// <summary>
        /// Extracts main module from assembly
        /// </summary>
        /// <param name="moduleDefinition"></param>
        private void ExtractModule(ModuleDefinition moduleDefinition)
        {
            this.MainModule = new Module()
                                  {
                                      Name = moduleDefinition.Name
                                  };

            ExtractTypes(MainModule, moduleDefinition.Types);
        }

        /// <summary>
        /// Extracts types from module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="types"></param>
        private void ExtractTypes(Module module, TypeDefinitionCollection types)
        {
            // first add all types, because i will need find depend types

            foreach (TypeDefinition typeDefinition in types)
            {
                if (typeDefinition.Name != "<Module>")
                {
                    var type = new Type()
                    {
                        FullName = typeDefinition.FullName,
                        Name = typeDefinition.Name,
                        Module = module
                    };

                    module.Types.Add(type);
                }
            }

            foreach (TypeDefinition typeDefinition in types)
            {
                if (typeDefinition.Name != "<Module>")
                {
                    var type = (from t in module.Types
                                where t.FullName == typeDefinition.FullName
                                select t).SingleOrDefault();

                    ExtractMethods(type, typeDefinition.Methods);

                    ExtractConstructors(type, typeDefinition.Constructors);
                }
            }


            // TODO: fields

        }

        /// <summary>
        /// Extracts methods and add them to method list for type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methods"></param>
        private void ExtractMethods(Type type, MethodDefinitionCollection methods)
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
                    ExtractInstructions(method, methodDefinition, methodDefinition.Body.Instructions);
                }
            }
        }

        /// <summary>
        /// Extracts constructors and add them to method list for type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructors"></param>
        private void ExtractConstructors(Type type, ConstructorCollection constructors)
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
                    ExtractInstructions(method, constructor, constructor.Body.Instructions);
                }
            }
        }

        /// <summary>
        /// Extracts method calls by extracting instrunctions
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodDefinition"></param>
        /// <param name="instructions"></param>
        public void ExtractInstructions(Method method, MethodDefinition methodDefinition,
            InstructionCollection instructions)
        {
            foreach (Instruction instruction in instructions)
            {
                var meth = ExtractInstruction(instruction) as MethodDefinition;
                if (meth != null)
                {
                    var type = (from t in method.Type.Module.Types
                                where t.FullName == meth.DeclaringType.FullName
                                select t).SingleOrDefault();

                    method.TypeUses.Add(type);

                    var findTargetMethod = (from t in type.Methods
                                            where t.Name == FormatMethodName(meth)
                                            select t).SingleOrDefault();

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
        public object ExtractInstruction(Instruction instruction)
        {
            if (instruction.Operand == null)
                return null;
            
            var nextInstruction = instruction.Operand as Instruction;

            if (nextInstruction != null)
                return ExtractInstruction(nextInstruction);
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
                    builder.Append(((ParameterDefinition)enumerator.Current).ParameterType.FullName);
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
    }
}
