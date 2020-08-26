using DopplerCustomDomain.Consul;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace DopplerCustomDomain.CustomDomainProvider
{
    public class CustomDomainProviderService : ICustomDomainProviderService
    {
        private readonly ILogger<CustomDomainProviderService> _logger;
        private readonly IConsulHttpClient _consulHttpClient;

        public CustomDomainProviderService(ILogger<CustomDomainProviderService> logger, IConsulHttpClient consulHttpClient)
        {
            _logger = logger;
            _consulHttpClient = consulHttpClient;
        }

        public async Task CreateCustomDomain(string domain, string service, TraefikRuleTypeEnum ruleType)
        {
            var httpsBaseUrl = GenerateHttpsRouteConsulUrl(domain);
            var httpBaseUrl = GenerateHttpRouteConsulUrl(domain);

            if (ruleType == TraefikRuleTypeEnum.HTTP_HTTPS ||
                ruleType == TraefikRuleTypeEnum.HTTP_HTTPS_WITHOUT_REDIRECT)
            {
                await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/entrypoints", "websecure_entry_point");
                await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/tls/certresolver", "letsencryptresolver");
                await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/rule", $"Host(`{domain}`)");
                await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/service", service);
            }

            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/entrypoints", "web_entry_point");
            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/service", service);
            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/rule", $"Host(`{domain}`)");
            if (ruleType != TraefikRuleTypeEnum.HTTP_HTTPS_WITHOUT_REDIRECT &&
                ruleType != TraefikRuleTypeEnum.HTTP)
            {
                await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/middlewares", "http_to_https@file");
            }
        }

        public async Task DeleteCustomDomain(string domain)
        {
            await _consulHttpClient.DeleteRecurseAsync($"{GenerateHttpsRouteConsulUrl(domain)}");
            await _consulHttpClient.DeleteRecurseAsync($"{GenerateHttpRouteConsulUrl(domain)}");
        }

        private string GenerateHttpsRouteConsulUrl(string domain) =>
            $"/v1/kv/traefik/http/routers/https_{domain}";

        private string GenerateHttpRouteConsulUrl(string domain) =>
            $"/v1/kv/traefik/http/routers/http_{domain}";
    }
}
