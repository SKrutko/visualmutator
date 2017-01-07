﻿namespace VisualMutator.Model.Tests.TestsTree
{
    #region

    using System;
    using System.Linq;
    using UsefulTools.CheckboxedTree;

    #endregion

    public abstract class TestTreeNode : CheckedNode, IExpandableNode
    {
        private TestNodeState _state;

        protected TestTreeNode(TestTreeNode parent, string name, bool hasChildren)
            : base(name, hasChildren)
        {
            Parent = parent;
        }

        public bool HasResults
        {
            get
            {
                return (State == TestNodeState.Failure || State == TestNodeState.Success
                    || State == TestNodeState.Inconclusive);
            }
        }

        public TestNodeState State
        {
            set
            {
                SetStatus(value, true, true);
            }
            get
            {
                return _state;
            }
        }

        public void SetStatus(TestNodeState value, bool updateChildren, bool updateParent)
        {
            if (_state != value)
            {
                _state = value;

                if (updateChildren && Children != null)
                {
                    if (!(value == TestNodeState.Inactive || value == TestNodeState.Running))
                    {
                        throw new InvalidOperationException("Tried to set invalid state: " + value);
                    }

                    foreach (var child in Children.Cast<TestTreeNode>())
                    {
                        child.SetStatus(value, updateChildren: true, updateParent: false);
                    }
                }

                if (updateParent && Parent != null)
                {
                    if (!(value == TestNodeState.Success || value == TestNodeState.Failure
                        || value == TestNodeState.Inconclusive))
                    {
                        throw new InvalidOperationException("Tried to set invalid state: " + value);
                    }

                    ((TestTreeNode)Parent).UpdateStateBasedOnChildren();
                }
                RaisePropertyChanged(() => State);
            }
        }

        private void UpdateStateBasedOnChildren()
        {
            var children = Children.Cast<TestTreeNode>().ToList();

            if (children.All(_ => _.HasResults))
            {
                TestNodeState state;
                if (children.Any(n => n.State == TestNodeState.Failure))
                {
                    state = TestNodeState.Failure;
                }
                else if (children.Any(n => n.State == TestNodeState.Inconclusive))
                {
                    state = TestNodeState.Inconclusive;
                }
                else
                {
                    state = TestNodeState.Success;
                }
                SetStatus(state, updateChildren: false, updateParent: true);
            }
        }

        public void Comm()
        {
            Name += "!";
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                SetAndRise(ref _isExpanded, value, () => IsExpanded);
            }
        }
    }
}