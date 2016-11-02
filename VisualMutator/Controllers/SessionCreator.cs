﻿namespace VisualMutator.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Infrastructure;
    using Model;
    using Model.CoverageFinder;
    using Model.Mutations.Operators;
    using Model.Mutations.Types;
    using Model.StoringMutants;
    using Model.Tests.TestsTree;
    using UsefulTools.CheckboxedTree;
    using UsefulTools.Core;
    using UsefulTools.ExtensionMethods;

    public class SessionCreator
    {
        private readonly ITypesManager _typesManager;
        private readonly IOperatorsManager _operatorsManager;
        private readonly CommonServices _svc;
        private readonly IMessageService _reporting;

        public SessionCreator(
           ITypesManager typesManager,
            IOperatorsManager operatorsManager,
            CommonServices svc,
            IMessageService reporting)
        {
            _typesManager = typesManager;
            _operatorsManager = operatorsManager;
            _svc = svc;
            _reporting = reporting;
            Events = new Subject<object>();
        }

        public Subject<object> Events { get; set; }

       

        public async Task<OperatorPackagesRoot> GetOperators()
        {
            try
            {
                OperatorPackagesRoot root = await _operatorsManager.GetOperators();
                return root;
            }
            catch (Exception e)
            {
                _reporting.ShowError(e.ToString());
                throw;
            }
            //Events.OnNext(root);
            
        }

       

       
        public async Task<List<AssemblyNode>> BuildAssemblyTree(Task<List<CciModuleSource>> assembliesTask,
            bool constrainedMutation, ICodePartsMatcher matcher)
        {
            var modules = await assembliesTask;
            var assemblies = _typesManager.CreateNodesFromAssemblies(modules, matcher)
                .Where(a => a.Children.Count > 0).ToList();

            if (constrainedMutation)
            {
                var root = new CheckedNode("");
                root.Children.AddRange(assemblies);
                TreeUtils.ExpandLoneNodes(root);
            }
            if(assemblies.Count == 0)
            {
                throw new InvalidOperationException(UserMessages.ErrorNoFilesToMutate());
            }
            //  _reporting.LogError(UserMessages.ErrorNoFilesToMutate());
            return assemblies;
            //Events.OnNext(assemblies);
        }

      


    }
}