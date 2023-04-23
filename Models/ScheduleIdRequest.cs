using Financial.Entities;
using System.ComponentModel.DataAnnotations;

namespace Financial.Models
{
    public class ScheduleIdRequest
    {
        [Required]
        public int? schedule_id { get; set; }
    }
}
