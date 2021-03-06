﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    internal sealed class DefaultUserClaimsEnricher : IUserClaimsEnricher
    {
        public Task Enrich(List<Claim> claims)
        {
            return Task.FromResult(0);
        }
    }
}