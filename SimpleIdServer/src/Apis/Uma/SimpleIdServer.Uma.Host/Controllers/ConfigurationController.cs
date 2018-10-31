using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Uma.Common.DTOs;
using SimpleIdServer.Uma.Host.Extensions;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Configuration)]
    public class ConfigurationController : Controller
    {
        private readonly List<string> _umaProfilesSupported = new List<string>
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };

        [HttpGet]
        public IActionResult GetConfiguration()
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                ClaimTokenProfilesSupported = new List<string>(),
                UmaProfilesSupported = _umaProfilesSupported,
                ResourceRegistrationEndpoint = $"{issuer}{Constants.RouteValues.ResourceSet}",
                PermissionEndpoint = $"{issuer}{Constants.RouteValues.Permission}",
                PoliciesEndpoint = $"{issuer}{Constants.RouteValues.Policies}",
                Issuer = issuer,
                AuthorizationEndpoint = null,
                TokenEndpoint = $"{issuer}{Constants.RouteValues.Token}",
                JwksUri = $"{issuer}{Constants.RouteValues.Jwks}",
                RegistrationEndpoint = $"{issuer}{Constants.RouteValues.Registration}",
                IntrospectionEndpoint = $"{issuer}{Constants.RouteValues.Introspection}",
                RevocationEndpoint = $"{issuer}{Constants.RouteValues.Token}/revoke",
            };
            return new OkObjectResult(result);
        }
    }
}
