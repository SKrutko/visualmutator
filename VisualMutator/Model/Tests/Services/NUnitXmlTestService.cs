namespace VisualMutator.Model.Tests.Services
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CoverageFinder;
    using log4net;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Internal;
    using Strilanc.Value;
    using TestsTree;
    using UsefulTools.Core;
    using UsefulTools.DependencyInjection;
    using UsefulTools.ExtensionMethods;

    #endregion

    public class NUnitXmlTestService : ITestsService
    {
        private readonly IFactory<NUnitTestsRunContext> _testsRunContextFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly CommonServices _svc;

        private const string FrameworkName = "NUnit";

        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _nunitConsolePath;
        private readonly INUnitWrapper _nUnitWrapper;

        public string NunitConsolePath
        {
            get { return _nunitConsolePath; }
        }

        public NUnitXmlTestService(
            IFactory<NUnitTestsRunContext> testsRunContextFactory,
            ISettingsManager settingsManager,
            INUnitWrapper nUnitWrapper,
            CommonServices svc)

        {
            _testsRunContextFactory = testsRunContextFactory;
            _settingsManager = settingsManager;
            _nUnitWrapper = nUnitWrapper;
            _svc = svc;

            _nunitConsolePath = FindConsolePath();
            _log.Info("Set NUnit Console path: " + _nunitConsolePath);
        }

        public void Cancel()
        {
        }

        public ITestsRunContext CreateRunContext(TestsLoadContext loadContext, string mutatedPath)
        {
            var selector = new TestsSelector(loadContext.Namespaces);
            return _testsRunContextFactory.CreateWithParams(_nunitConsolePath, mutatedPath, selector);
        }

        public string FindConsolePath()
        {
            var nUnitDirPath = _settingsManager["NUnitConsoleDirPath"];
            var nUnitConsolePath = Path.Combine(nUnitDirPath, "nunit3-console.exe");

            if (!_svc.FileSystem.File.Exists(nUnitConsolePath))
            {
                throw new FileNotFoundException(nUnitConsolePath + " file was found.");
            }
            return nUnitConsolePath;
        }

        public string FrameWorkName { get { return FrameworkName; } }

        public virtual May<TestsLoadContext> LoadTests(string assemblyPath)
        {
            try
            {
                var testRoot = _nUnitWrapper.LoadTests(assemblyPath.InList());

                int testCount = testRoot.Values.Count();

                if (testCount == 0)
                {
                    return May.NoValue;
                }

                var classNodes = BuildTestTree(testRoot);

                string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath); //TODO

                var context = new TestsLoadContext(FrameworkName, assemblyName, classNodes.ToList());

                UnloadTests();

                return context;
            }
            catch (Exception e)
            {
                _log.Error("Excception While loading tests: ", e);
                throw;
            }
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

        public void UnloadTests()
        {
            // _nUnitWrapper.UnloadProject();
        }

        private IEnumerable<TestNodeClass> BuildTestTree(IDictionary<string, List<string>> testFixtures)
        {
            foreach (var testFixture in testFixtures)
            {
                var c = new TestNodeClass(testFixture.Key.Split('.').Last())
                {
                    Namespace = testFixture.Key.Substring(0, testFixture.Key.LastIndexOf('.')),
                };

                foreach (var testCase in testFixture.Value)
                {
                    string testName = testCase;

                    var nodeMethod = new TestNodeMethod(c, testName)
                    {
                        TestId = new NUnitTestId(testCase),
                        Identifier = CreateIdentifier(testCase),
                    };

                    c.Children.Add(nodeMethod);
                }
                if (c.Children.Any())
                {
                    yield return c;
                }
            }
        }

        private MethodIdentifier CreateIdentifier(string testMethodName)
        {
            return new MethodIdentifier(testMethodName + "()");
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
            if (((Test)test).TestType == "TestFixture")
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
    }
}