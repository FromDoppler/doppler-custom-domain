using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DopplerCustomDomain.Test
{
    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        public virtual Task<AuthenticateResult> AuthenticateAsync(
            AuthorizationPolicy policy,
            HttpContext context)
        => Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                "FakeScheme")));

        public virtual Task<PolicyAuthorizationResult> AuthorizeAsync(
            AuthorizationPolicy policy,
            AuthenticateResult authenticationResult,
            HttpContext context,
            object? resource)
        => Task.FromResult(PolicyAuthorizationResult.Success());
    }
}
