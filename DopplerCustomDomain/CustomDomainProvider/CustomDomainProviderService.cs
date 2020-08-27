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

        public async Task CreateCustomDomain(string domain, string service, RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.HttpsOnly:
                    await CreateHttps(domain, service);
                    await CreateHttp(domain, service);
                    await CreateRedirectRule(domain);
                    break;
                case RuleType.HttpsAndHttp:
                    await CreateHttps(domain, service);
                    await CreateHttp(domain, service);
                    await DeleteRedirectRule(domain);
                    break;
                case RuleType.HttpOnly:
                    await CreateHttp(domain, service);
                    await DeleteRedirectRule(domain);
                    await DeleteHttps(domain);
                    break;
            }
        }

        private async Task CreateHttps(string domain, string service)
        {
            var httpsBaseUrl = GenerateHttpsRouteConsulUrl(domain);

            await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/entrypoints", "websecure_entry_point");
            await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/tls/certresolver", "letsencryptresolver");
            await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/rule", $"Host(`{domain}`)");
            await _consulHttpClient.PutStringAsync($"{httpsBaseUrl}/service", service);
        }

        private async Task CreateHttp(string domain, string service)
        {
            var httpBaseUrl = GenerateHttpRouteConsulUrl(domain);

            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/entrypoints", "web_entry_point");
            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/service", service);
            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/rule", $"Host(`{domain}`)");
        }

        private async Task CreateRedirectRule(string domain)
        {
            var httpBaseUrl = GenerateHttpRouteConsulUrl(domain);
            await _consulHttpClient.PutStringAsync($"{httpBaseUrl}/middlewares", "http_to_https@file");
        }

        public async Task DeleteCustomDomain(string domain)
        {
            await DeleteHttps(domain);
            await DeleteHttp(domain);
            await DeleteRedirectRule(domain);
        }

        private async Task DeleteHttps(string domain)
        {
            var httpsBaseUrl = GenerateHttpsRouteConsulUrl(domain);
            await _consulHttpClient.DeleteRecurseAsync($"{httpsBaseUrl}");
        }

        private async Task DeleteHttp(string domain)
        {
            var httpBaseUrl = GenerateHttpRouteConsulUrl(domain);
            await _consulHttpClient.DeleteRecurseAsync($"{httpBaseUrl}");
        }

        private async Task DeleteRedirectRule(string domain)
        {
            var httpBaseUrl = GenerateHttpRouteConsulUrl(domain);
            await _consulHttpClient.DeleteRecurseAsync($"{httpBaseUrl}/middlewares");
        }

        private string GenerateHttpsRouteConsulUrl(string domain) =>
            $"/v1/kv/traefik/http/routers/https_{domain}";

        private string GenerateHttpRouteConsulUrl(string domain) =>
            $"/v1/kv/traefik/http/routers/http_{domain}";
    }
}
