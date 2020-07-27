using System.Threading.Tasks;

namespace CuitService.TaxInfoProvider
{
    public interface ITaxInfoProviderService
    {
        Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit);
    }
}
