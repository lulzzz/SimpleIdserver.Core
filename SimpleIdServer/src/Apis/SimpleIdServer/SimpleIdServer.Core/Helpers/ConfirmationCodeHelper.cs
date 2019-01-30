using SimpleIdServer.Store;
using System;
using System.Threading.Tasks;

namespace SimpleIdServer.Core.Helpers
{
    public interface IConfirmationCodeHelper
    {
        Task<bool> Remove(string confirmationCode);
        Task<bool> Validate(string code);
    }

    internal sealed class ConfirmationCodeHelper : IConfirmationCodeHelper
    {
        private readonly IConfirmationCodeStore _confirmationCodeStore;

        public ConfirmationCodeHelper(IConfirmationCodeStore confirmationCodeStore)
        {
            _confirmationCodeStore = confirmationCodeStore;
        }

        public Task<bool> Remove(string confirmationCode)
        {
            if (string.IsNullOrWhiteSpace(confirmationCode))
            {
                throw new ArgumentNullException(nameof(confirmationCode));
            }

            return _confirmationCodeStore.Remove(confirmationCode);
        }

        public async Task<bool> Validate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            var confirmationCode = await _confirmationCodeStore.Get(code).ConfigureAwait(false);
            if (confirmationCode == null)
            {
                return false;
            }

            var expirationDateTime = confirmationCode.IssueAt.AddSeconds(confirmationCode.ExpiresIn);
            return DateTime.UtcNow < expirationDateTime;
        }
    }
}
