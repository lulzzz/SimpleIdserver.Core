﻿#region copyright
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
using SimpleIdServer.EF.Models;
using SimpleIdServer.Logging;

namespace SimpleIdServer.EF.Repositories
{
    public sealed class ConsentRepository : IConsentRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly ITechnicalEventSource _managerEventSource;

        public ConsentRepository(SimpleIdentityServerContext context, ITechnicalEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }
        
        public async Task<IEnumerable<Core.Common.Models.Consent>> GetConsentsForGivenUserAsync(string subject)
        {
            var resourceOwnerClaim = await _context.ResourceOwnerClaims
                .Include(r => r.Claim)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.ConsentClaims)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.ConsentScopes).ThenInclude(r => r.Scope)
                .Include(r => r.ResourceOwner).ThenInclude(r => r.Consents).ThenInclude(r => r.Client)
                .Where(r => r.Claim != null && r.Claim.IsIdentifier == true && r.Value == subject)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (resourceOwnerClaim == null)
            {
                return null;
            }

            return resourceOwnerClaim.ResourceOwner.Consents == null ? new Core.Common.Models.Consent[0] : resourceOwnerClaim.ResourceOwner.Consents.Select<Consent, Core.Common.Models.Consent>(c => c.ToDomain());
        }

        public async Task<bool> InsertAsync(Core.Common.Models.Consent record)
        {
            Core.Common.Models.Consent result = null;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var clientId = record.Client.ClientId;
                    var resourceOwnerId = record.ResourceOwner.Id;
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId).ConfigureAwait(false);
                    var resourceOwner = await _context.ResourceOwners.FirstOrDefaultAsync(r => r.Id == resourceOwnerId).ConfigureAwait(false);
                    var assignedClaims = new List<ConsentClaim>();
                    var assignedScopes = new List<ConsentScope>();
                    if (record.Claims != null)
                    {
                        var claimCodes = record.Claims;
                        var codes = await _context.Claims.Where(c => claimCodes.Contains(c.Code)).Select(c => c.Code).ToListAsync().ConfigureAwait(false);
                        foreach (var code in codes)
                        {
                            assignedClaims.Add(new ConsentClaim
                            {
                                ClaimCode = code
                            });
                        }
                    }

                    if (record.GrantedScopes != null)
                    {
                        var scopeNames = record.GrantedScopes.Select(g => g.Name);
                        var names = await _context.Scopes.Where(s => scopeNames.Contains(s.Name)).Select(s => s.Name).ToListAsync().ConfigureAwait(false);
                        foreach (var name in names)
                        {
                            assignedScopes.Add(new ConsentScope
                            {
                                ScopeName = name
                            });
                        }
                    }

                    var newConsent = new Consent
                    {
                        Id = record.Id,
                        Client = client,
                        ResourceOwner = resourceOwner,
                        ConsentClaims = assignedClaims,
                        ConsentScopes = assignedScopes
                    };

                    var insertedConsent = _context.Consents.Add(newConsent);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    result = insertedConsent.Entity.ToDomain();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Core.Common.Models.Consent record)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var consent = await _context.Consents.FirstOrDefaultAsync(c => c.Id == record.Id).ConfigureAwait(false);
                    if (consent == null)
                    {
                        return false;
                    }

                    _context.Consents.Remove(consent);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }
    }
}
