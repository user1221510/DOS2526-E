using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProductsAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }

        [JsonIgnore]
        public List<Sale> Sales { get; set; } = new();
    }
}