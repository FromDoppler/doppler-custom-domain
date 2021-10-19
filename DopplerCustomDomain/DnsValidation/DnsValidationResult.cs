using System;

namespace DopplerCustomDomain.DnsValidation
{
    public record DnsValidationResult(
        string DomainName,
        bool IsPointingToOurService
    );

    public record DnsValidationResultWithException(
        string DomainName,
        Exception Exception
    ) : DnsValidationResult(DomainName, false);
}
