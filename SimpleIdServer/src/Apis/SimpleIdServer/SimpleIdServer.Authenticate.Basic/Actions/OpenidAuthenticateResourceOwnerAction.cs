using SimpleIdServer.Authenticate.Basic.Helpers;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.Basic.Actions
{
    public interface IOpenidAuthenticateResourceOwnerAction
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, ClaimsPrincipal resourceOwnerPrincipal, string code, string issuerName);
    }

    internal sealed class OpenidAuthenticateResourceOwnerAction : IOpenidAuthenticateResourceOwnerAction
    {
        private readonly IParameterParserHelper _parameterParserHelper;
        private readonly IActionResultFactory _actionResultFactory;
        private readonly IAuthenticateHelper _authenticateHelper;

        public OpenidAuthenticateResourceOwnerAction(
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IAuthenticateHelper authenticateHelper)
        {
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _authenticateHelper = authenticateHelper;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter,ClaimsPrincipal resourceOwnerPrincipal, string code, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var resourceOwnerIsAuthenticated = resourceOwnerPrincipal.IsAuthenticated();
            var promptParameters = _parameterParserHelper.ParsePrompts(authorizationParameter.Prompt);
            if (resourceOwnerIsAuthenticated &&
                promptParameters != null &&
                !promptParameters.Contains(PromptParameter.login))
            {
                var subject = resourceOwnerPrincipal.GetSubject();
                var claims = resourceOwnerPrincipal.Claims.ToList();
                return await _authenticateHelper.ProcessRedirection(authorizationParameter,
                    code,
                    subject,
                    claims, issuerName);
            }

            return _actionResultFactory.CreateAnEmptyActionResultWithNoEffect();
        }
    }
}
