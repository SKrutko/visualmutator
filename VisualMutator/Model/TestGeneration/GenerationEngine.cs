﻿namespace VisualMutator.Model.TestGeneration
{
    #region

    using System;
    using System.IO;
    using System.Linq;
    using Model.Mutations.MutantsTree;
    using Roslyn.Compilers;
    using Roslyn.Compilers.CSharp;
    using StoringMutants;

    #endregion

    public class GenerationEngine
    {
        private string execTemplate =
@" using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VisualMutator_generated
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}";

        public void Process(IModuleSource original, Mutant mutant)
        {
            var tree = SyntaxTree.ParseText(execTemplate);

            CompilationUnitSyntax root = tree.GetRoot();
            var firstParameters =
                from methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Main"
                select methodDeclaration.ParameterList.Parameters.First();

            //  Syntax.MethodDeclaration(Syntax.)

            var comp = Compilation.Create("MyCompilation",
                new CompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(tree)
                .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location));

            var outputFileName = Path.Combine(Path.GetTempPath(), "MyCompilation.lib");
            var ilStream = new FileStream(outputFileName, FileMode.OpenOrCreate);

            var result = comp.Emit(ilStream);
            ilStream.Close();
            if (!result.Success)
            {
                var aggregate = result.Diagnostics.Select(a => a.Info.GetMessage()).Aggregate((a, b) => a + "\n" + b);
                throw new InvalidProgramException(aggregate);
            }

            /*  Compilation compilation = Compilation.Create("")
                  .AddReferences(new MetadataFileReference(typeof(object).Assembly.Location))
                  .AddSyntaxTrees()

              var method = ActeeFinder
              valueSupplier.supplyValues(method);*/
        }
    }
}