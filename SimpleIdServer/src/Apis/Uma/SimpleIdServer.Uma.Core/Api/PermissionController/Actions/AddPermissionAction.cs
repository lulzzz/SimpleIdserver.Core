#region copyright
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

namespace SimpleIdServer.Uma.Core.Api.PermissionController.Actions
{
    internal interface IAddPermissionAction
    {
        Task<string> Execute(string clientId, AddPermissionParameter addPermissionParameters);
        Task<string> Execute(string clientId, IEnumerable<AddPermissionParameter> addPermissionParameters);
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

        public async Task<string> Execute(string clientId, AddPermissionParameter addPermissionParameter)
        {
            var result = await Execute(clientId, new[] { addPermissionParameter });
            return result;
        }

        public async Task<string> Execute(string clientId, IEnumerable<AddPermissionParameter> addPermissionParameters)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(clientId);
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
                ClientId = clientId,
                CreateDateTime = DateTime.UtcNow,
                ExpiresIn = ticketLifetimeInSeconds,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(ticketLifetimeInSeconds)
            };
            var ticketLines = new List<TicketLine>();
            foreach(var addPermissionParameter in addPermissionParameters) // TH : ONE TICKET FOR MULTIPLE PERMISSIONS.
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
