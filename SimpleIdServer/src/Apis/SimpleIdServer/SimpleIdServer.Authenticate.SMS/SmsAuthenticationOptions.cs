using SimpleIdServer.Authenticate.Basic;
using SimpleIdServer.Twilio.Client;

namespace SimpleIdServer.Authenticate.SMS
{
    public class SmsAuthenticationOptions : BasicAuthenticateOptions
    {
        public SmsAuthenticationOptions()
        {
            Message = "The confirmation code is {0}";
            TwilioSmsCredentials = new TwilioSmsCredentials();
			IsSelfProvisioningEnabled = false;
        }

        public TwilioSmsCredentials TwilioSmsCredentials { get; set; }
        public string Message { get; set; }
		public bool IsSelfProvisioningEnabled { get; set; }
    }
}