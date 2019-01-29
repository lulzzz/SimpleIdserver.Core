using SimpleIdServer.Module;
using System.Collections.Generic;

namespace SimpleIdServer.Shell.ViewModels
{
    public class HomeTileViewModel
    {
        public string DisplayName { get; set; }
        public string Picture { get; set; }
        public RedirectUrl RedirectionUrl { get; set; }
    }

    public class HomeViewModel
    {
        public IEnumerable<HomeTileViewModel> Tiles { get; set; }
    }
}
