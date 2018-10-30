using SimpleIdServer.Core.Common;
using SimpleIdServer.Lib;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SimpleIdServer.Uma.Startup
{
    internal static class DefaultConfiguration
    {
        public static List<JsonWebKey> GetJsonWebKeys()
        {
            var serializedRsa = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlStringNetCore(true);
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlStringNetCore(true);
                }
            }

            return new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new []
                    {
                        KeyOperations.Sign,
                        KeyOperations.Verify
                    },
                    Kid = "1",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa,
                },
                new JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = new []
                    {
                        KeyOperations.Encrypt,
                        KeyOperations.Decrypt
                    },
                    Kid = "2",
                    Kty = KeyType.RSA,
                    Use = Use.Enc,
                    SerializedKey = serializedRsa,
                }
            };
        }
    }
}