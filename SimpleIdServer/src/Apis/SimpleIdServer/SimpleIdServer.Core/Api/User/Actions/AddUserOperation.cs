﻿using SimpleIdServer.AccountFilter;
using SimpleIdServer.Core.Api.Profile.Actions;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User.Actions
{
    public interface IAddUserOperation
    {
        Task<string> Execute(AddUserParameter addUserParameter, string issuer = null);
    }
    
    public class AddUserOperation : IAddUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAccountFilter _accountFilter;
        private readonly ILinkProfileAction _linkProfileAction;
        private readonly IOpenIdEventSource _openidEventSource;
        private readonly IEnumerable<IUserClaimsEnricher> _userClaimsEnricherLst;
        private readonly ISubjectBuilder _subjectBuilder;
        private readonly IAddUserCredentialsOperation _addUserCredentialsOperation;

        public AddUserOperation(IResourceOwnerRepository resourceOwnerRepository, 
            IClaimRepository claimRepository,
            ILinkProfileAction linkProfileAction,
            IAccountFilter accountFilter, 
            IOpenIdEventSource openIdEventSource,
            IEnumerable<IUserClaimsEnricher> userClaimsEnricherLst,
            ISubjectBuilder subjectBuilder,
            IAddUserCredentialsOperation addUserCredentialsOperation)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _linkProfileAction = linkProfileAction;
            _accountFilter = accountFilter;
            _openidEventSource = openIdEventSource;
            _userClaimsEnricherLst = userClaimsEnricherLst;
            _subjectBuilder = subjectBuilder;
            _addUserCredentialsOperation = addUserCredentialsOperation;
        }

        public async Task<string> Execute(AddUserParameter addUserParameter, string issuer = null)
        {
            if (addUserParameter == null)
            {
                throw new ArgumentNullException(nameof(addUserParameter));
            }
            
            var subject = await _subjectBuilder.BuildSubject().ConfigureAwait(false);
            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.GetAsync(subject) != null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.UnhandledExceptionCode, Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var newClaims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };

            // 2. Populate the claims.
            var existedClaims = await _claimRepository.GetAllAsync().ConfigureAwait(false);
            if (addUserParameter.Claims != null)
            {
                foreach (var claim in addUserParameter.Claims)
                {
                    if (!newClaims.Any(nc => nc.Type == claim.Type) && existedClaims.Any(c => c.Code == claim.Type))
                    {
                        newClaims.Add(claim);
                    }
                }
            }
            
            var isFilterValid = true;
            var userFilterResult = await _accountFilter.Check(newClaims).ConfigureAwait(false);
            if (!userFilterResult.IsValid)
            {
                isFilterValid = false;
                foreach(var ruleResult in userFilterResult.AccountFilterRules)
                {
                    if (!ruleResult.IsValid)
                    {
                        _openidEventSource.Failure($"the filter rule '{ruleResult.RuleName}' failed");
                        foreach (var errorMessage in ruleResult.ErrorMessages)
                        {
                            _openidEventSource.Failure(errorMessage);
                        }
                    }
                }
            }

            if (!isFilterValid)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheUserIsNotAuthorized);
            }

            // 3. Add the scim resource.
            if (_userClaimsEnricherLst != null)
            {
                foreach(var userClaimsEnricher in _userClaimsEnricherLst)
                {
                    await userClaimsEnricher.Enrich(newClaims).ConfigureAwait(false);
                }
            }

            // 4. Add the resource owner.
            var newResourceOwner = new ResourceOwner
            {
                Id = subject,
                Claims = newClaims,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow,
                IsBlocked = false
            };                        
            if (!await _resourceOwnerRepository.InsertAsync(newResourceOwner).ConfigureAwait(false))
            {
                throw new IdentityServerException(Errors.ErrorCodes.UnhandledExceptionCode, Errors.ErrorDescriptions.TheResourceOwnerCannotBeAdded);
            }

            // 5. Add credentials.
            if (addUserParameter.Credentials != null)
            {
                foreach(var c in addUserParameter.Credentials)
                {
                    c.UserId = subject;
                }

                await _addUserCredentialsOperation.Execute(addUserParameter.Credentials).ConfigureAwait(false);
            }

            // 6. Link to a profile.
            if (!string.IsNullOrWhiteSpace(issuer))
            {
                await _linkProfileAction.Execute(subject, addUserParameter.ExternalLogin, issuer).ConfigureAwait(false);
            }

            _openidEventSource.AddResourceOwner(newResourceOwner.Id);
            return subject;
        }
    }
}