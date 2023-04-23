using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class AuthSignUpRequest
    {
        [Required]
        public string email { get; set; } = "";

        [Required]
        public string username { get; set; } = "";

        [Required]
        public string name { get; set; } = "";

        [Required]
        public string password { get; set; } = "";
    }
}