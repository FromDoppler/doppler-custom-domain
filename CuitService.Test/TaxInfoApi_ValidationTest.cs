using AutoFixture;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CuitService.Test
{
    public class TaxInfoApi_ValidationTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpTest _httpTest;

        public TaxInfoApi_ValidationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _httpTest = new HttpTest();
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        [Theory]
        [InlineData("20-31111111-8")]
        [InlineData("20-31111111-6")]
        [InlineData("20-31111111-1")]
        public async Task GET_taxinfo_by_cuit_with_an_invalid_verification_digit_should_return_400_BadRequest(string cuit)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();

            var content = await response.Content.ReadAsStringAsync();
            var problemDetail = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("One or more validation errors occurred.", problemDetail.GetProperty("title").GetString());
            Assert.Collection(problemDetail.GetProperty("errors").EnumerateObject(),
                item =>
                {
                    Assert.Equal("cuit", item.Name);
                    Assert.Equal(1, item.Value.GetArrayLength());
                    Assert.Equal("The CUIT's verification digit is wrong.", item.Value.EnumerateArray().First().GetString());
                });
        }

        [Theory]
        [InlineData("20-3111111-8")]
        [InlineData("20-311111111-6")]
        public async Task GET_taxinfo_by_cuit_with_wrong_length_should_return_400_BadRequest(string cuit)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();

            var content = await response.Content.ReadAsStringAsync();
            var problemDetail = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("One or more validation errors occurred.", problemDetail.GetProperty("title").GetString());
            Assert.Collection(problemDetail.GetProperty("errors").EnumerateObject(),
                item =>
                {
                    Assert.Equal("cuit", item.Name);
                    Assert.Equal(1, item.Value.GetArrayLength());
                    Assert.Equal("The CUIT number must have 11 digits.", item.Value.EnumerateArray().First().GetString());
                });
        }

        [Theory]
        [InlineData("-")]
        [InlineData("-----")]
        [InlineData("%20%20")]
        [InlineData("%20%20-")]
        [InlineData("-%20%20-")]
        public async Task GET_taxinfo_by_cuit_with_dashes_or_spaces_should_return_400_BadRequest(string cuit)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();

            var content = await response.Content.ReadAsStringAsync();
            var problemDetail = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("One or more validation errors occurred.", problemDetail.GetProperty("title").GetString());
            Assert.Collection(problemDetail.GetProperty("errors").EnumerateObject(),
                item =>
                {
                    Assert.Equal("cuit", item.Name);
                    Assert.Equal(1, item.Value.GetArrayLength());
                    Assert.Equal("The CUIT number cannot be empty.", item.Value.EnumerateArray().First().GetString());
                });
        }

        [Theory]
        [InlineData("1234a5890")]
        [InlineData("1234a")]
        [InlineData("20 31111111 7")]
        [InlineData("20x31111111x7")]
        [InlineData("20,31111111,7")]
        [InlineData("20_31111111_7")]
        public async Task GET_taxinfo_by_cuit_with_invalid_characters_should_return_400_BadRequest(string cuit)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();

            var content = await response.Content.ReadAsStringAsync();
            var problemDetail = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.Equal("One or more validation errors occurred.", problemDetail.GetProperty("title").GetString());
            Assert.Collection(problemDetail.GetProperty("errors").EnumerateObject(),
                item =>
                {
                    Assert.Equal("cuit", item.Name);
                    Assert.Equal(1, item.Value.GetArrayLength());
                    Assert.Equal("The CUIT number cannot have other characters than numbers and dashes.", item.Value.EnumerateArray().First().GetString());
                });
        }

        [Theory]
        [InlineData("")]
        [InlineData("     ")]
        public async Task GET_taxinfo_by_cuit_without_segment_should_return_404_NotFound(string cuit)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();
        }


        [Theory]
        [InlineData("20311111117")]
        [InlineData("33123456780")]
        [InlineData("20-31111111-7")]
        [InlineData("3-3-1-2-3-4-5-6-7-8-0")]
        public async Task GET_taxinfo_by_cuit_should_accept_valid_cuit_numbers(string cuit)
        {
            // Arrange
            _httpTest.RespondWithJson(new { });
            var appFactory = _factory.WithBypassAuthorization()
                .AddConfiguration(new Dictionary<string, string>()
                {
                    ["TaxInfoProvider:UseDummyData"] = "false"
                });
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/taxinfo/by-cuit/{cuit}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _httpTest.ShouldHaveMadeACall();
        }
    }
}
