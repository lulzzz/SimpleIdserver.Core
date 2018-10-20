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
using System.Linq;
using SimpleIdServer.Core.Common;

namespace SimpleIdServer.Core.Jwt.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Convert a string into JWS algorithm.
        /// </summary>
        /// <param name="alg">String to be converted</param>
        /// <returns>JWS algorithm</returns>
        public static JwsAlg ToJwsAlg(this string alg)
        {
            var algName = Enum.GetNames(typeof(JwsAlg))
                .SingleOrDefault(a => a.ToLowerInvariant() == alg.ToLowerInvariant());
            if (algName == null)
            {
                return JwsAlg.RS256;
            }

            return (JwsAlg)Enum.Parse(typeof(JwsAlg), algName);
        }
    }
}
