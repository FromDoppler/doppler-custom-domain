using DopplerCustomDomain.DnsValidation;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DnsValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddDnsValidation(
            this IServiceCollection services,
            IConfigurationSection dnsValidationConfiguration)
        => services.Configure<DnsValidationConfiguration>(dnsValidationConfiguration)
            .AddSingleton<IDnsResolutionValidator, SystemDnsResolutionValidator>();
    }
}
