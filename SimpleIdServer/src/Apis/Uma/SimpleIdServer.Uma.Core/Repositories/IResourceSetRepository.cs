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

using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Parameters;

namespace SimpleIdServer.Uma.Core.Repositories
{
    public interface IResourceSetRepository
    {
        Task<SearchResourceSetResult> Search(SearchResourceSetParameter parameter);
        Task<bool> Insert(ResourceSet resourceSet);
        Task<ResourceSet> Get(string id);
        Task<bool> Update(ResourceSet resourceSet);
        Task<ICollection<ResourceSet>> GetAll();
        Task<bool> Delete(string id);
        Task<IEnumerable<ResourceSet>> Get(IEnumerable<string> ids);
    }
}