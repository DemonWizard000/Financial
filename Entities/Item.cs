namespace Financial.Entities
{
    public class Item
    {
        public string? Id { get; set; }
        public string? AccessToken { get; set; }
        public string? InstitutionId { get; set; }
        public string? InstitutionName { get; set; }
        public string? InstitutionUrl { get; set; }
        public string? InstitutionPrimaryColor { get; set; }
        public string? WebhookUrl { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? UserId { get; set; }
        public virtual AppUser? User { get; set; }
        public virtual List<Account>? Accounts { get; set; }
    }
}