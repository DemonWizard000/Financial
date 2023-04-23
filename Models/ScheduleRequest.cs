using Financial.Entities;
using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class ScheduleRequest {
        public int? schedule_id { get; set; }

        [Required]
        public string? description { get; set; }

        [Required]
        public string? payee { get; set; }

        [Required]
        public decimal amount { get; set; }

        [Required]
        public ScheduleType? type { get; set; }

        [Required]
        public RecurrencyMode? mode { get; set; }

        [Required]
        public DateTime start_date { get; set; }

        [Required]
        public string? account_id { get; set; }
        public string? transfer_acc_id { get; set; }
    }
}
