using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public enum RuleType
    {
        HttpsOnly = 1,
        HttpsAndHttp,
        HttpOnly
    }
}
