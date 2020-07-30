namespace DopplerCustomDomain.CustomDomainProvider
{
    public interface IServiceNameResolver
    {
        string? Resolve(string publicName);
    }
}
