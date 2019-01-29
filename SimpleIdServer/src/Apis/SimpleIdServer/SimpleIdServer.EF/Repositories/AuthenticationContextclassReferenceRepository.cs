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
    }
}
