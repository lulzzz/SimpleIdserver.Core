using SimpleIdServer.Uma.Core.Models;
using System.Collections.Generic;

namespace SimpleIdServer.Uma.Website.Host
{
    public class UmaAuthenticationWebsiteOptions
    {
        public string OpenidWellKnownConfigurationUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class UmaConfigurationWebsiteOptions
    {
        public ICollection<ResourceSet> ResourceSet { get; set; }
        public ICollection<Policy> Policies { get; set; }
    }

    public class UmaWebsiteOptions
    {
        public UmaWebsiteOptions()
        {
            Authentication = new UmaAuthenticationWebsiteOptions();
            Configuration = new UmaConfigurationWebsiteOptions();
        }

        public UmaAuthenticationWebsiteOptions Authentication { get; set; }
        public UmaConfigurationWebsiteOptions Configuration { get; set; }
    }
}