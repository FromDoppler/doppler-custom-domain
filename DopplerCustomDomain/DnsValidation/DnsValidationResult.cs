using System;

namespace DopplerCustomDomain.DnsValidation
{
    public record DnsValidationResult(
        string DomainName,
        bool IsPointingToOurService,
        DnsValidationVerdict Verdict
    );

    public record DnsValidationResultWithException(
        string DomainName,
        Exception Exception,
        DnsValidationVerdict Verdict
    ) : DnsValidationResult(DomainName, false, Verdict);
}
