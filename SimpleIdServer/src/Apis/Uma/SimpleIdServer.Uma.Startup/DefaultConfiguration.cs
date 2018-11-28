using SimpleIdServer.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SimpleIdServer.Uma.Startup
{
    internal static class DefaultConfiguration
    {
        public static List<JsonWebKey> GetJsonWebKeys()
        {
            var xml = GetXml();
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
                    SerializedKey = xml,
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
                    SerializedKey = xml
                }
            };
        }

        private static string GetXml()
        {
            var locationPath = GetLocationPath();
            var privateKeyLocationPath = Path.Combine(locationPath, "prk.txt");
            return File.ReadAllText(privateKeyLocationPath);
        }

        private static string GetLocationPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}