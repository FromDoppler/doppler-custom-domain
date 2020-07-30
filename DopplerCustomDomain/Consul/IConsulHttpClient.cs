using System.Threading.Tasks;

namespace DopplerCustomDomain.Consul
{
    public interface IConsulHttpClient
    {
        Task DeleteRecurseAsync(string url);
        Task PutStringAsync(string url, string value);
    }
}
