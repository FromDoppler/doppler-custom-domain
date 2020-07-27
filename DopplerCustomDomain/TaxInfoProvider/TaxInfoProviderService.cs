using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Tavis.UriTemplates;


namespace DopplerCustomDomain.TaxInfoProvider
{
    public class TaxInfoProviderService : ITaxInfoProviderService
    {
        private readonly ILogger<TaxInfoProviderService> _logger;
        private readonly TaxInfoProviderOptions _taxInfoProviderOptions;

        public TaxInfoProviderService(ILogger<TaxInfoProviderService> logger, IOptions<TaxInfoProviderOptions> taxInfoProviderOptions)
        {
            _logger = logger;
            _taxInfoProviderOptions = taxInfoProviderOptions.Value;
        }

        public async Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit)
        {
            var url = new UriTemplate(_taxInfoProviderOptions.UriTemplate)
                .AddParameter("host", _taxInfoProviderOptions.Host)
                .AddParameter("cuit", cuit.SimplifiedValue)
                .Resolve();

            var result = await url
                .WithHeader("UserName", _taxInfoProviderOptions.Username)
                .WithHeader("Password", _taxInfoProviderOptions.Password)
                .GetJsonAsync<TaxInfo>();

            return result;
        }
    }
}
