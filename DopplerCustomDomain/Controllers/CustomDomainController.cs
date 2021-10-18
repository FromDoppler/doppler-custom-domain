using DopplerCustomDomain.CustomDomainProvider;
using DopplerCustomDomain.DnsValidation;
using DopplerCustomDomain.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.Controllers
{
    [Authorize]
    [ApiController]
    public class CustomDomainController
    {
        private readonly ILogger<CustomDomainController> _logger;
        private readonly ICustomDomainProviderService _customDomainProviderService;
        private readonly IServiceNameResolver _serviceNameResolver;
        private readonly IDnsResolutionValidator _dnsResolutionValidator;

        public CustomDomainController(
            ILogger<CustomDomainController> logger,
            ICustomDomainProviderService customDomainProviderService,
            IServiceNameResolver serviceNameResolver,
            IDnsResolutionValidator dnsResolutionValidator)
        {
            _logger = logger;
            _customDomainProviderService = customDomainProviderService;
            _serviceNameResolver = serviceNameResolver;
            _dnsResolutionValidator = dnsResolutionValidator;
        }

        [HttpGet("/")]
        [AllowAnonymous]
        public string Home()
        {
            return "Custom Domain Service";
        }

        [HttpGet("/{domainName}/_ip-resolution")]
        public async Task<IActionResult> ValidateCustomDomainIPResolution([FromRoute] string domainName)
        {
            if (!(await _dnsResolutionValidator.IsNamePointingToOurServiceAsync(domainName)))
            {
                return new BadRequestObjectResult($"{domainName} does not resolve to our service IP address");
            }
            return new OkObjectResult($"{domainName} resolves to our service IP address");
        }

        [HttpPut("/{domainName}")]
        public async Task<IActionResult> CreateCustomDomain([FromRoute] string domainName, [FromBody] DomainConfiguration domainConfiguration)
        {
            var serviceName = _serviceNameResolver.Resolve(domainConfiguration.service);

            if (string.IsNullOrEmpty(serviceName))
            {
                return new NotFoundObjectResult($"Cannot find the service called: {domainConfiguration.service}");
            }

            await _customDomainProviderService.CreateCustomDomain(domainName, serviceName, domainConfiguration.ruleType);
            return new OkObjectResult("Custom Domain Created");
        }

        [HttpDelete("/{domain}")]
        public async Task<IActionResult> DeleteCustomDomain([FromRoute] string domain)
        {
            await _customDomainProviderService.DeleteCustomDomain(domain);

            return new OkObjectResult("Custom Domain Deleted");
        }
    }
}
