﻿namespace VisualMutator.Model.Tests.Services
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using log4net;
    using NUnit.Core;
    using Strilanc.Value;
    using TestsTree;
    using UsefulTools.Core;
    using UsefulTools.ExtensionMethods;

    #endregion
    public static class TestEx
    {
        public static IEnumerable<ITest> TestsEx(this ITest test)
        {
            return (test.Tests ?? new List<ITest>()).Cast<ITest>();
        }
    }
    public class NUnitTestService : ITestService
    {
        private readonly INUnitWrapper _nUnitWrapper;


        private ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public NUnitTestService(INUnitWrapper nUnitWrapper, IMessageService messageService)
        {
            _nUnitWrapper = nUnitWrapper;

        }

        public INUnitWrapper TestLoader
        {
            get
            {
                return _nUnitWrapper;
            }
        }


        public virtual May<TestsLoadContext> LoadTests(string assemblyPath)
        {
            var context = new TestsLoadContext();
            ITest testRoot = _nUnitWrapper.LoadTests(assemblyPath.InList());
            int testCount = testRoot.TestsEx().SelectMany(n => n.TestsEx()).Count();
            if (testCount == 0)
            {
                return May.NoValue;
            }
            BuildTestTree(testRoot, context);
            UnloadTests();
            return context;
        }

        public static IList<T> ConvertToListOf<T>(IList iList)
        {
            IList<T> result = new List<T>();
            if (iList != null)
            {
                foreach (T value in iList)
                {
                    result.Add(value);
                }
            }

            return result;
        }
        public virtual Task RunTests(TestsRunContext testsRunContext)
        {
//            Task<TestResult> runTests = TestLoader.RunTests();
//
//            return runTests.ContinueWith(testResult =>
//            {
//                var list = new List<TmpTestNodeMethod>();
//                if(testResult.Exception != null)
//                {
//                    _log.Error(testResult.Exception);
//                }
//                else
//                {
//                    _subscription = TestLoader.TestFinished.Subscribe(result =>
//                    {
//                        TmpTestNodeMethod node = new TmpTestNodeMethod(result.Test.TestName.FullName);
//                        //TestNodeMethod node = context.TestMap[result.Test.TestName.FullName];
//                        node.State = result.IsSuccess ? TestNodeState.Success : TestNodeState.Failure;
//                        node.Message = result.Message + "\n" + result.StackTrace;
//                        list.Add(node);
//                    }, () =>
//                    {
//
//                        
//                    });
//                    _subscription.Dispose();
//                }
//                return list;
//            });
            throw new NotImplementedException();
        }

        public void UnloadTests()
        {
            
            _nUnitWrapper.UnloadProject();
        }

        private void BuildTestTree(ITest test, TestsLoadContext context)
        {
            IEnumerable<ITest> classes = GetTestClasses(test).ToList();

            foreach (ITest testClass in classes.Where(c => c.Tests != null && c.Tests.Count != 0))
            {
                
                var c = new TestNodeClass(testClass.TestName.Name)
                {
                    Namespace = testClass.Parent.TestName.FullName,
                    FullName = testClass.TestName.FullName,

                };

                foreach (ITest testMethod in testClass.Tests.Cast<ITest>())
                {
                    if (_nUnitWrapper.NameFilter == null || _nUnitWrapper.NameFilter.Match(testMethod))
                    {
                        string testName = testMethod.TestName.FullName;
                        //if(!context.TestMap.ContainsKey(testName))
                      //  {
                        var nodeMethod = new TestNodeMethod(c, testName)
                            {
                                TestId = new NUnitTestId(testMethod.TestName),
                                Identifier = CreateIdentifier(testMethod),
                            };
                        c.Children.Add(nodeMethod);
                        _log.Debug("Adding test: " + testName);
                       // context.TestMap.Add(testName, nodeMethod);
                       // }
                      //  else
                      //  {
                     //       _log.Debug("Already exists test: " + testName);
                     //       //TODO: handle he case where parametrized test method may be present duplicated.
                     //   }
                    }
                }
                if(c.Children.Any())
                {
                    context.ClassNodes.Add(c);
                }
            }
        }

        private MethodIdentifier CreateIdentifier(ITest testMethod)
        {
            return new MethodIdentifier(testMethod.TestName.FullName + "()");
        }

        private IEnumerable<ITest> GetTestClasses(ITest test)
        {
           //TODO: return new[] { test }.SelectManyRecursive(t => t.Tests != null ? t.Tests.Cast<ITest>() : new ITest[0])
           //     .Where(t => t.TestType == "TestFixture");
            var list = new List<ITest>();
            GetTestClassesInternal(list, test);
            return list;
        }

        private void GetTestClassesInternal(List<ITest> list, ITest test)
        {
            var tests = test.Tests ?? new ITest[0];
            if (test.TestType == "TestFixture")
            {
                list.Add(test);
            }
            else
            {
                foreach (var t in tests.Cast<ITest>())
                {
                    GetTestClassesInternal(list, t);
                }
            }
        }


        public void Cancel()
        {
            _nUnitWrapper.Cancel();
        }






    }
}