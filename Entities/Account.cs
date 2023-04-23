using Going.Plaid.Entity;

namespace Financial.Entities
{
    public class Account
    {
        public string? Id { get; set; }
        public decimal? AvailableBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? LimitBalance { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Mask { get; set; }
        public string? Name { get; set; }
        public string? OfficialName { get; set; }
        public AccountSubtype? Subtype { get; set; }
        public AccountType? Type { get; set; }
        public string? ItemId { get; set; }
        public virtual Item? Item { get; set; }
        public virtual List<Transaction>? Transactions { get; set; }
        public virtual List<Schedule>? Schedules { get; set; }
        public virtual List<Generated>? Generateds{ get; set; }
    }
}