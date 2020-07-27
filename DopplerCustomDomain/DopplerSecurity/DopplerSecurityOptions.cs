using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace DopplerCustomDomain.DopplerSecurity
{
    public class DopplerSecurityOptions
    {
        public bool SkipLifetimeValidation { get; set; }
        public IEnumerable<SecurityKey> SigningKeys { get; set; } = new SecurityKey[0];
    }
}
