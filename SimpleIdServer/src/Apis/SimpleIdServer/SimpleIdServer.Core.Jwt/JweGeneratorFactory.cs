using SimpleIdServer.Core.Jwt.Encrypt;
using SimpleIdServer.Core.Jwt.Encrypt.Encryption;

namespace SimpleIdServer.Core.Jwt
{
    public class JweGeneratorFactory
    {
        public IJweGenerator BuildJweGenerator()
        {
            return new JweGenerator(new JweHelper(new AesEncryptionHelper()));
        }
    }
}