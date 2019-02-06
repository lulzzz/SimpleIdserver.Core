using System;
using System.Threading.Tasks;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Results;
using SimpleIdServer.Core.WebSite.Consent.Actions;

namespace SimpleIdServer.Core.WebSite.Consent
{
    public interface IConsentActions
    {
        Task<DisplayContentResult> DisplayConsent(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName);
        Task<ActionResult> ConfirmConsent(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName);
    }

    public class ConsentActions : IConsentActions
    {
        private readonly IDisplayConsentAction _displayConsentAction;
        private readonly IConfirmConsentAction _confirmConsentAction;

        public ConsentActions(
            IDisplayConsentAction displayConsentAction,
            IConfirmConsentAction confirmConsentAction)
        {
            _displayConsentAction = displayConsentAction;
            _confirmConsentAction = confirmConsentAction;
        }

        public async Task<DisplayContentResult> DisplayConsent(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }
            
            if (string.IsNullOrWhiteSpace(authenticatedSubject))
            {
                throw new ArgumentNullException(nameof(authenticatedSubject));
            }

            return await _displayConsentAction.Execute(authorizationParameter, authenticatedSubject, issuerName);
        }

        public async Task<ActionResult> ConfirmConsent(AuthorizationParameter authorizationParameter, string authenticatedSubject, string issuerName)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (string.IsNullOrWhiteSpace(authenticatedSubject))
            {
                throw new ArgumentNullException(nameof(authenticatedSubject));
            }

            return await _confirmConsentAction.Execute(authorizationParameter, authenticatedSubject, issuerName);
        }
    }
}
