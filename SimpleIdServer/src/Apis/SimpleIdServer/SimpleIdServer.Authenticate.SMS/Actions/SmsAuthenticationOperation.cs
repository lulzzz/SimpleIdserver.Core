using SimpleIdServer.Core.Api.User;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.IdentityStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdServer.Authenticate.SMS.Actions
{
    public interface ISmsAuthenticationOperation
    {
        Task<User> Execute(string phoneNumber, string authenticatedUserSubject = null);
    }

    internal sealed class SmsAuthenticationOperation : ISmsAuthenticationOperation
    {
        private readonly IGenerateAndSendSmsCodeOperation _generateAndSendSmsCodeOperation;
        private readonly IUserActions _userActions; 
		private readonly SmsAuthenticationOptions _smsAuthenticationOptions;
		
        public SmsAuthenticationOperation(IGenerateAndSendSmsCodeOperation generateAndSendSmsCodeOperation,IUserActions userActions, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            _generateAndSendSmsCodeOperation = generateAndSendSmsCodeOperation;
            _userActions = userActions;
			_smsAuthenticationOptions = smsAuthenticationOptions;
        }

        public async Task<User> Execute(string phoneNumber, string authenticatedUserSubject = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            // 1. Check user exists
            User user = null;
            if (!string.IsNullOrWhiteSpace(authenticatedUserSubject))
            {
                user = await _userActions.GetUser(authenticatedUserSubject).ConfigureAwait(false);
                if (!user.Claims.Any(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber && c.Value == phoneNumber))
                {
                    throw new InvalidOperationException("the phone number is not valid");
                }
            }
			else
            {
                user = await _userActions.GetUserByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
            }

			if (!_smsAuthenticationOptions.IsSelfProvisioningEnabled && user == null)
			{
				throw new InvalidOperationException("the user doesn't exist");
			}

            // 2. Send the confirmation code (SMS).
            await _generateAndSendSmsCodeOperation.Execute(phoneNumber);
            if (user != null)
            {
                return user;
            }

            // 3. Create a new resource owner.
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber),
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, "false")
            };
            var record = new AddUserParameter(claims);
            await _userActions.AddUser(record).ConfigureAwait(false);            
            return await _userActions.GetUserByClaim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, phoneNumber);
        }
    }
}