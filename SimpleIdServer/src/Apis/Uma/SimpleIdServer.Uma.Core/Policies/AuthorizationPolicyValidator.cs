using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Policies
{
    public interface IAuthorizationPolicyValidator
    {
        Task<ResourceValidationResult> IsAuthorized(string openidProvider, Ticket validTicket, ClaimTokenParameter claimTokenParameter);
    }

    internal class AuthorizationPolicyValidator : IAuthorizationPolicyValidator
    {
        private readonly IBasicAuthorizationPolicy _basicAuthorizationPolicy;
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AuthorizationPolicyValidator(
            IBasicAuthorizationPolicy basicAuthorizationPolicy,
            IResourceSetRepository resourceSetRepository,
            IUmaServerEventSource umaServerEventSource)
        {
            _basicAuthorizationPolicy = basicAuthorizationPolicy;
            _resourceSetRepository = resourceSetRepository;
            _umaServerEventSource = umaServerEventSource;
        }

        #region Public methods

        public async Task<ResourceValidationResult> IsAuthorized(string openidProvider, Ticket validTicket, ClaimTokenParameter claimTokenParameter)
        {
            if (string.IsNullOrWhiteSpace(openidProvider))
            {
                throw new ArgumentNullException(nameof(openidProvider));
            }

            if (validTicket == null)
            {
                throw new ArgumentNullException(nameof(validTicket));
            }
            
            if (validTicket.Lines == null || !validTicket.Lines.Any())
            {
                throw new ArgumentNullException(nameof(validTicket.Lines));
            }

            var resourceIds = validTicket.Lines.Select(l => l.ResourceSetId);
            var resources = await _resourceSetRepository.Get(resourceIds);
            if (resources == null || !resources.Any() || resources.Count() != resourceIds.Count())
            {
                throw new BaseUmaException(ErrorCodes.InternalError, ErrorDescriptions.SomeResourcesDontExist);
            }

            ResourceValidationResult validationResult = null;
            foreach (var ticketLine in validTicket.Lines)
            {
                var ticketLineParameter = new TicketLineParameter(ticketLine.Scopes);
                var resource = resources.First(r => r.Id == ticketLine.ResourceSetId);
                validationResult = await Validate(openidProvider, ticketLineParameter, resource, claimTokenParameter).ConfigureAwait(false);
                if (!validationResult.IsValid)
                {
                    _umaServerEventSource.AuthorizationPoliciesFailed(validTicket.Id);
                    return validationResult;
                }
            }

            return validationResult;
        }

        #endregion

        #region Private methods

        private Task<ResourceValidationResult> Validate(string openidProvider, TicketLineParameter ticketLineParameter, ResourceSet resource, ClaimTokenParameter claimTokenParameter)
        {
            if (resource.AuthPolicies == null || !resource.AuthPolicies.Any())
            {
                return Task.FromResult(new ResourceValidationResult
                {
                    IsValid = true
                });
            }

            return _basicAuthorizationPolicy.Execute(openidProvider, ticketLineParameter, resource.AuthPolicies, claimTokenParameter);
        }

        #endregion
    }
}
