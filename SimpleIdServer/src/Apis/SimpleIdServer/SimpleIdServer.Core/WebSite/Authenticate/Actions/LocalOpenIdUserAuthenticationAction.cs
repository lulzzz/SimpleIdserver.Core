using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Extensions;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.WebSite.Authenticate.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.WebSite.Authenticate.Actions
{
    public interface ILocalOpenIdUserAuthenticationAction
    {
        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        Task<LocalOpenIdAuthenticationResult> Execute(LocalAuthenticationParameter localAuthenticationParameter, AuthorizationParameter authorizationParameter, string code, string issuerName);
    }

    public class LocalOpenIdAuthenticationResult
    {
        public ActionResult ActionResult { get; set; }
        public ICollection<Claim> Claims { get; set; }
        public string TwoFactor { get; set; }
    }

    public class LocalOpenIdUserAuthenticationAction : ILocalOpenIdUserAuthenticationAction
    {
        private readonly IResourceOwnerAuthenticateHelper _resourceOwnerAuthenticateHelper;
        private readonly IAuthenticateHelper _authenticateHelper;

        public LocalOpenIdUserAuthenticationAction(IResourceOwnerAuthenticateHelper resourceOwnerAuthenticateHelper, IAuthenticateHelper authenticateHelper)
        {
            _resourceOwnerAuthenticateHelper = resourceOwnerAuthenticateHelper;
            _authenticateHelper = authenticateHelper;
        }

        #region Public methods

        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        public async Task<LocalOpenIdAuthenticationResult> Execute(LocalAuthenticationParameter localAuthenticationParameter, AuthorizationParameter authorizationParameter, string code, string issuerName)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationParameter));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var resourceOwner = await _resourceOwnerAuthenticateHelper.Authenticate(localAuthenticationParameter.UserName, localAuthenticationParameter.Password, authorizationParameter.AmrValues).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
            }

            var claims = resourceOwner.Claims == null ? new List<Claim>() : resourceOwner.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer));
            return new LocalOpenIdAuthenticationResult
            {
                ActionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter, code, resourceOwner.Id, claims, issuerName).ConfigureAwait(false),
                Claims = claims,
                TwoFactor = resourceOwner.TwoFactorAuthentication
            };
        }

        #endregion
    }
}
