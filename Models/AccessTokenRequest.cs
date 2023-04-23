using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class AccessTokenRequest
    {
        [Required]
        public string public_token { get; set; } = "";
    }
}
