#region copyright
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

using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Uma.Client.Configuration;
using SimpleIdServer.Uma.Client.Permission;
using SimpleIdServer.Uma.Client.Policy;
using SimpleIdServer.Uma.Client.ResourceSet;

namespace SimpleIdServer.Uma.Client
{
    public interface IIdentityServerUmaClientFactory
    {
        IPermissionClient GetPermissionClient();
        IResourceSetClient GetResourceSetClient();
        IPolicyClient GetPolicyClient();
    }

    public class IdentityServerUmaClientFactory : IIdentityServerUmaClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IdentityServerUmaClientFactory()
        {
            _httpClientFactory = new HttpClientFactory();
        }

        public IdentityServerUmaClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IPermissionClient GetPermissionClient()
        {
            return new PermissionClient(new AddPermissionsOperation(_httpClientFactory), new GetConfigurationOperation(_httpClientFactory));
        }

        public IResourceSetClient GetResourceSetClient()
        {
            return new ResourceSetClient(new AddResourceSetOperation(_httpClientFactory), new DeleteResourceSetOperation(_httpClientFactory),
                new GetResourcesOperation(_httpClientFactory), new GetResourceOperation(_httpClientFactory), new UpdateResourceOperation(_httpClientFactory),
                new GetConfigurationOperation(_httpClientFactory), new SearchResourcesOperation(_httpClientFactory));
        }

        public IPolicyClient GetPolicyClient()
        {
            return new PolicyClient(new AddPolicyOperation(_httpClientFactory), new GetPolicyOperation(_httpClientFactory),
                new DeletePolicyOperation(_httpClientFactory), new GetPoliciesOperation(_httpClientFactory), new AddResourceToPolicyOperation(_httpClientFactory),
                new DeleteResourceFromPolicyOperation(_httpClientFactory), new UpdatePolicyOperation(_httpClientFactory), new GetConfigurationOperation(_httpClientFactory),
                new SearchPoliciesOperation(_httpClientFactory));
        }
    }
}
