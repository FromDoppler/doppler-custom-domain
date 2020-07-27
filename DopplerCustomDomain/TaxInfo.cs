namespace DopplerCustomDomain
{
    public class TaxInfo
    {
        public string? ActividadPrincipal { get; set; }
        public string? Apellido { get; set; }
        public string? CUIT { get; set; }
        public string? CatIVA { get; set; }
        public string? CatImpGanancias { get; set; }
        public string? DomicilioCodigoPostal { get; set; }
        public string? DomicilioDatoAdicional { get; set; }
        public string? DomicilioDireccion { get; set; }
        public string? DomicilioLocalidad { get; set; }
        public string? DomicilioPais { get; set; }
        public string? DomicilioProvincia { get; set; }
        public string? DomicilioTipo { get; set; }
        public bool Error { get; set; }
        public string? EstadoCUIT { get; set; }
        public string? Message { get; set; }
        public bool Monotributo { get; set; }
        public string? MonotributoActividad { get; set; }
        public string? MonotributoCategoria { get; set; }
        public string? Nombre { get; set; }
        public string? PadronData { get; set; }
        public bool ParticipacionesAccionarias { get; set; }
        public bool PersonaFisica { get; set; }
        public string? RazonSocial { get; set; }
        public int StatCode { get; set; }
    }
}
