﻿namespace VisualMutator.Tests.UnitTesting
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Model.Tests;
    using Model.Tests.Services;
    using Moq;
    using NUnit.Core;
    using NUnit.Framework;
    using Operators;
    using UsefulTools.Core;
    using UsefulTools.Wpf;

    #endregion

    [TestFixture]
    public class NUnitServiceTests
    {
        private TestResult MockTestResult(string name, bool success)
        {
            var m = new Mock<ITest>();

            var tn = new TestName
                {
                    FullName = name,
                    Name = name
                };

            m.Setup(_ => _.TestName).Returns(tn);
            //   m.Object.

            var result = new TestResult(m.Object);

            if (success)
            {
                result.Success();
            }
            else
            {
                result.Error(new Exception("test exception"));
            }
            return result;
            /*
            var m = Mock.Of<ITest>(t =>
                t.TestName.UniqueName == name &&
                t.Tests.Add()

                );*/
        }

        [Test]
        public void Integration()
        {
            var m = new Mock<IMessageService>();
            var wrapper = new NUnitWrapper(m.Object);
            var service = new NUnitTestService(wrapper, m.Object);
            var session = new MutantTestSession();
            service.LoadTests(new List<string> {MutationTestsHelper.DsaPath, MutationTestsHelper.DsaPath},
                session);

            service.RunTests(session);

            Thread.Sleep(15000);
            Console.WriteLine(":session.TestMap.Count : " + session.TestMap.Count);
            
                
        }

        [Test]
        public void LoadingTests()
        {
            string nunitConsolePath =@"C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console-x86.exe";//"nunit-console",
            string outputFile = "nunit-results.xml";
            string arg = "\"" + MutationTestsHelper.DsaTestsPath + "\" /xml \"" + outputFile + "\" /nologo";
            

            var startInfo = new ProcessStartInfo
            {
                Arguments = arg,
                CreateNoWindow = true,
                ErrorDialog = true,
                RedirectStandardOutput = false,
                FileName = nunitConsolePath,
                UseShellExecute = false
            };

            Process proc = Process.Start(startInfo);
            bool res = proc.WaitForExit(1000 * 60);

            res = (res && proc.ExitCode >= 0);
        }

        /*
        [Test]
        public void LoadingTests()
        {
            List<ITest> testClasses;
            var mock = TestWrapperMocking.MockNUnitWrapperForLoad(out testClasses);
            var clas = TestWrapperMocking.MockTestFixture("Class1", "ns1");
            clas.Tests.Add(TestWrapperMocking.MockTest("Test1", clas));
            clas.Tests.Add(TestWrapperMocking.MockTest("Test2", clas));
            clas.Tests.Add(TestWrapperMocking.MockTest("Test3", clas));
            testClasses.Add(clas);

            var ser = new NUnitTestService(mock.Object, new Mock<IMessageService>().Object);


            IEnumerable<TestNodeClass> testNodeClasses = ser.LoadTests(new Collection<string> { "a", "b" });


            var testFixture = testNodeClasses.Single();

            testFixture.Children.Count.ShouldEqual(3);
            testFixture.Children.ElementAt(2).Name.ShouldEqual("Test3");
        }


        [Test]
        public void RunningTests()
        {
            List<ITest> testClasses;
            var wrapperMock = TestWrapperMocking.MockNUnitWrapperForLoad(out testClasses);

            var clas = TestWrapperMocking.MockTestFixture("Class1", "ns1");
            clas.Tests.Add(TestWrapperMocking.MockTest("Test1", clas));
            clas.Tests.Add(TestWrapperMocking.MockTest("Test2", clas));
            clas.Tests.Add(TestWrapperMocking.MockTest("Test3", clas));
            testClasses.Add(clas);


            var list = new List<TestResult>
            {
                MockTestResult("Test1", true),
                MockTestResult("Test2", true),
                MockTestResult("Test3", false),
            };

            wrapperMock.Setup(_ => _.TestFinished).Returns(list.ToObservable());
            wrapperMock.Setup(_ => _.RunFinished).Returns(new List<TestResult> { new TestResult(new TestName()) }
                .ToObservable());


            
            var ser = new NUnitTestService(wrapperMock.Object, new Mock<IMessageService>().Object);
            var classes = ser.LoadTests(new Collection<string> { "a", "b" });


            ser.RunTests();

            ser.TestMap[list.ElementAt(0).Test.TestName.UniqueName].State.ShouldEqual(TestNodeState.Success);
            ser.TestMap[list.ElementAt(1).Test.TestName.UniqueName].State.ShouldEqual(TestNodeState.Success);
            ser.TestMap[list.ElementAt(2).Test.TestName.UniqueName].State.ShouldEqual(TestNodeState.Failure);

           classes.Single().State.ShouldEqual(TestNodeState.Failure);
        }

        */
    }
}