namespace VisualMutator.Model.Tests
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using log4net;
    using Microsoft.Cci;
    using Services;
    using Strilanc.Value;
    using UsefulTools.Core;
    using UsefulTools.DependencyInjection;

    public class MsTestService : ITestsService
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISettingsManager _settingsManager;
        private readonly IFactory<MsTestRunContext> _contextFactory;

        public MsTestService(
            ISettingsManager settingsManager,
            IFactory<MsTestRunContext> contextFactory)
        {
            _settingsManager = settingsManager;
            _contextFactory = contextFactory;
            MsTestConsolePath = GetMstestExeDir();
        }

        private string GetMstestExeDir()
        {
            var vsProcessExeFileLocation = Process.GetCurrentProcess().MainModule.FileName.ToUpper();

            if (vsProcessExeFileLocation.Contains("MICROSOFT VISUAL STUDIO") && vsProcessExeFileLocation.Contains("COMMON7"))
            {
                vsProcessExeFileLocation = vsProcessExeFileLocation.Substring(0, vsProcessExeFileLocation.IndexOf("IDE"));
                return $"{vsProcessExeFileLocation}IDE\\MSTest.exe";
            }
            else
                throw new FileNotFoundException("Could not find MSTest.exe");
        }

        public string FrameWorkName { get { return "MsTest"; } }
        public string MsTestConsolePath { get; private set; }

        public May<TestsLoadContext> LoadTests(string assemblyPath)
        {
            _log.Info("MsTest loading tests...");
            var cci = new CciModuleSource(assemblyPath);

            var visitor = new MsTestTestsVisitor();
            var traverser = new CodeTraverser
            {
                PreorderVisitor = visitor
            };

            traverser.Traverse(cci.Module.Module);

            var classes = visitor.Classes.Where(c => c.Children.Count != 0).ToList();
            if (classes.Count != 0)
            {
                _log.Info("Tests loaded (" + classes.Count + " classes).");
                string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath); //TODO
                return new May<TestsLoadContext>(new TestsLoadContext(FrameWorkName, assemblyName, classes));
            }
            else
            {
                _log.Info("No tests found.");
                return May.NoValue;
            }
        }

        public void Cancel()
        {
        }

        public ITestsRunContext CreateRunContext(TestsLoadContext loadContext, string mutatedPath)
        {
            var tesele = new TestsSelector(loadContext.Namespaces);
            return _contextFactory.CreateWithParams(MsTestConsolePath, mutatedPath, tesele);
        }
    }
}