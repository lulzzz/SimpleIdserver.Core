using System.Security.Cryptography;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Uma.Host.Tests.Fakes;

namespace SimpleIdServer.Uma.Host.Tests
{
    public class SharedContext
    {
        private static SharedContext _instance;

        private SharedContext()
        {
            var serializedRsa = "<RSAKeyValue><Modulus>zU2tcIiC9z0bS0gNjMTX0g8+XBqPnsZGiD2nP7XOGGHGZf2yXkINMpGZbUsxuTW9ltLW67OTr0rjlvpZ5urUl9AyasHhK81UusbK2tFuaGD5bjkRdFc26+G4W260hMOzmMOuEVDc2iGcQADc2a/2j8T+Ee5tkHuI6sEOd7am+tE=</Modulus><Exponent>AQAB</Exponent><P>6I8ZdC8Fxxk/YSRCQ3zazEj6l53gGdbhxd9Hopzr4R5y4FTxZJi2ShC785whyioM1wD922AwBl6faXfg+sDHFw==</P><Q>4f9IIeyA+0mAPQikNhwqUYyLpj7EaBxhpOQgUnCPZRTGHVxFDyYLWHkLoOTOd0J/lLa7KS5RXp5Ma9FMw8p+Vw==</Q><DP>O6H+D+nC3IPf2aP3jeClJj8MavZjsZyFNj0D3HHKlmY9ZMLDR11VWPaji1sc2v8fXb52Wdt3VRrMW7oOqZ3nLw==</DP><DQ>We465vz06on6FM9+gOW+VUsnOxVZFNDObk41KnkOJrwYhhB0jq2l8CPi47iJDF4S5Lu+SInc6Vj2siTMdlD66w==</DQ><InverseQ>BdGAn1jFT/Rf6Rap+IRKszo16jY/q/ak94QxJQ8xHytUrVmmS7moShGIbfppXRGsJnfx9oPHFsS8mZOWcYYboA==</InverseQ><D>qLoPMa4vnEwPM3abFDbufIfko0N9B2tCqlOpMZYUNDufF1FCF29Hc2jv5D/pNKLzFpJe6cVjOaxdkUZdPod+gXFPV+4ueC6fot/FuPu6tuKGDvCIfcpy2XAybitr2UPYjl2kKqeRRWah4nCDtTwQlyH2y7ohhT/tfGt+cAVFmHU=</D></RSAKeyValue>";
            SignatureKey = new JsonWebKey
            {
                Alg = AllAlg.RS256,
                KeyOps = new KeyOperations[]
                {
                    KeyOperations.Sign,
                    KeyOperations.Verify
                },
                Kid = "11",
                Kty = KeyType.RSA,
                Use = Use.Sig,
                SerializedKey = serializedRsa,
            };
            EncryptionKey = new JsonWebKey
            {
                Alg = AllAlg.RSA1_5,
                KeyOps = new[]
                {
                    KeyOperations.Decrypt,
                    KeyOperations.Encrypt
                },
                Kid = "10",
                Kty = KeyType.RSA,
                Use = Use.Enc,
                SerializedKey = serializedRsa,
            };
            HttpClientFactory = FakeHttpClientFactory.Instance;
        }

        public static SharedContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SharedContext();
                }

                return _instance;
            }
        }

        public JsonWebKey EncryptionKey { get; }
        public JsonWebKey SignatureKey { get; }
        public FakeHttpClientFactory HttpClientFactory { get; }
    }
}
