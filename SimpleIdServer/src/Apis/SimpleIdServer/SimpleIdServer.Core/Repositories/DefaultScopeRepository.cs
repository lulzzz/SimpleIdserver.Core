using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Parameters;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Common.Results;
using SimpleIdServer.Core.Extensions;

namespace SimpleIdServer.Core.Repositories
{
    internal sealed class DefaultScopeRepository : IScopeRepository
    {
        public ICollection<Scope> _scopes;

        public DefaultScopeRepository(ICollection<Scope> scopes)
        {
            _scopes = scopes == null ? new List<Scope>() : scopes;
        }

        public Task<bool> DeleteAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
            if (sc == null)
            {
                return Task.FromResult(false);
            }

            _scopes.Remove(sc);
            return Task.FromResult(true);
        }

        public Task<ICollection<Scope>> GetAllAsync()
        {
            ICollection<Scope> res = _scopes.Select(s => s.Copy()).ToList();
            return Task.FromResult(res);
        }

        public Task<Scope> GetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var scope = _scopes.FirstOrDefault(s => s.Name == name);
            if (scope == null)
            {
                return Task.FromResult((Scope)null);
            }

            return Task.FromResult(scope.Copy());
        }

        public Task<bool> InsertAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.CreateDateTime = DateTime.UtcNow;
            _scopes.Add(scope.Copy());
            return Task.FromResult(true);
        }

        public Task<SearchScopeResult> Search(SearchScopesParameter parameter)
        {
            if(parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<Scope> result = _scopes;
            if (parameter.ScopeNames != null && parameter.ScopeNames.Any())
            {
                result = result.Where(c => parameter.ScopeNames.Any(n => c.Name.Contains(n)));
            }

            if (parameter.Types != null && parameter.Types.Any())
            {
                var scopeTypes = parameter.Types.Select(t => (ScopeType)t);
                result = result.Where(s => scopeTypes.Contains(s.Type));
            }

            var nbResult = result.Count();
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

            return Task.FromResult(new SearchScopeResult
            {
                Content = result.Select(r => r.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<ICollection<Scope>> SearchByNamesAsync(IEnumerable<string> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            ICollection<Scope> result = _scopes.Where(s => names.Contains(s.Name)).Select(s => s.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<bool> UpdateAsync(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var sc = _scopes.FirstOrDefault(s => s.Name == scope.Name);
            if (sc == null)
            {
                return Task.FromResult(false);
            }

            sc.Claims = scope.Claims;
            sc.Description = scope.Description;
            sc.IsDisplayedInConsent = scope.IsDisplayedInConsent;
            sc.IsExposed = scope.IsExposed;
            sc.IsOpenIdScope = scope.IsOpenIdScope;
            sc.Type = scope.Type;
            sc.UpdateDateTime = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
