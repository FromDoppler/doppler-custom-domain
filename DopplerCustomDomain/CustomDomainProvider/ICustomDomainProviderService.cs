using System.Collections.Generic;
using System.Threading.Tasks;

namespace DopplerCustomDomain.CustomDomainProvider
{
    public interface ICustomDomainProviderService
    {
        Task CreateCustomDomain(string domain, string service);

        Task DeleteCustomDomain(string domain);
    }
}
