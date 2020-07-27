using CuitService.TaxInfoProvider;
using Flurl.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace CuitService.Controllers
{
    [Authorize]
    [ApiController]
    public class TaxInfoController
    {
        private readonly ILogger<TaxInfoController> _logger;
        private readonly ITaxInfoProviderService _taxInfoProviderService;

        public TaxInfoController(ILogger<TaxInfoController> logger, ITaxInfoProviderService taxInfoProviderService)
        {
            _logger = logger;
            _taxInfoProviderService = taxInfoProviderService;
        }

        // TODO: rename cuitNumber parameter as cuit
        [HttpGet("/taxinfo/by-cuit/{cuit}")]
        public async Task<TaxInfo> GetTaxInfoByCuit([FromRoute] CuitNumber cuitNumber)
            => await _taxInfoProviderService.GetTaxInfoByCuit(cuitNumber);
    }
}
