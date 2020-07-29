using Microsoft.Extensions.Options;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public class ServiceNameResolver : IServiceNameResolver
    {
        private readonly TraefikConfiguration _traefikConfiguration;

        public ServiceNameResolver(IOptions<TraefikConfiguration> options)
        {
            _traefikConfiguration = options.Value;
        }

        public string? Resolve(string publicName)
        {
            if (!_traefikConfiguration.ServicesMapping.TryGetValue(publicName, out var serviceName))
            {
                return null;
            }

            return serviceName;
        }
    }
}
