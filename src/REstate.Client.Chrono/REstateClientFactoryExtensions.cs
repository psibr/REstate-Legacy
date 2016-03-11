using System;

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
