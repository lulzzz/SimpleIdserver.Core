using System.Collections.Generic;
using System.Security.Claims;
using SimpleIdServer.Core.Jwt.Extensions;

namespace SimpleIdServer.Core.Jwt.Mapping
{
    public interface IClaimsMapping
    {
        Dictionary<string, object> MapToOpenIdClaims(IEnumerable<Claim> claims);
    }

    public class ClaimsMapping : IClaimsMapping
    {
        public Dictionary<string, object> MapToOpenIdClaims(IEnumerable<Claim> claims)
        {
            var result = new Dictionary<string, object>();
            foreach (var claim in claims)
            {
                if (Constants.MapWifClaimsToOpenIdClaims.ContainsKey(claim.Type))
                {
                    result.Add(Constants.MapWifClaimsToOpenIdClaims[claim.Type], claim.GetClaimValue());
                }
                else
                {
                    result.Add(claim.Type, claim.GetClaimValue());
                }
            }
            
            return result;
        }
    }
}