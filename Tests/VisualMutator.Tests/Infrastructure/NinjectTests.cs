﻿namespace VisualMutator.Tests.Infrastructure
{
    #region

    using Ninject;
    using Ninject.Activation.Strategies;
    using Ninject.Modules;
    using NUnit.Framework;
    using SoftwareApproach.TestingExtensions;
    using UsefulTools.DependencyInjection;
    using VisualMutator.Infrastructure;
    using VisualMutator.Infrastructure.NinjectModules;

    #endregion

    [TestFixture]
    public class NinjectTests
    {
        private StandardKernel _kernel;

        [Test]
        public void TestPartialParameters()
        {
            var modules = new INinjectModule[]
            {
                new TestModule(),
            };

            _kernel = new StandardKernel();
            _kernel.Components.Add<IActivationStrategy, MyMonitorActivationStrategy>();

            _kernel.Load(modules);

            var factory = _kernel.Get<IFactory<SomeObject>>();
            SomeObject withParams = factory.CreateWithParams(1);
            withParams.Parameter.ShouldEqual(1);
            withParams.Module.ShouldNotBeNull();
        }

        public class TestModule : NinjectModule
        {
            public override void Load()
            {
                Kernel.Bind<SomeMainModule>().ToSelf();
                Kernel.Bind<SomeObject>().ToSelf().AndFromFactory();
            }
        }

        [Test]
        public void TestPartialParametersInChild()
        {
            var modules = new INinjectModule[]
            {
                new TestModuleChilds(),
            };

            _kernel = new StandardKernel();
            _kernel.Components.Add<IActivationStrategy, MyMonitorActivationStrategy>();

            _kernel.Load(modules);

            var main1 = _kernel.Get<SomeMainModule>();
            var main2 = _kernel.Get<SomeMainModule>();

            Assert.AreSame(main1, main2);

            var factory = _kernel.Get<IFactory<SomeTimedModule>>();
            SomeTimedModule someTimedModule1 = factory.Create();
            SomeTimedModule someTimedModule2 = factory.Create();

            Assert.AreNotSame(someTimedModule1.InnerModule, someTimedModule2.InnerModule);

            SomeTimedObject someTimedObject = someTimedModule1.ObjFactory.CreateWithParams(1);

            Assert.AreSame(someTimedObject.ModuleTimed, someTimedModule1.InnerModule);
            Assert.AreSame(someTimedObject.Module, main2);

            SomeTimedObject someTimedObject2 = someTimedModule1.ObjFactory.CreateWithParams(2);
            Assert.AreNotSame(someTimedObject2.TimedObjectInner, someTimedObject.TimedObjectInner);
            Assert.AreSame(someTimedObject2.TimedObjectInner, someTimedObject2.InnerInner.Inner);
        }

        public class TestModuleChilds : NinjectModule
        {
            public override void Load()
            {
                //  Kernel.Load(new ContextPreservationModule());
                Kernel.Bind<SomeMainModule>().ToSelf().InSingletonScope();
                Kernel.BindObjectRoot<SomeTimedModule>().ToSelf(childKernel =>
                {
                    //childKernel.Bind<SomeTimedObject>().ToSelf().AndFromFactory();
                    childKernel.Bind<SomeInnerModule>().ToSelf().InSingletonScope();

                    childKernel.BindObjectRoot<SomeTimedObject>().ToSelf(child =>
                    {
                        child.Bind<SomeTimedObject>().ToSelf().InSingletonScope();
                        child.Bind<SomeTimedObjectInner>().ToSelf().InSingletonScope();
                        child.Bind<SomeTimedObjectInnerInner>().ToSelf().InSingletonScope();
                    });
                });

                Kernel.Bind<SomeObject>().ToSelf().AndFromFactory();
            }
        }

        public class SomeMainModule
        {
        }

        public class SomeInnerModule
        {
        }

        public class SomeTimedModule
        {
            public SomeInnerModule InnerModule { get; set; }
            private readonly IFactory<SomeTimedObject> _objFactory;

            public SomeTimedModule(IFactory<SomeTimedObject> objFactory, SomeInnerModule innerModule)
            {
                InnerModule = innerModule;
                _objFactory = objFactory;
            }

            public IFactory<SomeTimedObject> ObjFactory
            {
                get { return _objFactory; }
            }
        }

        public class SomeTimedModule2
        {
            public SomeInnerModule InnerModule
            {
                get;
                set;
            }

            private readonly IFactory<SomeTimedObject> _objFactory;

            public SomeTimedModule2(IFactory<SomeTimedObject> objFactory, SomeInnerModule innerModule)
            {
                InnerModule = innerModule;
                _objFactory = objFactory;
            }

            public IFactory<SomeTimedObject> ObjFactory
            {
                get
                {
                    return _objFactory;
                }
            }
        }

        public class SomeObject
        {
            private readonly SomeMainModule _module;
            private readonly int _parameter;

            public SomeObject(SomeMainModule module, int parameter)
            {
                _module = module;
                _parameter = parameter;
            }

            public SomeMainModule Module
            {
                get { return _module; }
            }

            public int Parameter
            {
                get { return _parameter; }
            }
        }

        public class SomeTimedObject
        {
            public SomeInnerModule ModuleTimed { get; set; }
            public SomeTimedObjectInner TimedObjectInner { get; set; }
            public SomeTimedObjectInnerInner InnerInner { get; set; }
            private readonly SomeMainModule _module;
            private readonly int _parameter;

            public SomeTimedObject(SomeMainModule module,
                SomeInnerModule moduleTimed,
                SomeTimedObjectInner timedObjectInner,
                SomeTimedObjectInnerInner innerInner, int parameter)
            {
                ModuleTimed = moduleTimed;
                TimedObjectInner = timedObjectInner;
                InnerInner = innerInner;
                _module = module;
                _parameter = parameter;
            }

            public SomeMainModule Module
            {
                get
                {
                    return _module;
                }
            }

            public int Parameter
            {
                get
                {
                    return _parameter;
                }
            }
        }

        public class SomeTimedObjectInner
        {
            public static int i = 0;
            public int id;

            public SomeTimedObjectInner()
            {
                id = i++;
            }
        }

        public class SomeTimedObjectInnerInner
        {
            public SomeTimedObjectInner Inner { get; set; }
            public static int i = 0;
            public int id;

            public SomeTimedObjectInnerInner(SomeTimedObjectInner inner)
            {
                Inner = inner;
                id = i++;
            }
        }
    }
}