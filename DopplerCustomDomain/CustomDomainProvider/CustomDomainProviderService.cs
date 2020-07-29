using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavis.UriTemplates;


namespace DopplerCustomDomain.CustomDomainProvider
{
    public class CustomDomainProviderService : ICustomDomainProviderService
    {
        private readonly ILogger<CustomDomainProviderService> _logger;

        public CustomDomainProviderService(ILogger<CustomDomainProviderService> logger)
        {
            _logger = logger;
        }

        public Task CreateCustomDomain(string domain, string service)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteCustomDomain(string domain)
        {
            throw new System.NotImplementedException();
        }
    }
}
