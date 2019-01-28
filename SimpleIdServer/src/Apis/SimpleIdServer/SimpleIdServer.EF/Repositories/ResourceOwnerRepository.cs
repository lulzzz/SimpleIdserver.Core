using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Common.Results;
using SimpleIdServer.EF.Extensions;
using SimpleIdServer.EF.Models;
using SimpleIdServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains = SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.EF.Repositories
{
    public sealed class ResourceOwnerRepository : IResourceOwnerRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly ITechnicalEventSource _managerEventSource;

        public ResourceOwnerRepository(SimpleIdentityServerContext context, ITechnicalEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<Domains.ResourceOwner> GetResourceOwnerByClaim(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            try
            {
                var result = await _context.ResourceOwners.Include(r => r.Claims).Include(r => r.Credentials)
                    .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == key && c.Value == value))
                    .ConfigureAwait(false);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch(Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }
        
        public async Task<Domains.ResourceOwner> GetAsync(string id)
        {
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                var result = await _context.ResourceOwners
                    .Include(r => r.Claims)
                    .Include(r => r.Credentials)
                    .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code && c.Value == id))
                    .ConfigureAwait(false);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<ICollection<Domains.ResourceOwner>> GetAsync(IEnumerable<System.Security.Claims.Claim> claims)
        {
            if (claims == null)
            {
                return new List<Domains.ResourceOwner>();
            }

            return await _context.ResourceOwners
                .Include(r => r.Claims)
                .Include(r => r.Credentials)
                .Where(r => claims.All(c => r.Claims.Any(sc => sc.Value == c.Value && sc.ClaimCode == c.Type)))
                .Select(u => u.ToDomain())
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<ICollection<Domains.ResourceOwner>> GetAllAsync()
        {
            return await _context.ResourceOwners.Select(u => u.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                var record = await _context.ResourceOwners
                   .Include(r => r.Claims)
                   .Include(r => r.Credentials)
                   .Include(r => r.Consents)
                   .FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code && c.Value == id));
                if (record == null)
                {
                    return false;
                }

                _context.ResourceOwners.Remove(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }
        }

        public async Task<bool> InsertAsync(Domains.ResourceOwner resourceOwner)
        {
            try
            {
                var user = new ResourceOwner
                {
                    Id = resourceOwner.Id,
                    IsBlocked = resourceOwner.IsBlocked,
                    TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication,
                    Claims = new List<ResourceOwnerClaim>(),
                    CreateDateTime = DateTime.UtcNow,
                    UpdateDateTime = DateTime.UtcNow
                };

                if (resourceOwner.Claims != null)
                {
                    foreach (var claim in resourceOwner.Claims)
                    {
                        user.Claims.Add(new ResourceOwnerClaim
                        {
                            Id = Guid.NewGuid().ToString(),
                            ResourceOwnerId = user.Id,
                            ClaimCode = claim.Type,
                            Value = claim.Value
                        });
                    }
                }

                _context.ResourceOwners.Add(user);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateAsync(Domains.ResourceOwner resourceOwner)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.ResourceOwners
                        .Include(r => r.Claims)
                        .FirstOrDefaultAsync(r => r.Id == resourceOwner.Id).ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    record.IsBlocked = resourceOwner.IsBlocked;
                    record.TwoFactorAuthentication = resourceOwner.TwoFactorAuthentication;
                    record.UpdateDateTime = DateTime.UtcNow;
                    record.Claims = new List<ResourceOwnerClaim>();
                    _context.ResourceOwnerClaims.RemoveRange(record.Claims);
                    if (resourceOwner.Claims != null)
                    {
                        foreach (var claim in resourceOwner.Claims)
                        {
                            record.Claims.Add(new ResourceOwnerClaim
                            {
                                Id = Guid.NewGuid().ToString(),
                                ResourceOwnerId = record.Id,
                                ClaimCode = claim.Type,
                                Value = claim.Value
                            });
                        }
                    }

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            try
            {
                var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
                if (claimIdentifier == null)
                {
                    throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
                }

                IQueryable<Models.ResourceOwner> result = _context.ResourceOwners
                    .Include(r => r.Claims)
                    .Include(r => r.Credentials);

                if (parameter.Subjects != null)
                {                    
                    result = result.Where(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code 
                        && parameter.Subjects.Any(s => c.Value.Contains(s))));
                }

                if (result == null)
                {
                    return null;
                }

                var nbResult = await result.CountAsync().ConfigureAwait(false);
                if (parameter.Order != null)
                {
                    switch (parameter.Order.Target)
                    {
                        case "update_datetime":
                            switch (parameter.Order.Type)
                            {
                                case OrderTypes.Asc:
                                    result = result.OrderBy(c => c.UpdateDateTime);
                                    break;
                                case OrderTypes.Desc:
                                    result = result.OrderByDescending(c => c.UpdateDateTime);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    result = result.OrderByDescending(c => c.UpdateDateTime);
                }

                if (parameter.IsPagingEnabled)
                {
                    result = result.Skip(parameter.StartIndex).Take(parameter.Count);
                }

                return new SearchResourceOwnerResult
                {
                    Content = result.Select(r => r.ToDomain()),
                    StartIndex = parameter.StartIndex,
                    TotalResults = nbResult
                };
            }
            catch (Exception ex)
            {
                _managerEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<bool> UpdateCredential(string subject, Domains.ResourceOwnerCredential credential)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            var claimIdentifier = await _context.Claims.FirstOrDefaultAsync(c => c.IsIdentifier).ConfigureAwait(false);
            if (claimIdentifier == null)
            {
                throw new InvalidOperationException("no claim can be used to uniquely identified the resource owner");
            }

            var user = await _context.ResourceOwners.Include(r => r.Credentials).FirstOrDefaultAsync(r => r.Claims.Any(c => c.ClaimCode == claimIdentifier.Code && c.Value == subject))
                    .ConfigureAwait(false);
            if (user == null)
            {
                return false;
            }

            var cred = user.Credentials.FirstOrDefault(c => c.Type == credential.Type);
            if (cred == null)
            {
                cred = new ResourceOwnerCredential
                {
                    Type = credential.Type
                };
                user.Credentials.Add(cred);
            }

            cred.BlockedDateTime = credential.BlockedDateTime;
            cred.ExpirationDateTime = credential.ExpirationDateTime;
            cred.FirstAuthenticationFailureDateTime = credential.FirstAuthenticationFailureDateTime;
            cred.IsBlocked = credential.IsBlocked;
            cred.NumberOfAttempts = credential.NumberOfAttempts;
            cred.ResourceOwnerId = user.Id;
            cred.Value = credential.Value;
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
    }
}
