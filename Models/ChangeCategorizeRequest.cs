using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class ChangeCategorizeRequest {
        [Required]
        public decimal amount { get; set; }

        [Required]
        public int date{ get; set; }
    }
}
