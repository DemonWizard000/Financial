namespace Financial.Entities
{
    public class Generated
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public string? Payee { get; set; }
        public decimal Amount { get; set; }
        public ScheduleType? Type { get; set; }
        public RecurrencyMode? Mode { get; set; }
        public DateTime Date { get; set; }
        public string? TransactionId { get; set; }
        public string? TransferAccId { get; set; }
        public string? AccountId { get; set; }
        public int? ScheduleId { get; set; }
        public virtual Schedule? Schedule { get; set; }
    }
}