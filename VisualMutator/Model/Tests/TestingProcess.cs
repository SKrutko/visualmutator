﻿namespace VisualMutator.Model.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading.Tasks;
    using Controllers;
    using Infrastructure;
    using log4net;
    using Mutations.MutantsTree;
    using UsefulTools.ExtensionMethods;

    public class TestingProcess
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OptionsModel _optionsModel;
        private readonly IRootFactory<TestingMutant> _mutantTestingFactory;
        private readonly Subject<SessionEventArgs> _sessionEventsSubject;

        private readonly int _allMutantsCount;
        private int _testedNonEquivalentMutantsCount;
        private int _mutantsKilledCount;
        //AKB
        private int _numberOfMarkedEq;
        private int _numberOfFirstOrderMutants;
        private readonly WorkerCollection<Mutant> _mutantsWorkers;
        private int _testedMutantsCount;
        private bool _stopping;


        public TestingProcess(
            OptionsModel optionsModel,
            IRootFactory<TestingMutant> mutantTestingFactory,
            // ------- on creation
            Subject<SessionEventArgs> sessionEventsSubject,
            ICollection<Mutant> allMutants)
        {
            _optionsModel = optionsModel;
            _mutantTestingFactory = mutantTestingFactory;
            _sessionEventsSubject = sessionEventsSubject;

            _allMutantsCount = allMutants.Count;
            _testedNonEquivalentMutantsCount = 0;
            _testedMutantsCount = 0;
            //AKB
            _numberOfFirstOrderMutants = 0;
            _numberOfMarkedEq = 0;

            _log.Info("Testing process: all:" + _allMutantsCount);

            _mutantsWorkers = new WorkerCollection<Mutant>(allMutants,
               _optionsModel.ProcessingThreadsCount, TestOneMutant);
        }

        public void RaiseTestingProgress()
        {
            _log.Info("**********************************************************");

            _log.Info("Testing progress: all:"+ _allMutantsCount +
                ", tested: "+ _testedNonEquivalentMutantsCount
                +"killed: "+ _mutantsKilledCount);

            _log.Info("**********************************************************");

            _sessionEventsSubject.OnNext(new TestingProgressEventArgs(OperationsState.Testing)
            {
                NumberOfAllMutants = _allMutantsCount,
                NumberOfAllMutantsTested = _testedMutantsCount,
                Description = ("Mutants tested: {0}/{1} " + (_stopping ? "(Stop request)" : ""))
                             .Formatted(_testedMutantsCount ,
                                 _allMutantsCount),
            });

            _sessionEventsSubject.OnNext(new MutationScoreInfoEventArgs(OperationsState.Testing)
            {
                NumberOfAllNonEquivalent = _testedNonEquivalentMutantsCount,
                NumberOfMutantsKilled = _mutantsKilledCount,
                //AKB
                NumberOfFirstOrderMutants = _numberOfFirstOrderMutants,
                NumberOfMarkedEq = _numberOfMarkedEq,
            });
        }

        public void Stop()
        {
            _mutantsWorkers.Stop();
            _stopping = true;
        }

        public void Start(Action endCallback)
        {
            _stopping = false;
            _mutantsWorkers.Start(endCallback);
        }

        public async Task TestOneMutant(Mutant mutant)
        {
            try
            {
                _log.Info("Testing process for mutant: "+ mutant.Id);
                IObjectRoot<TestingMutant> testingMutant = _mutantTestingFactory.CreateWithParams(_sessionEventsSubject, mutant);
                await testingMutant.Get.RunAsync();
            }
            catch (Exception e)
            {
                _log.Error(e);
                mutant.MutantTestSession.ErrorMessage = e.ToString();
                mutant.MutantTestSession.ErrorDescription = e.Message;
                mutant.State = MutantResultState.Error;
            }
            lock (this)
            {
                
                _testedNonEquivalentMutantsCount++;
                _testedMutantsCount++;
                _mutantsKilledCount = _mutantsKilledCount.IncrementedIf(mutant.State == MutantResultState.Killed);
                //AKB
                if (mutant.Id.IndexOf("First Order Mutant")!=-1)
                {
                    _numberOfFirstOrderMutants++;
                }
                RaiseTestingProgress();
            }
        }


        public void TestWithHighPriority(Mutant mutant)
        {
           _mutantsWorkers.LockingMoveToFront(mutant);
        }

        public void MarkedAsEqivalent(bool equivalent)
        {
            lock (this)
            {
                if (equivalent)
                {
                    _testedNonEquivalentMutantsCount--;
                    _numberOfMarkedEq++;

                }
                else
                {
                    _testedNonEquivalentMutantsCount++;
                    _numberOfMarkedEq--;
                }
           
                _sessionEventsSubject.OnNext(new MutationScoreInfoEventArgs(OperationsState.None)
                {
                    NumberOfMutantsKilled = _mutantsKilledCount,
                    NumberOfAllNonEquivalent = _testedNonEquivalentMutantsCount,
                    //AKB
                    NumberOfFirstOrderMutants = _numberOfFirstOrderMutants,
                    NumberOfMarkedEq = _numberOfMarkedEq,
                });
            }
        }
    }
}