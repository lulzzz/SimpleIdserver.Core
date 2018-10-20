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

using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;

namespace SimpleIdServer.Uma.Core.Api.ResourceSetController
{
    public interface IResourceSetActions
    {
        Task<string> AddResourceSet(AddResouceSetParameter addResouceSetParameter);
        Task<ResourceSet> GetResourceSet(string id);
        Task<bool> UpdateResourceSet(UpdateResourceSetParameter updateResourceSetParameter);
        Task<bool> RemoveResourceSet(string resourceSetId);
        Task<IEnumerable<string>> GetAllResourceSet();
        Task<IEnumerable<string>> GetPolicies(string resourceId);
        Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter);
    }

    internal class ResourceSetActions : IResourceSetActions
    {
        private readonly IAddResourceSetAction _addResourceSetAction;
        private readonly IGetResourceSetAction _getResourceSetAction;
        private readonly IUpdateResourceSetAction _updateResourceSetAction;
        private readonly IDeleteResourceSetAction _deleteResourceSetAction;
        private readonly IGetAllResourceSetAction _getAllResourceSetAction;
        private readonly IGetPoliciesAction _getPoliciesAction;
        private readonly ISearchResourceSetOperation _searchResourceSetOperation;

        public ResourceSetActions(
            IAddResourceSetAction addResourceSetAction,
            IGetResourceSetAction getResourceSetAction,
            IUpdateResourceSetAction updateResourceSetAction,
            IDeleteResourceSetAction deleteResourceSetAction,
            IGetAllResourceSetAction getAllResourceSetAction,
            IGetPoliciesAction getPoliciesAction,
            ISearchResourceSetOperation searchResourceSetOperation)
        {
            _addResourceSetAction = addResourceSetAction;
            _getResourceSetAction = getResourceSetAction;
            _updateResourceSetAction = updateResourceSetAction;
            _deleteResourceSetAction = deleteResourceSetAction;
            _getAllResourceSetAction = getAllResourceSetAction;
            _getPoliciesAction = getPoliciesAction;
            _searchResourceSetOperation = searchResourceSetOperation;
        }

        public Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter)
        {
            return _searchResourceSetOperation.Execute(parameter);
        }

        public Task<string> AddResourceSet(AddResouceSetParameter addResouceSetParameter)
        {
            return _addResourceSetAction.Execute(addResouceSetParameter);
        }

        public Task<ResourceSet> GetResourceSet(string id)
        {
            return _getResourceSetAction.Execute(id);
        }

        public Task<bool> UpdateResourceSet(UpdateResourceSetParameter updateResourceSetParameter)
        {
            return _updateResourceSetAction.Execute(updateResourceSetParameter);
        }

        public Task<bool> RemoveResourceSet(string resourceSetId)
        {
            return _deleteResourceSetAction.Execute(resourceSetId);
        }

        public Task<IEnumerable<string>> GetAllResourceSet()
        {
            return _getAllResourceSetAction.Execute();
        }

        public Task<IEnumerable<string>> GetPolicies(string resourceId)
        {
            return _getPoliciesAction.Execute(resourceId);
        }
    }
}
