namespace DopplerCustomDomain.DnsValidation
{
    public enum DnsValidationVerdict
    {
        Allow = 0,
        Ignore,
        // TODO: Consider adding a new option "Block" to throw BadRequest error in place of simply ignore the domain
    }
}
