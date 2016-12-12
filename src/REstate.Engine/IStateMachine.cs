using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public interface IStateMachine
    {
        string MachineInstanceId { get; }

        string MachineDefinitionId { get; }

        Task FireAsync(
            Trigger trigger, 
            string contentType,
            string payload, 
            CancellationToken cancellationToken);

        Task FireAsync(
            Trigger trigger,
            string contentType, 
            string payload, 
            string lastCommitTag,
            CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(State state, CancellationToken cancellationToken);

        Task<State> GetCurrentStateAsync(CancellationToken cancellationToken);

        Task<ICollection<Trigger>> GetPermittedTriggersAsync(CancellationToken cancellationToken);
    }
}
