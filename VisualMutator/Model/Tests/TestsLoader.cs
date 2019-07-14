namespace VisualMutator.Model.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using log4net;
    using Services;
    using Strilanc.Value;
    using TestsTree;

    public class TestsLoader
    {
        private readonly TestServiceManager _testServiceManager;
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TestsLoader(TestServiceManager testServiceManager)
        {
            _testServiceManager = testServiceManager;
        }

        public async Task<TestsRootNode> LoadTests(IList<string> assembliesPaths)
        {
            _log.Info("Loading tests from: " + string.Join(",", assembliesPaths));
            var testsRootNode = new TestsRootNode();

            await GetTestAssemblyNodes(assembliesPaths, testsRootNode);

            return testsRootNode;
        }

        private async Task GetTestAssemblyNodes(IList<string> assembliesPaths, TestsRootNode testsRootNode)
        {
            await LoadFor(assembliesPaths, testsRootNode);
        }

        private async Task LoadFor(IEnumerable<string> path1, TestsRootNode testsRootNode)
        {
            foreach (string path in path1)
            {
                var contexts = await _testServiceManager.LoadTests(path);

                foreach (var testContext in contexts.OrderBy(p => p.AssemblyName()))
                {
                    var testNodeAssembly = new TestNodeAssembly(testsRootNode, testContext.AssemblyName());

                    testNodeAssembly.TestsLoadContexts = new List<TestsLoadContext> { testContext };

                    var allClassNodes = testContext.ClassNodes;

                    IEnumerable<TestNodeNamespace> testNamespaces = TestsLoadContext.GroupTestClasses(allClassNodes.ToList(), testNodeAssembly);

                    testNodeAssembly.Children.AddRange(testNamespaces);

                    testsRootNode.Children.Add(testNodeAssembly);
                }
            }
        }
    }
}