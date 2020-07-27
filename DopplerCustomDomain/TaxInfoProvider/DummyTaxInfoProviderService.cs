using System.Threading.Tasks;

namespace DopplerCustomDomain.TaxInfoProvider
{
    public class DummyTaxInfoProviderService : ITaxInfoProviderService
    {
        public DummyTaxInfoProviderService()
        {
        }

        public Task<TaxInfo> GetTaxInfoByCuit(CuitNumber cuit)
            => Task.FromResult(new TaxInfo()
            {
                ActividadPrincipal = "620100-SERVICIOS DE CONSULTORES EN INFORMÁTICA Y SUMINISTROS DE PROGRAMAS DE INFORMÁTICA",
                Apellido = null,
                CUIT = cuit.cuit,
                CatIVA = "RI",
                CatImpGanancias = "RI",
                DomicilioCodigoPostal = "7600",
                DomicilioDatoAdicional = null,
                DomicilioDireccion = "CALLE FALSA 123 Piso:2",
                DomicilioLocalidad = "MAR DEL PLATA SUR",
                DomicilioPais = "AR",
                DomicilioProvincia = "01",
                DomicilioTipo = "FISCAL",
                Error = false,
                EstadoCUIT = "ACTIVO",
                Message = null,
                Monotributo = false,
                MonotributoActividad = null,
                MonotributoCategoria = null,
                Nombre = null,
                PadronData = null,
                ParticipacionesAccionarias = true,
                PersonaFisica = false,
                RazonSocial = "RZS C.S. SA",
                StatCode = 0
            });
    }
}
