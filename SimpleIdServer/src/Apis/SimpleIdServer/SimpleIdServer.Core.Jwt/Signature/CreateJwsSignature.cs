﻿#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Extensions;
using SimpleIdServer.Core.Jwt.Serializer;

namespace SimpleIdServer.Core.Jwt.Signature
{
    public interface ICreateJwsSignature
    {
        string SignWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string combinedJwsNotSigned);
        bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature);
#if NET461
        string SignWithEllipseCurve(string serializedKeys,
           string combinedJwsNotSigned);
         bool VerifyWithEllipticCurve(string serializedKeys,
            string message,
            byte[] signature);
#endif
    }

    public class CreateJwsSignature : ICreateJwsSignature
    {
        private readonly IEnumerable<JwsAlg> _supportedAlgs = new List<JwsAlg>
        {
            JwsAlg.RS256,
            JwsAlg.RS384,
            JwsAlg.RS512
        };

        private readonly Dictionary<JwsAlg, string> _mappingWinJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, string>
        {
            {
                JwsAlg.RS256, "SHA256"
            },
            {
                JwsAlg.RS384, "SHA384"
            },
            {
                JwsAlg.RS512, "SHA512"
            }
        };
        private readonly Dictionary<JwsAlg, HashAlgorithmName> _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, HashAlgorithmName>
        {
            {
                JwsAlg.RS256, HashAlgorithmName.SHA256
            },
            {
                JwsAlg.RS384, HashAlgorithmName.SHA384
            },
            {
                JwsAlg.RS512, HashAlgorithmName.SHA512
            }
        };

#if NET461
        private readonly ICngKeySerializer _cngKeySerializer;

        public CreateJwsSignature(ICngKeySerializer cngKeySerializer)
        {
            _cngKeySerializer = cngKeySerializer;
        }
#endif

        public CreateJwsSignature()
        {
        }

#region Rsa agorithm

        public string SignWithRsa(
            JwsAlg algorithm, 
            string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (!_supportedAlgs.Contains(algorithm))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var hashMethod = _mappingWinJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    var bytesToBeSigned = ASCIIEncoding.ASCII.GetBytes(combinedJwsNotSigned);
                    rsa.FromXmlStringNetCore(serializedKeys);
                    var byteToBeConverted = rsa.SignData(bytesToBeSigned, hashMethod);
                    return byteToBeConverted.Base64EncodeBytes();
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    var hashMethod = _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    var bytesToBeSigned = ASCIIEncoding.ASCII.GetBytes(combinedJwsNotSigned);
                    rsa.FromXmlStringNetCore(serializedKeys);
                    var byteToBeConverted = rsa.SignData(bytesToBeSigned, 0, bytesToBeSigned.Length, hashMethod, RSASignaturePadding.Pkcs1);
                    return byteToBeConverted.Base64EncodeBytes();
                }
            }
        }

        public bool VerifyWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string input,
            byte[] signature)
        {
            if (!_supportedAlgs.Contains(algorithm))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            var plainBytes = ASCIIEncoding.ASCII.GetBytes(input);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var hashMethod = _mappingWinJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    rsa.FromXmlStringNetCore(serializedKeys);
                    return rsa.VerifyData(plainBytes, hashMethod, signature);
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    var hashMethod = _mappingLinuxJwsAlgorithmToRsaHashingAlgorithms[algorithm];
                    rsa.FromXmlStringNetCore(serializedKeys);
                    return rsa.VerifyData(plainBytes, signature, hashMethod, RSASignaturePadding.Pkcs1);
                }

            }
        }

        #endregion

#region Elliptic Curve algorithm

#if NET461

        public string SignWithEllipseCurve(string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (string.IsNullOrWhiteSpace(combinedJwsNotSigned))
            {
                throw new ArgumentNullException("combinedJwsNotSigned");
            }

            var cngKey = _cngKeySerializer.DeserializeCngKeyWithPrivateKey(serializedKeys);
            var plainTextBytes = Encoding.UTF8.GetBytes(combinedJwsNotSigned);
            using (var ec = new ECDsaCng(cngKey))
            {
                return ec
                    .SignData(plainTextBytes, 0, plainTextBytes.Count())
                    .Base64EncodeBytes();
                // return ec.SignData(plainTextBytes).Base64EncodeBytes();
            }
        }

        public bool VerifyWithEllipticCurve(string serializedKeys,
            string message,
            byte[] signature)
        {
            if (string.IsNullOrWhiteSpace(serializedKeys))
            {
                throw new ArgumentNullException("serializedKeys");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("message");
            }

            if (signature == null || !signature.Any())
            {
                throw new ArgumentNullException("signature");
            }

            var cngKey = _cngKeySerializer.DeserializeCngKeyWithPrivateKey(serializedKeys);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var ecsdaSignature = new ECDsaCng(cngKey))
            {
                return ecsdaSignature.VerifyData(messageBytes,
                    0,
                    messageBytes.Length,
                    signature);
                // return ecsdaSignature.VerifyData(messageBytes,
                //    signature);
            }
        }
#endif

#endregion
    }
}
