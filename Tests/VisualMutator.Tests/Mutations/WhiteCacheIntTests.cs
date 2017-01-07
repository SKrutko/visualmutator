﻿namespace VisualMutator.Tests.Mutations
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Infrastructure;
    using Model.StoringMutants;
    using Moq;
    using Ninject;
    using Ninject.Modules;
    using Ninject.Syntax;
    using NUnit.Framework;
    using SoftwareApproach.TestingExtensions;
    using UsefulTools.Paths;
    using Util;
    using VisualMutator.Infrastructure;
    using VisualMutator.Infrastructure.NinjectModules;

    [TestFixture]
    public class WhiteCacheIntTests
    {
        private StandardKernel _kernel;


        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel();
            _kernel.Load(new IntegrationTestModule());
        }

        [Test]
        public void Test()
        {
            var paths = new[] {
                 TestProjects.DsaPath,
                 TestProjects.DsaTestsPath,
                 TestProjects.DsaTestsPath2}.Select(_ => new FilePathAbsolute(_)).ToList();

            _kernel.Bind<IProjectClonesManager>().To<ProjectClonesManager>().InSingletonScope();
            _kernel.Bind<ProjectFilesClone>().ToSelf().AndFromFactory();
            _kernel.Bind<FilesManager>().ToSelf().InSingletonScope();
            _kernel.Bind<WhiteCache>().ToSelf().AndFromFactory();
            _kernel.BindMock<IHostEnviromentConnection>(mock =>
            {
                mock.Setup(_ => _.GetProjectAssemblyPaths()).Returns(paths);
                mock.Setup(_ => _.GetTempPath()).Returns(Path.GetTempPath());
            });

            var cache = _kernel.GetFromFactory<WhiteCache>(2);
            cache.Initialize().Wait();

            var a = cache.GetWhiteSourceAsync("Dsa.Test").Result;
            var b = cache.GetWhiteSourceAsync("Dsa").Result;
            var c = cache.GetWhiteSourceAsync("Dsa").Result;

            a.Modules.Count.ShouldEqual(1);
            b.Modules.Count.ShouldEqual(1);
            c.Modules.Count.ShouldEqual(1);
        }



    }


   
}