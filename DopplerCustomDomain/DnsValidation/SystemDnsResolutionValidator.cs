using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DopplerCustomDomain.DnsValidation
{
    public class SystemDnsResolutionValidator : IDnsResolutionValidator
    {
        private readonly HashSet<IPAddress> _expectedIPs;
        private readonly DnsValidationVerdict _notResolvingVerdict;
        private readonly ILogger<SystemDnsResolutionValidator> _logger;

        public SystemDnsResolutionValidator(
            ILogger<SystemDnsResolutionValidator> logger,
            IOptions<DnsValidationConfiguration> configuration)
        {
            _logger = logger;
            _expectedIPs = new HashSet<IPAddress>(configuration.Value.OurServersIPs.Select(x => IPAddress.Parse(x)));
            _notResolvingVerdict = configuration.Value.NotResolvingVerdict;
        }

        public async Task<DnsValidationResult> ValidateAsync(string domainName)
        {
            try
            {
                var result = await Dns.GetHostAddressesAsync(domainName);
                return result.All(_expectedIPs.Contains) ? new PointingToUsDnsValidationResult(domainName)
                    : new NotPointingToUsDnsValidationResult(domainName, _notResolvingVerdict);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error resolving IP address for {domainName}, assuming that it is not pointing to our service", domainName);
                return new DnsValidationResultWithException(domainName, e, _notResolvingVerdict);
            }
        }
    }
}
