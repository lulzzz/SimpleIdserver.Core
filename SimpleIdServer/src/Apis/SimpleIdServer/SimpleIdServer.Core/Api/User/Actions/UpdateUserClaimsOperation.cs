using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IUpdateUserClaimsOperation
    {
        Task<bool> Execute(string subject, IEnumerable<ClaimAggregate> claims);
    }

    internal class UpdateUserClaimsOperation : IUpdateUserClaimsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;

        public UpdateUserClaimsOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IClaimRepository claimRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
        }
        
        public async Task<bool> Execute(string subject, IEnumerable<ClaimAggregate> claims)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var resourceOwner = await _resourceOwnerRepository.GetAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            var supportedClaims = await _claimRepository.GetAllAsync();
            claims = claims.Where(c => supportedClaims.Any(sp => sp.Code == c.Code && !Jwt.Constants.NotEditableResourceOwnerClaimNames.Contains(c.Code)));
            var claimsToBeRemoved = resourceOwner.Claims
                .Where(cl => claims.Any(c => c.Code == cl.Type))
                .Select(cl => resourceOwner.Claims.IndexOf(cl))
                .OrderByDescending(p => p)
                .ToList();
            foreach(var index in claimsToBeRemoved)
            {
                resourceOwner.Claims.RemoveAt(index);
            }         
            
            foreach(var claim in claims)
            {
                if (string.IsNullOrWhiteSpace(claim.Value))
                {
                    continue;
                }

                resourceOwner.Claims.Add(new Claim(claim.Code, claim.Value));
            }

            Claim updatedClaim;
            if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            resourceOwner.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner);
        }
    }
}
