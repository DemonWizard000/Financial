using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class ItemIdRequest {
        [Required]
        public string? item_id { get; set; }
    }
}
