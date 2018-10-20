﻿using System.Threading.Tasks;
using SimpleIdServer.Common.Client;
using SimpleIdServer.UserManagement.Client.Operations;
using SimpleIdServer.UserManagement.Client.Results;
using SimpleIdServer.UserManagement.Common.Requests;

namespace SimpleIdServer.UserManagement.Client
{
    public interface IProfileClient
    {
        Task<BaseResponse> LinkMyProfile(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> LinkProfile(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null);
        Task<BaseResponse> UnlinkMyProfile(string requestUrl, string externalSubject, string authorizationHeaderValue = null);
        Task<BaseResponse> UnlinkProfile(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null);
        Task<GetProfilesResult> GetMyProfiles(string requestUrl, string authorizationHeaderValue = null);
        Task<GetProfilesResult> GetProfiles(string requestUrl, string currentSubject, string authorizationHeaderValue = null);
    }

    internal sealed class ProfileClient : IProfileClient
    {
        private readonly ILinkProfileOperation _linkProfileOperation;
        private readonly IUnlinkProfileOperation _unlinkProfileOperation;
        private readonly IGetProfilesOperation _getProfilesOperation;

        public ProfileClient(ILinkProfileOperation linkProfileOperation, IUnlinkProfileOperation unlinkProfileOperation, IGetProfilesOperation getProfilesOperation)
        {
            _linkProfileOperation = linkProfileOperation;
            _unlinkProfileOperation = unlinkProfileOperation;
            _getProfilesOperation = getProfilesOperation;
        }

        public Task<BaseResponse> LinkMyProfile(string requestUrl, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            return _linkProfileOperation.Execute(requestUrl, linkProfileRequest, authorizationHeaderValue);
        }

        public Task<BaseResponse> LinkProfile(string requestUrl, string currentSubject, LinkProfileRequest linkProfileRequest, string authorizationHeaderValue = null)
        {
            return _linkProfileOperation.Execute(requestUrl, currentSubject, linkProfileRequest, authorizationHeaderValue);
        }

        public Task<BaseResponse> UnlinkMyProfile(string requestUrl, string externalSubject, string authorizationHeaderValue = null)
        {
            return _unlinkProfileOperation.Execute(requestUrl, externalSubject, authorizationHeaderValue);
        }

        public Task<BaseResponse> UnlinkProfile(string requestUrl, string externalSubject, string currentSubject, string authorizationHeaderValue = null)
        {
            return _unlinkProfileOperation.Execute(requestUrl, externalSubject, currentSubject, authorizationHeaderValue);
        }

        public Task<GetProfilesResult> GetMyProfiles(string requestUrl, string authorizationHeaderValue = null)
        {
            return _getProfilesOperation.Execute(requestUrl, authorizationHeaderValue);
        }

        public Task<GetProfilesResult> GetProfiles(string requestUrl, string currentSubject, string authorizationHeaderValue = null)
        {
            return _getProfilesOperation.Execute(requestUrl, currentSubject, authorizationHeaderValue);
        }
    }
}
