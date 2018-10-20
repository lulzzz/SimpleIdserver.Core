using SimpleIdServer.Core.Jwt.Serializer;
using SimpleIdServer.Core.Jwt.Signature;

namespace SimpleIdServer.Core.Jwt
{
    public class JwsGeneratorFactory
    {
        public IJwsGenerator BuildJwsGenerator()
        {
            ICreateJwsSignature createJwsSignature;
#if NET461
            createJwsSignature = new CreateJwsSignature(new CngKeySerializer());
#else
            createJwsSignature = new CreateJwsSignature();
#endif
            return new JwsGenerator(createJwsSignature);
        }
    }
}