using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class AuthSignInRequest
    {

        [Required]
        public string email { get; set; } = "";

        [Required]
        public string password { get; set; } = "";

        [Required]
        public bool rememberMe { get; set; }
    }
}