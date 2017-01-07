﻿namespace VisualMutator.Tests
{
    #region

    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class SolutionTypesTests
    {
     /*   private static List<TypeDefinition> CreateTypeDefinitions()
        {
            var a = TypeAttributes.Public;
            var list = new List<TypeDefinition>
                {
                    new TypeDefinition("One.Two.Three.Four.Five", "Type1", a),
                    new TypeDefinition("One.Two.Three.Four.Five", "Type2", a),
                    new TypeDefinition("One.Two.Three", "Type3", a),
                    new TypeDefinition("One", "Type4", a),
                    new TypeDefinition("One", "Type5", a),
                    new TypeDefinition("One.Two", "Typy6", a),
                    new TypeDefinition("One.Two.Three.Four", "Type7", a),
                    new TypeDefinition("One.Two.Three.Four", "Type8", a),
                    new TypeDefinition("One.Two.Three.Four", "Type9", a),
                    new TypeDefinition("One.XXX.Three.Four", "Type10", a),
                    new TypeDefinition("One.XXX.Three.Four", "Type11", a),
                    new TypeDefinition("One.Two.YYY.Four", "Type12", a),
                    new TypeDefinition("One.ZZZ.YYY.Four", "Type13", a),
                    new TypeDefinition("One.ZZZ.YYY.Four", "Type14", a),
                    new TypeDefinition("Other", "Type15", a),
                };
            return list;
        }


        [Test]
        public void ExtractNextNamespacePart_Test()
        {
            var manager = new SolutionTypesManager(null, null);

            manager.ExtractNextNamespacePart("One.Two.Three.Four.Five", "").ShouldEqual("One");
            manager.ExtractNextNamespacePart("One.Two.Three", "").ShouldEqual("One");
            manager.ExtractNextNamespacePart("One", "").ShouldEqual("One");

            manager.ExtractNextNamespacePart("One.Two.Three.Four.Five", "One.Two").ShouldEqual("Three");
            manager.ExtractNextNamespacePart("One.Two.Three", "One.Two").ShouldEqual("Three");
        }

        [Test]
        public void GetIncludedTypes_Test()
        {
            List<TypeDefinition> list = CreateTypeDefinitions();

            string path = @"C:\TestAssembly.dll";

            var mock = new Mock<ICommonCompilerInfra>();

            //    var assembly = CecilUtils.CreateAssembly("TestAssembly", list);
            //     mock.Setup(_ => _.ReadAssembly(path)).Returns(assembly);


            var m = new Mock<IHostEnviromentConnection>();
            m.Setup(_ => _.GetProjectAssemblyPaths()).Returns(new[] {path.ToFilePathAbs()});

            var manager = new SolutionTypesManager(mock.Object, m.Object);

            // Act
            IList<AssemblyNode> assemblies = manager.GetTypesFromAssemblies();

            // Assert
            //    CollectionAssert.AreEquivalent(manager.GetIncludedTypes(assemblies), list);
        }

        [Test]
        public void GetTypesFromAssemblies_Creates_Valid_Tree()
        {
            List<TypeDefinition> list = CreateTypeDefinitions();

            string path = @"C:\TestAssembly.dll";

            var mock = new Mock<ICommonCompilerInfra>();

            //  var assembly = CecilUtils.CreateAssembly("TestAssembly", list);
            //   mock.Setup(_ => _.ReadAssembly(path)).Returns(assembly);


            var m = new Mock<IHostEnviromentConnection>();
            m.Setup(_ => _.GetProjectAssemblyPaths()).Returns(new[] {path.ToFilePathAbs()});

            var manager = new SolutionTypesManager(mock.Object, m.Object);

            // Act
            IList<AssemblyNode> assemblies = manager.GetTypesFromAssemblies();

            // Assert
            //  assemblies.Select(t => t.AssemblyDefinition).Single().ShouldEqual(assembly);

            assemblies.Single().Children.Count.ShouldEqual(2);

            CheckedNode one = assemblies.Single().Children.First();
            one.Children.Count.ShouldEqual(5);
            CheckedNode two = one.Children.Single(_ => _.Name == "Two");
            two.Children.Count.ShouldEqual(3);
            CheckedNode merged = one.Children.Single(_ => _.Name == "ZZZ.YYY.Four");
            merged.Children.Count.ShouldEqual(2);
        }*/
    }
}