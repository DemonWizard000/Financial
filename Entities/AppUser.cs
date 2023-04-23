using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Financial.Entities
{
    public class AppUser: IdentityUser
    {
        public string? Name { get; set; }
        public DateTime? JoinDate { get; set; }
        public decimal CategorizeAmountPercent { get; set; } = 0;
        public int CategorizeDateRange { get; set; } = 15;
        public List<Item>? Items { get; set; }
    }
}