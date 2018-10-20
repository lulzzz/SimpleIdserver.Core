using System;

namespace SimpleIdServer.Twilio.Client
{
    public class TwilioException : Exception
    {
        public TwilioException(string message) : base(message) { }
    }
}
