﻿using SimpleIdServer.Module;
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
        }

        public IEnumerable<EditCredentialLinkViewModel> Links { get; set; }
    }
}
