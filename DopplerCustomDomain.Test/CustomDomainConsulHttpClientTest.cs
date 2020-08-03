using AutoFixture;
using DopplerCustomDomain.Consul;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DopplerCustomDomain.Test
{
    public class CustomDomainConsulHttpClientTest
    {
        private readonly IOptions<ConsulOptions> _options; 

        private readonly string _endpoint;
        private readonly string _value;

        public CustomDomainConsulHttpClientTest()
        {
            var fixture = new Fixture();

            _options = Options.Create(new ConsulOptions
            {
                BaseAddress = $"http://{fixture.Create<string>()}/"
            });

            _endpoint = fixture.Create<string>();
            _value = fixture.Create<string>();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task PutStringAsync_should_accept_a_success_StatusCode(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var clientMock = new Mock<HttpClient>();
            clientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });


            var sut = new ConsulHttpClient(clientMock.Object, _options);

            // Act & Assert
            await sut.PutStringAsync(_endpoint, _value);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task PutStringAsync_should_throw_HttpException_when_receive_a_non_success_StatusCode(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var clientMock = new Mock<HttpClient>();
            clientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            var sut = new ConsulHttpClient(clientMock.Object, _options);

            
            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                // Act
                await sut.PutStringAsync(_endpoint, _value);
            });
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task DeleteRecurseAsync_should_accept_a_success_StatusCode(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var clientMock = new Mock<HttpClient>();
            clientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });


            var sut = new ConsulHttpClient(clientMock.Object, _options);

            // Act & Assert
            await sut.DeleteRecurseAsync(_endpoint);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task DeleteRecurseAsync_should_throw_HttpException_when_receive_a_non_success_StatusCode(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var clientMock = new Mock<HttpClient>();
            clientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = httpStatusCode });

            var sut = new ConsulHttpClient(clientMock.Object, _options);


            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                // Act
                await sut.DeleteRecurseAsync(_endpoint);
            });
        }
    }
}
