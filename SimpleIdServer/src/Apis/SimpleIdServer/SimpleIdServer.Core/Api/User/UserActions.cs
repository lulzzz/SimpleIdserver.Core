using SimpleIdServer.Core.Api.User.Actions;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.User
{
    public interface IUserActions
    {
        Task<IEnumerable<Consent>> GetConsents(string subject);
        Task<bool> DeleteConsent(string consentId);
        Task<IdentityStore.Models.User> GetUser(string subject);
        Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims);
        Task<string> AddUser(AddUserParameter addUserParameter, string issuer = null);
        Task<IdentityStore.Models.User> GetUserByClaim(string claimKey, string claimValue);
        Task<IdentityStore.Models.User> GetUserByCredentials(string credentialType, string value);
        Task<bool> AddCredentials(IEnumerable<AddUserCredentialParameter> addUserCredentialParameterLst);
        Task<bool> UpdateCredential(UpdateUserCredentialParameter updateUserCredentialParameter);
    }

    internal class UserActions : IUserActions
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IUpdateUserClaimsOperation _updateUserClaimsOperation;
        private readonly IAddUserOperation _addUserOperation;
        private readonly IGetUserByClaimOperation _getUserByClaimOperation;
        private readonly IGetUserByCredentialOperation _getUserByCredentialOperation;
        private readonly IAddUserCredentialsOperation _addUserCredentialsOperation;
        private readonly IUpdateUserCredentialOperation _updateUserCredentialOperation;

        public UserActions(
            IGetConsentsOperation getConsentsOperation,
            IRemoveConsentOperation removeConsentOperation,
            IGetUserOperation getUserOperation,
            IUpdateUserClaimsOperation updateUserClaimsOperation,
            IAddUserOperation addUserOperation,
            IGetUserByClaimOperation getUserByClaimOperation,
            IGetUserByCredentialOperation getUserByCredentialOperation,
            IAddUserCredentialsOperation addUserCredentialsOperation,
            IUpdateUserCredentialOperation updateUserCredentialOperation)
        {
            _getConsentsOperation = getConsentsOperation;
            _removeConsentOperation = removeConsentOperation;
            _getUserOperation = getUserOperation;
            _updateUserClaimsOperation = updateUserClaimsOperation;
            _addUserOperation = addUserOperation;
            _getUserByClaimOperation = getUserByClaimOperation;
            _getUserByCredentialOperation = getUserByCredentialOperation;
            _addUserCredentialsOperation = addUserCredentialsOperation;
            _updateUserCredentialOperation = updateUserCredentialOperation;
        }

        public Task<IEnumerable<Consent>> GetConsents(string subject)
        {
            return _getConsentsOperation.Execute(subject);
        }

        public Task<bool> DeleteConsent(string consentId)
        {
            return _removeConsentOperation.Execute(consentId);
        }

        public Task<IdentityStore.Models.User> GetUser(string subject)
        {
            return _getUserOperation.Execute(subject);
        }

        public Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims)
        {
            return _updateUserClaimsOperation.Execute(subject, claims);
        }

        public Task<string> AddUser(AddUserParameter addUserParameter, string issuer = null)
        {
            return _addUserOperation.Execute(addUserParameter, issuer);
        }

        public Task<IdentityStore.Models.User> GetUserByClaim(string claimKey, string claimValue)
        {
            return _getUserByClaimOperation.Execute(claimKey, claimValue);
        }

        public Task<IdentityStore.Models.User> GetUserByCredentials(string credentialType, string value)
        {
            return _getUserByCredentialOperation.Execute(credentialType, value);
        }

        public Task<bool> AddCredentials(IEnumerable<AddUserCredentialParameter> addUserCredentialParameterLst)
        {
            return _addUserCredentialsOperation.Execute(addUserCredentialParameterLst);
        }

        public Task<bool> UpdateCredential(UpdateUserCredentialParameter updateUserCredentialParameter)
        {
            return _updateUserCredentialOperation.Execute(updateUserCredentialParameter);
        }
    }
}
