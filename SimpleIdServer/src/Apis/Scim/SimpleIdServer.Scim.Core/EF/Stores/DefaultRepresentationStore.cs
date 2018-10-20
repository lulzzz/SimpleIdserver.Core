using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdServer.Scim.Common.DTOs;
using SimpleIdServer.Scim.Common.Models;
using SimpleIdServer.Scim.Core.EF.Extensions;
using SimpleIdServer.Scim.Core.EF.Helpers;
using SimpleIdServer.Scim.Core.Parsers;
using SimpleIdServer.Scim.Core.Results;
using SimpleIdServer.Scim.Core.Stores;
using Representation = SimpleIdServer.Scim.Core.EF.Models.Representation;
using RepresentationAttribute = SimpleIdServer.Scim.Common.Models.RepresentationAttribute;

namespace SimpleIdServer.Scim.Core.EF.Stores
{
    public class DefaultRepresentationStore : IRepresentationStore
    {
        public List<Representation> _representations;
        private ITransformers _transformers;
        private ISchemaStore _schemaStore;

        public DefaultRepresentationStore(List<Representation> representations, ISchemaStore schemaStore)
        {
            _representations = representations == null ? new List<Representation>() : representations;
            _transformers = new Transformers();
            _schemaStore = schemaStore;
        }

        public Task<bool> AddRepresentation(Common.Models.Representation representation)
        {
            var record = new Representation
            {
                Id = representation.Id,
                Created = representation.Created,
                LastModified = representation.LastModified,
                ResourceType = representation.ResourceType,
                Version = representation.Version,
                Attributes = GetRepresentationAttributes(representation)
            };
            _representations.Add(record);
            return Task.FromResult(true);
        }

        public Task<Common.Models.Representation> GetRepresentation(string id)
        {
            var representation = _representations.FirstOrDefault(r => r.Id == id);
            if (representation == null)
            {
                return Task.FromResult((Common.Models.Representation)null);
            }

            var record = representation.ToDomain();
            record.Attributes = GetRepresentationAttributes(representation);
            return Task.FromResult(record);
        }

        public Task<bool> RemoveRepresentation(Common.Models.Representation representation)
        {
            var record = _representations.FirstOrDefault(r => r.Id == representation.Id);
            if (record == null)
            {
                return Task.FromResult(false);
            }

            _representations.Remove(record);
            return Task.FromResult(true);
        }

        public Task<PaginatedResult<Common.Models.Representation>> SearchRepresentations(string resourceType, SearchParameter searchParameter)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            int totalResults;
            var result= QueryHelper.SearchRepresentations(_representations.AsQueryable(), resourceType, searchParameter, out totalResults).ToList();
            var content = result.Select(r =>
            {
                var rep = r.ToDomain();
                rep.Attributes = GetRepresentationAttributes(r);
                return rep;
            }).ToList();
            return Task.FromResult(new PaginatedResult<Common.Models.Representation>
            {
                Content = content,
                StartIndex = searchParameter.StartIndex,
                Count = totalResults
            });
        }

