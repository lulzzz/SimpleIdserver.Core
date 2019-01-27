using SimpleIdServer.Module;
using System.Collections.Generic;

namespace SimpleIdServer.UserManagement.ViewModels
{
    public class EditCredentialLinkViewModel
    {
        public string DisplayName { get; set; }
        public RedirectUrl Href { get; set; }
    }

    public class EditCredentialViewModel
    {
        public EditCredentialViewModel()
        {
            Links = new List<EditCredentialLinkViewModel>();
            TwoFactorAuthTypes = new List<string>();
        }

        public IEnumerable<EditCredentialLinkViewModel> Links { get; set; }
        public IEnumerable<string> TwoFactorAuthTypes { get; set; }
        public bool CanUpdateTwoFactorAuthentication { get; set; }
        public string SelectedTwoFactorAuthType { get; set; }
    }
}
