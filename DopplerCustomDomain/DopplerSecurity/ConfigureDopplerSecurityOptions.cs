using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DopplerCustomDomain.DopplerSecurity
{
    public class ConfigureDopplerSecurityOptions : IConfigureOptions<DopplerSecurityOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IFileProvider _fileProvider;

        public ConfigureDopplerSecurityOptions(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _fileProvider = webHostEnvironment.ContentRootFileProvider;
        }

        private static string ReadToEnd(IFileInfo fileInfo)
        {
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static RsaSecurityKey ParseXmlString(string xmlString)
        {
            using var rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.FromXmlString(xmlString);
            var rsaParameters = rsaProvider.ExportParameters(false);
            return new RsaSecurityKey(RSA.Create(rsaParameters));
        }

        public void Configure(DopplerSecurityOptions options)
        {
            var path = _configuration.GetValue("PublicKeysFolder", "public-keys");
            var files = path is null
                ? Enumerable.Empty<IFileInfo>()
                : _fileProvider.GetDirectoryContents(path).Where(x => !x.IsDirectory);
            var publicKeys = files
                .Select(ReadToEnd)
                .Select(ParseXmlString)
                .ToArray();

            options.SkipLifetimeValidation = false;
            options.SigningKeys = publicKeys;
        }
    }
}
