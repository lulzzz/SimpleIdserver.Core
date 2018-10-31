using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Services;
using System;
using System.Linq;

namespace SimpleIdServer.Core.Authenticate
{
    public interface IClientSecretPostAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);
        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientSecretPostAuthentication : IClientSecretPostAuthentication
    {
        private readonly IClientPasswordService _clientPasswordService;

        public ClientSecretPostAuthentication(IClientPasswordService clientPasswordService)
        {
            _clientPasswordService = clientPasswordService;
        }

        public Core.Common.Models.Client AuthenticateClient(AuthenticateInstruction instruction, Core.Common.Models.Client client)
        {
            if (instruction == null || client == null)
            {
                throw new ArgumentNullException("the instruction or client parameter cannot be null");
            }

            if (client.Secrets == null)
            {
                return null;
            }

            var clientSecret = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.SharedSecret);
            if (clientSecret == null)
            {
                return null;
            }
            
            var sameSecret = string.Compare(clientSecret.Value, _clientPasswordService.Encrypt(instruction.ClientSecretFromHttpRequestBody), StringComparison.CurrentCultureIgnoreCase) == 0;
            return sameSecret ? client : null;
        }
        
        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("the instruction parameter cannot be null");
            }

            return instruction.ClientIdFromHttpRequestBody;
        }
    }
}
