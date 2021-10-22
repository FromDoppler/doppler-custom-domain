namespace DopplerCustomDomain.DnsValidation
{
    public class DnsValidationConfiguration
    {
        public string[] OurServersIPs { get; init; } = new string[0];
        public DnsValidationVerdict NotResolvingVerdict { get; init; } = DnsValidationVerdict.Allow;
    }
}
