using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace SimpleIdServer.IdentityStore.LDAP.Extensions
{
    public static class SearchResultAttributeCollectionExtensions
    {
        public static IEnumerable<string> GetAttributes(this SearchResultAttributeCollection attributes, string attributeName)
        {
            if (!attributes.Contains(attributeName))
            {
                return new List<string>();
            }

            var attributeValues = attributes[attributeName];
            var result = new List<string>();
            foreach (var attrV in attributeValues)
            {
                result.Add(attrV.ToString());
            }

            return result;
        }
    }
}
