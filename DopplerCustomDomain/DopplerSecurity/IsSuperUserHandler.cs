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
            var isSuperUserClaim = context.User.FindFirst(c => c.Type.Equals("isSU"));

            if (isSuperUserClaim == null)
            {
                _logger.LogWarning("The token hasn't super user permissions.");
                return Task.CompletedTask;
            }

            if (!bool.TryParse(isSuperUserClaim.Value, out var isSuperUser))
            {
                _logger.LogWarning($"The token's super user permissions value is invalid: `{isSuperUserClaim.Value}`.");
                return Task.CompletedTask;
            }

            if (!isSuperUser)
            {
                _logger.LogWarning("The token super user permissions is false.");
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
