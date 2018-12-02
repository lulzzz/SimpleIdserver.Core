using Newtonsoft.Json;
using SimpleIdServer.Uma.Core.Errors;
using SimpleIdServer.Uma.Core.Exceptions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Services;
using SimpleIdServer.Uma.Core.Stores;
using SimpleIdServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdServer.Uma.Core.Api.PermissionController.Actions
{
    internal interface IAddPermissionAction
    {
        Task<string> Execute(IEnumerable<string> audiences, AddPermissionParameter addPermissionParameters);
        Task<string> Execute(IEnumerable<string> audiences, IEnumerable<AddPermissionParameter> addPermissionParameters);
    }

    internal class AddPermissionAction : IAddPermissionAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
        private readonly ITicketStore _ticketStore;
        private readonly IRepositoryExceptionHelper _repositoryExceptionHelper;
        private readonly IUmaConfigurationService _configurationService;
        private readonly IUmaServerEventSource _umaServerEventSource;

        public AddPermissionAction(
            IResourceSetRepository resourceSetRepository,
            ITicketStore ticketStore,
            IRepositoryExceptionHelper repositoryExceptionHelper,
            IUmaConfigurationService configurationService,
            IUmaServerEventSource umaServerEventSource)
        {
            _resourceSetRepository = resourceSetRepository;
            _ticketStore = ticketStore;
            _repositoryExceptionHelper = repositoryExceptionHelper;
            _configurationService = configurationService;
            _umaServerEventSource = umaServerEventSource;
        }

        public async Task<string> Execute(IEnumerable<string> audiences, AddPermissionParameter addPermissionParameter)
        {
            if (audiences == null)
            {
                throw new ArgumentNullException(nameof(audiences));
            }

            if (addPermissionParameter == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameter));
            }

            var result = await Execute(audiences, new[] { addPermissionParameter });
            return result;
        }

        public async Task<string> Execute(IEnumerable<string> audiences, IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            if (audiences == null)
            {
                throw new ArgumentNullException(nameof(audiences));
            }

            if (addPermissionParameters == null)
            {
                throw new ArgumentNullException(nameof(addPermissionParameters));
            }

            var json = addPermissionParameters == null ? string.Empty : JsonConvert.SerializeObject(addPermissionParameters);
            _umaServerEventSource.StartAddPermission(json);
            await CheckAddPermissionParameter(addPermissionParameters);
            var ticketLifetimeInSeconds = await _configurationService.GetTicketLifeTime();
            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString(),
                Audiences = audiences,
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = ticketLifetimeInSeconds,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(ticketLifetimeInSeconds)
            };
            var ticketLines = new List<TicketLine>();
            foreach(var addPermissionParameter in addPermissionParameters)
            {
                ticketLines.Add(new TicketLine
                {
                    Id = Guid.NewGuid().ToString(),
                    Scopes = addPermissionParameter.Scopes,
                    ResourceSetId = addPermissionParameter.ResourceSetId
                });
            }

            ticket.Lines = ticketLines;
            if(!await _ticketStore.AddAsync(ticket))
            {
                throw new BaseUmaException(ErrorCodes.InternalError, ErrorDescriptions.TheTicketCannotBeInserted);
            }

            _umaServerEventSource.FinishAddPermission(json);
            return ticket.Id;
        }

        private async Task CheckAddPermissionParameter(IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            // 1. Get resource sets.
            var resourceSets = await _repositoryExceptionHelper.HandleException(ErrorDescriptions.TheResourceSetsCannotBeRetrieved,
                () => _resourceSetRepository.Get(addPermissionParameters.Select(p => p.ResourceSetId)));

            // 2. Check parameters & scope exist.
            foreach (var addPermissionParameter in addPermissionParameters)
            {
                if (string.IsNullOrWhiteSpace(addPermissionParameter.ResourceSetId))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.ResourceSetId));
                }

                if (addPermissionParameter.Scopes == null ||
                    !addPermissionParameter.Scopes.Any())
                {
                    throw new BaseUmaException(ErrorCodes.InvalidRequestCode, string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, Constants.AddPermissionNames.Scopes));
                }

                var resourceSet = resourceSets.FirstOrDefault(r => addPermissionParameter.ResourceSetId == r.Id);
                if (resourceSet == null)
                {
                    throw new BaseUmaException(ErrorCodes.InvalidResourceSetId, string.Format(ErrorDescriptions.TheResourceSetDoesntExist, addPermissionParameter.ResourceSetId));
                }

                if (resourceSet.Scopes == null ||
                    addPermissionParameter.Scopes.Any(s => !resourceSet.Scopes.Contains(s)))
                {
                    throw new BaseUmaException(ErrorCodes.InvalidScope, ErrorDescriptions.TheScopeAreNotValid);
                }
            }
        }
    }
}
