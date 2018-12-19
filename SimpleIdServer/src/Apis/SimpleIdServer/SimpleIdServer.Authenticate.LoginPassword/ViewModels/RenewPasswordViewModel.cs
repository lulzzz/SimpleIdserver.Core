﻿using System.ComponentModel.DataAnnotations;

namespace SimpleIdServer.Authenticate.LoginPassword.ViewModels
{
    public class RenewPasswordViewModel
    {
        [Required]
        public string ActualPassword { get; set; }
        [Required]
        [Compare("ActualPassword", ErrorMessage = "Confirm password doesn't match, type again !")]
        public string ConfirmActualPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "Confirm password doesn't match, type again !")]
        public string ConfirmNewPassword { get; set; }
        public string Code { get; set; }
    }
}
