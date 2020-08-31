using System.Collections.Generic;
using System.Threading.Tasks;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public interface ICustomDomainProviderService
    {
        Task CreateCustomDomain(string domain, string service, RuleType ruleType);

        Task DeleteCustomDomain(string domain);
    }
}
