using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.DopplerSecurity
{
    public class IsSuperUserHandler : AuthorizationHandler<IsSuperUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            IsSuperUserRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type.Equals("isSU")))
            {
                return Task.CompletedTask;
            }

            var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals("isSU")).Value);
            if (isSuperUser)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
