using SimpleIdServer.Core.Common.Models;
using System;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Host
{
    internal static class DefaultConfiguration
    {
        public static List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "uma_protection",
                Description = "Access to UMA permission, resource set",
                IsOpenIdScope = false,
                IsDisplayedInConsent = false,
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "register_client",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeType.ProtectedApi,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow
            }
        };
    }
}