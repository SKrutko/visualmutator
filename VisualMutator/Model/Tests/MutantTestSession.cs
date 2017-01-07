namespace VisualMutator.Model.Tests
{
    #region

    using System;
    using System.Collections.Generic;
    using UsefulTools.Core;

    #endregion

    public class MutantTestSession : ModelElement
    {
        public MutantTestSession()
        {
        }

        public string ErrorMessage
        {
            get;
            set;
        }

        public string ErrorDescription
        {
            get;
            set;
        }

        public long TestingTimeMiliseconds
        {
            get;
            set;
        }

        public IList<string> AssembliesWithTests { get; set; }

        private bool _isComplete;

        public bool IsComplete
        {
            get
            {
                return _isComplete;
            }
            set
            {
                SetAndRise(ref _isComplete, value, () => IsComplete);
            }
        }

        public Exception Exception { get; set; }
        public DateTime TestingEnd { get; set; }
    }
}