using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DopplerCustomDomain.DnsValidation
{
    // NOTE: These tests work query to real dns servers on the Internet
    public class SystemDnsResolutionValidatorTest : DnsResolutionValidatorTest<SystemDnsResolutionValidator>
    {
        public SystemDnsResolutionValidatorTest()
            : base(new SystemDnsResolutionValidator(Mock.Of<ILogger<SystemDnsResolutionValidator>>()))
        {
        }
    }

    public abstract class DnsResolutionValidatorTest<Tsut>
        where Tsut : IDnsResolutionValidator
    {
        private readonly Tsut _sut;

        public DnsResolutionValidatorTest(Tsut sut)
        {
            _sut = sut;
        }

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
            // Act
            var result = await _sut.IsNamePointingToOurServiceAsync(domainName);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("www.google.com")] // A
        [InlineData("www.fromdoppler.com")] // A
        [InlineData("www.makingsense.com")] // CNAME => makingsense.com

        public async Task PUT_domain_should_return_BadRequest_and_not_store_domain_when_it_does_not_resolve_to_our_IP(string domainName)
        {
            // Act
            var result = await _sut.IsNamePointingToOurServiceAsync(domainName);

            // Assert
            Assert.False(result);
        }
    }
}
