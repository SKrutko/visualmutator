﻿namespace VisualMutator.Tests.UnitTesting
{
    #region

    using NUnit.Framework.Interfaces;

    #endregion

    public static class TestWrapperMocking
    {
        public static ITest MockTest(string name, ITest parentFixture)
        {
            return null;
            //var m = new Mock<ITest>();

            //var tn = new TestName
            //    {
            //        FullName = name,
            //        Name = name
            //    };
            //var ptn = new TestName
            //    {
            //        FullName = parentFixture.TestName.FullName,
            //        Name = parentFixture.TestName.Name,
            //    };

            //m.Setup(_ => _.TestName).Returns(tn);
            //m.Setup(_ => _.Parent.TestName).Returns(ptn);
            //m.Setup(_ => _.TestType).Returns("Test");
            //return m.Object;
        }

        public static ITest MockTestFixture(string name, string namespaceName)
        {
            return null;
            //var m = new Mock<ITest>();

            //var tn = new TestName
            //    {
            //        FullName = name,
            //        Name = name
            //    };
            //var ptn = new TestName
            //    {
            //        FullName = namespaceName,
            //        Name = namespaceName
            //    };

            //m.Setup(_ => _.TestName).Returns(tn);
            //m.Setup(_ => _.Parent.TestName).Returns(ptn);
            //m.Setup(_ => _.Tests).Returns(new ArrayList());
            //m.Setup(_ => _.TestType).Returns("TestFixture");
            //return m.Object;
        }

        /*  public static Mock<INUnitWrapper> MockNUnitWrapperForLoad(out List<ITest> testClasses)
          {
              var wrapperMock = new Mock<INUnitWrapper>();

              var list = new List<ITest>();
              wrapperMock.Setup(_ => _.TestLoaded).Returns(list.ToObservable<ITest>());
              wrapperMock.Setup(_ => _.TestLoadFailed).Returns(new List<Exception>().ToObservable());
              testClasses = list;
              return wrapperMock;
          }*/
    }
}