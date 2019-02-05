#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Linq;
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
            foreach(var groupedClaim in claims.GroupBy(c => c.Type))
            {
                object record;
                if (groupedClaim.Count() == 1)
                {
                    record = groupedClaim.First().GetClaimValue();
                }
                else
                {
                    var lst = new List<object>();
                    foreach (var gClaim in groupedClaim)
                    {
                        lst.Add(gClaim.GetClaimValue());
                    }

                    record = lst;
                }

                var claim = groupedClaim.First();
                if (Constants.MapWifClaimsToOpenIdClaims.ContainsKey(claim.Type))
                {
                    result.Add(Constants.MapWifClaimsToOpenIdClaims[claim.Type], record);
                }
                else
                {
                    result.Add(claim.Type, record);
                }
            }
            
            return result;
        }
    }
}