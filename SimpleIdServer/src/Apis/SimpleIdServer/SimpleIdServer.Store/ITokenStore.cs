﻿using System.Threading.Tasks;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;

namespace SimpleIdServer.Store
{
    public interface ITokenStore
    {
        /// <summary>
        /// Try to get a valid access token.
        /// </summary>
        /// <param name="scopes"></param>
        /// <param name="clientId"></param>
        /// <param name="idTokenJwsPayload"></param>
        /// <param name="userInfoJwsPayload"></param>
        /// <returns></returns>
        Task<GrantedToken> GetToken(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload);
        Task<GrantedToken> GetRefreshToken(string GetRefreshToken);
        Task<GrantedToken> GetAccessToken(string accessToken);
        Task<bool> AddToken(GrantedToken grantedToken);
        Task<bool> RemoveRefreshToken(string refreshToken);
        Task<bool> RemoveAccessToken(string accessToken);
        Task<bool> Clean();
    }
}