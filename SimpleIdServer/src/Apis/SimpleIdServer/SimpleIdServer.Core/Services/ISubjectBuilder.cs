using System.Threading.Tasks;

namespace SimpleIdServer.Core.Services
{
    public interface ISubjectBuilder
    {
        Task<string> BuildSubject();
    }
}
