namespace SimpleIdServer.UserManagement
{
    public class UserManagementOptions
    {
        public UserManagementOptions()
        {
            CanUpdateTwoFactorAuthentication = false;
        }
        
        public bool CanUpdateTwoFactorAuthentication { get; set; }
    }
}
