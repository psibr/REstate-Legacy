using System;

namespace REstateClient
{
    public interface IREstateClientFactory
    {
        Uri ApiKeyAuthAddress { get; }

        REstateAuthClient GetAuthClient();

        REstateConfigurationClient GetConfigurationClient(Uri baseAddress);

        REstateConfigurationClient GetConfigurationClient(string baseAddress);
    }
}
