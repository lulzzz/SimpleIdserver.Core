using Microsoft.EntityFrameworkCore;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.EF.Repositories
{
    internal sealed class AuthenticationContextclassReferenceRepository : IAuthenticationContextclassReferenceRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public AuthenticationContextclassReferenceRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        public async Task<bool> Add(AuthenticationContextclassReference acr)
        {
            _context.AuthenticationContextclassReferences.Add(new Models.AuthenticationContextclassReference
            {
                AmrLst = acr.AmrLst == null ? string.Empty : string.Join(",", acr.AmrLst),
                DisplayName =  acr.DisplayName,
                Name = acr.Name,
                IsDefault = acr.IsDefault,
                Type = (int)acr.Type
            });
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Delete(string name)
        {
            var result = await _context.AuthenticationContextclassReferences.FirstOrDefaultAsync(c => c.Name == name).ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            _context.AuthenticationContextclassReferences.Remove(result);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<IEnumerable<AuthenticationContextclassReference>> Get()
        {
            var result = await _context.AuthenticationContextclassReferences.ToListAsync().ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.Select(s => s.ToDomain());
        }

        public async Task<AuthenticationContextclassReference> Get(string name)
        {
            var result = await _context.AuthenticationContextclassReferences.FirstOrDefaultAsync(a => a.Name == name).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<IEnumerable<AuthenticationContextclassReference>> Get(IEnumerable<string> names)
        {
            var result = await _context.AuthenticationContextclassReferences.Where(a => names.Contains(a.Name)).ToListAsync().ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.Select(s => s.ToDomain());
        }

        public async Task<AuthenticationContextclassReference> GetDefault()
        {
            var result = await _context.AuthenticationContextclassReferences.FirstOrDefaultAsync(a => a.IsDefault).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<bool> Update(AuthenticationContextclassReference acr)
        {
            var result = await _context.AuthenticationContextclassReferences.FirstOrDefaultAsync(a => a.Name == acr.Name).ConfigureAwait(false);
            if (result == null)
            {
                return false;
            }

            result.AmrLst = acr.AmrLst == null ? string.Empty : string.Join(",", acr.AmrLst);
            result.DisplayName = acr.DisplayName;
            result.Type = (int)acr.Type;
            return true;
        }
    }
}
