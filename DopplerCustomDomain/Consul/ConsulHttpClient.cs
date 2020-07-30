using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DopplerCustomDomain.Consul
{
    public class ConsulHttpClient : IConsulHttpClient
    {
        private readonly HttpClient _httpClient;

        public ConsulHttpClient(HttpClient httpClient, IOptions<ConsulOptions> options)
        {
            httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
            _httpClient = httpClient;
        }

        public async Task PutStringAsync(string url, string value)
        {
            var response = await _httpClient.PutAsync(url, new StringContent(value, Encoding.UTF8));
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteRecurseAsync(string url)
        {
            // TODO: support URLs with query string already set
            var response = await _httpClient.DeleteAsync($"{url}?recurse=true");
            response.EnsureSuccessStatusCode();
        }
    }

}
