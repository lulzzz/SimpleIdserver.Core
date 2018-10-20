using System.Collections.Generic;

namespace SimpleIdServer.Module
{
    public interface IModule
    {
        void Init(IDictionary<string, string> options);
    }
}