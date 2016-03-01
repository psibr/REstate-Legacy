using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using REstate.Client;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace REstate.Chrono.Susanoo
{
    public class ChronoConsumer
    {
        private readonly IChronoEngine _engine;
        private readonly AuthenticatedSession _clientAuthenticatedSession;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public ChronoConsumer(IChronoEngine engine, AuthenticatedSession clientAuthenticatedSession)
        {
            _engine = engine;
            _clientAuthenticatedSession = clientAuthenticatedSession;
        }

        public void Start() => Task.Run(() =>
        {
            foreach (var chronoTrigger in _engine.GetChronoStream(_cts.Token))
            {
                try
                {
                    var currentState = _clientAuthenticatedSession
                        .GetMachineState(chronoTrigger.MachineInstanceId).Result;

                    if (currentState.StateName != chronoTrigger.StateName)
                        _engine.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();
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

                                _engine.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();

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
                    throw; //Log
                }

            }
        }, _cts.Token);

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}