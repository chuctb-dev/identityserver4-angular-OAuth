using AuthenticationServer.Core.Entities.IdentityAggregate;

namespace AuthenticationServer.Quickstart.Account
{
    public class RegisterResponseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public RegisterResponseViewModel(AppUser user)
        {
            Id = user.Id;
            Name = user.UserName;
            Email = user.Email;
        }
    }
}
