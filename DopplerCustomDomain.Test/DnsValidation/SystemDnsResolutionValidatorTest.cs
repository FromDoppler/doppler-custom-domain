using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DopplerCustomDomain.DnsValidation
{
    // NOTE: These tests work query to real dns servers on the Internet
    public class SystemDnsResolutionValidatorTest : DnsResolutionValidatorTest<SystemDnsResolutionValidator>
    {
        protected override SystemDnsResolutionValidator CreateSut(Mock<ILogger<SystemDnsResolutionValidator>> loggerMock, IOptions<DnsValidationConfiguration> configuration) =>
            new SystemDnsResolutionValidator(loggerMock.Object, configuration);
    }

    public abstract class DnsResolutionValidatorTest<Tsut>
        where Tsut : IDnsResolutionValidator
    {
        protected abstract Tsut CreateSut(Mock<ILogger<Tsut>> loggerMock, IOptions<DnsValidationConfiguration> configuration);

        protected Tsut CreateSut() =>
            CreateSut(
                new Mock<ILogger<Tsut>>(),
                Options.Create<DnsValidationConfiguration>(new DnsValidationConfiguration()
                {
                    OurServersIPs = new[] { "184.106.28.222", "161.47.111.90", "161.47.111.91" }
                })
            );

        [Theory]
        [InlineData("trk2.relaytrk.com")] // A
        [InlineData("trk.relaytrk.com")] // A
        [InlineData("relaytrk.dopplerrelay.com")] // CNAME => trk2.relaytrk.com
        [InlineData("relaytrk.dopplerrelay.org")] // CNAME => trk.relaytrk.com
        [InlineData("dopplerpages.com")] // A
        [InlineData("www.dopplerpages.com")] // CNAME => dopplerpages.com
        [InlineData("r198.ddns.net")] // CNAME => www.dopplerpages.com
        public async Task IsNamePointingToOurServiceAsync_should_return_ok_for_well_configured_domains(string domainName)
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(domainName);

            // Assert
            Assert.True(result.IsPointingToOurService);
        }

        [Theory]
        [InlineData("www.google.com")] // A
        [InlineData("www.fromdoppler.com")] // A
        [InlineData("www.makingsense.com")] // CNAME => makingsense.com

        public async Task PUT_domain_should_return_BadRequest_and_not_store_domain_when_it_does_not_resolve_to_our_IP(string domainName)
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.ValidateAsync(domainName);

            // Assert
            Assert.False(result.IsPointingToOurService);
        }
    }
}
