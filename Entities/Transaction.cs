using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Financial.Entities
{
    public class Transaction
    {
        [Key, Column(Order = 0)]
        public string? Id { get; set; }
        public decimal Amount { get; set; }
        public string? CurrencyCode { get; set; }
        public DateTime Date { get; set; }
        public string? Name { get; set; }
        // public IReadOnlyList<string>? Category { get; set; }
        public bool? Consolidated { get; set; }
        [Key, Column(Order = 1)]
        public string? AccountId { get; set; }
        public virtual Account? Account { get; set; }
    }
}