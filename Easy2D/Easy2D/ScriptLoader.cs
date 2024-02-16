using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace Easy2D
{
    public class ScriptLoader
    {
        public static object LoadAndInstantiate(string filePath, string className, string namespaceName)
        {

            if (!File.Exists(filePath))
                return null;

            string code = File.ReadAllText(filePath);

            CompilerParameters compilerParams = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            // Füge die Assembly deines Projekts hinzu
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            compilerParams.ReferencedAssemblies.Add(assemblyPath);

            // Füge weitere Assemblys hinzu, falls erforderlich
            compilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            compilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            compilerParams.ReferencedAssemblies.Add("System.Linq.dll");

            CompilerResults results = CodeDomProvider.CreateProvider("CSharp")
                .CompileAssemblyFromSource(compilerParams, code);

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
                return null;
            }
            else
            {

                Assembly assembly = results.CompiledAssembly;
                Type[] types = assembly.GetExportedTypes();
                foreach (Type type in types)
                {

                    object instance = Activator.CreateInstance(type);
                    return instance;

                }

                Console.WriteLine($"Keine Klasse im Namespace {namespaceName} gefunden.");
                return null;

                /*
                Assembly assembly = results.CompiledAssembly;
                Type type = assembly.GetType($"{namespaceName}.{className}");
                if (type != null)
                {
                    object instance = Activator.CreateInstance(type);
                    return instance;
                }
                else
                {
                    Console.WriteLine($"Klasse {className} im Namespace {namespaceName} nicht gefunden.");
                    return null;
                }
                */

            }
        }
    }

}
