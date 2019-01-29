using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.WebSite.User;

namespace SimpleIdServer.Authenticate.SMS.Actions
{
    public interface ISmsAuthenticationOperation
    {
        Task<ResourceOwner> Execute(string phoneNumber, string authenticatedUserSubject = null);
    }

    internal sealed class SmsAuthenticationOperation : ISmsAuthenticationOperation
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IUserActions _userActions; 
		private readonly SmsAuthenticationOptions _smsAuthenticationOptions;
		
        public SmsAuthenticationOperation(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation, IResourceOwnerRepository resourceOwnerRepository, IUserActions userActions, ISubjectBuilder subjectBuilder,
            SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _resourceOwnerRepository = resourceOwnerRepository;
            _userActions = userActions;
			_smsAuthenticationOptions = smsAuthenticationOptions;
        }

        public async Task<ResourceOwner> Execute(string phoneNumber, string authenticatedUserSubject = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            // 1. Check user exists
            ResourceOwner resourceOwner = null;
            if (!string.IsNullOrWhiteSpace(authenticatedUserSubject))
            {
                resourceOwner = await _resourceOwnerRepository.GetAsync(authenticatedUserSubject).ConfigureAwait(false);
                if (!resourceOwner.Claims.Any(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber && c.Value == phoneNumber))
                {
                    throw new InvalidOperationException("the phone number is not valid");
                }
            }
			else
            {
                resourceOwner = await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
            }

			if (!_smsAuthenticationOptions.IsSelfProvisioningEnabled && resourceOwner == null)
			{
				throw new InvalidOperationException("the user doesn't exist");
			}

            // 2. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber);
            if (resourceOwner != null)
            {
                return resourceOwner;
            }

            // 3. Create a new resource owner.
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false")
            };
            var record = new AddUserParameter(Guid.NewGuid().ToString(), claims);
            await _userActions.AddUser(record).ConfigureAwait(false);            
            return await _resourceOwnerRepository.GetResourceOwnerByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
        }
    }
}