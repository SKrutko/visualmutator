﻿namespace VisualMutator.Model.Mutations
{
    using System.Collections.Generic;
    using Microsoft.Cci;
    using MutantsTree;

    public class MutationResult
    {
        private readonly Mutant _mutant;

        public Mutant Mutant
        {
            get { return _mutant; }
        }

        public MutationResult(Mutant mutant, ICciModuleSource mutatedModules, List<CciModuleSource> old, IMethodDefinition methodMutated, List<IMethodDefinition> additionalMethodsMutated = null)
        {
            _mutant = mutant;
            MutatedModules = mutatedModules;
            Old = old;
            MethodMutated = methodMutated;
            AdditionalMethodsMutated = additionalMethodsMutated;
        }

        public ICciModuleSource MutatedModules { get; private set; }
        public List<CciModuleSource> Old { get; set; }
        public IMethodDefinition MethodMutated { get; set; }
        public List<IMethodDefinition> AdditionalMethodsMutated { get; set; }
    }
}