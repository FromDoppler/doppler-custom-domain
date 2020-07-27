using System.Threading.Tasks;

namespace DopplerCustomDomain.TaxInfoProvider
{
    public interface ITaxInfoProviderService
    {
        Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit);
    }
}
