using Financial.Entities;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class AuthSignInResponse
    {
        // public int Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Token { get; set; }


        public AuthSignInResponse(AppUser user, string token)
        {
            // Id = user.Id;
            Email = user.Email;
            Username = user.UserName;
            Token = token;
        }
    }
}