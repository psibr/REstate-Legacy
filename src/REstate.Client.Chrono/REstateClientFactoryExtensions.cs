using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Client.Chrono
{
    public static class REstateClientFactoryExtensions
    {
        public static REstateChronoClient GetChronoClient(this IREstateClientFactory factory, string baseAddress)
        {
            return new REstateChronoClient(factory.ApiKeyAuthAddress, baseAddress);
        }

        public static REstateChronoClient GetChronoClient(this IREstateClientFactory factory, Uri baseAddress)
        {
            return new REstateChronoClient(factory.ApiKeyAuthAddress, baseAddress);
        }

    }
}
