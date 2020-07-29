using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public class TraefikConfiguration
    {
        public Dictionary<string, string> ServicesMapping { get; set; } = new Dictionary<string, string>();
    }
}
