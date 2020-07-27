namespace CuitService.TaxInfoProvider
{
    public class TaxInfoProviderOptions
    {
        public bool UseDummyData { get; set; }
        public string UriTemplate { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
