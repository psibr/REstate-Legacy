using System;
using Nancy.ModelBinding;
using REstate.Chrono;
using REstate.Web.Modules;

namespace REstate.Web.Chrono.Modules
{
    /// <summary>
    /// Chrono module.
    /// </summary>
    public class ChronoModule
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

            Post["AddChronoTrigger", "/triggers", true] = async (parameters, ct) =>
            {
                var addChronoTriggerRequest = this.Bind<ChronoTrigger>();

                try
                {
                    await _chronoRepository.AddChronoTrigger(addChronoTriggerRequest, ct);
                }
                catch (Exception ex)
                {
                    throw;
                }

                return 200;
            };

        }

    }
}