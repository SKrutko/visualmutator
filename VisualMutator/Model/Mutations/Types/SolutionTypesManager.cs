﻿namespace VisualMutator.Model.Mutations.Types
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CoverageFinder;
    using Extensibility;
    using log4net;
    using Microsoft.Cci;
    using MutantsTree;
    using UsefulTools.CheckboxedTree;
    using UsefulTools.ExtensionMethods;
    using UsefulTools.Paths;
    using MethodIdentifier = Extensibility.MethodIdentifier;

    #endregion

    public interface ITypesManager
    {
        bool IsAssemblyLoadError { get; set; }

        IList<AssemblyNode> CreateNodesFromAssemblies(List<CciModuleSource> modules,
            ICodePartsMatcher constraints);

        MutationFilter CreateFilterBasedOnSelection(ICollection<AssemblyNode> assemblies);
    }

    public static class Helpers
    {
        public static string GetTypeFullName(this INamespaceTypeReference t)
        {
            var nsPart = TypeHelper.GetNamespaceName(t.ContainingUnitNamespace, NameFormattingOptions.None);
            var typePart = t.Name.Value + (t.MangleName ? "`" + t.GenericParameterCount : "");
            return nsPart + "." + typePart;
        }
    }

    public class SolutionTypesManager : ITypesManager
    {
        public bool IsAssemblyLoadError { get; set; }

        public SolutionTypesManager()
        {
        }

        public MutationFilter CreateFilterBasedOnSelection(ICollection<AssemblyNode> assemblies)
        {
            var methods = assemblies
                .SelectManyRecursive<CheckedNode>(node => node.Children, node => node.IsIncluded ?? true, leafsOnly: true)
                .OfType<MethodNode>().Select(type => type.MethodDefinition).ToList();
            return new MutationFilter(new List<TypeIdentifier>(), methods.Select(m => new MethodIdentifier(m)).ToList());
        }

        public IList<AssemblyNode> CreateNodesFromAssemblies(List<CciModuleSource> modules, ICodePartsMatcher constraints)
        {
            var matcher = constraints.Join(new ProperlyNamedMatcher());

            List<AssemblyNode> assemblyNodes = modules.OrderBy(p => p.Module.Name).Select(m => CreateAssemblyNode(m.Module, matcher)).ToList();
            var root = new RootNode();
            root.Children.AddRange(assemblyNodes);
            root.IsIncluded = true;
            // root.Children[1].IsIncluded = false; //experiment toDel

            return assemblyNodes;
        }

        public AssemblyNode CreateAssemblyNode(IModuleInfo module,
            ICodePartsMatcher matcher)
        {
            var assemblyNode = new AssemblyNode(module.Name);
            assemblyNode.AssemblyPath = module.Module.Location.ToFilePathAbs();
            System.Action<CheckedNode, ICollection<INamedTypeDefinition>> typeNodeCreator = (parent, leafTypes) =>
            {
            foreach (INamedTypeDefinition typeDefinition in (To sie rozwala) leafTypes.OrderBy(p => p.Name))
                {
                // _log.Debug("For types: matching: ");
                if (matcher.Matches(typeDefinition))
                {
                    var type = new TypeNode(parent, typeDefinition.Name.Value);
                    foreach (var method in typeDefinition.Methods.OrderBy(p => p.Name))
                    {
                        if (matcher.Matches(method))
                            type.Children.Add(new MethodNode(type, method.Name.Value, method, false));
                    }

                    parent.Children.Add(type);
                }
            }
        };

        private Func<INamedTypeDefinition, string> namespaceExtractor = typeDef =>
             TypeHelper.GetDefiningNamespace(typeDef).Name.Value;

        NamespaceGrouper<INamespaceTypeDefinition, CheckedNode>.
            GroupTypes(assemblyNode,
                    namespaceExtractor,

                    (parent, name) => private newTypeNamespaceNode(parent, name),

                    typeNodeCreator,

                        module.Module.GetAllTypes().privateToList());

            //remove empty amespaces.
            //TODO to refactor...
            private List<TypeNamespaceNode> checkedNodes = assemblyNode.Children.OfType<TypeNamespaceNode>().ToList();

            foreach (private TypeNamespaceNode node in checkedNodes)
            {
private                RemoveFromParentIfEmpty(node);
    }

            return assemblyNode;
        }

public void RemoveFromParentIfEmpty(MutationNode node)
{
    var children = node.Children.ToList();
    while (children.OfType<TypeNamespaceNode>().Any())
    {
        TypeNamespaceNode typeNamespaceNode = node.Children.OfType<TypeNamespaceNode>().First();
        RemoveFromParentIfEmpty(typeNamespaceNode);
        children.Remove(typeNamespaceNode);
    }
    while (children.OfType<TypeNode>().Any())
    {
        TypeNode typeNamespaceNode = children.OfType<TypeNode>().First();
        RemoveFromParentIfEmpty(typeNamespaceNode);
        children.Remove(typeNamespaceNode);
    }
    if (!node.Children.Any())
    {
        node.Parent.Children.Remove(node);
        node.Parent = null;
    }
}

public class ProperlyNamedMatcher : CodePartsMatcher
{
    public override bool Matches(IMethodReference method)
    {
        return true;
    }

    public override bool Matches(ITypeReference typeReference)
    {
        INamedTypeReference named = typeReference as INamedTypeReference;
        return named != null
               && !named.Name.Value.StartsWith("<")
               && !named.Name.Value.Contains("=");
    }
}
    }
}