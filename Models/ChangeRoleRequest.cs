using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class ChangeRoleRequest
    {

        [Required]
        public string user_id { get; set; } = "";

        [Required]
        public string role { get; set; } = "";
    }
}