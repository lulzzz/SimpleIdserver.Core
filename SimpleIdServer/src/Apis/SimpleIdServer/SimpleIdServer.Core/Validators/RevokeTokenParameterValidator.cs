using System;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;

namespace SimpleIdServer.Core.Validators
{
    public interface IRevokeTokenParameterValidator
    {
        void Validate(RevokeTokenParameter parameter);
    }

    internal sealed class RevokeTokenParameterValidator : IRevokeTokenParameterValidator
    {
        public void Validate(RevokeTokenParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            // Read this RFC for more information
            if (string.IsNullOrWhiteSpace(parameter.Token))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, Constants.IntrospectionRequestNames.Token));
            }
        }
    }
}
