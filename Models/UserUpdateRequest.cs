using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class UserUpdateRequest
    {
        [Required]
        public string email { get; set; } = "";

        [Required]
        public string username { get; set; } = "";

        [Required]
        public string name { get; set; } = "";
    }
}