﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SimpleIdServer.Core.Api.UserInfo;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Host.Extensions;

namespace SimpleIdServer.Host.Controllers.Api
{
    [Route(Constants.EndPoints.UserInfo)]
    public class UserInfoController : Controller
    {
        private readonly IUserInfoActions _userInfoActions;

        public UserInfoController(IUserInfoActions userInfoActions)
        {
            _userInfoActions = userInfoActions;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return await ProcessRequest();
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            return await ProcessRequest();
        }

        private async Task<ActionResult> ProcessRequest()
        {
            var accessToken = await TryToGetTheAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
            }

            var result = await _userInfoActions.GetUserInformation(accessToken);
            return result.Content;
        }

        private async Task<string> TryToGetTheAccessToken()
        {
            var accessToken = GetAccessTokenFromAuthorizationHeader();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            accessToken = await GetAccessTokenFromBodyParameter();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            return GetAccessTokenFromQueryString();
        }

        /// <summary>
        /// Get an access token from the authorization header.
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromAuthorizationHeader()
        {
            const string authorizationName = "Authorization";
            StringValues values;
            if (!Request.Headers.TryGetValue(authorizationName, out values)) {
                return string.Empty;
            }

            var authenticationHeader = values.First();
            var authorization = AuthenticationHeaderValue.Parse(authenticationHeader);
            var scheme = authorization.Scheme;
            if (string.Compare(scheme, "Bearer", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                return string.Empty;
            }

            return authorization.Parameter;
        }

        /// <summary>
        /// Get an access token from the body parameter.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenFromBodyParameter()
        {
            const string contentTypeName = "Content-Type";
            const string contentTypeValue = "application/x-www-form-urlencoded";
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var emptyResult = string.Empty;
            StringValues values;
            if (Request.Headers == null 
                || !Request.Headers.TryGetValue(contentTypeName, out values))
            {
                return emptyResult;
            }

            var contentTypeHeader = values.First();
            if (string.Compare(contentTypeHeader, contentTypeValue) !=  0)
            {
                return emptyResult;
            }

            var content = await Request.ReadAsStringAsync();
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(content);
            if (!queryString.Keys.Contains(accessTokenName))
            {
                return emptyResult;
            }

            var result = default(StringValues);
            queryString.TryGetValue(accessTokenName, out result);
            return result.First();
        }

        /// <summary>
        /// Get an access token from the query string
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromQueryString()
        {
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var query = Request.Query;
            var record = query.FirstOrDefault(q => q.Key == accessTokenName);
            if (record.Equals(default(KeyValuePair<string, StringValues>))) {
                return string.Empty;
            } else {
                return record.Value.First();
            }
        }
    }
}