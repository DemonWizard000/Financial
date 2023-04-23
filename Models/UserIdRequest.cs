using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class UserIdRequest
    {

        [Required]
        public string user_id { get; set; } = "";
    }
}