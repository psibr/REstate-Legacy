using Nancy.ModelBinding;
using Nancy;
using REstate.Scheduler;
using REstate.Scheduling;

namespace REstate.Web.Modules
{
    /// <summary>
    /// Chrono module.
    /// </summary>
    public sealed class ChronoModule
        : SecuredModule
    {
        private readonly IChronoRepositoryFactory _chronoRepositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChronoModule"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="chronoRepository">The chrono repository.</param>
        public ChronoModule(REstateConfiguration configuration, IChronoRepositoryFactory chronoRepositoryFactory)
            : base(configuration, "/scheduler", claim => claim.Type == "claim" && claim.Value == "operator")
        {
            _chronoRepositoryFactory = chronoRepositoryFactory;

            Post("/triggers", async (parameters, ct) =>
            {
                var chronoRepository = _chronoRepositoryFactory.OpenRepository(Context.CurrentUser?.GetApiKey());

                var addChronoTriggerRequest = this.Bind<ChronoTrigger>();

                await chronoRepository.AddChronoTriggerAsync(addChronoTriggerRequest, ct).ConfigureAwait(false);

                return HttpStatusCode.OK;
            });

        }

    }
}