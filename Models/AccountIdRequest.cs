using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class AccountIdRequest {
        [Required]
        public string? account_id { get; set; }
    }
}
