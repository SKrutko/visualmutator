﻿namespace VisualMutator.Tests.Results
{
    #region

    using System;
    using VisualMutator.Model.Decompilation.CodeDifference;
    using NUnit.Framework;

    #endregion

    [TestFixture, Explicit]
    public class Difference
    {
        private struct Ss
        {
            public readonly int i;

            public Ss(int v)
            {
                i = v;
            }
        }

        [Test]
        public void Test0()
        {
            var aa = new Ss();
            Console.WriteLine(aa.i);
            Assert.Fail();
        }

        //SomeNamespace.Namespace:
        public void Method()
        {
            int i = 0;
            for (int j = 0; j < 10; j++)
            {
                i = i + j;
            }
            // int j=4;
        }

        //
        //            string mutatedMethod = @"
        //
        //    //SomeNamespace.Namespace:
        //    public void Method()
        //    {
        //        int i = 0;
        //        for(int j=0; j<10; j++)
        //        {
        //            i = i - j;
        //        }
        //        int j=4;
        //        int j2=5;
        //    }
        //
        //";

        //  var diff = new CodeDifferenceCreator(
        //       new AssembliesManager()).GetDiff(CodeLanguage.CSharp, testMethod, mutatedMethod);

        [Test]
        public void Test2()
        {
            var ilCodeLineEqualityComparer = new ILCodeLineEqualityComparer();
            Assert.IsTrue(ilCodeLineEqualityComparer.Equals(@"IL_00ca: mov 435", @"IL_0aeg: mov 435"));
        }
    }
}