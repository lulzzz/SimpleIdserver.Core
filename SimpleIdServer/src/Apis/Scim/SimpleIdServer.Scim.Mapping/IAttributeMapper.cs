using System.Threading.Tasks;
using SimpleIdServer.Scim.Common.Models;

namespace SimpleIdServer.Scim.Mapping
{
    public interface IAttributeMapper
    {
        Task Map(Representation representation, string schemaId);
    }
}