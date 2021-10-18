using DopplerCustomDomain.DnsValidation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DnsValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddDnsValidation(this IServiceCollection services)
            => services.AddSingleton<IDnsResolutionValidator, SystemDnsResolutionValidator>();
    }
}
