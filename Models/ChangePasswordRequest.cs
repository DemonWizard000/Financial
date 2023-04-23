using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Financial.Models
{
    public class ChangePasswordRequest
    {
        public string current { get; set; } = "";

        public string update { get; set; } = "";
    }
}