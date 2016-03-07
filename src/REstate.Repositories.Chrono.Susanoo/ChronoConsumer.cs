using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using REstate.Chrono;
using REstate.Client;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace REstate.Repositories.Chrono.Susanoo
{
    public class ChronoConsumer : IChronoConsumer
    {
        private readonly IChronoRepository _repository;
        private readonly IInstancesSession _clientAuthenticatedSession;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public ChronoConsumer(IChronoRepository repository, IInstancesSession clientAuthenticatedSession)
        {
            _repository = repository;
            _clientAuthenticatedSession = clientAuthenticatedSession;
        }

        public void Start() => Task.Run(() =>
        {
            foreach (var chronoTrigger in _repository.GetChronoStream(_cts.Token))
            {
                try
                {
                    var currentState = _clientAuthenticatedSession
                        .GetMachineState(chronoTrigger.MachineInstanceId).Result;

                    if (currentState.StateName != chronoTrigger.StateName)
                        _repository.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();
                    else
                    {
                        try
                        {
                            using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew,
                                new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted},
                                TransactionScopeAsyncFlowOption.Enabled))
                            {
                                _clientAuthenticatedSession.FireTrigger(chronoTrigger.MachineInstanceId,
                                    chronoTrigger.TriggerName, chronoTrigger.Payload).Wait();

                                _repository.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();

                                transaction.Complete();
                            }
                        }
                        catch (StateConflictException) { }
                        catch (AggregateException ex)
                            when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                        {
                            //log here, but otherwise ignore, will be deleted on next pass.
                        }
                    }
                }
                catch (Exception ex)
                {
                     //Log
                }

            }
        }, _cts.Token);

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}