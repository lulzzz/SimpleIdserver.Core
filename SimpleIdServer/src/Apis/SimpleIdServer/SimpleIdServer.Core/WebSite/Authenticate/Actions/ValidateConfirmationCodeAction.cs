using System;
using System.Threading.Tasks;
using SimpleIdServer.Store;

namespace SimpleIdServer.Core.WebSite.Authenticate.Actions
{
    public interface IValidateConfirmationCodeAction
    {
        Task<bool> Execute(string code);
    }

    internal class ValidateConfirmationCodeAction : IValidateConfirmationCodeAction
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public ValidateConfirmationCodeAction(IConfirmationCodeStore confirmationCodeStore)
        {
            _confirmationCodeStore = confirmationCodeStore;
        }

        public async Task<bool> Execute(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var confirmationCode = await _confirmationCodeStore.Get(code);
            if (confirmationCode == null)
            {
                return false;
            }

            var expirationDateTime = confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn);
            return DateTime.UtcNow < expirationDateTime;
        }
    }
}
