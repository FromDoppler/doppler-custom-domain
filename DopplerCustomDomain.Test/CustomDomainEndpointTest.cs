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
    }
}
