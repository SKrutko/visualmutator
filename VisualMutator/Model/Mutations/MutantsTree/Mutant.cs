namespace VisualMutator.Model.Mutations.MutantsTree
{
    #region

    using System.Collections.Generic;
    using Tests;
    using Tests.Services;
    using UsefulTools.ExtensionMethods;
    using UsefulTools.Switches;

    #endregion

    public class Mutant : MutationNode
    {
        public Mutant(string id, MutantGroup parent, MutationTarget mutationTarget)
            : base("Mutant", false)
        {
            _id = id;
            _mutationTarget = mutationTarget;

            _mutantTestSession = new MutantTestSession();

            Parent = parent;
            UpdateDisplayedText();
        }

        public Mutant(string id, MutationTarget mutationTarget)
           : base("Mutant", false)
        {
            _id = id;
            _mutationTarget = mutationTarget;
            _mutantTestSession = new MutantTestSession();
            UpdateDisplayedText();
        }

        private /*readonly*/ string _id;

        private /*readonly*/ MutationTarget _mutationTarget;

        public List<MutationTarget> _mutationTargets = new List<MutationTarget>();

        public int _nrTimesWasAdded = 0;

        public MutationTarget MutationTarget
        {
            get
            {
                return _mutationTarget;
            }
            set
            {
                _mutationTarget = value;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private int _numberOfFailedTests;

        public int NumberOfFailedTests
        {
            get
            {
                return _numberOfFailedTests;
            }
            set
            {
                SetAndRise(ref _numberOfFailedTests, value, () => NumberOfFailedTests);
            }
        }

        private string _displayedText;

        public string DisplayedText
        {
            get
            {
                return _displayedText;
            }
            set
            {
                SetAndRise(ref _displayedText, value, () => DisplayedText);
            }
        }

        public override string ToString()
        {
            return MutationTarget.Variant.Signature;
        }

        public string Description
        {
            get
            {
                return MutationTarget.Variant.Signature;
            }
        }

        private readonly MutantTestSession _mutantTestSession;

        public MutantTestSession MutantTestSession
        {
            get
            {
                return _mutantTestSession;
            }
        }

        public MutantKilledSubstate KilledSubstate { get; set; }

        private bool _isEquivalent;

        public bool IsEquivalent
        {
            get
            {
                return _isEquivalent;
            }
            set
            {
                SetAndRise(ref _isEquivalent, value, () => IsEquivalent);
                UpdateDisplayedText();
            }
        }

        public List<ITestsRunContext> TestRunContexts
        {
            get;
            set;
        }

        public long CreationTimeMilis { get; set; }

        protected override void SetState(MutantResultState value, bool updateChildren, bool updateParent)
        {
            base.SetState(value, updateChildren, updateParent);
            UpdateDisplayedText();
        }

        public void UpdateDisplayedText()
        {
            string stateText =
             Switch.Into<string>().From(State)
             .Case(MutantResultState.Untested, "Untested")
             .Case(MutantResultState.Creating, "Creating mutant...")
             .Case(MutantResultState.Writing, "Writing mutant...")
             .Case(MutantResultState.Tested, "Executing tests...")
             .Case(MutantResultState.Killed, () =>
             {
                 return Switch.Into<string>().From(KilledSubstate)
                     .Case(MutantKilledSubstate.Normal, () => "Killed by {0} tests".Formatted(NumberOfFailedTests))
                     .Case(MutantKilledSubstate.Inconclusive, () => "Killed by {0} tests".Formatted(NumberOfFailedTests))
                     .Case(MutantKilledSubstate.Cancelled, () => "Timed out")
                     .GetResult();
             })
             .Case(MutantResultState.Live, "Live")
             .Case(MutantResultState.Error, () => MutantTestSession.ErrorDescription)
             .GetResult();

            if (IsEquivalent)
            {
                stateText = "Equivalent";
            }

            DisplayedText = "{0} - {1} - {2}".Formatted(Id, MutationTarget.Variant.Signature, stateText);
        }
    }

    public enum MutantKilledSubstate
    {
        Normal,
        Inconclusive,
        Cancelled,
    }
}