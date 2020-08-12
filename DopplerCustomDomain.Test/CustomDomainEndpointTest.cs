using AutoFixture;
using DopplerCustomDomain.Consul;
using DopplerCustomDomain.CustomDomainProvider;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DopplerCustomDomain.Test
{
    public class CustomDomainEndpointTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private const string WEBSECURE_ENTRY_POINT = "websecure_entry_point";
        private const string WEB_ENTRY_POINT = "web_entry_point";
        private const string LETSENCRYPT_RESOLVER = "letsencryptresolver";
        private const string HTTP_TO_HTTPS_MIDDLEWARE = "http_to_https@file";

        public CustomDomainEndpointTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Create_custom_domain_should_response_OK_when_ConsultHttpClient_do_not_fail()
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var domainConfiguration = new DomainConfiguration
            {
                service = "relay-tracking"
            };

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(x => x.PutStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory.WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
            {
                services.RemoveAll<IConsulHttpClient>();
                services.AddSingleton(consulHttpClientMock.Object);
            })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_custom_domain_should_response_NotFound_when_the_service_is_not_supported()
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var serviceName = fixture.Create<string>();
            var domainConfiguration = new DomainConfiguration
            {
                service = serviceName
            };

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(x => x.PutStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}")
            {
                Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("forms-landing", "doppler_forms_service_prod@docker")]
        [InlineData("relay-tracking", "relay-actions-api_service_prod@docker")]
        public async Task Create_custom_domain_should_send_all_keys_to_consul_when_success(string serviceName, string expectedServiceConfiguration)
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var domainConfiguration = new DomainConfiguration
            {
                service = serviceName
            };
            var expectedHttpsBaseUrl = $"/v1/kv/traefik/http/routers/https_{domainName}";
            var expectedHttpBaseUrl = $"/v1/kv/traefik/http/routers/http_{domainName}";

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.SetReturnsDefault(Task.CompletedTask);

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory
                .WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConsulHttpClient>();
                    services.AddSingleton(consulHttpClientMock.Object);
                })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpsBaseUrl}/entrypoints", WEBSECURE_ENTRY_POINT),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpsBaseUrl}/tls/certresolver", LETSENCRYPT_RESOLVER),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpsBaseUrl}/rule", $"Host(`{domainName}`)"),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpsBaseUrl}/service", expectedServiceConfiguration),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpBaseUrl}/entrypoints", WEB_ENTRY_POINT),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpBaseUrl}/rule", $"Host(`{domainName}`)"),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpBaseUrl}/middlewares", HTTP_TO_HTTPS_MIDDLEWARE),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.PutStringAsync($"{expectedHttpBaseUrl}/service", expectedServiceConfiguration),
                Times.Once);

            consulHttpClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Delete_endpoint_should_response_OK_when_receive_success_response_from_consul()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(c => c.DeleteRecurseAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory
                .WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConsulHttpClient>();
                    services.AddSingleton(consulHttpClientMock.Object);
                })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_endpoint_should_response_InternalServerError_when_received_a_not_success_response_from_consul()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(c => c.DeleteRecurseAsync(It.IsAny<string>())).Throws<HttpRequestException>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory
                .WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConsulHttpClient>();
                    services.AddSingleton(consulHttpClientMock.Object);
                })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Delete_custom_domain_should_send_all_keys_to_consul_when_success()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var httpsBaseUrl = $"/v1/kv/traefik/http/routers/https_{domainName}";
            var httpBaseUrl = $"/v1/kv/traefik/http/routers/http_{domainName}";

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.SetReturnsDefault(Task.CompletedTask);

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory
                .WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConsulHttpClient>();
                    services.AddSingleton(consulHttpClientMock.Object);
                })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            consulHttpClientMock.Verify(
                x => x.DeleteRecurseAsync($"{httpsBaseUrl}"),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.DeleteRecurseAsync($"{httpBaseUrl}"),
                Times.Once);


            consulHttpClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_custom_domain_should_response_InternalServerError_when_receive_a_not_success_response_from_consul()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var domainConfiguration = new DomainConfiguration
            {
                service = "relay-tracking"
            };

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(c => c.PutStringAsync(It.IsAny<string>(), It.IsAny<string>())).Throws<HttpRequestException>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = appFactory
                .WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IConsulHttpClient>();
                    services.AddSingleton(consulHttpClientMock.Object);
                })).CreateClient();


            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        //Authentication token super user is true
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.j1qzmKcnpCCBoXAtK9QuzCcnkIedK_kpwlrQ315VX_bwuxNxDBeEgKCOcjACUaNnf92bStGVYxXusSlnCgWApjlFG4TRgcTNsBC_87ZMuTgjP92Ou_IHi5UVDkiIyeQ3S_-XpYGFksgzI6LhSXu2T4LZLlYUHzr6GN68QWvw19m1yw6LdrNklO5qpwARR4WEJVK-0dw2-t4V9jK2kR8zFkTYtDUFPEQaRXFBpaPWAdI1p_Dk_QDkeBbmN_vTNkF7JwmqXRRAaz5fiMmcgzFmayJFbM0Y9LUeaAYFSZytIiYZuNitVixWZEcXT_jwtfHpyDwZKY1-HlyMmUJJuVsf2A")]
        //Authentication token super user is false
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ3MDksImV4cCI6MTU5Nzc2NDcxOSwiaWF0IjoxNTk3NzY0NzA5LCJpc1NVIjpmYWxzZX0.K7khi_qhvj0eF3ahZzNcRkzrRPDFR_q-5xAujSeFG3GaFhJIhgARX7fsA4iPPhTJtFA1oqF54d-vyNhGAhBDFzSKUHyRegdRJ5FiQwcQ537PbZUfCc702sEi-MjzfpkP1PZrk0Zrn5-ybUDJi-6qjia8_YxvA4px8KGPT10Z6PnrpeCuWtESmMlSre7CgCRpydXZ0XkV0hsn-CD8p5oSV9iMCXS3npJBBhzLvw9B_LienlnJQMVs88ykSDqZNUWdGMVTO4QF4JChd67W7B9I0MmmbtgCZ5yo0EwykYR6RaZYihtKjesmHlBcFaHJc1C-3V8TQ3L0-81PpemqZd_3yQ")]
        //Authentication token is not super user
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1NzcsImV4cCI6MTU5Nzc2NDU4NywiaWF0IjoxNTk3NzY0NTc3fQ.S3qzN2kCR7VtrkCG-FTq_Hrv377Fn8wevryAhHHKq5SupMsEaa1SYAdNZdlMLZyyZQUe95UYM4_Ba63Kbm9zu6fkh_xKfmLGbiZhEjJM5nVR0HLa7mAPTNY25YrfRtQRyyLvLDJ1KSXIY_iUd1IT1hQAIqMG7pD29eD6RY4_n_z619AgET94F_Jj7w505JvNNR7z5fpW5ZM1XaEPlrCbXVfCKtLLxM8YlNRBOmyJRG2ideaRfqEw7vb3AIW6c4EdHV1c9EBsYGfWkSJZOOpXKoOpUmvhVLmxpctTNNq_iS67JE3AFlkatboq9z8l9DHDIdoveIE6unHq4YgUmltnDg")]
        //Authentication token is invalid
        [InlineData(HttpStatusCode.Unauthorized, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9")]
        public async Task Create_custom_domain_should_response_depending_on_the_authorization_token(HttpStatusCode httpStatusCode, string token)
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var domainConfiguration = new DomainConfiguration
            {
                service = "relay-tracking"
            };

            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.Setup(x => x.PutStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            using var appFactory = _factory.WithDisabledLifeTimeValidation();

            var client = appFactory.WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
            {
                services.RemoveAll<IConsulHttpClient>();
                services.AddSingleton(consulHttpClientMock.Object);
            })).CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            Assert.NotNull(response);
            Assert.Equal(httpStatusCode, response.StatusCode);
        }
    }
}
