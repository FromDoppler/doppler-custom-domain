using DopplerCustomDomain.DopplerSecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DopplerSecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddDopplerSecurity(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureDopplerSecurityOptions>();

            services
                .AddOptions<AuthorizationOptions>()
                .Configure(o =>
                {
                    o.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                    o.AddPolicy(nameof(IsSuperUserRequirement), policy =>
                    {
                        policy.Requirements.Add(new IsSuperUserRequirement());
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.Build();
                    });
                });

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<DopplerSecurityOptions>>((o, securityOptions) =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKeys = securityOptions.Value.SigningKeys,
                        ValidateIssuer = false,
                        ValidateLifetime = !securityOptions.Value.SkipLifetimeValidation,
                        ValidateAudience = false,
                    };
                });

            services.AddAuthentication().AddJwtBearer();

            services.AddAuthorization();

            return services;
        }
    }
}
