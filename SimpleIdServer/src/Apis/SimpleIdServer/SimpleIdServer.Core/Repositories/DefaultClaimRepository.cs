﻿using System;
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
    internal sealed class DefaultClaimRepository : IClaimRepository
    {
        public ICollection<ClaimAggregate> _claims;

        public DefaultClaimRepository(ICollection<ClaimAggregate> claims)
        {
            _claims = claims == null ? new List<ClaimAggregate>() : claims;
        }

        public Task<bool> Delete(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var claim = _claims.FirstOrDefault(c => c.Code == code);
            if (claim == null)
            {
                return Task.FromResult(false);
            }

            _claims.Remove(claim);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<ClaimAggregate>> GetAllAsync()
        {
            return Task.FromResult((IEnumerable<ClaimAggregate>)_claims.Select(c => c.Copy()));
        }

        public Task<ClaimAggregate> GetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var res = _claims.FirstOrDefault(c => c.Code == name);
            if (res == null)
            {
                return Task.FromResult((ClaimAggregate)null);
            }

            return Task.FromResult(res.Copy());
        }

        public Task<bool> InsertAsync(AddClaimParameter claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            _claims.Add(new ClaimAggregate
            {
                Code = claim.Code,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            });
            return Task.FromResult(true);
        }

        public Task<SearchClaimsResult> Search(SearchClaimsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IEnumerable<ClaimAggregate> result = _claims;
            if (parameter.ClaimKeys != null)
            {
                result = result.Where(c => parameter.ClaimKeys.Any(ck => c.Code.Contains(ck)));
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

            return Task.FromResult(new SearchClaimsResult
            {
                Content = result.Select(c => c.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> Update(ClaimAggregate claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var result = _claims.FirstOrDefault(c => c.Code == claim.Code);
            if (result == null)
            {
                return Task.FromResult(false);
            }

            result.Value = claim.Value;
            result.UpdateDateTime = claim.UpdateDateTime;
            return Task.FromResult(true);
        }
    }
}
