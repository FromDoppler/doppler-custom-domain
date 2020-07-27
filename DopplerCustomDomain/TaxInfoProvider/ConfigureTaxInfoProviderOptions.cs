using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DopplerCustomDomain.TaxInfoProvider
{
    public class ConfigureTaxInfoProviderOptions : IConfigureOptions<TaxInfoProviderOptions>
    {
        private const string DefaultConfigurationSectionName = "TaxInfoProvider";

        private readonly IConfiguration _configuration;

        public ConfigureTaxInfoProviderOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(TaxInfoProviderOptions options)
        {
            _configuration.Bind(DefaultConfigurationSectionName, options);
        }
    }
}
