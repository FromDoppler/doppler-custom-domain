using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DopplerCustomDomain.DnsValidation
{
    public class SystemDnsResolutionValidator : IDnsResolutionValidator
    {
        // TODO: read this from configuration based on the environment
        private static readonly HashSet<IPAddress> _expectedIPs = new HashSet<IPAddress>()
        {
            IPAddress.Parse("184.106.28.222"),
        };

        private readonly ILogger<SystemDnsResolutionValidator> _logger;

        public SystemDnsResolutionValidator(ILogger<SystemDnsResolutionValidator> logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsNamePointingToOurServiceAsync(string domainName)
        {
            try
            {
                var result = await Dns.GetHostAddressesAsync(domainName);
                return result.All(_expectedIPs.Contains);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error resolving IP address for {domainName}, assuming that it is not pointing to our service", domainName);
                return false;
            }
        }
    }
}
