using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace DopplerCustomDomain.Test
{
    public static class WebApplicationFactoryHelper
    {
        public static WebApplicationFactory<Startup> WithBypassAuthorization(this WebApplicationFactory<Startup> factory)
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(
                    // TODO: review if this is the best way to bypass the authentication
                    services => services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>()));

        public static WebApplicationFactory<Startup> AddConfiguration(this WebApplicationFactory<Startup> factory, IEnumerable<KeyValuePair<string, string>> initialData)
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureAppConfiguration(
                    (builderContext, configurationBuilder) => configurationBuilder.AddInMemoryCollection(initialData)));
    }
}
