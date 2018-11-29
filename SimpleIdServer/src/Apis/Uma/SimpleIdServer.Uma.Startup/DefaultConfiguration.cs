using SimpleIdServer.Core.Common;
using SimpleIdServer.Uma.Core.Models;
using System;
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

        public static List<ResourceSet> GetResources()
        {
            return new List<ResourceSet>
            {
                new ResourceSet
                {
                    Id = "1",
                    IconUri = "https://www.sri.edu.au/wp-content/uploads/2016/10/Gym-Icon.png",
                    Name = "mulan_picture",
                    Type = "picture",
                    Scopes = new List<string>
                    {
                        "read", "write", "delete"
                    },
                    Owner = "administrator",
                    Uri = "http://localhost/mulan.png"
                }
            };
        }

        public static List<Policy> GetPolicies()
        {
            return new List<Policy>
             {
                 new Policy
                 {
                     Id = Guid.NewGuid().ToString(),
                     ResourceSetIds = new List<string> { "1" },
                     Rules = new List<PolicyRule>
                     {
                         new PolicyRule
                         {
                             Id = Guid.NewGuid().ToString(),
                             Claims = new List<Claim>
                             {
                                 new Claim
                                 {
                                     Type = "sub",
                                     Value = "03"
                                 },
                                 new Claim
                                 {
                                     Type = "role",
                                     Value = "user"
                                 },
                             },
                             Scopes = new List<string>
                             {
                                 "read"
                             }
                         },
                         new PolicyRule
                         {
                             Id = Guid.NewGuid().ToString(),
                             Claims = new List<Claim>
                             {
                                 new Claim
                                 {
                                     Type = "sub",
                                     Value = "04"
                                 },
                                 new Claim
                                 {
                                     Type = "role",
                                     Value = "user"
                                 },
                             },
                             Scopes = new List<string>
                             {
                                 "write", "delete"
                             }
                         }
                     }
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