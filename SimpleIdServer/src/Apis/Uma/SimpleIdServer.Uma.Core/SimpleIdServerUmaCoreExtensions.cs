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

using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Uma.Core.Api.PermissionController;
using SimpleIdServer.Uma.Core.Api.PermissionController.Actions;
using SimpleIdServer.Uma.Core.Api.PolicyController;
using SimpleIdServer.Uma.Core.Api.PolicyController.Actions;
using SimpleIdServer.Uma.Core.Api.ResourceSetController;
using SimpleIdServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdServer.Uma.Core.Api.Token;
using SimpleIdServer.Uma.Core.Api.Token.Actions;
using SimpleIdServer.Uma.Core.Helpers;
using SimpleIdServer.Uma.Core.JwtToken;
using SimpleIdServer.Uma.Core.Models;
using SimpleIdServer.Uma.Core.Policies;
using SimpleIdServer.Uma.Core.Repositories;
using SimpleIdServer.Uma.Core.Services;
using SimpleIdServer.Uma.Core.Stores;
using SimpleIdServer.Uma.Core.Validators;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Core
{
    public static class SimpleIdServerUmaCoreExtensions
    {
        public static IServiceCollection AddSimpleIdServerUmaCore(this IServiceCollection serviceCollection, UmaConfigurationOptions umaConfigurationOptions = null, ICollection<ResourceSet> resources = null, ICollection<Policy> policies = null)
        {
            RegisterDependencies(serviceCollection, umaConfigurationOptions, resources, policies);
            return serviceCollection;
        }

        private static void RegisterDependencies(IServiceCollection serviceCollection, UmaConfigurationOptions umaConfigurationOptions = null, ICollection<ResourceSet> resources = null, ICollection<Policy> policies = null)
        {
            serviceCollection.AddTransient<IResourceSetActions, ResourceSetActions>();
            serviceCollection.AddTransient<IAddResourceSetAction, AddResourceSetAction>();
            serviceCollection.AddTransient<IGetResourceSetAction, GetResourceSetAction>();
            serviceCollection.AddTransient<IUpdateResourceSetAction, UpdateResourceSetAction>();
            serviceCollection.AddTransient<IDeleteResourceSetAction, DeleteResourceSetAction>();
            serviceCollection.AddTransient<IGetAllResourceSetAction, GetAllResourceSetAction>();
            serviceCollection.AddTransient<IResourceSetParameterValidator, ResourceSetParameterValidator>();
            serviceCollection.AddTransient<IPermissionControllerActions, PermissionControllerActions>();
            serviceCollection.AddTransient<IAddPermissionAction, AddPermissionAction>();
            serviceCollection.AddTransient<IRepositoryExceptionHelper, RepositoryExceptionHelper>();
            serviceCollection.AddTransient<IAuthorizationPolicyValidator, AuthorizationPolicyValidator>();
            serviceCollection.AddTransient<IBasicAuthorizationPolicy, BasicAuthorizationPolicy>();
            serviceCollection.AddTransient<ICustomAuthorizationPolicy, CustomAuthorizationPolicy>();
            serviceCollection.AddTransient<IAddAuthorizationPolicyAction, AddAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IPolicyActions, PolicyActions>();
            serviceCollection.AddTransient<IGetAuthorizationPolicyAction, GetAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IDeleteAuthorizationPolicyAction, DeleteAuthorizationPolicyAction>();
            serviceCollection.AddTransient<IGetAuthorizationPoliciesAction, GetAuthorizationPoliciesAction>();
            serviceCollection.AddTransient<IUpdatePolicyAction, UpdatePolicyAction>();
            serviceCollection.AddTransient<IJwtTokenParser, JwtTokenParser>();
            serviceCollection.AddTransient<IAddResourceSetToPolicyAction, AddResourceSetToPolicyAction>();
            serviceCollection.AddTransient<IDeleteResourcePolicyAction, DeleteResourcePolicyAction>();
            serviceCollection.AddTransient<IGetPoliciesAction, GetPoliciesAction>();
            serviceCollection.AddTransient<ISearchAuthPoliciesAction, SearchAuthPoliciesAction>();
            serviceCollection.AddTransient<ISearchResourceSetOperation, SearchResourceSetOperation>();
            serviceCollection.AddTransient<IUmaTokenActions, UmaTokenActions>();
            serviceCollection.AddTransient<IGetTokenByTicketIdAction, GetTokenByTicketIdAction>();
            serviceCollection.AddSingleton<IUmaConfigurationService>(new DefaultUmaConfigurationService(umaConfigurationOptions));
            serviceCollection.AddSingleton<IPolicyRepository>(new DefaultPolicyRepository(policies));
            serviceCollection.AddSingleton<IResourceSetRepository>(new DefaultResourceSetRepository(resources));
            serviceCollection.AddSingleton<ITicketStore, DefaultTicketStore>();
        }
    }
}
