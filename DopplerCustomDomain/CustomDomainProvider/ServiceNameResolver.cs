using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public class ServiceNameResolver : IServiceNameResolver
    {
        private readonly TraefikConfiguration _traefikConfiguration;

        public ServiceNameResolver(IOptions<TraefikConfiguration> options)
        {
            _traefikConfiguration = options.Value;
        }

        public Services Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
