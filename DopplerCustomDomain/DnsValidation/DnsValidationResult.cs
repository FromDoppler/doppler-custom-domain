using System;

namespace DopplerCustomDomain.DnsValidation
{
    public record DnsValidationResult(
        string DomainName,
        bool IsPointingToOurService,
        DnsValidationVerdict Verdict
    );

    public record PointingToUsDnsValidationResult(
        string DomainName
    ) : DnsValidationResult(DomainName, true, DnsValidationVerdict.Allow);

    public record NotPointingToUsDnsValidationResult(
        string DomainName,
        DnsValidationVerdict Verdict
    ) : DnsValidationResult(DomainName, false, Verdict);

    public record DnsValidationResultWithException(
        string DomainName,
        Exception Exception,
        DnsValidationVerdict Verdict
    ) : DnsValidationResult(DomainName, false, Verdict);
}
