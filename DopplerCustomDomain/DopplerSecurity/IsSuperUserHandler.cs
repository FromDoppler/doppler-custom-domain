using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DopplerCustomDomain.DopplerSecurity
{
    public class IsSuperUserHandler : AuthorizationHandler<IsSuperUserRequirement>
    {
        private readonly ILogger<IsSuperUserHandler> _logger;

        public IsSuperUserHandler(ILogger<IsSuperUserHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            IsSuperUserRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type.Equals("isSU")))
            {
                _logger.LogWarning("The token hasn't super user permissions.");
                return Task.CompletedTask;
            }

            var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals("isSU")).Value);
            if (isSuperUser)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            _logger.LogWarning("The token super user permissions is false.");
            return Task.CompletedTask;
        }
    }
}
