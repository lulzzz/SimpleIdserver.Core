using System;
using System.Linq;
using SimpleIdServer.Scim.Core.EF.Extensions;
using SimpleIdServer.Scim.Core.EF.Models;
using SimpleIdServer.Scim.Core.Parsers;

namespace SimpleIdServer.Scim.Core.EF
{
    public static class QueryHelper
    {
        public static IQueryable<Representation> SearchRepresentations(IQueryable<Representation> reprs, string resourceType, SearchParameter searchParameter, out int totalResults)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            IQueryable<Representation> representations = reprs
                .Where(r => r.ResourceType == resourceType);
            if (searchParameter.Filter != null)
            {
                var lambdaExpression = searchParameter.Filter.EvaluateFilter(representations);
                representations = (IQueryable<Representation>)lambdaExpression.Compile().DynamicInvoke(representations);
            }

            totalResults = representations.Count();
            representations = representations.Skip(searchParameter.StartIndex);
            representations = representations.Take(searchParameter.Count);
            return representations;
        }

        public static IQueryable<RepresentationAttribute> SearchValues(IQueryable<Representation> reprs, IQueryable<RepresentationAttribute> reprsAttrs, string resourceType, Filter filter)
        {
            IQueryable<Representation> representations = reprs.Where(r => r.ResourceType == resourceType);
            IQueryable<RepresentationAttribute> representationAttributes = reprsAttrs;
            var lambdaExpression = filter.EvaluateSelection(representations, representationAttributes);
            var res = (IQueryable<RepresentationAttribute>)lambdaExpression.Compile().DynamicInvoke(representations);
            return res;
        }
    }
}
