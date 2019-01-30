using SimpleIdServer.Authenticate.Basic.Helpers;
using SimpleIdServer.Authenticate.LoginPassword.Parameters;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.LoginPassword.Actions
{
    public interface ILoginPwdAuthenticateAction
    {
        Task<LoginPwdAuthenticateResult> Execute(LoginPasswordAuthParameter parameter, AuthorizationParameter authorizationParameter, string code, string issuerName);
    }

    public class LoginPwdAuthenticateResult
    {
        public ActionResult ActionResult { get; set; }
        public List<Claim> Claims { get; set; }
    }

    internal sealed class LoginPwdAuthenticateAction : ILoginPwdAuthenticateAction
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;
        private readonly IAuthenticateHelper _authenticateHelper;

        public LoginPwdAuthenticateAction(IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper, IAuthenticateHelper authenticateHelper)
        {
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
            _authenticateHelper = authenticateHelper;
        }

        public async Task<LoginPwdAuthenticateResult> Execute(LoginPasswordAuthParameter parameter, AuthorizationParameter authorizationParameter, string code, string issuerName)
        {
            if(parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }


            var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(parameter.Login, parameter.Password, new[] { Constants.AMR }).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
            }

            var claims = resourceOwner.Claims == null ? new List<Claim>() : resourceOwner.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer));
            return new LoginPwdAuthenticateResult
            {
                ActionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter, code, resourceOwner.Id, claims, issuerName).ConfigureAwait(false),
                Claims = claims
            };
        }
    }
}
