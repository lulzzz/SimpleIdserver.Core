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

using SimpleIdentityServer.Scim.Core.EF;
using SimpleIdentityServer.Scim.Core.EF.Stores;
using SimpleIdentityServer.Scim.Core.Stores;
using System;

namespace SimpleIdentityServer.Scim.Core.Tests.Fixture
{
    public class StoresFixture : IDisposable
    {
        public StoresFixture()
        {
            SchemaStore = new DefaultSchemaStore(new System.Collections.Generic.List<EF.Models.Schema>
            {
                DefaultSchemas.GroupSchema,
                DefaultSchemas.UserSchema
            });
        }

        public ISchemaStore SchemaStore { get; private set; }

        public void Dispose()
        {
            SchemaStore.Dispose();
        }
    }
}
