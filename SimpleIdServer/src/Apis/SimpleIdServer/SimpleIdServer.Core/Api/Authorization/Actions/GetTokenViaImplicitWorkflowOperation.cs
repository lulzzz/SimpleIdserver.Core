﻿#region copyright
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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using SimpleIdServer.Core.Api.Authorization.Common;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.OAuth.Logging;

namespace SimpleIdServer.Core.Api.Authorization.Actions
{
    public interface IGetTokenViaImplicitWorkflowOperation
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Core.Common.Models.Client client, string issuerName);
    }

    public class GetTokenViaImplicitWorkflowOperation : IGetTokenViaImplicitWorkflowOperation
    {
        private readonly IProcessAuthorizationRequest _processAuthorizationRequest;
        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;
        private readonly IClientValidator _clientValidator;
        private readonly IOAuthEventSource _oAuthEventSource;

        public GetTokenViaImplicitWorkflowOperation(
            IProcessAuthorizationRequest processAuthorizationRequest,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IClientValidator clientValidator,
            IOAuthEventSource oAuthEventSource)
        {
            _processAuthorizationRequest = processAuthorizationRequest;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _oAuthEventSource = oAuthEventSource;
            _clientValidator = clientValidator;
        }

        public async Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, IPrincipal principal, Core.Common.Models.Client client, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(authorizationParameter.Nonce))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.StandardAuthorizationRequestParameterNames.NonceName),
                    authorizationParameter.State);
            }

            _oAuthEventSource.StartImplicitFlow(
                authorizationParameter.ClientId, 
                authorizationParameter.Scope,
                authorizationParameter.Claims == null ? string.Empty : authorizationParameter.Claims.ToString());

            if (!_clientValidator.CheckGrantTypes(client, GrantType.@implicit))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        authorizationParameter.ClientId,
                        "implicit"),
                    authorizationParameter.State);
            }

            var result = await _processAuthorizationRequest.ProcessAsync(authorizationParameter, principal as ClaimsPrincipal, client, issuerName).ConfigureAwait(false);
            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var claimsPrincipal = principal as ClaimsPrincipal;
                await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, claimsPrincipal, client, issuerName);
            }
            
            var actionTypeName = Enum.GetName(typeof(TypeActionResult), result.Type);
            _oAuthEventSource.EndImplicitFlow(
                authorizationParameter.ClientId,
                actionTypeName,
                result.RedirectInstruction == null ? string.Empty : Enum.GetName(typeof(IdentityServerEndPoints), result.RedirectInstruction.Action));

            return result;
        }
    }
}
