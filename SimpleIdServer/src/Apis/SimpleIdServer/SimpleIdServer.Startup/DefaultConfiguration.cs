using SimpleIdServer.Core.Common;
using SimpleIdServer.Lib;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Helpers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SimpleIdServer.Startup
{
    public static class DefaultConfiguration
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

            serializedRsa = "<RSAKeyValue><Modulus>t91zX9do7VnkX9ZO5UDjbyriZEdB8ATFid9j7NrVV9Ej024xK1J2kTyUDNS8MAc+jBgYe9OcVmXz/Ctn9SZVwV+6ksbZUuzOvQty3c1u6viu6lJjg9lUfJLVaL+RPbzHzyVr007pvYeHwfhsILLYQO4IhoLwNjJY99M/1Vmk/8E=</Modulus><Exponent>AQAB</Exponent><P>y6TrTIB3uBehTIWw5uFIz3/ELNM6nSYP/0Pkfe+HC4VBMrMXlj28PSb9cnQIL71EU85QKcoDUjPDYeUoF5RuPw==</P><Q>5yK+ArbJGexcqWYdfZ5yAVPqWEg1Mmz1BbAaOC3qZhN0JSly7TOBFjqFXDiifmg5bTS7H4q5+Wx5OCAWEkcR/w==</Q><DP>vrb0pfCqLf3zUXbi9VaGmc1OK6ymeAXtdWJf2pE4J9Hj/Vc7/7hRUfPx5/5CrHLUSqgs6vYFpjZUBJpXsb2QgQ==</DP><DQ>YBkhxx8YHZ8YJ5Y9TK1D2Sl6lZnwBDco6GR/gjwU6LvN3mWNUvHHCebq65zgco4C0lTKOCMFj556B8vPYWoLIQ==</DQ><InverseQ>FFs7vMX4zmCrMMHvY/iuPtf8wGWDGBacqZl1bdZBeBwmSuvw2YQk5/sru5avevFigTlQYqTGoEWLPZM1gzn0/A==</InverseQ><D>t6KOq8dp/bzNMcbKT4AKZypOqFbfDUjGvpgFpjc94xJ3lKC2rQ0UbKQzPclvFwz1NFiQg4Pq3gO/tjjoAFnERMcIaZNZpHlry4NFvGdSlScJeGXjulewCacD3rMIG59vsHa0x8CoXms3U9hvaADzRan+kn8Lv6m4EQ3sbbmFpz0=</D></RSAKeyValue>";
            return  new List<JsonWebKey>
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

        public static List<Core.Common.Models.Client> GetClients()
        {
            return new List<Core.Common.Models.Client>
            {
                new Core.Common.Models.Client
                {
                    ClientId = "a2195dd9-d9e0-4e1b-bf29-41226d3af072",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "80932880-6170-47c8-9345-b204a4172577"
                        }
                    },
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "profile"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    ApplicationType = ApplicationTypes.web,
                    RedirectionUrls = new List<string>
                    {
                        "http://localhost:4200/Home/Callback"
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code,
                        GrantType.password
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code,
                        ResponseType.id_token,
                        ResponseType.token
                    }
                },
                new Core.Common.Models.Client
                {
                    ClientId = "clientId",
                    Secrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Type = ClientSecretTypes.SharedSecret,
                            Value = "pwd"
                        }
                    },
                    AllowedScopes = new List<Scope>
                    {
                        new Scope
                        {
                            Name = "openid"
                        },
                        new Scope
                        {
                            Name = "profile"
                        }
                    },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                    ApplicationType = ApplicationTypes.native,
                    RedirectionUrls = new List<string>
                    {
                        "com.sidxamarin.apps:/oauth2redirect"
                    },
                    GrantTypes = new List<GrantType>
                    {
                        GrantType.authorization_code
                    },
                    ResponseTypes = new List<ResponseType>
                    {
                        ResponseType.code
                    },
                    RequirePkce = true
                }
            };
        }

        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new List<Claim>
                    {
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator"),
                        new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role, "[ 'administrator', 'role']")
                    },
                    Password = PasswordHelper.ComputeHash("password"),
                    IsLocalAccount = true
                }
            };
        }

        public static List<Translation> GetTranslations()
        {
            return new List<Translation>
            {
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                    Value = "the client {0} would like to access"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                    Value = "individual claims"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.NameCode,
                    Value = "Name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginCode,
                    Value = "Login"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                    Value = "Password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                    Value = "Username"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                    Value = "Confirm"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.CancelCode,
                    Value = "Cancel"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                    Value = "Login with your local account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                    Value = "Login with your external account"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                    Value = "policy"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Tos,
                    Value = "Terms of Service"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendCode,
                    Value = "Send code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Code,
                    Value = "Code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.EditResourceOwner,
                    Value = "Edit resource owner"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourName,
                    Value = "Your name"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourPassword,
                    Value = "Your password"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Email,
                    Value = "Email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.YourEmail,
                    Value = "Your email"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                    Value = "Two authentication factor"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.UserIsUpdated,
                    Value = "User has been updated"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                    Value = "Send a confirmation code"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.Phone,
                    Value = "Phone"
                },
                new Translation
                {
                    LanguageTag = "en",
                    Code = Core.Constants.StandardTranslationCodes.HashedPassword,
                    Value = "Hashed password"
                }
            };
        }
    }
}