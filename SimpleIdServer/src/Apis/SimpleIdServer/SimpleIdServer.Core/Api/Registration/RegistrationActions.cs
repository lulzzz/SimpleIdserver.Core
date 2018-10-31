using SimpleIdServer.Bus;
using SimpleIdServer.Core.Api.Registration.Actions;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Dtos.Responses;
using SimpleIdServer.OAuth.Events;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Api.Registration
{
    public interface IRegistrationActions
    {
        Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter);
    }

    public class RegistrationActions : IRegistrationActions
    {
        private readonly IRegisterClientAction _registerClientAction;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPayloadSerializer _payloadSerializer;

        public RegistrationActions(IRegisterClientAction registerClientAction, IEventPublisher eventPublisher, IPayloadSerializer payloadSerializer)
        {
            _registerClientAction = registerClientAction;
            _eventPublisher = eventPublisher;
            _payloadSerializer = payloadSerializer;
        }

        public async Task<ClientRegistrationResponse> PostRegistration(RegistrationParameter registrationParameter)
        {
            if (registrationParameter == null)
            {
                throw new ArgumentNullException(nameof(registrationParameter));
            }

            var processId = Guid.NewGuid().ToString();
            try
            {
                _eventPublisher.Publish(new RegistrationReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(registrationParameter), 0));
                var result = await _registerClientAction.Execute(registrationParameter).ConfigureAwait(false);
                _eventPublisher.Publish(new RegistrationResultReceived(Guid.NewGuid().ToString(), processId, _payloadSerializer.GetPayload(result), 1));
                return result;
            }
            catch(IdentityServerException ex)
            {
                _eventPublisher.Publish(new OAuthErrorReceived(Guid.NewGuid().ToString(), processId, ex.Code, ex.Message, 1));
                throw;
            }
        }
    }
}
