using DopplerCustomDomain.CustomDomainProvider;
using DopplerCustomDomain.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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

        public CustomDomainController(
            ILogger<CustomDomainController> logger,
            ICustomDomainProviderService customDomainProviderService,
            IServiceNameResolver serviceNameResolver)
        {
            _logger = logger;
            _customDomainProviderService = customDomainProviderService;
            _serviceNameResolver = serviceNameResolver;
        }

        [HttpGet("/")]
        [AllowAnonymous]
        public string Home()
        {
            return "Custom Domain Service";
        }


        [HttpPut("/{domainName}")]
        public async Task<IActionResult> CreateCustomDomain([FromRoute] string domainName, [FromBody] DomainConfiguration domainConfiguration)
        {
            var serviceName = _serviceNameResolver.Resolve(domainConfiguration.service);

            if (string.IsNullOrEmpty(serviceName))
            {
                return new NotFoundObjectResult($"Cannot find the service called: {domainConfiguration.service}");
            }

            TraefikRuleTypeEnum ruleTypeEnum;
            if (!Enum.TryParse(domainConfiguration.ruleType, out ruleTypeEnum))
            {
                ruleTypeEnum = TraefikRuleTypeEnum.HTTP;
            }

            await _customDomainProviderService.CreateCustomDomain(domainName, serviceName, ruleTypeEnum);
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