        public Task<IEnumerable<RepresentationAttribute>> SearchValues(string resourceType, Filter filter)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var representationAttributes = GetAllRepresentationAttributes();
            var result = QueryHelper.SearchValues(_representations.AsQueryable(), representationAttributes.AsQueryable(), resourceType, filter).ToList();
            IEnumerable<RepresentationAttribute> r = GetRepresentationAttributes(resourceType, result);
            return Task.FromResult(r);
        }

        public Task<bool> UpdateRepresentation(Common.Models.Representation representation)
        {
            var record = _representations.FirstOrDefault(r => r.Id == representation.Id);
            if (record == null)
            {
                return Task.FromResult(false);
            }

            record.SetData(representation);
            record.Attributes = GetRepresentationAttributes(representation);
            return Task.FromResult(true);
        }

        #region Private methods

        private List<RepresentationAttribute> GetRepresentationAttributes(Representation representation)
        {
            if (representation.Attributes == null)
            {
                return new List<RepresentationAttribute>();
            }

            return GetRepresentationAttributes(representation.ResourceType, representation.Attributes);
        }

        private List<RepresentationAttribute> GetRepresentationAttributes(string type, List<Models.RepresentationAttribute> attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var schema = _schemaStore.GetSchema(GetResourceSchemaType(type)).Result;
            var result = new List<RepresentationAttribute>();
            foreach (var attribute in attributes)
            {
                var transformed = _transformers.Transform(attribute);
                if (transformed == null)
                {
                    continue;
                }

                var schemaAttr = schema.Attributes.FirstOrDefault(a => a.Id == attribute.SchemaAttributeId);
                UpdateRepresentationAttribute(transformed, schemaAttr);
                result.Add(transformed);
            }

            return result;
        }

        private List<Models.RepresentationAttribute> GetRepresentationAttributes(Common.Models.Representation representation)
        {
            if (representation.Attributes == null)
            {
                return new List<Models.RepresentationAttribute>();
            }

            var schema = _schemaStore.GetSchema(GetResourceSchemaType(representation.ResourceType)).Result;
            var result = new List<Models.RepresentationAttribute>();
            foreach (var attribute in representation.Attributes)
            {
                var transformed = _transformers.Transform(attribute);
                if (transformed == null)
                {
                    continue;
                }

                var schemaAttr = schema.Attributes.FirstOrDefault(a => a.Id == transformed.SchemaAttributeId);
                UpdateRepresentationAttribute(transformed, schemaAttr);
                result.Add(transformed);
            }

            return result;
        }

        private void UpdateRepresentationAttribute(RepresentationAttribute representationAttribute, SchemaAttributeResponse schemaAttr)
        {
            representationAttribute.SchemaAttribute = schemaAttr;
            if (schemaAttr.Type != "complex")
            {
                return;
            }

            var complex = representationAttribute as ComplexRepresentationAttribute;
            var complexSchema = schemaAttr as ComplexSchemaAttributeResponse;
            foreach (var child in complex.Values)
            {
                if (schemaAttr.MultiValued)
                {
                    var subComplex = child as ComplexRepresentationAttribute;
                    foreach(var c in subComplex.Values)
                    {
                        var sra = complexSchema.SubAttributes.First(s => s.Id == c.SchemaAttribute.Id);
                        UpdateRepresentationAttribute(c, sra);
                    }
                }
                else
                {
                    var subRepresentationAttr = complexSchema.SubAttributes.First(s => s.Id == child.SchemaAttribute.Id);
                    UpdateRepresentationAttribute(child, subRepresentationAttr);
                }
            }
        }

        private void UpdateRepresentationAttribute(Models.RepresentationAttribute representationAttribute, SchemaAttributeResponse schemaAttr)
        {
            representationAttribute.SchemaAttribute = schemaAttr.ToModel();
            if (schemaAttr.Type != "complex")
            {
                return;
            }

            var complex = schemaAttr as ComplexSchemaAttributeResponse;
            foreach(var child in representationAttribute.Children)
            {
                if (schemaAttr.MultiValued)
                {
                    foreach (var c in child.Children)
                    {
                        var sra = complex.SubAttributes.First(s => s.Id == c.SchemaAttributeId);
                        UpdateRepresentationAttribute(c, sra);
                    }
                }
                else
                {
                    var subAttr = complex.SubAttributes.First(s => s.Id == child.SchemaAttributeId);
                    UpdateRepresentationAttribute(child, subAttr);
                }
            }
        }

        private List<Core.EF.Models.RepresentationAttribute> GetAllRepresentationAttributes()
        {
            var representationAttributes = new List<Models.RepresentationAttribute>();
            foreach (var repr in _representations)
            {
                if (repr.Attributes == null)
                {
                    continue;
                }

                foreach(var attr in repr.Attributes)
                {
                    GetRepresentationAttribute(attr, representationAttributes);
                }
            }

            return representationAttributes;
        }

        private void GetRepresentationAttribute(Core.EF.Models.RepresentationAttribute reprAttr, List<Core.EF.Models.RepresentationAttribute> representationAttrs)
        {
            representationAttrs.Add(reprAttr);
            if (reprAttr.Children != null)
            {
                foreach (var child in reprAttr.Children)
                {
                    GetRepresentationAttribute(child, representationAttrs);
                }
            }
        }

        #endregion

        private static string GetResourceSchemaType(string type)
        {
            if (type == Scim.Common.Constants.ResourceTypes.User)
            {
                return Scim.Common.Constants.SchemaUrns.User;
            }

            return Scim.Common.Constants.SchemaUrns.Group;
        }
    }
}
