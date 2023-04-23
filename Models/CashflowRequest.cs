using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class CashflowRequest {
        [Required]
        public string? item_id { get; set; }
        [Required]
        public int days { get; set; }
    }
}
