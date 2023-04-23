using Going.Plaid.Entity;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class PlaidItemResponse
    {
        public Item? Item { get; set; }

        public Institution? Institution { get; set; }
    }
}
