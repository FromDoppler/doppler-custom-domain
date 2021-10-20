namespace DopplerCustomDomain.DnsValidation
{
    public record DnsValidationResult(
        string DomainName,
        bool IsPointingToOurService
    );
}
