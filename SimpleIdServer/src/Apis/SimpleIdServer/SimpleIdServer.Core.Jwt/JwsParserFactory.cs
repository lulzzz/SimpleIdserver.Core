﻿using SimpleIdServer.Core.Jwt.Signature;

namespace SimpleIdServer.Core.Jwt
{
    public class JwsParserFactory
    {
        public IJwsParser BuildJwsParser()
        {
            var createJwsSignature = new CreateJwsSignature();
            return new JwsParser(createJwsSignature);
        }
    }
}
