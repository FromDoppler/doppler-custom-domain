using System.Threading.Tasks;

namespace DopplerCustomDomain.DnsValidation
{
    public interface IDnsResolutionValidator
    {
        Task<bool> IsNamePointingToOurServiceAsync(string domainName);
    }
}
