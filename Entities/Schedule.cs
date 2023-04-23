using System.Runtime.Serialization;

namespace Financial.Entities
{
    public enum ScheduleType
    {
        Income,
        Expense,
        Transfer
    }
    public enum RecurrencyMode
    {
        Daily,
        Weekly,
        Monthly,
        OneDay
    }
    public class Schedule
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public string? Payee { get; set; }
        public decimal Amount { get; set; }
        public ScheduleType? Type { get; set; }
        public RecurrencyMode? Mode { get; set; }
        public DateTime StartDate { get; set; }
        public string? TransferAccId { get; set; }
        public string? AccountId { get; set; }
        public virtual Account? Account { get; set; }
        public virtual List<Generated>? Generateds { get; set; }
    }
}