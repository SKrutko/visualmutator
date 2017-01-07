﻿namespace VisualMutator.Model
{
    #region

    using System.Collections.Generic;
    using Mutations;
    using Mutations.Types;

    #endregion

    public class MutationTestingSession
    {
        public MutationTestingSession()
        {
            MutantsGrouped = new List<AssemblyNode>();
            Filter = MutationFilter.AllowAll();
            Choices = new MutationSessionChoices();
        }

        public IList<AssemblyNode> MutantsGrouped { get; set; }
        public double MutationScore { get; set; }
        public MutationFilter Filter { get; set; }
        public MutationSessionChoices Choices { get; set; }
    }
}