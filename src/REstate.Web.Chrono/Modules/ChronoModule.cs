using Nancy.ModelBinding;
using REstate.Chrono;
using System;
using Nancy;
using Psibr.Platform.Nancy.Modules;

namespace REstate.Web.Chrono.Modules
{
    /// <summary>
    /// Chrono module.
    /// </summary>
    public sealed class ChronoModule
        : SecuredModule
    {
        private readonly IChronoRepository _chronoRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChronoModule"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="chronoRepository">The chrono repository.</param>
        public ChronoModule(ChronoRoutePrefix prefix,
            IChronoRepository chronoRepository)
            : base(prefix)
        {
            _chronoRepository = chronoRepository;

            Post("/triggers", async (parameters, ct) =>
            {
                var addChronoTriggerRequest = this.Bind<ChronoTrigger>();

                await _chronoRepository.AddChronoTrigger(addChronoTriggerRequest, ct);

                return HttpStatusCode.OK;
            });

        }

    }
}