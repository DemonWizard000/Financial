using Going.Plaid.Entity;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class PlaidInfoResponse
    {
        public string? ItemId { get; set; }

        public string? AccessToken { get; set; }

        public Products[]? Products { get; set; }
    }
}
