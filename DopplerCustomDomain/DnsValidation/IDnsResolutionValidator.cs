using System.Threading.Tasks;

namespace DopplerCustomDomain.DnsValidation
{
    public interface IDnsResolutionValidator
    {
        Task<DnsValidationResult> ValidateAsync(string domainName);
    }
}
