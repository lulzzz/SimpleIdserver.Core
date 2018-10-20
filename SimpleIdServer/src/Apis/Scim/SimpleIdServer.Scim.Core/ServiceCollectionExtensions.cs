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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.Scim.Core.Apis;
using SimpleIdServer.Scim.Core.EF.Helpers;
using SimpleIdServer.Scim.Core.EF.Models;
using SimpleIdServer.Scim.Core.EF.Stores;
using SimpleIdServer.Scim.Core.Factories;
using SimpleIdServer.Scim.Core.Parsers;
using SimpleIdServer.Scim.Core.Stores;
using SimpleIdServer.Scim.Core.Validators;

namespace SimpleIdServer.Scim.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScimCore(this IServiceCollection services, List<Representation> representations = null, List<Schema> schemas = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IRepresentationRequestParser, RepresentationRequestParser>();
            services.AddTransient<IJsonParser, RepresentationRequestParser>();
            services.AddTransient<IRepresentationResponseParser, RepresentationResponseParser>();
            services.AddTransient<IPatchRequestParser, PatchRequestParser>();
            services.AddTransient<ISearchParameterParser, SearchParameterParser>();
            services.AddTransient<IFilterParser, FilterParser>();
            services.AddTransient<IBulkRequestParser, BulkRequestParser>();
            services.AddTransient<IAddRepresentationAction, AddRepresentationAction>();
            services.AddTransient<IGetRepresentationAction, GetRepresentationAction>();
            services.AddTransient<IDeleteRepresentationAction, DeleteRepresentationAction>();
            services.AddTransient<IUpdateRepresentationAction, UpdateRepresentationAction>();
            services.AddTransient<IPatchRepresentationAction, PatchRepresentationAction>();
            services.AddTransient<IGetRepresentationsAction, GetRepresentationsAction>();
            services.AddTransient<IBulkAction, BulkAction>();
            services.AddTransient<IGroupsAction, GroupsAction>();
            services.AddTransient<IUsersAction, UsersAction>();
            services.AddTransient<IApiResponseFactory, ApiResponseFactory>();
            services.AddTransient<IErrorResponseFactory, ErrorResponseFactory>();
            services.AddTransient<ICommonAttributesFactory, CommonAttributesFactory>();
            services.AddTransient<IParametersValidator, ParametersValidator>();
            services.AddTransient<ITransformers, Transformers>();
            var schemaStore = new DefaultSchemaStore(schemas);
            services.AddSingleton<IRepresentationStore>(new DefaultRepresentationStore(representations, schemaStore));
            services.AddSingleton<ISchemaStore>(schemaStore);
            return services;
        }
    }
}
