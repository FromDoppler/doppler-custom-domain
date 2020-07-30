using DopplerCustomDomain.CustomDomainProvider;
using Flurl.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace DopplerCustomDomain.Controllers
{
    [Authorize]
    [ApiController]
    public class CustomDomainController
    {
        private readonly ILogger<CustomDomainController> _logger;
        private readonly ICustomDomainProviderService _customDomainProviderService;
        private readonly IServiceNameResolver _serviceNameResolver;

        public CustomDomainController(
            ILogger<CustomDomainController> logger,
            ICustomDomainProviderService customDomainProviderService,
            IServiceNameResolver serviceNameResolver)
        {
            _logger = logger;
            _customDomainProviderService = customDomainProviderService;
            _serviceNameResolver = serviceNameResolver;
        }
    }
}
