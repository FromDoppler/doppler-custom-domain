using AutoFixture;
using DopplerCustomDomain.Api;
using DopplerCustomDomain.Consul;
using DopplerCustomDomain.CustomDomainProvider;
using DopplerCustomDomain.DnsValidation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        private Mock<IConsulHttpClient> CreateConsulHttpClientMock()
        {
            var consulHttpClientMock = new Mock<IConsulHttpClient>();
            consulHttpClientMock.SetReturnsDefault(Task.CompletedTask);
            return consulHttpClientMock;
        }

        private Mock<ICustomDomainProviderService> CreateCustomDomainProviderServiceMock()
        {
            var customDomainProviderServiceMock = new Mock<ICustomDomainProviderService>();
            customDomainProviderServiceMock.SetReturnsDefault(Task.CompletedTask);
            return customDomainProviderServiceMock;
        }

        private Mock<IDnsResolutionValidator> CreateDnsResolutionValidatorMock()
        {
            var dnsResolutionValidatorMock = new Mock<IDnsResolutionValidator>();
            dnsResolutionValidatorMock.SetReturnsDefault(Task.FromResult(true));
            return dnsResolutionValidatorMock;
        }

        private HttpClient CreateHttpClient(
            WebApplicationFactory<Startup> appFactory,
            IConsulHttpClient? consulHttpClient = null,
            ICustomDomainProviderService? customDomainProviderService = null,
            IDnsResolutionValidator? dnsResolutionValidator = null,
            ILogger<CustomDomainController>? customDomainControllerLogger = null)
        => appFactory.WithWebHostBuilder((e) => e.ConfigureTestServices(services =>
        {
            if (consulHttpClient != null)
            {
                services.AddSingleton(consulHttpClient);
            }
            if (customDomainProviderService != null)
            {
                services.AddSingleton(customDomainProviderService);
            }
            if (dnsResolutionValidator != null)
            {
                services.AddSingleton(dnsResolutionValidator);
            }
            if (customDomainControllerLogger != null)
            {
                services.AddSingleton(customDomainControllerLogger);
            }
        })).CreateClient();

        [Fact]
        public async Task Create_custom_domain_should_response_OK_when_ConsultHttpClient_do_not_fail()
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var domainConfiguration = new
            {
                service = "relay-tracking",
                ruleType = "HttpsOnly"
            };

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: CreateConsulHttpClientMock().Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_custom_domain_should_response_NotFound_when_the_service_is_not_supported()
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var serviceName = fixture.Create<string>();
            var domainConfiguration = new
            {
                service = serviceName,
                ruleType = "HttpsOnly"
            };

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: CreateConsulHttpClientMock().Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}")
            {
                Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("forms-landing", "doppler_forms_service_prod@docker", RuleType.HttpsOnly)]
        [InlineData("relay-tracking", "relay-actions-api_service_prod@docker", RuleType.HttpsOnly)]
        public async Task Create_custom_domain_should_send_all_keys_to_consul_when_success(string serviceName, string expectedServiceConfiguration, RuleType ruleTypeName)
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var domainConfiguration = new
            {
                service = serviceName,
                ruleType = ruleTypeName.ToString()
            };
            var expectedHttpsBaseUrl = $"/v1/kv/traefik/http/routers/https_{domainName}";
            var expectedHttpBaseUrl = $"/v1/kv/traefik/http/routers/http_{domainName}";

            var consulHttpClientMock = CreateConsulHttpClientMock();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: consulHttpClientMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
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

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: CreateConsulHttpClientMock().Object);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_endpoint_should_response_InternalServerError_when_received_a_not_success_response_from_consul()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();

            var consulHttpClientMock = CreateConsulHttpClientMock();
            consulHttpClientMock.Setup(c => c.DeleteRecurseAsync(It.IsAny<string>())).Throws<HttpRequestException>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: consulHttpClientMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

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

            var consulHttpClientMock = CreateConsulHttpClientMock();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: consulHttpClientMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"http://localhost/{domainName}");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            consulHttpClientMock.Verify(
                x => x.DeleteRecurseAsync($"{httpsBaseUrl}"),
                Times.Once);

            consulHttpClientMock.Verify(
                x => x.DeleteRecurseAsync($"{httpBaseUrl}"),
                Times.Once());

            consulHttpClientMock.Verify(
                x => x.DeleteRecurseAsync($"{httpBaseUrl}/middlewares"),
                Times.Once());

            consulHttpClientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_custom_domain_should_response_InternalServerError_when_receive_a_not_success_response_from_consul()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var domainConfiguration = new
            {
                service = "relay-tracking",
                ruleType = "HttpsOnly"
            };

            var consulHttpClientMock = CreateConsulHttpClientMock();
            consulHttpClientMock.Setup(c => c.PutStringAsync(It.IsAny<string>(), It.IsAny<string>())).Throws<HttpRequestException>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: consulHttpClientMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        //Authentication token super user is true
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.ZOjcLy7DkpyhcJTI7ZGKQfkjrWW1B8TZvFYjwXDiZrZEgZSlKNG0P6ecu1MDtgEhRKVIIRAEvtNVTNg7JRYV9wMFuBOqYuiQT0yddccYbhN6w6W8gS_yJsY6AxombY_fMPezvuXxf9ScZC7qmHNDV-JbR8jaxyoY0HRpVBesD6sD3lSprNQDvZlw_jaHeisF21-rrDyW2XwKPpCu5mVllOn_Nsg8w1K44wKG5GgKIaP_8ItfQUI5fyflx6LrXGkQ1tP43wEYveDycVB7CJ9DRAd4oI4eKoGygTNm3wO1ab4mlGautmY8qB7SDbuLjhPFRch2WsWsCz4dSNJp268dvw")]
        //Authentication token super user is false
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ3MDksImV4cCI6MTU5Nzc2NDcxOSwiaWF0IjoxNTk3NzY0NzA5LCJpc1NVIjpmYWxzZX0.QDZolMwgEVP18-coDEbWajFbjhqPGFGOgHQusTda1gid__FzCO5w1idGhMoAuiyfRdVVzuF9I5Iz_Opx020xVkyPUl3EDU32-RHn2OBQOtmOlvna2cJyeQk0LwsWTf1lnvUKamBKUeztl2IXJXNcXwXt9y7hC6fMlYsn3hDRA0YcIfv1Q37iz8_cHYQ7O2HB1JuZRUwkhfobMYvXDLt3GS8u8MNSM_hKTmlf6wII-jRG-G25ePFibkChld2Rc5cjzVQy_VM9q83BZiSSeaoLUm0NNw49eACiQ50KY_YhY2GeEnptA1p3JicKMGWB_RNp3MdC632EZmtPtCjn8TkRHA")]
        //Authentication token is not super user
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1NzcsImV4cCI6MTU5Nzc2NDU4NywiaWF0IjoxNTk3NzY0NTc3fQ.bIUKKuIZOPZapoB05v3N5h_dHfu7R_O_DZ2pu2j3esJd3kwUjxEwqVVI_l97yBMScaCnbsdEyt4w1nKYwI5vj6UQR7GJoR6TERPfFtpiO0zlGEIWPJu9zI3fgA7HfJifw5B6fQidDHDYUbbM3oHD9cn7CiB4XizEe-6LGnjlBzo5Hr1Rsrz6-eD5UQhx7FkqLLRFDhIQ9cn_36Wc9ylzfvmzKZ4ZAn4Q5-s3f2rkN-tuXiBAxrwkgXhOZ72f8dj5mED6PLauH3uPEbaMcrVKD-CIe9Una5zq-zWtsZVasSQeO1_lCjQzhhTQXfwrWJ9WBx1ozkDA9XzJiiS_jAMqMA")]
        //Authentication token is invalid
        [InlineData(HttpStatusCode.Unauthorized, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9")]
        public async Task Create_custom_domain_should_response_depending_on_the_authorization_token(HttpStatusCode httpStatusCode, string token)
        {
            // Arrange
            var fixture = new Fixture();

            var domainName = fixture.Create<string>();
            var domainConfiguration = new
            {
                service = "relay-tracking",
                ruleType = "HttpsOnly"
            };

            using var appFactory = _factory.WithDisabledLifeTimeValidation();

            var client = CreateHttpClient(
                appFactory,
                consulHttpClient: CreateConsulHttpClientMock().Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = new StringContent(JsonSerializer.Serialize(domainConfiguration), Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            Assert.Equal(httpStatusCode, response.StatusCode);
        }


        [Fact]
        public async Task GET_domain_ip_resolution_should_return_Ok_when_it_resolves_to_our_IP()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();

            var dnsResolutionValidatorMock = CreateDnsResolutionValidatorMock();
            dnsResolutionValidatorMock.Setup(x => x.ValidateAsync(domainName)).ReturnsAsync(new DnsValidationResult(domainName, true));

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                dnsResolutionValidator: dnsResolutionValidatorMock.Object);

            // Act
            var response = await client.GetAsync($"http://localhost/{domainName}/_ip-resolution");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_domain_ip_resolution_should_return_BadRequest_when_it_does_not_resolve_to_our_IP()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();

            var dnsResolutionValidatorMock = CreateDnsResolutionValidatorMock();
            dnsResolutionValidatorMock.Setup(x => x.ValidateAsync(domainName)).ReturnsAsync(new DnsValidationResult(domainName, false));

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                dnsResolutionValidator: dnsResolutionValidatorMock.Object);

            // Act
            var response = await client.GetAsync($"http://localhost/{domainName}/_ip-resolution");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PUT_domain_should_log_warning_and_store_domain_when_it_does_not_resolve_to_our_IP()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var expectedRuleType = RuleType.HttpsOnly;
            var ruleType = expectedRuleType.ToString();
            var expectedService = "relay-actions-api_service_prod@docker";
            var service = "relay-tracking";

            var customDomainProviderServiceMock = CreateCustomDomainProviderServiceMock();
            var dnsResolutionValidatorMock = CreateDnsResolutionValidatorMock();
            dnsResolutionValidatorMock.Setup(x => x.ValidateAsync(domainName)).ReturnsAsync(new DnsValidationResult(domainName, false));

            var customDomainControllerLoggerMock = new Mock<ILogger<CustomDomainController>>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                customDomainProviderService: customDomainProviderServiceMock.Object,
                dnsResolutionValidator: dnsResolutionValidatorMock.Object,
                customDomainControllerLogger: customDomainControllerLoggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = JsonContent.Create(new { service, ruleType });

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            customDomainProviderServiceMock.Verify(x => x.CreateCustomDomain(domainName, expectedService, expectedRuleType), Times.Once);
            customDomainProviderServiceMock.Verify(x => x.CreateCustomDomain(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RuleType>()), Times.Once);
            customDomainControllerLoggerMock.VerifyLog(LogLevel.Warning, $"WARNING: {domainName} does not resolve to our service IP address. Result: DnsValidationResult {{ DomainName = {domainName}, IsPointingToOurService = False }}", Times.Once);
        }

        [Fact]
        public async Task PUT_domain_should_not_log_warning_and_store_domain_when_it_resolves_to_our_IP()
        {
            // Arrange
            var fixture = new Fixture();
            var domainName = fixture.Create<string>();
            var expectedRuleType = RuleType.HttpsOnly;
            var ruleType = expectedRuleType.ToString();
            var expectedService = "relay-actions-api_service_prod@docker";
            var service = "relay-tracking";

            var customDomainProviderServiceMock = CreateCustomDomainProviderServiceMock();
            var dnsResolutionValidatorMock = CreateDnsResolutionValidatorMock();
            dnsResolutionValidatorMock.Setup(x => x.ValidateAsync(domainName)).ReturnsAsync(new DnsValidationResult(domainName, true));

            var customDomainControllerLoggerMock = new Mock<ILogger<CustomDomainController>>();

            using var appFactory = _factory.WithBypassAuthorization();

            var client = CreateHttpClient(
                appFactory,
                customDomainProviderService: customDomainProviderServiceMock.Object,
                dnsResolutionValidator: dnsResolutionValidatorMock.Object,
                customDomainControllerLogger: customDomainControllerLoggerMock.Object);

            var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost/{domainName}");
            request.Content = JsonContent.Create(new { service, ruleType });

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            customDomainProviderServiceMock.Verify(x => x.CreateCustomDomain(domainName, expectedService, expectedRuleType), Times.Once);
            customDomainProviderServiceMock.Verify(x => x.CreateCustomDomain(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RuleType>()), Times.Once);
            customDomainControllerLoggerMock.VerifyLog(LogLevel.Warning, "does not resolve to our service IP address", Times.Never);
        }
    }
}
