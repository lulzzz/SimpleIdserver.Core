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

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.WebSite.Authenticate.Actions;
using SimpleIdServer.Core.WebSite.User.Actions;

namespace SimpleIdServer.Core.WebSite.User
{
    public interface IUserActions
    {
        Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal);
        Task<bool> DeleteConsent(string consentId);
        Task<ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal);
        Task<bool> UpdateCredentials(ChangePasswordParameter changePasswordParameter);
        Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims);
        Task<bool> UpdateTwoFactor(string subject, string twoFactorAuth);
        Task<string> AddUser(AddUserParameter addUserParameter, string issuer = null);
    }

    internal class UserActions : IUserActions
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IUpdateUserClaimsOperation _updateUserClaimsOperation;
        private readonly IAddUserOperation _addUserOperation;
        private readonly IUpdateUserTwoFactorAuthenticatorOperation _updateUserTwoFactorAuthenticatorOperation;
        private readonly IChangePasswordAction _changePasswordAction;

        public UserActions(
            IGetConsentsOperation getConsentsOperation,
            IRemoveConsentOperation removeConsentOperation,
            IGetUserOperation getUserOperation,
            IUpdateUserClaimsOperation updateUserClaimsOperation,
            IAddUserOperation addUserOperation,
            IUpdateUserTwoFactorAuthenticatorOperation updateUserTwoFactorAuthenticatorOperation,
            IChangePasswordAction changePasswordAction)
        {
            _getConsentsOperation = getConsentsOperation;
            _removeConsentOperation = removeConsentOperation;
            _getUserOperation = getUserOperation;
            _updateUserClaimsOperation = updateUserClaimsOperation;
            _addUserOperation = addUserOperation;
            _updateUserTwoFactorAuthenticatorOperation = updateUserTwoFactorAuthenticatorOperation;
            _changePasswordAction = changePasswordAction;
        }

        public Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal)
        {
            return _getConsentsOperation.Execute(claimsPrincipal);
        }

        public Task<bool> DeleteConsent(string consentId)
        {
            return _removeConsentOperation.Execute(consentId);
        }

        public Task<ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal)
        {
            return _getUserOperation.Execute(claimsPrincipal);
        }

        public Task<bool> UpdateCredentials(ChangePasswordParameter changePasswordParameter)
        {
            return _changePasswordAction.Execute(changePasswordParameter);
        }

        public Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims)
        {
            return _updateUserClaimsOperation.Execute(subject, claims);
        }

        public Task<bool> UpdateTwoFactor(string subject, string twoFactorAuth)
        {
            return _updateUserTwoFactorAuthenticatorOperation.Execute(subject, twoFactorAuth);
        }

        public Task<string> AddUser(AddUserParameter addUserParameter, string issuer = null)
        {
            return _addUserOperation.Execute(addUserParameter, issuer);
        }
    }
}
